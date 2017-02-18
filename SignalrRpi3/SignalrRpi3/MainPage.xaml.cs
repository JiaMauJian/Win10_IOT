using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.AspNet.SignalR.Client;
using System.Threading;
using System.Diagnostics;

using Windows.Devices.Gpio;

using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.ApplicationModel;

//空白頁項目範本收錄在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SignalrRpi3
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IHubProxy _hubProxy;
        private HubConnection _HubConnection;
        private SynchronizationContext _currentContext;

        private const int LED_PIN = 5;
        private GpioPin _pin;
        private GpioPinValue _pinValue;

        private const string SRGS_FILE = "Grammar\\grammar.xml";
        private SpeechRecognizer _recognizer;
        private const string TAG_CMD = "cmd";

        public MainPage()
        {
            this.InitializeComponent();

            _HubConnection = new HubConnection("http://10.1.53.218/signalrsvr/");
            _hubProxy = _HubConnection.CreateHubProxy("MMSHub");
            _HubConnection.Start().Wait();

            _currentContext = SynchronizationContext.Current;
            _hubProxy.On<string>("showServerVersion", (version) =>
                _currentContext.Post(delegate
                {
                    textBox.Text = string.Format("VER.{0}", version);
                    _pinValue = GpioPinValue.Low;
                    _pin.Write(_pinValue);
                }, null)
            );

            InitGPIO();

            InitSpeechRecognizer();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            CallServerFunction("GetServerVersion");
        }

        private void CallServerFunction(string method, params object[] args)
        {
            try
            {
                if (_HubConnection.State == ConnectionState.Connected)
                {
                    _hubProxy.Invoke(method, args).Wait();
                }                    
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                _pin = null;
                return;
            }

            _pin = gpio.OpenPin(LED_PIN);
            _pinValue = GpioPinValue.High;
            _pin.Write(_pinValue);
            _pin.SetDriveMode(GpioPinDriveMode.Output);
        }

        private async void InitSpeechRecognizer()
        {
            // Initialize recognizer
            _recognizer = new SpeechRecognizer();

            // Set event handlers
            _recognizer.StateChanged += RecognizerStateChanged;
            _recognizer.ContinuousRecognitionSession.ResultGenerated += RecognizerResultGenerated;

            // Load Grammer file constraint
            string fileName = String.Format(SRGS_FILE);
            StorageFile grammarContentFile = await Package.Current.InstalledLocation.GetFileAsync(fileName);

            SpeechRecognitionGrammarFileConstraint grammarConstraint = new SpeechRecognitionGrammarFileConstraint(grammarContentFile);

            // Add to grammer constraint
            _recognizer.Constraints.Add(grammarConstraint);

            // Compile grammer
            SpeechRecognitionCompilationResult compilationResult = await _recognizer.CompileConstraintsAsync();

            Debug.WriteLine("Status: " + compilationResult.Status.ToString());

            // If successful, display the recognition result.
            if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
            {
                Debug.WriteLine("Result: " + compilationResult.ToString());

                await _recognizer.ContinuousRecognitionSession.StartAsync();
            }
            else
            {
                Debug.WriteLine("Status: " + compilationResult.Status);
            }
        }

        private void RecognizerResultGenerated(SpeechContinuousRecognitionSession session, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // Output debug strings
            Debug.WriteLine(args.Result.Status);
            Debug.WriteLine(args.Result.Text);

            if (args.Result.Text == "check the server version")
            {
                CallServerFunction("GetServerVersion");
            }
                

        }

        private void RecognizerStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {           
            Debug.WriteLine("Speech recognizer state: " + args.State.ToString());
        }

    }
}
