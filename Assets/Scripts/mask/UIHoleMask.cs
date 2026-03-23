using UnityEngine;
using UnityEngine.UI;

public class UIHoleMask : MonoBehaviour
{[Header("固定的黑洞区域 (不可见)")]
    public Image fixedHoleArea;[Header("会移动的UI元素")]
    public Image[] movingElement;

    void Start()
    {
        // 1. 生成黑洞材质
        Material matHole = new Material(Shader.Find("UI/Default"));
        matHole.SetInt("_Stencil", 1);
        matHole.SetInt("_StencilComp", 8); // Always
        matHole.SetInt("_StencilOp", 2);   // Replace
        matHole.SetInt("_ColorMask", 0);   // 设为0使其完全透明隐形
        fixedHoleArea.material = matHole;

        // 2. 生成移动元素材质
        Material matMoving = new Material(Shader.Find("UI/Default"));
        matMoving.SetInt("_Stencil", 1);
        matMoving.SetInt("_StencilComp", 6); // NotEqual (遇到1就不显示)
        matMoving.SetInt("_StencilOp", 0);   // Keep
        for (int i = 0; i < movingElement.Length; i++)
        {
            movingElement[i].material = matMoving;
        }
    }
}