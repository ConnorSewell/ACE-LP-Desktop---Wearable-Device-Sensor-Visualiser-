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
 

        int tabCount = 0;
        List<TabValueStore> tabValStore = new List<TabValueStore>();
        GUISetUpHelper guiSH = new GUISetUpHelper();
        int currTab;

        public Form1()
        {
           
            InitializeComponent();
            tabControl1.MouseDoubleClick += tabControl1_MouseDoubleClick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           

        }

        //http://stackoverflow.com/questions/25478922/how-to-trigger-event-when-clicking-on-a-selected-tab-page-header-of-a-tab-contro
        //^ Accessed: 06/04/2017 @ 12:20 --- Used to get index of clicked tab header
        private void tabControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < tabControl1.TabCount; i++)
            {
                if(tabControl1.GetTabRect(i).Contains(e.Location))
                {
                    tabControl1.TabPages.Remove(tabControl1.TabPages[i]);
                    tabValStore.RemoveAt(i);

                    MessageBox.Show("Tab size: " + tabValStore.Count);
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
                tabValStore[currTab].setAllPlaying();

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
                    }
                };

                this.Invoke(methodInvoker);
            }
        }

        private void tabControl1_SelectingTab(Object sender, TabControlCancelEventArgs e)
        {
            currTab = tabControl1.SelectedIndex;
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

        private void periodicGraphUpdate(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine(source.ToString());

            //http://stackoverflow.com/questions/14890295/update-label-from-another-thread
            //^ Accessed: 27/03/2017 @ 03:30
            MethodInvoker methodInvoker = delegate
            {
                Console.WriteLine("... " + currTab);
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

        int count = 0;
        int testCounter = 0;

        int lastPosition = 0;
       
        private void trackBar_ValueChanged(object sender, System.EventArgs e)
        {
            
            currentPosition = tabValStore[currTab].getTrackBar().Value;
            //currentVal = tabValStore[currTab].getTrackBar().Value;

            if (tabValStore[currTab].getAllPlaying())
            {

                if (currentPosition <= tabValStore[currTab].getMediaPlayer().currentMedia.duration)
                {
                    if (tabValStore[currTab].getMediaPlayer().playState != WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        tabValStore[currTab].getMediaPlayer().Ctlcontrols.play();
                    }
                }

                if (tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition > currentPosition + 0.5)
                {
                    //mediaPlayers[currTab].Ctlcontrols.currentPosition = currentPosition;

                    //Console.WriteLine("Media player ahead by at least 0.01");
                    //Console.WriteLine("Current position:" + currentPosition);
                }
                else if (tabValStore[currTab].getMediaPlayer().Ctlcontrols.currentPosition < currentPosition - 0.5)
                {
                    if (tabValStore[currTab].getMediaPlayer().playState == WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        //Console.WriteLine("Current Media position:" + mediaPlayers[currTab].Ctlcontrols.currentPosition);
                        //currentVal = mediaPlayers[currTab].Ctlcontrols.currentPosition;
                        //mediaPlayers[currTab].Ctlcontrols.currentPosition = currentPosition;
                    }
                }
            }


            tabValStore[currTab].getGyroscopePlot().Model.Axes.ElementAt(0).Maximum = ((currentPosition * 24) + (1-0.003));
            tabValStore[currTab].getGyroscopePlot().Model.Axes.ElementAt(0).Minimum = (currentPosition * 24) - 47;

            tabValStore[currTab].getAccelerometerPlot().Model.Axes.ElementAt(0).Maximum = ((currentPosition * 24) + (1 - 0.003));
            tabValStore[currTab].getAccelerometerPlot().Model.Axes.ElementAt(0).Minimum = (currentPosition * 24) - 47;

            tabValStore[currTab].getAudioLevelPlot().Model.Axes.ElementAt(0).Maximum = ((currentPosition * 24) + (1 - 0.003));
            tabValStore[currTab].getAudioLevelPlot().Model.Axes.ElementAt(0).Minimum = (currentPosition * 24) - 47;

            tabValStore[currTab].getGyroscopePlot().InvalidatePlot(true);
            tabValStore[currTab].getAccelerometerPlot().InvalidatePlot(true);
            tabValStore[currTab].getAudioLevelPlot().InvalidatePlot(true);

           double val = 0.00;
           if (testCounter >= 2)
           {
                FunctionSeries xSeries = (FunctionSeries)tabValStore[currTab].getAudioLevelPlot().Model.Series[0];
                if (currentPosition > lastPosition)
                {
                    if (xSeries.Points.Count >= 8000)
                    {
                        xSeries.Points.RemoveRange(0, 8000);
                        xSeries.Points.AddRange(guiSH.getNewAudioData(16000, (currentPosition * 24) - 23));
                        tabValStore[currTab].getAudioLevelPlot().Model.Series[0] = xSeries;
                    }
                }
                else
                {
                   if(xSeries.Points.Count >= 16000)
                  {
                        //MessageBox.Show("Size is: " + xSeries.Points.Count);
                        xSeries.Points.RemoveRange(0, 16000);
                        //xSeries.Points.
                        xSeries.Points.InsertRange(0, guiSH.getNewAudioData(32000, 1));
                        tabValStore[currTab].getAudioLevelPlot().Model.Series[0] = xSeries;
                    }
                }
           
            }
           
            count++;
            testCounter++;

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

            if (mins < 10)
            {
                convertedMins = "0" + mins.ToString();
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

        private void syncSleeper()
        {
            Thread.Sleep(15);
            MethodInvoker methodInvoker = delegate
            {
                tabValStore[currTab].getMediaPlayer().Ctlcontrols.play();
            };

            this.Invoke(methodInvoker);

        }

        private void Form1_Load_2(object sender, EventArgs e)
        {

        }
//
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
            setGPSBox();
            setStartButton();
            setTimer();

            //allPlaying.Add(false);
            //elapsedTimes.Add(0.00);

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
                            pv = guiSH.createGraph(sensorFiles[i], "Accelerometer", tabValStore[tabValStore.Count - 1].getTrackBar());
                            tabValStore[tabValStore.Count - 1].setAccelerometerPlot(pv);
                            tab.Controls.Add(pv);
                            break;
                        case "GyroscopeData.txt":
                            pv = guiSH.createGraph(sensorFiles[i], "Gyroscope", tabValStore[tabValStore.Count - 1].getTrackBar());
                            tabValStore[tabValStore.Count - 1].setGyroscopePlot(pv);
                            tab.Controls.Add(pv);
                            break;
                        case "Video.mp4":
                            videoPath = sensorFiles[i];
                            break;
                        case "Audio.raw":
                            audioFound = true;
                            pv = guiSH.createGraph(sensorFiles[i], "AudioLevel", tabValStore[tabValStore.Count - 1].getTrackBar());
                            tabValStore[tabValStore.Count - 1].setAudioLevelPlot(pv);
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
                pv = guiSH.createGraph(path + "//Audio.raw", "AudioLevel", tabValStore[tabValStore.Count - 1].getTrackBar());
                tabValStore[tabValStore.Count - 1].setAudioLevelPlot(pv);
                tab.Controls.Add(pv);
            }

            tab.Controls.Add(mediaPlayer);

            tabControl1.Selecting += new TabControlCancelEventHandler(tabControl1_SelectingTab);
            tabControl1.Controls.Add(tab);
           
            guiSH.setMediaPlayerProperties(videoPath, mediaPlayer);
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
            timer.Interval = 1000;
            timer.Elapsed += periodicGraphUpdate;
            tabValStore[tabValStore.Count - 1].setTimer(timer);

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
    }
}




