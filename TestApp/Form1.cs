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
        List<TabPage> tabs = new List<TabPage>();
        

        AxWMPLib.AxWindowsMediaPlayer mediaOne = new AxWMPLib.AxWindowsMediaPlayer();
       
        public Form1()
        {
            InitializeComponent();
            mediaOne = new AxWMPLib.AxWindowsMediaPlayer();
          
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
                pv.Location = new Point(25, 280);
                pv.Size = new Size(705, 200);
            }
            else if(graphType.Equals("Gyroscope"))
            {
                pv.Location = new Point(25, 485);
                pv.Size = new Size(705, 200);
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
            xAxis.Maximum = 5;
            xAxis.Minimum = 0;

            OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis();
            yAxis.Position = OxyPlot.Axes.AxisPosition.Left;
            //yAxis.AbsoluteMaximum = 10;
            //yAxis.AbsoluteMinimum = -10;

            FunctionSeries xSeries = new FunctionSeries();
            FunctionSeries ySeries = new FunctionSeries();
            FunctionSeries zSeries = new FunctionSeries();

            xSeries.StrokeThickness = 0.1;
            xSeries.Color = OxyColor.FromRgb(139, 0, 0);
           
            ySeries.StrokeThickness = 0.1;
            ySeries.Color = OxyColor.FromRgb(50, 100, 0);
          
            zSeries.StrokeThickness = 0.1;
            zSeries.Color = OxyColor.FromRgb(25, 25, 112);
    

            PlotModel pm = new PlotModel();
            pm.TextColor = OxyColor.FromRgb(0, 0, 0);

            pv.BackColor = Color.White;

            double maxY = 0;
            double minY = 0;

            string line;
            StreamReader file = new StreamReader(filePath);
            line = file.ReadLine();
            line = file.ReadLine();
            string[] parsedLine = line.Split(',');
            double startValNegation = (Convert.ToDouble(parsedLine[3]));
            double lastTime = 0;
            xSeries.Points.Add(new DataPoint(0, Convert.ToDouble(parsedLine[0])));
            ySeries.Points.Add(new DataPoint(0, Convert.ToDouble(parsedLine[1])));
            zSeries.Points.Add(new DataPoint(0, Convert.ToDouble(parsedLine[2])));

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
                        xSeries.Points.Add(new DataPoint(seconds, Convert.ToDouble(parsedLine[0])));
                        ySeries.Points.Add(new DataPoint(seconds, Convert.ToDouble(parsedLine[1])));
                        zSeries.Points.Add(new DataPoint(seconds, Convert.ToDouble(parsedLine[2])));
                        
                    }
                }
         
            }

            pm.Series.Add(xSeries);
            pm.Series.Add(ySeries);
            pm.Series.Add(zSeries);

            //yAxis.AbsoluteMaximum = maxY;
            //yAxis.AbsoluteMinimum = minY;

            Console.WriteLine("Min: " + minY);
            Console.WriteLine("Max: " + maxY);

            System.Diagnostics.Debug.WriteLine("Min: " + minY);
            System.Diagnostics.Debug.WriteLine("Max: " + maxY);

            pm.Axes.Add(xAxis);
            pm.Axes.Add(yAxis);
            pv.Model = pm;

            file.Close();
            return pv;
        }

      

        private void loadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage tab = new TabPage();

            AxWMPLib.AxWindowsMediaPlayer mediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            mediaPlayer.Location = new Point(25, 5);
            mediaPlayer.Size = new Size(370, 260);

            //Using http://stackoverflow.com/questions/11624298/how-to-use-openfiledialog-to-select-a-folder
            //^ For opening folder and parsing file names. Accessed: 16/03/2017 @ 20:10
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            DialogResult result = openFolderDialog.ShowDialog();

            string videoURL = ""; 

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string[] sensorFiles = Directory.GetFiles(openFolderDialog.SelectedPath);
                string folderName = openFolderDialog.SelectedPath.Substring(openFolderDialog.SelectedPath.LastIndexOf("\\") + 1);
                tab.Text = folderName;
                for (int i = 0; i < sensorFiles.Length; i++)
                {
                    string sensorFile = sensorFiles[i].Substring(sensorFiles[i].LastIndexOf("\\") + 1);

                    switch (sensorFile)
                    {
                        case "AccelerometerData.txt":
                            tab.Controls.Add(parseAndCreateGraph(sensorFiles[i], "Accelerometer"));
                            break;
                        case "GyroscopeData.txt":
                            tab.Controls.Add(parseAndCreateGraph(sensorFiles[i], "Gyroscope"));
                            break;
                        case "Video.mp4":
                            tab.Controls.Add(mediaPlayer);
                            videoURL = sensorFiles[i];
                            break;
                        default:
                            MessageBox.Show(sensorFile + " is not a valid Sensor Data File");
                            break;
                    }
                }
            }

            tabs.Add(tab);
            tabControl1.Controls.Add(tab);
            mediaPlayer.URL = videoURL;
        }

        private void Form1_Load_2(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }  
}


           

