using System.IO.Ports;
using System.Text;
using Vixen.Commands;
using Vixen.Module;
using Vixen.Module.Controller;
using System.Timers;
using System;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using VixenModules.Output.GenericSocket;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace VixenModules.Output.GenericSocket
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

	public class Module : ControllerModuleInstanceBase
	{
		private Data _Data;
		private CommandHandler _commandHandler;
		private byte[] _packet;
		private byte[] _header;
		private byte[] _footer;
		private int headerLen = 0;
		private int footerLen = 0;
		private System.Timers.Timer _retryTimer;
		private int _retryCounter;
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();
        private AppServer _server;
        private AppSession _session;

        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

		public Module()
		{
			_commandHandler = new CommandHandler();
			DataPolicyFactory = new DataPolicyFactory();

			//set 2 minute timer before retrying to access com port
			_retryTimer = new System.Timers.Timer(120000);
			_retryTimer.Elapsed += new ElapsedEventHandler(_retryTimer_Elapsed);
			_retryCounter = 0;
		}

		public override void UpdateState(int chainIndex, ICommand[] outputStates)
		{

            if (_session != null && _session.Connected)
            {
                int channel = 0;
                foreach (ICommand os in outputStates) {
                    string val = "0";
                    if (os != null)
                    {
                        val = os.CommandValue.ToString();
                    }
                    _session.Send(String.Format("{0}:{1}", channel, val));
                    channel += 1;
                }
            }
		}

		public override bool HasSetup
		{
			get { return true; }
		}

		public override bool Setup()
		{
			using (SetupDialog setup = new SetupDialog(_Data)) {
				if (setup.ShowDialog() == DialogResult.OK) {
					initModule();
					return _Data.IsValid;
				}
			}

			return false;
		}

		public override IModuleDataModel ModuleData
		{
			get { return _Data; }
			set
			{
				_Data = (Data) value;
				initModule();
			}
		}

		public override void Start()
		{
			base.Start();
            _server.Start();
		}

		public override void Stop()
		{
            _server.Stop();
			base.Stop();
		}

		private void initModule()
		{
            dropExistingSocket();
            createSocketFromData();
		}

		private void dropExistingSocket()
		{
			if (_server != null) {
                _server.Stop();
			}

            _server = null;
		}

        private void createSocketFromData()
		{
            if (_Data.IsValid && _server == null)
            {
                _server = new AppServer();
                _server.Setup(_Data.Port);
                _server.NewSessionConnected += new SessionHandler<AppSession>(server_NewRequestReceived);
            }
		}

        private void server_NewRequestReceived(AppSession session)
        {
            _session = session;
        }

		public void _retryTimer_Elapsed(object source, ElapsedEventArgs e)
		{
			Logging.Info("Attempting to start controller.");
			Start();
		}
	}
}