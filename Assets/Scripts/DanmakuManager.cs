using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;
using Liluo.BiliBiliLive; // 引入插件命名空间

public class DanmakuManager : MonoBehaviour
{[Header("直播间设置")]
    public int roomID = 213; // 替换为你想测试的B站直播间房间号

    [Header("UI 设置")]
    public GameObject danmakuPrefab; // 我们等下要做的弹幕预制体
    public Transform canvasTransform; // 画布节点

    private IBiliBiliLiveRequest req;
    
    // 线程安全队列：用来把子线程接收到的弹幕转移到主线程去生成 UI
    private ConcurrentQueue<string> danmakuQueue = new ConcurrentQueue<string>();

    async void Start()
    {
        // 1. 连接 B站直播间
        req = await BiliBiliLive.Connect(roomID);
        Debug.Log($"已成功连接到直播间: {roomID}");

        // 2. 绑定弹幕回调事件
        req.OnDanmuCallBack += OnDanmuReceived;
    }

    // 当有人发弹幕时，这个函数会被触发（在后台线程）
    private void OnDanmuReceived(BiliBiliLiveDanmuData data)
    {
        // 拼接用户名和弹幕内容
        string msg = $"{data.username}: {data.content}";
        
        // 压入队列，交给 Update 去处理
        danmakuQueue.Enqueue(msg);
    }

    void Update()
    {
        // 3. Unity主线程：检查队列里有没有新弹幕，有的话就生成实体
        if (danmakuQueue.TryDequeue(out string textMsg))
        {
            SpawnDanmaku(textMsg);
        }
    }

    private void SpawnDanmaku(string textMsg)
    {
        // 生成弹幕预制体
        GameObject go = Instantiate(danmakuPrefab, canvasTransform);
        
        // 随机高度，防止弹幕全挤在同一行 (假设你的Canvas高度在-300到300之间)
        float randomY = Random.Range(-300f, 300f); 
        
        // 初始位置设定在屏幕最右侧外边 (假设X=800)
        go.transform.localPosition = new Vector3(800f, randomY, 0); 
        
        // 给 UI Text 赋值
        go.GetComponent<Text>().text = textMsg;
    }

    void OnDestroy()
    {
        // 释放监听，断开连接
        if (req != null)
        {
            req.DisConnect();
            req = null;
        }
    }
}