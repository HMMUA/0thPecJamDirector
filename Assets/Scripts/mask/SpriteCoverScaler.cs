using UnityEngine;[RequireComponent(typeof(SpriteRenderer))]
public class SpriteCoverScaler : MonoBehaviour
{
    [Header("基础区域设置")][Tooltip("基础图片的尺寸 (依据 UsePixelSize 决定是像素还是世界单位)")]
    public Vector2 baseSize = new Vector2(100f, 100f);[Tooltip("基础图片当时的 Transform Scale 值")]
    public Vector2 baseScale = Vector2.one;[Header("计算模式")][Tooltip("勾选: 输入的 baseSize 为像素(如1920x1080) \n不勾选: baseSize 为世界坐标单位(受Pixels Per Unit影响)")]
    public bool usePixelSize = true;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 当替换了 Sprite 后调用此函数，自动等比缩放以刚好覆盖基础区域
    /// </summary>
    public void AdjustScaleToCover()
    {
        Debug.Log($"<color=yellow>调整封面缩放以覆盖区域，当前Sprite: {spriteRenderer.sprite?.name ?? "无"}</color>");
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer.sprite == null)
        {
            Debug.LogWarning("SpriteRenderer没有图片，无法进行缩放计算！");
            return;
        }

        // 1. 获取新替换图片的原始尺寸
        Vector2 newSpriteSize;
        if (usePixelSize)
        {
            // 使用图片的实际像素尺寸
            newSpriteSize = new Vector2(spriteRenderer.sprite.rect.width, spriteRenderer.sprite.rect.height);
        }
        else
        {
            // 使用图片在世界空间中的单位尺寸（不受Transform影响的原始大小）
            newSpriteSize = spriteRenderer.sprite.bounds.size;
        }

        // 防止除以0报错
        if (newSpriteSize.x == 0 || newSpriteSize.y == 0) return;

        // 2. 计算目标覆盖区域的绝对大小
        float targetWidth = Mathf.Abs(baseSize.x * baseScale.x);
        float targetHeight = Mathf.Abs(baseSize.y * baseScale.y);

        // 3. 计算为了覆盖目标区域，X和Y方向各自【至少】需要的缩放倍数
        float scaleX = targetWidth / newSpriteSize.x;
        float scaleY = targetHeight / newSpriteSize.y;

        // 4. 等比缩放且要求【刚好完全覆盖】区域，因此取X和Y中较大的那个缩放值
        // (注：如果需求是【刚好全部放进】区域不被裁切，这里换成 Mathf.Min 即可)
        float finalScale = Mathf.Max(scaleX, scaleY);

        // 5. 应用新的等比缩放值 (保留原本的Z轴缩放和翻转符号)
        // 使用 Mathf.Sign 是为了防止如果你的物体原本做了镜像翻转(Scale为负数)被破坏
        float signX = transform.localScale.x >= 0 ? 1f : -1f;
        float signY = transform.localScale.y >= 0 ? 1f : -1f;

        transform.localScale = new Vector3(
            finalScale * signX,
            finalScale * signY,
            transform.localScale.z
        );

        Debug.Log($"调整完成！新Scale: {transform.localScale}, 目标区域: ({targetWidth} x {targetHeight}), 图片原始尺寸: ({newSpriteSize.x} x {newSpriteSize.y}), 计算的缩放倍数: {finalScale}");
    }

    // --------------------------------------------------------
    // 【编辑器小工具】：在Inspector中右键脚本名称，点击此按钮可自动抓取当前Sprite设置
    // --------------------------------------------------------
    [ContextMenu("从当前Sprite自动获取基础设置")]
    private void SetupFromCurrent()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            if (usePixelSize)
                baseSize = new Vector2(sr.sprite.rect.width, sr.sprite.rect.height);
            else
                baseSize = sr.sprite.bounds.size;

            baseScale = new Vector2(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y));
            Debug.Log("已成功获取当前Sprite的尺寸和Scale作为基础区域！");
        }
    }
}