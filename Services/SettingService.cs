using System.IO;
using System.Text.Json;

namespace AnotherGamepadPlus.Helpers
{
    public class SettingService
    {
        // 配置文件路径（保存在应用程序数据目录）
        private readonly string _configPath;

        // 默认配置
        private readonly Settings _defaultSettings = new Settings
        {
            DeadZone = 0.1f,
            Sensitivity = 10.0f,
        };

        public SettingService()
        {
            // 获取应用程序数据目录
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "AnotherGamepadPlus");

            // 确保目录存在
            Directory.CreateDirectory(appFolder);

            // 配置文件完整路径
            _configPath = Path.Combine(appFolder, "settings.json");
        }

        // 加载保存的设置，如果没有则返回默认设置
        public Settings LoadSettings()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    return JsonSerializer.Deserialize<Settings>(json) ?? _defaultSettings;
                }
            }
            catch (Exception ex)
            {
                // 处理读取错误，如文件损坏等
                Console.WriteLine($"Load Settings Failed: {ex.Message}");
            }

            // 返回默认设置
            return _defaultSettings;
        }

        // 保存设置到文件
        public void SaveSettings(Settings settings)
        {
            try
            {
                // 确保设置值在有效范围内
                settings.DeadZone = Math.Clamp(settings.DeadZone, 0.0f, 0.5f);
                settings.Sensitivity = Math.Clamp(settings.Sensitivity, 1.0f, 30.0f);

                // 序列化并保存
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true, // 格式化输出，便于人工编辑
                    IncludeFields = true
                });

                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save Settings Failed: {ex.Message}");
            }
        }
    }

    // 设置数据模型
    public class Settings
    {
        public float DeadZone { get; set; }
        public float Sensitivity { get; set; }
    }
}
