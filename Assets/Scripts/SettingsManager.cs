using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class LiveProgressReporterSettings
{
    public string apiUrl;
    public int timeoutSeconds;
    public string authorizationHeader;
    public string cookieHeader;
}

[System.Serializable]
public class DirectorSystemSettings
{
    public string videoResourcePath;
}

[System.Serializable]
public class SpriteLoaderSettings
{
    public string spriteResourcePath;
    public bool useAbsolutePath = true;
}

[System.Serializable]
public class SettingsData
{
    public LiveProgressReporterSettings LiveProgressReporter;
    public DirectorSystemSettings DirectorSystem;
    public SpriteLoaderSettings SpriteLoader;
}

public class SettingsManager : MonoBehaviour
{
    private static SettingsManager instance;
    private SettingsData settingsData;

    public static SettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SettingsManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SettingsManager");
                    instance = go.AddComponent<SettingsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // 单例模式
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // 启动时加载设置
        LoadSettings();
    }

    /// <summary>
    /// 从 StreamingAssets/SettingsConfig.json 加载设置
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            string settingsPath = System.IO.Path.Combine(Application.streamingAssetsPath, "SettingsConfig.json");
            
            if (!System.IO.File.Exists(settingsPath))
            {
                Debug.LogError($"❌ SettingsConfig.json 文件未找到！路径：{settingsPath}");
                return;
            }

            string json = System.IO.File.ReadAllText(settingsPath);
            settingsData = JsonUtility.FromJson<SettingsData>(json);
            Debug.Log($"<color=green>✓ 设置已成功从 StreamingAssets 加载</color>");

            // 应用配置到各个系统
            ApplySettings();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 加载设置文件失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 将读取的设置应用到各个系统
    /// </summary>
    private void ApplySettings()
    {
        // 应用 DirectorSystem 设置
        DirectorSystem directorSystem = FindObjectOfType<DirectorSystem>();
        if (directorSystem != null && settingsData.DirectorSystem != null)
        {
            if (!string.IsNullOrEmpty(settingsData.DirectorSystem.videoResourcePath))
            {
                directorSystem.videoResourcePath = settingsData.DirectorSystem.videoResourcePath;
                Debug.Log($"[DirectorSystem] 视频加载位置已更新：{settingsData.DirectorSystem.videoResourcePath}");
            }
        }

        // 应用 LiveProgressReporter 设置
        LiveProgressReporter reporter = FindObjectOfType<LiveProgressReporter>();
        if (reporter != null && settingsData.LiveProgressReporter != null)
        {
            if (!string.IsNullOrEmpty(settingsData.LiveProgressReporter.apiUrl))
            {
                reporter.apiUrl = settingsData.LiveProgressReporter.apiUrl;
                Debug.Log($"[LiveProgressReporter] 接口 URL 已更新");
            }

            if (settingsData.LiveProgressReporter.timeoutSeconds > 0)
            {
                reporter.timeoutSeconds = settingsData.LiveProgressReporter.timeoutSeconds;
                Debug.Log($"[LiveProgressReporter] 超时时间已更新：{settingsData.LiveProgressReporter.timeoutSeconds}秒");
            }

            if (!string.IsNullOrEmpty(settingsData.LiveProgressReporter.authorizationHeader))
            {
                reporter.authorizationHeader = settingsData.LiveProgressReporter.authorizationHeader;
                Debug.Log($"[LiveProgressReporter] 鉴权配置已更新");
            }

            if (!string.IsNullOrEmpty(settingsData.LiveProgressReporter.cookieHeader))
            {
                reporter.cookieHeader = settingsData.LiveProgressReporter.cookieHeader;
                Debug.Log($"[LiveProgressReporter] Cookie 已更新");
            }
        }

        // 应用 SpriteLoader 设置
        SpriteLoader spriteLoader = FindObjectOfType<SpriteLoader>();
        if (spriteLoader != null && settingsData.SpriteLoader != null)
        {
            if (!string.IsNullOrEmpty(settingsData.SpriteLoader.spriteResourcePath))
            {
                spriteLoader.spriteResourcePath = settingsData.SpriteLoader.spriteResourcePath;
                Debug.Log($"[SpriteLoader] Sprite 加载路径已更新：{settingsData.SpriteLoader.spriteResourcePath}");
            }

            spriteLoader.useAbsolutePath = settingsData.SpriteLoader.useAbsolutePath;
            Debug.Log($"[SpriteLoader] 加载模式已设置：{(settingsData.SpriteLoader.useAbsolutePath ? "绝对路径" : "相对路径")}");
        }

        // 应用 TransitionController 的 SpriteLoader 配置
        TransitionController transitionController = FindObjectOfType<TransitionController>();
        if (transitionController != null && settingsData.SpriteLoader != null)
        {
            transitionController.ConfigureSpriteLoader(
                settingsData.SpriteLoader.spriteResourcePath,
                settingsData.SpriteLoader.useAbsolutePath
            );
            Debug.Log($"[TransitionController] SpriteLoader 配置已应用");
        }
    }

    /// <summary>
    /// 获取当前的设置数据（可用于调试或动态查询）
    /// </summary>
    public SettingsData GetSettings()
    {
        return settingsData;
    }

    /// <summary>
    /// 重新加载设置（用于调试或动态更新）
    /// </summary>
    public void ReloadSettings()
    {
        Debug.Log("重新加载设置...");
        LoadSettings();
    }
}
