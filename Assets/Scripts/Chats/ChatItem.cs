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

    public AdjustTMProSizeByText adjustNameSize;

    // 头像缓存，防止同一个人连续发言重复下载图片
    private static Dictionary<string, Texture2D> avatarCache = new Dictionary<string, Texture2D>();

    // 注意：这里多加了一个 avatarUrl 参数
    public void Setup(string uid, string username, string message, string avatarUrl)
    {
        nameText.text = $"[{uid}] {username}:";
        messageText.text = message;

        // 尝试从缓存加载头像
        if (avatarCache.TryGetValue(uid, out Texture2D cachedTex))
        {
            avatarImage.texture = cachedTex;
        }
        else if (!string.IsNullOrEmpty(avatarUrl))
        {
            // 直接下载官方给的合法头像链接，绝对不会被拦截！
            StartCoroutine(DownloadAvatar(avatarUrl, uid));
        }

        adjustNameSize.UpdateSizeByText();
    }

    private IEnumerator DownloadAvatar(string url, string uid)
    {
        // 官方头像 URL 有时是 http，安全起见在手机端最好替换为 https
        string secureUrl = url.Replace("http://", "https://");
        
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(secureUrl))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(req);
                avatarImage.texture = tex;
                
                if (!avatarCache.ContainsKey(uid))
                {
                    avatarCache.Add(uid, tex);
                }
            }
            else
            {
                Debug.LogWarning($"头像下载失败: {req.error}");
            }
        }
    }
}