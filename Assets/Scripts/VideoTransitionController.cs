using UnityEngine;
using UnityEngine.Video;

public class VideoTransitionController : MonoBehaviour
{
    public VideoPlayer playerA;
    public VideoPlayer playerB;
    public Animator transition3DAnimator; // 控制3D转场动画的组件


    public float vedioEndingThreshold = 2.0f; // 视频快结束的时间阈值
    public float transitionDuration = 3.0f; // 转场动画的持续时间

    // 假设当前用A播放
    void Update()
    {
        // 检查视频A是否快播放结束 (例如还剩2秒)
        if (playerA.isPlaying && (playerA.length - playerA.time) < vedioEndingThreshold)
        {
            StartTransition();
        }
    }

    void StartTransition()
    {
        // 1. 播放3D转场动画
        if(transition3DAnimator != null)
        {
            transition3DAnimator.SetTrigger("PlayTransition");
        }
        else
        {
            Debug.LogWarning("Transition Animator is not assigned!");
        }
    }

    // 这个方法可以通过3D动画的 Animation Event 在动画遮挡屏幕的最高潮时调用
    public void OnScreenCovered() 
    {
        // 2. 切换视频输出
        playerA.Pause();
        playerB.Play();
        
        // 切换RawImage显示的RenderTexture到PlayerB的纹理
        // ...
    }
}