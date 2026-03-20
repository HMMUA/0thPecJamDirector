using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;

public class MockChatManager : MonoBehaviour
{
    [Header("UI 绑定")]
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

    // 准备几个真实的B站头像测试链接（或者用随便的网图）
    private string[] testAvatars = new string[]
    {
        "https://i0.hdslb.com/bfs/face/member/noface.jpg", // B站默认无头像
        "https://i1.hdslb.com/bfs/face/41da8d0eb0bfce876e6a18882ca9f899e32a6857.jpg", // 真实测试头像1
        "https://i2.hdslb.com/bfs/face/b70f6e62e4582d4fa5d48d86047e64eb57d7504e.jpg"  // 真实测试头像2
    };

    void Start()
    {
        // 开启模拟器，每隔 0.5 到 2 秒发一条弹幕
        StartCoroutine(MockDanmakuGenerator());
    }

    private IEnumerator MockDanmakuGenerator()
    {
        int count = 1;
        while (true)
        {
            // 随机等待一点时间，模拟真实直播间有人断断续续发弹幕
            yield return new WaitForSeconds(Random.Range(0.5f, 2.0f));

            string randomAvatar = testAvatars[Random.Range(0, testAvatars.Length)];
            
            danmakuQueue.Enqueue(new DanmuData
            {
                uid = "UID_" + Random.Range(1000, 9999),
                username = "测试观众" + count,
                message = "这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 这是一条测试弹幕内容 " + count,
                avatarUrl = randomAvatar
            });
            count++;
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
        // 调用你之前写好的带有头像下载逻辑的 Setup
        go.GetComponent<ChatItem>().Setup(data.uid, data.username, data.message, data.avatarUrl);

        if (contentTransform.childCount > maxMessageCount)
        {
            Destroy(contentTransform.GetChild(0).gameObject);
        }

        // 强制刷新并滚动到底部
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}