using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FTPOverSocket.Service
{
    class SocketService
    {
        private const int BUFFER_SIZE = 4096;
        private string address;
        private Socket socket;

        public SocketService(string address, int port)
        {
            this.address = address;
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(new IPEndPoint(IPAddress.Parse(address), port));
        }

        public string Check()
        {
            socket.Send(Encoding.UTF8.GetBytes("{\"action\":\"check\"}"));
            byte[] bytes = new byte[BUFFER_SIZE];
            int size = socket.Receive(bytes, bytes.Length, 0);
            string response = Encoding.UTF8.GetString(bytes, 0, size);
            return response;
        }

        public string[] Dir()
        {
            socket.Send(Encoding.UTF8.GetBytes("{\"action\":\"dir\"}"));
            byte[] bytes = new byte[BUFFER_SIZE];
            int size = socket.Receive(bytes, bytes.Length, 0);
            string response = Encoding.UTF8.GetString(bytes, 0, size);
            string[] names = response.Split('\n');
            return names;
        }

        public void Close()
        {
            socket.Send(Encoding.UTF8.GetBytes("{\"action\":\"quit\"}"));
            socket.Close();
        }

        public void Get(string filename)
        {
            socket.Send(Encoding.UTF8.GetBytes("{\"action\":\"get\",\"filename\":\"" + filename + "\"}"));
            if (File.Exists(@".\files\" + filename))
                File.Delete(@".\files\" + filename);
            using (FileStream fs = File.Create(@".\files\" + filename))
            {
                byte[] bytes = new byte[BUFFER_SIZE];
                while (true)
                {
                    int size = socket.Receive(bytes, bytes.Length, 0);
                    if (size == 0)
                    {
                        fs.Close();
                        break;
                    }
                    fs.Write(bytes, 0, size);
                    if (size < BUFFER_SIZE)
                    {
                        fs.Close();
                        break;
                    }
                }
            }
        }

        public void Upload(string filepath, string filename)
        {
            socket.Send(Encoding.UTF8.GetBytes("{\"action\":\"upload\",\"filename\":\"" + filename + "\"}"));
            using (FileStream fs = File.OpenRead(filepath))
            {
                byte[] bytes = new byte[BUFFER_SIZE];
                bool isFirst = true;
                while (true)
                {
                    int size = fs.Read(bytes, 0, BUFFER_SIZE);
                    if (isFirst)
                    {
                        if (size == 0)
                        {
                            this.socket.Send(Encoding.UTF8.GetBytes("EMPTYFILE"));
                            fs.Close();
                            break;
                        }
                        isFirst = false;
                    }
                    if (size == 0)
                    {
                        fs.Close();
                        this.socket.Send(Encoding.UTF8.GetBytes("DONE"));
                        break;
                    }
                    this.socket.Send(bytes);
                    if (size < BUFFER_SIZE)
                    {
                        fs.Close();
                        this.socket.Send(Encoding.UTF8.GetBytes("DONE"));
                        break;
                    }
                }
            }
        }
    }
}
