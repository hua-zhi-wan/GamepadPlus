using System.Windows;
using System.Windows.Threading;
using AnotherGamepadPlus.Helpers;
using AnotherGamepadPlus.Services;

namespace AnotherGamepadPlus.Views
{
    public partial class MainWindow : Window
    {
        private bool isPaused = false;
        private bool backPressedTime = false;
        private bool startPressedTime = false;

        private readonly ControllerService _controllerService;
        private readonly MouseService _mouseService;
        private readonly ScreenService _screenService;
        private readonly DispatcherTimer _mousePositionTimer;

        private readonly NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();

            // 初始化托盘图标
            _notifyIcon = new NotifyIcon
            {
                // 设置图标（需要添加一个图标文件到项目中，并设置为"资源"）
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location),

                // 设置托盘图标显示的文本
                Text = "AGP",

                // 允许显示托盘图标
                Visible = true
            };
            InitializeNotifyIcon();

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

        private void InitializeNotifyIcon()
        {

            // 创建右键菜单
            var contextMenu = new ContextMenuStrip();

            // 添加"显示"菜单项
            var showItem = new ToolStripMenuItem("Show / Hide to Tray");
            showItem.Click += ShowItem_Click;
            contextMenu.Items.Add(showItem);

            // 添加"暂停"菜单项
            var pauseItem = new ToolStripMenuItem("Pause / Resume");
            pauseItem.Click += PauseItem_Click;
            contextMenu.Items.Add(pauseItem);

            // 添加"退出"菜单项
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += ExitItem_Click;
            contextMenu.Items.Add(exitItem);

            // 关联右键菜单
            _notifyIcon.ContextMenuStrip = contextMenu;

            // 双击托盘图标显示窗口
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            // 设置窗口状态变化时的处理
            this.StateChanged += MainWindow_StateChanged;
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            // 当窗口最小化时
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
            else
            {
                this.ShowInTaskbar = true;
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowItem_Click(sender, e);
        }

        private void ShowItem_Click(object sender, EventArgs e)
        {
            // 如果窗口已最小化，则显示并恢复状态
            if (this.WindowState == WindowState.Minimized)
            {
                // 显示窗口并恢复状态
                ShowAndRestoreWindow();
            }
            else
            {
                // 如果窗口已显示，则最小化到托盘
                this.WindowState = WindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        private void PauseItem_Click(object sender, EventArgs e)
        {
            ChangePausedStatus();
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {

            // 关闭应用程序
            System.Windows.Application.Current.Shutdown();
        }

        private void ShowAndRestoreWindow()
        {
            // 显示窗口
            this.Show();

            // 恢复窗口状态（如果需要）
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }

            // 激活窗口
            this.Activate();
        }

        // // 重写关闭窗口方法，改为最小化到托盘
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 清理托盘图标
            _notifyIcon.Dispose();

            base.OnClosing(e);
        }

        void ChangePausedStatus()
        {
            isPaused = !isPaused;
            PausedStatusLabel.Content = isPaused ? "Paused" : "Running";
            PausedStatusLabel.Foreground = isPaused ? System.Windows.Media.Brushes.Orange : System.Windows.Media.Brushes.Blue;
            if (!isPaused)
            {
                _controllerService.Vibrate(30000, 30000, 200);
            }
        }

        private void RegisterEventHandlers()
        {
            // Back键状态
            _controllerService.BackButtonStateChanged += (pressed) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (pressed)
                        backPressedTime = true;
                    else
                        backPressedTime = false;
                    CheckPauseCombo();
                });
            };

            // Start键状态
            _controllerService.StartButtonStateChanged += (pressed) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (pressed)
                        startPressedTime = true;
                    else
                        startPressedTime = false;
                    CheckPauseCombo();
                });
            };

            void CheckPauseCombo()
            {
                if (backPressedTime && startPressedTime)
                {
                    backPressedTime = false;
                    startPressedTime = false;
                    ChangePausedStatus();
                }
            }

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
                        _controllerService.Vibrate(30000, 30000, 200);
                    }
                });
            };

            // 摇杆移动
            _controllerService.LeftStickMoved += (x, y) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (!isPaused)
                        _mouseService.MoveMouse(x, y);
                });
            };

            int triggerThreshold = 20;

            // 左扳机键 (滚轮上)
            _controllerService.LeftTriggerChanged += (value) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (!isPaused && value > triggerThreshold)
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
                    if (!isPaused && value > triggerThreshold)
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
                    if (!isPaused)
                    {
                        if (pressed)
                            MouseService.LeftButtonDown();
                        else
                            MouseService.LeftButtonUp();
                    }
                });
            };

            // L键 (左键)
            _controllerService.LButtonStateChanged += (pressed) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (!isPaused)
                    {
                        if (pressed)
                            MouseService.LeftButtonDown();
                        else
                            MouseService.LeftButtonUp();
                    }
                });
            };

            // B键 (右键)
            _controllerService.BButtonStateChanged += (pressed) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (!isPaused)
                    {
                        if (pressed)
                            MouseService.RightButtonDown();
                        else
                            MouseService.RightButtonUp();
                    }
                });
            };

            // X键 (中键)
            _controllerService.XButtonStateChanged += (pressed) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (!isPaused)
                    {
                        if (pressed)
                            MouseService.MiddleButtonDown();
                        else
                            MouseService.MiddleButtonUp();
                    }
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

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.ToString(),
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _controllerService.Dispose();
            _mousePositionTimer.Stop();
        }
    }
}
