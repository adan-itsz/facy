using System;
using System.Threading;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;
using Windows.Media.Capture;
using Windows.System.Display;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Media.MediaProperties;

namespace facy.UWP
{
    public sealed partial class MainPage
    {
        private MediaCapture _mediaCapture;
        bool isPreviewing;
        bool ban = false;
        DisplayRequest displayRequest = new DisplayRequest();
        private SerialDevice serialPort = null;
        DataReader dataReaderObject = null;
        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;
        private DeviceInformation _cameraDevice;
        private FaceDetectionEffect _faceDetectionEffect;


        public MainPage()
        {
            this.InitializeComponent();
            listOfDevices = new ObservableCollection<DeviceInformation>();
           // LoadApplication(new facy.App());
          // ListAvailablePorts();
           Application.Current.Resuming += Application_Resuming;

        }

        private async void ListAvailablePorts()
        {
            try
            {
               // var dialog = new MessageDialog(valor.ToString());
                
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                for(int i = 0; i < dis.Count; i++)
                {
                   
                    listOfDevices.Add(dis[i]);
                }
                SerialPortConfiguration();

            }
            catch(Exception ex)
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
                Listen();
            }
            catch(Exception ex)
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

        private async Task InitializeCameraAsync()
        {
            if (_cameraDevice == null)
            {
                // Get the camera devices
                var cameraDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                // try to get the back facing device for a phone
                var backFacingDevice = cameraDevices
                    .FirstOrDefault(c => c.EnclosureLocation != null && c.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);

                var preferredDevice = backFacingDevice ?? cameraDevices.FirstOrDefault();
                _cameraDevice = preferredDevice;

                // Create MediaCapture
                _mediaCapture = new MediaCapture();
                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = _cameraDevice.Id };

                
                await _mediaCapture.InitializeAsync(settings);

                // Set the preview source for the CaptureElement
                PreviewControl.Source = _mediaCapture;

                // Start viewing through the CaptureElement 
                await _mediaCapture.StartPreviewAsync();

                await monitoreoDeCamara(_cameraDevice);

                

            }
        }

        private async Task monitoreoDeCamara(DeviceInformation cameraDevice)
        {
            var definition = new FaceDetectionEffectDefinition();
            definition.SynchronousDetectionEnabled = false;
            definition.DetectionMode = FaceDetectionMode.HighPerformance;

            _faceDetectionEffect = (await _mediaCapture.AddVideoEffectAsync(definition, MediaStreamType.VideoPreview)) as FaceDetectionEffect;
            _faceDetectionEffect.FaceDetected += FaceDetectionEffect_FaceDetected;

            _faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(100);
            _faceDetectionEffect.Enabled = true;

        }
        private async void FaceDetectionEffect_FaceDetected(FaceDetectionEffect sender, FaceDetectedEventArgs args)
        {
           if (args.ResultFrame.DetectedFaces.Count > 0)
            {
                try
                {
                    ban = true;
                    //aqui se tomara la foto
                    var storageFolder = KnownFolders.SavedPictures;

                    // Create the file that we're going to save the photo to.
                    var file = await storageFolder.CreateFileAsync("sample.jpg", CreationCollisionOption.ReplaceExisting);

                    // Update the file with the contents of the photograph.
                    await _mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
                }catch(Exception ex)
                {

                }
            
            }

        }

        private async void Application_Resuming(object sender, object o)
        {
            await InitializeCameraAsync();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeCameraAsync();
        }
    
        private async Task ReadData(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;
            uint ReadBufferLength = 1024;
            cancellationToken.ThrowIfCancellationRequested();
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);
            UInt32 bytesRead = await loadAsyncTask;
            string valor="";
            
            if (bytesRead > 0)
            {
                valor = dataReaderObject.ReadString(bytesRead); //lee lo enviado por el serial port
            }
            if (valor == "#\r\n")
            {
                //   var dialog = new MessageDialog(valor.ToString());
                //   await dialog.ShowAsync();

                //llama a buscar rostro en la camara
               // await monitoreoDeCamara(_cameraDevice);
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
