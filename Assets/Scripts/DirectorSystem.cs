using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Video;[RequireComponent(typeof(VideoPlayer))]


[System.Serializable]
public class VideoData
{
    //public string videoName; // 视频名称或ID（没用到）
    public VideoClip clip;   // 视频文件（如果通过拖拽或Resources加载）
    public string videoUrl;  // 视频路径（如果通过绝对路径/URL加载，比如StreamingAssets，暂时用不到）

    public string id; // 唯一标识符，方便引用

    public string stage;//场
    public string number;//编号

    public string artist;//曲师
    public string title;//曲名
    public string designer;//谱师

    public bool isBlackHorse; // 是否是黑马视频
    public bool isInterval; // 是否是过渡视频

    public bool isLoop; // 是否循环播放（如果需要的话）

}
public class DirectorSystem : MonoBehaviour
{
    // 定义视频数据结构（对应你表里的每一行数据）
    //[System.Serializable]
    public TextAsset tableData; // 用于存储表格数据的文本资产（CSV/JSON）

    [Header("是否用绝对路径加载")]
    public bool useAbsolutePath = true; // 是否使用绝对路径加载视频资源（如果为false，则使用Resources加载）


    [Header("视频加载位置")]
    public string videoResourcePath = @"E:\0th\Videos"; // 视频资源的路径（相对于 Resources 文件夹）

    [Header("播放器引用")]
    public VideoPlayer videoPlayer;




    [Header("播放列表")]
    public List<VideoData> playlist = new List<VideoData>();

    public TransitionController transitionController; // 过渡控制器引用（可选）
    public float preEndTriggerTime = 2.5f; // 触发预留函数的时间点（单位：秒）

    // 当前播放的索引
    public int currentIndex = -1;
    
    // 标记：当前视频是否已经触发过“倒数3秒”事件
    private bool hasTriggeredPreEnd = false;
    private bool isPlaying = false;

    void Awake()
    {
        // 自动获取 VideoPlayer 组件
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // 可选：监听视频完全播放结束的事件
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void Update()
    {
        // 核心逻辑：检查是否还差3秒结束
        if (videoPlayer.isPlaying && !hasTriggeredPreEnd && videoPlayer.length > 0)
        {
            // 计算剩余时间 (VideoPlayer 的时间单位是 double)
            double timeRemaining = videoPlayer.length - videoPlayer.time;

            // 如果剩余时间小于等于 3 秒，触发预留函数
            if (timeRemaining <= preEndTriggerTime)
            {
                hasTriggeredPreEnd = true;
                OnVideoAlmostEnded();
            }
        }
    }

    // ================== 功能 1：从表里导入播放列表 ==================
    /// <summary>
    /// 模拟从表格（CSV/JSON）导入播放列表
    /// </summary>
    public void LoadPlaylistFromTable()
    {
        // TODO: 在这里解析你的表格数据，并填充到 playlist 中
        // 示例：
        // playlist.Clear();
        // playlist.Add(new VideoData { videoName = "剧情1", videoUrl = "路径..." });
        playlist.Clear();
        tableData = Resources.Load<TextAsset>("VideoPlaylist"); // 假设你的表格数据放在 Resources 文件夹下
        string[] lines = tableData.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            if (line.StartsWith("#")) continue; // 跳过注释行
            if (string.IsNullOrWhiteSpace(line)) continue; // 跳过空行
            if (line.StartsWith("id")) continue; // 跳过表头行
            string[] fields = line.Split(','); // 假设 CSV 格式，字段用逗号分隔
            if (fields.Length >= 5) // 确保有足够的字段
            {
                VideoData data = new VideoData
                {
                    id = fields[0].Trim(),
                    stage = fields[1].Trim(),
                    number = fields[2].Trim(),
                    isBlackHorse = fields[3].Trim() == "1",
                    isInterval = fields[4].Trim() == "1",
                    artist = fields[5].Trim(),
                    title = fields[6].Trim(),
                    designer = fields[7].Trim(),
                    isLoop = fields[8].Trim() == "0"
                };
                playlist.Add(data);
            }
            else
            {
                Debug.LogWarning("表格行格式不正确，跳过： " + line);
            }
        }
        
        Debug.Log("播放列表已成功从表格导入，共 " + playlist.Count + " 个视频。");
        LinkVideoResources(); // 导入后自动链接视频资源
    }

