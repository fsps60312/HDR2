using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HDR2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SourceImagePanel sourceImagePanel;
        TargetImagePanel targetImagePanel;
        SettingsPanel settingsPanel;
        void InitializeViews()
        {
            this.Content = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition{Height=new GridLength(1,GridUnitType.Auto)},
                    new RowDefinition{Height=new GridLength(2,GridUnitType.Star)},
                    new RowDefinition{Height=new GridLength(1,GridUnitType.Auto)},
                    new RowDefinition{Height=new GridLength(6,GridUnitType.Star)}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition{Width=new GridLength(9,GridUnitType.Star)},
                    new ColumnDefinition{Width=new GridLength(7,GridUnitType.Star) }
                },
                Children=
                {
                    new WrapPanel
                    {
                        Children=
                        {
                            new Button{Content="Open"}.Set(Open),
                            new Button{Content="Generate"}.Set(Run),
                            new Button{Content="Save"}.Set(Save)
                        }
                    }.Set(0,0),
                    new LogPanel().Set(1,0),
                    new Label{Content="Settings:"}.Set(2,0),
                    (settingsPanel=new SettingsPanel()).Set(3,0),
                    new Label{Content="Source Images:"}.Set(0,1),
                    (sourceImagePanel=new SourceImagePanel()).Set(1,1),
                    new Label{Content="Result Image:"}.Set(2,1),
                    (targetImagePanel=new TargetImagePanel()).Set(3,1)
                }
            };
        }
        void RegisterEvents()
        {
            this.KeyDown += MainWindow_KeyDown;
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Run();
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.O) Open();
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.S) Save();
        }
        void Open() { sourceImagePanel.OpenImages(); }
        async void Run() { targetImagePanel.ShowImage(await settingsPanel.ProcessImage(sourceImagePanel.GetImages())); }
        void Save() { targetImagePanel.SaveImage(); }
        public MainWindow()
        {
            InitializeComponent();
            InitializeViews();
            RegisterEvents();
            LogPanel.Log("Ready.");
        }
    }
}
