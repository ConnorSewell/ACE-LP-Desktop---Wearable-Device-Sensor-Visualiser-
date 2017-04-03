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
        private static double[] audioValues = new double[16000];
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

        private OxyPlot.Axes.LinearAxis setXAxis(int max)
        {
            OxyPlot.Axes.LinearAxis xAxis = new OxyPlot.Axes.LinearAxis();
            xAxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            xAxis.Maximum = max;
            xAxis.Minimum = 0;
            xAxis.Unit = " Frames ";
            xAxis.FontSize = 12;
            return xAxis;
        }

        private OxyPlot.Axes.LinearAxis setYAxis(string unit)
        {
            OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis();
            yAxis.Position = OxyPlot.Axes.AxisPosition.Left;
            yAxis.Unit = unit;
            yAxis.FontSize = 14;
            return yAxis;
        }

        private void checkMaxVal3Y(double x, double y, double z, ref double maxValYAxis)
        {
            if (x > maxValYAxis)
                maxValYAxis = x;
            if (y > maxValYAxis)
                maxValYAxis = y;
            if (z > maxValYAxis)
                maxValYAxis = z;
        }

        private void checkMinVal3Y(double x, double y, double z, ref double minValYAxis)
        {
            if (x < minValYAxis)
                minValYAxis = x;
            if (y < minValYAxis)
                minValYAxis = y;
            if (z < minValYAxis)
                minValYAxis = z;
        }

        private void setLegend()
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

        OxyPlot.Axes.LinearAxis xAxis = new OxyPlot.Axes.LinearAxis();
        OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis();
        PlotModel pm;
        OxyPlot.WindowsForms.PlotView pv;

        public OxyPlot.WindowsForms.PlotView createGraph(string filePath, string graphType, TrackBar trackBar)
        {
            xAxis = new OxyPlot.Axes.LinearAxis();
            yAxis = new OxyPlot.Axes.LinearAxis();
            pm = new PlotModel();
            pv = new OxyPlot.WindowsForms.PlotView();


            pv.Controller = new OxyPlot.PlotController();
            pv.Controller.UnbindKeyDown(OxyKey.Right);
            pv.Controller.UnbindKeyDown(OxyKey.Left);

            pm.TextColor = OxyColor.FromRgb(0, 0, 0);
            pm.Padding = new OxyThickness(0, 10, 25, 15);
            pv.BackColor = Color.White;

            //pm.Title = graphType;
            //pm.TitleFontSize = 12;
            //pm.TitleFontWeight = 0;

            //https://www.youtube.com/watch?v=VC-nlI_stx4
            //^ Temp ref
            if (graphType.Equals("Accelerometer"))
            {
                pv.Location = new Point(460, 50);
                pv.Size = new Size(829, 205);
                xAxis = setXAxis(120);
                yAxis = setYAxis(" m/" + "s\u00B2 ");
                setGyroscopeOrAccelerometerGraphData(filePath, graphType, trackBar);
            }
            else if (graphType.Equals("Gyroscope"))
            {
                pv.Location = new Point(460, 260);
                pv.Size = new Size(829, 205);
                xAxis = setXAxis(120);
                yAxis = setYAxis(" m/" + "s\u00B2 ");
                setGyroscopeOrAccelerometerGraphData(filePath, graphType, trackBar);
            }
            else if (graphType.Equals("AudioLevel"))
            {
                pv.Location = new Point(460, 475);
                pv.Size = new Size(829, 205);
                xAxis = setXAxis(48);
                yAxis = setYAxis(" dB ");
                setAudioLevelsGraphData(filePath, trackBar);
            }


            pm.Axes.Add(xAxis);
            pm.Axes.Add(yAxis);
            pv.Model = pm;

            return pv;
        }

        Stream inputStream;

        private void setAudioLevelsGraphData(string filePath, TrackBar trackBar)
        {
            FunctionSeries xSeries = new FunctionSeries();
            xSeries.StrokeThickness = 0.1;
            xSeries.Color = OxyColor.FromRgb(139, 0, 0);

            double maxValYAxis = 34000, minValYAxis = -34000, lastVal = 0;
            byte[] bytes = new byte[32000];
            int bytesRead = 0, total = 0, count = 0;

            inputStream = File.OpenRead(filePath);
            IList<DataPoint> tester = getNewAudioData(32000, 0);
            //IList<DataPoint> tester2 = getNewAudioData(8000);
            xSeries.Points.AddRange(tester);

            setValueRanges(trackBar, minValYAxis, maxValYAxis, lastVal);
            setLegend();

            xSeries.Title = "Audio Level Data (RED)";
            pm.Series.Add(xSeries);

        }

        private void setGyroscopeOrAccelerometerGraphData(string filePath, string graphType, TrackBar trackBar)
        {
            FunctionSeries xSeries = new FunctionSeries();
            FunctionSeries ySeries = new FunctionSeries();
            FunctionSeries zSeries = new FunctionSeries();

            xSeries.StrokeThickness = 0.1;
            xSeries.Color = OxyColor.FromRgb(139, 0, 0);

            ySeries.StrokeThickness = 0.1;
            ySeries.Color = OxyColor.FromRgb(50, 100, 0);

            zSeries.StrokeThickness = 0.1;
            zSeries.Color = OxyColor.FromRgb(25, 25, 112);

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

            xSeries.Points.Add(new DataPoint(1, z));
            ySeries.Points.Add(new DataPoint(1, y));
            zSeries.Points.Add(new DataPoint(1, z));

            checkMaxVal3Y(x, y, z, ref maxValYAxis);
            checkMinVal3Y(x, y, z, ref minValYAxis);

            double firstVal = Convert.ToDouble(parsedLine[2]) / 1000000000.0;
            double lastVal = 0;
            long frames = 2;
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

                        checkMaxVal3Y(x, y, z, ref maxValYAxis);
                        checkMinVal3Y(x, y, z, ref minValYAxis);

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

            setValueRanges(trackBar, minValYAxis, maxValYAxis, lastVal);
            setLegend();

            xSeries.Title = graphType + " X (RED)";
            ySeries.Title = graphType + " Y (GREEN)";
            zSeries.Title = graphType + " Z (BLUE)";

            pm.Series.Add(xSeries);
            pm.Series.Add(ySeries);
            pm.Series.Add(zSeries);

            file.Close();
        }


        public IList<DataPoint> getNewAudioData(int amount, int lastFrame)
        {
            IList<DataPoint> data = new List<DataPoint>();

            byte[] bytes = new byte[amount];
            int bytesRead = 0;

            double precisionFactor = 0.003;
            int count = 1;

            if (lastFrame != 0)
            {
                count = ((lastFrame / 24) * 8000) + 1;
            }

            bytesRead = inputStream.Read(bytes, 0, amount);

            for (int i = 0; i < bytesRead; i+=2)
            {
                short shortVal = (short)((bytes[i]) | (bytes[i + 1]) << 8);
                data.Add(new DataPoint(precisionFactor * count, Convert.ToInt16(shortVal)));
                count++;
            }

            return data;
        }

        private void setValueRanges(TrackBar trackBar, double minValYAxis, double maxValYAxis, double lastVal)
        {
            if (lastVal > trackBar.Maximum)
                trackBar.Maximum = (int)Math.Ceiling(lastVal);

            double step = maxValYAxis;

            if (Math.Abs(minValYAxis) > maxValYAxis)
            {
                step = Math.Abs(minValYAxis);
            }

            yAxis.MajorStep = (int)Math.Ceiling(step);
            yAxis.Maximum = (int)Math.Ceiling(maxValYAxis);

            yAxis.Minimum = (int)Math.Ceiling(minValYAxis);
            
         

        }
      
    }
}


//using (Stream fileInputStream = File.OpenRead(filePath))
// {
//     while ((bytesRead = fileInputStream.Read(bytes, 0, 32000)) > 0)
//     {
//         for (int i = 0; i < bytesRead; i += 2)
//         {
//             short shortVal = (short)((bytes[i]) | (bytes[i + 1]) << 8);
//             Console.WriteLine(Convert.ToInt16(shortVal));
//         }

//         total += bytesRead;
//         count++;
//     }
// }

//      if (bytesRead != 32000)
//      {
//          for (int i = 0; i < bytesRead; i += 2)
//          {

// audioValues.Add((shortVal));
//              Console.WriteLine(Convert.ToInt16(shortVal));
//          }
//          total += bytesRead;
//      }

// double val = 0.00;
// for (int i = 1; i < 16001; i++)
// {
// string test = audioValues.ElementAt(i).ToString();
//     val = (double)(i/((double)8000/(double)24));
//Console.WriteLine(test);
//xSeries.Points.Add(new DataPoint(val, audioValues.ElementAt(i)));

// Console.WriteLine("i is: " + i);

//xSeries.Points.Add(new DataPoint((currentVal * 24) + i, rand.Next(0, 100)));
//}