    public void LinkVideoResources()
    {
        if (useAbsolutePath)
        {
            // 如果使用绝对路径加载视频资源，直接在播放时设置 URL 即可，无需预先加载 VideoClip
            foreach (VideoData data in playlist)
            {
                string fileName = data.id + "_Video.mp4";
                string absolutePath = Path.Combine(videoResourcePath, fileName);
                if (File.Exists(absolutePath))
                {
                    data.videoUrl = absolutePath;
                    Debug.Log($"已设置视频 URL：{absolutePath}");
                }
                else
                {
                    Debug.LogWarning($"未找到视频文件：{absolutePath}，请检查路径和文件名！");
                }
            }

            Debug.Log("使用绝对路径加载视频资源，将在播放时设置 URL。");
            return;
        }
        foreach (VideoData data in playlist)
        {
            // 假设视频资源命名规则为 "Video_{id}"，并放在 Resources/Videos 文件夹下
            string fileName = data.id +"_Video.mp4";
            string resourcePath = Path.Combine(videoResourcePath, fileName);
            
            VideoClip clip = Resources.Load<VideoClip>(resourcePath);


            if (clip != null)
            {
                data.clip = clip;
                Debug.Log($"成功加载视频资源：{resourcePath}");
            }
            else
            {
                Debug.LogWarning($"未找到视频资源：{resourcePath}，请检查命名和路径！");
            }
        }
    }

    // ================== 功能 2：开始播放 ==================
    /// <summary>
    /// 开始播放列表（从第0个开始）
    /// </summary>
    public void StartPlaylist()
    {
        if (playlist.Count == 0)
        {
            Debug.LogWarning("播放列表为空，无法播放！");
            return;
        }

        currentIndex = 0;
        PlayVideo(currentIndex);
    }


    // ================== 功能 3：预留函数（还差3秒结束时触发） ==================
    /// <summary>
    /// 当视频还剩 3 秒结束时触发的预留函数
    /// </summary>
    private void OnVideoAlmostEnded()
    {
        isPlaying = false;
        Debug.Log($"<color=green>【触发事件】</color> 视频 '{playlist[currentIndex].id}' 还差3秒结束！");
        
        // TODO: 在这里写你的逻辑
        // 比如：弹出UI选项、提前加载下一个视频资源、淡出画面等
        // 示例：如果有过渡控制器，可以在这里调用它的过渡函数
        if (transitionController != null && currentIndex + 1 < playlist.Count)
         transitionController.StartTransition(playlist[currentIndex+1]);
        else Debug.LogWarning("过渡控制器未设置或没有下一个视频，无法触发过渡！");

    }


    // ================== 功能 4：播放下一个视频 ==================
    /// <summary>
    /// 外部调用此函数，立刻开始播放下一个视频
    /// </summary>
    public void PlayNextVideo()
    {
        

        currentIndex++;
        
        // 检查是否播放到了列表末尾
        if (currentIndex >= playlist.Count)
        {
            Debug.Log("播放列表已全部播放完毕！");
            videoPlayer.Stop();
            return;
        }

        PlayVideo(currentIndex);
    }

    public void OnClickNextButton()
    {
        if (isPlaying)
        {
            SkipToEnd();
        }
        else
        {
            Debug.LogWarning("当前没有视频在播放，无法跳过！");
        }
    }

    
    // ================== 内部辅助播放方法 ==================
    private void PlayVideo(int index)
    {
        VideoData currentData = playlist[index];

        // 判断是使用本地 Clip 还是 URL 路径
        if (currentData.clip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = currentData.clip;
        }
        else if (!string.IsNullOrEmpty(currentData.videoUrl))
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = currentData.videoUrl;
        }
        else
        {
            Debug.LogError($"视频 '{currentData.id}' 没有关联的 VideoClip，无法播放！");
            return;
        }

        // 重置触发标记
        hasTriggeredPreEnd = false;
        
        // 开始播放
        videoPlayer.Play();
        isPlaying = true;
        Debug.Log($"正在播放第 {index + 1} 个视频：{currentData.id}");

    }

    /// <summary>
    /// 当视频自然播放到最后一帧时的事件回调
    /// </summary>
    private void OnVideoFinished(VideoPlayer vp)
    {
        // 如果你需要视频播完自动切下一个，取消下面这行的注释：
        // PlayNextVideo();
    }

    public void SkipToEnd()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.time = videoPlayer.length - preEndTriggerTime - 2f; // 跳到视频末尾
            Debug.Log("已跳到视频末尾！");
        }
    }
}