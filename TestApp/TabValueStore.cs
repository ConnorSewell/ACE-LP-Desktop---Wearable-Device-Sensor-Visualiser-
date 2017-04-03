using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    class TabValueStore
    {
        private TrackBar trackBar;
        private Label timerLabel;
        private System.Timers.Timer timer;
        private OxyPlot.WindowsForms.PlotView accelerometerPlot;
        private OxyPlot.WindowsForms.PlotView gyroscopePlot;
        private OxyPlot.WindowsForms.PlotView audioLevelPlot;
        private Button startStopButton;
        private double elapsedTime = 0.00;
        Boolean allPlaying = false;
        private AxWMPLib.AxWindowsMediaPlayer mediaPlayer;

        public void setElapsedTime(double elapsedTime)
        {
            this.elapsedTime = elapsedTime;
        }

        public double getElapsedTime()
        {
            return elapsedTime;
        }

        public void setAllPlaying()
        {
            allPlaying = !allPlaying;
        }

        public Boolean getAllPlaying()
        {
            return allPlaying;
        }

        public void setMediaPlayer(AxWMPLib.AxWindowsMediaPlayer mediaPlayer)
        {
            this.mediaPlayer = mediaPlayer;
        }

        public AxWMPLib.AxWindowsMediaPlayer getMediaPlayer()
        {
            return mediaPlayer;
        }

        public void setStartStopButton(Button startStopButton)
        {
            this.startStopButton = startStopButton;
        }

        public void setStartStopButtonText(string text)
        {
            this.startStopButton.Text = text;
        }

        public Button getStartStopButton()
        {
            return startStopButton;
        }

        public void setTrackBar(TrackBar trackBar)
        {
            this.trackBar = trackBar;
        }

        public TrackBar getTrackBar()
        {
            return trackBar;
        }

        public void setTimerLabel(Label timerLabel)
        {
            this.timerLabel = timerLabel;
        }

        public Label getTimerLabel()
        {
            return timerLabel;
        }

        public void setTimer(System.Timers.Timer timer)
        {
            this.timer = timer;
        }

        public System.Timers.Timer getTimer()
        {
            return timer;
        }

        public void setAccelerometerPlot(OxyPlot.WindowsForms.PlotView accelerometerPlot)
        {
            this.accelerometerPlot = accelerometerPlot;
        }

        public OxyPlot.WindowsForms.PlotView getAccelerometerPlot()
        {
            return accelerometerPlot;
        }

        public void setGyroscopePlot(OxyPlot.WindowsForms.PlotView gyroscopePlot)
        {
            this.gyroscopePlot = gyroscopePlot;
        }

        public OxyPlot.WindowsForms.PlotView getGyroscopePlot()
        {
            return gyroscopePlot;
        }

        public void setAudioLevelPlot(OxyPlot.WindowsForms.PlotView audioLevelPlot)
        {
            this.audioLevelPlot = audioLevelPlot;
        }

        public OxyPlot.WindowsForms.PlotView getAudioLevelPlot()
        {
            return audioLevelPlot;
        }
    }
}
