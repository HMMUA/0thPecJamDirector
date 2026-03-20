using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;
// 引入官方 SDK 的命名空间（具体以你导入的 SDK 为准，通常是这几个）
using OpenBLive.Runtime;
using OpenBLive.Runtime.Data;
using OpenBLive.Runtime.Utilities;
using Newtonsoft.Json;

public class ChatManager : MonoBehaviour
{[Header("开发者凭证 (控制台获取)")]
    public string accessKeyId = "这里填你的 AccessKey ID";
    public string accessKeySecret = "这里填你的 AccessKey Secret";
    public long appId = 123456; // 填你的项目ID

    [Header("主播身份码 (开播获取)")]
    public string code = "这里填13位身份码";

    [Header("UI 绑定")]
    public GameObject chatItemPrefab; 
    public Transform contentTransform; 
    public ScrollRect scrollRect; 
    public int maxMessageCount = 50;

    // 弹幕数据结构体
    private struct DanmuData
    {
        public string uid;
        public string username;
        public string message;
        public string avatarUrl; // 官方多给了这个好东西
    }

    private ConcurrentQueue<DanmuData> danmakuQueue = new ConcurrentQueue<DanmuData>();
    
    // 官方的 WebSocket 客户端实例（使用 OpenBLive 的 BLiveClient）
    private BLiveClient webSocketClient;
    private string gameId;

    async void Start()
    {
        // 1. 初始化鉴权信息
        SignUtility.accessKeyId = accessKeyId;
        SignUtility.accessKeySecret = accessKeySecret;

        // 2. 开启互动玩法项目 (通过身份码和AppId向B站请求WebSocket地址)
        var ret = await BApi.StartInteractivePlay(code, appId.ToString());
        var appStartInfo = JsonConvert.DeserializeObject<AppStartInfo>(ret);

        if (appStartInfo == null || appStartInfo.Code != 0)
        {
            Debug.LogError($"连接失败，错误信息: {appStartInfo?.Message}");
            return;
        }

        gameId = appStartInfo.GetGameId();

        Debug.Log("项目启动成功！正在建立 WebSocket...");

        // 3. 建立 WebSocket 连接（使用 OpenBLive 提供的 WebSocketBLiveClient）
        webSocketClient = new WebSocketBLiveClient(appStartInfo.GetWssLink(), appStartInfo.GetAuthBody());
        webSocketClient.Connect();

        // 4. 绑定各种官方事件！
        webSocketClient.OnDanmaku += OnDanmakuReceived; // 弹幕
        // webSocketClient.OnGift += OnGiftReceived;    // 礼物（如果有需要可以取消注释）
        // webSocketClient.OnSuperChat += OnSC;         // 醒目留言
    }

    // 官方传过来的弹幕事件
    private void OnDanmakuReceived(Dm dm)
    {
        // 把数据扔进主线程队列
        danmakuQueue.Enqueue(new DanmuData
        {
            uid = dm.openId,             // 官方现在的 UID 叫 OpenId（为了隐私加密的）
            username = dm.userName,      // 用户名
            message = dm.msg,            // 弹幕内容
            avatarUrl = dm.userFace      // 官方直接提供的头像 URL ！！！
        });
    }

    void Update()
    {
        if (danmakuQueue.TryDequeue(out DanmuData data))
        {
            SpawnChatMessage(data);
        }
        
        // 在 Unity 下需要把底层 ws 的消息队列分发到主线程
        if (webSocketClient is WebSocketBLiveClient wsClient && wsClient.ws != null)
        {
            wsClient.ws.DispatchMessageQueue();
        }
    }

    private void SpawnChatMessage(DanmuData data)
    {
        GameObject go = Instantiate(chatItemPrefab, contentTransform);
        
        // 传入多出来的 avatarUrl
        go.GetComponent<ChatItem>().Setup(data.uid, data.username, data.message, data.avatarUrl);

        if (contentTransform.childCount > maxMessageCount)
        {
            Destroy(contentTransform.GetChild(0).gameObject);
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    async void OnDestroy()
    {
        // 退出时断开连接并结束玩法项目，这点很重要！否则主播的资源会被卡住
        if (webSocketClient != null)
        {
            webSocketClient.Dispose();
        }

        if (!string.IsNullOrEmpty(gameId))
        {
            await BApi.EndInteractivePlay(appId.ToString(), gameId);
        }
    }
}