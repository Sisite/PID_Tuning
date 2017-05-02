﻿using System;
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

namespace PIDTuner1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort serialPort;
        Thread serialThread;
        //Encoding asciiEncoding = Encoding.ASCII;
        bool _continue = false;
        public MainWindow()
        {    
            InitializeComponent();
            getComPorts();

            
        }

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

            return Encoding.ASCII.GetString(uni);
        }

        private string asciiToUtf (string str)
        {
            byte[] ascii = Encoding.ASCII.GetBytes(str);

            return Encoding.Unicode.GetString(ascii);
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
            
            while (_continue) {
                
                string message = serialPort.ReadLine();
                return asciiToUtf(message);
            }
            return null;
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
                }
                _continue = true;
                getParameters();
                if (serialThread == null || (serialThread != null && !serialThread.IsAlive))
                {
                    serialThread = new Thread(new ThreadStart(populateParameters));
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
            steeringP.Text = splitString[4];
            steeringI.Text = splitString[5];
            steeringD.Text = splitString[6];

        }

        private void tuneMotor_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            { 
                string str = (utfToAscii("TM:" + motorP.Text + ":" + motorI.Text + ":" + motorP.Text + ":;"));
                serialPort.Write(str);
            }
        }

        private void tuneSteering_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                string str = (utfToAscii("TS:" + steeringP.Text + ":" + steeringI.Text + ":" + steeringP.Text + ":;"));
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
    }
}
