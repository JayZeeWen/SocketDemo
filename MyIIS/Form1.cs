using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyIIS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(txtIPAddress.Text), int.Parse(txtPort.Text)));
            socket.Listen(10);
            txtLog.Text = "服务启动成功\r\n" + txtLog.Text; 
            ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessRequest), socket);

        }
        #region 处理http请求
        private void ProcessRequest(object state)
        {
            Socket socket = state as Socket;
            while(true)
            {
                var proxSocket = socket.Accept();
                byte[] data = new byte[1024 * 1024 * 2];
                int realLen = proxSocket.Receive(data, 0, data.Length, SocketFlags.None);
                string requestText = Encoding.Default.GetString(data, 0, realLen);

                //解析   请求报文， 处理请求报文，返回相应内容
                //1抽象
                HttpContext context = new HttpContext(requestText);
                HttpApplication application = new HttpApplication();
                application.ProcessRequest(context);
                proxSocket.Send(context.Response.GetResponseHeader());
                proxSocket.Send(context.Response.Body);
                proxSocket.Shutdown(SocketShutdown.Both);
                proxSocket.Close();

            }
        }
        #endregion
    }
}
