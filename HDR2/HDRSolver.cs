using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace HDR2
{
    class TestHDRSolver : HDRSolver
    {
        protected override double[] Solve()
        {
            var data = images[0].data;
            double[] ans = new double[data.Length];
            for (int i = 0; i < data.Length; i++) ans[i] = data[i];
            return ans;
        }
    }
    abstract class HDRSolver
    {
        protected List<MyImage> images { get; private set; } = new List<MyImage>();
        protected int width { get; private set; }
        protected int height { get; private set; }
        protected int stride { get; private set; }
        //protected double dpi_x { get; private set; }
        //protected double dpi_y { get; private set; }
        //protected PixelFormat format { get; private set; }
        //protected BitmapPalette palette { get; private set; }
        public void AddImage(MyImage image)
        {
            images.Add(image);
        }
        protected abstract double[] Solve();
        public MyImageD RunHDR()
        {
            images = images.OrderBy(i => i.exposure).ToList();
            width = images[0].width;height = images[0].height;stride = images[0].stride;
            foreach (var image in images) Trace.Assert(image.height == height && image.width == width && image.stride == stride);
            //ans.Freeze();
            return new MyImageD(Solve(),images[0]);
        }
    }
}
