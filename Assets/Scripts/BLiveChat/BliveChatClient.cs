using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class BliveChatClient : MonoBehaviour
{
    [Header("blivechat 本地地址")]
    public string wsUrl = "ws://127.0.0.1:12450/api/chat"; 

    [Header("你的B站直播间房间号")]
    public int roomID = 213; // ⚠️ 测试前务必改成真实的房间号！[Header("UI 绑定")]
    public GameObject chatItemPrefab; 
    public Transform contentTransform; 
    public ScrollRect scrollRect; 
    public int maxMessageCount = 50;

    private struct DanmuData
    {
        public string uid;
        public string username;
        public string message;
        public string avatarUrl; 
    }

    private ConcurrentQueue<DanmuData> danmakuQueue = new ConcurrentQueue<DanmuData>();
    private ClientWebSocket ws;
    private CancellationTokenSource cts;

    async void Start()
    {
        ws = new ClientWebSocket();
        cts = new CancellationTokenSource();
        
        try 
        {
            Debug.Log($"正在连接 blivechat: {wsUrl} ...");
            await ws.ConnectAsync(new Uri(wsUrl), cts.Token);
            
            // 💡 1. 核心关键：连上后，必须立刻告诉服务器我们要听哪个房间！
            string authJson = $"{{\"cmd\": 1, \"data\": {{\"roomId\": {roomID}}}}}";
            byte[] authBytes = Encoding.UTF8.GetBytes(authJson);
            await ws.SendAsync(new ArraySegment<byte>(authBytes), WebSocketMessageType.Text, true, cts.Token);
            
            Debug.Log($"✅ 成功发送订阅房间 [{roomID}] 请求！请发一条弹幕测试。");

            // 💡 2. 同时开启“接收通道”和“心跳通道”
            _ = ReceiveLoop();
            _ = HeartbeatLoop();
        }
        catch (Exception e)
        {
            Debug.LogError("❌ 连接失败: " + e.Message);
        }
    }

    // 💡 3. 核心关键：每隔 10 秒钟，向 blivechat 发送一次心跳包，告诉它“我还活着，别断开！”
    private async Task HeartbeatLoop()
    {
        string hbJson = "{\"cmd\": 0, \"data\": {}}";
        byte[] hbBytes = Encoding.UTF8.GetBytes(hbJson);

        while (ws.State == WebSocketState.Open && !cts.IsCancellationRequested)
        {
            await Task.Delay(10000); // 挂起等待10秒
            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(new ArraySegment<byte>(hbBytes), WebSocketMessageType.Text, true, cts.Token);
            }
        }
    }

    private async Task ReceiveLoop()
    {
        byte[] buffer = new byte[1024 * 16]; 

        while (ws.State == WebSocketState.Open && !cts.IsCancellationRequested)
        {
            try
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ParseData(jsonMessage);
                }
            }
            catch (Exception)
            {
                break; 
            }
        }
    }

    private void ParseData(string json)
    {
        Debug.Log($"收到原始数据: {json}");
        try
        {
            JObject obj = JObject.Parse(json);
            int cmd = obj["cmd"]?.Value<int>() ?? -1;

            if (cmd == 0) return; // 心跳包静默忽略

            // 💡 只有当 cmd == 2（弹幕）时才解析
            if (cmd == 2)
            {
                JToken dataToken = obj["data"];
                if (dataToken == null) return;

                // 🚨 核心修复：判断发过来的是不是一个数组！
                if (dataToken.Type == JTokenType.Array)
                {
                    JArray dataArray = (JArray)dataToken;
                    
                    // 根据你发来的真实数据，我们破解了它的数组排列位置：
                    // [0] = 头像URL
                    // [1] = UID
                    // [2] = 用户名
                    // [4] = 真实弹幕内容
                    
                    string avatarUrl = dataArray.Count > 0 ? dataArray[0]?.ToString() : "";
                    string uid = dataArray.Count > 1 ? dataArray[1]?.ToString() : "0";
                    string username = dataArray.Count > 2 ? dataArray[2]?.ToString() : "神秘人";
                    string message = dataArray.Count > 4 ? dataArray[4]?.ToString() : "";

                    danmakuQueue.Enqueue(new DanmuData
                    {
                        uid = uid,
                        username = username,
                        message = message,
                        avatarUrl = avatarUrl
                    });
                    
                    Debug.Log($"🎉 完美解析！[{username}] 说了: {message}");
                }
                // （备用容错）如果将来版本更新它又变回了 Object 格式
                else if (dataToken.Type == JTokenType.Object)
                {
                    string username = dataToken["authorName"]?.ToString() ?? "神秘人";
                    string message = dataToken["content"]?.ToString() ?? "";
                    string avatarUrl = dataToken["avatarUrl"]?.ToString() ?? ""; 
                    string uid = dataToken["id"]?.ToString() ?? username;

                    danmakuQueue.Enqueue(new DanmuData
                    {
                        uid = uid,
                        username = username,
                        message = message,
                        avatarUrl = avatarUrl
                    });
                    
                    Debug.Log($"🎉 完美解析！[{username}] 说了: {message}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 解析出错: {e.Message}\n错误数据: {json}");
        }
    }

    void Update()
    {
        if (danmakuQueue.TryDequeue(out DanmuData data))
        {
            SpawnChatMessage(data);
        }
    }

    private void SpawnChatMessage(DanmuData data)
    {
        GameObject go = Instantiate(chatItemPrefab, contentTransform);
        
        // 传入给之前写好的预制体脚本去加载头像和内容
        go.GetComponent<ChatItem>().Setup(data.uid, data.username, data.message, data.avatarUrl);

        if (contentTransform.childCount > maxMessageCount)
        {
            Destroy(contentTransform.GetChild(0).gameObject);
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        ws?.Dispose();
    }
}