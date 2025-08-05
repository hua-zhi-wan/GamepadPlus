using System.Runtime.InteropServices;
using AnotherGamepadPlus.Helpers;

namespace AnotherGamepadPlus.Services
{
    public class MouseService
    {
        private readonly ScreenService _screenService;
        private float _sensitivity = 10.0f;
        private float _sensitivity_factor = 1.0f;
        private float _deadZone = 0.1f;

        public float Sensitivity
        {
            get => _sensitivity;
            set => _sensitivity = Math.Max(1f, Math.Min(30.0f, value));
        }

        public float DeadZone
        {
            get => _deadZone;
            set => _deadZone = Math.Max(0.0f, Math.Min(0.5f, value));
        }

        public float SensitivityFactor
        {
            get => _sensitivity_factor;
            set => _sensitivity_factor = Math.Max(0f, Math.Min(5f, value));
        }

        public MouseService(ScreenService screenService)
        {
            _screenService = screenService;
        }

        public void MoveMouse(float xDelta, float yDelta)
        {
            // 原有移动逻辑保持不变
            float magnitude = MathF.Sqrt(xDelta * xDelta + yDelta * yDelta);
            if (magnitude < _deadZone) return;

            float scale = (magnitude - _deadZone) / (1.0f - _deadZone);
            xDelta = xDelta / magnitude * scale;
            yDelta = yDelta / magnitude * scale;

            var currentPos = Cursor.Position;
            int newX = currentPos.X + (int)(xDelta * _sensitivity * _sensitivity_factor);
            int newY = currentPos.Y - (int)(yDelta * _sensitivity * _sensitivity_factor);
            var adjustedPos = _screenService.AdjustPositionToScreens(new Point(newX, newY));
            NativeMethods.SetCursorPos(adjustedPos.X, adjustedPos.Y);
        }

        // 构造鼠标输入事件
        private static void SendMouseInput(MouseEventFlags flags, uint data = 0)
        {
            var input = new INPUT[]
            {
                new INPUT
                {
                    type = InputType.INPUT_MOUSE,
                    mi = new MouseInputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            dx = 0,
                            dy = 0,
                            mouseData = data,
                            dwFlags = flags,
                            time = 0,
                            dwExtraInfo = UIntPtr.Zero
                        }
                    }
                }
            };

            NativeMethods.SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void LeftButtonDown()
        {
            SendMouseInput(MouseEventFlags.MOUSEEVENTF_LEFTDOWN);
        }

        public static void LeftButtonUp()
        {
            SendMouseInput(MouseEventFlags.MOUSEEVENTF_LEFTUP);
        }

        public static void RightButtonDown()
        {
            SendMouseInput(MouseEventFlags.MOUSEEVENTF_RIGHTDOWN);
        }

        public static void RightButtonUp()
        {
            SendMouseInput(MouseEventFlags.MOUSEEVENTF_RIGHTUP);
        }

        public static void MiddleButtonDown()
        {
            SendMouseInput(MouseEventFlags.MOUSEEVENTF_MIDDLEDOWN);
        }

        public static void MiddleButtonUp()
        {
            SendMouseInput(MouseEventFlags.MOUSEEVENTF_MIDDLEUP);
        }

        public static void ScrollWheel(int delta)
        {
            // 滚轮值以120为单位
            SendMouseInput(MouseEventFlags.MOUSEEVENTF_WHEEL, (uint)delta);
        }

        public static Point GetCurrentPosition()
        {
            return Cursor.Position;
        }
    }
}
