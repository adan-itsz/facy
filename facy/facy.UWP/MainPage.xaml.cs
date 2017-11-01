using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Enumeration;

namespace facy.UWP
{
    public sealed partial class MainPage
    {
        private SerialDevice serialPort = null;
        DataReader dataReaderObject = null;
        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;


        public MainPage()
        {
            this.InitializeComponent();
            listOfDevices = new ObservableCollection<DeviceInformation>;
            LoadApplication(new facy.App());
            SerialPortConfiguration();
        }

        private async void ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                for(int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }
               

            }catch(Exception ex)
            {

            }
        }

        private async void SerialPortConfiguration()
        {
            DeviceInformation entry = (DeviceInformation)listOfDevices[0];
            try
            {
                serialPort = await SerialDevice.FromIdAsync(entry.Id);
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.BaudRate = 9600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;
                ReadCancellationTokenSource = new CancellationTokenSource();

            }catch(Exception ex)
            {

            }
        }

        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);
                    while (true)
                    {
                        await ReadData(ReadCancellationTokenSource.Token);
                    }
                }
            }catch(Exception ex)
            {

            }
        }

        private async Task ReadData(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;
            uint ReadBufferLength = 1024;
            cancellationToken.ThrowIfCancellationRequested();
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
               //leer puerto =dataReaderObject.ReadString(bytesRead)
            }
        }
        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
            listOfDevices.Clear();
        }

    }
}
