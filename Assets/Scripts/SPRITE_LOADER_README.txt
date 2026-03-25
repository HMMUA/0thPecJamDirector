================================
从绝对路径加载 Sprite 功能说明
================================

【功能概述】

新增 SpriteLoader 类，支持从绝对路径或 Resources 文件夹加载 Sprite。

特点：
✓ 自动处理多种图片格式（.png, .jpg, .jpeg, .bmp, .tga）
✓ 文件名查询时可不带扩展名
✓ 内置缓存机制，避免重复加载
✓ 支持批量加载
✓ 与 SettingsManager 集成，支持配置文件控制


【配置文件】

在 SettingsConfig.json 中添加 SpriteLoader 配置：

{
  "SpriteLoader": {
    "spriteResourcePath": "E:\\0th\\Sprites",
    "useAbsolutePath": true
  }
}

字段说明：
- spriteResourcePath: Sprite 文件所在的目录
- useAbsolutePath: 是否使用绝对路径
  * true: 从文件系统加载（推荐，更灵活）
  * false: 从 Resources 文件夹加载


【快速开始】

1. 启动时自动应用配置
   - SettingsManager 会自动查找 SpriteLoader 并应用配置

2. 在脚本中使用
   ```csharp
   SpriteLoader loader = GetComponent<SpriteLoader>();
   Sprite sprite = loader.LoadSprite("my_sprite");  // 自动查找合适的扩展名
   ```

3. 设置到 UI 上
   ```csharp
   GetComponent<Image>().sprite = sprite;
   ```


【API 说明】

【主要方法】

1. LoadSprite(string fileName)
   - 加载单个 Sprite
   - 参数：文件名（可不带扩展名）
   - 返回：Sprite 对象或 null

2. LoadSpriteBatch(string[] fileNames)
   - 批量加载多个 Sprite
   - 参数：文件名数组
   - 返回：Dictionary<文件名, Sprite>

3. ClearCache()
   - 清空所有缓存的 Sprite
   - 用于释放内存

4. RemoveFromCache(string fileName)
   - 从缓存中移除单个 Sprite
   - 参数：文件名

【属性】

- spriteResourcePath: Sprite 文件夹路径
- useAbsolutePath: 是否使用绝对路径
- pixelsPerUnit: Sprite 的单位像素数（默认 100）


【使用示例】

【示例 1】加载单个 Sprite
```csharp
SpriteLoader loader = GetComponent<SpriteLoader>();
Sprite icon = loader.LoadSprite("button_icon");

// 应用到 UI Image
GetComponent<Image>().sprite = icon;
```

【示例 2】批量加载 Sprite
```csharp
string[] names = { "icon1", "icon2", "background" };
var sprites = loader.LoadSpriteBatch(names);

foreach (var pair in sprites)
{
    Debug.Log($"加载: {pair.Key} = {pair.Value.name}");
}
```

【示例 3】处理不同的扩展名
```csharp
// 这些调用都有效：
Sprite s1 = loader.LoadSprite("image");        // 自动查找 .png/.jpg/.etc
Sprite s2 = loader.LoadSprite("image.png");    // 直接使用 .png
Sprite s3 = loader.LoadSprite("image.jpg");    // 直接使用 .jpg
```

【示例 4】释放内存
```csharp
// 使用完毕后清空缓存
loader.ClearCache();

// 或只移除特定的 Sprite
loader.RemoveFromCache("large_texture");
```


【文件结构】

假设配置路径为 E:\\0th\\Sprites，目录结构：

E:\0th\Sprites\
├── icon_play.png
├── icon_pause.png
├── icon_stop.png
├── background.jpg
├── ui/
│   ├── button_1.png
│   └── button_2.png
└── characters/
    ├── hero.png
    └── enemy.png

加载方式：
```csharp
// 根目录
Sprite s1 = loader.LoadSprite("icon_play");

// 不支持子目录自动查询，需要指定完整相对路径
// Sprite s2 = loader.LoadSprite("ui/button_1");  // 如果需要可修改代码支持
```


【扩展名支持】

✓ .png（推荐用于透明背景）
✓ .jpg / .jpeg（推荐用于照片）
✓ .bmp（位图）
✓ .tga（TGA 格式）

其他格式可通过修改 SpriteLoader.cs 的 supportedExtensions 数组添加支持


【性能建议】

1. 使用缓存
   - SpriteLoader 内置缓存，避免重复加载同一文件

2. 批量加载
   - 使用 LoadSpriteBatch() 加载多个 Sprite

3. 及时清理
   - 场景切换时调用 ClearCache() 释放内存

4. 预加载
   - 在适当时机预加载常用 Sprite（如加载屏幕时）


【调试】

如果 Sprite 加载失败：

1. 检查 Console 日志
   - "[SpriteLoader] 未找到图片文件..." 表示文件不存在

2. 验证文件路径
   - 确保 spriteResourcePath 正确
   - 确保文件确实存在于该目录

3. 验证文件名
   - 检查大小写是否正确（某些系统大小写敏感）
   - 确保文件扩展名被支持

4. 验证配置
   - 确保 SettingsConfig.json 的 JSON 格式有效
   - 确保 useAbsolutePath 与实际情况相符


【与其他系统集成】

SettingsManager 会在启动时自动：
1. 读取 SettingsConfig.json
2. 查找场景中的 SpriteLoader
3. 应用配置：
   - spriteResourcePath
   - useAbsolutePath

示例输出：
```
[SpriteLoader] Sprite 加载路径已更新：E:\0th\Sprites
[SpriteLoader] 加载模式已设置：绝对路径
```

================================
