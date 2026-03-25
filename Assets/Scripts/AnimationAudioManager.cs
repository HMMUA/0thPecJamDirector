using UnityEngine;

[RequireComponent(typeof(AudioSource))] // 自动添加AudioSource组件
public class AnimationAudioManager : MonoBehaviour
{
    [Header("播放器设置")][Tooltip("用于播放声音的AudioSource，如果不填会自动获取物体上的组件")]
    public AudioSource audioSource;[Header("音效列表 (用于索引或名称播放)")]
    [Tooltip("在这里配置所有可能用到的音效")]
    public AudioClip[] audioClips;

    private void Awake()
    {
        // 自动获取自身挂载的AudioSource组件
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// 方法1：通过索引播放（在Animation Event的 Int 参数中填入数字）
    /// 优点：最高效，不用每次拖拽音效文件。
    /// </summary>
    public void PlayAudioByIndex(int index)
    {
        if (index >= 0 && index < audioClips.Length)
        {
            if (audioClips[index] != null)
            {
                audioSource.PlayOneShot(audioClips[index]);
            }
        }
        else
        {
            Debug.LogWarning($"[AnimationAudioManager] 索引越界！当前索引: {index}，数组长度: {audioClips.Length}", this);
        }
    }

    /// <summary>
    /// 方法2：通过名称播放（在Animation Event的 String 参数中填入音效名称）
    /// 优点：最直观，方便知道当前帧播放的是什么声音。
    /// </summary>
    public void PlayAudioByName(string clipName)
    {
        foreach (var clip in audioClips)
        {
            if (clip != null && clip.name == clipName)
            {
                audioSource.PlayOneShot(clip);
                return;
            }
        }
        Debug.LogWarning($"[AnimationAudioManager] 未在列表中找到名称为 '{clipName}' 的音效！", this);
    }

    /// <summary>
    /// 方法3：直接传入音效播放（在Animation Event的 Object 参数中直接拖入AudioClip）
    /// 优点：不需要在脚本列表中提前配置，想播什么直接拖。
    /// </summary>
    public void PlayAudioDirectly(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("[AnimationAudioManager] 传入的AudioClip为空！", this);
        }
    }
}