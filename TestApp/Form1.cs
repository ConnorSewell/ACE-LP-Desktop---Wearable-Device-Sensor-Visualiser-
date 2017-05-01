using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopAppHons
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

    //This is the main form, essentially acts as a controller. Code is not optimal. I.
    public partial class Form1 : Form
    {

        List<TabValueStore> tabValStore = new List<TabValueStore>();
        List<GUISetUpHelper> guiSH = new List<GUISetUpHelper>();
        List<String> openFiles = new List<String>();
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

            for (int i = 0; i < tabControl1.TabCount; i++)
            {
                Rectangle r = tabControl1.GetTabRect(i);
                Rectangle closeButton = new Rectangle(r.Right - 11, r.Top + 5, 9, 8);
                if (closeButton.Contains(e.Location))
                {
                    tabValStore.ElementAt(i).getTimer().Enabled = false;
                    tabValStore.ElementAt(i).getTimer().Dispose();
                    tabControl1.TabPages.RemoveAt(i);
                    tabValStore.ElementAt(i).getMediaPlayer().Ctlcontrols.stop();
                    tabValStore.RemoveAt(i);
                    guiSH.ElementAt(i).closeFile();
                    guiSH.RemoveAt(i);
                    openFiles.RemoveAt(i);
                }

            }

        }

        int hours;
        int mins;
        string convertedHours;
        string convertedMins;
        string convertedSecs;

        private void playAllData(object sender, System.EventArgs e)
        {
            tabValStore[currTab].getTimer().Enabled = false;
            if (tabValStore[currTab].getAllPlaying())
            {
                tabValStore[currTab].setStartStopButtonText("Start");
                tabValStore[currTab].setAllPlaying(false);
                tabValStore[currTab].getTimer().Enabled = false;
                tabValStore[currTab].getMediaPlayer().Ctlcontrols.pause();
            }
            else
            {
                tabValStore[currTab].getMediaPlayer().Ctlcontrols.play();
                tabValStore[currTab].setStartStopButtonText("Stop");
                tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition = tabValStore[currTab].getCurrentPosition();
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
                        tabValStore[currTab].setAllPlaying(true);
                    }
                };

                this.Invoke(methodInvoker);
            }
        }

        int lastVal = -10;
        private void tabControl1_SelectingTab(Object sender, TabControlCancelEventArgs e)
        {
            currTab = tabControl1.SelectedIndex;

            if (currTab != lastVal)
            {
                lastVal = currTab;
                if (currTab < 0 && tabValStore.Count >= 1)
                    currTab = 0;
                try
                {
                    for (int i = 0; i < tabValStore.Count; i++)
                    {
                        if (i != currTab)
                        {
                            tabValStore[i].getStartStopButton().Text = "Start";
                            tabValStore[i].getTimer().Enabled = false;
                            tabValStore[i].setAllPlaying(false);
                            tabValStore[i].getMediaPlayer().Ctlcontrols.pause();
                        }
                        else
                        {
                            if (!tabValStore[i].getTimer().Enabled)
                            {
                                tabValStore[i].getTimer().Enabled = true;

                                if (tabValStore[i].getAllPlaying())
                                    tabValStore[i].getMediaPlayer().Ctlcontrols.play();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Fault Safety Measure - 0 tabs results in a tab index of -1");
                }
            }
        }

        private void periodicGraphUpdate(Object source, ElapsedEventArgs e)
        {



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
        int lastPosition = 0;
        int offSet = 0;
        Boolean back = false;
        int currentVal = 0;

        private void trackBar_ValueChanged(object sender, System.EventArgs e)
        {

            tabValStore[currTab].setCurrentPosition(tabValStore[currTab].getTrackBar().Value);

            if (tabValStore[currTab].getCurrentPosition() >= 2)
            {
                if (tabValStore[currTab].getCurrentPosition() > tabValStore[currTab].getLastPosition() + 1)
                {
                    back = false;
                    offSet = 16000 * (tabValStore[currTab].getCurrentPosition() - (tabValStore[currTab].getLastPosition() + 1));
                }
                else if (tabValStore[currTab].getCurrentPosition() < tabValStore[currTab].getLastPosition())
                {
                    back = true;
                    offSet = 16000 * (tabValStore[currTab].getLastPosition() - tabValStore[currTab].getCurrentPosition()) + 32000;
                }
                else
                {
                    offSet = 0;
                }


                if (tabValStore[currTab].getAllPlaying())
                {

                    if (tabValStore[currTab].getCurrentPosition() <= tabValStore[currTab].getMediaPlayer().currentMedia.duration)
                    {
                        if (tabValStore[currTab].getMediaPlayer().playState != WMPLib.WMPPlayState.wmppsPlaying)
                        {
                            tabValStore[currTab].getMediaPlayer().Ctlcontrols.play();
                        }
                    }

                    if (tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition > tabValStore[currTab].getCurrentPosition() + 0.15)
                    {
                        tabValStore[currTab].getMediaPlayer().Ctlcontrols.pause();
                        tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition = tabValStore[currTab].getCurrentPosition() - 0.1;
                        tabValStore[currTab].getMediaPlayer().Ctlcontrols.play();
                    }
                    else if (tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition < tabValStore[currTab].getCurrentPosition() - 0.15)
                    {
                        if (tabValStore[currTab].getMediaPlayer().playState == WMPLib.WMPPlayState.wmppsPlaying)
                        {
                            tabValStore[currTab].getMediaPlayer().Ctlcontrols.pause();
                            tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition = tabValStore[currTab].getCurrentPosition() + 0.1;
                            tabValStore[currTab].getMediaPlayer().Ctlcontrols.play();
                        }
                    }
                }

                tabValStore[currTab].incrementPanCount();
                if (tabValStore[currTab].getPanCount() >= 2)
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

            currentTimerVal = Convert.ToInt32(tabValStore[currTab].getCurrentPosition());

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

            tabValStore[currTab].setElapsedTime(tabValStore[currTab].getCurrentPosition());

            tabValStore[currTab].setLastPosition(tabValStore[currTab].getCurrentPosition());
        }

        private void updateAccelerometerGraph()
        {
            FunctionSeries accelerometerXSeries = (FunctionSeries)tabValStore[currTab].getAccelerometerPlot().Model.Series[0];
            FunctionSeries accelerometerYSeries = (FunctionSeries)tabValStore[currTab].getAccelerometerPlot().Model.Series[1];
            FunctionSeries accelerometerZSeries = (FunctionSeries)tabValStore[currTab].getAccelerometerPlot().Model.Series[2];

            accelerometerXSeries.Points.RemoveRange(0, accelerometerXSeries.Points.Count);
            accelerometerYSeries.Points.RemoveRange(0, accelerometerYSeries.Points.Count);
            accelerometerZSeries.Points.RemoveRange(0, accelerometerZSeries.Points.Count);

            List<DataPoint>[] accelerometerVals = guiSH.ElementAt(currTab).getAccelerometerData(tabValStore[currTab].getCurrentPosition() - 2, 2, 0);
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

            List<DataPoint>[] gyroscopeVals = guiSH.ElementAt(currTab).getGyroscopeData(tabValStore[currTab].getCurrentPosition() - 2, 2, 0);
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
            if (tabValStore[currTab].getCurrentPosition() > tabValStore[currTab].getLastPosition())
            {
                if (xSeries.Points.Count == 16000 && tabValStore[currTab].getCurrentPosition() >= 3)
                {
                    xSeries.Points.RemoveRange(0, 8000);
                    xSeries.Points.AddRange(guiSH.ElementAt(currTab).getNewAudioData(offSet, (tabValStore[currTab].getCurrentPosition() * 24) - 23, true));
                    tabValStore[currTab].getAudioLevelPlot().Model.Series[0] = xSeries;
                }
            }
            else if (tabValStore[currTab].getCurrentPosition() < tabValStore[currTab].getLastPosition() && tabValStore[currTab].getCurrentPosition() >= 2)
            {
                if (xSeries.Points.Count < 16000)
                {
                    offSet = offSet - (32000 - xSeries.Points.Count * 2);
                }
                xSeries.Points.RemoveRange(0, xSeries.Points.Count);
                xSeries.Points.InsertRange(0, guiSH.ElementAt(currTab).getNewAudioData(offSet, (tabValStore[currTab].getCurrentPosition() * 24) - 47, false));
                tabValStore[currTab].getAudioLevelPlot().Model.Series[0] = xSeries;
            }

        }

        private void shiftGraphs()
        {
            tabValStore[currTab].getGyroscopePlot().Model.Axes.ElementAt(0).Maximum = ((tabValStore[currTab].getCurrentPosition() * 24) + (1 - 0.003));
            tabValStore[currTab].getGyroscopePlot().Model.Axes.ElementAt(0).Minimum = (tabValStore[currTab].getCurrentPosition() * 24) - 47;

            tabValStore[currTab].getAccelerometerPlot().Model.Axes.ElementAt(0).Maximum = ((tabValStore[currTab].getCurrentPosition() * 24) + (1 - 0.003));
            tabValStore[currTab].getAccelerometerPlot().Model.Axes.ElementAt(0).Minimum = (tabValStore[currTab].getCurrentPosition() * 24) - 47;

            tabValStore[currTab].getAudioLevelPlot().Model.Axes.ElementAt(0).Maximum = ((tabValStore[currTab].getCurrentPosition() * 24) + (1 - 0.003));
            tabValStore[currTab].getAudioLevelPlot().Model.Axes.ElementAt(0).Minimum = (tabValStore[currTab].getCurrentPosition() * 24) - 47;

            tabValStore[currTab].getGyroscopePlot().InvalidatePlot(true);
            tabValStore[currTab].getAccelerometerPlot().InvalidatePlot(true);
            tabValStore[currTab].getAudioLevelPlot().InvalidatePlot(true);
        }

        private void syncForwards()
        {
            tabValStore[currTab].getMediaPlayer().settings.rate = 2;
            Thread.Sleep(300);
            tabValStore[currTab].getMediaPlayer().settings.rate = 1;
        }

        private void syncBackwards()
        {
            tabValStore[currTab].getMediaPlayer().settings.rate = 0.5;
            Thread.Sleep(300);
            tabValStore[currTab].getMediaPlayer().settings.rate = 1;
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

            //Using http://stackoverflow.com/questions/11624298/how-to-use-openfiledialog-to-select-a-folder
            //^ For opening folder and parsing file names. Accessed: 16/03/2017 @ 20:10
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            DialogResult result = openFolderDialog.ShowDialog();

            string videoPath = null;
            Boolean audioFound = false;
            string folderName = null;
            string path = null;

            PlotView pv;
            String accelerometerPath = null;
            String gyroscopePath = null;
            String audioPath = null;

            Boolean folderAlreadyOpen = false;

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string[] sensorFiles = Directory.GetFiles(openFolderDialog.SelectedPath);
                for (int j = 0; j < openFiles.Count; j++)
                {
                    if (openFiles.ElementAt(j).Equals(openFolderDialog.SelectedPath))
                    {
                        folderAlreadyOpen = true;
                    }
                }

                if (!folderAlreadyOpen)
                {
                    tab = new TabPage();
                    mediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
                    GUISetUpHelper gusetUpHelper = new GUISetUpHelper();

                    guiSH.Add(gusetUpHelper);
                    TabValueStore newTabValStore = new TabValueStore();
                    tabValStore.Add(newTabValStore);

                    setTrackBar();
                    setTimerLabel();
                    setStartButton();
                    setTimer();

                    folderName = openFolderDialog.SelectedPath.Substring(openFolderDialog.SelectedPath.LastIndexOf("\\") + 1);
                    openFiles.Add(openFolderDialog.SelectedPath);

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


                    if (videoPath != null && accelerometerPath != null && gyroscopePath != null)
                    {

                        if (!audioFound)
                        {

                            extractAudioFromVideo(videoPath, path);
                            audioPath = path + "//Audio.raw";
                            pv = guiSH.ElementAt(guiSH.Count - 1).createGraph(path + "//Audio.raw", "AudioLevel", tabValStore[tabValStore.Count - 1].getTrackBar());
                            tabValStore[tabValStore.Count - 1].setAudioLevelPlot(pv);
                            tab.Controls.Add(pv);
                        }

                        if (audioFound)
                        {
                            pv = guiSH.ElementAt(guiSH.Count - 1).createGraph(audioPath, "AudioLevel", tabValStore[tabValStore.Count - 1].getTrackBar());
                            tabValStore[tabValStore.Count - 1].setAudioLevelPlot(pv);
                            tab.Controls.Add(pv);
                        }

                        guiSH.ElementAt(guiSH.Count - 1).setAudioLevelsGraphData(audioPath, tabValStore[tabValStore.Count - 1].getTrackBar());

                        pv = guiSH.ElementAt(guiSH.Count - 1).createGraph(accelerometerPath, "Accelerometer", tabValStore[tabValStore.Count - 1].getTrackBar());
                        tabValStore[tabValStore.Count - 1].setAccelerometerPlot(pv);
                        tab.Controls.Add(pv);

                        guiSH.ElementAt(guiSH.Count - 1).setGyroscopeOrAccelerometerGraphData(accelerometerPath, "Accelerometer", tabValStore[tabValStore.Count - 1].getTrackBar());

                        pv = guiSH.ElementAt(guiSH.Count - 1).createGraph(gyroscopePath, "Gyroscope", tabValStore[tabValStore.Count - 1].getTrackBar());
                        tabValStore[tabValStore.Count - 1].setGyroscopePlot(pv);
                        tab.Controls.Add(pv);

                        guiSH.ElementAt(guiSH.Count - 1).setGyroscopeOrAccelerometerGraphData(gyroscopePath, "Gyroscope", tabValStore[tabValStore.Count - 1].getTrackBar());
                        tab.Controls.Add(mediaPlayer);

                        tabControl1.Selecting += new TabControlCancelEventHandler(tabControl1_SelectingTab);
                        tabControl1.Controls.Add(tab);
                        tabControl1.SelectedTab = tab;

                        guiSH.ElementAt(guiSH.Count - 1).setMediaPlayerProperties(videoPath, mediaPlayer);
                        mediaPlayer.Ctlcontrols.stop();
                        tabValStore[tabValStore.Count - 1].setMediaPlayer(mediaPlayer);
                    }
                    else
                    {
                        MessageBox.Show("Missing Files");
                        tabValStore.RemoveAt(tabValStore.Count - 1);
                        guiSH.RemoveAt(guiSH.Count - 1);
                        openFiles.RemoveAt(openFiles.Count - 1);
                    }
                }
                else
                {
                    MessageBox.Show("This folder is already open");
                }
            }
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
            guiSH.ElementAt(guiSH.Count - 1).setTrackBarProperties(trackBar);
            trackBar.ValueChanged += new System.EventHandler(trackBar_ValueChanged);
            tab.Controls.Add(trackBar);
            tabValStore[tabValStore.Count - 1].setTrackBar(trackBar);
        }

        private void setTimerLabel()
        {
            Label label = new Label();
            guiSH.ElementAt(guiSH.Count - 1).setTimeLabelProperties(label);
            tab.Controls.Add(label);
            tabValStore[tabValStore.Count - 1].setTimerLabel(label);
        }

        private void setGPSBox()
        {
            PictureBox pictureBox = new PictureBox();
            guiSH.ElementAt(guiSH.Count - 1).setGPSPointProperties(pictureBox);
            tab.Controls.Add(pictureBox);
        }

        private void setStartButton()
        {
            Button startAllButton = new Button();
            guiSH.ElementAt(guiSH.Count - 1).setStartButton(startAllButton);
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




