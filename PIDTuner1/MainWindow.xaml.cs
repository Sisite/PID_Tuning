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
        Casting formCast = new Casting();
        public MainWindow()
        {
            InitializeComponent();
            
            getComPorts();
            disconnectBtn.IsEnabled = false;
            fetchBtn.IsEnabled = false;
            byte[] tmp = formCast.stringToArray("H");
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
           // ConstantChangesChart cw = new ConstantChangesChart();
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
            if (serialPort.IsOpen) {

                serialPort.Write(formCast.utfToAscii("F0;"));
            }
        }

        private String readFromSerial ()
        {          
            string message = serialPort.ReadLine();
            //Console.WriteLine(message);
            return formCast.asciiToUtf(message);
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
                string str = (formCast.utfToAscii("TM:" + motorP.Text + ":" + motorI.Text + ":" + motorD.Text + ":;"));
                serialPort.Write(str);
            }
        }

        private void tuneSteering_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                string str = (formCast.utfToAscii("TS:" + steeringP.Text + ":" + steeringI.Text + ":" + steeringD.Text + ":;"));
                serialPort.Write(str);
            }
        }

        private void change_Speed_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                string str = (formCast.utfToAscii("CV:" + speedBox.Text + ":;"));
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
                string str = (formCast.utfToAscii("LS:" + lpfBoxSteering.Text + ":;"));
                serialPort.Write(str);
            }
        }

        private void setLPFButtonMotor_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                string str = (formCast.utfToAscii("LM:" + lpfBoxMotor.Text + ":;"));
                serialPort.Write(str);
            }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                string str = (formCast.utfToAscii("S1:;"));
                serialPort.Write(str);
            }
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                string str = (formCast.utfToAscii("S0:;"));
                serialPort.Write(str);
            }
        }
    }

}