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
            if (!double.TryParse(SettingsPanel.ToneArg(0), out alpha)) alpha = 0.18;// low key: 0.18, high key: 0.5
            if (!double.TryParse(SettingsPanel.ToneArg(1), out Lwhite)) Lwhite = 3;
        }
        double alpha,Lwhite;
        byte HeatToByte(double alpha,double Le,double Lw)
        {
            double Lm = alpha * (Lw / Le);
            return (256 * (Lm*(1+Lm/(Lwhite*Lwhite)) / (1 + Lm))).ClampByte();
        }
        protected override byte[] Solve(MyImageD image)
        {
            byte[] ans = new byte[image.data.Length];
            double Le = 0;
            for (int i = 0; i < image.height; i++)
            {
                for (int j = 0; j < image.width; j++)
                {
                    int k = i * image.stride + j * 4;
                    if(!(image.data[k + 0] + image.data[k + 1] + image.data[k + 2]>=0))
                    {
                        LogPanel.Log($"Weird: {image.data[k + 0]}, {image.data[k + 1]}, {image.data[k + 2]}");
                    }
                    Le += Math.Log(1 + image.data[k + 0] + image.data[k + 1] + image.data[k + 2]);
                }
            }
            //LogPanel.Log($"Sum of Log(Lw): {Le}");
            Le = Math.Exp(Le / (image.height * image.width));
            LogPanel.Log($"alpha = {alpha}, Lwhite = {Lwhite}, Le = {Le}");
            for (int i = 0; i < image.height; i++)
            {
                for (int j = 0; j < image.width; j++)
                {
                    int k = i * image.stride + j * 4;
                    ans[k + 0] = HeatToByte(alpha,Le,image.data[k + 0]);
                    ans[k + 1] = HeatToByte(alpha,Le,image.data[k + 1]);
                    ans[k + 2] = HeatToByte(alpha,Le,image.data[k + 2]);
                    ans[k + 3] = 255;
                }
            }
            return ans;
        }
    }
}
