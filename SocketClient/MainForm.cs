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

namespace SocketClient
{
    public partial class MainForm : Form
    {
        public Socket ClientSocket
        {
            get;
            set;
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //客户端连接服务器端
            //创建socket对象
            Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            ClientSocket = socket;

            //2连接服务器端
            try
            { 
                socket.Connect(IPAddress.Parse(txtIPAddress.Text), int.Parse(txtPort.Text));
                AppendTextToLog("成功连接服务器端");
            }
            catch(Exception ex)
            {
                MessageBox.Show("在下输了，稍等");
                Thread.Sleep(500);
                btnStart_Click(this, e);
                return;
            }

            //3发送消息 接受消息
            Thread thread = new Thread(new ParameterizedThreadStart(ReceiveClientMsg));
            thread.IsBackground = true;
            thread.Start(ClientSocket);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if(ClientSocket.Connected)
            {
                byte[] data = Encoding.Default.GetBytes(txtMsg.Text);
                ClientSocket.Send(data, 0, data.Length, SocketFlags.None);
            }
        }

        #region 接受数据
        private void ReceiveClientMsg(object state)
        {
            var proxSocket = (Socket)state;
            byte[] data = new byte[1024 * 1024];
            while (true)
            {
                int realLen = 0;
                try
                {
                    realLen = proxSocket.Receive(data, 0, data.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    //异常退出
                    AppendTextToLog("服务器端" + proxSocket.RemoteEndPoint.ToString() + "异常退出");
                    StopConnect();//停止连接
                }
                if (realLen <= 0)
                {
                    //正常退出
                    AppendTextToLog(string.Format("接受到服务器端 {0} 正常退出", proxSocket.RemoteEndPoint.ToString()));
                    StopConnect();//停止连接
                    return;//终结当前异步线程
                }

                //接受带协议的数据，1：字符串 2：文件   3：闪屏
                if( data[0] == 1 )
                {
                    string strMsg =  ProcessReceivestring(data);
                    AppendTextToLog(string.Format("接受到服务器端 {0} 的消息：{1}", proxSocket.RemoteEndPoint.ToString(), strMsg));
                }
                
            }
        }
        #endregion 

        private void StopConnect()
        {
            try
            {
                if (ClientSocket.Connected)
                {
                    ClientSocket.Shutdown(SocketShutdown.Both);
                    ClientSocket.Close(100);
                }
            }
            catch(Exception ex)
            {

            }
        }

        public void AppendTextToLog(string txt)
        {
            //跨线程访问
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(s =>
                {
                    this.txtLog.Text = string.Format("{0}\r\n", s) + this.txtLog.Text;
                }), txt);
            }
            else
            {
                this.txtLog.Text = string.Format("{0}\r\n", txt) + this.txtLog.Text;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopConnect();
        }

        #region 处理接受的字符串
        public string ProcessReceivestring(byte[] data)
        {
            //拿到实际的字符串
            string str = Encoding.Default.GetString(data, 1, data.Length - 1);
            return str;
            //把接受的数据放到文本框上
            
        }
        #endregion 
    }
}
