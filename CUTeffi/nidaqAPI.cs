using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NationalInstruments.DAQmx;
using NationalInstruments;
using System.Diagnostics;
using MathNet.Numerics;
using System.IO;
using System.Threading;
using CUTeffi;

namespace CUTeffi
{
    class nidaqAPI
    {
        
        private AnalogMultiChannelReader analogInReader;
        private AIExcitationSource excitationSource;
        private AIAccelerometerSensitivityUnits sensitivityUnits;
        private AITerminalConfiguration terminalConfiguration;
        private AnalogEdgeStartTriggerSlope triggerSlope;
        private AICoupling inputCoupling;

        private NationalInstruments.DAQmx.Task myTask;
        private NationalInstruments.DAQmx.Task runningTask;
        private AsyncCallback analogCallback;
        private AnalogWaveform<double>[] data;

        int indexP;
        double SPmax;
        int iii;
        
        public StreamWriter SW_RMSData;
        public StreamWriter SW_State;

        [STAThread]
        public void StartDAQ(double a)
        {

            triggerSlope = AnalogEdgeStartTriggerSlope.Rising;
            sensitivityUnits = AIAccelerometerSensitivityUnits.MillivoltsPerG;
            terminalConfiguration = (AITerminalConfiguration)(-1);
            excitationSource = AIExcitationSource.Internal;
            inputCoupling = AICoupling.AC;

            myTask = new NationalInstruments.DAQmx.Task();
            AIChannel aiChannel;

            SPmax = a;
            double Vmin = -5;
            double Vmax = 5;
            double sen = 100;
            double EVN = 0.004;
            double[] chan = new double[4] { 1, 1, 0, 0 };
            ////
            indexP = 1;
            iii = 0;
            ////
            
            SW_RMSData = new StreamWriter(System.Environment.CurrentDirectory + "\\logData\\RMSData.txt");
            SW_State = new StreamWriter(System.Environment.CurrentDirectory + "\\logData\\State.txt");
            
            for (int i = 0; i < chan.Length; i++)
            {
                if (chan[i] == 1)
                {
                    aiChannel = myTask.AIChannels.CreateAccelerometerChannel("cDAQ1Mod1/ai" + Convert.ToString(i), "",
                        terminalConfiguration, Vmin, Vmax, sen, sensitivityUnits, excitationSource,
                        EVN, AIAccelerationUnits.G);
                    aiChannel.Coupling = inputCoupling;
                }

            }
            
            myTask.Timing.ConfigureSampleClock("", Convert.ToDouble(12800),
                SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, Convert.ToInt32(1280));

            myTask.Control(TaskAction.Verify);

            runningTask = myTask;
            analogInReader = new AnalogMultiChannelReader(myTask.Stream);
            analogCallback = new AsyncCallback(AnalogInCallback);



            analogInReader.SynchronizeCallbacks = true;
            analogInReader.BeginReadWaveform(Convert.ToInt32(1280), analogCallback, myTask);

        }

