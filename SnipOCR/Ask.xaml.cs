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

namespace SnipOCR
{
    /// <summary>
    /// Ask.xaml 的交互逻辑
    /// </summary>
    public partial class Ask : Window
    {
        public Ask()
        {
            InitializeComponent();

        }

        private void Sure_Click(object sender, RoutedEventArgs e)
        {
            string[] CloseMg = new string[2] { "Mini", "false" };
            if (MiniButton.IsChecked == true)
            {
                CloseMg[0] = "Mini";
            }
            else
            {
                CloseMg[0] = "Exit";
            }
            if (checkBox.IsChecked == true)
            {
                CloseMg[1] = "true";
            }
            if (!MyXml.FindNode("Close"))
            {
                MyXml.InsertNode("CloseStyle", CloseMg[0], 1, "Close","Main");
                //MyXml.InsertNode("CloseStyle",CloseMg[0], 1);
                MyXml.InsertNode("CloseShow", CloseMg[1], 2,"Close");
            }
            else
            {
                MyXml.ChangeNode("CloseStyle", CloseMg[0],"Close");
                MyXml.ChangeNode("CloseShow", CloseMg[1],"Close");
            }

            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }

        }
    }
}
