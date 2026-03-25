using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SpriteLoader 使用示例
/// 展示如何从绝对路径加载 Sprite
/// </summary>
public class SpriteLoaderExample : MonoBehaviour
{
    [SerializeField]
    private Image imageDisplay; // 用于显示加载的 Sprite

    private SpriteLoader spriteLoader;

    private void Start()
    {
        // 获取或创建 SpriteLoader
        spriteLoader = GetComponent<SpriteLoader>();
        if (spriteLoader == null)
        {
            spriteLoader = gameObject.AddComponent<SpriteLoader>();
        }

        // 方式 1：加载单个 Sprite
        LoadSingleSprite();

        // 方式 2：批量加载 Sprite
        // LoadMultipleSprites();
    }

    /// <summary>
    /// 示例 1：加载单个 Sprite
    /// </summary>
    private void LoadSingleSprite()
    {
        // 文件名不需要扩展名，SpriteLoader 会自动查找 .png, .jpg 等
        Sprite sprite = spriteLoader.LoadSprite("icon_play");

        if (sprite != null)
        {
            Debug.Log($"<color=green>✓ Sprite 加载成功：{sprite.name}</color>");

            // 应用到 UI Image
            if (imageDisplay != null)
            {
                imageDisplay.sprite = sprite;
            }
        }
        else
        {
            Debug.LogWarning("✗ Sprite 加载失败");
        }
    }

    /// <summary>
    /// 示例 2：批量加载 Sprite
    /// </summary>
    private void LoadMultipleSprites()
    {
        string[] spriteNames = { "icon_play", "icon_pause", "icon_stop", "background" };

        var loadedSprites = spriteLoader.LoadSpriteBatch(spriteNames);

        foreach (var kvp in loadedSprites)
        {
            Debug.Log($"[SpriteLoaderExample] 已加载：{kvp.Key} -> {kvp.Value.name}");
        }
    }

    /// <summary>
    /// 示例 3：使用 UI Button 加载 Sprite
    /// </summary>
    public void OnPlayButtonClicked()
    {
        Sprite playIcon = spriteLoader.LoadSprite("icon_play");
        GetComponent<Image>().sprite = playIcon;
    }

    /// <summary>
    /// 示例 4：清空缓存（释放内存）
    /// </summary>
    public void ClearSpriteCache()
    {
        spriteLoader.ClearCache();
        Debug.Log("[SpriteLoaderExample] 已清空 Sprite 缓存");
    }

    /// <summary>
    /// 示例 5：从缓存中移除单个 Sprite
    /// </summary>
    public void RemoveSpriteFromCache(string spriteName)
    {
        spriteLoader.RemoveFromCache(spriteName);
    }
}
