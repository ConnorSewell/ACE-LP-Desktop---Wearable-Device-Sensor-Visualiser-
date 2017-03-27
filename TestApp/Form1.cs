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
    using System.Threading;
    using System.Timers;

    public partial class Form1 : Form
    {

        
        List<OxyPlot.WindowsForms.PlotView> accelerometerPlots = new List<OxyPlot.WindowsForms.PlotView>();
        List<OxyPlot.WindowsForms.PlotView> gyroscopePlots = new List<OxyPlot.WindowsForms.PlotView>();
        List<TabPage> tabs = new List<TabPage>();

        Boolean allPlaying = false;

        AxWMPLib.AxWindowsMediaPlayer mediaOne = new AxWMPLib.AxWindowsMediaPlayer();
       

        public Form1()
        {
            InitializeComponent();
            mediaOne = new AxWMPLib.AxWindowsMediaPlayer();
            //pv = 
            

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

            OxyPlot.Axes.LinearAxis xAxis = new OxyPlot.Axes.LinearAxis();
            OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis();
            OxyPlot.WindowsForms.PlotView pv = new OxyPlot.WindowsForms.PlotView();
            PlotModel pm = new PlotModel();

            if (graphType.Equals("Accelerometer"))
            {
                pv.Location = new Point(25, 335);
                pv.Size = new Size(705, 150);
            }
            else if(graphType.Equals("Gyroscope"))
            {
                pv.Location = new Point(25, 500);
                pv.Size = new Size(705, 150);
            }

            pv.Controller = new OxyPlot.PlotController();
            pv.Controller.UnbindKeyDown(OxyKey.Right);
            pv.Controller.UnbindKeyDown(OxyKey.Left);

           
            //this.Controls.Add(pv);

            xAxis = new OxyPlot.Axes.LinearAxis();
            xAxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            xAxis.Maximum = 5;
            xAxis.Minimum = 0;

            yAxis = new OxyPlot.Axes.LinearAxis();
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

            if (graphType.Equals("Accelerometer"))
            {
                accelerometerPlots.Add(pv);
                //pv.Controller.BindKeyDown(OxyKey.D3, PlotCommands.PanLeft);
                //pv.Controller.BindKeyDown(OxyKey.D4, PlotCommands.PanRight);
            }
            else if (graphType.Equals("Gyroscope"))
            {
                gyroscopePlots.Add(pv);
                //pv.Controller.BindKeyDown(OxyKey.D5, PlotCommands.PanLeft);
                //pv.Controller.BindKeyDown(OxyKey.D6, PlotCommands.PanRight);
            }

            file.Close();
            return pv;
        }


        Label timeElapsed = new Label();
        TrackBar trackBar = new TrackBar();
        AxWMPLib.AxWindowsMediaPlayer mediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
        TabPage tab = new TabPage();
        Button startAllButton = new Button();

        private void loadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mediaPlayer.Location = new Point(25, 60);
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
                            //tab.Controls.
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

            startAllButton.Location = new Point(15, 6);
            startAllButton.Size = new Size(50, 20);
            startAllButton.Text = "Start";
            startAllButton.Click += new System.EventHandler(playAllData);
            tab.Controls.Add(startAllButton);

            trackBar.Location = new Point(68, 5);
            trackBar.Size = new Size(645, 10);
            trackBar.TickStyle = 0;
            trackBar.TickFrequency = 0;
            trackBar.SmallChange = 0;
            trackBar.LargeChange = 0;
            trackBar.Maximum = 300;
            
            //trackBar.TickFrequency = 0;
            //trackBar.Value = 60;

            trackBar.ValueChanged += new System.EventHandler(trackBar_ValueChanged);
            tab.Controls.Add(trackBar);

            timeElapsed.Location = new Point(715, 8);
            timeElapsed.Text = "00:00:00";
            tab.Controls.Add(timeElapsed);
            //tabs.Add(tab);
            tabControl1.Controls.Add(tab);

            mediaPlayer.settings.autoStart = true;
            //mediaPlayer.settigs...
            mediaPlayer.URL = videoURL;
            mediaPlayer.Ctlcontrols.stop();
            //mediaPlayer.Ctlcontrols.stop();
           
        }

        int hours;
        int mins;

        string convertedHours;
        string convertedMins;
        string convertedSecs;

        double currentVal;
        int currentPosition;
        System.Timers.Timer timer;

        private void playAllData(object sender, System.EventArgs e)
        {
            if (allPlaying)
            {
                startAllButton.Text = "Start";
                allPlaying = false;
                timer.Stop();
                timer = null;
                mediaPlayer.Ctlcontrols.stop();
            }
            else
            {
                startAllButton.Text = "Stop";
                mediaPlayer.Ctlcontrols.currentPosition = currentPosition;
                mediaPlayer.Ctlcontrols.play();
                allPlaying = true;

                Thread threadTest = new Thread(threadWait);
                threadTest.Start();

                WMPLib.WMPPlayState lol = mediaPlayer.playState;
                Console.WriteLine("Lol: " + lol.ToString());
               
            }
        }

        private void threadWait()
        {
            Boolean test = true;
            while (test)
            {
                MethodInvoker methodInvoker = delegate
                {
                    if (mediaPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        //mediaPlayer.Ctlcontrols.pause();
                        
                        timer = new System.Timers.Timer();
                        timer.Interval = 1000;
                        timer.Elapsed += periodicGraphUpdate;
                        timer.Enabled = true;
                        mediaPlayer.Ctlcontrols.currentPosition = currentPosition;
                        timer.AutoReset = true;
                        test = false;
                    }
                };

                this.Invoke(methodInvoker);
            }
        }

        private void periodicGraphUpdate(Object source, ElapsedEventArgs e)
        {
            currentPosition += 1;
          
            //http://stackoverflow.com/questions/14890295/update-label-from-another-thread
            //^ Accessed: 27/03/2017 @ 03:30
            MethodInvoker methodInvoker = delegate
            {
                this.trackBar.Value = currentPosition;
            };

            this.Invoke(methodInvoker);
        }

        int currentTimerVal;
        private void trackBar_ValueChanged(object sender, System.EventArgs e)
        {
            currentPosition = trackBar.Value;
            currentVal = trackBar.Value;
            

            if (allPlaying)
            {
                if (mediaPlayer.Ctlcontrols.currentPosition > currentPosition + 0.02)
                {
                    mediaPlayer.Ctlcontrols.pause();
                    Thread syncer = new Thread(syncSleeper);
                    syncer.Start();
                    //mediaPlayer.Ctlcontrols.currentPosition = currentPosition;
                    Console.WriteLine("Media player ahead by at least 0.02");
                }
                else if (mediaPlayer.Ctlcontrols.currentPosition < currentPosition - 0.02)
                {
                    currentVal = mediaPlayer.Ctlcontrols.currentPosition;
                    Console.WriteLine("Media player behind by at least 0.02");
                }
            }


            gyroscopePlots[0].Model.Axes.ElementAt(0).Maximum = currentVal;
            gyroscopePlots[0].Model.Axes.ElementAt(0).Minimum = currentVal - 5;

            accelerometerPlots[0].Model.Axes.ElementAt(0).Maximum = currentVal;
            accelerometerPlots[0].Model.Axes.ElementAt(0).Minimum = currentVal - 5;

            Console.WriteLine(accelerometerPlots[0].Model.Axes.ElementAt(0).Maximum);

            gyroscopePlots[0].InvalidatePlot(true);
            accelerometerPlots[0].InvalidatePlot(true);

            if(!allPlaying)
            mediaPlayer.Ctlcontrols.currentPosition = currentVal;

            currentTimerVal = Convert.ToInt32(currentVal);

            while (currentTimerVal >= 3600)
            {
                hours++;
                currentTimerVal -= 3600;
            }

            while(currentTimerVal >= 60)
            {
                mins++;
                currentTimerVal -= 60;
            }

            if(hours < 10)
            {
                convertedHours = "0" + hours.ToString();
            }

            if(mins < 10)
            {
                convertedMins = "0" + mins.ToString();
            }

            convertedSecs = currentTimerVal.ToString();

            if (currentTimerVal < 10)
            {
                convertedSecs = "0" + convertedSecs;
            }

            timeElapsed.Text = convertedHours + ":" + convertedMins + ":" + convertedSecs;
            hours = 0;
            mins = 0;
          
        }

        private void syncSleeper()
        {
            Thread.Sleep(15);
                MethodInvoker methodInvoker = delegate
                {
                    mediaPlayer.Ctlcontrols.play();
                };

                this.Invoke(methodInvoker);
            
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


           

