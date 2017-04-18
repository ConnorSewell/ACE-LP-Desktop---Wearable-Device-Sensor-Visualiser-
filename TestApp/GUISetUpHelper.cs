using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        long startTime = 0;
        public void setTrackBarProperties(TrackBar trackBar)
        {
            trackBar.Location = new Point(68, 5);
            trackBar.Size = new Size(1297, 10);
            trackBar.TickStyle = 0;
            trackBar.TickFrequency = 0;
            trackBar.SmallChange = 0;
            trackBar.LargeChange = 0;
        }

        public void setTimeLabelProperties(Label timerLabel)
        {
            timerLabel.Location = new Point(1363, 8);
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
            mediaPlayer.URL = videoPath;
            mediaPlayer.Location = new Point(5, 175);
            mediaPlayer.Size = new Size(510, 375);
            mediaPlayer.stretchToFit = false;
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
            xAxis.Maximum = 48.997;
            xAxis.Minimum = 1;
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

            //https://www.youtube.com/watch?v=VC-nlI_stx4
            //^ Temp ref
            if (graphType.Equals("Accelerometer"))
            {
                pv.Location = new Point(520, 50);
                pv.Size = new Size(900, 205);
              
                xAxis = setXAxis(49);
                yAxis = setYAxis(" m/" + "s\u00B2 ");
                setGyroscopeOrAccelerometerGraphData(filePath, graphType, trackBar);
            }
            else if (graphType.Equals("Gyroscope"))
            {
                pv.Location = new Point(520, 260);
                pv.Size = new Size(900, 205);
              
                xAxis = setXAxis(49);
                yAxis = setYAxis(" rad/s ");
                setGyroscopeOrAccelerometerGraphData(filePath, graphType, trackBar);
            }
            else if (graphType.Equals("AudioLevel"))
            {
                pv.Location = new Point(520, 470);
                pv.Size = new Size(900, 205);
          
                xAxis = setXAxis(49);
                yAxis = setYAxis(" Audio Level % ");
                setAudioLevelsGraphData(filePath, trackBar);
            }


            pm.Axes.Add(xAxis);
            pm.Axes.Add(yAxis);
            pv.Model = pm;

            return pv;
        }
  
        BinaryReader audioBinaryReader;
        double audioLength;

        private void setAudioLevelsGraphData(string filePath, TrackBar trackBar)
        {
            FunctionSeries xSeries = new FunctionSeries();
            xSeries.StrokeThickness = 0.1;
            xSeries.Color = OxyColor.FromRgb(139, 0, 0);
            xSeries.Title = "Audio Level Data (RED)";

            double maxValYAxis = 100, minValYAxis = -100;

            audioBinaryReader = new BinaryReader(File.Open(filePath, FileMode.Open));
            startTime = audioBinaryReader.BaseStream.Length;

            audioLength = (double)startTime / 16000;

            setValueRanges(trackBar, minValYAxis, maxValYAxis, audioLength);
            setLegend();
            xSeries.Points.AddRange(getNewAudioData(0, 1, true));
            pm.Series.Add(xSeries);

        }


        List<String> accelerometerData = new List<String>();
        List<String> gyroscopeData = new List<String>();

        //Returns series for gryoscope or accelerometer data
        public List<DataPoint>[] getAccelerometerData(double startTime, double duration, int offSet)
        {
            Boolean directionRight = true;
            // if (offSet < 0)
            //     directionRight = false;

            startTime += accelerometerStartTime;
            List<DataPoint>[] seriesData = new List<DataPoint>[3];
            seriesData[0] = new List<DataPoint>();
            seriesData[1] = new List<DataPoint>();
            seriesData[2] = new List<DataPoint>();

            int currIndex = 0;
            double timestamp = 0;

            for (int i = 0; i < accelerometerData.Count; i++)
            {
                String[] vals = accelerometerData.ElementAt(i).Split(',');
                timestamp = ((Double.Parse(vals[3]) - accelerometerFirstTime) / 1000000000); 

                if(timestamp >= startTime)
                {
                    if (timestamp <= (startTime + duration))
                    {
                        seriesData[0].Add(new DataPoint(1 + (timestamp - accelerometerStartTime) * 24, Double.Parse(vals[0])));
                        seriesData[1].Add(new DataPoint(1 + (timestamp - accelerometerStartTime) * 24, Double.Parse(vals[1])));
                        seriesData[2].Add(new DataPoint(1 + (timestamp - accelerometerStartTime) * 24, Double.Parse(vals[2])));
                    }
                    else
                    {
                        seriesData[0].Add(new DataPoint(1 + (timestamp - accelerometerStartTime) * 24, Double.Parse(vals[0])));
                        seriesData[1].Add(new DataPoint(1 + (timestamp - accelerometerStartTime) * 24, Double.Parse(vals[1])));
                        seriesData[2].Add(new DataPoint(1 + (timestamp - accelerometerStartTime) * 24, Double.Parse(vals[2])));
                        break;
                    }
                }

                currIndex++;
            }

            return seriesData;
        }

  
        public List<DataPoint>[] getGyroscopeData(double startTime, double duration, int offSet)
        {
            startTime += gyroscopeStartTime;
            List<DataPoint>[] seriesData = new List<DataPoint>[3];
            seriesData[0] = new List<DataPoint>();
            seriesData[1] = new List<DataPoint>();
            seriesData[2] = new List<DataPoint>();

            double timestamp = 0;
            for (int i = 0; i < gyroscopeData.Count; i++)
            {
               String[] vals = gyroscopeData.ElementAt(i).Split(',');
               timestamp = ((Double.Parse(vals[3]) - gyroscopeFirstTime) / 1000000000);

                if (timestamp >= startTime)
                {
                    if (timestamp <= (startTime + duration))
                    {
                        seriesData[0].Add(new DataPoint(1 + (timestamp - gyroscopeStartTime) * 24, Double.Parse(vals[0])));
                        seriesData[1].Add(new DataPoint(1 + (timestamp - gyroscopeStartTime) * 24, Double.Parse(vals[1])));
                        seriesData[2].Add(new DataPoint(1 + (timestamp - gyroscopeStartTime) * 24, Double.Parse(vals[2])));
                    }
                    else
                    {
                        seriesData[0].Add(new DataPoint(1 + (timestamp - gyroscopeStartTime) * 24, Double.Parse(vals[0])));
                        seriesData[1].Add(new DataPoint(1 + (timestamp - gyroscopeStartTime) * 24, Double.Parse(vals[1])));
                        seriesData[2].Add(new DataPoint(1 + (timestamp - gyroscopeStartTime) * 24, Double.Parse(vals[2])));
                        break;
                    }
                }
            }
            return seriesData;
        }

        double startValNegation;
        double accelerometerStartTime;
        double gyroscopeStartTime;
        double accelerometerFirstTime;
        double gyroscopeFirstTime;

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
            startValNegation = (Convert.ToDouble(parsedLine[3]));

            if(graphType.Equals("Accelerometer"))
            {
                accelerometerFirstTime = startValNegation;
            }
            else
            {
                gyroscopeFirstTime = startValNegation;
            }

            double lastTime = 0;

            double x = Convert.ToDouble(parsedLine[0]);
            double y = Convert.ToDouble(parsedLine[1]);
            double z = Convert.ToDouble(parsedLine[2]);

            if(graphType.Equals("Accelerometer"))
            {
                accelerometerData.Add(line);
            }
            else if(graphType.Equals("Gyroscope"))
            {
                gyroscopeData.Add(line);
            }

            checkMaxVal3Y(x, y, z, ref maxValYAxis);
            checkMinVal3Y(x, y, z, ref minValYAxis);

            double numberInCheck;

            while ((line = file.ReadLine()) != null)
            {
                parsedLine = line.Split(',');
                if (parsedLine.Length == 4)
                {
                    bool numerCheck = double.TryParse(parsedLine[3], out numberInCheck);
                    if(numerCheck)
                    {
                        if (Convert.ToDouble(parsedLine[3]) > lastTime)
                        {
                            x = Convert.ToDouble(parsedLine[0]);
                            y = Convert.ToDouble(parsedLine[1]);
                            z = Convert.ToDouble(parsedLine[2]);

                            checkMaxVal3Y(x, y, z, ref maxValYAxis);
                            checkMinVal3Y(x, y, z, ref minValYAxis);

                            lastTime = Convert.ToDouble(parsedLine[3]);

                            if (graphType.Equals("Accelerometer"))
                            {
                                accelerometerData.Add(line);
                            }
                            else if (graphType.Equals("Gyroscope"))
                            {
                                gyroscopeData.Add(line);
                            }

                        }
                    }
                }
                else
                {
                    //Rough compensation for any data which failed to write properly at end.
                    lastTime += 20000000;
                }
            }
           
            if(graphType.Equals("Accelerometer"))
            {
                accelerometerStartTime = ((lastTime - startValNegation) - audioLength * 1000000000)/1000000000;
            }
            else if (graphType.Equals("Gyroscope"))
            {
                gyroscopeStartTime = ((lastTime - startValNegation) - audioLength * 1000000000) / 1000000000;
            }

            setValueRanges(trackBar, minValYAxis, maxValYAxis, audioLength);
            setLegend();

            xSeries.Title = graphType + " X (RED)";
            ySeries.Title = graphType + " Y (GREEN)";
            zSeries.Title = graphType + " Z (BLUE)";

            if(graphType.Equals("Accelerometer"))
            {
                List<DataPoint>[] values = getAccelerometerData(0, 2, 0);
                xSeries.Points.AddRange(values[0]);
                ySeries.Points.AddRange(values[1]);
                zSeries.Points.AddRange(values[2]);
            }
            else if(graphType.Equals("Gyroscope"))
            {
                List<DataPoint>[] values = getGyroscopeData(0, 2, 0);
                xSeries.Points.AddRange(values[0]);
                ySeries.Points.AddRange(values[1]);
                zSeries.Points.AddRange(values[2]);
            }

            pm.Series.Add(xSeries);
            pm.Series.Add(ySeries);
            pm.Series.Add(zSeries);

            file.Close();
        }

        Boolean lastDirectionRight;

        public IList<DataPoint> getNewAudioData(int offset, int currFrame, Boolean directionRight)
        {
            IList<DataPoint> data = new List<DataPoint>();

            lastDirectionRight = directionRight;

            byte[] bytes;

            double precisionFactor = 0.003;
            double xPoint = 0;
            
            long start = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            int count = ((currFrame / 24) * 8000);
            int amount = 0;

            if (currFrame != 1 && directionRight)           
                amount = 16000;  
            else   
                amount = 32000;

            if (!directionRight)
                offset = -offset;

            audioBinaryReader.BaseStream.Seek(offset, SeekOrigin.Current);
            bytes = audioBinaryReader.ReadBytes(amount);

            double amplitude = 0;
            for (int i = 0; i < bytes.Length; i+=2)
            {
                short shortVal = (short)((bytes[i]) | (bytes[i + 1]) << 8);
                amplitude = Convert.ToInt16(shortVal);
                if (amplitude > 0)
                    amplitude = (amplitude / 32767) * 100;
                else
                    amplitude = (amplitude / 32768) * 100;

                xPoint = (precisionFactor * count) + 1;
                data.Add(new DataPoint(xPoint, amplitude));
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

            yAxis.Minimum = (int)Math.Floor(minValYAxis);
            
        }
      
    }
}
