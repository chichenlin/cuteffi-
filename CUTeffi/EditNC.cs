using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CUTeffi;

namespace CUTeffi
{
    class EditNC
    {
        List<string> NCi = new List<string>();
        public StreamReader sr;

        public void editNC(double maxSP, double CuttingDepth, double ToolNumber, string s, string saveNC) {

            //string fileName = "D4000426_2.cnc";
            if (CUTeffiForm.material_Al.Checked == true)
            {
                sr = new StreamReader(s + "\\NC\\O7202forAL2.txt");////
            }
            if (CUTeffiForm.material_Iron.Checked == true)
            {
                sr = new StreamReader(s + "\\NC\\O7202forIron.txt");////
            }
            //StreamReader sr = new StreamReader(s + "\\NC\\O7202.txt");////

            while (!sr.EndOfStream)
            {
                string str = "";

                str = sr.ReadLine();

                str = str.Trim();

                if (str == "%")
                {
                    continue;
                }

                NCi.Add(str);

            }

            EidtNc(maxSP, CuttingDepth, ToolNumber);

            writeNC(saveNC);

            ////str.ReadLine();
            ////str.ReadToEnd();
            ////str.Close(); 
        }

        private void EidtNc(double maxSP, double CuttingDepth, double ToolNumber)
        {
            string[] split;

            string line = "";

            List<string> filter;

            for (int i = 0; i < NCi.Count; i++)
            {
                if (NCi[i].Substring(0, 1) == "#")
                {
                    split = NCi[i].Split(new char[] { '=', '#', '(' });

                    filter = new List<string>();

                    foreach (string s in split)
                        filter.Add(s.Trim());

                    for (int ii = 1; ii < filter.Count; ii++)
                    {
                        if (ii == 1 & filter[ii] == "1")
                        {
                            //filter[ii + 1] = textBox1.Text;

                            line = "#" + filter[ii] + "=" + Convert.ToString(maxSP) + "(" + filter[ii + 2];////

                            NCi[i] = line;


                        }
                        if (ii == 1 & filter[ii] == "2")
                        {

                            line = "#" + filter[ii] + "=" + Convert.ToString(CuttingDepth) + "(" + filter[ii + 2];////

                            NCi[i] = line;


                        }
                        if (ii == 1 & filter[ii] == "3")
                        {

                            line = "#" + filter[ii] + "=" + Convert.ToString(ToolNumber) + "(" + filter[ii + 2];////

                            NCi[i] = line;


                        }

                        line = "";
                    }
                }
            }
        }

        private void writeNC(string saveNC)
        {
            string NewNC = "";

            for (int i = 0; i < NCi.Count; i++)
            {
                NewNC += NCi[i] + "\r\n";

            }

            StreamWriter str = new StreamWriter(saveNC);

            str.WriteLine(NewNC);

            str.Close();

            //string newNC = "";
                       
        }

    }
}
