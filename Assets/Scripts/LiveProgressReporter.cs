using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 直播进度上报工具
/// </summary>
public class LiveProgressReporter : MonoBehaviour
{
    [Header("接口配置")]
    public string apiUrl = "https://pecjam-web-git-full-disclose-napties-projects.vercel.app/?_vercel_share=eJcoE8NHufKddG5xqGlrZhPI2gdarpSM";

    [Tooltip("上报请求超时时间（秒），到达后将中断请求并输出超时反馈")]
    public int timeoutSeconds = 15;

    [Header("鉴权配置 (可在此填写或通过代码动态赋值)")]
    [Tooltip("例如: Bearer xxxxx")]
    public string authorizationHeader;
    
    [Tooltip("测试环境的 Cookie")]
    public string cookieHeader;

    [Header("调试选项")]
    [Tooltip("启用上报结果日志输出（仅用于调试）")]
    public bool enableLogging = false;

    // ==========================================
    // 事件：供外部监听上报结果
    // ==========================================

    /// <summary>
    /// 上报完成事件：参数 (isSuccess, message)
    /// </summary>
    public event System.Action<bool, string> OnReportCompleted;
    [System.Serializable]
    private class ReportBody
    {
        public string entryId;
    }

    // ==========================================
    // 供你外部调用的公开函数
    // ==========================================

    /// <summary>
    /// 1. 上报当前正在播放的稿件
    /// </summary>
    /// <param name="entryId">当前稿件ID</param>
    public void ReportNowPlaying(string entryId)
    {
        if (string.IsNullOrEmpty(entryId))
        {
            Debug.LogWarning("[LiveProgressReporter] entryId 为空，放弃上报");
            return;
        }
        
        StartCoroutine(PostNowPlayingRoutine(entryId));
    }

    /// <summary>
    /// 2. 移除当前正在播放的稿件（中场休息、直播结束）
    /// </summary>
    public void ClearNowPlaying()
    {
        StartCoroutine(DeleteNowPlayingRoutine());
    }

    // ==========================================
    // 内部网络请求逻辑
    // ==========================================

    private IEnumerator PostNowPlayingRoutine(string entryId)
    {
        // 构造 JSON 数据
        ReportBody data = new ReportBody { entryId = entryId };
        string jsonData = JsonUtility.ToJson(data);

        // Unity 原生发送 POST JSON 的标准做法
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // 设置 Content-Type
            request.SetRequestHeader("Content-Type", "application/json");
            
            // 注入鉴权信息
            ApplyAuthHeaders(request);

            // 设置超时
            request.timeout = timeoutSeconds;

            // 等待请求返回
            yield return request.SendWebRequest();

            //Debug.Log($"[LiveProgressReporter] 请求完成，状态码: {request.responseCode}");

            // Unity 2020+ 使用 result 判断
            if (request.result == UnityWebRequest.Result.Success)
            {
                string successMessage = $"成功上报正在播放的稿件: {entryId}";
                if (enableLogging) Debug.Log($"[LiveProgressReporter] {successMessage}");
                OnReportCompleted?.Invoke(true, successMessage);
            }
            else
            {
                bool isTimeout = !string.IsNullOrEmpty(request.error) && request.error.ToLower().Contains("timeout");
                string errorMessage = isTimeout ? $"上报超时（{timeoutSeconds}s）" : $"上报失败: {request.error}";
                if (enableLogging) Debug.LogWarning($"[LiveProgressReporter] {errorMessage}");
                OnReportCompleted?.Invoke(false, errorMessage);
            }
        }
    }

    private IEnumerator DeleteNowPlayingRoutine()
    {
        // 发送 DELETE 请求
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "DELETE"))
        {
            // DELETE 默认不需要 body，但配置一个 downloadHandler 以便读取接口报错信息
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // 注入鉴权信息
            ApplyAuthHeaders(request);

            // 设置超时
            request.timeout = timeoutSeconds;

            // 等待请求返回
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string successMessage = "成功移除当前播放稿件（休息/结束状态）";
                if (enableLogging) Debug.Log($"[LiveProgressReporter] {successMessage}");
                OnReportCompleted?.Invoke(true, successMessage);
            }
            else
            {
                bool isTimeout = !string.IsNullOrEmpty(request.error) && request.error.ToLower().Contains("timeout");
                string errorMessage = isTimeout ? $"移除请求超时（{timeoutSeconds}s）" : $"移除失败: {request.error}";
                if (enableLogging) Debug.LogError($"[LiveProgressReporter] {errorMessage}");
                OnReportCompleted?.Invoke(false, errorMessage);
            }
        }
    }

    /// <summary>
    /// 统一处理鉴权 Header 的注入
    /// </summary>
    private void ApplyAuthHeaders(UnityWebRequest request)
    {
        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            request.SetRequestHeader("Authorization", authorizationHeader);
        }

        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request.SetRequestHeader("Cookie", cookieHeader);
        }
    }
}