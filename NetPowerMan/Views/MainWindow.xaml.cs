using NetPowerMan.Model;
using NetPowerMan.Services;
using NetPowerMan.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace NetPowerMan.Views
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables
        System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();
        private const int WM_QUERYENDSESSION = 0x11;
        private const int WM_ENDSESSION = 0x16;
        private readonly ILogger _logger;
        private bool BallonTipIsShown = false;
        #endregion
        public MainWindow(ILogger logger)
        {
            InitializeComponent();
            _logger = logger;
            //NotifyIcon init
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/NetPowerMan;component/logo.ico")).Stream;
            this.nIcon.Icon = new System.Drawing.Icon(iconStream);
            iconStream.Dispose();
            this.nIcon.Visible = true;
            this.nIcon.Text = "NetPowerMan";

            //Contextmenu init
            nIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            nIcon.ContextMenuStrip.Items.Add("Quit", null, ContextMenuStripQuit);

            //event link
            nIcon.MouseClick += NIcon_Click;
            nIcon.BalloonTipShown += NIcon_BalloonTipShown;
            nIcon.BalloonTipClosed += NIcon_BalloonTipClosed;
            //this.Loaded += new RoutedEventHandler(MyWindow_Loaded);
            this.DataContextChanged += MainWindow_DataContextChanged;
            //MainSpin.Duration = new Duration(new TimeSpan(20000));
            this.Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Spinner spinner = new Spinner();
            //spinner.Duration2 = TimeSpan.FromSeconds(2);
            //spinner.From = 0;
            //spinner.To = 360;
            //spinner.Height = 50;
            //spinner.Width = 50;
            //spinner.SpinnerColor = new BrushConverter().ConvertFromString("#DFFF4500") as SolidColorBrush;
            //Binding vis = new Binding("IsVisible")
            //{
            //    Mode = BindingMode.TwoWay,
            //    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            //};
            //vis.Converter = new Views.BooleanConverter<Visibility>(Visibility.Hidden,Visibility.Visible);
            //spinner.SetBinding(ContentControl.VisibilityProperty, vis);
            //spinner.Margin = new Thickness(0,60,0,0);
            //MainGrid.Children.Add(spinner);
            //Grid.SetRow(spinner, 1);
            //Grid.SetColumn(spinner, 0);
            //Grid.SetColumnSpan(spinner, 2);
            //spinner.ToolTip = new ToolTip(){ Content = "Stop"};
            //spinner.MouseDown += Spinner_MouseDown;

            //Spinner spinner2 = new Spinner();
            //spinner2.Duration2 = TimeSpan.FromSeconds(2);
            //spinner2.Height = 20;
            //spinner2.Width = 20;
            //spinner2.From = 360;
            //spinner2.To = 0;
            //spinner2.SpinnerColor = Brushes.OrangeRed;
            //spinner2.SetBinding(ContentControl.VisibilityProperty, vis);
            //spinner2.Margin = new Thickness(0, 60, 0, 0);
            //MainGrid.Children.Add(spinner2);
            //Grid.SetRow(spinner2, 1);
            //Grid.SetColumn(spinner2, 0);
            //Grid.SetColumnSpan(spinner2, 2);

            //Spinner spinner3 = new Spinner();
            //spinner3.Duration2 = TimeSpan.FromSeconds(2.8);
            //spinner3.Height = 20;
            //spinner3.Width = 20;
            //spinner3.From = 360;
            //spinner3.To = 0;
            //spinner3.SpinnerColor = Brushes.OrangeRed;
            //spinner3.SetBinding(ContentControl.VisibilityProperty, vis);
            //spinner3.Margin = new Thickness(0, 60, 60, 0);
            //MainGrid.Children.Add(spinner3);
            //Grid.SetRow(spinner3, 1);
            //Grid.SetColumn(spinner3, 1);

            //Spinner spinner4 = new Spinner();
            //spinner4.Duration2 = TimeSpan.FromSeconds(2.5);
            //spinner4.Height = 20;
            //spinner4.Width = 20;
            //spinner4.From = 360;
            //spinner4.To = 0;
            //spinner4.SpinnerColor = Brushes.OrangeRed;
            //spinner4.SetBinding(ContentControl.VisibilityProperty, vis);
            //spinner4.Margin = new Thickness(60, 60, 0, 0);
            //MainGrid.Children.Add(spinner4);
            //Grid.SetRow(spinner4, 1);
            //Grid.SetColumn(spinner4, 0);

            //spinner.StoryBoardCommand.Stop();
            //spinner2.StoryBoardCommand.Stop();
            //spinner3.StoryBoardCommand.Stop();
            //spinner4.StoryBoardCommand.Stop();

            var initBlurEffect = new WindowBlureffect(this, AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND) { BlurOpacity = 10 };
        }
        private void Spinner_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed) 
            {
                MainViewModel mainViewModel = this.DataContext as MainViewModel;
                mainViewModel.IsVisible = true;
                mainViewModel.DisposeLabelDot.Cancel();
            }
        }
        private void NIcon_BalloonTipShown(object sender, EventArgs e)
        {
            BallonTipIsShown = true;
        }
        private void NIcon_BalloonTipClosed(object sender, EventArgs e)
        {
            BallonTipIsShown = false;
        }
        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MainViewModel mainViewModel = this.DataContext as MainViewModel;
            mainViewModel.DeviceStatusChanged += MainViewModel_DeviceStatusChanged;
        }
        private void MainViewModel_DeviceStatusChanged(string DeviceName, string DeviceStatus)
        {
            //Avoid long queue of notify, example: in case of lost network connection will notify only the first status changes
            //during the ShowBalloonTipy time.
            if (!BallonTipIsShown)
            {
                this.nIcon.ShowBalloonTip(1000, "Info", string.Format("{0} is {1}!", DeviceName, DeviceStatus), System.Windows.Forms.ToolTipIcon.Info);
            }
        }
        private void NIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //mainViewModel.StartPingLoop();
                MainViewModel temp = this.DataContext as MainViewModel;
                temp.StartPing("Resume");
                this.Show();
            }
        }
        private void ContextMenuStripQuit(object sender, EventArgs e)
        {
            nIcon.Dispose();
            Application.Current.Shutdown();
        }
        private void ExpanderCollapsed(object sender, RoutedEventArgs e)
        {
            if (ExpanderBorder.CornerRadius.TopRight > 0)
                ExpanderBorder.CornerRadius = new CornerRadius(8, 8, 8, 8);
        }
        private void ExpanderExpanded(object sender, RoutedEventArgs e)
        {
            if (ExpanderBorder.CornerRadius.TopRight > 0)
                ExpanderBorder.CornerRadius = new CornerRadius(8, 8, 0, 0);

            //Spinner spinner = new Spinner();
            //spinner.Duration2 = TimeSpan.FromSeconds(10);
            //spinner.Height = 50;
            //spinner.Width = 50;
            //spinner.SpinnerColor = Brushes.Red;
            //Binding vis = new Binding("IsVisible")
            //{
            //    Mode = BindingMode.TwoWay,
            //    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            //};
            //vis.Converter = new BooleanToVisibilityConverter();

            //spinner.SetBinding(ContentControl.VisibilityProperty, vis);

            //MainGrid.Children.Add(spinner);
            //Grid.SetRow(spinner, 1);
            //Grid.SetColumn(spinner, 0);
            //Grid.SetColumnSpan(spinner, 2);

        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ExpanderHeaderGrid.Width = exp.ActualWidth - 28;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel temp = this.DataContext as MainViewModel;   
            this.Hide();
            this.nIcon.ShowBalloonTip(1, "Info", "PowerManager is still running in background", System.Windows.Forms.ToolTipIcon.Info);
            temp.StopPing();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            if (source != null)
            {
                source.AddHook(WndProc);

                //Services.BlurCornerRadius blurCornerRadius = new BlurCornerRadius(this);
                //WindowBlureffect tt = new WindowBlureffect(this, AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND) { BlurOpacity = 10 };
            }
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            string createText = string.Empty;

            if (msg == SystemMessage.WM_SHOWME)
            {
                System.Windows.Forms.MouseEventArgs SimulMouseCkicked = new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, 1, 1, 1);
                NIcon_Click(null, SimulMouseCkicked);
                _logger.Info("WndProc: WM_SHOWME");
            }

            if (msg == WM_QUERYENDSESSION)
            {
                createText = DateTime.Now.ToString() + ": WM_QUERYENDSESSION: wParam=" + wParam.ToInt64().ToString() +
                    " lParam=" + wParam.ToInt64().ToString() + "\n\r";
                _logger.Info(createText);

                string s = "Waiting all device off...";
                int elementSize = Marshal.SizeOf(typeof(IntPtr));
                IntPtr intPtr = Marshal.AllocHGlobal(elementSize);

                //from vista there is a new shutdown management, i need call this to stop the shutdown
                WindowsUnmanaged.ShutdownBlockReasonCreate(hwnd, s);

                //will not recive again this maessage next time windows send..
                handled = true;

                //return 1 so shutdown will continue (OnSessionEnding) is fired
                //return 0 shutdown will stop OnSessionEnding is not fired            
                Marshal.WriteIntPtr(intPtr, elementSize, (IntPtr)(1));
                return intPtr;
            }

            if (msg == WM_ENDSESSION)
            {
                WindowsUnmanaged.ShutdownBlockReasonDestroy(hwnd);

                //LPARAM and LRESULT are a typedef for LONG_PTR (x64 or x32, depends on windows)
                //long d = Marshal.ReadInt64(lParam);
                //long dd = Marshal.ReadInt64(wParam);
                //long d = (long)lParam.ToInt64();
                //uint dd = (uint)wParam.ToInt32();

                createText = DateTime.Now.ToString() + ": WM_ENDSESSION: wParam=" + wParam.ToInt64().ToString() +
                    " lParam=" + wParam.ToInt64().ToString() + "\n\r";
                _logger.Info(createText);

                return IntPtr.Zero;
            }
            return IntPtr.Zero;
        }
    }
}
