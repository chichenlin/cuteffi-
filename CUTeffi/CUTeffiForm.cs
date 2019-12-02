using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using NIDAQ;
using MathWorks.MATLAB.NET.Arrays;

namespace CUTeffi
{
    public partial class CUTeffiForm : Form
    {
        public SplashScreen FSS = null;
        public int indexPanelSetting = 0;  //  0 = 設定面板關,  1 = 設定面板開
        public int indexProccessMode = 0;  //  0 = 預設,  1 = 自訂
        public int indexDAQMode = 0; //  0 = 停止,  1 = 擷取
        public int indexProgramState = 0;  //  0 = 初始化,  1 = 擷取中,  2 = 閒置
        public static int indexMaterial = 1; // 1 = 鋁合金,  2 = 不鏽鋼
        public double OperatingSPmax;
        public static int varNtest = 12;
        public static double threshold;
        public static TextBox RMStextbox;//及時RMS


        //public static StreamWriter SW_RMSData;
        //public static StreamWriter SW_State;

        public static double[] A = new double[4];

        //NIDAQ.Class1 initialDAQ = new NIDAQ.Class1();
        //NIDAQ.Class1 startDAQ = new NIDAQ.Class1();
        //NIDAQ.Class1 stopDAQ = new NIDAQ.Class1();
        nidaqAPI nidaq = new nidaqAPI();
        
        public CUTeffiForm()
        {
            InitializeComponent();
            this.Size = new Size(416, 539);
            panelSetting.Location = new Point(0, 50);
            RMStextbox = this.textBox10;//及時RMS
            initialCUTeffi();
           
        }

        private void statepanel(int indexProgramState)
        {
            if (indexProgramState == 0)
            {
                panel2.BackColor = Color.Yellow;
                label14.Text = " 初始化";
            }
            else if (indexProgramState == 1)
            {
                label14.Text = "計算中";
                panel2.BackColor = Color.Yellow;
            }
            else
            {
                panel2.BackColor = Color.GreenYellow;
                label14.Text = "  就緒";
            }
        }


        private void initialCUTeffi()
        {
            indexProgramState = 0;
            statepanel(indexProgramState);

            textBox4.Text = Convert.ToString(Convert.ToDouble(textBox1.Text) * 0.9);
            textBox2.Text = Convert.ToString(Convert.ToDouble(textBox4.Text) - (varNtest - 1) * 250);

            label4.Text = " ";
            label5.Text = " ";
            label6.Text = " ";
            label24.Text = " ";
            label25.Text = " ";
            label26.Text = " ";
            label27.Text = " ";
            label28.Text = " ";

            buttonStop.Visible = false;
            //SW_RMSData = new StreamWriter(System.Environment.CurrentDirectory + "\\logData\\RMSData.txt");
            //SW_State = new StreamWriter(System.Environment.CurrentDirectory + "\\logData\\State.txt");
            //MWArray[] result = initialDAQ.NIDAQ(2, 1, Convert.ToDouble(textBox1.Text));

            //Array myshowResult1 = result[0].ToArray();
            //Array myshowResult2 = result[1].ToArray();

            nidaq.StartDAQ(10000);
            Thread.Sleep(2000);
            nidaq.StopDAQ();

            indexProgramState = 2;
            statepanel(indexProgramState);
        }


        private void buttonStop_Click(object sender, EventArgs e)
        {
            nidaq.StopDAQ();
            buttonTest.Visible = true;
            buttonStop.Visible = false;
            indexProgramState = 2;
            statepanel(indexProgramState);
            Refresh();
        }


        //---------------------------------------------------------------------------------------//
        //------------------------------------- start test ---------------------------------------//
        //---------------------------------------------------------------------------------------//
        private void buttonTest_Click(object sender, EventArgs e)  //  開始測試鈕
        {
            buttonTest.Visible = false;
            buttonStop.Visible = true;

            indexProgramState = 1;
            statepanel(indexProgramState);
            Refresh();

            OperatingSPmax = Convert.ToDouble(textBox4.Text);
            //SW_RMSData = new StreamWriter(System.Environment.CurrentDirectory + "\\logData\\RMSData.txt");
            //SW_State = new StreamWriter(System.Environment.CurrentDirectory + "\\logData\\State.txt");
            ////Thread.Sleep(14000);
            //A[0] = 13000;
            //A[1] = 12750;
            //A[2] = 10750;
            //A[3] = 13500;

            threshold = Convert.ToDouble(textBox9.Text);
            
            nidaq.StartDAQ(OperatingSPmax);
            
            


            //nidaq.AA();

            //MWArray[] result_startDAQ = startDAQ.NIDAQ(2, 2, OperatingSPmax);

            //Array indexDAQMode = result_startDAQ[0].ToArray();
            //Array vecOptimalSP = result_startDAQ[1].ToArray();

            //double[] A = vecOptimalSP.OfType<double>().ToArray();

            //Thread.Sleep(3000);
            //double[] A = SPOptimization(OperatingSPmax);

            //label24.Text = Convert.ToString(A[0]);
            //label4.Text = Convert.ToString(A[1]);
            //label5.Text = Convert.ToString(A[2]);
            //label6.Text = Convert.ToString(A[3]);

            //double FeedPerFlute = Convert.ToDouble(textBox7.Text);
            //double Nunber_Flute = Convert.ToDouble(textBox8.Text);
            //label25.Text = Convert.ToString(A[0] * FeedPerFlute * Nunber_Flute);
            //label26.Text = Convert.ToString(A[1] * FeedPerFlute * Nunber_Flute);
            //label27.Text = Convert.ToString(A[2] * FeedPerFlute * Nunber_Flute);
            //label28.Text = Convert.ToString(A[3] * FeedPerFlute * Nunber_Flute);

            //indexProgramState = 2;
            //statepanel(indexProgramState);

        }

