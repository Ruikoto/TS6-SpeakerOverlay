using System;
using System.IO;
using System.Text.Json;
using TS6_SpeakerOverlay.Models;

namespace TS6_SpeakerOverlay.Services
{
    public static class ConfigService
    {
        private const string CONFIG_FILE = "config.json";

        public static AppConfig Load()
        {
            if (!File.Exists(CONFIG_FILE))
            {
                return new AppConfig(); // 如果文件不存在，返回默认配置
            }

            try
            {
                string json = File.ReadAllText(CONFIG_FILE);
                var config = JsonSerializer.Deserialize<AppConfig>(json);
                return config ?? new AppConfig();
            }
            catch
            {
                return new AppConfig(); // 解析失败也返回默认
            }
        }

        public static void Save(AppConfig config)
        {
            try
            {
                // WriteIndented = true 让生成的 JSON 有换行和缩进，方便用户手动修改
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] Save failed: {ex.Message}");
            }
        }
    }
}