using System.Text.Json.Serialization;

namespace TS6_SpeakerOverlay.Models
{
    // 基础消息外壳
    public class Ts6Message
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("payload")]
        public System.Text.Json.Nodes.JsonNode? Payload { get; set; }
    }

    // 认证请求 (发送给 TS6)
    public class AuthRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "auth";

        [JsonPropertyName("payload")]
        public AuthPayload Payload { get; set; } = new();
    }

    public class AuthPayload
    {
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; } = "com.ts6.overlay";

        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0.0";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "Speaker Overlay";
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = "A lightweight overlay for gamers.";
        
        [JsonPropertyName("content")]
        public AuthContent Content { get; set; } = new();
    }

    public class AuthContent
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = ""; // 第一次为空，认证后保存
    }
}