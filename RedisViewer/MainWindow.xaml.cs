using System.Windows;
using System.Windows.Controls;
using RedisViewer.LIB;
using System.Text;
using System.Collections.Generic;

namespace RedisViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RedisDB redisDB;

        public MainWindow()
        {
            InitializeComponent();
            redisDB = RedisDB.Instance;
            Title = redisDB.GetConnectionInfo;

            // Populate TreeView with all the keys with their values
            treeView.ItemsSource = redisDB.GetKeysList();

            PrintKeys();
        }

        /// <summary>
        /// Handle Redis commands written by user in TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendCommandToRedis(object sender, RoutedEventArgs e)
        {
            string[] splitText = textBoxMessage.Text.Split(' ');
            string operation = splitText[0].ToLower();

            string key, value, response;
            bool success;
            // Determine what operation should be called based on the first word of the textbox
            switch (operation)
            {
                case "set":
                    // SET <key> <value>
                    if (splitText.Length == 3)
                    {
                        key = splitText[1];
                        value = splitText[2];
                        success = redisDB.SetKey(key, value);
                        response = (success == true) ? "OK" : "ERROR";
                        textBlockOutput.Text += string.Format("{0}: {1} {2} {3}\n", response, operation, key, value);
                    }
                    // SET <key> <value> <expiration>
                    else if (splitText.Length == 4)
                    {
                        key = splitText[1];
                        value = splitText[2];
                        int expiration = int.Parse(splitText[3]);
                        success = redisDB.SetKeyWithExpiration(key, value, expiration);
                        response = (success == true) ? "OK" : "ERROR";
                        textBlockOutput.Text += string.Format("{0}: {1} {2} {3} {4}\n", response, operation, key, value, expiration);
                    }
                    else
                        textBoxMessage.Text = "[ERROR][SYNTAX]: SET KEY VALUE";
                    break;

                case "del":
                    // DEL <key>
                    if (splitText.Length == 2)
                    {
                        key = splitText[1];
                        success = redisDB.DelKey(key);
                        response = (success == true) ? "OK" : "ERROR";
                        textBlockOutput.Text += string.Format("{0}: {1} {2}\n", response, operation, key);
                    }
                    // DEL <keys>
                    else if (splitText.Length > 2)
                    {
                        // Construct array of keys from input, first element in splitText is "del" command - start with second element
                        string[] keys = new string[splitText.Length - 1];
                        for (int i = 0; i < keys.Length; i++)
                        {
                            keys[i] = splitText[i + 1];
                        }

                        long responseLong = redisDB.DelKeys(keys);

                        // Output
                        StringBuilder sb = new StringBuilder(string.Format("{0}: {1} ", responseLong, operation));
                        foreach (string k in keys)
                            sb.Append(string.Format("{0} ", k));
                        sb.Append("\n");
                        textBlockOutput.Text += sb.ToString();
                    }
                    else
                        textBoxMessage.Text = "[ERROR][SYNTAX]: DEL KEY [, KEY, ...]";
                    break;

                case "ttl":
                    // TTL <key>
                    if (splitText.Length == 2)
                    {
                        key = splitText[1];
                        int? successTTL = redisDB.TTLKey(key);
                        if (successTTL is int)
                            textBlockOutput.Text += string.Format("Key {0} expires in {1} seconds\n", key, successTTL);
                        else
                            textBlockOutput.Text += string.Format("Key {0} does not expire/exist\n", key);
                    }
                    else
                        textBoxMessage.Text = "[ERROR][SYNTAX]: TTL KEY";
                    break;

                default:
                    textBoxMessage.Text = "[ERROR]: UNKNOWN OPERATION!";
                    break;
            }
        }

        /// <summary>
        /// Print the data of all keys stored in Redis database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintKeys(object sender = null, RoutedEventArgs e = null)
        {
            // Clear Tree View
            treeView_Copy.Items.Clear();

            // Prepare stack panel for tree view items
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            treeView_Copy.Items.Add(panel);

            // Add treeview items to stack panel
            foreach (var keyElement in redisDB.GetKeysList())
            {
                TreeViewItem tvi = new TreeViewItem();
                tvi.Header = keyElement.Key;

                for (int i = 0; i < keyElement.Value.Length; i++)
                {
                    TreeViewItem tviChild = new TreeViewItem();
                    tviChild.Header = keyElement.Value[i];
                    tvi.Items.Add(tviChild);
                }

                panel.Children.Add(tvi);
            }
        }

        /// <summary>
        /// Clear the TextBox element that holds the output of operations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearOutputBox(object sender, RoutedEventArgs e)
        {
            textBlockOutput.Text = string.Empty;
        }
    }
}
