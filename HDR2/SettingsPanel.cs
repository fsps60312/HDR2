using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HDR2
{
    class SettingsPanel:ContentControl
    {
        static SettingsPanel instance;
        public static string ToneArg(int i) { return instance.textBoxes_tone_params[i].Text; }
        StackPanel stackPanel_HDR, stackPanel_ToneMapping,stackPanel_tone_params;
        Dictionary<RadioButton, Func<HDRSolver>> radio_HDR = new Dictionary<RadioButton, Func<HDRSolver>>();
        Dictionary<RadioButton, Func<ToneMappingSolver>> radio_ToneMapping = new Dictionary<RadioButton, Func<ToneMappingSolver>>();
        List<TextBox> textBoxes_tone_params=new List<TextBox>();
        public SettingsPanel()
        {
            radio_HDR.Add(new RadioButton { Content = "Robertson" }, () => new RobertsonHDRSolver());
            radio_HDR.Add(new RadioButton { Content = "Enhanced Robertson" }, () => new EnhancedRobertsonHDRSolver());
            radio_ToneMapping.Add(new RadioButton { Content = "Heat Map" }, () => new HeatMapToneMapping());
            radio_ToneMapping.Add(new RadioButton { Content = "Global Operator" }, () => new GlobalOperatorToneMapping());
            radio_ToneMapping.Add(new RadioButton { Content = "Test" }, () => new TestToneMappingSolver());
            for (int i = 0; i < 2; i++) textBoxes_tone_params.Add(new TextBox { MinWidth = 100 });
            InitializeViews();
            instance = this;
        }
        void InitializeViews()
        {
            this.Content = new Grid
            {
                RowDefinitions=
                {
                    new RowDefinition{Height=new GridLength(1,GridUnitType.Auto)},
                    new RowDefinition{Height=new GridLength(1,GridUnitType.Star)}
                },
                Children=
                {
                    new Grid
                    {
                        RowDefinitions=
                        {
                            new RowDefinition{Height=new GridLength(1,GridUnitType.Auto)},
                            new RowDefinition{Height=new GridLength(1,GridUnitType.Auto)},
                            new RowDefinition{Height=new GridLength(1,GridUnitType.Auto)},
                            new RowDefinition{Height=new GridLength(1,GridUnitType.Auto)}
                        },
                        ColumnDefinitions=
                        {
                            new ColumnDefinition{Width=new GridLength(1,GridUnitType.Auto)},
                            new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)}
                        },
                        Children=
                        {
                            new Label{Content="HDR:"}.Set(0,0),
                            new ScrollViewer
                            {
                                HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,
                                VerticalScrollBarVisibility=ScrollBarVisibility.Disabled,
                                Content=(stackPanel_HDR=new StackPanel{Orientation=Orientation.Horizontal})
                            }.Set(0,1),
                            new Label{Content="Tone Mapping:"}.Set(2,0),
                            new ScrollViewer
                            {
                                HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,
                                VerticalScrollBarVisibility=ScrollBarVisibility.Disabled,
                                Content=(stackPanel_ToneMapping=new StackPanel{Orientation=Orientation.Horizontal})
                            }.Set(2,1),
                            new ScrollViewer
                            {
                                HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,
                                VerticalScrollBarVisibility=ScrollBarVisibility.Disabled,
                                Content=(stackPanel_tone_params=new StackPanel{Orientation=Orientation.Horizontal})
                            }.Set(3,1)
                        }
                    }.Set(0,0)
                }
            };
            foreach (var p in radio_HDR) stackPanel_HDR.Children.Add(p.Key);
            foreach (var p in radio_ToneMapping) stackPanel_ToneMapping.Children.Add(p.Key);
            for (int i = 0; i < textBoxes_tone_params.Count; i++)
            {
                stackPanel_tone_params.Children.Add(new Label { Content = $"arg{i + 1}" });
                stackPanel_tone_params.Children.Add(textBoxes_tone_params[i]);
            }
        }
        public async Task<MyImage> ProcessImage(List<MyImage>images)
        {
            HDRSolver hdr = null;
            foreach(var p in radio_HDR) if (p.Key.IsChecked == true) { hdr = p.Value(); break; }
            if (hdr == null) { LogPanel.Log("No HDR selected."); return null; }
            ToneMappingSolver tone = null;
            foreach(var p in radio_ToneMapping) if (p.Key.IsChecked == true) { tone = p.Value();break; }
            if (tone == null) { LogPanel.Log("No Tone-Mapping selected.");return null; }
            return await Task.Run(() =>
            {
                LogPanel.Log("Running HDR...");
                foreach (var image in images) hdr.AddImage(image);
                MyImageD heat_map = hdr.RunHDR();
                LogPanel.Log("Running Tone Mapping...");
                MyImage ans = tone.RunToneMapping(heat_map);
                LogPanel.Log("OK.");
                return ans;
            });
        }
    }
}
