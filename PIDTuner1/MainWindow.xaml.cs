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

namespace PIDTuner1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort serialPort;
        Encoding ascii = Encoding.ASCII;
        Encoding unicode = Encoding.Unicode;
        public MainWindow()
        {
            InitializeComponent();
            getComPorts();
            disconnectBtn.IsEnabled = false;
            fetchBtn.IsEnabled = false;
            //pidSteeringCollection;
            SeriesCollection SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 4, 6, 5, 2 ,4 }
                },
                new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> { 6, 7, 3, 4 ,6 },
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Series 3",
                    Values = new ChartValues<double> { 4,2,7,2,7 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }
            };

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            YFormatter = value => value.ToString("C");

            //modifying the series collection will animate and update the chart
            SeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });

            //modifying any series values will also animate and update the chart
            SeriesCollection[3].Values.Add(5d);

            DataContext = this;


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
        
        private string utfToAscii (string str)
        {
            byte[] uni = Encoding.Unicode.GetBytes(str);

            // Convert to ASCII
            byte[] asciiB = Encoding.Convert(unicode, ascii, uni);
            char[] asciiC = new char[ascii.GetCharCount(asciiB, 0, asciiB.Length)];
            ascii.GetChars(asciiB, 0, asciiB.Length, asciiC, 0);
            string asciiS = new string(asciiC);
            Console.WriteLine(asciiS);

            return asciiS;
        }

        private string asciiToUtf (string str)
        {
            
            byte[] asc = Encoding.ASCII.GetBytes(str);
            byte[] uniB = Encoding.Convert(ascii, unicode, asc);

            char[] uniC = new char[unicode.GetCharCount(uniB, 0, uniB.Length)];
            unicode.GetChars(uniB, 0, uniB.Length, uniC, 0);
            string uniS = new string(uniC);

            Console.WriteLine(uniS);


            return uniS;
        }


        //Communication example T:M:4000.0:4000.0:4000.0:;
        //                   MODE:P:I:D:Endstring;   
        //Communication example with Steering T:M:1.0:2.5:3.5:S:0.5:0.3:4.3:;

        private void getParameters() {
            if (serialPort.IsOpen) {

                serialPort.Write(utfToAscii("F0;"));
            }
        }

        private String readFromSerial ()
        {          
            string message = serialPort.ReadLine();
            //Console.WriteLine(message);
            return asciiToUtf(message);
        }
        

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
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
                serialPort.NewLine = ";" ;
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
            string paramString = readFromSerial();
            string[] splitString;
            splitString = paramString.Split(':');
            speedBox.Text = splitString[0];
            motorP.Text = splitString[1];
            motorI.Text = splitString[2];
            motorD.Text = splitString[3];
            lpfBoxMotor.Text = splitString[4];
            steeringP.Text = splitString[5];
            steeringI.Text = splitString[6];
            steeringD.Text = splitString[7];
            lpfBoxSteering.Text = splitString[8];
            

        }

        private void tuneMotor_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            { 
                string str = (utfToAscii("TM:" + motorP.Text + ":" + motorI.Text + ":" + motorD.Text + ":;"));
                serialPort.Write(str);
            }
        }

        private void tuneSteering_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                string str = (utfToAscii("TS:" + steeringP.Text + ":" + steeringI.Text + ":" + steeringD.Text + ":;"));
                serialPort.Write(str);
            }
        }

        private void change_Speed_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                string str = (utfToAscii("CV:" + speedBox.Text + ":;"));
                serialPort.Write(str);
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
            if (serialPort != null)
            {
                string str = (utfToAscii("LS:" + lpfBoxSteering.Text + ":;"));
                serialPort.Write(str);
            }
        }

        private void setLPFButtonMotor_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                string str = (utfToAscii("LM:" + lpfBoxMotor.Text + ":;"));
                serialPort.Write(str);
            }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                string str = (utfToAscii("S1:;"));
                serialPort.Write(str);
            }
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                string str = (utfToAscii("S0:;"));
                serialPort.Write(str);
            }
        }
    }

}