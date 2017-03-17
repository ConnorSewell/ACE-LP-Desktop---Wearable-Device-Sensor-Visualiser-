using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    using OxyPlot;
    using OxyPlot.Series;
    using System.IO;
    

    public partial class Form1 : Form
    {

        OxyPlot.WindowsForms.PlotView accelerometerPlot;
        OxyPlot.WindowsForms.PlotView gyroscopePlot;

        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
               

        }

        private void axWindowsMediaPlayer1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            MessageBox.Show("Key");
        }

        private OxyPlot.WindowsForms.PlotView parseAndCreateGraph(string filePath, string graphType)
        {
            //https://www.youtube.com/watch?v=VC-nlI_stx4
            //^ Temp ref
            OxyPlot.WindowsForms.PlotView pv = new OxyPlot.WindowsForms.PlotView();

            if (graphType.Equals("Accelerometer"))
            {
                pv.Location = new Point(18, 300);
                pv.Size = new Size(720, 200);
            }
            else if(graphType.Equals("Gyroscope"))
            {
                pv.Location = new Point(18, 500);
                pv.Size = new Size(720, 200);
            }

            pv.Controller = new OxyPlot.PlotController();
            pv.Controller.UnbindKeyDown(OxyKey.Right);
            pv.Controller.UnbindKeyDown(OxyKey.Left);

            if(graphType.Equals("Accelerometer"))
            {
                pv.Controller.BindKeyDown(OxyKey.D3, PlotCommands.PanLeft);
                pv.Controller.BindKeyDown(OxyKey.D4, PlotCommands.PanRight);
            }
            else if(graphType.Equals("Gyroscope"))
            {
                pv.Controller.BindKeyDown(OxyKey.D5, PlotCommands.PanLeft);
                pv.Controller.BindKeyDown(OxyKey.D6, PlotCommands.PanRight);
            }
            this.Controls.Add(pv);

            OxyPlot.Axes.LinearAxis xAxis = new OxyPlot.Axes.LinearAxis();
            xAxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            xAxis.Maximum = 10;
            xAxis.Minimum = 0;

            OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis();
            yAxis.Position = OxyPlot.Axes.AxisPosition.Left;

            FunctionSeries fs = new FunctionSeries();
            PlotModel pm = new PlotModel();

            double currVal = 0.00;

            String line;
            StreamReader file = new StreamReader(filePath);

            line = file.ReadLine();
            string[] parsedLine = line.Split(',');
            double startValNegation = (Convert.ToDouble(parsedLine[3]));
            double lastTime = 0;
            fs.Points.Add(new DataPoint(0, Convert.ToDouble(parsedLine[0])));

            int i = 1;
            Random rand = new Random();
            while ((line = file.ReadLine()) != null)
            {
                parsedLine = line.Split(',');
                if (parsedLine.Length == 4)
                {
                    if(Convert.ToDouble(parsedLine[3]) > lastTime)
                    {
                        double timeStamp = Convert.ToDouble(parsedLine[3]) - startValNegation;
                        double seconds = timeStamp / 1000000000.0;
                        lastTime = timeStamp;
                        fs.Points.Add(new DataPoint(seconds, Convert.ToDouble(parsedLine[0])));
                    }
                }
                //fs.Points.Add(new DataPoint(i, rand.Next(0, 1s0)));
                //Console.WriteLine("Val 1: " + seconds + " Val 2: " + parsedLine[0]);
            }

            pm.Series.Add(fs);
            pm.Axes.Add(xAxis);
            pm.Axes.Add(yAxis);
            pv.Model = pm;
            //pv.Model.Han
            file.Close();
            return pv;
        }

        //a
        
        private void keyDown(object sender, OxyKeyEventArgs e)
        {

        }

        private void loadVideo(string filePath)
        {
            MessageBox.Show("Whew Video: " + filePath);

            axWindowsMediaPlayer1.URL = filePath;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.uiMode = "none";
        }

        private void loadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //Using http://stackoverflow.com/questions/11624298/how-to-use-openfiledialog-to-select-a-folder
            //^ For opening folder and parsing file names. Accessed: 16/03/2017 @ 20:10
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            DialogResult result = openFolderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string[] sensorFiles = Directory.GetFiles(openFolderDialog.SelectedPath);
                for (int i = 0; i < sensorFiles.Length; i++)
                {
                    string sensorFile = sensorFiles[i].Substring(sensorFiles[i].LastIndexOf("\\") + 1);

                    switch (sensorFile)
                    {
                        case "AccelerometerData.txt":
                            accelerometerPlot = parseAndCreateGraph(sensorFiles[i], "Accelerometer");
                            break;
                        case "GyroscopeData.txt":
                            gyroscopePlot = parseAndCreateGraph(sensorFiles[i], "Gyroscope");
                            break;
                        case "Video.mp4":
                            loadVideo(sensorFiles[i]);
                            break;
                        default:
                            //MessageBox.Show(sensorFile + " is not a valid Sensor Data File");
                            break;
                    }
                }
            }
        }

        private void Form1_Load_2(object sender, EventArgs e)
        {

        }
    }

  
}


           

