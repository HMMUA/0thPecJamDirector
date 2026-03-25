using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionController : MonoBehaviour
{
    public Animator transitionAnimator; // 过渡动画控制器引用
    public DirectorSystem directorSystem; // 导演系统引用
    public CanvasinfoController canvasinfoController; // Canvas信息控制器引用
    
    private SpriteLoader spriteLoader; // Sprite 加载器引用


    public SpriteRenderer[] image;//曲绘

    public SpriteRenderer[] titleImage;//标题
    public SpriteRenderer[] titleImageOutline;//标题描边
    public SpriteMask[] titleImageMask;//标题遮罩

    public SpriteRenderer[] artistImage;//曲师
    public SpriteRenderer[] artistImageOutline;//曲师描边
    public SpriteMask[] artistImageMask;//曲师遮罩

    public SpriteRenderer[] designerImage;//谱师
    public SpriteRenderer[] designerImageOutline;//谱师描边
    public SpriteMask[] designerImageMask;//谱师遮罩

    public SpriteCoverScaler[] coverScaler; // 用于曲绘的SpriteCoverScaler组件数组



    // Start is called before the first frame update
    void Start()
    {
        // 自动获取或创建 SpriteLoader
        if (spriteLoader == null)
        {
            spriteLoader = GetComponent<SpriteLoader>();
            if (spriteLoader == null)
            {
                spriteLoader = gameObject.AddComponent<SpriteLoader>();
                Debug.Log("[TransitionController] SpriteLoader 已自动创建");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTransition(VideoData videoData)
    {
        transitionAnimator.Play("idle", 0, 0); // 重置动画状态
        transitionAnimator.SetTrigger("StartTransition");
        ResetInformation(videoData);
    }

    public void StartIntervalTransition(VideoData videoData)
    {
        transitionAnimator.Play("idle", 0, 0); // 重置动画状态
        transitionAnimator.SetTrigger("StartIntervalTransition");
        //ResetInformation(videoData);
    }

    public void ResetInformation(VideoData videoData)
    {
        string id = videoData.id;
        Debug.Log($"<color=cyan>【ResetInformation开始】视频ID: {id}</color>");

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("【错误】视频ID为空，无法加载图片资源！");
            return;
        }

        // 加载曲绘
        for (int i = 0; i < image.Length; i++)
        {
                if (videoData.coverSprite != null)
                {
                    image[i].sprite = videoData.coverSprite;
                    Debug.Log($"✓ 曲绘[{i}] 加载成功: 使用封面Sprite");
                }
                else
                {
                    Debug.LogWarning($"✗ 曲绘[{i}] 加载失败: 视频数据中没有封面Sprite");
                }
        }
        for (int i = 0; i < coverScaler.Length; i++)
        {
            if (coverScaler[i] != null)
            {
                coverScaler[i].AdjustScaleToCover();
                Debug.Log($"✓ 曲绘[{i}] 调整缩放成功");
            }
            else
            {
                Debug.LogWarning($"✗ 曲绘[{i}] 缩放调整失败: SpriteCoverScaler组件未设置");
            }
        }
        
        // 加载标题
        for (int i = 0; i < titleImage.Length; i++)
        {
            Sprite sprite = LoadSpriteWithFallback(id, "_T");
            if (sprite != null)
            {
                titleImage[i].sprite = sprite;
                Debug.Log($"✓ 标题[{i}] 加载成功：{id}_T");
            }
            else
            {
                Debug.LogWarning($"✗ 标题[{i}] 加载失败：{id}_T");
            }
        }

        // 加载标题描边
        for (int i = 0; i < titleImageOutline.Length; i++)
        {
            Sprite sprite = LoadSpriteWithFallback(id, "_T_O");
            if (sprite != null)
            {
                titleImageOutline[i].sprite = sprite;
                Debug.Log($"✓ 标题描边[{i}] 加载成功：{id}_T_O");
            }
            else
            {
                Debug.LogWarning($"✗ 标题描边[{i}] 加载失败：{id}_T_O");
            }
        }

        // 加载标题遮罩
        for (int i = 0; i < titleImageMask.Length; i++)
        {
            Sprite sprite = LoadSpriteWithFallback(id, "_T");
            if (sprite != null)
            {
                titleImageMask[i].sprite = sprite;
                Debug.Log($"✓ 标题遮罩[{i}] 加载成功：{id}_T");
            }
            else
            {
                Debug.LogWarning($"✗ 标题遮罩[{i}] 加载失败：{id}_T");
            }
        }

        // 加载谱师
        for (int i = 0; i < designerImage.Length; i++)
        {
            Sprite sprite = LoadSpriteWithFallback(id, "_D");
            if (sprite != null)
            {
                designerImage[i].sprite = sprite;
                Debug.Log($"✓ 谱师[{i}] 加载成功：{id}_D");
            }
            else
            {
                Debug.LogWarning($"✗ 谱师[{i}] 加载失败：{id}_D");
            }
        }

        // 加载谱师描边
        for (int i = 0; i < designerImageOutline.Length; i++)
        {
            Sprite sprite = LoadSpriteWithFallback(id, "_D_O");
            if (sprite != null)
            {
                designerImageOutline[i].sprite = sprite;
                Debug.Log($"✓ 谱师描边[{i}] 加载成功：{id}_D_O");
            }
            else
            {
                Debug.LogWarning($"✗ 谱师描边[{i}] 加载失败：{id}_D_O");
            }
        }

        // 加载谱师遮罩
        for (int i = 0; i < designerImageMask.Length; i++)
        {
            Sprite sprite = LoadSpriteWithFallback(id, "_D");
            if (sprite != null)
            {
                designerImageMask[i].sprite = sprite;
                Debug.Log($"✓ 谱师遮罩[{i}] 加载成功：{id}_D");
            }
            else
            {
                Debug.LogWarning($"✗ 谱师遮罩[{i}] 加载失败：{id}_D");
            }
        }

        // 加载曲师
        for (int i = 0; i < artistImage.Length; i++)
        {
            Sprite sprite = LoadSpriteWithFallback(id, "_A");
            if (sprite != null)
            {
                artistImage[i].sprite = sprite;
                Debug.Log($"✓ 曲师[{i}] 加载成功：{id}_A");
            }
            else
            {
                Debug.LogWarning($"✗ 曲师[{i}] 加载失败：{id}_A");
            }
        }

        // 加载曲师描边
        for (int i = 0; i < artistImageOutline.Length; i++)
        {
            Sprite sprite = LoadSpriteWithFallback(id, "_A_O");
            if (sprite != null)
            {
                artistImageOutline[i].sprite = sprite;
                Debug.Log($"✓ 曲师描边[{i}] 加载成功：{id}_A_O");
            }
            else
            {
                Debug.LogWarning($"✗ 曲师描边[{i}] 加载失败：{id}_A_O");
            }
        }

        // 加载曲师遮罩
        for (int i = 0; i < artistImageMask.Length; i++)
        {
            Sprite sprite = LoadSpriteWithFallback(id, "_A");
            if (sprite != null)
            {
                artistImageMask[i].sprite = sprite;
                Debug.Log($"✓ 曲师遮罩[{i}] 加载成功：{id}_A");
            }
            else
            {
                Debug.LogWarning($"✗ 曲师遮罩[{i}] 加载失败：{id}_A");
            }
        }

        Debug.Log($"<color=cyan>【ResetInformation完成】视频ID: {id}</color>");
    }

    /// <summary>
    /// 通用 Sprite 加载方法 - 先尝试 SpriteLoader（绝对路径），再回退到 Resources
    /// </summary>
    /// <param name="baseName">文件基础名称（如视频ID）</param>
    /// <param name="suffix">文件后缀（如 "_T", "_T_O", "_A" 等）</param>
    /// <returns>加载成功的 Sprite 或 null</returns>
    private Sprite LoadSpriteWithFallback(string baseName, string suffix)
    {
        if (spriteLoader == null)
        {
            spriteLoader = GetComponent<SpriteLoader>();
            if (spriteLoader == null)
            {
                // 如果没有 SpriteLoader，直接使用 Resources
                return TryLoadFromResources(baseName, suffix);
            }
        }

        string fileName = baseName + suffix;

        // 尝试先从 SpriteLoader（绝对路径/ 配置路径）加载
        Sprite sprite = spriteLoader.LoadSprite(fileName);
        if (sprite != null)
        {
            return sprite;
        }

        // 降级：尝试从 Resources 加载
        Debug.Log($"[TransitionController] 未从 SpriteLoader 找到 {fileName}，尝试从 Resources 加载...");
        return TryLoadFromResources(baseName, suffix);
    }

    /// <summary>
    /// 从 Resources 尝试加载 Sprite（备选方案）
    /// </summary>
    private Sprite TryLoadFromResources(string baseName, string suffix)
    {
        string path = $"Texture/{baseName}{suffix}";
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            Debug.Log($"[TransitionController] 已从 Resources 加载：{path}");
            return sprite;
        }
        return null;
    }

    /// <summary>
    /// 设置 SpriteLoader 的配置参数
    /// 可由 SettingsManager 或外部脚本调用
    /// </summary>
    /// <param name="resourcePath">Sprite 资源路径</param>
    /// <param name="useAbsolute">是否使用绝对路径</param>
    public void ConfigureSpriteLoader(string resourcePath, bool useAbsolute)
    {
        if (spriteLoader == null)
        {
            spriteLoader = GetComponent<SpriteLoader>();
            if (spriteLoader == null)
            {
                spriteLoader = gameObject.AddComponent<SpriteLoader>();
            }
        }

        spriteLoader.spriteResourcePath = resourcePath;
        spriteLoader.useAbsolutePath = useAbsolute;
        Debug.Log($"[TransitionController] SpriteLoader 已配置：{resourcePath}，使用绝对路径：{useAbsolute}");
    }

    public void EndTransition()
    {
        directorSystem.PlayNextVideo();
    }
    public void HideCanvasInfo()
    {
        canvasinfoController.HideInfo();
    }

    public void ShowCanvasInfo()
    {
        canvasinfoController.ShowInfo();
    }

    public void ShowIntervalInfo()
    {
        canvasinfoController.ShowInterval();
    }


    public void StartInterval()
    {
        directorSystem.HideVideo();
    }

    public void EndInterval()
    {
        directorSystem.ShowVideo();
    }





}
