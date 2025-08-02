using System;
using System.Threading;
using System.Threading.Tasks;
using AnotherGamepadPlus.Helpers;

namespace AnotherGamepadPlus.Services
{
    public class ControllerService : IDisposable
    {
        private readonly uint _controllerIndex = 0; // 默认使用第一个手柄
        private bool _isRunning;
        private CancellationTokenSource _cts;
        private Task _pollingTask;

        // 事件定义
        public event Action<bool> ConnectionStatusChanged;
        public event Action<float, float> LeftStickMoved;
        public event Action<byte> LeftTriggerChanged;
        public event Action<byte> RightTriggerChanged;
        public event Action<bool> AButtonStateChanged;
        public event Action<bool> BButtonStateChanged;
        public event Action<bool> XButtonStateChanged;
        public event Action<bool> YButtonStateChanged;
        public event Action<bool> LBStateChanged;
        public event Action<bool> RBStateChanged;
        public event Action<bool> LButtonStateChanged;

        // 状态跟踪
        private bool _isConnected;
        private bool _aButtonPressed;
        private bool _bButtonPressed;
        private bool _xButtonPressed;
        private bool _lbPressed;
        private bool _rbPressed;
        private bool _lButtonPressed;

        public void StartMonitoring()
        {
            _isRunning = true;
            _cts = new CancellationTokenSource();
            _pollingTask = Task.Run(PollControllerState, _cts.Token);
        }

        public void StopMonitoring()
        {
            _isRunning = false;
            _cts?.Cancel();
        }

        public void SetVibration(ushort leftMotor, ushort rightMotor)
        {
            if (!_isConnected) return;

            var vibration = new XInputVibration
            {
                wLeftMotorSpeed = leftMotor,
                wRightMotorSpeed = rightMotor
            };

            NativeMethods.XInputSetState(_controllerIndex, ref vibration);
        }

        private void PollControllerState()
        {
            while (_isRunning)
            {
                var result = NativeMethods.XInputGetState(_controllerIndex, out var state);
                var isConnected = result == 0;

                // 连接状态变化
                if (isConnected != _isConnected)
                {
                    _isConnected = isConnected;
                    ConnectionStatusChanged?.Invoke(isConnected);
                }

                if (isConnected)
                {
                    // 处理左摇杆
                    var lx = NormalizeThumbValue(state.Gamepad.sThumbLX);
                    var ly = NormalizeThumbValue(state.Gamepad.sThumbLY);
                    LeftStickMoved?.Invoke(lx, ly);

                    // 处理扳机键
                    LeftTriggerChanged?.Invoke(state.Gamepad.bLeftTrigger);
                    RightTriggerChanged?.Invoke(state.Gamepad.bRightTrigger);

                    // 处理A按钮
                    var aPressed = (state.Gamepad.wButtons & Constants.XINPUT_GAMEPAD_A) != 0;
                    if (aPressed != _aButtonPressed)
                    {
                        _aButtonPressed = aPressed;
                        AButtonStateChanged?.Invoke(aPressed);
                    }

                    // 处理B按钮
                    var bPressed = (state.Gamepad.wButtons & Constants.XINPUT_GAMEPAD_B) != 0;
                    if (bPressed != _bButtonPressed)
                    {
                        _bButtonPressed = bPressed;
                        BButtonStateChanged?.Invoke(bPressed);
                    }

                    // 处理X按钮
                    var xPressed = (state.Gamepad.wButtons & Constants.XINPUT_GAMEPAD_X) != 0;
                    if (xPressed != _xButtonPressed)
                    {
                        _xButtonPressed = xPressed;
                        XButtonStateChanged?.Invoke(xPressed);
                    }

                    // 处理LB按钮
                    var lbPressed = (state.Gamepad.wButtons & Constants.XINPUT_GAMEPAD_LEFT_SHOULDER) != 0;
                    if (lbPressed != _lbPressed)
                    {
                        _lbPressed = lbPressed;
                        LBStateChanged?.Invoke(lbPressed);
                    }

                    // 处理RB按钮
                    var rbPressed = (state.Gamepad.wButtons & Constants.XINPUT_GAMEPAD_RIGHT_SHOULDER) != 0;
                    if (rbPressed != _rbPressed)
                    {
                        _rbPressed = rbPressed;
                        RBStateChanged?.Invoke(rbPressed);
                    }

                    // 处理L按钮
                    var lButtonPressed = (state.Gamepad.wButtons & Constants.XINPUT_GAMEPAD_LEFT_THUMB) != 0;
                    if (lButtonPressed != _lButtonPressed)
                    {
                        _lButtonPressed = lButtonPressed;
                        LButtonStateChanged?.Invoke(lButtonPressed);
                    }
                }

                // 控制 polling 频率 (约100Hz)
                Thread.Sleep(10);
            }
        }

        // 将摇杆值标准化到 [-1.0, 1.0] 范围
        private float NormalizeThumbValue(short value)
        {
            if (value == 0) return 0;
            return (float)value / (value > 0 ? Constants.MAX_THUMB_VALUE : -Constants.MIN_THUMB_VALUE);
        }

        public void Dispose()
        {
            StopMonitoring();
            _cts?.Dispose();
        }
    }
}
