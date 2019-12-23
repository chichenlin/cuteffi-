using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUTeffi
{
    public class MultiScale_Entropy
    {
        public static double[] MultiScaleEn3(double[] data, int scale)
        {
            double[] MSE = new double[scale];
            double[] buf = new double[(data.Length)/scale];
            double r = 0.15 * STD(data);
            for (int i = 0; i < scale; i++)
            {
                buf = croasgrain(data, i);
                MSE[i] = SampEn1(buf, r);
            }
            return MSE;
            //% 重複疊到的尺度訊號進行SE計算後取平均的MSE
        }
        public static double[] croasgrain(double[] buf, int scale)
        {
            double L = buf.Length,sum;
            int k = 0;
            double[] Dst = new double[buf.Length/(scale+1)];
            for (int i = 0; i < L - scale; i += (scale+1))// i = 1:scale: L - scale + 1
            {
                sum = 0;
                for (int j = i; j < i+scale+1; j++)
                {
                    sum = sum + buf[j];
                }
                Dst[k] = sum / (1+scale);
                //Dst[k] = data[i: i + scale - 1].Average();
                k = k + 1;
            }
            return Dst;
        }

        public static double SampEn1(double[] SE, double r)
        {
            double l = SE.Length;
            double Nn = 0;
            double Nd = 0;
            for (int i = 0; i < l - 2; i++)//i = 1:l - 2
            {
                for (int j = 1; j < l - 2; j++) //j = i + 1:l - 2
                {
                    if (Math.Abs(SE[i] - SE[j]) < r && Math.Abs(SE[i + 1] - SE[j + 1]) < r)
                    {
                        Nn = Nn + 1;
                        if (Math.Abs(SE[i + 2] - SE[j + 2]) < r)
                        {
                            Nd = Nd + 1;
                        }
                    }
                }
            }
            double entropy = -Math.Log(Nd / Nn);
            return entropy;
        }
        public static double STD(double[] num)
        {
            double avg = num.Average();

            double SumOfSqrs = 0.0;
            foreach (double d in num)
            {
                SumOfSqrs += Math.Pow(d - avg, 2);
            }
            return Math.Sqrt((SumOfSqrs / (num.Length-1)));
        }

    }

}
