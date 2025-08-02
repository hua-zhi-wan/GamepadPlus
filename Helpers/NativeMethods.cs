using System;
using System.Runtime.InteropServices;

namespace AnotherGamepadPlus.Helpers
{
    internal static class NativeMethods
    {
        // 鼠标位置设置
        [DllImport("user32.dll")]
        internal static extern bool SetCursorPos(int x, int y);

        // 鼠标事件
        [DllImport("user32.dll")]
        internal static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);

        // XInput相关函数 - 替代第三方库
        [DllImport("xinput1_4.dll")]
        internal static extern uint XInputGetState(uint dwUserIndex, out XInputState pState);

        [DllImport("xinput1_4.dll")]
        internal static extern uint XInputSetState(uint dwUserIndex, ref XInputVibration pVibration);
    }

    // XInput结构体定义
    [StructLayout(LayoutKind.Sequential)]
    public struct XInputState
    {
        public uint dwPacketNumber;
        public XInputGamepad Gamepad;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XInputGamepad
    {
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XInputVibration
    {
        public ushort wLeftMotorSpeed;
        public ushort wRightMotorSpeed;
    }
}
