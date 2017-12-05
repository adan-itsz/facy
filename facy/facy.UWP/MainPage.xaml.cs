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
using Windows.Media.Capture;
using Windows.System.Display;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Media.MediaProperties;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using Microsoft.ProjectOxford.Common.Contract;
using Windows.UI.Popups;

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
        string path = @"C:\Users\Adán\Pictures\sample.jpg";
        static string subscriptionKey = "a53f005c45b84adba817bffacf34fe54";
        bool bandera = false;
        string variableCondicion = "#\r\n";



        public MainPage()
        {
            this.InitializeComponent();
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
           
            

        }
        private readonly IFaceServiceClient _faceServiceClient
               = new FaceServiceClient(subscriptionKey, "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");

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
                // obtiene las camaras disponibles
                var cameraDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

               
                var backFacingDevice = cameraDevices
                    .FirstOrDefault(c => c.EnclosureLocation != null && c.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);

                var preferredDevice = backFacingDevice ?? cameraDevices.FirstOrDefault();
                _cameraDevice = preferredDevice;

                // Crea MediaCapture
                _mediaCapture = new MediaCapture();
                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = _cameraDevice.Id };

                
                await _mediaCapture.InitializeAsync(settings);

                // envia fuente de video
                PreviewControl.Source = _mediaCapture;

                // comienza el viewer
                await _mediaCapture.StartPreviewAsync();

                // monitoreoDeCamara(_cameraDevice);

                

            }
        }

        private async void monitoreoDeCamara(DeviceInformation cameraDevice)
        {
             AppendMessage("en proceso");
             var definition = new FaceDetectionEffectDefinition();
            definition.SynchronousDetectionEnabled = false;
            definition.DetectionMode = FaceDetectionMode.HighPerformance;
            
                _faceDetectionEffect = (await _mediaCapture.AddVideoEffectAsync(definition, MediaStreamType.VideoPreview)) as FaceDetectionEffect;
             _faceDetectionEffect.FaceDetected +=  FaceDetectionEffect_FaceDetected;
      
            _faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(100);
                _faceDetectionEffect.Enabled = true;
            

        }
        private void AppendMessage(string message)
        {

            textResults.Text = $"{message}\r\n{textResults.Text}";
           // textResults.Text = $"{message}\r\n";
        }

        private async void FaceDetectionEffect_FaceDetected(FaceDetectionEffect sender, FaceDetectedEventArgs args)
        {
           
            if (args.ResultFrame.DetectedFaces.Count > 0)
            {

                try
                {
               //     _faceDetectionEffect.Enabled = false;
                    _faceDetectionEffect.FaceDetected -= FaceDetectionEffect_FaceDetected;
                    //aqui se tomara la foto
                    var storageFolder = KnownFolders.PicturesLibrary;
                   
                    // Crea el archivo de foto.
                    var file = await storageFolder.CreateFileAsync("sample.jpg", CreationCollisionOption.ReplaceExisting);

                    // actualiza archivo con la foto
                    await _mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
                    await MakeAnalysisRequest(path);
                   // _faceDetectionEffect.FaceDetected += FaceDetectionEffect_FaceDetected;

                }
                catch(Exception ex)
                {

                }
            
            }

        }
         async Task MakeAnalysisRequest(string imageFilePath)
        {           

            // convierte imagen a array de bytes y envia para su tratamiento.
            byte[] byteData = GetImageAsByteArray(imageFilePath);
            var faces = await DetectFaces(new MemoryStream(byteData));
            double edad=0.0;
            if (faces == null) {
            }
            else
            {
                var faceCount = 1;
                

                foreach (var face in faces)
                {
                    await crearObjeto(face.FaceAttributes.Age, face.FaceAttributes.Gender, face.FaceAttributes.Glasses, face.FaceAttributes.Emotion);
                  
                   
                }
                
                
            }
            await Task.Delay(8000);
            var messageDialog = new MessageDialog("No internet connection has been found.");
            await messageDialog.ShowAsync();
            //AppendMessage("terminado");
            variableCondicion = "#\r\n";
            SerialPortConfiguration();
        }
        private async Task crearObjeto(double age, string gender, Glasses glasses, EmotionScores emotion)
        {
            persona p;
            p = new persona(age, gender,glasses, emotion);

        }

        private async Task<Face[]> DetectFaces(Stream imageStream)
        {
            var attributes = new List<FaceAttributeType>();
            attributes.Add(FaceAttributeType.Age);
            attributes.Add(FaceAttributeType.Gender);
            attributes.Add(FaceAttributeType.Smile);
            attributes.Add(FaceAttributeType.Glasses);
            attributes.Add(FaceAttributeType.FacialHair);
            attributes.Add(FaceAttributeType.Emotion);
            Face[] faces = null;
            try
            {
                faces = await _faceServiceClient.DetectAsync(imageStream, true, true, attributes);
            }
            catch (FaceAPIException exception)
            {
               
            }
         
            return faces;
        }

        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }


        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            
                FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            
            
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
            if (valor == variableCondicion)
            {
                variableCondicion = "x";
                serialPort = null;
                //llama a buscar rostro en la camara
                monitoreoDeCamara(_cameraDevice);

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
