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

        private OxyPlot.WindowsForms.PlotView parseAndCreateGraph(string filePath, string graphType)
        {
            //https://www.youtube.com/watch?v=VC-nlI_stx4
            //^ Temp ref
            OxyPlot.WindowsForms.PlotView pv = new OxyPlot.WindowsForms.PlotView();
            //pv.Dock = DockStyle.Left;

            if (graphType.Equals("Accelerometer"))
            {
                pv.Location = new Point(10, 300);
                pv.Size = new Size(720, 200);
            }
            else if(graphType.Equals("Gyroscope"))
            {
                pv.Location = new Point(10, 500);
                pv.Size = new Size(720, 200);
            }

            this.Controls.Add(pv);

            OxyPlot.Axes.LinearAxis xAxis = new OxyPlot.Axes.LinearAxis();
            xAxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            xAxis.Maximum = 200;
            xAxis.Minimum = 0;

            OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis();
            yAxis.Position = OxyPlot.Axes.AxisPosition.Left;

            FunctionSeries fs = new FunctionSeries();
            PlotModel pm = new PlotModel();

            String line;
            StreamReader file = new StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                string[] parsedLine = line.Split(',');
                fs.Points.Add(new DataPoint(Convert.ToDouble(parsedLine[0]), Convert.ToDouble(parsedLine[1])));
            }

            pm.Series.Add(fs);
            pm.Axes.Add(xAxis);
            pm.Axes.Add(yAxis);
            pv.Model = pm;
            file.Close();
            return pv;
        }

        private void loadVideo(string filePath)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

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


           

