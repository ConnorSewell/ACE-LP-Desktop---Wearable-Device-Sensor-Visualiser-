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

        List<TabValueStore> tabValStore = new List<TabValueStore>();
        GUISetUpHelper guiSH = new GUISetUpHelper();
        int currTab;

        public Form1()
        {

            InitializeComponent();
            tabControl1.MouseClick += tabControl1_MouseClick;

        }


        private void Form1_Load(object sender, EventArgs e)
        {


        }

        //http://stackoverflow.com/questions/15452070/create-a-new-rectangle-next-to-other-object-location
        //^ Accessed: 06/04/2017 @ 13:04 for rectangle creation/determining if location clicked was on rectangle (close button)
        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            int range = 1;
            if(currTab > 0)
            {
                range = currTab;
            }
            for (int i = 0; i < range; i++)
            {
                MessageBox.Show("Tab: " + i);
                Rectangle r = tabControl1.GetTabRect(i);
                Rectangle closeButton = new Rectangle(r.Right - 11, r.Top + 5, 9, 8);
                if (closeButton.Contains(e.Location))
                {
                    tabValStore.ElementAt(i).getTimer().Enabled = false;
                    tabValStore.ElementAt(i).getTimer().Dispose();
                    tabControl1.TabPages.RemoveAt(i);
                    tabValStore.ElementAt(i).getMediaPlayer().Ctlcontrols.stop();
                    tabValStore.RemoveAt(i);
                }

            }

        }

        private void tabControl1_DrawCloseButton(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {


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
            tabValStore[currTab].getTimer().Enabled = false;
            if (tabValStore[currTab].getAllPlaying())
            {
                tabValStore[currTab].setStartStopButtonText("Start");
                tabValStore[currTab].setAllPlaying();
                tabValStore[currTab].getTimer().Enabled = false;
                tabValStore[currTab].getMediaPlayer().Ctlcontrols.pause();
            }
            else
            {
                tabValStore[currTab].getMediaPlayer().Ctlcontrols.play();
                tabValStore[currTab].setStartStopButtonText("Stop");
                tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition = currentPosition;

                Thread waitMedia = new Thread(threadWait);
                waitMedia.Start();

            }
        }

        private void threadWait()
        {
            Boolean test = true;
            while (test)
            {
                MethodInvoker methodInvoker = delegate
                {
                    if (tabValStore[currTab].getMediaPlayer().playState == WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        tabValStore[currTab].getTimer().Enabled = true;
                        tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition = tabValStore[currTab].getElapsedTime();
                        tabValStore[currTab].getTimer().AutoReset = true;
                        test = false;
                        Thread.Sleep(300);
                        tabValStore[currTab].setAllPlaying();
                    }
                };

                this.Invoke(methodInvoker);
            }
        }

        private void tabControl1_SelectingTab(Object sender, TabControlCancelEventArgs e)
        {
            currTab = tabControl1.SelectedIndex;
            if (currTab < 0 && tabValStore.Count >= 1)
                currTab = 0;
            try
            {
                for (int i = 0; i < tabValStore.Count; i++)
                {
                    if (i != currTab)
                    {
                        tabValStore[i].getTimer().Enabled = false;
                    }
                    else
                    {
                        if (!tabValStore[i].getTimer().Enabled)
                        {
                            tabValStore[i].getTimer().Enabled = true;
                        }
                    }
                }
            }
            catch(Exception)
            {
                Console.WriteLine("Fault Safety Measure - 0 tabs results in a tab index of -1");
            }
        }

        private void periodicGraphUpdate(Object source, ElapsedEventArgs e)
        {

            //MessageBox.Show("Current Tab: " + currTab);

            //http://stackoverflow.com/questions/14890295/update-label-from-another-thread
            //^ Accessed: 27/03/2017 @ 03:30
            MethodInvoker methodInvoker = delegate
            {
                if (tabValStore[currTab].getAllPlaying())
                {
                    tabValStore[currTab].setElapsedTime(tabValStore[currTab].getElapsedTime() + 1);
                    if (tabValStore[currTab].getElapsedTime() > tabValStore[currTab].getTrackBar().Maximum)
                    {
                        tabValStore[currTab].setElapsedTime(tabValStore[currTab].getTrackBar().Maximum);
                    }
                    else
                    {
                        tabValStore[currTab].getTrackBar().Value = Convert.ToInt16(tabValStore[currTab].getElapsedTime());
                    }
                }
            };

            this.Invoke(methodInvoker);
        }

        int currentTimerVal;
        int panCount = 0;
        int lastPosition = 0;
        int offSet = 0;
        Boolean back = false;

        private void trackBar_ValueChanged(object sender, System.EventArgs e)
        {

            currentPosition = tabValStore[currTab].getTrackBar().Value;

            if (currentPosition >= 2)
            {
                if (currentPosition > lastPosition + 1)
                {
                    back = false;
                    offSet = 16000 * (currentPosition - (lastPosition + 1));
                }
                else if (currentPosition < lastPosition)
                {
                    back = true;
                    offSet = 16000 * (lastPosition - currentPosition) + 32000;
                }
                else
                {
                    offSet = 0;
                }


                if (tabValStore[currTab].getAllPlaying())
                {

                    if (currentPosition <= tabValStore[currTab].getMediaPlayer().currentMedia.duration)
                    {
                        if (tabValStore[currTab].getMediaPlayer().playState != WMPLib.WMPPlayState.wmppsPlaying)
                        {
                            tabValStore[currTab].getMediaPlayer().Ctlcontrols.play();
                        }
                    }

                    if (tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition > currentPosition + 0.3)
                    {
                        //Thread pauseMediaSyncThread = new Thread(pauseMedia);
                        //pauseMediaSyncThread.Start();
                        Console.WriteLine("Current Media position:" + tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition);
                    }
                    else if (tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition < currentPosition - 0.15)
                    {
                        if (tabValStore[currTab].getMediaPlayer().playState == WMPLib.WMPPlayState.wmppsPlaying)
                        {
                            Thread fastForwardSyncThread = new Thread(syncForwards);
                            fastForwardSyncThread.Start();
                            Console.WriteLine("Media player ahead by at least 0.01");
                        }
                    }
                }

                panCount++;
                if (panCount >= 2)
                {
                    updateAccelerometerGraph();
                    updateGyroscopeGraph();
                    updateAudioGraph();
                }

            }

            shiftGraphs();
        

            FunctionSeries ySeries = (FunctionSeries)tabValStore[currTab].getAudioLevelPlot().Model.Series[0];

            if (!tabValStore[currTab].getAllPlaying())
                tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition = currentVal;

            currentTimerVal = Convert.ToInt32(currentPosition);

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
            else
            {
                convertedHours = hours.ToString();
            }

            if (mins < 10)
            {
                convertedMins = "0" + mins.ToString();
            }
            else
            {
                convertedMins = mins.ToString();
            }

            convertedSecs = currentTimerVal.ToString();

            if (currentTimerVal < 10)
            {
                convertedSecs = "0" + convertedSecs;
            }

            tabValStore[currTab].getTimerLabel().Text = convertedHours + ":" + convertedMins + ":" + convertedSecs;
            hours = 0;
            mins = 0;

            tabValStore[currTab].setElapsedTime(currentPosition);

            lastPosition = currentPosition;

        }

        private void updateAccelerometerGraph()
        {
            FunctionSeries accelerometerXSeries = (FunctionSeries)tabValStore[currTab].getAccelerometerPlot().Model.Series[0];
            FunctionSeries accelerometerYSeries = (FunctionSeries)tabValStore[currTab].getAccelerometerPlot().Model.Series[1];
            FunctionSeries accelerometerZSeries = (FunctionSeries)tabValStore[currTab].getAccelerometerPlot().Model.Series[2];

            accelerometerXSeries.Points.RemoveRange(0, accelerometerXSeries.Points.Count);
            accelerometerYSeries.Points.RemoveRange(0, accelerometerYSeries.Points.Count);
            accelerometerZSeries.Points.RemoveRange(0, accelerometerZSeries.Points.Count);

            List<DataPoint>[] accelerometerVals = guiSH.getAccelerometerData(currentPosition - 2, 2, 0);
            accelerometerXSeries.Points.AddRange(accelerometerVals[0]);
            accelerometerYSeries.Points.AddRange(accelerometerVals[1]);
            accelerometerZSeries.Points.AddRange(accelerometerVals[2]);

            tabValStore[currTab].getAccelerometerPlot().Model.Series[0] = accelerometerXSeries;
            tabValStore[currTab].getAccelerometerPlot().Model.Series[1] = accelerometerYSeries;
            tabValStore[currTab].getAccelerometerPlot().Model.Series[2] = accelerometerZSeries;
        }

        private void updateGyroscopeGraph()
        {
            FunctionSeries gyroscopeXSeries = (FunctionSeries)tabValStore[currTab].getGyroscopePlot().Model.Series[0];
            FunctionSeries gyroscopeYSeries = (FunctionSeries)tabValStore[currTab].getGyroscopePlot().Model.Series[1];
            FunctionSeries gyroscopeZSeries = (FunctionSeries)tabValStore[currTab].getGyroscopePlot().Model.Series[2];

            gyroscopeXSeries.Points.RemoveRange(0, gyroscopeXSeries.Points.Count);
            gyroscopeYSeries.Points.RemoveRange(0, gyroscopeYSeries.Points.Count);
            gyroscopeZSeries.Points.RemoveRange(0, gyroscopeZSeries.Points.Count);

            List<DataPoint>[] gyroscopeVals = guiSH.getGyroscopeData(currentPosition - 2, 2, 0);
            gyroscopeXSeries.Points.AddRange(gyroscopeVals[0]);
            gyroscopeYSeries.Points.AddRange(gyroscopeVals[1]);
            gyroscopeZSeries.Points.AddRange(gyroscopeVals[2]);

            tabValStore[currTab].getGyroscopePlot().Model.Series[0] = gyroscopeXSeries;
            tabValStore[currTab].getGyroscopePlot().Model.Series[1] = gyroscopeYSeries;
            tabValStore[currTab].getGyroscopePlot().Model.Series[2] = gyroscopeZSeries;
        }

        private void updateAudioGraph()
        {
            FunctionSeries xSeries = (FunctionSeries)tabValStore[currTab].getAudioLevelPlot().Model.Series[0];
            if (currentPosition > lastPosition)
            {
                if (xSeries.Points.Count == 16000)
                {
                    xSeries.Points.RemoveRange(0, 8000);
                    xSeries.Points.AddRange(guiSH.getNewAudioData(offSet, (currentPosition * 24) - 23, true));
                    tabValStore[currTab].getAudioLevelPlot().Model.Series[0] = xSeries;
                }
            }
            else if (currentPosition < lastPosition && xSeries.Points.Count >= 16000 && currentPosition >= 2)
            {
                if (xSeries.Points.Count >= 16000)
                {
                    xSeries.Points.RemoveRange(0, 16000);
                    xSeries.Points.InsertRange(0, guiSH.getNewAudioData(offSet, (currentPosition * 24) - 47, false));
                    tabValStore[currTab].getAudioLevelPlot().Model.Series[0] = xSeries;
                }
            }

        }

        private void shiftGraphs()
        {
            tabValStore[currTab].getGyroscopePlot().Model.Axes.ElementAt(0).Maximum = ((currentPosition * 24) + (1 - 0.003));
            tabValStore[currTab].getGyroscopePlot().Model.Axes.ElementAt(0).Minimum = (currentPosition * 24) - 47;

            tabValStore[currTab].getAccelerometerPlot().Model.Axes.ElementAt(0).Maximum = ((currentPosition * 24) + (1 - 0.003));
            tabValStore[currTab].getAccelerometerPlot().Model.Axes.ElementAt(0).Minimum = (currentPosition * 24) - 47;

            tabValStore[currTab].getAudioLevelPlot().Model.Axes.ElementAt(0).Maximum = ((currentPosition * 24) + (1 - 0.003));
            tabValStore[currTab].getAudioLevelPlot().Model.Axes.ElementAt(0).Minimum = (currentPosition * 24) - 47;

            tabValStore[currTab].getGyroscopePlot().InvalidatePlot(true);
            tabValStore[currTab].getAccelerometerPlot().InvalidatePlot(true);
            tabValStore[currTab].getAudioLevelPlot().InvalidatePlot(true);
        }

        private void syncForwards()
        {
            tabValStore[currTab].getMediaPlayer().settings.rate = 2;
            Thread.Sleep(500);
            tabValStore[currTab].getMediaPlayer().settings.rate = 1;
        }

        private void pauseMedia()
        {
            tabValStore[currTab].getMediaPlayer().Ctlcontrols.pause();
            Thread.Sleep(300);
            tabValStore[currTab].getMediaPlayer().Ctlcontrols.play();
        }

        private void syncSleeper()
        {
            Thread.Sleep(15);
            MethodInvoker methodInvoker = delegate
            {
                tabValStore[currTab].getMediaPlayer().Ctlcontrols.play();
            };

            this.Invoke(methodInvoker);
        }

        TabPage tab;
        AxWMPLib.AxWindowsMediaPlayer mediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
        private void menuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tab = new TabPage();
            mediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();

            TabValueStore newTabValStore = new TabValueStore();
            tabValStore.Add(newTabValStore);

            setTrackBar();
            setTimerLabel();
            setStartButton();
            setTimer();

            //Using http://stackoverflow.com/questions/11624298/how-to-use-openfiledialog-to-select-a-folder
            //^ For opening folder and parsing file names. Accessed: 16/03/2017 @ 20:10
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            DialogResult result = openFolderDialog.ShowDialog();

            string videoPath = "";
            Boolean audioFound = false;
            string folderName = null;
            string path = null;

            PlotView pv;

            String accelerometerPath = null;
            String gyroscopePath = null;
            String audioPath = null;

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string[] sensorFiles = Directory.GetFiles(openFolderDialog.SelectedPath);
                folderName = openFolderDialog.SelectedPath.Substring(openFolderDialog.SelectedPath.LastIndexOf("\\") + 1);
                tab.Text = folderName + "   x";
                path = openFolderDialog.SelectedPath;

                for (int i = 0; i < sensorFiles.Length; i++)
                {
                    string sensorFile = sensorFiles[i].Substring(sensorFiles[i].LastIndexOf("\\") + 1);

                    switch (sensorFile)
                    {
                        case "AccelerometerData.txt":
                            accelerometerPath = sensorFiles[i];
                            tabValStore.ElementAt(tabValStore.Count - 1).setAccelerometerDataPath(accelerometerPath);
                            break;
                        case "GyroscopeData.txt":
                            gyroscopePath = sensorFiles[i];
                            tabValStore.ElementAt(tabValStore.Count - 1).setGyroscopeDataPath(gyroscopePath);
                            break;
                        case "Video.mp4":
                            videoPath = sensorFiles[i];
                            break;
                        case "Audio.raw":
                            audioPath = sensorFiles[i];
                            tabValStore.ElementAt(tabValStore.Count - 1).setAudioDataPath(audioPath);
                            audioFound = true;
                            break;
                        default:
                            MessageBox.Show("File not matching any sensor found... ");
                            break;

                    }
                }
            }

            if (audioFound)
            {
                pv = guiSH.createGraph(audioPath, "AudioLevel", tabValStore[tabValStore.Count - 1].getTrackBar());
                tabValStore[tabValStore.Count - 1].setAudioLevelPlot(pv);
                tab.Controls.Add(pv);
            }

            if (!audioFound)
            {
                extractAudioFromVideo(videoPath, path);
                pv = guiSH.createGraph(path + "//Audio.raw", "AudioLevel", tabValStore[tabValStore.Count - 1].getTrackBar());
                tabValStore[tabValStore.Count - 1].setAudioLevelPlot(pv);
                tab.Controls.Add(pv);
            }

            pv = guiSH.createGraph(accelerometerPath, "Accelerometer", tabValStore[tabValStore.Count - 1].getTrackBar());
            tabValStore[tabValStore.Count - 1].setAccelerometerPlot(pv);
            tab.Controls.Add(pv);

            pv = guiSH.createGraph(gyroscopePath, "Gyroscope", tabValStore[tabValStore.Count - 1].getTrackBar());
            tabValStore[tabValStore.Count - 1].setGyroscopePlot(pv);
            tab.Controls.Add(pv);

            tab.Controls.Add(mediaPlayer);

            tabControl1.Selecting += new TabControlCancelEventHandler(tabControl1_SelectingTab);
            tabControl1.Controls.Add(tab);

            guiSH.setMediaPlayerProperties(videoPath, mediaPlayer);
            mediaPlayer.Ctlcontrols.stop();
            tabValStore[tabValStore.Count - 1].setMediaPlayer(mediaPlayer);

        }

        //http://stackoverflow.com/questions/1707516/c-sharp-and-ffmpeg-preferably-without-shell-commands
        //^ Used for below method (using ffmpeg). Accessed: 02/04/2017 @ 01:31
        private void extractAudioFromVideo(string videoPath, string path)
        {
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
            tabValStore[tabValStore.Count - 1].setTrackBar(trackBar);
        }

        private void setTimerLabel()
        {
            Label label = new Label();
            guiSH.setTimeLabelProperties(label);
            tab.Controls.Add(label);
            tabValStore[tabValStore.Count - 1].setTimerLabel(label);
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
            tabValStore[tabValStore.Count - 1].setStartStopButton(startAllButton);
            //buttons.Add(startAllButton);
        }

        private void setTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            tabValStore[tabValStore.Count - 1].setTimer(timer);
            tabValStore[tabValStore.Count - 1].getTimer().Interval = 1000;
            tabValStore[tabValStore.Count - 1].getTimer().Elapsed += periodicGraphUpdate;
            timer = null;

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void inalidatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabValStore[currTab].getAudioLevelPlot().InvalidatePlot(true);
        }

        private void invaliToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabValStore[currTab].getAudioLevelPlot().InvalidatePlot(true);
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void userGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(Directory.GetCurrentDirectory());
            System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "\\User Manual.pdf");
        }
    }
}




