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
    using System;
    using System.Diagnostics;
    using OxyPlot.WindowsForms;

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

        GUISetUpHelper guiSH = new GUISetUpHelper();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        double max = 0.00;

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
                    if (!timers[i].Enabled)
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

        int count = 0;
        int testCounter = 0;

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
            gyroscopePlots[tabControl1.SelectedIndex].Model.Axes.ElementAt(0).Minimum = (currentVal * 24) - 48;

            accelerometerPlots[tabControl1.SelectedIndex].Model.Axes.ElementAt(0).Maximum = currentVal * 24;
            accelerometerPlots[tabControl1.SelectedIndex].Model.Axes.ElementAt(0).Minimum = (currentVal * 24) - 48;

            audioLevelPlots[tabControl1.SelectedIndex].Model.Axes.ElementAt(0).Maximum = currentVal * 24;
            audioLevelPlots[tabControl1.SelectedIndex].Model.Axes.ElementAt(0).Minimum = (currentVal * 24) - 48;

            gyroscopePlots[tabControl1.SelectedIndex].InvalidatePlot(true);
            accelerometerPlots[tabControl1.SelectedIndex].InvalidatePlot(true);
            audioLevelPlots[tabControl1.SelectedIndex].InvalidatePlot(true);

           double val = 0.00;
           if (testCounter >= 2)
           {
                FunctionSeries xSeries = (FunctionSeries)audioLevelPlots[tabControl1.SelectedIndex].Model.Series[0];
                if(xSeries.Points.Count >= 8000)
                {
                    xSeries.Points.RemoveRange(0, 7999);
                    xSeries.Points.AddRange(guiSH.getNewAudioData(16000, ((int)currentVal * 24) - 23));
                    audioLevelPlots[tabControl1.SelectedIndex].Model.Series[0] = xSeries;
                }
           
            }
           
            count++;
            testCounter++;

            if (!allPlaying[tabControl1.SelectedIndex])
                mediaPlayers[tabControl1.SelectedIndex].Ctlcontrols.currentPosition = currentVal;

            currentTimerVal = Convert.ToInt32(currentVal);

            while (currentTimerVal >= 3600)
            {
                hours++;
                currentTimerVal -= 3600;
            }

            while (currentTimerVal >= 60)
            {
                mins++;
                currentTimerVal -= 60;
            }

            if (hours < 10)
            {
                convertedHours = "0" + hours.ToString();
            }

            if (mins < 10)
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

        TabPage tab;

        private void menuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tab = new TabPage();
            AxWMPLib.AxWindowsMediaPlayer mediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();

            setTrackBar();
            setTimerLabel();
            setGPSBox();
            setStartButton();
            setTimer();

            allPlaying.Add(false);
            elapsedTimes.Add(0.00);

            //Using http://stackoverflow.com/questions/11624298/how-to-use-openfiledialog-to-select-a-folder
            //^ For opening folder and parsing file names. Accessed: 16/03/2017 @ 20:10
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            DialogResult result = openFolderDialog.ShowDialog();

            string videoPath = "";
            Boolean audioFound = false;
            string folderName = null;
            string path = null;

            PlotView pv;

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string[] sensorFiles = Directory.GetFiles(openFolderDialog.SelectedPath);
                folderName = openFolderDialog.SelectedPath.Substring(openFolderDialog.SelectedPath.LastIndexOf("\\") + 1);
                tab.Text = folderName;
                path = openFolderDialog.SelectedPath;

                for (int i = 0; i < sensorFiles.Length; i++)
                {
                    string sensorFile = sensorFiles[i].Substring(sensorFiles[i].LastIndexOf("\\") + 1);

                    switch (sensorFile)
                    {
                        case "AccelerometerData.txt":
                            pv = guiSH.createGraph(sensorFiles[i], "Accelerometer", trackBars[trackBars.Count - 1]);
                            accelerometerPlots.Add(pv);
                            tab.Controls.Add(pv);
                            break;
                        case "GyroscopeData.txt":
                            MessageBox.Show("Trackbar size: " + trackBars.Count);
                            pv = guiSH.createGraph(sensorFiles[i], "Gyroscope", trackBars[trackBars.Count - 1]);
                            gyroscopePlots.Add(pv);
                            tab.Controls.Add(pv);
                            break;
                        case "Video.mp4":
                            videoPath = sensorFiles[i];
                            break;
                        case "Audio.raw":
                            audioFound = true;
                            pv = guiSH.createGraph(sensorFiles[i], "AudioLevel", trackBars[trackBars.Count - 1]);
                            audioLevelPlots.Add(pv);
                            tab.Controls.Add(pv);
                            break;
                        default:
                            MessageBox.Show("File not matching any sensor found... ");
                            break;

                    }
                }
            }

            if (!audioFound)
            {
                extractAudioFromVideo(videoPath, path);
                pv = guiSH.createGraph(path + "//Audio.raw", "AudioLevel", trackBars[trackBars.Count - 1]);
                audioLevelPlots.Add(pv);
                tab.Controls.Add(pv);
            }

            tabControl1.Selecting += new TabControlCancelEventHandler(tabControl1_SelectingTab);
            tabControl1.Controls.Add(tab);

            tab.Controls.Add(mediaPlayer);
            guiSH.setMediaPlayerProperties(videoPath, mediaPlayer);
            mediaPlayers.Add(mediaPlayer);
        }

   

        //http://stackoverflow.com/questions/1707516/c-sharp-and-ffmpeg-preferably-without-shell-commands
        //^ Used for below method (using ffmpeg). Accessed: 02/04/2017 @ 01:31
        private void extractAudioFromVideo(string videoPath, string path)
        {
            MessageBox.Show(path + "\\Audio.raw");
            Process proc = new Process();
            proc.StartInfo.FileName = "ffmpeg";
            proc.StartInfo.Arguments = "-i " + videoPath + " -f s16le -acodec pcm_s16le " + (path + "\\Audio.raw");
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            if (!proc.Start())
            {
                Console.WriteLine("Error Starting Process");
                return;
            }
            StreamReader reader = proc.StandardError;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
            proc.Close();

        }

        private void setTrackBar()
        {
            TrackBar trackBar = new TrackBar();
            guiSH.setTrackBarProperties(trackBar);
            trackBar.ValueChanged += new System.EventHandler(trackBar_ValueChanged);
            tab.Controls.Add(trackBar);
            trackBars.Add(trackBar);
        }

        private void setTimerLabel()
        {
            Label label = new Label();
            guiSH.setTimeLabelProperties(label);
            timerLabels.Add(label);
            tab.Controls.Add(label);
        }

        private void setGPSBox()
        {
            PictureBox pictureBox = new PictureBox();
            guiSH.setGPSPointProperties(pictureBox);
            tab.Controls.Add(pictureBox);
        }

        private void setStartButton()
        {
            Button startAllButton = new Button();
            guiSH.setStartButton(startAllButton);
            startAllButton.Click += new System.EventHandler(playAllData);
            tab.Controls.Add(startAllButton);
            buttons.Add(startAllButton);
        }

        private void setTimer()
        {
            timers.Add(new System.Timers.Timer());
            timers[timers.Count - 1].Interval = 1000;
            timers[timers.Count - 1].Elapsed += periodicGraphUpdate;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}




