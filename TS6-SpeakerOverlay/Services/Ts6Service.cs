using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using Websocket.Client;
using TS6_SpeakerOverlay.Models;
using TS6_SpeakerOverlay.Helpers;

namespace TS6_SpeakerOverlay.Services
{
    public class Ts6Service
    {
        private const string Url = "ws://127.0.0.1:5899";
        private readonly WebsocketClient _client;
        private string _savedApiKey = "";

        // 事件定义
        public event Action<List<User>, string, int>? OnChannelListUpdated; // 参数：所有用户, 我的频道ID, 我的客户端ID
        public event Action<int, bool>? OnTalkStatusChanged;
        public event Action<int, string>? OnClientMoved; // 用户移动到新频道
        public event Action<User>? OnClientEnterView; // 新用户进入视野
        public event Action<int>? OnClientLeftView; // 用户离开视野

        public Ts6Service()
        {
            LoadApiKey();

            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket());
            _client = new WebsocketClient(new Uri(Url), factory);
            _client.ReconnectTimeout = TimeSpan.FromSeconds(10); // 增加重连间隔，避免频繁重试
            _client.ErrorReconnectTimeout = TimeSpan.FromSeconds(30); // 错误后等待更久再重连

            _client.ReconnectionHappened.Subscribe(info =>
            {
                Console.WriteLine($"[WS] 🟢 已连接 ({info.Type})");
                SendAuth();
            });

            _client.DisconnectionHappened.Subscribe(info =>
            {
                Console.WriteLine($"[WS] 🔴 连接断开 ({info.Type}) - 将自动重连...");
            });

            _client.MessageReceived.Subscribe(msg => HandleMessage(msg.Text));
        }

        public async Task StartAsync() => await _client.Start();

        private void LoadApiKey()
        {
            var key = ConfigHelper.LoadApiKey();
            if (key != null)
            {
                _savedApiKey = key;
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

            var json = JsonSerializer.Serialize(auth);
            _client.Send(json);
        }

        private void HandleMessage(string? json)
        {
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var node = JsonNode.Parse(json);
                var type = node?["type"]?.ToString();

                switch (type)
                {
                    case "auth":
                        HandleAuthResponse(node);
                        break;
                    case "talkStatusChanged":
                        HandleTalkStatus(node);
                        break;
                    case "clientMoved":
                        HandleClientMoved(node);
                        break;
                    case "clientEnterView":
                        HandleClientEnterView(node);
                        break;
                    case "clientLeftView":
                        HandleClientLeftView(node);
                        break;
                    default:
                        // 输出未处理的消息类型用于调试
                        Console.WriteLine($"[WS] 未处理的消息类型: {type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] 解析失败: {ex.Message}");
            }
        }

        private void HandleAuthResponse(JsonNode? node)
        {
            var payload = node?["payload"];
            if (payload == null) return;

            // 保存 API Key
            var apiKeyNode = payload["apiKey"];
            if (apiKeyNode != null)
            {
                var newKey = apiKeyNode.ToString();
                if (!string.IsNullOrEmpty(newKey) && newKey != _savedApiKey)
                {
                    _savedApiKey = newKey;
                    ConfigHelper.SaveApiKey(newKey);
                }
            }

            // 解析连接信息
            var connections = payload["connections"]?.AsArray();
            if (connections == null || connections.Count == 0) return;

            var conn0 = connections[0];
            var myClientId = conn0?["clientId"]?.GetValue<int>() ?? 0;

            // 解析用户列表
            var clientInfos = conn0?["clientInfos"]?.AsArray();
            if (clientInfos == null) return;

            var allUsers = new List<User>();
            var myChannelId = "";

            foreach (var client in clientInfos)
            {
                var id = client["id"]?.GetValue<int>() ?? 0;
                var name = client["properties"]?["nickname"]?.ToString() ?? "Unknown";
                var chId = client["channelId"]?.ToString() ?? "";
                var isTalking = client["properties"]?["flagTalking"]?.GetValue<bool>() ?? false;

                if (id == myClientId) myChannelId = chId;

                allUsers.Add(new User
                {
                    ClientId = id,
                    Name = name,
                    ChannelId = chId,
                    IsTalking = isTalking
                });
            }

            Console.WriteLine($"[Auth] 我的 ID: {myClientId}, 频道: {myChannelId}, 共 {allUsers.Count} 人");
            OnChannelListUpdated?.Invoke(allUsers, myChannelId, myClientId);
        }

        private void HandleTalkStatus(JsonNode? node)
        {
            var payload = node?["payload"];
            if (payload == null) return;

            var clientId = payload["clientId"]?.GetValue<int>() ?? 0;
            var status = payload["status"]?.GetValue<int>() ?? 0;
            var isTalking = (status == 1);

            OnTalkStatusChanged?.Invoke(clientId, isTalking);
        }

        private void HandleClientMoved(JsonNode? node)
        {
            var payload = node?["payload"];
            if (payload == null) return;

            var clientId = payload["clientId"]?.GetValue<int>() ?? 0;
            var newChannelId = payload["newChannelId"]?.ToString() ?? "";

            Console.WriteLine($"[WS] 用户 {clientId} 移动到频道: {newChannelId}");
            OnClientMoved?.Invoke(clientId, newChannelId);
        }

        private void HandleClientEnterView(JsonNode? node)
        {
            var payload = node?["payload"];
            if (payload == null) return;

            var clientId = payload["clientId"]?.GetValue<int>() ?? 0;
            var name = payload["clientNickname"]?.ToString() ?? "Unknown";
            var channelId = payload["clientChannelId"]?.ToString() ?? "";

            var newUser = new User
            {
                ClientId = clientId,
                Name = name,
                ChannelId = channelId,
                IsTalking = false
            };

            Console.WriteLine($"[WS] 新用户进入视野: {name} (ID: {clientId})");
            OnClientEnterView?.Invoke(newUser);
        }

        private void HandleClientLeftView(JsonNode? node)
        {
            var payload = node?["payload"];
            if (payload == null) return;

            var clientId = payload["clientId"]?.GetValue<int>() ?? 0;

            Console.WriteLine($"[WS] 用户离开视野: {clientId}");
            OnClientLeftView?.Invoke(clientId);
        }
    }
}