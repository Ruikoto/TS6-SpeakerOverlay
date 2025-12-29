using System;
using System.IO;
using System.Linq; // 必须引用
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes; // 必须引用
using System.Reactive.Linq;
using System.Threading.Tasks;
using Websocket.Client;
using TS6_SpeakerOverlay.Models; // 必须引用

namespace TS6_SpeakerOverlay.Services
{
    public class Ts6Service
    {
        private const string URL = "ws://127.0.0.1:5899"; 
        private const string KEY_FILE = "apikey.txt"; 
        private WebsocketClient _client;
        private string _savedApiKey = "";

        // 事件定义
        public event Action<List<User>, string>? OnChannelListUpdated;
        public event Action<int, bool>? OnTalkStatusChanged;

        public Ts6Service()
        {
            LoadApiKey(); 

            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket());
            _client = new WebsocketClient(new Uri(URL), factory);
            _client.ReconnectTimeout = TimeSpan.FromSeconds(5);
            
            _client.ReconnectionHappened.Subscribe(info =>
            {
                Console.WriteLine($"[WS] 🟢 已连接 ({info.Type})");
                SendAuth();
            });

            _client.MessageReceived.Subscribe(msg => HandleMessage(msg.Text));
        }

        public async Task StartAsync() => await _client.Start();

        private void LoadApiKey()
        {
            if (File.Exists(KEY_FILE))
            {
                _savedApiKey = File.ReadAllText(KEY_FILE).Trim();
                Console.WriteLine($"[Config] 读取到保存的 Key: {_savedApiKey}");
            }
        }

        private void SendAuth()
        {
            Console.WriteLine("[WS] 📤 发送认证...");
            var auth = new AuthRequest();
            if (!string.IsNullOrEmpty(_savedApiKey))
            {
                auth.Payload.Content.ApiKey = _savedApiKey;
            }
            
            string json = JsonSerializer.Serialize(auth);
            _client.Send(json);
        }

        private void HandleMessage(string? json)
        {
            if (string.IsNullOrEmpty(json)) return;

            try 
            {
                var node = JsonNode.Parse(json);
                string? type = node?["type"]?.ToString();

                switch (type)
                {
                    case "auth":
                        HandleAuthResponse(node);
                        break;
                    case "talkStatusChanged":
                        HandleTalkStatus(node);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] 解析失败: {ex.Message}");
            }
        }

        // --- 这里是刚才修改过的地方 ---
        private void HandleAuthResponse(JsonNode? node)
        {
            var payload = node?["payload"];
            if (payload == null) return;

            // 1. 保存 Key
            var newKey = payload["apiKey"]?.ToString();
            if (!string.IsNullOrEmpty(newKey) && newKey != _savedApiKey)
            {
                _savedApiKey = newKey;
                File.WriteAllText(KEY_FILE, newKey);
                Console.WriteLine("[Auth] ✅ 新 Key 已保存。");
            }

            // 2. 解析连接信息
            var connections = payload["connections"]?.AsArray();
            if (connections == null || connections.Count == 0) return;

            var conn0 = connections[0];
            int myClientId = conn0?["clientId"]?.GetValue<int>() ?? 0;
            
            // 3. 解析用户列表
            var clientInfos = conn0?["clientInfos"]?.AsArray();
            if (clientInfos == null) return;

            var allUsers = new List<User>();
            string myChannelId = "";

            foreach (var client in clientInfos)
            {
                int id = client["id"]?.GetValue<int>() ?? 0;
                
                // === 关键修正：从 properties 获取 nickname ===
                string name = client["properties"]?["nickname"]?.ToString() ?? "Unknown";
                // ==========================================

                string chId = client["channelId"]?.ToString() ?? "";
                bool isTalking = client["properties"]?["flagTalking"]?.GetValue<bool>() ?? false;

                if (id == myClientId) myChannelId = chId;

                allUsers.Add(new User 
                { 
                    ClientId = id,
                    Name = name,
                    ChannelId = chId, 
                    IsTalking = isTalking
                });
            }

            Console.WriteLine($"[Info] 我 ({myClientId}) 在频道: {myChannelId}, 共有 {allUsers.Count} 人");
            OnChannelListUpdated?.Invoke(allUsers, myChannelId);
        }

        private void HandleTalkStatus(JsonNode? node)
        {
            var payload = node?["payload"];
            if (payload == null) return;

            int clientId = payload["clientId"]?.GetValue<int>() ?? 0;
            int status = payload["status"]?.GetValue<int>() ?? 0;
            bool isTalking = (status == 1); 

            OnTalkStatusChanged?.Invoke(clientId, isTalking);
        }
    }
}