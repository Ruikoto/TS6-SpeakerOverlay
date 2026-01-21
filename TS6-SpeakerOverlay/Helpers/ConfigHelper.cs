using System.IO;

namespace TS6_SpeakerOverlay.Helpers
{
    /// <summary>
    /// 配置文件管理帮助类
    /// </summary>
    public static class ConfigHelper
    {
        private const string AppName = "TS6-SpeakerOverlay";
        private const string ApiKeyFileName = "apikey.txt";

        /// <summary>
        /// 获取应用程序配置文件夹路径
        /// 位置：%APPDATA%\TS6-SpeakerOverlay
        /// </summary>
        public static string ConfigFolderPath
        {
            get
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var configPath = Path.Combine(appDataPath, AppName);

                // 确保文件夹存在
                if (!Directory.Exists(configPath))
                {
                    Directory.CreateDirectory(configPath);
                    Console.WriteLine($"[Config] 创建配置文件夹: {configPath}");
                }

                return configPath;
            }
        }

        /// <summary>
        /// 获取 API Key 文件的完整路径
        /// </summary>
        public static string ApiKeyFilePath => Path.Combine(ConfigFolderPath, ApiKeyFileName);

        /// <summary>
        /// 读取 API Key
        /// </summary>
        public static string? LoadApiKey()
        {
            try
            {
                if (File.Exists(ApiKeyFilePath))
                {
                    var key = File.ReadAllText(ApiKeyFilePath).Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        Console.WriteLine($"[Config] 从 {ApiKeyFilePath} 读取到 API Key");
                        return key;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] 读取 API Key 失败: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 保存 API Key
        /// </summary>
        public static void SaveApiKey(string apiKey)
        {
            try
            {
                File.WriteAllText(ApiKeyFilePath, apiKey);
                Console.WriteLine($"[Config] API Key 已保存到: {ApiKeyFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] 保存 API Key 失败: {ex.Message}");
            }
        }
    }
}
