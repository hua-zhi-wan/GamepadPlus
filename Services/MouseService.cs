using System;
using System.Drawing;
using System.Windows.Forms;
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
            // 应用死区过滤
            float magnitude = MathF.Sqrt(xDelta * xDelta + yDelta * yDelta);
            if (magnitude < _deadZone) return;

            // 重新计算死区映射
            float scale = (magnitude - _deadZone) / (1.0f - _deadZone);
            xDelta = xDelta / magnitude * scale;
            yDelta = yDelta / magnitude * scale;

            // 获取当前鼠标位置
            var currentPos = Cursor.Position;

            // 计算新位置 Y轴反转以符合直觉
            int newX = currentPos.X + (int)(xDelta * _sensitivity * _sensitivity_factor);
            int newY = currentPos.Y - (int)(yDelta * _sensitivity * _sensitivity_factor);

            // 调整位置以适应多屏幕
            var adjustedPos = _screenService.AdjustPositionToScreens(new Point(newX, newY));

            // 设置新位置
            NativeMethods.SetCursorPos(adjustedPos.X, adjustedPos.Y);
        }

        public static void LeftButtonDown()
        {
            NativeMethods.mouse_event(Constants.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        }

        public static void LeftButtonUp()
        {
            NativeMethods.mouse_event(Constants.MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }

        public static void RightButtonDown()
        {
            NativeMethods.mouse_event(Constants.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
        }

        public static void RightButtonUp()
        {
            NativeMethods.mouse_event(Constants.MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
        }
        public static void MiddleButtonDown()
        {
            NativeMethods.mouse_event(Constants.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, UIntPtr.Zero);
        }

        public static void MiddleButtonUp()
        {
            NativeMethods.mouse_event(Constants.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, UIntPtr.Zero);
        }

        public static void ScrollWheel(int delta)
        {
            NativeMethods.mouse_event(Constants.MOUSEEVENTF_WHEEL, 0, 0, (uint)delta, UIntPtr.Zero);
        }

        public static Point GetCurrentPosition()
        {
            return Cursor.Position;
        }
    }
}