        public void StopDAQ()
        {
            // Dispose of the task
            SW_RMSData.Dispose();
            SW_State.Dispose();


            runningTask = null;
            myTask.Dispose();

            ////
            //indexP = 0;
            //iii = 0;
            ////
        }
        
        
        private void AnalogInCallback(IAsyncResult ar)
        {
            if (runningTask != null && runningTask == ar.AsyncState)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                // Read the available data from the channels
                data = analogInReader.EndReadWaveform(ar);

                double[] vecTime = new double[data[0].SampleCount];

                double[] d00 = data[0].GetRawData();
                double[] d10 = data[1].GetRawData();
                PrecisionDateTime[] T = data[0].GetPrecisionTimeStamps();

                double[] d0 = new double[d00.Length];
                double[] d1 = new double[d10.Length];

                for(int i = 0; i < d0.Length; i++)
                {
                    d0[i] = (d00[i] - d00.Average())*10;
                    d1[i] = (d10[i] - d10.Average())*10;
                }

                for (int i = 0; i < data[0].SampleCount; i++)
                {
                    vecTime[i] = T[i].TimeOfDay.TotalSeconds;
                }
                double varFs = 1 / ((vecTime[data[0].SampleCount - 1] - vecTime[0]) / data[0].SampleCount);

                int indexMaterial = CUTeffiForm.indexMaterial;

                double rms_xdata = rootMeanSquare(d0)*10;
                double rms_ydata = rootMeanSquare(d1)*10;
                double rms_vibration = Math.Sqrt(Math.Pow(rms_xdata, 2) + Math.Pow(rms_ydata, 2));


                ////////////////////////////FFT////////////////////////////////////////
                //double[] d0Copy = new double[d0.Length * 10];

                //for (int i = 0; i < d0.Length; i++)
                //{
                //    d0Copy[i] = d0[i];
                //}
                //double[] realdata = d0Copy;
                //double[] imagdata = new double[d0Copy.Length];

                //int numFFT = Convert.ToInt16(Math.Floor(Convert.ToDecimal(d0Copy.Length / 2)));
                //double[] vecFFT = new double[numFFT];
                //double[] vecFqtmp = new double[numFFT];

                //for(int i = 1; i<numFFT; i++)
                //{
                //    vecFqtmp[i] = vecFqtmp[i - 1] + (varFs / d0Copy.Length);
                //}                

                //MathNet.Numerics.IntegralTransforms.Fourier.Forward(realdata, imagdata, MathNet.Numerics.IntegralTransforms.FourierOptions.Matlab);

                //for (int i = 0; i < numFFT; i++)
                //{
                //    vecFFT[i] = Math.Sqrt(Math.Pow(realdata[i], 2) + Math.Pow(imagdata[i], 2));
                //}

                //double varSP;
                //if(indexP == 0)
                //{
                //    int numvecFFT2 = Convert.ToInt16(Math.Floor((SPmax + 200) / 60) - 10);
                //    double[] vecFFT2 = new double[numvecFFT2];
                //    Array.Copy(vecFFT, 10, vecFFT2, 0, numvecFFT2);
                //    int loc = vecFFT2.ToList().IndexOf(vecFFT2.Max());
                //    varSP = vecFqtmp[loc + 10] * 60;
                //}
                //else
                //{
                //    int numvecFFT2 = Convert.ToInt16(Math.Floor((SPmax + 200) / 60) - 10);
                //    double[] vecFFT2 = new double[numvecFFT2];
                //    Array.Copy(vecFFT, 10, vecFFT2, 0, numvecFFT2);
                //    int loc = vecFFT2.ToList().IndexOf(vecFFT2.Max());
                //    varSP = vecFqtmp[loc + 10] * 60;
                //}
                /////////////////////
                //{
                //    int numvecFFT2 = Convert.ToInt16(Math.Floor((SPmax + 1000) / 60) - Math.Floor((SPmax - 5000) / 60) - 1);
                //    double[] vecFFT2 = new double[numvecFFT2];
                //    Array.Copy(vecFFT, Convert.ToInt16(Math.Floor((SPmax - 5000) / 60) - 1), vecFFT2, 0, numvecFFT2);
                //    int loc = vecFFT2.ToList().IndexOf(vecFFT2.Max());
                //    varSP = vecFqtmp[loc + Convert.ToInt16(Math.Floor((SPmax - 5000) / 60) - 1)] * 60;
                //}
                double varSP;
                //SW_RMSData.Write(vecTime[0]);
                //SW_RMSData.Write(",");
                //SW_RMSData.Write(varSP);
                //SW_RMSData.Write(",");
                //SW_RMSData.WriteLine(rms_vibration);

                double[] vecSP = new double[12];
                if (indexMaterial == 1)
                {
                    double[] vecSP2 = new double[] { 0, 2000, 750, 2500, 250, 1750, 1000, 2750, 1250, 2250, 1500, 500 };
                    for (int i = 0; i < 12; i++)
                    {
                        vecSP[i] = SPmax - vecSP2[i];//---------------------------------------------------------------------------------------------
                    }
                }
                else if (indexMaterial == 2)
                {
                    double[] vecSP2 = new double[] { 0, 1600, 600, 2000, 200, 1400, 800, 2200, 1000, 1800, 1200, 400 };
                    for (int i = 0; i < 12; i++)
                    {
                        vecSP[i] = SPmax - vecSP2[i];//---------------------------------------------------------------------------------------------
                    }
                }
                //for (int i = 12; i >= 1; i--)
                //{
                //    vecSP[i - 1] = SPmax - 250 * (13 - i - 1);//---------------------------------------------------------------------------------------------
                //}
                ////////////////////////////////////////////////////
                //double[] varSPP = new double[vecSP.Length];
                //for (int i = 0; i < vecSP.Length; i++)
                //{
                //    varSPP[i] = vecSP[i];
                //}
                double threshold = CUTeffiForm.threshold;
                if (rms_vibration > threshold) // 閥值定義：轉與不轉振動量大小
                {
                    
                    //if (varSP >= 800)
                    //{
                    if (indexP == 0)
                    {
                        indexP = 1;
                    }
                    else if (indexP == vecSP.Length +1)
                    {
                        //if (Math.Abs(varSP - vecSP[11]) >= 1000)indexP >= 12 && rms_vibration < 6
                        //if (vecSP[indexP-2] == 13000)
                        //    {
                                SW_State.WriteLine(vecTime[0]);
                                StopDAQ();
                                SPOptimization(SPmax);
                        return;
                        //}
                    }
                    varSP = vecSP[indexP - 1];
                    if (indexP <= vecSP.Length)
                        {
                            
                            if (Math.Abs(varSP - vecSP[indexP-1]) <= 120)//Math.Abs(varSP - vecSP[indexP - 1]) <= 120    varSPP[indexP - 1] == vecSP[indexP - 1]
                            {
                                    iii = iii + 1;
                                    Console.WriteLine(iii);
                                    if (iii == 10)
                                    {
                                        iii = 0;
                                        indexP = indexP + 1;
                                        SW_State.WriteLine(vecTime[0]);
                                        //Console.WriteLine(indexP + " , " + rms_vibration + " , " + varSPP[indexP-1] + " , " + vecSP[indexP-1] + " , " + iii);
                                    }
                            }
                        //}
                    }
                    SW_RMSData.Write(vecTime[0]);
                    SW_RMSData.Write(",");
                    SW_RMSData.Write(varSP);
                    SW_RMSData.Write(",");
                    SW_RMSData.WriteLine(rms_vibration);
                    Console.Write(indexP + "  " + varSP + "  " + vecTime[0] + "  " + rms_vibration + " ");
                    threshold = CUTeffiForm.threshold;
                    
                    
                }
                else
                {
                    Console.WriteLine("Spindle stop now");
                }
                
                ///////////////////////////////////////////////////////////////////////////
                //if (rms_vibration >= 0.2)
                //{
                //    if (varSP >= 800)
                //    {
                //        if (indexP == 0)
                //        {
                //            indexP = 1;
                //        }
                //        else if (indexP == vecSP.Length + 1)
                //        {
                //            if (Math.Abs(varSP - vecSP[11]) >= 1000)
                //            {
                //                SW_State.WriteLine(vecTime[0]);
                //                StopDAQ();


                //                SPOptimization(SPmax);

                //            }
                //        }

                //        if (indexP <= vecSP.Length)
                //        {
                //            if (Math.Abs(varSP - vecSP[indexP - 1]) <= 120)
                //            {
                //                iii = iii + 1;
                //                if (iii == 10)
                //                {
                //                    iii = 0;
                //                    indexP = indexP + 1;
                //                    SW_State.WriteLine(vecTime[0]);
                //                }
                //            }
                //        }
                //    }
                //}



                ////////////////////////////////////////////////////////////////////////

                sw.Stop();
                TimeSpan ts2 = sw.Elapsed;
                //Console.Write(indexP + "  " + varSP + "  " + vecTime[0] + "  ");
                Console.WriteLine(ts2);

                analogInReader.BeginMemoryOptimizedReadWaveform(Convert.ToInt32(3200),
                    analogCallback, myTask, data);
                int AAA = 1;
            }
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

