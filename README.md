# SnipOCR
基于C#WPF的OCR程序
1. 调用百度智能云接口，由于使用的是免费接口，所以存在接口调用失败的情况
2. 实现了截图功能，通过在屏幕上截图从而进行下一步的OCR
3. 调用接口的类为OCRApi
4. 对XML文件进行操作的类为MyXml
5. 在使用请先打开MainWindow.xaml.cs，在下面的程序中填入相关信息：
```csharp

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
```
