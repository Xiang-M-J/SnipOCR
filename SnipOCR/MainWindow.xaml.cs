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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using IWshRuntimeLibrary;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseEventHandler = System.Windows.Input.MouseEventHandler;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace SnipOCR
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 

    public enum ResizeDirection
    {
        /// <summary>
        /// 左
        /// </summary>
        Left = 1,
        /// <summary>
        /// 右
        /// </summary>
        Right = 2,
        /// <summary>
        /// 上
        /// </summary>
        Top = 3,
        /// <summary>
        /// 左上
        /// </summary>
        TopLeft = 4,
        /// <summary>
        /// 右上
        /// </summary>
        TopRight = 5,
        /// <summary>
        /// 下
        /// </summary>
        Bottom = 6,
        /// <summary>
        /// 左下
        /// </summary>
        BottomLeft = 7,
        /// <summary>
        /// 右下
        /// </summary>
        BottomRight = 8,
    }

    public partial class MainWindow : Window
    {
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
        private static MemoryStream globalMemoryStream = new System.IO.MemoryStream();
        public MainWindow()
        {
            InitializeComponent();
            InitNotifyIcon();
            
            this.SourceInitialized += delegate (object sender, EventArgs e)
            {
                this._HwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            };
            StartAutomaticallyCreate("StickyNote");
            this.MouseMove += new MouseEventHandler(Window_MouseMove);
            this.Topmost = true;
            if (this.Topmost == true)
            {
                Pin.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(169, 170, 170));
                Pin.ToolTip = "取消置顶";
            }
            if (!System.IO.File.Exists(MyXml.XMLPATH))
            {
                // 填入 API Key 和 Secret Key
                MyXml.CreateXml("API Key", "Secret Key");
            }
            
            
        }

        // 图片资源声明
        private string[] Image = new string[]
        {
            "pack://application:,,,/resources/Mini.png",
            "pack://application:,,,/resources/max.png"
        };

        private bool Enter = false;                         // 定义变量判断是否回车换行
        
        private NotifyIcon notifyIcon;                      // 定义托盘图标

        /// <summary>
        /// 拉伸窗口 Begin
        /// </summary>
        private HwndSource _HwndSource;
        private const int WM_SYSCOMMAND = 0x112;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private Dictionary<ResizeDirection, Cursor> cursors = new Dictionary<ResizeDirection, Cursor>
        {
            {ResizeDirection.Top, Cursors.SizeNS},
            {ResizeDirection.Bottom, Cursors.SizeNS},
            {ResizeDirection.Left, Cursors.SizeWE},
            {ResizeDirection.Right, Cursors.SizeWE},
            {ResizeDirection.TopLeft, Cursors.SizeNWSE},
            {ResizeDirection.BottomRight, Cursors.SizeNWSE},
            {ResizeDirection.TopRight, Cursors.SizeNESW},
            {ResizeDirection.BottomLeft, Cursors.SizeNESW}
        };
    
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if (element != null && !element.Name.Contains("Resize")) this.Cursor = Cursors.Arrow;
            }
        }

        private void ResizePressed(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ResizeDirection direction = (ResizeDirection)Enum.Parse(typeof(ResizeDirection), element.Name.Replace("Resize", ""));

            this.Cursor = cursors[direction];
            if (e.LeftButton == MouseButtonState.Pressed) ResizeWindow(direction);
        }

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_HwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        ///<summary>
        /// 窗口拉伸 End
        ///</summary>


        private void InitNotifyIcon()
        {
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.BalloonTipText = "OCR";
            this.notifyIcon.ShowBalloonTip(2000);
            this.notifyIcon.Text = "OCR";
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            this.notifyIcon.Visible = true;
            //打开菜单项
            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("显示");
            open.Click += new EventHandler(ShowWindow);
            //退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("退出");
            exit.Click += new EventHandler(CloseWindow);
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) this.ShowWindow(o, e);
            });
        }
        private void ShowWindow(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = true;
            this.Activate();
        }
        private void HideWindow(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Visibility = Visibility.Hidden;
            this.WindowState = WindowState.Minimized;
        }
        private void CloseWindow(object sender, EventArgs e)
        {
            this.notifyIcon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        }

        #region 设置开机自启
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exeName">程序名</param>
        /// <returns>bool</returns>
        public bool StartAutomaticallyCreate(string exeName)
        {
            try
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk");
                //设置快捷方式的目标所在的位置(源程序完整路径) 
                shortcut.TargetPath = System.Windows.Forms.Application.ExecutablePath;
                //应用程序的工作目录 
                //当用户没有指定一个具体的目录时，快捷方式的目标应用程序将使用该属性所指定的目录来装载或保存文件。 
                shortcut.WorkingDirectory = System.Environment.CurrentDirectory;
                //MessageBox.Show(shortcut.WorkingDirectory);
                //目标应用程序窗口类型(1.Normal window普通窗口,3.Maximized最大化窗口,7.Minimized最小化) 
                shortcut.WindowStyle = 1;
                //快捷方式的描述 
                shortcut.Description = exeName + "_Ink";
                //设置快捷键(如果有必要的话.) 
                //shortcut.Hotkey = "CTRL+ALT+D"; 
                shortcut.Save();
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("设置开机自启时出现错误");
            }
            return false;
        }

        /// <summary>
        /// 开机自启删除
        /// </summary>
        /// <param name="exeName">程序名称</param>
        /// <returns></returns>
        public bool StartAutomaticallyDel(string exeName)
        {
            try
            {
                System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk");
                return true;
            }
            catch (Exception) { }
            return false;
            
        }
        #endregion

        /// <summary>
        /// 获取屏幕图像
        /// </summary>
        /// <returns>图像文件</returns>
        private Bitmap GetScreen()
        {
            System.Drawing.Rectangle rc = System.Windows.Forms.SystemInformation.VirtualScreen;
            Bitmap bitmap = new Bitmap(rc.Width, rc.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }
        /// <summary>
        /// 点击截图并识别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Snap_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            SnapWin snapWin = new SnapWin(GetScreen())
            {
                sendMessage = GetSnapArea
            };
            snapWin.ShowDialog();

            this.WindowState = WindowState.Normal;
        }
        /// <summary>
        /// 获取截屏区域的文字
        /// </summary>
        /// <param name="bitmap"></param>
        private void GetSnapArea(Bitmap bitmap)
        {

            Show.Text = null;
            DateTime startTime = DateTime.Now;
            string str = OCRApi.getFileBase64(bitmap);
            string text = OCRApi.GetWords(str,true);
            OCRApi.Str2Json(text,WordsNum,Show);
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            NeedTime.Text = string.Format("{0:N3}", duration.TotalSeconds) + " S";
            this.Visibility = Visibility.Visible;
        }
        
        /// <summary>
        /// 保存截图区域
        /// </summary>
        /// <param name="bitmap"></param>
        private void SaveSnapArea(Bitmap bitmap)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG|*.png|JPG|*.jpg|JPEG|*.jpeg|BMP|*.bmp|TIFF|*.tiff|WEBP|*.webp";
            sfd.RestoreDirectory = true;
            if (sfd.ShowDialog() == true)
            {
                bitmap.Save(sfd.FileName);   
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (MyXml.GetXmlValue("CloseShow")=="true")
            {
                if (MyXml.GetXmlValue("CloseStyle") == "Mini")
                {

                    this.ShowInTaskbar = false;
                    this.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.notifyIcon.Visible = false;
                    this.Close();
                }
            }
            else
            {
                Ask ask = new Ask();
                ask.ShowDialog();
                if (MyXml.GetXmlValue("CloseStyle") == "Mini")
                {
                    this.ShowInTaskbar = false;
                    this.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.notifyIcon.Visible = false;
                    this.Close();
                }
            }
            
            //this.Close();
            //this.ShowInTaskbar = false;
            //this.Visibility = Visibility.Collapsed;
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState!=WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
                Max.Background = new ImageBrush(new BitmapImage(new Uri(Image[0])));
            }
            else
            {
                this.WindowState = WindowState.Normal;
                Max.Background= new ImageBrush(new BitmapImage(new Uri(Image[1])));
            }
            
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 移动窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }

        }
        

        private void OpenPic_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PNG|*.png|JPG|*.jpg|JPEG|*.jpeg|BMP|*.bmp|TIFF|*.tiff|WEBP|*.webp";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog()==true)
            {

                Show.Text = null;
                DateTime startTime = DateTime.Now;
                string text = OCRApi.GetWords(ofd.FileName);
                OCRApi.Str2Json(text,WordsNum,Show);
                DateTime endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime;
                NeedTime.Text = string.Format("{0:N3}", duration.TotalSeconds) + " S";
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            OCRApi.Clear();
            Show.Text = null;
            WordsNum.Text = null;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (Show.Text != null)
            {
                Clipboard.SetDataObject(Show.Text);
            }
        }

        private void Pin_Click(object sender, RoutedEventArgs e)
        {
            if (this.Topmost == true)
            {
                this.Topmost = false;
                Pin.Background = null;
                Pin.ToolTip = "置顶";
            }
            else
            {
                this.Topmost = true;
                Pin.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(169,170,170));
                Pin.ToolTip = "取消置顶";
            }
        }

        private void Grid_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Link;                           
            else e.Effects = DragDropEffects.None;                     

        }
        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void Grid_Drop(object sender, System.Windows.DragEventArgs e)
        {

            string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            string[] path = fileName.Split('.');
            string suffix = path[path.Length-1];
            if (suffix=="png"||suffix=="jpg"||suffix=="webm"||suffix=="jpeg"||suffix=="tiff")
            {
                Show.Text = null;
                DateTime startTime = DateTime.Now;
                string text = OCRApi.GetWords(fileName);
                OCRApi.Str2Json(text,WordsNum,Show);
                DateTime endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime;
                NeedTime.Text = string.Format("{0:N3}", duration.TotalSeconds) + " S";
            }
            else
            {
                MessageBox.Show("文件格式错误");
            }
            
        }

        private void CutScr_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            SnapWin snapWin = new SnapWin(GetScreen())
            {
                sendMessage = SaveSnapArea
            };
            snapWin.ShowDialog();

            this.WindowState = WindowState.Normal;
        }

        private void IsEnter_Click(object sender, RoutedEventArgs e)
        {
            if (Enter == false)
            {
                Show.Text = OCRApi.EnterString;
                Enter = true;
                IsEnter.ToolTip = "文本不换行";
            }
            else
            {
                Enter = false;
                Show.Text = OCRApi.NoEnterString;
                IsEnter.ToolTip = "文本换行";
            }
        }



        private void Import_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "txt|*.txt|docx|*.doc";
            sfd.RestoreDirectory = true;
            if (sfd.ShowDialog() == true)
            {
                StreamWriter streamWriter = new StreamWriter(sfd.FileName);
                streamWriter.Write(Show.Text);
                streamWriter.Close();
            }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("该功能正在开发...");

        }

        /// <summary>
        /// 手写笔迹识别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandWrite_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PNG|*.png|JPG|*.jpg|JPEG|*.jpeg|BMP|*.bmp|TIFF|*.tiff|WEBP|*.webp";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == true)
            {
                
                try
                {
                    Show.Text = null;
                    DateTime startTime = DateTime.Now;
                    string text = OCRApi.getHandWriting(ofd.FileName);
                    OCRApi.Str2Json(text, WordsNum, Show);
                    DateTime endTime = DateTime.Now;
                    TimeSpan duration = endTime - startTime;
                    NeedTime.Text = string.Format("{0:N3}", duration.TotalSeconds) + " S";
                }
                catch
                {
                    MessageBox.Show("手写笔迹识别出现错误，可能是识别次数过多");
                }
                
            }
        }
    }

}
