using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Security.Policy;

namespace arduino_awaiter
{
    public partial class MyForm : Form
    {
        public MyForm()
        {
            InitializeComponent();
            ArduinoComms = new ArduinoComms();
            ArduinoComms.Log += (sender, e) =>
            {
                richTextBox.AppendText($@"{DateTime.Now:hh\:mm\:ss\.ffff}: {e.Message}{Environment.NewLine}");
                richTextBox.SelectionStart = richTextBox.Text.Length;
                richTextBox.ScrollToCaret();
            };
            buttonHome.Click += async (sender, e) =>
            {
                UseWaitCursor = true;
                buttonHome.Enabled = false;
                await ArduinoComms.Home();
                buttonHome.Enabled = true;
                UseWaitCursor = false;
                // Cursor may need to be "nudged" to redraw
                Cursor.Position = new Point(Cursor.Position.X + 1, 0);
            };
        }
        ArduinoComms ArduinoComms { get; }
    }
    public class ArduinoComms
    {
        #region S I M
        class MockSerialPort
        {
            public MockSerialPort(byte[] simBuffer) => SimBuffer = simBuffer;
            public byte[] SimBuffer { get; }
        };
        #endregion S I M

        public ArduinoComms()
        {
            Port.DataReceived += Port_DataReceived;
        }

        public SerialPort Port = new SerialPort(/*parameters here*/); //creates and instances an internal serial port.

        public SemaphoreSlim XDone = new SemaphoreSlim(1, 1);
        public SemaphoreSlim YDone = new SemaphoreSlim(1, 1);
        public SemaphoreSlim Homed = new SemaphoreSlim(1, 1);
        public SemaphoreSlim Ready = new SemaphoreSlim(1, 1);
        public SemaphoreSlim Stopped = new SemaphoreSlim(1, 1);
        public SemaphoreSlim Locked = new SemaphoreSlim(1, 1);

        string NewDataContent = "Default newDataContent - should be inaccessible. If you see this, an error has occurred.";

        public async Task Home()
        {
            Logger($"Beginning home");
            try
            {
                // Await for any previous calls to clear
                await Homed.WaitAsync(timeout: TimeSpan.FromSeconds(10));
                // Send command to arduino as Fire and Forget.
                _ = MockWriteMyCommand(2);
                await Homed.WaitAsync();
            }
            finally
            {
                Homed.Release();
            }
            Logger("finished home, beginning backoff");
            try
            {
                await XDone.WaitAsync(timeout: TimeSpan.FromSeconds(10));
                _ = MockWriteMyCommand(0, backoff: true);
                await XDone.WaitAsync();
            }
            finally
            {
                XDone.Release();
            }
            try
            {
                await YDone.WaitAsync(timeout: TimeSpan.FromSeconds(10));
                _ = MockWriteMyCommand(1, backoff: true);
                await YDone.WaitAsync();
            }
            finally
            {
                YDone.Release();
            }
            Logger($"Finished home{Environment.NewLine}");
        }
        Random _rando = new Random(Seed: 1); // Seed is for repeatability during testing
        private async Task MockWriteMyCommand(int cmd, bool? backoff = null)
        {
            switch (cmd)
            {
                case 0:
                    await Task.Delay(TimeSpan.FromSeconds(_rando.Next(1, 4)));
                    Port_DataReceived(new MockSerialPort(System.Text.Encoding.ASCII.GetBytes($"XDone backoff={backoff}")), default);
                    break;
                case 1:
                    await Task.Delay(TimeSpan.FromSeconds(_rando.Next(1, 4)));
                    Port_DataReceived(new MockSerialPort(System.Text.Encoding.ASCII.GetBytes($"YDone backoff={backoff}")), default);
                    break;
                case 2:
                    await Task.Delay(TimeSpan.FromSeconds(_rando.Next(1, 4)));
                    Port_DataReceived(new MockSerialPort(System.Text.Encoding.ASCII.GetBytes("Home done")), default);
                    break;
                default:
                    Debug.Fail("Unrecognized command");
                    break;
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buf;
            switch (sender?.GetType().Name)
            {
                case nameof(SerialPort):
                    var spL = (SerialPort)sender;
                    buf = new byte[spL.BytesToRead]; //instantiates a buffer of appropriate length.
                    spL.Read(buf, 0, buf.Length); //reads from the sender, which inherits the data from the sender, which *is* our serial port.
                    break;
                case nameof(MockSerialPort):
                    var mspL = (MockSerialPort)sender;
                    buf = mspL.SimBuffer;
                    break;
                default: throw new NotImplementedException();
            }

            NewDataContent = $"{System.Text.Encoding.ASCII.GetString(buf)}"; //assembles the byte array into a string.
            Logger($"Received: {NewDataContent}"); //prints the result for debug.
            string[] thingsToParse = NewDataContent.Split('\n'); //splits the string into an array along the newline in case multiple responses are sent in the same message.

            foreach (string thing in thingsToParse) //checks each newline instance individually.
            {
                switch (thing)
                {
                    case string c when c.Contains("Home done"): //checks incoming data for the arduino's report phrase "Home done" when it is homed.
                        Homed.Release();
                        Logger($"Homed");
                        break;
                    case string c when c.Contains("XDone"):
                        XDone.Release();
                        Logger($"XDone");
                        break;
                    case string c when c.Contains("YDone"):
                        YDone.Release();
                        Logger($"YDone");
                        break;

                    default: break; //do nothing
                }
            }
        }
        public event EventHandler<LoggerMessageArgs> Log;
        public void Logger(string message) => Log?.Invoke(this, new LoggerMessageArgs(message));
    }

    public class LoggerMessageArgs
    {
        public LoggerMessageArgs(string message) => Message = message;

        public string Message { get; }
    }
}
