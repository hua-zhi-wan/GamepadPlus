namespace AnotherGamepadPlus.Helpers
{
    /// <summary>
    /// 版本信息集中管理类
    /// </summary>
    public static class VersionInfo
    {
        // 在这里统一修改版本号
        public const string Major = "1";
        public const string Minor = "0";
        public const string Patch = "0";

        /// <summary>
        /// 完整版本号（格式：主版本.次版本.修订号）
        /// </summary>
        public static string FullVersion => $"{Major}.{Minor}.{Patch}";

        /// <summary>
        /// 带前缀的版本号（格式：v主版本.次版本.修订号）
        /// </summary>
        public static string VersionWithPrefix => $"v{FullVersion}";
    }
}
