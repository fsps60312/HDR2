using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDR2
{
    /// <summary>
    /// Implements page 24 in the slide: https://www.csie.ntu.edu.tw/~cyy/courses/vfx/19spring/lectures/handouts/lec04_tonemapping.pdf
    /// </summary>
    /// arg1: alpha
    class GlobalOperatorToneMapping:ToneMappingSolver
    {
        public GlobalOperatorToneMapping()
        {
            if (!double.TryParse(SettingsPanel.ToneArg(0), out alpha)) alpha = 3;// low key: 0.18, high key: 0.5
            if (!double.TryParse(SettingsPanel.ToneArg(1), out Lwhite)) Lwhite = 100;
        }
        public override List<string> GetArgs() { return new List<string> { "α", "Lwhite" }; }
        double alpha,Lwhite;
        int width, height, stride;
        double[] data;
        //double Gaussian(int x,int y,double sigma)
        //{
        //    return Math.Exp(-(x * x + y * y) / (2 * sigma * sigma)) / (2 * Math.PI * sigma * sigma);
        //}
        //double Lblur(int x,int y,double sigma)
        //{
        //}
        //double V(int x,int y,double sigma)
        //{
        //    double Lblur_s = Lblur(x, y, sigma);
        //    double Lblur_s1 = Lblur(x, y, sigma+1);
        //    return (Lblur_s - Lblur_s1) / (2 ^? *a / (sigma * sigma) + Lblur_s);
        //}
        byte HeatToByte(double alpha,double Le,double Lw)
        {
            double Lm = alpha * (Lw / Le);
            return (256 * (Lm*(1+Lm/(Lwhite*Lwhite)) / (1 + Lm))).ClampByte();
        }
        protected override byte[] Solve(MyImageD image)
        {
            data = image.data;
            width = image.width;
            height = image.height;
            stride = image.stride;
            byte[] ans = new byte[data.Length];
            double Le = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int k = i * stride + j * 4;
                    if(!(data[k + 0] + data[k + 1] + data[k + 2]>=0))
                    {
                        LogPanel.Log($"Weird: {data[k + 0]}, {data[k + 1]}, {data[k + 2]}");
                    }
                    Le += Math.Log(1 + data[k + 0] + data[k + 1] + data[k + 2]);
                }
            }
            //LogPanel.Log($"Sum of Log(Lw): {Le}");
            Le = Math.Exp(Le / (height * width));
            LogPanel.Log($"alpha = {alpha}, Lwhite = {Lwhite}, Le = {Le}");
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int k = i * stride + j * 4;
                    ans[k + 0] = HeatToByte(alpha,Le,data[k + 0]);
                    ans[k + 1] = HeatToByte(alpha,Le,data[k + 1]);
                    ans[k + 2] = HeatToByte(alpha,Le,data[k + 2]);
                    ans[k + 3] = 255;
                }
            }
            return ans;
        }
    }
}
