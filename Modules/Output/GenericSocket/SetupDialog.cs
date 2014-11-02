using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace VixenModules.Output.GenericSocket
{
	public partial class SetupDialog : Form
	{
		private Data _data;

		public SetupDialog(Data data)
		{
			InitializeComponent();

			_data = data;

            if (data.Port > 0 && data.Port < 65535)
            {
                tbPort.Text = data.Port.ToString();
            }
            else
            {
                tbPort.Text = Data.DEFAULT_PORT.ToString();
            }

		}

		private void btnOkay_Click(object sender, EventArgs e)
		{
            int port;
			_data.Port = int.TryParse(tbPort.Text, out port) ? port : Data.DEFAULT_PORT;
		}
	}
}