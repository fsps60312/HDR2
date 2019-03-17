using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MetadataExtractor;

namespace HDR2
{
    class MyImageD
    {
        public double[] data { get; private set; }
        public int height { get; private set; }
        public int width { get; private set; }
        public int stride { get; private set; }
        public double dpi_x { get; private set; }
        public double dpi_y { get; private set; }
        public PixelFormat format { get; private set; }
        public BitmapPalette palette { get; private set; }
        public MyImageD(double[]_data,MyImage template)
        {
            data = _data;
            height = template.height; width = template.width;
            stride = template.stride;
            dpi_x = template.dpi_x; dpi_y = template.dpi_y;
            format = template.format;
            palette = template.palette;
        }
    }
    class MyImage
    {
        public void SetExposure(double _exposure) { exposure = _exposure; }
        public double exposure { get; private set; }
        public byte[] data { get; private set; }
        public int height { get; private set; }
        public int width { get; private set; }
        public int stride { get; private set; }
        public double dpi_x { get; private set; }
        public double dpi_y { get; private set; }
        public PixelFormat format { get; private set; }
        public BitmapPalette palette { get; private set; }
        public MyImage(string image_name)
        {
            exposure = GetImageExposure(image_name);
            var image = new BitmapImage(new Uri(image_name));
            height = image.PixelHeight; width = image.PixelWidth;
            stride = width * 4;
            dpi_x = image.DpiX; dpi_y = image.DpiY;
            data = new byte[height * stride];
            format = image.Format;
            palette = image.Palette;
            image.CopyPixels(data, stride, 0);
        }
        public MyImage(byte[] _data, MyImage template)
        {
            data = _data;
            height = template.height; width = template.width;
            stride = template.stride;
            dpi_x = template.dpi_x; dpi_y = template.dpi_y;
            format = template.format;
            palette = template.palette;
        }
        public MyImage(byte[] _data, MyImageD template)
        {
            data = _data;
            height = template.height; width = template.width;
            stride = template.stride;
            dpi_x = template.dpi_x; dpi_y = template.dpi_y;
            format = template.format;
            palette = template.palette;
        }
        public BitmapSource ToBitmapSource()
        {
            return BitmapSource.Create(width, height,
                    dpi_x, dpi_y,
                    format, palette,
                    data, stride);
        }
        static int counter = 0;
        static double GetImageExposure(string filename)
        {
            var directories = ImageMetadataReader.ReadMetadata(filename);
            var ToDouble = new Func<string, double>(s =>
            {
                if (s.EndsWith(" sec"))
                {
                    s = s.Remove(s.Length - " sec".Length);
                    if (s.IndexOf('/') == -1) return double.Parse(s);
                    else
                    {
                        var t = s.Split('/');
                        if (t.Length == 2) return double.Parse(t[0]) / double.Parse(t[1]);
                    }
                }
                return double.NaN;
            });
            counter++;
            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    if(counter<=1)LogPanel.Log($"directory: {directory.Name} \ttag: {tag.Name} \tdescription: {tag.Description}");
                    if (directory.Name == "Exif SubIFD" && tag.Name == "Exposure Time")
                        return ToDouble(tag.Description);
                }
            }
            return double.NaN;
        }
    }
}
