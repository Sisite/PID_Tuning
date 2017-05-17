using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using System.Globalization;
using Wpf.CartesianChart.ConstantChanges;
using System.Threading.Tasks;


namespace PIDTuner1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //float errorValue = 0;
        SerialPort serialPort;
        Casting formCast = new Casting();
        int threadStarted = 0;
        //ConstantChangesChart cw = new ConstantChangesChart();
        
        public MainWindow()
        {
            InitializeComponent();
            
            getComPorts();
            disconnectBtn.IsEnabled = false;
            fetchBtn.IsEnabled = false;
           // byte[] tmp = formCast.stringToArray("H");
            //Console.WriteLine(formCast.stringToArray("HEJ"));
            //pidSteeringCollection;
            //CartesianChart cartesianChart = new CartesianChart();
            SeriesCollection SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { -1.0, 0.01, 5.0, 2.0 ,4.0 }
                }
            };

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            YFormatter = value => value.ToString("C");

            //modifying the series collection will animate and update the chart
           /* SeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { -0.1 , 0.1, 1, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });*/

            //modifying any series values will also animate and update the chart
           // SeriesCollection[1].Values.Add(0d);
            SeriesCollection[0].Values.Add(5d);

            //cartesianChart.Series = SeriesCollection;
            

            DataContext = this;
            

            //cw.ShowInTaskbar = false;
            //cw.Owner = Application.Current.MainWindow;
            //cw.Show();

        }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        //SeriesCollection pidSteeringCollection = new SeriesCollection
        //{
        //    new LineSeries
        //    {
        //        Values = new ChartValues<double> {3,5,6,2}
        //    },
        //    new ColumnSeries
        //    {
        //        Values = new ChartValues<decimal> {4,6,2,4}
        //    }
        //};
    

        private void getComPorts ()
        {
            string[] comPorts = SerialPort.GetPortNames();
            ComboBoxItem tmpItem;
            foreach (string port in comPorts)
            {
                tmpItem = new ComboBoxItem();
                tmpItem.Tag = port;
                tmpItem.Content = port;
                comPortList.Items.Add(tmpItem);

            }
            if (comPortList.Items.Count > 0)
            {
                (comPortList.Items[0] as ComboBoxItem).IsSelected = true;
            }
        }



        //Communication example T:M:4000.0:4000.0:4000.0:;
        //                   MODE:P:I:D:Endstring;   
        //Communication example with Steering T:M:1.0:2.5:3.5:S:0.5:0.3:4.3:;

        private void getParameters() {
            //byte[] command = { Convert.ToByte(formCast.utfToAscii("F")), Convert.ToByte(formCast.utfToAscii("0")) };
            byte[] command = { 0x46, 0x30 };
            IEnumerable<byte> encoded = COBS.Encode(command);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }
        }

        private void refreshChart()
        {
            while(true)
            {
                byte[] command = { 0x45, 0x4F }; // Pid error for steering
                IEnumerable<byte> encoded = COBS.Encode(command);

                if (serialPort != null)
                {
                    serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
                }

                List<byte> paramByteArr = readFromSerial(10).ToList<byte>();
                ConstantChangesChart.errorValue = formCast.arrayToFloat(paramByteArr, 0);
                ConstantChangesChart.outputValue = formCast.arrayToFloat(paramByteArr, 4);
                Thread.Sleep(200);
            }

        }

        public IEnumerable<byte> readFromSerial (int nrOfBytes)
        {
            byte[] byteArr = new byte[nrOfBytes];
            int readCount;
            int offset = 0;

            while (nrOfBytes > 0 && (readCount = serialPort.Read(byteArr, offset, nrOfBytes)) > 0)
            {
                offset += readCount;
                nrOfBytes -= readCount;
            }

            //serialPort.Read(byteArr, offset, nrOfBytes);
            //IEnumerable<byte> decodedByteArr = COBS.Decode(byteArr.AsEnumerable<byte>());
            //return decodedByteArr;
            return COBS.Decode(byteArr.AsEnumerable<byte>());
        }
        

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
           // cartesianChart.Series[0].Values.;

            if (comPortList.SelectedItem != null && baudRateList.SelectedItem != null)
            {
                string portName = (comPortList.SelectedItem as ComboBoxItem).Tag.ToString();
                int baudRate = Int32.Parse((string)(baudRateList.SelectedItem as ComboBoxItem).Tag);
                if (serialPort != null)
                {
                    serialPort.Close();
                    serialPort = null; 
                }
                serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
                //serialPort.NewLine = ";";
                if (serialPort.IsOpen == false)
                {
                    serialPort.Open();
                    connectBtn.IsEnabled = false;
                    disconnectBtn.IsEnabled = true;
                    fetchBtn.IsEnabled = true;
                }               
            }

        }

        private void populateParameters ()
        {

            List<byte> paramByteArr = readFromSerial(50).ToList<byte>();
            speedBox.Text = formCast.arrayToFloat(paramByteArr, 0).ToString();
            motorP.Text = formCast.arrayToFloat(paramByteArr, 4).ToString();
            motorI.Text = formCast.arrayToFloat(paramByteArr, 8).ToString();
            motorD.Text = formCast.arrayToFloat(paramByteArr, 12).ToString();
            lpfBoxMotor.Text = formCast.arrayToFloat(paramByteArr, 16).ToString();
            steeringP.Text = formCast.arrayToFloat(paramByteArr, 20).ToString();
            steeringI.Text = formCast.arrayToFloat(paramByteArr, 24).ToString();
            steeringD.Text = formCast.arrayToFloat(paramByteArr, 28).ToString();
            lpfBoxSteering.Text = formCast.arrayToFloat(paramByteArr, 32).ToString();
            ffBox.Text = formCast.arrayToFloat(paramByteArr, 36).ToString();
            distBox.Text = formCast.arrayToFloat(paramByteArr, 40).ToString();
            timeBox.Text = formCast.arrayToFloat(paramByteArr, 44).ToString();




        }


        private void tuneMotor_Click(object sender, RoutedEventArgs e)
        {
            //byte[] command = { Convert.ToByte(formCast.utfToAscii("T")), Convert.ToByte(formCast.utfToAscii("M")) };
            byte[] command = { 0x54, 0x4D };
            byte[] P = formCast.stringToFloatByteArray(motorP.Text);
            byte[] I = formCast.stringToFloatByteArray(motorI.Text);
            byte[] D = formCast.stringToFloatByteArray(motorD.Text);

            byte[][] matrix = { command, P, I, D };
            byte[] combined = formCast.combineByteArrs(matrix);
            IEnumerable<byte> encoded = COBS.Encode(combined);
            
            if (serialPort != null)
            { 
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }
        }

        private void tuneSteering_Click(object sender, RoutedEventArgs e)
        {
            //  byte[] command = { Convert.ToByte(formCast.utfToAscii("T")), Convert.ToByte(formCast.utfToAscii("S")) };
            byte[] command = { 0x54, 0x53 };
            byte[] P = formCast.stringToFloatByteArray(steeringP.Text);
            byte[] I = formCast.stringToFloatByteArray(steeringI.Text);
            byte[] D = formCast.stringToFloatByteArray(steeringD.Text);

            byte[][] matrix = { command, P, I, D };
            byte[] combined = formCast.combineByteArrs(matrix);
            IEnumerable<byte> encoded = COBS.Encode(combined);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }
           
        }

        private void change_Speed_Btn_Click(object sender, RoutedEventArgs e)
        {
            //byte[] command = { Convert.ToByte(formCast.utfToAscii("C")), Convert.ToByte(formCast.utfToAscii("V")) };
            byte[] command = { 0x43, 0x56 };
            byte[] speed = formCast.stringToFloatByteArray(speedBox.Text);

            byte[][] matrix = { command, speed};
            byte[] combined = formCast.combineByteArrs(matrix);
            IEnumerable<byte> encoded = COBS.Encode(combined);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }
        }

        private void fetchBtn_Click(object sender, RoutedEventArgs e)
        {
            getParameters();
            /*  if (serialThread == null || (serialThread != null && !serialThread.IsAlive))
              {
                  serialThread = new Thread(new ThreadStart(populateParameters));
              } */
            populateParameters();

        }

        private void disconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if(serialPort.IsOpen)
            {
                serialPort.Close();
                connectBtn.IsEnabled = true;
                fetchBtn.IsEnabled = false;
                disconnectBtn.IsEnabled = false;
            }
        }

        private void setLPFButtonSteering_Click(object sender, RoutedEventArgs e)
        {
            //byte[] command = { Convert.ToByte(formCast.utfToAscii("L")), Convert.ToByte(formCast.utfToAscii("S")) };
            byte[] command = { 0x4C, 0x53 };
            byte[] lpf = formCast.stringToFloatByteArray(lpfBoxSteering.Text);

            byte[][] matrix = { command, lpf };
            byte[] combined = formCast.combineByteArrs(matrix);
            IEnumerable<byte> encoded = COBS.Encode(combined);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }

        }

        private void setLPFButtonMotor_Click(object sender, RoutedEventArgs e)
        {
            //byte[] command = { Convert.ToByte(formCast.utfToAscii("L")), Convert.ToByte(formCast.utfToAscii("M")) };
            byte[] command = { 0x4C, 0x4D };
            byte[] lpf = formCast.stringToFloatByteArray(lpfBoxMotor.Text);

            byte[][] matrix = { command, lpf };
            byte[] combined = formCast.combineByteArrs(matrix);
            IEnumerable<byte> encoded = COBS.Encode(combined);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            //byte[] command = { Convert.ToByte(formCast.utfToAscii("S")), Convert.ToByte(formCast.utfToAscii("1")) };
            byte[] command = { 0x53, 0x31 };
            IEnumerable<byte> encoded = COBS.Encode(command);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            //byte[] command = { Convert.ToByte(formCast.utfToAscii("S")), Convert.ToByte(formCast.utfToAscii("0")) };
            byte[] command = { 0x53, 0x30 };
            IEnumerable<byte> encoded = COBS.Encode(command);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }
        }

        private void readBtn_Click(object sender, RoutedEventArgs e)
        {
            if (threadStarted == 0)
            {
                Task.Factory.StartNew(refreshChart);
                threadStarted = 1;
            }

        }

        private void ffBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] command = { 0x46, 0x46 };
            byte[] ff = formCast.stringToFloatByteArray(ffBox.Text);

            byte[][] matrix = { command, ff };
            byte[] combined = formCast.combineByteArrs(matrix);
            IEnumerable<byte> encoded = COBS.Encode(combined);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }
        }

        private void distBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] command = { 0x4F, 0x44 };
            byte[] dist = formCast.stringToFloatByteArray(distBox.Text);

            byte[][] matrix = { command, dist };
            byte[] combined = formCast.combineByteArrs(matrix);
            IEnumerable<byte> encoded = COBS.Encode(combined);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }

        }

        private void timeBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] command = { 0x4F, 0x54 };
            byte[] time = formCast.stringToFloatByteArray(timeBox.Text);

            byte[][] matrix = { command, time };
            byte[] combined = formCast.combineByteArrs(matrix);
            IEnumerable<byte> encoded = COBS.Encode(combined);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }

        }

        private void oaBtn_Click(object sender, RoutedEventArgs e)
        {
            string onTgl = "1";
            byte[] command = { 0x54, 0x4F };
            byte[] dist = formCast.stringToFloatByteArray(onTgl);

            byte[][] matrix = { command, dist };
            byte[] combined = formCast.combineByteArrs(matrix);
            IEnumerable<byte> encoded = COBS.Encode(command);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }
        }

        private void oaOffBtn_Click(object sender, RoutedEventArgs e)
        {
            string offTgl = "0";
            byte[] command = { 0x54, 0x4F };
            byte[] dist = formCast.stringToFloatByteArray(offTgl);

            byte[][] matrix = { command, dist };
            byte[] combined = formCast.combineByteArrs(matrix);
            IEnumerable<byte> encoded = COBS.Encode(command);

            if (serialPort != null)
            {
                serialPort.Write(encoded.ToArray(), 0, encoded.ToArray().Length);
            }
        }

    }
    }

}