using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDR2
{
    class TestToneMappingSolver:ToneMappingSolver
    {
        public override List<string> GetArgs()
        {
            return new List<string>();
        }
        protected override byte[] Solve(MyImageD image)
        {
            byte[] ans = new byte[image.data.Length];
            for (int i = 0; i < image.height; i++)
            {
                for (int j = 0; j < image.width; j++)
                {
                    int k = i * image.stride + j * 4;
                    ans[k + 0] = HeatToByte(image.data[k + 0]);
                    ans[k + 1] = HeatToByte(image.data[k + 1]);
                    ans[k + 2] = HeatToByte(image.data[k + 2]);
                    ans[k + 3] = 255;
                }
            }
            return ans;
        }
        byte HeatToByte(double heat)
        {
            return (256 * (heat / (1 + heat))).ClampByte();
        }
    }
    abstract class ToneMappingSolver
    {
        public abstract List<string> GetArgs();
        protected abstract byte[] Solve(MyImageD image);
        public MyImage RunToneMapping(MyImageD image) { return new MyImage(Solve(image), image); }
    }
}
