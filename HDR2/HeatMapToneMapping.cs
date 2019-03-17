using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDR2
{
    class HeatMapToneMapping:ToneMappingSolver
    {
        void GetHeatColor(double v,out byte r,out byte g,out byte b)
        {
            if (v < 0) r = g = b = 0;
            else if (v <= 0.25)
            {
                r = 0;
                g = (v / 0.25 * 256).ClampByte();
                b = 255;
            }
            else if (v <= 0.5)
            {
                r = 0;
                g = 255;
                b = ((0.5 - v) / 0.25 * 256).ClampByte();
            }
            else if (v <= 0.75)
            {
                r = ((v - 0.5) / 0.25 * 256).ClampByte();
                g = 255;
                b = 0;
            }
            else if (v <= 1)
            {
                r = 255;
                g = ((1 - v) / 0.25 * 256).ClampByte();
                b = 0;
            }
            else r = g = b = 255;
        }
        protected override byte[] Solve(MyImageD image)
        {
            double mx = double.MinValue, mn = double.MaxValue;
            for (int i = 0; i < image.height; i++)
            {
                for (int j = 0; j < image.width; j++)
                {
                    int k = i * image.stride + j * 4;
                    double v = Math.Log10(1+image.data[k + 0] + image.data[k + 1] + image.data[k + 2]);
                    if (v > mx) mx = v;
                    if (v < mn) mn = v;
                }
            }
            LogPanel.Log($"min heat: {Math.Pow(10, mn).ToString("E")}");
            LogPanel.Log($"max heat: {Math.Pow(10, mx).ToString("E")}");
            byte[] ans = new byte[image.data.Length];
            for (int i = 0; i < image.height; i++)
            {
                for (int j = 0; j < image.width; j++)
                {
                    int k = i * image.stride + j * 4;
                    double v = Math.Log10(1+image.data[k + 0] + image.data[k + 1] + image.data[k + 2]);
                    GetHeatColor((v - mn) / (mx - mn), out byte r, out byte g, out byte b);
                    ans[k + 0] = b;
                    ans[k + 1] = g;
                    ans[k + 2] = r;
                    ans[k + 3] = 255;
                }
            }
            return ans;
        }
    }
}
