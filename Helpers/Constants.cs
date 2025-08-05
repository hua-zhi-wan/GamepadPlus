namespace AnotherGamepadPlus.Helpers
{
    internal static class Constants
    {
        // 鼠标事件常量
        // public const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        // public const uint MOUSEEVENTF_LEFTUP = 0x04;
        // public const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        // public const uint MOUSEEVENTF_RIGHTUP = 0x10;
        // public const uint MOUSEEVENTF_MIDDLEDOWN = 0x20;
        // public const uint MOUSEEVENTF_MIDDLEUP = 0x40;
        // public const uint MOUSEEVENTF_WHEEL = 0x0800;

        // 手柄按钮常量
        public const ushort XINPUT_GAMEPAD_START = 0x0010;
        public const ushort XINPUT_GAMEPAD_BACK = 0x0020;
        public const ushort XINPUT_GAMEPAD_A = 0x1000;
        public const ushort XINPUT_GAMEPAD_B = 0x2000;
        public const ushort XINPUT_GAMEPAD_X = 0x4000;
        public const ushort XINPUT_GAMEPAD_Y = 0x8000;
        public const ushort XINPUT_GAMEPAD_LEFT_SHOULDER = 0x0100;
        public const ushort XINPUT_GAMEPAD_RIGHT_SHOULDER = 0x0200;
        public const ushort XINPUT_GAMEPAD_LEFT_THUMB = 0x0040;
        public const ushort XINPUT_GAMEPAD_RIGHT_THUMB = 0x0040;

        // 摇杆范围常量
        public const short MIN_THUMB_VALUE = -32768;
        public const short MAX_THUMB_VALUE = 32767;
    }
}
