using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace HDR2
{
    class SourceImagePanel:ContentControl
    {
        StackPanel stackPanel;
        public SourceImagePanel() { InitializeViews(); }
        void InitializeViews()
        {
            this.Content = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = (stackPanel = new StackPanel { Orientation = Orientation.Horizontal })
            };
        }
        void ShowImages()
        {
            var add_image = new Action<MyImage>(img =>
              {
                  stackPanel.Children.Add(new Grid
                  {
                      Margin=new Thickness(2),
                      RowDefinitions =
                      {
                        new RowDefinition{Height=new GridLength(1,GridUnitType.Auto)},
                        new RowDefinition{Height=new GridLength(1,GridUnitType.Star)}
                      },
                      Children =
                      {
                        new Label{Content=$"exposure = {img.exposure}",HorizontalAlignment=HorizontalAlignment.Center}.Set(0,0),
                        new Image{Source=img.ToBitmapSource()}.Set(1,0)
                      }
                  });
              });
            stackPanel.Children.Clear();
            foreach (var img in images) add_image(img);
        }
        List<MyImage> images = null;
        public List<MyImage>GetImages()
        {
            if (images == null) LogPanel.Log("Warning: [SourceImagePanel] images == null");
            if (images.Count >= 1 && double.IsNaN(images[0].exposure))
            {
                LogPanel.Log("Warning: [SourceImagePanel] image file doesn't contain exposure time information, generating according to power of 2...");
                //images.Sort((a, b) =>
                //{
                //    int ans = 0;
                //    int height = a.height, width = a.width,stride=a.stride;
                //    for (int i = 0; i < height; i++)
                //    {
                //        for (int j = 0; j < width; j++)
                //        {
                //            int k = i * stride + j * 4;
                //            if (!(a.data[k + 0] == 255 && a.data[k + 1] == 0 && a.data[k + 2] == 0) &&
                //                !(b.data[k + 0] == 255 && b.data[k + 1] == 0 && b.data[k + 2] == 0))
                //            {
                //                int a_c = a.data[k + 0] + a.data[k + 1] + a.data[k + 2];
                //                int b_c = b.data[k + 0] + b.data[k + 1] + b.data[k + 2];
                //                var c= a_c.CompareTo(b_c);
                //                ans += c > 0 ? 1 : c < 0 ? -1 : 0;
                //            }
                //        }
                //    }
                //    return ans;
                //});
                double exposure = 1;
                foreach (var img in images)
                {
                    img.SetExposure(exposure);
                    exposure /= 2;
                }
                ShowImages();
            }
            return images;
        }
        public void OpenImages()
        {
            LogPanel.Log("Opening images...");
            var dialog = new OpenFileDialog { Multiselect = true };
            if (dialog.ShowDialog() == true)
            {
                var ans = dialog.FileNames;
                if (ans != null)
                {
                    images = new List<MyImage>();
                    foreach (var f in ans)
                    {
                        LogPanel.Log(f);
                        images.Add(new MyImage(f));
                    }
                    images = images.OrderBy(v => v.exposure).ToList();
                    LogPanel.Log($"{ans.Length} files selected.");
                    ShowImages();
                }
                else LogPanel.Log("No files selected.");
            }
            else LogPanel.Log("Canceled.");
        }
    }
}
