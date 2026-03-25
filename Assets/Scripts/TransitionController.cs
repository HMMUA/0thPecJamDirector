using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionController : MonoBehaviour
{
    public Animator transitionAnimator; // 过渡动画控制器引用
    public DirectorSystem directorSystem; // 导演系统引用
    public CanvasinfoController canvasinfoController; // Canvas信息控制器引用


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
            // string path = $"Texture/{id}";
            // Sprite sprite = Resources.Load<Sprite>(path);
            // if (sprite != null)
            // {
            //     image[i].sprite = sprite;
            //     Debug.Log($"✓ 曲绘[{i}] 加载成功: {path}");
            // }
            // else
            // {
            //     Debug.LogWarning($"✗ 曲绘[{i}] 加载失败: {path} (检查Resources/Texture文件夹是否存在该文件)");
            // }
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
            string path = $"Texture/{id}_T";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                titleImage[i].sprite = sprite;
                Debug.Log($"✓ 标题[{i}] 加载成功: {path}");
            }
            else
            {
                Debug.LogWarning($"✗ 标题[{i}] 加载失败: {path}");
            }
        }

        // 加载标题描边
        for (int i = 0; i < titleImageOutline.Length; i++)
        {
            string path = $"Texture/{id}_T_O";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                titleImageOutline[i].sprite = sprite;
                Debug.Log($"✓ 标题描边[{i}] 加载成功: {path}");
            }
            else
            {
                Debug.LogWarning($"✗ 标题描边[{i}] 加载失败: {path}");
            }
        }

        // 加载标题遮罩
        for (int i = 0; i < titleImageMask.Length; i++)
        {
            string path = $"Texture/{id}_T";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                titleImageMask[i].sprite = sprite;
                Debug.Log($"✓ 标题遮罩[{i}] 加载成功: {path}");
            }
            else
            {
                Debug.LogWarning($"✗ 标题遮罩[{i}] 加载失败: {path}");
            }
        }

        // 加载谱师
        for (int i = 0; i < designerImage.Length; i++)
        {
            string path = $"Texture/{id}_D";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                designerImage[i].sprite = sprite;
                Debug.Log($"✓ 谱师[{i}] 加载成功: {path}");
            }
            else
            {
                Debug.LogWarning($"✗ 谱师[{i}] 加载失败: {path}");
            }
        }

        // 加载谱师描边
        for (int i = 0; i < designerImageOutline.Length; i++)
        {
            string path = $"Texture/{id}_D_O";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                designerImageOutline[i].sprite = sprite;
                Debug.Log($"✓ 谱师描边[{i}] 加载成功: {path}");
            }
            else
            {
                Debug.LogWarning($"✗ 谱师描边[{i}] 加载失败: {path}");
            }
        }

        // 加载谱师遮罩
        for (int i = 0; i < designerImageMask.Length; i++)
        {
            string path = $"Texture/{id}_D";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                designerImageMask[i].sprite = sprite;
                Debug.Log($"✓ 谱师遮罩[{i}] 加载成功: {path}");
            }
            else
            {
                Debug.LogWarning($"✗ 谱师遮罩[{i}] 加载失败: {path}");
            }
        }

        // 加载曲师
        for (int i = 0; i < artistImage.Length; i++)
        {
            string path = $"Texture/{id}_A";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                artistImage[i].sprite = sprite;
                Debug.Log($"✓ 曲师[{i}] 加载成功: {path}");
            }
            else
            {
                Debug.LogWarning($"✗ 曲师[{i}] 加载失败: {path}");
            }
        }

        // 加载曲师描边
        for (int i = 0; i < artistImageOutline.Length; i++)
        {
            string path = $"Texture/{id}_A_O";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                artistImageOutline[i].sprite = sprite;
                Debug.Log($"✓ 曲师描边[{i}] 加载成功: {path}");
            }
            else
            {
                Debug.LogWarning($"✗ 曲师描边[{i}] 加载失败: {path}");
            }
        }

        // 加载曲师遮罩
        for (int i = 0; i < artistImageMask.Length; i++)
        {
            string path = $"Texture/{id}_A";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                artistImageMask[i].sprite = sprite;
                Debug.Log($"✓ 曲师遮罩[{i}] 加载成功: {path}");
            }
            else
            {
                Debug.LogWarning($"✗ 曲师遮罩[{i}] 加载失败: {path}");
            }
        }

        Debug.Log($"<color=cyan>【ResetInformation完成】视频ID: {id}</color>");
    }

    public void EndTransition()
    {
        directorSystem.PlayNextVideo();
    }
    public void HideCanvasInfo()
    {
        canvasinfoController.HideInfo();
    }

    public void ShowCanvasInfo( )
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
