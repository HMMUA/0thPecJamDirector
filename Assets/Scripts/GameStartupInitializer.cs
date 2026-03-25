using UnityEngine;

/// <summary>
/// 游戏启动配置初始化脚本
/// 放置此脚本在启动场景中的任意 GameObject 上
/// 或者放在场景的启动管理器中
/// </summary>
public class GameStartupInitializer : MonoBehaviour
{
    [Header("初始化选项")]
    [Tooltip("是否在启动时加载设置")]
    public bool loadSettingsOnStart = true;

    [Tooltip("是否输出详细的初始化日志")]
    public bool verboseLogging = true;

    private void Awake()
    {
        if (loadSettingsOnStart)
        {
            // 确保 SettingsManager 单例被创建和初始化
            var settingsManager = SettingsManager.Instance;
            if (verboseLogging)
            {
                Debug.Log("<color=cyan>[GameStartup] SettingsManager 已初始化</color>");
            }
        }
    }

    private void Start()
    {
        if (verboseLogging)
        {
            // 输出当前的配置信息（用于调试）
            var settings = SettingsManager.Instance.GetSettings();
            if (settings != null)
            {
                Debug.Log("<color=yellow>【当前游戏配置】</color>");
                Debug.Log($"  DirectorSystem.videoResourcePath: {settings.DirectorSystem.videoResourcePath}");
                Debug.Log($"  LiveProgressReporter.apiUrl: {settings.LiveProgressReporter.apiUrl}");
            }
        }
    }
}
