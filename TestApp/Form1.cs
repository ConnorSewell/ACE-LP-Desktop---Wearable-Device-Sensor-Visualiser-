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
    using System.Media;
    using System.Threading;
    using System.Timers;

    public partial class Form1 : Form
    {

     
        List<OxyPlot.WindowsForms.PlotView> accelerometerPlots = new List<OxyPlot.WindowsForms.PlotView>();
        List<OxyPlot.WindowsForms.PlotView> gyroscopePlots = new List<OxyPlot.WindowsForms.PlotView>();
        List<OxyPlot.WindowsForms.PlotView> audioLevelPlots = new List<OxyPlot.WindowsForms.PlotView>();
        List<TrackBar> trackBars = new List<TrackBar>();
        List<Button> buttons = new List<Button>();
        List<TabPage> tabs = new List<TabPage>();
        List<AxWMPLib.AxWindowsMediaPlayer> mediaPlayers = new List<AxWMPLib.AxWindowsMediaPlayer>();
        List<double> elapsedTimes = new List<double>();
        List<Boolean> allPlaying = new List<Boolean>();
        List<System.Timers.Timer> timers = new List<System.Timers.Timer>();
        List<Label> timerLabels = new List<Label>();


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
            
        }

        double max = 0.00;

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
                pv.Location = new Point(460, 50);
                pv.Size = new Size(829, 205);
            }
            else if(graphType.Equals("Gyroscope"))
            {
                pv.Location = new Point(460, 260);
                pv.Size = new Size(829, 205);
            }
            else if(graphType.Equals("AudioLevelData"))
            {
                pv.Location = new Point(460, 475);
                pv.Size = new Size(829, 205);
            }

            pv.Controller = new OxyPlot.PlotController();
            pv.Controller.UnbindKeyDown(OxyKey.Right);
            pv.Controller.UnbindKeyDown(OxyKey.Left);
        
            xAxis = new OxyPlot.Axes.LinearAxis();
            xAxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            xAxis.Maximum = 120;
            xAxis.Minimum = 0;
            xAxis.Unit = " Frames ";
            xAxis.FontSize = 14;
            
            yAxis = new OxyPlot.Axes.LinearAxis();
            yAxis.Position = OxyPlot.Axes.AxisPosition.Left;
            yAxis.Unit = " m/" + "s\u00B2 ";
            yAxis.FontSize = 14;
            
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
            pm.Padding = new OxyThickness(0, 10, 25, 30);

            pv.BackColor = Color.White;

            double maxY = 0;
            double minY = 0;
            double maxValYAxis = 0;

            string line;
            StreamReader file = new StreamReader(filePath);
            line = file.ReadLine();
            string[] parsedLine = line.Split(',');
            double startValNegation = (Convert.ToDouble(parsedLine[3]));
            double lastTime = 0;

            double x;
            double y;
            double z;

            x = Convert.ToDouble(parsedLine[0]);
            y = Convert.ToDouble(parsedLine[1]);
            z = Convert.ToDouble(parsedLine[2]);

            xSeries.Points.Add(new DataPoint(0, z));
            ySeries.Points.Add(new DataPoint(0, y));
            zSeries.Points.Add(new DataPoint(0, z));

            if (x > maxValYAxis)
                maxValYAxis = x;
            if (y > maxValYAxis)
                maxValYAxis = y;
            if (z > maxValYAxis)
                maxValYAxis = z;

            double firstVal = Convert.ToDouble(parsedLine[2])/1000000000.0;
            double lastVal = 0;
            long frames = 1;

            
         
            double timeStamp;
            double seconds;

            while ((line = file.ReadLine()) != null)
            {
                parsedLine = line.Split(',');
                if (parsedLine.Length == 4)
                {
                    if(Convert.ToDouble(parsedLine[3]) > lastTime)
                    {
                        x = Convert.ToDouble(parsedLine[0]);
                        y = Convert.ToDouble(parsedLine[1]);
                        z = Convert.ToDouble(parsedLine[2]);

                        if (x > maxValYAxis)
                            maxValYAxis = x;
                        if (y > maxValYAxis)
                            maxValYAxis = y;
                        if (z > maxValYAxis)
                            maxValYAxis = z;

                        timeStamp = Convert.ToDouble(parsedLine[3]) - startValNegation;
                        seconds = timeStamp / 1000000000.0;
                        lastTime = timeStamp;

                        xSeries.Points.Add(new DataPoint(frames, x));
                        ySeries.Points.Add(new DataPoint(frames, y));
                        zSeries.Points.Add(new DataPoint(frames, z));

                        frames++;
                        lastVal = seconds;

                    }
                }
            }

            MessageBox.Show("Max: " + (int)Math.Ceiling(maxValYAxis));

            yAxis.MajorStep = (int)Math.Ceiling(maxValYAxis);
            yAxis.Maximum = (int)Math.Ceiling(maxValYAxis);
           
           
            //yAxis.
            //yAxis.Label


            if (lastVal > trackBars[trackBars.Count - 1].Maximum)
            trackBars[trackBars.Count - 1].Maximum = (int)Math.Ceiling(lastVal);

            if (graphType.Equals("Accelerometer") || graphType.Equals("Gyroscope"))
            {
                pm.Title = graphType;
                pm.TitleFontSize = 12;
                pm.TitleFontWeight = 0;
                xSeries.Title = graphType + " X (RED)";
                ySeries.Title = graphType + " Y (GREEN)";
                zSeries.Title = graphType + " Z (BLUE)";
                

            }
            else if (graphType.Equals("AudioLevels"))
            {
                
            }

            pm.IsLegendVisible = true;
         
            pm.LegendPlacement = LegendPlacement.Outside;
            pm.LegendPosition = LegendPosition.BottomLeft;
            pm.LegendOrientation = LegendOrientation.Horizontal;
            pm.LegendMaxHeight = 1;
            pm.LegendMargin = 0;
            pm.LegendSymbolLength = 16;
            pm.LegendFontSize = 12;
            pm.LegendPadding = 0;

            pm.Series.Add(xSeries);
            pm.Series.Add(ySeries);
            pm.Series.Add(zSeries);


            pm.Axes.Add(xAxis);
            pm.Axes.Add(yAxis);
            pv.Model = pm;

            if (graphType.Equals("Accelerometer"))
            { 
                accelerometerPlots.Add(pv);
            }
            else if (graphType.Equals("Gyroscope"))
            {
                gyroscopePlots.Add(pv);
            }
            else if(graphType.Equals("AudioLevels"))
            {
                audioLevelPlots.Add(pv);
            }

            file.Close();
            return pv;
        }

        private void loadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        int hours;
        int mins;

        string convertedHours;
        string convertedMins;
        string convertedSecs;

        double currentVal;
        int currentPosition;

        private void playAllData(object sender, System.EventArgs e)
        {
            if (allPlaying.ElementAt(tabControl1.SelectedIndex))
            {
                buttons[tabControl1.SelectedIndex].Text = "Start";
                allPlaying[tabControl1.SelectedIndex] = false;
                timers[tabControl1.SelectedIndex].Enabled = false;
               
                mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.stop();
            }
            else
            {
                mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.play();
                buttons[tabControl1.SelectedIndex].Text = "Stop";
                mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.currentPosition = currentPosition;
                allPlaying[tabControl1.SelectedIndex] = true;
                //MessageBox.Show("Duration: " + mediaPlayers[tabControl1.SelectedIndex].currentMedia.durationString);

                Thread threadTest = new Thread(threadWait);
                threadTest.Start();
               
            }
        }

        private void threadWait()
        {
            Boolean test = true;
            while (test)
            {
                MethodInvoker methodInvoker = delegate
                {
                    if (mediaPlayers[tabControl1.SelectedIndex].playState == WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        timers[tabControl1.SelectedIndex].Enabled = true;
                        mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.currentPosition = elapsedTimes[tabControl1.SelectedIndex];
                        timers[tabControl1.SelectedIndex].AutoReset = true;
                        test = false;
                    }
                };

                this.Invoke(methodInvoker);
            }
        }

        private void tabControl1_SelectingTab(Object sender, TabControlCancelEventArgs e)
        {
                for (int i = 0; i < timers.Count; i++)
                {
                    if (i != tabControl1.SelectedIndex)
                    {
                        timers[i].Enabled = false;
                    }
                    else
                    {
                        if(!timers[i].Enabled)
                        {
                        timers[i].Enabled = true;
                        }
                    }
                }
        }

        private void periodicGraphUpdate(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine(source.ToString());
            
            //http://stackoverflow.com/questions/14890295/update-label-from-another-thread
            //^ Accessed: 27/03/2017 @ 03:30
            MethodInvoker methodInvoker = delegate
            {
                Console.WriteLine("... " + tabControl1.SelectedIndex);
                if (allPlaying[tabControl1.SelectedIndex])
                {
                    elapsedTimes[tabControl1.SelectedIndex] += 1;
                    Console.WriteLine("... " + elapsedTimes[tabControl1.SelectedIndex]);
                    if (elapsedTimes[tabControl1.SelectedIndex] > trackBars[tabControl1.SelectedIndex].Maximum)
                    {
                        elapsedTimes[tabControl1.SelectedIndex] = trackBars[tabControl1.SelectedIndex].Maximum;
                    }
                    else
                    {
                        trackBars[tabControl1.SelectedIndex].Value = Convert.ToInt16(elapsedTimes[tabControl1.SelectedIndex]);
                    }
                }
            };

            this.Invoke(methodInvoker);
        }

        int currentTimerVal;
        private void trackBar_ValueChanged(object sender, System.EventArgs e)
        {
            currentPosition = trackBars[tabControl1.SelectedIndex].Value;
            currentVal = trackBars[tabControl1.SelectedIndex].Value;

            if (allPlaying[tabControl1.SelectedIndex])
            {

                if (currentPosition <= mediaPlayers[tabControl1.SelectedIndex].currentMedia.duration)
                {
                    if (mediaPlayers[tabControl1.SelectedIndex].playState != WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.play();
                    }
                }

                if (mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.currentPosition > currentPosition + 0.5)
                {
                    //mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.currentPosition = currentPosition;

                    //Console.WriteLine("Media player ahead by at least 0.01");
                    //Console.WriteLine("Current position:" + currentPosition);
                }
                else if (mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.currentPosition < currentPosition - 0.5)
                {
                    if (mediaPlayers[tabControl1.SelectedIndex].playState == WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        //Console.WriteLine("Current Media position:" + mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.currentPosition);
                        //currentVal = mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.currentPosition;
                        //mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.currentPosition = currentPosition;
                    }
                }
            }

            gyroscopePlots[tabControl1.SelectedIndex].Model.Axes.ElementAt(0).Maximum = (currentVal * 24);
            gyroscopePlots[tabControl1.SelectedIndex].Model.Axes.ElementAt(0).Minimum = (currentVal * 24) - 120;

            accelerometerPlots[tabControl1.SelectedIndex].Model.Axes.ElementAt(0).Maximum = currentVal * 24;
            accelerometerPlots[tabControl1.SelectedIndex].Model.Axes.ElementAt(0).Minimum = (currentVal * 24) - 120;

            Console.WriteLine(accelerometerPlots[tabControl1.SelectedIndex].Model.Axes.ElementAt(0).Maximum);

            gyroscopePlots[tabControl1.SelectedIndex].InvalidatePlot(true);
            accelerometerPlots[tabControl1.SelectedIndex].InvalidatePlot(true);

            if(!allPlaying[tabControl1.SelectedIndex])
                mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.currentPosition = currentVal;

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

            timerLabels[tabControl1.SelectedIndex].Text = convertedHours + ":" + convertedMins + ":" + convertedSecs;
            hours = 0;
            mins = 0;

            elapsedTimes[tabControl1.SelectedIndex] = currentPosition;
        }

        private void syncSleeper()
        {
            Thread.Sleep(15);
                MethodInvoker methodInvoker = delegate
                {
                    mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.play();
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

        private void menuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage tab = new TabPage();
            AxWMPLib.AxWindowsMediaPlayer mediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            Button startAllButton = new Button();
            TrackBar trackBar = new TrackBar();
            Label timerLabel = new Label();

            trackBar.Location = new Point(68, 5);
            trackBar.Size = new Size(1162, 10);
            trackBar.TickStyle = 0;
            trackBar.TickFrequency = 0;
            trackBar.SmallChange = 0;
            trackBar.LargeChange = 0;
            trackBar.ValueChanged += new System.EventHandler(trackBar_ValueChanged);
            tab.Controls.Add(trackBar);
            trackBars.Add(trackBar);

            mediaPlayers.Add(mediaPlayer);
            allPlaying.Add(false);
            elapsedTimes.Add(0.00);
            timerLabels.Add(timerLabel);

            timerLabel.Location = new Point(1228, 8);
            timerLabel.Text = "00:00:00";
            tab.Controls.Add(timerLabel);


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
                        case "AudioLevelData.txt":
                            tab.Controls.Add(parseAndCreateGraph(sensorFiles[i], "AudioLevelData"));
                            break;
                        case "Video.mp4":
                            tab.Controls.Add(mediaPlayer);
                            videoURL = sensorFiles[i];
                            break;
                        case "Audio.wav":
                            //tab.Controls.Add(parseAndCreateGraph(sensorFiles[i], "Audio"));
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
            buttons.Add(startAllButton);

            tabControl1.Selecting += new TabControlCancelEventHandler(tabControl1_SelectingTab);

            PictureBox pictureBox = new PictureBox();
            pictureBox.Location = new Point(5, 385);
            pictureBox.Size = new Size(450, 295);
            pictureBox.BackColor = Color.Navy;
            pictureBox.BorderStyle = BorderStyle.None;
            tab.Controls.Add(pictureBox);

            tabControl1.Controls.Add(tab);

            mediaPlayer.settings.autoStart = true;
            
            //mediaPlayer.settigs...
            
            mediaPlayer.URL = videoURL;
            mediaPlayer.Ctlcontrols.stop();

            mediaPlayer.Location = new Point(5, 50);
            mediaPlayer.Size = new Size(450, 330);

            timers.Add(new System.Timers.Timer());

            timers[timers.Count - 1].Interval = 1000;
            timers[timers.Count - 1].Elapsed += periodicGraphUpdate;

        }
    }  
}


           

