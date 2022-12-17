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
        class OptionButton<T> : RadioButton
        {
            Func<T> gen;
            SettingsPanel parent;
            public OptionButton(SettingsPanel _parent,string _text, Func<T>_gen)
            {
                parent = _parent;
                this.Content = _text;
                gen = _gen;
                this.Checked += delegate
                {
                    parent.SetToneMappingMethod(gen);
                };
            }
        }
        Func<ToneMappingSolver> selected_tone_mapping_method = null;
        Func<HDRSolver> selected_hdr_method = null;
        void SetToneMappingMethod<T>(Func<T> gen)
        {
            List<string> args;
            List<TextBox> txbs;
            StackPanel stk;
            if (typeof(T) == typeof(ToneMappingSolver))
            {
                args = (selected_tone_mapping_method = gen as Func<ToneMappingSolver>)().GetArgs();
                txbs= textBoxes_tone_params = new List<TextBox>();
                stk = stackPanel_tone_params;
            }
            else if (typeof(T) == typeof(HDRSolver))
            {
                args = (selected_hdr_method = gen as Func<HDRSolver>)().GetArgs();
                txbs = textBoxes_hdr = new List<TextBox>();
                stk = stackPanel_hdr_params;
            }
            else throw new Exception($"SetToneMappingMethod: unknown type of T: {typeof(T)}");

            stk.Children.Clear();
            foreach (string arg in args)
            {
                var txb = new TextBox { MinWidth = 100 };
                stk.Children.Add(new Label { Content = arg });
                stk.Children.Add(txb);
                txbs.Add(txb);
            }
        }
        static SettingsPanel instance;
        StackPanel stackPanel_hdr_params, stackPanel_tone_params;
        List<TextBox> textBoxes_tone_params = new List<TextBox>(), textBoxes_hdr= new List<TextBox>();
        public static string ToneArg(int i) { if (!(0 <= i && i < instance.textBoxes_tone_params.Count)) return null; return instance.textBoxes_tone_params[i].Text; }
        public static string HDRArg(int i) { if (!(0 <= i && i < instance.textBoxes_hdr.Count)) return null; return instance.textBoxes_hdr[i].Text; }
        public SettingsPanel()
        {
            InitializeViews();
            instance = this;
        }
        void InitializeViews()
        {
            StackPanel stackPanel_ToneMapping;
            StackPanel stackPanel_hdr;
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
                                Content=(stackPanel_hdr=new StackPanel{Orientation=Orientation.Horizontal})
                            }.Set(0,1),
                            new ScrollViewer
                            {
                                HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,
                                VerticalScrollBarVisibility=ScrollBarVisibility.Disabled,
                                Content=(stackPanel_hdr_params=new StackPanel{Orientation=Orientation.Horizontal})
                            }.Set(1,1),
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
            stackPanel_hdr.Children.Add(new OptionButton<HDRSolver>(this, "Robertson", () => new RobertsonHDRSolver()));
            stackPanel_hdr.Children.Add(new OptionButton<HDRSolver>(this, "Enhanced Robertson", () => new EnhancedRobertsonHDRSolver()));
            stackPanel_ToneMapping.Children.Add(new OptionButton<ToneMappingSolver>(this, "Heat Map", () => new HeatMapToneMapping()));
            stackPanel_ToneMapping.Children.Add(new OptionButton<ToneMappingSolver>(this, "Global Operator", () => new GlobalOperatorToneMapping()));
            stackPanel_ToneMapping.Children.Add(new OptionButton<ToneMappingSolver>(this, "Test", () => new TestToneMappingSolver()));
        }
        public async Task<MyImage> ProcessImage(List<MyImage>images)
        {
            HDRSolver hdr = selected_hdr_method?.Invoke();
            if (hdr == null) { LogPanel.Log("No HDR selected."); return null; }
            ToneMappingSolver tone = selected_tone_mapping_method?.Invoke();
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
