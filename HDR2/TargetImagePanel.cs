using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HDR2
{
    class TargetImagePanel:ContentControl
    {
        Image image_viewer;
        void InitializeViews()
        {
            this.Content = new Grid
            {
                Children =
                {
                    (image_viewer = new Image()).Set(0, 0)
                }
            };
        }
        public TargetImagePanel()
        {
            InitializeViews();
        }
        MyImage image;
        public void ShowImage(MyImage _image)
        {
            image = _image;
            image_viewer.Source = image.ToBitmapSource();
        }
        public void SaveImage()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            if (dialog.ShowDialog() != true) { LogPanel.Log("Canceled.");return; }
            var stream = dialog.OpenFile();
            image.ToBitmapSource().Save(stream);
            LogPanel.Log($"Saved as {dialog.FileName}");
        }
    }
}
