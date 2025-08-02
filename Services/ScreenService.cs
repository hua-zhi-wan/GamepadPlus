using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AnotherGamepadPlus.Services
{
    public class ScreenService
    {
        private readonly List<Screen> _screens;
        private Screen _currentScreen;

        public event Action<Screen> CurrentScreenChanged;

        public Screen CurrentScreen
        {
            get => _currentScreen;
            private set
            {
                if (_currentScreen != value)
                {
                    _currentScreen = value;
                    CurrentScreenChanged?.Invoke(value);
                }
            }
        }

        public ScreenService()
        {
            _screens = Screen.AllScreens.ToList();
            _currentScreen = Screen.PrimaryScreen;
        }

        // 调整鼠标位置以适应多屏幕布局
        public Point AdjustPositionToScreens(Point desiredPosition)
        {
            // 找到当前应该所在的屏幕
            CurrentScreen = _screens.FirstOrDefault(s => s.Bounds.Contains(desiredPosition)) 
                          ?? _screens.FirstOrDefault(s => s.Bounds.IntersectsWith(new Rectangle(desiredPosition, Size.Empty)))
                          ?? Screen.PrimaryScreen;

            // 计算虚拟桌面边界
            var virtualBounds = GetVirtualScreenBounds();
            
            // 限制在虚拟桌面范围内
            int constrainedX = Math.Clamp(desiredPosition.X, virtualBounds.Left, virtualBounds.Right - 1);
            int constrainedY = Math.Clamp(desiredPosition.Y, virtualBounds.Top, virtualBounds.Bottom - 1);

            return new Point(constrainedX, constrainedY);
        }

        // 获取所有屏幕组合的虚拟边界
        public Rectangle GetVirtualScreenBounds()
        {
            if (!_screens.Any())
                return Screen.PrimaryScreen.Bounds;

            int minX = _screens.Min(s => s.Bounds.Left);
            int minY = _screens.Min(s => s.Bounds.Top);
            int maxX = _screens.Max(s => s.Bounds.Right);
            int maxY = _screens.Max(s => s.Bounds.Bottom);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public IEnumerable<Screen> GetAllScreens() => _screens.AsReadOnly();
    }
}
