using FTPOverSocket.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FTPOverSocket
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string address;
        private string port;
        private SocketService socket;
        public LoginWindow(string address, string port)
        {
            InitializeComponent();
            this.address = address;
            this.port = port;
            this.socket = SocketService.getInstance();
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = textboxUsername.Text;
            string password = passwordboxPassword.Password;
            bool isServer = this.socket.Connect(this.address, int.Parse(this.port));
            if (!isServer)
            {
                MessageBox.Show("Connection error!");
            }
            else
            {
                if (socket.Login(username, password))
                {
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Wrong username or password!");
                    this.socket.Close();
                }
            }
        }
    }
}
