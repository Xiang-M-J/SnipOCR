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
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Ink;
using Rectangle = System.Drawing.Rectangle;
using MessageBox = System.Windows.MessageBox;
using Image = System.Drawing.Image;
using System.Drawing.Imaging;
using Size = System.Windows.Size;

namespace SnipOCR
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
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
        public Window1()
        {
            InitializeComponent();
            MouseDown += Ink_MouseDown;
            MouseUp += Ink_MouseUp;
            MouseMove += Ink_MouseMove;
            MouseRightButtonDown += Ink_MouseRightButtonDown;
            //DrawingAttributes drawingAttributes = new DrawingAttributes
            //{
            //    Color = Colors.Red,
            //    Width = 2,
            //    Height = 2,
            //    StylusTip = StylusTip.Rectangle,
            //    //FitToCurve = true,
            //    IsHighlighter = false,
            //    IgnorePressure = true,

            //};
            //Ink.DefaultDrawingAttributes = drawingAttributes;
        }


        private Bitmap ScreenShot;
        private Bitmap GetScreen()
        {
            System.Drawing.Rectangle rc = System.Windows.Forms.SystemInformation.VirtualScreen;
            ScreenShot = new Bitmap(rc.Width, rc.Height);

            using (Graphics g = Graphics.FromImage(ScreenShot))
            {
                g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }
            ScreenShot.Save("D:\\VS2017Project\\SnipOCR\\SnipOCR\\resources\\123.png");

            return ScreenShot;

        }
        private Bitmap Getmap(Bitmap bitmap)
        {
            //Screen.Background = new ImageBrush(GetScreen());
            BitmapSource returnSource;
            IsSnap = true;
            try
            {
                returnSource = Imaging.CreateBitmapSourceFromHBitmap(GetScreen().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                returnSource = null;
            }
            ImageBrush b = new ImageBrush();
            b.ImageSource = returnSource;
            b.Stretch = Stretch.Fill;
            //BitmapImage bitmapImage = new BitmapImage();
            //using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            //{
            //    GetScreen().Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            //    bitmapImage.BeginInit();
            //    bitmapImage.StreamSource = ms;
            //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //    bitmapImage.EndInit();
            //    bitmapImage.Freeze();
            //}
            StartSnap();
            Ink.Background = b;
            //Ink.Opacity = 0.3;

            Ink.EditingMode = InkCanvasEditingMode.None;
            

            
            return null;
        }
        

        private void Snap_Click_1(object sender, RoutedEventArgs e)
        {

            //this.WindowState = WindowState.Minimized;

            Bitmap bitmap = GetScreen();
            String str=getFileBase64(bitmap);
            TEXT.Text = str;
            //Getmap(bitmap);
            //MessageBox.Show(DESKTOP.Width.ToString()+'\t'+DESKTOP.Height.ToString());
            //MessageBox.Show(ScaleX.ToString() + '\t' + ScaleY.ToString());
            //MessageBox.Show(getDpi().ToString());

        }
        private bool ISDown = false;
        System.Windows.Point Startpoint =new System.Windows.Point();
        private void Ink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ISDown = true;
            sure.Visibility = Visibility.Collapsed;
            Startpoint = Mouse.GetPosition(this);
            //System.Windows.MessageBox.Show(x.ToString());
        }
        Rect rect;
        private void Ink_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsSnap)
            {
                if (ISDown == true)
                {
                    System.Windows.Point point = Mouse.GetPosition(this);
                    rect = new Rect(Startpoint, point);
                    rec.Visibility = Visibility.Visible;
                    rec.Margin = new Thickness(rect.Left, rect.Top, 0, 0);
                    rec.Height = rect.Height;
                    rec.Width = rect.Width;
                    //System.Drawing.Rectangle rc = System.Windows.Forms.SystemInformation.VirtualScreen;
                    //System.Drawing.Rectangle rectangle = new Rectangle((int)rect.X,(int)rect.Y,(int)rect.Width,(int)rect.Height);
                    //var bitmap = new Bitmap((int)rectangle.Width, (int)rectangle.Height);

                    //using (Graphics g = Graphics.FromImage(bitmap))
                    //{

                    //    g.CopyFromScreen((int)rectangle.X, (int)rectangle.Y, 0, 0, rectangle.Size, CopyPixelOperation.SourceCopy);
                    //}

                    //Rect maskrect1 = new Rect(0, 0,SystemInformation.VirtualScreen.Width, rect.Top);
                    //MessageBox.Show(maskrect1.Left.ToString() + '\t' + maskrect1.Top.ToString() + '\t' + maskrect1.Bottom.ToString() + '\t' + maskrect1.Right.ToString());
                    //Rect maskrect2 = new Rect(0, rect.Top, rect.Left, rect.Height);
                    //Rect maskrect3 = new Rect(0, rect.Bottom, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height-rect.Bottom);
                    //Rect maskrect4 = new Rect(rect.Right, rect.Top, SystemInformation.VirtualScreen.Width-rect.Right, rect.Height);

                    //mask1.Margin = new Thickness(0, 0, 0, rect.Top);
                    ////mask1.Height = maskrect1.Height;
                    ////mask1.Width = maskrect1.Width;
                    //mask1.Height = rect.Top;
                    //mask1.Width = SystemInformation.VirtualScreen.Width;
                    //mask2.Margin = new Thickness(maskrect2.Left, maskrect2.Top, maskrect2.Right, maskrect2.Bottom);
                    //mask2.Height = maskrect2.Height;
                    //mask2.Width = maskrect2.Width;
                    //mask3.Margin = new Thickness(maskrect3.Left, maskrect3.Top, maskrect3.Right, maskrect3.Bottom);
                    //mask3.Height = maskrect3.Height;
                    //mask3.Width = maskrect3.Width;
                    //mask4.Margin = new Thickness(maskrect4.Left, maskrect4.Top, maskrect4.Right, maskrect4.Bottom);
                    //mask4.Height = maskrect4.Height;
                    //mask4.Width = maskrect4.Width;
                    //Image image = ScreenShot;


                    //Image img = ScreenShot;
                    //Rectangle dest = new Rectangle((int)snapArea.X, (int)snapArea.Y, (int)snapArea.Width, (int)snapArea.Height);
                    //GraphicsUnit units = GraphicsUnit.Pixel;
                    //Graphics graphics=Graphics.FromImage(img);
                    //graphics.DrawImage(img, dest, (int)snapArea.X, (int)snapArea.Y, (int)snapArea.Width, (int)snapArea.Height, units);

                    // Create rectangle for adjusted image.
                    //Rectangle destRect2 = new Rectangle((int)snapArea.X, (int)snapArea.Y, (int)snapArea.Width, (int)snapArea.Height);

                    // Create image attributes and set large gamma.
                    //ImageAttributes imageAttr = new ImageAttributes();
                    //imageAttr.SetGamma(4.0F);

                    //// Draw adjusted image to screen.
                    //graphics.DrawImage(img, destRect2, (int)snapArea.X, (int)snapArea.Y, (int)snapArea.Width, (int)snapArea.Height, units, imageAttr);

                    //PaintEventArgs paintEventArgs=new PaintEventArgs();

                }
            }
        }
        Rect snapArea = new Rect();
        private void Ink_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsSnap)
            {
                //MessageBox.Show(rect.Top.ToString()+'\t'+rect.Left.ToString()+'\t'+ rect.Bottom.ToString()+'\t'+ rect.Right.ToString());
                ISDown = false;
                if (rec.Visibility == Visibility.Collapsed)
                {
                    sure.Visibility = Visibility.Collapsed;
                }
                else
                {
                    sure.Visibility = Visibility.Visible;
                }
                if (rect.Width >= 10 && rect.Height >= 10)
                {
                    
                    if (SystemInformation.VirtualScreen.Height - rect.Bottom >= 0)
                    {
                        snapArea = rect;
                        sure.Margin = new Thickness(rect.Right - 40, rect.Bottom, 0, 0);

                    }
                    else
                    {
                        snapArea = rect;
                        sure.Margin = new Thickness(rect.Left + 10, rect.Top + 22, 0, 0);
                    }
                }

            }

        }

        private void Ink_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ISDown = false;
            sure.Visibility = Visibility.Collapsed;
            rec.Visibility = Visibility.Collapsed;
            Restore();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Escape))
            {
                ISDown = false;
                sure.Visibility = Visibility.Collapsed;
                rec.Visibility = Visibility.Collapsed;
                Restore();
            }
        }
        private void Restore()
        {
            IsSnap = false;
            this.WindowState = WindowState.Normal;
            this.Width = 500;
            this.Height = 300;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Ink.Height = 0;
            Ink.Width = 0;
            this.Visibility = Visibility.Visible;
            rec.Visibility = Visibility.Collapsed;
            
            Snap.Visibility = Visibility.Visible;
            Ink.Visibility = Visibility.Collapsed;
        }
        private void StartSnap()
        {
            this.WindowState = WindowState.Normal;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Background = new SolidColorBrush();
            this.Visibility = Visibility.Visible;
            this.Left = 0;
            this.Top = 0;
            this.Height = SystemParameters.VirtualScreenHeight;
            this.Width = SystemParameters.VirtualScreenWidth;
            Snap.Visibility = Visibility.Collapsed;
            Ink.Visibility = Visibility.Visible;
            //mask(this.Width,this.Height);
        }
        private void mask(double width,double height)
        {
            mask1.Visibility = Visibility.Visible;
            mask2.Visibility = Visibility.Visible;
            mask3.Visibility = Visibility.Visible;
            mask4.Visibility = Visibility.Visible;
            Rect maskrect1 = new Rect(0, 0, width,Height/2);
            //MessageBox.Show(maskrect1.Left.ToString() + '\t' + maskrect1.Top.ToString() + '\t' + maskrect1.Bottom.ToString() + '\t' + maskrect1.Right.ToString());
            Rect maskrect2 = new Rect(0, Height/2, width/2, 0);
            Rect maskrect3 = new Rect(0, Height/2, width, height/2);
            Rect maskrect4 = new Rect(width/2, Height/2, width/2, 0);

            mask1.Margin = new Thickness(maskrect1.Left, maskrect1.Top, maskrect1.Right, maskrect1.Bottom);
            mask1.Height = maskrect1.Height;
            mask1.Width = maskrect1.Width;
            mask2.Margin = new Thickness(maskrect2.Left, maskrect2.Top, 0, 0);
            mask2.Height = maskrect2.Height;
            mask2.Width = maskrect2.Width;
            mask3.Margin = new Thickness(maskrect3.Left, maskrect3.Top, 0, 0);
            mask3.Height = maskrect3.Height;
            mask3.Width = maskrect3.Width;
            mask4.Margin = new Thickness(maskrect4.Left, maskrect4.Top, 0, 0);
            mask4.Height = maskrect4.Height;
            mask4.Width = maskrect4.Width;
            MessageBox.Show(maskrect1.Width.ToString() + '\t' +mask1.Width.ToString()+'\t'+mask1.Margin.Left.ToString());
            

        }
        private void none(PaintEventArgs e)
        {
            Image img = ScreenShot;
            Rectangle dest = new Rectangle(0,0,(int)snapArea.Width,(int)snapArea.Height);
            GraphicsUnit units = GraphicsUnit.Pixel;
            e.Graphics.DrawImage(img, dest, (int)snapArea.X, (int)snapArea.Y, (int)snapArea.Width, (int)snapArea.Height, units);

            // Create rectangle for adjusted image.
            Rectangle destRect2 = new Rectangle(100, 175, 450, 150);

            // Create image attributes and set large gamma.
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetGamma(4.0F);

            // Draw adjusted image to screen.
            e.Graphics.DrawImage(img, destRect2, (int)snapArea.X, (int)snapArea.Y, (int)snapArea.Width, (int)snapArea.Height, units, imageAttr);

        }
        private void Sure_Click(object sender, RoutedEventArgs e)
        {
            if (IsSnap)
            {
                //var sourceRect = rect.ToRectangle();
                double dpi = getDpi();
                System.Drawing.Rectangle rectangle = new Rectangle((int)((snapArea.X+2)*dpi), (int)((snapArea.Y+2)*dpi), (int)((snapArea.Width-4)*dpi), (int)((snapArea.Height-4)*dpi));
                var bitmap1 = new Bitmap((int)(snapArea.Width*dpi), (int)(snapArea.Height*dpi), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //MessageBox.Show(snapArea.X.ToString() + '\t' + snapArea.Y.ToString() + '\t' + snapArea.Width.ToString() + '\t' + snapArea.Height.ToString());
                //MessageBox.Show(rectangle.X.ToString() + '\t' + rectangle.Y.ToString() + '\t' + rectangle.Width.ToString() + '\t' + rectangle.Height.ToString());

                using (Graphics g = Graphics.FromImage(bitmap1))
                {
                    //g.CopyFromScreen((int)snapArea.X, (int)snapArea.Y, 0, 0, rectangle.Size, CopyPixelOperation.SourceCopy);
                    g.CopyFromScreen(rectangle.X, rectangle.Y, 0, 0, rectangle.Size, CopyPixelOperation.SourceCopy);
                }
                bitmap1.Save("D:\\VS2017Project\\SnipOCR\\SnipOCR\\resources\\snap.png");
                Restore();
            }
            

        }
        //    double getDpi()
        //    {
        ////        double dDpi = 1;
        ////        HDC desktopDc = GetDC(NULL);
        ////        // Get native resolution
        ////        float horizontalDPI = GetDeviceCaps(desktopDc, LOGPIXELSX);
        ////        float verticalDPI = GetDeviceCaps(desktopDc, LOGPIXELSY);
        ////        int dpi = (horizontalDPI + verticalDPI) / 2;
        ////        dDpi = 1 + ((dpi - 96) / 24) * 0.25;
        ////        if (dDpi < 1)
        ////        {
        ////            dDpi = 1;
        ////        }

        ////::ReleaseDC(NULL, desktopDc);
        ////        return dDpi;
        //    }
        //public static Size DESKTOP
        //{
        //    get
        //    {
        //        IntPtr hdc = GetDC(IntPtr.Zero);
        //        Size size = new Size();
        //        size.Width = GetDeviceCaps(hdc, DESKTOPHORZRES);
        //        size.Height = GetDeviceCaps(hdc, DESKTOPVERTRES);
        //        ReleaseDC(IntPtr.Zero, hdc);
               
        //        return size;
        //    }
        //}
        //public static float ScaleX
        //{
        //    get
        //    {
        //        IntPtr hdc = GetDC(IntPtr.Zero);
        //        int t = GetDeviceCaps(hdc, DESKTOPHORZRES);
        //        int d = GetDeviceCaps(hdc, HORZRES);
        //        float ScaleX = (float)GetDeviceCaps(hdc, DESKTOPHORZRES) / (float)GetDeviceCaps(hdc, HORZRES);
        //        ReleaseDC(IntPtr.Zero, hdc);
        //        return ScaleX;
        //    }
        //}
        //public static float ScaleY
        //{
        //    get
        //    {
        //        IntPtr hdc = GetDC(IntPtr.Zero);
        //        float ScaleY = (float)(float)GetDeviceCaps(hdc, DESKTOPVERTRES) / (float)GetDeviceCaps(hdc, VERTRES);
        //        ReleaseDC(IntPtr.Zero, hdc);
        //        return ScaleY;
        //    }
        //}

        double getDpi()
        {
            double dDpi = 1;
            // Get desktop dc
            IntPtr desktopDc = GetDC(IntPtr.Zero);
            // Get native resolution
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
        public static String getFileBase64(Bitmap bitmap)
        {
            //MemoryStream ms = new MemoryStream();
            

            using (MemoryStream stream = new MemoryStream())

            {

                bitmap.Save(stream, ImageFormat.Jpeg);

                byte[] data = new byte[stream.Length];

                stream.Seek(0, SeekOrigin.Begin);

                stream.Read(data, 0, Convert.ToInt32(stream.Length));
                String str = Convert.ToBase64String(data);
                //return data;
                return str;
            }
            //return null;
        }
    }
}
