using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatDemo
{
    public partial class MainForm : Form
    {
        public List<Socket> ClientSockets = new List<Socket>();
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //创建socket对象
            AppendTextToLog("创建服务器端");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //绑定端口和IP
            IPEndPoint point = new IPEndPoint(IPAddress.Parse(txtIPAddress.Text), int.Parse(txtPort.Text));
            socket.Bind(point);

            //开启侦听    
            socket.Listen(10);//等待连接的队列

            //接受客户端连接
            ThreadPool.QueueUserWorkItem(new WaitCallback(AcceptClientConnect), socket);

        }

        #region 发送字符串
        // 发送消息        
        private void btnSend_Click(object sender, EventArgs e)
        {
            foreach (var client in ClientSockets)
            {
                if (client.Connected)
                {
                    //原始字符串
                    byte[] data = Encoding.Default.GetBytes(txtMsg.Text);

                    //对原始数据加上协议的头部字节
                    byte[] result = new byte[data.Length + 1];
                    //设置当前协议的头部字节是1 : 1代表字符串 
                    result[0] = 1;
                    //把原始数据放到最终的字节数据中
                    Buffer.BlockCopy(data, 0, result, 1, data.Length);
                    client.Send(result, 0, result.Length, SocketFlags.None);
                }
            }
        }
        #endregion   
      
        #region 发送闪屏
        private void btnSendAction_Click(object sender, EventArgs e)
        {
            foreach(var proxSocket in ClientSockets)
            {
                if(proxSocket.Connected)
                {
                    proxSocket.Send(new byte[]{2},SocketFlags.None);
                }
            }
        }
        #endregion 

        #region 发送文件
        private void btnSendFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                byte[] data = File.ReadAllBytes(ofd.FileName);
                byte[] result = new byte[data.Length + 1];
                result[0] = 3;
                Buffer.BlockCopy(data, 0, result,1,data.Length);
                foreach (var proxSocket in ClientSockets)
                {
                    if (!proxSocket.Connected)
                    {
                        continue;
                    }
                    proxSocket.Send(result, SocketFlags.None);
                }
            }            
        }
        #endregion 

        private void AcceptClientConnect(object state)
        {
            var socket = (Socket)state;
            AppendTextToLog("客户端开始连接");
            while (true)
            {
                Socket proxSocket = socket.Accept();
                AppendTextToLog("客户端" + proxSocket.RemoteEndPoint.ToString() + "连接上");
                ClientSockets.Add(proxSocket);

                //不停接受当前连接用户的消息
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveClientMsg),proxSocket);

            }

        }

        //接受客户端消息
        private void ReceiveClientMsg(object state)
        {
            var proxSocket = (Socket)state;
            byte[] data = new byte[1024*1024];
            while(true)
            {
                int realLen = 0 ;
                try
                {
                    realLen = proxSocket.Receive(data, 0, data.Length, SocketFlags.None);
                }
                catch(Exception ex)
                {
                    //异常退出
                    AppendTextToLog("客户端" + proxSocket.RemoteEndPoint.ToString() + "异常退出");
                    StopConnect(proxSocket);//停止连接
                }
                

                if (realLen <= 0)
                {
                    //正常退出
                    AppendTextToLog(string.Format("接受到客户端 {0} 正常退出", proxSocket.RemoteEndPoint.ToString()));
                    ClientSockets.Remove(proxSocket);
                    StopConnect(proxSocket);//停止连接
                    return;//终结当前异步线程
                }
                string str = Encoding.Default.GetString(data, 0, realLen);
                //把接受的数据放到文本框上
                AppendTextToLog(string.Format("接受到客户端 {0} 的消息：{1}",proxSocket.RemoteEndPoint.ToString(),str));
            }
        }

        private void StopConnect(Socket proxSocket)
        {
            try
            {
                if (proxSocket.Connected)
                {
                    proxSocket.Shutdown(SocketShutdown.Both);
                    proxSocket.Close(100);
                }
            }
            catch (Exception ex)
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

        
    }
}
