using UnityEngine;

public class DanmakuItem : MonoBehaviour
{
    public float speed = 150f; // 弹幕飘过的速度

    void Update()
    {
        // 每帧向左移动
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // 如果弹幕飞出了屏幕最左侧（根据你的画布大小自行调整阈值），就销毁自身，释放内存
        if (transform.localPosition.x < -1000f) 
        {
            Destroy(gameObject);
        }
    }
}