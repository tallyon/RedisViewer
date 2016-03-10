using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RedisViewer.LIB;

namespace RedisViewer
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow : Window
    {
        private RedisDB redis;

        public ConnectWindow()
        {
            InitializeComponent();
            redis = RedisDB.Instance;
        }

        private void btnConnectToServer(object sender, RoutedEventArgs e)
        {
            string hostname = textBoxHost.Text;
            int port;
            string password = textBoxPassword.Text;

            // Prepare hostname and port
            if (textBoxHost.Text.Split(':').Length > 1)
            {
                hostname = textBoxHost.Text.Split(':')[0];
                port = Convert.ToInt32(textBoxHost.Text.Split(':')[1]);
            }
            else
            {
                hostname = textBoxHost.Text;
                port = 6379;
            }

            // Show "Connecting..." label
            labelConnecting.Visibility = Visibility.Visible;

            // Try to connect
            if (redis.Connect(hostname, port) == false)
            {
                // If connection failed show dialog box with information
                UnableToConnectPopup unableToConnect = new UnableToConnectPopup();
                unableToConnect.popupInfo.Text = string.Format("Unable to connect to {0}:{1}!", hostname, port);
                unableToConnect.ShowDialog();
                // Hide "Connecting..." label
                labelConnecting.Visibility = Visibility.Hidden;
            }
            else
            {
                MainWindow main = new MainWindow();
                Application.Current.MainWindow = main;
                Close();
                main.Show();
            }
        }
    }
}
