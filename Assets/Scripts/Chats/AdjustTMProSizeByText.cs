using UnityEngine;
using TMPro;
 
public class AdjustTMProSizeByText : MonoBehaviour
{
    private TMP_Text tmpText;
 
    private RectTransform rectTransform;
 
    
 
    void Start()
    {
 
        if (tmpText == null)
        {
            
            tmpText = GetComponent<TMP_Text>();
        }
 
        rectTransform = GetComponent<RectTransform>();
 
        UpdateSizeByText();
    }
 
    void Update()
    {
        // 在Update中实时更新TMP Text的大小
        UpdateSizeByText();
    }
 
    public void UpdateSizeByText()
    {
        if (tmpText != null && rectTransform != null)
        {
           
 
            // 获取TMP Text的最佳大小，根据字数和其他文本样式来计算
            // 保持当前宽度，计算在该宽度下文本所需的高度
            float currentWidth = rectTransform.rect.width;
            Vector2 preferredSize = tmpText.GetPreferredValues(tmpText.text, currentWidth, 0);

            // 只更新高度，保持宽度不变
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, preferredSize.y);
        }
    }
}