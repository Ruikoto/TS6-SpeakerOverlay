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
        private System.Timers.Timer? _heartbeatTimer;
        private bool _hasEverConnected;
        private bool _isReconnectingForRefresh;

        // 事件定义
        public event Action<List<User>, string, int>? OnChannelListUpdated;
        public event Action<int, bool>? OnTalkStatusChanged;
        public event Action<int, string>? OnClientMoved;
        public event Action<User>? OnClientEnterView;
        public event Action<int>? OnClientLeftView;
        public event Action<int, string>? OnClientPropertiesUpdated;
        public event Action? OnConnected;
        public event Action? OnDisconnected;
        public event Action? OnServerConnecting;

        public Ts6Service()
        {
            LoadApiKey();

            var factory = new Func<ClientWebSocket>(() =>
            {
                var ws = new ClientWebSocket();
                ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
                return ws;
            });

            _client = new WebsocketClient(new Uri(Url), factory);
            _client.ReconnectTimeout = null; // Disable auto-disconnect detection
            _client.ErrorReconnectTimeout = TimeSpan.FromSeconds(2); // Fast reconnection
            _client.IsReconnectionEnabled = true;

            _client.ReconnectionHappened.Subscribe(info =>
            {
                Console.WriteLine($"[WS] 🟢 已连接 ({info.Type})");

                // Only notify on initial connection
                if (info.Type == ReconnectionType.Initial)
                {
                    OnConnected?.Invoke();
                }

                SendAuth();
                StartHeartbeat();
            });

            _client.DisconnectionHappened.Subscribe(info =>
            {
                // Don't log or notify if this is an intentional reconnect for refresh
                if (_isReconnectingForRefresh)
                {
                    return;
                }

                Console.WriteLine($"[WS] 🔴 连接断开 ({info.Type}) - 将自动重连...");

                if (_hasEverConnected)
                {
                    OnDisconnected?.Invoke();
                }

                StopHeartbeat();
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
                    case "clientPropertiesUpdated":
                        HandleClientPropertiesUpdated(node);
                        break;
                    case "connectStatusChanged":
                        HandleConnectStatusChanged(node);
                        break;
                    case "clientSelfPropertyUpdated":
                        // Ignore self property updates
                        break;
                    case "clientChannelGroupChanged":
                    case "channelPropertiesUpdated":
                    case "log":
                    case "groupInfo":
                    case "neededPermissions":
                    case "serverPropertiesUpdated":
                    case "channels":
                    case "channelsSubscribed":
                    case "getScreenshareSupport":
                    case "soundDeviceListChanged":
                    case "ffmpegCodersChanged":
                        // Ignore these events
                        break;
                    default:
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

            // Save API Key
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

            // Parse connections
            var connections = payload["connections"]?.AsArray();
            if (connections == null || connections.Count == 0)
            {
                Console.WriteLine("[Auth] TS6 未连接到服务器，等待连接...");
                _hasEverConnected = true;
                return;
            }

            var conn0 = connections[0];
            var myClientId = conn0?["clientId"]?.GetValue<int>() ?? 0;

            // Parse user list
            var clientInfos = conn0?["clientInfos"]?.AsArray();
            if (clientInfos == null)
            {
                _hasEverConnected = true;
                return;
            }

            var allUsers = new List<User>();
            var myChannelId = "";

            foreach (var client in clientInfos)
            {
                if (client == null) continue;
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
            _hasEverConnected = true;
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

        private void HandleClientPropertiesUpdated(JsonNode? node)
        {
            var payload = node?["payload"];
            if (payload == null) return;

            var clientId = payload["clientId"]?.GetValue<int>() ?? 0;
            var properties = payload["properties"];

            if (properties == null) return;
            var nickname = properties["nickname"]?.ToString();
            if (!string.IsNullOrEmpty(nickname))
            {
                OnClientPropertiesUpdated?.Invoke(clientId, nickname);
            }
        }

        private void HandleConnectStatusChanged(JsonNode? node)
        {
            var payload = node?["payload"];
            if (payload == null) return;

            var status = payload["status"]?.GetValue<int>() ?? -1;

            // Status codes: 0=disconnected, 1=connecting, 2=connected, 3=initializing, 4=connected_and_ready
            var statusName = status switch
            {
                0 => "disconnected",
                1 => "connecting",
                2 => "connected",
                3 => "initializing",
                4 => "connected_and_ready",
                _ => $"unknown({status})"
            };

            Console.WriteLine($"[WS] TS6 连接状态变化: {statusName} ({status})");

            if (status == 1)
            {
                Console.WriteLine("[WS] TS6 开始连接新服务器，准备清空缓存...");
                OnServerConnecting?.Invoke();
            }
            else if (status == 4 && !_isReconnectingForRefresh)
            {
                Console.WriteLine("[WS] TS6 已完全连接，将重连 WebSocket 以刷新用户列表...");

                // Force reconnect to get fresh auth response with all user info
                _isReconnectingForRefresh = true;

                Task.Run(async () =>
                {
                    await Task.Delay(500);
                    Console.WriteLine("[WS] 🔄 主动重连以刷新数据...");
                    await _client.Reconnect();

                    await Task.Delay(1000);
                    _isReconnectingForRefresh = false;
                });
            }
        }

        private void StartHeartbeat()
        {
            StopHeartbeat();

            _heartbeatTimer = new System.Timers.Timer(15000); // 15 seconds
            _heartbeatTimer.Elapsed += (_, _) => SendHeartbeat();
            _heartbeatTimer.AutoReset = true;
            _heartbeatTimer.Start();
        }

        private void StopHeartbeat()
        {
            if (_heartbeatTimer == null) return;
            _heartbeatTimer.Stop();
            _heartbeatTimer.Dispose();
            _heartbeatTimer = null;
        }

        private void SendHeartbeat()
        {
            try
            {
                if (!_client.IsRunning) return;
                var ping = new { type = "ping" };
                var json = JsonSerializer.Serialize(ping);
                _client.Send(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Heartbeat] 发送失败: {ex.Message}");
            }
        }
    }
}