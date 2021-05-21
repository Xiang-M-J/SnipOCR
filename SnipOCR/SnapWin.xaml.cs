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
using System.Drawing;
using System.Runtime.InteropServices;
using Rectangle = System.Drawing.Rectangle;
using System.Windows.Interop;

namespace SnipOCR
{
    /// <summary>
    /// SnapWin.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class SnapWin : Window
    {
        public delegate void SendMessage(Bitmap bitmap);
        public SendMessage sendMessage;
        #region Win32 API
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr ptr);
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(
        IntPtr hdc, // handle to DC
        int nIndex // index of capability
        );
        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);
        #endregion
        #region DeviceCaps常量
        const int HORZRES = 8;
        const int VERTRES = 10;
        const int LOGPIXELSX = 88;
        const int LOGPIXELSY = 90;
        const int DESKTOPVERTRES = 117;
        const int DESKTOPHORZRES = 118;
        #endregion

        private bool IsSnap = false;

        private Bitmap ScreenShot;
        //private Bitmap CutShot;
        public SnapWin(Bitmap bitmap)
        {
            InitializeComponent();
            this.Topmost = true;
            ScreenShot = bitmap;
            MouseDown += Ink_MouseDown;
            MouseUp += Ink_MouseUp;
            MouseMove += Ink_MouseMove;
            MouseRightButtonDown += Ink_MouseRightButtonDown;
            BitmapSource source;
            IsSnap = true;

            try
            {
                source = Imaging.CreateBitmapSourceFromHBitmap(ScreenShot.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                source = null;
            }
            ImageBrush b = new ImageBrush();
            b.ImageSource = source;
            b.Stretch = Stretch.Fill;
            Ink.Background = b;
            Ink.EditingMode = InkCanvasEditingMode.None;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Visibility = Visibility.Visible;
            this.Left = 0;
            this.Top = 0;
            this.Height = SystemParameters.VirtualScreenHeight;
            this.Width = SystemParameters.VirtualScreenWidth;
        }
        
        private bool ISDown = false;                    // 判断是否鼠标左键按下
        System.Windows.Point Startpoint = new System.Windows.Point();
        private void Ink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ISDown = true;
            Conf.Visibility = Visibility.Collapsed;
            Startpoint = Mouse.GetPosition(this);
        }
        Rect rect;
        private void Ink_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsSnap)
            {
                if (ISDown == true)
                {
                    System.Windows.Point point = Mouse.GetPosition(this);
                    rect = new Rect(Startpoint, point);
                    Bord.Visibility = Visibility.Visible;
                    Bord.Margin = new Thickness(rect.Left, rect.Top, 0, 0);
                    Bord.Height = rect.Height;
                    Bord.Width = rect.Width;
                }
            }
        }
        Rect snapArea = new Rect();
        private void Ink_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsSnap)
            {
                ISDown = false;
                if (Bord.Visibility == Visibility.Collapsed)
                {
                    Conf.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Conf.Visibility = Visibility.Visible;
                }
                if (rect.Width >= 10 && rect.Height >= 10)
                {

                    if (System.Windows.Forms.SystemInformation.VirtualScreen.Height - rect.Bottom >= 0)
                    {
                        snapArea = rect;
                        Conf.Margin = new Thickness(rect.Right - 40, rect.Bottom, 0, 0);

                    }
                    else
                    {
                        snapArea = rect;
                        Conf.Margin = new Thickness(rect.Left + 10, rect.Top + 22, 0, 0);
                    }
                }

            }

        }

        private void Ink_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ISDown = false;
            Conf.Visibility = Visibility.Collapsed;
            Bord.Visibility = Visibility.Collapsed;

        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Escape))
            {
                ISDown = false;
                Conf.Visibility = Visibility.Collapsed;
                Bord.Visibility = Visibility.Collapsed;
                this.Close();
            }
        }
        /// <summary>
        /// 确认区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Conf_Click(object sender, RoutedEventArgs e)
        {
            if (IsSnap)
            {
                double dpi = getDpi();
                Rectangle rectangle = new Rectangle((int)((snapArea.X + 2) * dpi), (int)((snapArea.Y + 2) * dpi), (int)((snapArea.Width - 4) * dpi), (int)((snapArea.Height - 4) * dpi));
                var bitmap1 = new Bitmap((int)(snapArea.Width * dpi), (int)(snapArea.Height * dpi), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
               
                using (Graphics g = Graphics.FromImage(bitmap1))
                {
                    g.CopyFromScreen(rectangle.X, rectangle.Y, 0, 0, rectangle.Size, CopyPixelOperation.SourceCopy);
                }
                this.Cursor = Cursors.Wait;
                sendMessage(bitmap1);

                this.Close();
            }


        }
        /// <summary>
        /// 获取系统dpi，这一步很重要
        /// </summary>
        /// <returns></returns>
        double getDpi()
        {
            double dDpi = 1;
            IntPtr desktopDc = GetDC(IntPtr.Zero);
            float horizontalDPI = GetDeviceCaps(desktopDc, LOGPIXELSX);
            float verticalDPI = GetDeviceCaps(desktopDc, LOGPIXELSY);
            int dpi = (int)(horizontalDPI + verticalDPI) / 2;
            dDpi = 1 + ((dpi - 96) / 24) * 0.25;
            if (dDpi < 1)
            {
                dDpi = 1;
            }
            ReleaseDC(IntPtr.Zero, desktopDc);
            return dDpi;
        }
    }
}
