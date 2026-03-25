using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Sprite 加载器 - 支持从绝对路径或相对路径加载 Sprite
/// 文件名查询已支持扩展名自动处理（.png, .jpg, .jpeg 等）
/// </summary>
public class SpriteLoader : MonoBehaviour
{
    [Header("Sprite 资源配置")]
    [Tooltip("Sprite 文件所在的目录路径")]
    public string spriteResourcePath = "E:\\0th\\Sprites";

    [Tooltip("是否使用绝对路径加载（true: 绝对路径, false: Resources 相对路径）")]
    public bool useAbsolutePath = true;

    [Tooltip("Sprite 的单位像素数")]
    public float pixelsPerUnit = 100f;

    // Sprite 缓存，避免重复加载
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    /// <summary>
    /// 加载 Sprite（自动选择加载方式）
    /// </summary>
    /// <param name="fileName">文件名（可以不带扩展名）</param>
    /// <returns>加载成功的 Sprite，失败返回 null</returns>
    public Sprite LoadSprite(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning("[SpriteLoader] 文件名为空");
            return null;
        }

        // 检查缓存
        if (spriteCache.ContainsKey(fileName))
        {
            return spriteCache[fileName];
        }

        Sprite sprite = null;

        if (useAbsolutePath)
        {
            sprite = LoadSpriteFromAbsolutePath(fileName);
        }
        else
        {
            sprite = LoadSpriteFromResources(fileName);
        }

        if (sprite != null)
        {
            spriteCache[fileName] = sprite;
        }

        return sprite;
    }

    /// <summary>
    /// 从绝对路径加载 Sprite
    /// 支持多种图片格式（.png, .jpg, .jpeg, .bmp, .tga）
    /// </summary>
    /// <param name="fileName">文件名（可以不带扩展名）</param>
    /// <returns>加载成功的 Sprite</returns>
    private Sprite LoadSpriteFromAbsolutePath(string fileName)
    {
        // 支持的图片扩展名
        string[] supportedExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".tga" };

        // 如果文件名已有扩展名，直接使用
        if (HasValidExtension(fileName))
        {
            return LoadSpriteFromFile(Path.Combine(spriteResourcePath, fileName));
        }

        // 尝试不同的扩展名
        foreach (string ext in supportedExtensions)
        {
            string fullPath = Path.Combine(spriteResourcePath, fileName + ext);
            if (File.Exists(fullPath))
            {
                return LoadSpriteFromFile(fullPath);
            }
        }

        Debug.LogWarning($"[SpriteLoader] 未找到图片文件：{Path.Combine(spriteResourcePath, fileName)}（尝试了所有支持的扩展名）");
        return null;
    }

    /// <summary>
    /// 从 Resources 相对路径加载 Sprite
    /// </summary>
    /// <param name="resourcePath">相对于 Resources 的路径</param>
    /// <returns>加载成功的 Sprite</returns>
    private Sprite LoadSpriteFromResources(string resourcePath)
    {
        // 移除扩展名（Resources.Load 不需要文件扩展名）
        string pathWithoutExt = Path.ChangeExtension(resourcePath, null);
        string fullResourcePath = Path.Combine(spriteResourcePath, pathWithoutExt);

        Sprite sprite = Resources.Load<Sprite>(fullResourcePath);
        if (sprite != null)
        {
            Debug.Log($"[SpriteLoader] 成功从 Resources 加载 Sprite：{fullResourcePath}");
            return sprite;
        }

        Debug.LogWarning($"[SpriteLoader] 未在 Resources 中找到 Sprite：{fullResourcePath}");
        return null;
    }

    /// <summary>
    /// 从文件系统加载 Sprite
    /// </summary>
    /// <param name="filePath">完整的文件路径</param>
    /// <returns>加载成功的 Sprite</returns>
    private Sprite LoadSpriteFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[SpriteLoader] 文件不存在：{filePath}");
            return null;
        }

        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(fileData);

            // 创建 Sprite
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );

            sprite.name = Path.GetFileNameWithoutExtension(filePath);
            Debug.Log($"[SpriteLoader] 成功加载 Sprite：{filePath}");
            return sprite;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SpriteLoader] 加载 Sprite 失败：{filePath}，错误：{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 检查文件名是否包含有效的图片扩展名
    /// </summary>
    private bool HasValidExtension(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLower();
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".tga";
    }

    /// <summary>
    /// 清空 Sprite 缓存
    /// </summary>
    public void ClearCache()
    {
        spriteCache.Clear();
        Debug.Log("[SpriteLoader] Sprite 缓存已清空");
    }

    /// <summary>
    /// 从缓存中移除指定的 Sprite
    /// </summary>
    /// <param name="fileName">文件名</param>
    public void RemoveFromCache(string fileName)
    {
        if (spriteCache.ContainsKey(fileName))
        {
            spriteCache.Remove(fileName);
            Debug.Log($"[SpriteLoader] 已从缓存中移除：{fileName}");
        }
    }

    /// <summary>
    /// 批量加载 Sprite（返回字典）
    /// </summary>
    /// <param name="fileNames">文件名列表</param>
    /// <returns>文件名 -> Sprite 的字典</returns>
    public Dictionary<string, Sprite> LoadSpriteBatch(string[] fileNames)
    {
        var result = new Dictionary<string, Sprite>();
        foreach (string fileName in fileNames)
        {
            Sprite sprite = LoadSprite(fileName);
            if (sprite != null)
            {
                result[fileName] = sprite;
            }
        }
        Debug.Log($"[SpriteLoader] 批量加载完成：成功 {result.Count}/{fileNames.Length}");
        return result;
    }
}