        public void AA()
        {
            SPOptimization(8000);
        }

        //------------------------------------------------------------------------------------------------------------------------//
        //------------------------------------- Function of spindle speed optimization ---------------------------------------//
        //-----------------------------------------------------------------------------------------------------------------------//
        public void SPOptimization(double maxSP)
        //public static double SPOptimization(double maxSP)
        {

            //------------------------------------- Read log_State ---------------------------------------//
            int counter1 = 0;
            string line1;
            StreamReader SR_State = new StreamReader(System.Environment.CurrentDirectory + "\\logdata\\State.txt");
            List<double> DataRead1 = new List<double>();

            while ((line1 = SR_State.ReadLine()) != null)
            {
                DataRead1.Add(Convert.ToDouble(line1));
                //Console.WriteLine(DataRead[counter]);
                counter1++;
            }
            double[] vecState = new double[counter1-1];



            for (int i = 0; i < vecState.Length; i++)//counter1
            {
                vecState[i] = DataRead1[i];
            }
            //string APPpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            //string PathName = APPpath;
            //PathName = new Uri(PathName).LocalPath;
            //FileStream File_log_State = File.Open(PathName + "\\log_State.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            ////FileStream File_log_State = File.Open(@"C:\Users\User\source\repos\log_State.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //BinaryReader Reader_log_State = new BinaryReader(File_log_State);
            //int dlState = System.Convert.ToInt16(File_log_State.Length);
            //byte[] byteState = Reader_log_State.ReadBytes(dlState);

            //byte[] byteState2 = new byte[8];
            //double[] vecState = new double[dlState / 8];
            //for (int i = 1; i <= dlState / 8; i++)
            //{
            //    for (int j = 1; j <= 8; j++)
            //    {
            //        byteState2[j - 1] = byteState[8 * (i - 1) + j - 1];
            //    }
            //    vecState[i - 1] = BitConverter.ToDouble(byteState2, 0);
            //}
            //Reader_log_State.Close();
            //File_log_State.Close();

            //------------------------------------- Read log_RMSData ---------------------------------------//
            int counter = 0;
            string line;
            StreamReader SR_RMSData = new StreamReader(System.Environment.CurrentDirectory + "\\logData\\RMSData.txt");
            List<List<double>> DataRead = new List<List<double>>();

            while ((line = SR_RMSData.ReadLine()) != null)
            {
                string[] split;
                split = line.Split(new char[] { ',' });
                DataRead.Add(new List<double>() { Convert.ToDouble(split[0]), Convert.ToDouble(split[1]),
                    Convert.ToDouble(split[2]) });
                //Console.WriteLine(DataRead[counter]);
                counter++;
            }
            double[] vecTime = new double[counter];
            double[] SpindleSpeed = new double[counter];
            double[] RMSData = new double[counter];


            for (int i = 0; i < counter; i++)
            {
                vecTime[i] = DataRead[i][0];
                SpindleSpeed[i] = DataRead[i][1];
                RMSData[i] = DataRead[i][2];
            }
            SR_RMSData.Dispose();
            SR_RMSData.Close();

            SR_State.Close();
            //FileStream File_log_RMSData = File.Open(PathName + "\\log_RMSData.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //BinaryReader Reader_log_RMSData = new BinaryReader(File_log_RMSData);
            //int dlRMSData = System.Convert.ToInt16(File_log_RMSData.Length);
            //byte[] byteRMSData = Reader_log_RMSData.ReadBytes(dlRMSData);

            //byte[] byteRMSData2 = new byte[8];
            //double[] vecRMSData = new double[dlRMSData / 8];
            //for (int i = 1; i <= dlRMSData / 8; i++)
            //{
            //    for (int j = 1; j <= 8; j++)
            //    {
            //        byteRMSData2[j - 1] = byteRMSData[8 * (i - 1) + j - 1];
            //    }
            //    vecRMSData[i - 1] = BitConverter.ToDouble(byteRMSData2, 0);
            //}

            //Reader_log_RMSData.Close();
            //File_log_RMSData.Close();

            //double[] vecTime = new double[dlRMSData / 8 / 3];
            //double[] SpindleSpeed = new double[dlRMSData / 8 / 3];
            //double[] RMSData = new double[dlRMSData / 8 / 3];

            //for (int j = 1; j <= dlRMSData / 8 / 3; j++)
            //{
            //    vecTime[j - 1] = vecRMSData[3 * (j - 1)];
            //    SpindleSpeed[j - 1] = vecRMSData[3 * (j - 1) + 1];
            //    RMSData[j - 1] = vecRMSData[3 * (j - 1) + 2];
            //}
            //------------------------------------- Spindle speed optimization ---------------------------------------//
            int indexMaterial = CUTeffiForm.indexMaterial;
            double[] vecSP = new double[12];
            if (indexMaterial == 1)
            {
                double[] vecSP2 = new double[] { 0, 2000, 750, 2500, 250, 1750, 1000, 2750, 1250, 2250, 1500, 500 };
                for (int i = 0; i < 12; i++)
                {
                    vecSP[i] = maxSP - vecSP2[i];//---------------------------------------------------------------------------------------------
                }
            }
            else if (indexMaterial == 2)
            {
                double[] vecSP2 = new double[] { 0, 1600, 600, 2000, 200, 1400, 800, 2200, 1000, 1800, 1200, 400 };
                for (int i = 0; i < 12; i++)
                {
                    vecSP[i] = maxSP - vecSP2[i];//---------------------------------------------------------------------------------------------
                }
            }
            //double[] vecSP = new double[12];//-------------------------------------------------------------------------------------------------
            int[] vecLoc = new int[vecState.Length];
            //for (int i = 12; i >= 1; i--)
            //{
            //    vecSP[i - 1] = maxSP - 250 * (13 - i - 1);
            //}

            for (int i = 1; i <= vecState.Length; i++)
            {
                vecLoc[i - 1] = Array.IndexOf(vecTime, vecState[i - 1]); ;
            }

            double[] Crms = new double[vecSP.Length];
            for (int i = 1; i < vecSP.Length; i++)
            {
                int varRange = vecLoc[i] - vecLoc[i - 1];
                int varLoc1 = Convert.ToInt32(Math.Floor(varRange * 0.8));
                int varLoc2 = Convert.ToInt32(Math.Floor(varRange * 0.1));
                double[] arrayA = new double[varLoc1 - varLoc2 + 1];
                Array.Copy(RMSData, vecLoc[i] - varLoc1, arrayA, 0, varLoc1 - varLoc2 + 1);
                double varA = rootMeanSquare(arrayA);
                Crms[i - 1] = varA;
            }

            double[] OptimizedSP = new double[4];
            double[] arrayB = new double[Crms.Length];
            Array.Copy(Crms, 0, arrayB, 0, Crms.Length);
            for (int i = 1; i <= 4; i++)
            {
                int varB = Array.IndexOf(arrayB, arrayB.Min());
                OptimizedSP[i - 1] = vecSP[varB];
                arrayB[varB] = 1000000000;
            }
            //return OptimizedSP;
            double[] A = CUTeffiForm.A;
            A = OptimizedSP;
            SplashScreen.cut.panelupdate(A);
            //CUTeffiForm CE = new CUTeffiForm();
            //CE.panelupdate(A);

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
    }
}