        private void buttonSetting_Click(object sender, EventArgs e)
        {
            if(indexPanelSetting == 0) {
                panelSetting.Visible = true;
                indexPanelSetting = 1;
            }
            else
            {
                panelSetting.Visible = false;
                indexPanelSetting = 0;
            }
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
            indexProccessMode = 0;
            checkBox1.Checked = true;
            checkBox2.Checked = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;

            if (string.IsNullOrEmpty(textBox1.Text)) { }
            else
            {
                textBox4.Text = Convert.ToString(Convert.ToDouble(textBox1.Text) * 0.9);
                if (Convert.ToDouble(textBox4.Text) - (varNtest - 1) * 250 > 0)
                {
                    textBox2.Text = Convert.ToString(Convert.ToDouble(textBox4.Text) - (varNtest - 1) * 250);
                }
                else
                {
                    textBox2.Text = "0";
                }
            }

        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                checkBox2.Checked = false;
                MessageBox.Show("請先輸入主軸的最高轉速", "Error"); 
                return;
            }
            else
            {
                textBox1.Enabled = false;
                indexProccessMode = 1;
                checkBox1.Checked = false;
                checkBox2.Checked = true;
                textBox3.Enabled = true;
                textBox4.Enabled = true;
            }

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox4.Text)) { }
            else
            {
                if (Convert.ToDouble(textBox4.Text) > Convert.ToDouble(textBox1.Text))
                {
                    textBox4.Text = Convert.ToString(Convert.ToDouble(textBox1.Text) * 0.9);
                    if (Convert.ToDouble(textBox4.Text) - (varNtest - 1) * 250 > 0)
                    {
                        textBox2.Text = Convert.ToString(Convert.ToDouble(textBox4.Text) - ( (varNtest - 1) * 250));
                    }
                    else
                    {
                        textBox2.Text = "0";
                    }
                }
                else
                {
                    if (Convert.ToDouble(textBox4.Text) - (varNtest - 1) * 250 > 0)
                    {
                        textBox2.Text = Convert.ToString(Convert.ToDouble(textBox4.Text) - (varNtest - 1) * 250);
                    }
                    else
                    {
                        textBox2.Text = "0";
                    }
                }
            }
            

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(textBox1.Text)) { }
            else
            {
                textBox4.Text = Convert.ToString(Convert.ToDouble(textBox1.Text)*0.9);
                if (Convert.ToDouble(textBox4.Text) - (varNtest - 1) * 250 > 0)
                {
                    textBox2.Text = Convert.ToString(Convert.ToDouble(textBox4.Text) - (varNtest - 1) * 250);
                }
                else
                {
                    textBox2.Text = "0";
                }
            }

         }

        private void CUTeffi_FormClosed(object sender, FormClosedEventArgs e)
        {
            FSS.Close();
        }

        private void CUTeffi_Shown(object sender, EventArgs e)
        {
            FSS.Hide();
        }

       


        //--------------------------------------------------------------------------------------------------------------//
        //------------------------------------- Function of root mean square ---------------------------------------//
        //-------------------------------------------------------------------------------------------------------------//
        private static double rootMeanSquare(double[] x)
        {
            double sum = 0;
            for (int i = 0; i < x.Length; i++)
            {
                sum += (x[i] * x[i]);
            }
            return Math.Sqrt(sum / x.Length);
        }


        //--------------------------------------------------------------------------------------------------------------//
        //-------------------------------- Function of optimized result exporting -----------------------------------//
        //--------------------------------------------------------------------------------------------------------------//
        private void buttonExport_Click(object sender, EventArgs e)
        {
            string[] SaveSP = new string[4];
            string[] SaveFeed = new string[4];

            SaveSP[0] = label24.Text;
            SaveSP[1] = label4.Text;
            SaveSP[2] = label5.Text;
            SaveSP[3] = label6.Text;

            SaveFeed[0] = label25.Text;
            SaveFeed[1] = label26.Text;
            SaveFeed[2] = label27.Text;
            SaveFeed[3] = label28.Text;



            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = "c:\\";
            sfd.Filter = "txt files (*.txt)|*.txt";
            sfd.ShowDialog();
            string saveFileName = sfd.FileName;

            if (string.IsNullOrEmpty(saveFileName)){}
            else {
                using (StreamWriter sw = new StreamWriter(saveFileName))
                {
                    sw.Write("刀號: ");
                    sw.WriteLine(textBox5.Text);
                    sw.Write("刀具直徑: ");
                    sw.Write(textBox6.Text);
                    sw.WriteLine(" mm");
                    sw.WriteLine("");

                    sw.WriteLine("建議參數");
                    sw.Write("S: ");
                    sw.Write(SaveSP[0]);
                    sw.Write(" RPM, ");
                    sw.Write("F: ");
                    sw.Write(SaveFeed[0]);
                    sw.WriteLine(" mm/min");
                    sw.WriteLine("");
                    sw.WriteLine("其他參數");


                    for (int i = 1; i < 4; i++)
                    {
                        sw.Write("S: ");
                        sw.Write(SaveSP[i]);
                        sw.Write(" RPM, ");
                        sw.Write("F: ");
                        sw.Write(SaveFeed[i]);
                        sw.WriteLine(" mm/min");
                    }
                }

            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s = Application.StartupPath;////

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = "c:\\";
            sfd.Filter = "txt files (*.txt)|*.txt";
            sfd.ShowDialog();
            string saveNC = sfd.FileName;

            if (string.IsNullOrEmpty(saveNC)) { }
            else
            {
                EditNC nc = new EditNC();
                double maxSP = Convert.ToDouble(textBox4.Text);
                double CuttingDepth = Convert.ToDouble(textBox3.Text);
                double ToolNumber = Convert.ToDouble(textBox5.Text);
                nc.editNC(maxSP, CuttingDepth, ToolNumber, s, saveNC);  //textbox4: maximum spindle speed, textbox3: cutting depth, textbox5: tool number
                Thread.Sleep(1000);
            }

        }

        private void checkBox3_Click(object sender, EventArgs e)
        {
            checkBox3.Checked = true;
            checkBox4.Checked = false;
            indexMaterial = 1;

            checkBox1.Enabled = true;
            checkBox2.Enabled = true;
            textBox2.Enabled = false;
            textBox4.Enabled = false;

            if (indexProccessMode == 0)
            {
                textBox2.Enabled = false;
                textBox4.Enabled = false;
                textBox3.Enabled = false;
            }
            else
            {
                textBox2.Enabled = false;
                textBox4.Enabled = false;
                textBox3.Enabled = true;
            }

            textBox4.Text = Convert.ToString(Convert.ToDouble(textBox1.Text) * 0.9);
            if (Convert.ToDouble(textBox4.Text) - (varNtest - 1) * 250 > 0)
            {
                textBox2.Text = Convert.ToString(Convert.ToDouble(textBox4.Text) - (varNtest - 1) * 250);
            }
            else
            {
                textBox2.Text = "0";
            }

        }

        private void checkBox4_Click(object sender, EventArgs e)
        {
            checkBox3.Checked = false;
            checkBox4.Checked = true;
            indexMaterial = 2;


            checkBox1.Enabled = false;
            checkBox2.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;

            textBox2.Text = "800";
            textBox4.Text = "3000";
            
        }

        public void panelupdate(double[] A)
        {

            label24.Text = Convert.ToString(A[0]);
            label4.Text = Convert.ToString(A[1]);
            label5.Text = Convert.ToString(A[2]);
            label6.Text = Convert.ToString(A[3]);

            double FeedPerFlute = Convert.ToDouble(textBox7.Text);
            double Nunber_Flute = Convert.ToDouble(textBox8.Text);
            label25.Text = Convert.ToString(A[0] * FeedPerFlute * Nunber_Flute);
            label26.Text = Convert.ToString(A[1] * FeedPerFlute * Nunber_Flute);
            label27.Text = Convert.ToString(A[2] * FeedPerFlute * Nunber_Flute);
            label28.Text = Convert.ToString(A[3] * FeedPerFlute * Nunber_Flute);

            indexProgramState = 2;
            statepanel(indexProgramState);
            buttonTest.Visible = true;
            buttonStop.Visible = false;
            Refresh();
        }
    }


}
