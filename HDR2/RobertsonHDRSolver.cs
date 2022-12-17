using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HDR2
{
    class RobertsonHDRSolver:HDRSolver
    {
        public override List<string> GetArgs()
        {
            return new List<string>();
        }
        double[] g_mapping = new double[256];
        double E;
        protected virtual double w(byte z,double exposure) { return Math.Max(1e-9, Math.Min(z, 255 - z) - 25); }
        private void Optimize()
        {
            for (int i = 0; i < g_mapping.Length; i++) g_mapping[i] = i;
            E = 0;
            Dictionary<double, int[]> data = new Dictionary<double, int[]>();
            foreach (var img in images)
            {
                if (!data.ContainsKey(img.exposure))
                {
                    data.Add(img.exposure, new int[256]);
                }
                var s = img.data;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int k = i * stride + j * 4;
                        byte b = s[k + 0];
                        byte g = s[k + 1];
                        byte r = s[k + 2];
                        if (!(r == 0 && g == 0 && b == 255))
                        {
                            ++data[img.exposure][r];
                            ++data[img.exposure][g];
                            ++data[img.exposure][b];
                        }
                    }
                }
            }
            /// arg min sum(w(z)*(g(z)-E*t)^2)
            /// = arg min sum(w(z)*(g(z)g(z)-2g(z)Et+EEtt))
            /// dE: sum(w(z)*(-2g(z)t+2Ett))=0
            ///     sum(w(z)ttE-w(z)g(z)t)=0
            /// dg: sum(w(z)*(2g(z)-2Et), for g of specular z)=0
            ///     sum(g(z)-Et)=0
            var optimize_E = new Action(() =>
            {
                double a = 0, b = 0; // ax=b
                foreach (var p in data)
                {
                    double exposure = p.Key;
                    var cnts = p.Value;
                    for (byte z = 0; ; z++)
                    {
                        if (cnts[z]>0)
                        {
                            Trace.Assert(!double.IsNaN(g_mapping[z]));
                            a += cnts[z] * w(z,exposure) * exposure * exposure;
                            b += cnts[z] * w(z, exposure) * g_mapping[z] * exposure;
                        }
                        if (z == 255) break;
                    }
                }
                E = b / a;
            });
            var optimize_g = new Action(() =>
            {
                for (byte z = 0; ; z++)
                {
                    double a = 0, b = 0;//ax=b, x is g(z)
                    foreach (var p in data)
                    {
                        double exposure = p.Key;
                        var cnts = p.Value;
                        a += cnts[z];
                        b += cnts[z] * E * exposure;
                    }
                    if (a != 0) g_mapping[z] = b / a;
                    else g_mapping[z] = double.NaN;
                    if (z == 255) break;
                }
                Trace.Assert(!double.IsNaN(g_mapping[128]));
                double avg = g_mapping[128];
                for (int z = 0; z < 256; z++) g_mapping[z] /= avg;
            });
            LogPanel.Log($"Converging from E = {E}");
            for (double origin_E = E + 1; Math.Abs(origin_E - E) > 1e-9;)
            {
                origin_E = E;
                optimize_E();
                LogPanel.Log($"E = {E}");
                optimize_g();
            }
        }
        protected override double[] Solve()
        {
            Optimize();
            var GetHeat = new Func<int,int, double>((_,i) =>
            {
                double a = 0, b = 0;//ax=b
                foreach (var img in images)
                {
                    if (!(img.data[_ + 0] == 255 && img.data[_ + 1] == 0 && img.data[_ + 2] == 0))
                    {
                        Trace.Assert(!double.IsNaN(g_mapping[img.data[i]]));
                        a += w(img.data[i], img.exposure);
                        b += w(img.data[i], img.exposure) * g_mapping[img.data[i]] / img.exposure;
                    }
                }
                return a==0?0:b / a;
            });
            double[] ans = new double[height * stride];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int k = i * stride + j * 4;
                    ans[k + 0] = GetHeat(k,k + 0);
                    ans[k + 1] = GetHeat(k, k + 1);
                    ans[k + 2] = GetHeat(k, k + 2);
                    ans[k + 3] = 255;
                }
            }
            return ans;
        }
    }
}
