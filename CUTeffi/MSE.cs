using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUTeffi
{
    public class MultiScale_Entropy
    {
        public double[] MultiScaleEn3(double[] data, int scale)
        {
            double[] MSE = new double[scale];
            double[] buf = new double[(data.Length-scale)/scale];
            double r = 0.15 * STD(data);
            for (int i = 0; i < scale; i++)
            {
                buf = croasgrain(data, i);
                MSE[i] = SampEn1(buf, r);
            }
            return MSE;
            //% 重複疊到的尺度訊號進行SE計算後取平均的MSE
        }
        public double[] croasgrain(double[] data, int scale)
        {
            double L = data.Length,sum;
            int k = 0;
            double[] Dst = new double[(data.Length-scale)/scale];
            for (int i = 0; i < L - scale; i += scale)// i = 1:scale: L - scale + 1
            {
                sum = 0;
                for (int j = i; j < i+scale; j++)
                {
                    sum = sum + data[j];
                }
                Dst[k] = sum / scale;
                //Dst[k] = data[i: i + scale - 1].Average();
                k = k + 1;
            }
            return Dst;
        }

        public double SampEn1(double[] data, double r)
        {
            double l = data.Length;
            double Nn = 0;
            double Nd = 0;
            for (int i = 0; i < l - 1; i++)//i = 1:l - 2
            {
                for (int j = 1; j < l - 1; j++) //j = i + 1:l - 2
                {
                    if (Math.Abs(data[i] - data[j]) < r && Math.Abs(data[i + 1] - data[j + 1]) < r)
                    {
                        Nn = Nn + 1;
                        if (Math.Abs(data[i + 2] - data[j + 2]) < r)
                        {
                            Nd = Nd + 1;
                        }
                    }
                }
            }
            double entropy = -Math.Log(Nd / Nn);
            return entropy;
        }
        public double STD(double[] num)
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
