using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class ChatItem : MonoBehaviour
{
    public RawImage avatarImage;
    public TMP_Text nameText;
    public TMP_Text messageText;
    public AdjustTMProSizeByText adjuster;

    // 💡 新增：全局静态缓存，记录 UID 对应的“真实名字”，避免重复反查
    private static Dictionary<string, string> nameCache = new Dictionary<string, string>();
    // 全局静态缓存，记录 UID 对应的“头像”
    private static Dictionary<string, Texture2D> avatarCache = new Dictionary<string, Texture2D>();

    public void Setup(string uid, string username, string message, string avatarUrl)
    {
        messageText.text = message;
        adjuster.UpdateSizeByText(); // 更新文本框大小以适应内容

        // 💡 1. 处理名字：优先从缓存拿真实名字
        if (nameCache.TryGetValue(uid, out string realName))
        {
            nameText.text = $"[{uid}] {realName}:";
        }
        else if (username.Contains("*")) 
        {
            // 如果没缓存，且名字被打码了，就显示“获取中”，并开启网络反查
            nameText.text = $"[{uid}] 加载中...:";
            StartCoroutine(FetchRealNameAndAvatar(uid, avatarUrl));
        }
        else
        {
            // 如果名字正常，直接显示
            nameText.text = $"[{uid}] {username}:";
            nameCache[uid] = username; // 顺手存入缓存
        }

        // 2. 处理头像：优先从缓存拿
        if (avatarCache.TryGetValue(uid, out Texture2D cachedTex))
        {
            avatarImage.texture = cachedTex;
        }
        else if (!username.Contains("*") && !string.IsNullOrEmpty(avatarUrl))
        {
            // 如果名字没被打码，说明数据完整，直接下载传入的头像URL
            StartCoroutine(DownloadAvatar(avatarUrl, uid));
        }
    }

    // 💡 核心魔法：拿着 UID 去 B站公开接口查出真实名字！
    private IEnumerator FetchRealNameAndAvatar(string uid, string defaultAvatarUrl)
    {
        // B站名片 API：只要有 UID 就能查到一切公开信息
        string apiUrl = $"https://api.bilibili.com/x/web-interface/card?mid={uid}";
        using (UnityWebRequest req = UnityWebRequest.Get(apiUrl))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                string json = req.downloadHandler.text;
                
                // 从 JSON 中提取真实名字
                string realName = ExtractJsonValue(json, "\"name\":\"");
                if (!string.IsNullOrEmpty(realName))
                {
                    nameCache[uid] = realName; // 存入缓存
                    nameText.text = $"[{uid}] {realName}:"; // 更新 UI
                }

                // 从 JSON 中顺便提取最高清的头像 URL 并下载
                string faceUrl = ExtractJsonValue(json, "\"face\":\"");
                if (!string.IsNullOrEmpty(faceUrl))
                {
                    faceUrl = faceUrl.Replace("\\/", "/"); // 处理转义符
                    StartCoroutine(DownloadAvatar(faceUrl, uid));
                }
                else if (!string.IsNullOrEmpty(defaultAvatarUrl))
                {
                    StartCoroutine(DownloadAvatar(defaultAvatarUrl, uid));
                }
            }
        }
    }

    private IEnumerator DownloadAvatar(string url, string uid)
    {
        string secureUrl = url.Replace("http://", "https://");
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(secureUrl))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(req);
                avatarImage.texture = tex;
                avatarCache[uid] = tex; // 存入缓存
            }
        }
    }

    // 一个极其轻量的 JSON 字符串截取工具，免去额外引库的麻烦
    private string ExtractJsonValue(string json, string key)
    {
        int startIndex = json.IndexOf(key);
        if (startIndex == -1) return null;
        
        startIndex += key.Length;
        int endIndex = json.IndexOf("\"", startIndex);
        if (endIndex == -1) return null;
        
        return json.Substring(startIndex, endIndex - startIndex);
    }
}