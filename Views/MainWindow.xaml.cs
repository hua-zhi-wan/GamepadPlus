using System.Windows;
using System.Windows.Threading;
using AnotherGamepadPlus.Helpers;
using AnotherGamepadPlus.Services;

namespace AnotherGamepadPlus.Views
{
    public partial class MainWindow : Window
    {
        private readonly ControllerService _controllerService;
        private readonly MouseService _mouseService;
        private readonly ScreenService _screenService;
        private readonly DispatcherTimer _mousePositionTimer;

        public MainWindow()
        {
            InitializeComponent();

            // 初始化服务
            _screenService = new ScreenService();
            _mouseService = new MouseService(_screenService);
            _controllerService = new ControllerService();

            // 初始化定时器用于更新鼠标位置显示
            _mousePositionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _mousePositionTimer.Tick += MousePositionTimer_Tick;
            _mousePositionTimer.Start();

            // 注册事件处理
            RegisterEventHandlers();

            // 开始监控手柄
            _controllerService.StartMonitoring();
        }

        private void RegisterEventHandlers()
        {
            // 手柄连接状态变化
            _controllerService.ConnectionStatusChanged += (connected) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    ControllerStatusLabel.Content = connected ? "Connected" : "Unconnected";
                    ControllerStatusLabel.Foreground = connected ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
                    
                    // 连接成功时震动提示
                    if (connected)
                    {
                        _controllerService.SetVibration(30000, 30000);
                        DispatcherTimer vibrateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
                        vibrateTimer.Tick += (s, e) =>
                        {
                            _controllerService.SetVibration(0, 0);
                            vibrateTimer.Stop();
                        };
                        vibrateTimer.Start();
                    }
                });
            };

            // 摇杆移动
            _controllerService.LeftStickMoved += (x, y) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    _mouseService.MoveMouse(x, y);
                });
            };

            int triggerThreshold = 20;

            // 左扳机键 (滚轮上)
            _controllerService.LeftTriggerChanged += (value) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (value > triggerThreshold) // 阈值
                    {
                        MouseService.ScrollWheel((int)MathTool.MapValueClamped(value - triggerThreshold, triggerThreshold, 255f, 0f, 80f)); // 上滚
                    }
                });
            };

            // 右扳机键 (滚轮下)
            _controllerService.RightTriggerChanged += (value) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (value > triggerThreshold) // 阈值
                    {
                        MouseService.ScrollWheel(-(int)MathTool.MapValueClamped(value - triggerThreshold, triggerThreshold, 255f, 0f, 80f)); // 下滚
                    }
                });
            };

            // A键 (左键)
            _controllerService.AButtonStateChanged += (pressed) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (pressed)
                        MouseService.LeftButtonDown();
                    else
                        MouseService.LeftButtonUp();
                });
            };

            // L键 (左键)
            _controllerService.LButtonStateChanged += (pressed) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (pressed)
                        MouseService.LeftButtonDown();
                    else
                        MouseService.LeftButtonUp();
                });
            };

            // B键 (右键)
            _controllerService.BButtonStateChanged += (pressed) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (pressed)
                        MouseService.RightButtonDown();
                    else
                        MouseService.RightButtonUp();
                });
            };

            // X键 (中键)
            _controllerService.XButtonStateChanged += (pressed) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (pressed)
                        MouseService.MiddleButtonDown();
                    else
                        MouseService.MiddleButtonUp();
                });
            };

            // LB键 (精确模式)
            _controllerService.LBStateChanged += (pressed) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (pressed)
                        _mouseService.SensitivityFactor = 1 / 3f;
                    else
                        _mouseService.SensitivityFactor = 1f;
                });
            };

            // RB键 (快速模式)
            _controllerService.RBStateChanged += (pressed) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (pressed)
                        _mouseService.SensitivityFactor = 3f;
                    else
                        _mouseService.SensitivityFactor = 1f;
                });
            };

            // 屏幕变化
            _screenService.CurrentScreenChanged += (screen) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    CurrentScreenLabel.Content = screen.DeviceName;
                });
            };
        }

        private void MousePositionTimer_Tick(object sender, EventArgs e)
        {
            // 更新鼠标位置显示
            System.Drawing.Point pos = MouseService.GetCurrentPosition();
            MousePositionLabel.Content = $"({pos.X}, {pos.Y})";
        }

        private void SensitivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mouseService != null && SensitivityValueLabel != null)
            {
                _mouseService.Sensitivity = (float)e.NewValue;
                SensitivityValueLabel.Content = e.NewValue.ToString("0.0");
            }
        }

        private void DeadZoneSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mouseService != null && DeadZoneValueLabel != null)
            {
                _mouseService.DeadZone = (float)e.NewValue;
                DeadZoneValueLabel.Content = e.NewValue.ToString("0.0");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _controllerService.Dispose();
            _mousePositionTimer.Stop();
        }
    }
}
