using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    class GUISetUpHelper
    {
        public void setTrackBarProperties(TrackBar trackBar)
        {
            trackBar.Location = new Point(68, 5);
            trackBar.Size = new Size(1162, 10);
            trackBar.TickStyle = 0;
            trackBar.TickFrequency = 0;
            trackBar.SmallChange = 0;
            trackBar.LargeChange = 0;
        }

        public void setTimeLabelProperties(Label timerLabel)
        {
            timerLabel.Location = new Point(1228, 8);
            timerLabel.Text = "00:00:00";
        }

        public void setGPSPointProperties(PictureBox pictureBox)
        {
            pictureBox.Location = new Point(5, 385);
            pictureBox.Size = new Size(450, 295);
            pictureBox.BackColor = Color.Navy;
            pictureBox.BorderStyle = BorderStyle.None;
        }

        public void setMediaPlayerProperties(string videoPath, AxWMPLib.AxWindowsMediaPlayer mediaPlayer)
        {
            mediaPlayer.settings.autoStart = true;
            mediaPlayer.URL = videoPath;
            mediaPlayer.Ctlcontrols.stop();
            mediaPlayer.Location = new Point(5, 50);
            mediaPlayer.Size = new Size(450, 330);
        }

        public void setStartButton(Button startAllButton)
        {
            startAllButton.Location = new Point(15, 6);
            startAllButton.Size = new Size(50, 20);
            startAllButton.Text = "Start";
        }

        private OxyPlot.Axes.LinearAxis setXAxis()
        {
            OxyPlot.Axes.LinearAxis xAxis = new OxyPlot.Axes.LinearAxis();
            xAxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            xAxis.Maximum = 120;
            xAxis.Minimum = 0;
            xAxis.Unit = " Frames ";
            xAxis.FontSize = 14;
            return xAxis;
        }

        private OxyPlot.Axes.LinearAxis setYAxis()
        {
            OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis();
            yAxis.Position = OxyPlot.Axes.AxisPosition.Left;
            yAxis.Unit = " m/" + "s\u00B2 ";
            yAxis.FontSize = 14;
            return yAxis;
        }

        private void checkMaxValY(double x, double y, double z, ref double maxValYAxis)
        {
            if (x > maxValYAxis)
                maxValYAxis = x;
            if (y > maxValYAxis)
                maxValYAxis = y;
            if (z > maxValYAxis)
                maxValYAxis = z;
        }

        private void checkMinValY(double x, double y, double z, ref double minValYAxis)
        {
            if (x < minValYAxis)
                minValYAxis = x;
            if (y < minValYAxis)
                minValYAxis = y;
            if (z < minValYAxis)
                minValYAxis = z;
        }

        private void setLegend(ref PlotModel pm)
        {
            pm.IsLegendVisible = true;
            pm.LegendPlacement = LegendPlacement.Outside;
            pm.LegendPosition = LegendPosition.BottomLeft;
            pm.LegendOrientation = LegendOrientation.Horizontal;
            pm.LegendMaxHeight = 1;
            pm.LegendMargin = 0;
            pm.LegendSymbolLength = 16;
            pm.LegendFontSize = 12;
            pm.LegendPadding = 0;
        }


        public OxyPlot.WindowsForms.PlotView parseAndCreateGraph(string filePath, string graphType, TrackBar trackBar)
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
            else if (graphType.Equals("Gyroscope"))
            {
                pv.Location = new Point(460, 260);
                pv.Size = new Size(829, 205);
            }
            else if (graphType.Equals("AudioLevelData"))
            {
                pv.Location = new Point(460, 475);
                pv.Size = new Size(829, 205);
            }

            pv.Controller = new OxyPlot.PlotController();
            pv.Controller.UnbindKeyDown(OxyKey.Right);
            pv.Controller.UnbindKeyDown(OxyKey.Left);

            xAxis = setXAxis();
            yAxis = setYAxis();


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

            double maxValYAxis = 0, minValYAxis = 0;

            string line;
            StreamReader file = new StreamReader(filePath);
            line = file.ReadLine();
            string[] parsedLine = line.Split(',');
            double startValNegation = (Convert.ToDouble(parsedLine[3]));
            double lastTime = 0;

            double x = Convert.ToDouble(parsedLine[0]);
            double y = Convert.ToDouble(parsedLine[1]);
            double z = Convert.ToDouble(parsedLine[2]);

            xSeries.Points.Add(new DataPoint(0, z));
            ySeries.Points.Add(new DataPoint(0, y));
            zSeries.Points.Add(new DataPoint(0, z));

            checkMaxValY(x, y, z, ref maxValYAxis);
            checkMinValY(x, y, z, ref minValYAxis);

            double firstVal = Convert.ToDouble(parsedLine[2]) / 1000000000.0;
            double lastVal = 0;
            long frames = 1;
            double timeStamp;
            double seconds;

            while ((line = file.ReadLine()) != null)
            {
                parsedLine = line.Split(',');
                if (parsedLine.Length == 4)
                {
                    if (Convert.ToDouble(parsedLine[3]) > lastTime)
                    {
                        x = Convert.ToDouble(parsedLine[0]);
                        y = Convert.ToDouble(parsedLine[1]);
                        z = Convert.ToDouble(parsedLine[2]);

                        checkMaxValY(x, y, z, ref maxValYAxis);
                        checkMinValY(x, y, z, ref minValYAxis);

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

            double step = maxValYAxis;

            if (Math.Abs(minValYAxis) > maxValYAxis)
            {
                step = Math.Abs(minValYAxis);
            }
            yAxis.MajorStep = (int)Math.Ceiling(step);
            yAxis.Maximum = (int)Math.Ceiling(maxValYAxis);
            if (minValYAxis >= 0)
            {
                yAxis.Minimum = (int)Math.Ceiling(minValYAxis);
            }
            else
            {
                yAxis.Minimum = -((int)Math.Ceiling(Math.Abs(minValYAxis)));
            }


            if (lastVal > trackBar.Maximum)
                trackBar.Maximum = (int)Math.Ceiling(lastVal);

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

            setLegend(ref pm);

            pm.Series.Add(xSeries);
            pm.Series.Add(ySeries);
            pm.Series.Add(zSeries);

            pm.Axes.Add(xAxis);
            pm.Axes.Add(yAxis);
            pv.Model = pm;


            file.Close();
            return pv;
        }

    }
}
