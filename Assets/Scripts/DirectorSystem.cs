using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.Video;
using UnityEngine.UI;

[RequireComponent(typeof(VideoPlayer))]


[System.Serializable]
public class VideoData
{
    //public string videoName; // 视频名称或ID（没用到）
    public VideoClip clip;   // 视频文件（到时候再读/解析）
    public string videoUrl;  // 视频路径（如果通过绝对路径/URL加载，比如StreamingAssets，暂时用不到）
    public string videoInternetUrl; // 视频的互联网URL
    public string imageInternetUrl; // 视频封面图片的互联网URL（如果需要显示封面的话）
    public Sprite coverSprite; // 加载后的封面图片 Sprite，用于在其他地方使用

    public string id; // 唯一标识符，方便引用

    public string submissionTrack;//场（字母部分）
    public int isMainStage; // 是否内场（1是0否，2啥都不是）
    public string number;//编号（数字部分）

    public string artist;//曲师
    public string title;//曲名
    public string designer;//谱师

    public bool isDarkHorse; // 是否是黑马视频
    public bool isInterval; // 是否是过渡视频（节目单里不需要）

    public bool isLoop; // 是否循环播放（节目单里不需要）

    

}
public class DirectorSystem : MonoBehaviour
{
    [Header("是否使用测试视频")]
    public bool isUsingTestVideo;

    [Header("是否使用链接视频源")]
    public bool isUsingInternetVideo;

    [Header("是否使用链接封面图片")]
    public bool isUsingInternetImage;

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

    // 首个网络视频是否已准备好播放（已预加载/可用）
    public bool firstInternetVideoReady { get; private set; } = false;
    private bool firstInternetVideoNotified = false;

    // 首个网络封面图片是否已准备好（已预加载/可用）
    public bool firstInternetImageReady { get; private set; } = false;
    private bool firstInternetImageNotified = false;

    // 事件回调：首个视频可播放
    public delegate void FirstVideoReadyCallback(VideoData data);
    public event FirstVideoReadyCallback OnFirstVideoReady;

    // 事件回调：首个封面图片可使用
    public delegate void FirstImageReadyCallback(VideoData data);
    public event FirstImageReadyCallback OnFirstImageReady;

    // 事件回调：过渡视频触发
    public delegate void IntervalEventHandler(VideoData data);
    public event IntervalEventHandler OnIntervalEvent;

    // 标记：当前视频是否已经触发过“倒数3秒”事件
    private bool hasTriggeredPreEnd = false;
    private bool isPlaying = false;

    // 标记：是否启用“视频即将结束”检查（循环视频默认禁用）
    private bool enablePreEndCheck = true;

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
        // 核心逻辑：检查是否还差3秒结束（仅在启用检查时）
        if (enablePreEndCheck && videoPlayer.isPlaying && !hasTriggeredPreEnd && videoPlayer.length > 0)
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
        playlist.Clear();
        tableData = Resources.Load<TextAsset>("VideoPlaylist"); // 假设你的表格数据放在 Resources 文件夹下
        string[] lines = tableData.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length == 0)
        {
            Debug.LogWarning("表格数据为空！");
            return;
        }
        
        // 找到表头行（假设第一行是表头）
        string headerLine = lines[0];
        List<string> headerFields = SplitCsvLine(headerLine);
        Dictionary<string, int> columnIndex = new Dictionary<string, int>();
        for (int i = 0; i < headerFields.Count; i++)
        {
            columnIndex[headerFields[i].Trim()] = i;
        }
        
        foreach (string line in lines)
        {
            if (line.StartsWith("#")) continue; // 跳过注释行
            if (string.IsNullOrWhiteSpace(line)) continue; // 跳过空行
            if (line == headerLine) continue; // 跳过表头行
            
            List<string> fields = SplitCsvLine(line); // 支持带引号的字段中包含逗号
            if (fields.Count < headerFields.Count) // 确保字段数量匹配
            {
                Debug.LogWarning("表格行字段数量不匹配，跳过： " + line);
                continue;
            }
            
            VideoData data = new VideoData();
            
            // 根据表头名字自动映射字段
            if (columnIndex.ContainsKey("id"))
                data.id = fields[columnIndex["id"]].Trim();
            if (columnIndex.ContainsKey("submissionTrack"))
                data.submissionTrack = fields[columnIndex["submissionTrack"]].Trim();
            if (columnIndex.ContainsKey("isMainStage"))
                data.isMainStage = fields[columnIndex["isMainStage"]].Trim() == "1" ? 1 : (fields[columnIndex["isMainStage"]].Trim() == "2" ? 2 : 0);
            if (columnIndex.ContainsKey("number"))
                data.number = fields[columnIndex["number"]].Trim();
            if (columnIndex.ContainsKey("isDarkHorse"))
                data.isDarkHorse = fields[columnIndex["isDarkHorse"]].Trim() == "1";
            if (columnIndex.ContainsKey("isInterval"))
                data.isInterval = fields[columnIndex["isInterval"]].Trim() == "1";
            if (columnIndex.ContainsKey("artist"))
                data.artist = fields[columnIndex["artist"]].Trim();
            if (columnIndex.ContainsKey("title"))
                data.title = fields[columnIndex["title"]].Trim();
            if (columnIndex.ContainsKey("designer"))
                data.designer = fields[columnIndex["designer"]].Trim();
            if (columnIndex.ContainsKey("isLoop"))
                data.isLoop = fields[columnIndex["isLoop"]].Trim() == "1";
            if (columnIndex.ContainsKey("videoInternetUrl"))
                data.videoInternetUrl = fields[columnIndex["videoInternetUrl"]].Trim();
            if (columnIndex.ContainsKey("imageInternetUrl"))
                data.imageInternetUrl = fields[columnIndex["imageInternetUrl"]].Trim();
            
            // 检查必需字段
            if (string.IsNullOrEmpty(data.id))
            {
                Debug.LogWarning("缺少 id 字段，跳过行： " + line);
                continue;
            }
            
            playlist.Add(data);
        }
        
        Debug.Log("播放列表已成功从表格导入，共 " + playlist.Count + " 个视频。");
        LinkVideoResources(); // 导入后自动链接视频资源
    }

    /// <summary>
    /// 简单 CSV 解析（支持用双引号包含字段，并允许字段中包裹逗号或双引号转义）
    /// </summary>
    /// <param name="line">一行 CSV 文本</param>
    /// <returns>解析后的字段列表</returns>
    private List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        if (line == null)
            return result;

        bool inQuotes = false;
        var field = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 双引号转义
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    result.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }
        }

        result.Add(field.ToString());
        return result;
    }

    public void LinkVideoResources()
    {
        // 如果使用网络封面图片，启动预加载
        if (isUsingInternetImage)
        {
            PreloadAllNetworkImages();
        }

        if(isUsingTestVideo)
        {
            string fileName = "TestVideo.mp4"; // 替换为你的测试视频文件名
            string testPath = Path.Combine(videoResourcePath, fileName);
            foreach (VideoData data in playlist)
            {
                
                if(File.Exists(testPath))
                {
                    data.videoUrl = testPath;
                }
                else
                {
                    Debug.LogWarning($"未找到测试视频文件：{testPath}，请检查路径和文件名！");
                }
            }
            Debug.Log("已链接测试视频资源。");
            return;
        }

        if (isUsingInternetVideo)
        {
            foreach (VideoData data in playlist)
            {
                if (data.isInterval)
                {
                    Debug.Log($"跳过过渡视频 {data.id} 的网络链接，保持原有设置。");
                    continue;
                }

                if (!string.IsNullOrEmpty(data.videoInternetUrl))
                {
                    data.videoUrl = data.videoInternetUrl;
                    Debug.Log($"已设置网络视频 URL：{data.id} -> {data.videoInternetUrl}");
                }
                else
                {
                    Debug.LogWarning($"视频 {data.id} 没有 videoInternetUrl，无法设置网络 URL。");
                }
            }

            Debug.Log("已链接网络视频资源（videoInternetUrl -> videoUrl）。");
            // 开始预加载。如果想在脚本启动时自动预加载，可在外部调用此方法。
            PreloadAllNetworkVideos();
            return;
        }
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

    // ================== 功能 1.5：网络URL预加载（可提前执行） ==================
    /// <summary>
    /// 对当前 playlist 中的网络视频 URL 进行可达性检测，并提前准备首个视频文件
    /// </summary>
    public void PreloadAllNetworkVideos()
    {
        if (playlist.Count == 0)
        {
            Debug.LogWarning("播放列表为空，无法预加载网络视频！");
            return;
        }

        firstInternetVideoReady = false;
        firstInternetVideoNotified = false;
        StartCoroutine(PreloadAllNetworkVideosCoroutine());
    }

    private IEnumerator PreloadAllNetworkVideosCoroutine()
    {
        VideoData firstReadyData = null;

        foreach (var data in playlist)
        {
            if (string.IsNullOrEmpty(data.videoInternetUrl))
            {
                if(data.isInterval)
                {
                    Debug.Log($"跳过过渡视频 {data.id} 的网络预加载，保持原有设置。");
                }
                else
                {
                    Debug.LogWarning($"视频 {data.id} 没有 videoInternetUrl，跳过网络预加载。");
                }
                //Debug.LogWarning($"视频 {data.id} 丢失网络 URL，跳过预加载。");
                continue;
            }

            using (UnityWebRequest request = UnityWebRequest.Head(data.videoInternetUrl))
            {
                request.timeout = 15;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"网络视频预加载失败：{data.id} {data.videoInternetUrl}，错误：{request.error}");
                }
                else
                {
                    Debug.Log($"网络视频可访问：{data.id} {data.videoInternetUrl}");

                    if (firstReadyData == null)
                    {
                        firstReadyData = data;
                        firstInternetVideoReady = true;
                        NotifyFirstVideoReady(data);
                        StartCoroutine(PrepareFirstVideoCoroutine(data));
                    }
                }
            }
        }

        if (firstReadyData == null)
        {
            Debug.LogWarning("没有可用的网络视频 URL，可尝试检查数据或网络连接。");
        }
        else
        {
            Debug.Log("已完成所有网络视频预加载检测。");
        }
    }

    private IEnumerator PrepareFirstVideoCoroutine(VideoData data)
    {
        if (string.IsNullOrEmpty(data.videoInternetUrl))
            yield break;

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = data.videoInternetUrl;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        Debug.Log($"网络视频准备就绪：{data.id}，可以开始播放。");

        // 预加载成功后不自动播放，保留当前播放器状态。
        videoPlayer.Stop();
    }

    private void NotifyFirstVideoReady(VideoData data)
    {
        if (firstInternetVideoNotified) return;

        firstInternetVideoNotified = true;
        Debug.Log($"<color=green>网络视频已加载完毕：{data.id}</color>");

        if (OnFirstVideoReady != null)
        {
            OnFirstVideoReady.Invoke(data);
        }
    }

    // ================== 功能 1.7：网络封面图片预加载（可提前执行） ==================
    /// <summary>
    /// 对当前 playlist 中的网络封面图片 URL 进行可达性检测，并提前准备首个图片文件
    /// </summary>
    public void PreloadAllNetworkImages()
    {
        if (playlist.Count == 0)
        {
            Debug.LogWarning("播放列表为空，无法预加载网络封面图片！");
            return;
        }

        firstInternetImageReady = false;
        firstInternetImageNotified = false;
        StartCoroutine(PreloadAllNetworkImagesCoroutine());
    }

    private IEnumerator PreloadAllNetworkImagesCoroutine()
    {
        VideoData firstReadyData = null;

        foreach (var data in playlist)
        {
            if (string.IsNullOrEmpty(data.imageInternetUrl))
            {
                Debug.LogWarning($"视频 {data.id} 没有网络封面图片 URL，跳过预加载。");
                continue;
            }

            using (UnityWebRequest request = UnityWebRequest.Head(data.imageInternetUrl))
            {
                request.timeout = 15;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"网络封面图片预加载失败：{data.id} {data.imageInternetUrl}，错误：{request.error}");
                }
                else
                {
                    Debug.Log($"网络封面图片可访问：{data.id} {data.imageInternetUrl}");

                    // 如果是第一个可用封面，触发一次事件通知
                    if (firstReadyData == null)
                    {
                        firstReadyData = data;
                        firstInternetImageReady = true;
                        NotifyFirstImageReady(data);
                    }

                    // 无论是否为第一个，全部发起准备协程（异步下载并保存coverSprite）
                    StartCoroutine(PrepareFirstImageCoroutine(data));
                }
            }
        }

        if (firstReadyData == null)
        {
            Debug.LogWarning("没有可用的网络封面图片 URL，可尝试检查数据或网络连接。");
        }
        else
        {
            Debug.Log("已完成所有网络封面图片预加载检测。");
        }
    }

    private IEnumerator PrepareFirstImageCoroutine(VideoData data)
    {
        if (string.IsNullOrEmpty(data.imageInternetUrl))
            yield break;

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(data.imageInternetUrl))
        {
            request.timeout = 15;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"网络封面图片准备失败：{data.id} {data.imageInternetUrl}，错误：{request.error}");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    data.coverSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    Debug.Log($"网络封面图片准备就绪：{data.id}，Sprite 已存储。");
                }
                else
                {
                    Debug.LogWarning($"网络封面图片准备失败：{data.id} {data.imageInternetUrl}，纹理为空");
                }
            }
        }
    }

    private void NotifyFirstImageReady(VideoData data)
    {
        if (firstInternetImageNotified) return;

        firstInternetImageNotified = true;
        Debug.Log($"<color=blue>首个网络封面图片已加载完毕：{data.id}</color>");

        if (OnFirstImageReady != null)
        {
            OnFirstImageReady.Invoke(data);
        }
    }

    /// <summary>
    /// 触发过渡视频的特殊转场和事件
    /// </summary>
    /// <param name="data">过渡视频数据</param>
    private void TriggerInterval(VideoData data)
    {
        Debug.Log($"<color=yellow>触发过渡视频：{data.id}</color>");
        
        // 触发特殊转场
        if (transitionController != null)
        {
            transitionController.StartIntervalTransition(data);
        }
        else
        {
            Debug.LogWarning("过渡控制器未设置，无法触发转场！");
        }
        
        // 触发特殊事件
        if (OnIntervalEvent != null)
        {
            OnIntervalEvent.Invoke(data);
        }
    }

    // ================== 功能 1.6：从URL加载封面图片并存储为Sprite ==================
    /// <summary>
    /// 从指定的 URL 加载封面图片并存储到 VideoData.coverSprite 中
    /// </summary>
    /// <param name="data">要加载封面的 VideoData</param>
    public void LoadCoverSpriteForVideoData(VideoData data)
    {
        if (data == null)
        {
            Debug.LogWarning("VideoData 为空，无法加载封面！");
            return;
        }

        if (string.IsNullOrEmpty(data.imageInternetUrl))
        {
            Debug.LogWarning($"VideoData {data.id} 的 imageInternetUrl 为空，无法加载封面！");
            return;
        }

        StartCoroutine(LoadCoverSpriteCoroutine(data));
    }

    private IEnumerator LoadCoverSpriteCoroutine(VideoData data)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(data.imageInternetUrl))
        {
            request.timeout = 15;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"封面图片加载失败：{data.id} {data.imageInternetUrl}，错误：{request.error}");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    // 创建 Sprite 并存储到 VideoData
                    data.coverSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    Debug.Log($"封面 Sprite 加载成功并存储：{data.id} {data.imageInternetUrl}");
                }
                else
                {
                    Debug.LogWarning($"封面图片加载失败：{data.id} {data.imageInternetUrl}，纹理为空");
                }
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
        
        // 检查下一个视频
        if (currentIndex + 1 < playlist.Count)
        {
            VideoData nextData = playlist[currentIndex + 1];
            if (nextData.isInterval)
            {
                // 如果下一个是过渡视频，触发特殊的转场
                TriggerInterval(nextData);
            }
            else if (transitionController != null)
            {
                // 否则，触发普通转场
                transitionController.StartTransition(nextData);
            }
        }
        else
        {
            Debug.LogWarning("没有下一个视频，无法触发过渡！");
        }
    }


    // ================== 功能 4：播放下一个视频 ==================
    /// <summary>
    /// 外部调用此函数，立刻开始播放下一个视频
    /// </summary>
    public void PlayNextVideo()
    {
        ShowVideo(); // 确保视频画面可见（如果之前调用过 HideVideo）

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

        // 如果是过渡视频，使用绝对路径加载本地视频文件
        if (currentData.isInterval)
        {
            string fileName = currentData.id + ".mp4";
            string absolutePath = Path.Combine(videoResourcePath, fileName);
            if (File.Exists(absolutePath))
            {
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = absolutePath;
            }
            else
            {
                Debug.LogError($"过渡视频文件不存在：{absolutePath}");
                return;
            }
        }
        else
        {
            // 判断是使用本地 Clip、绝对路径 URL、或互联网 URL
            if (currentData.clip != null)
            {
                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = currentData.clip;
            }
            else if (isUsingInternetVideo && !string.IsNullOrEmpty(currentData.videoInternetUrl))
            {
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = currentData.videoInternetUrl;
            }
            else if (!string.IsNullOrEmpty(currentData.videoUrl))
            {
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = currentData.videoUrl;
            }
            else
            {
                Debug.LogError($"视频 '{currentData.id}' 没有关联的 VideoClip 或 URL，无法播放！");
                return;
            }
        }

        // 加载封面 Sprite（如果有 URL 且未加载）
        if (isUsingInternetImage && !string.IsNullOrEmpty(currentData.imageInternetUrl) && currentData.coverSprite == null)
        {
            LoadCoverSpriteForVideoData(currentData);
        }

        // 重置触发标记
        hasTriggeredPreEnd = false;
        
        // 开始播放
        videoPlayer.Play();
        isPlaying = true;
        Debug.Log($"正在播放第 {index + 1} 个视频：{currentData.id}");


        if (currentData.isLoop)
        {
            videoPlayer.isLooping = true;
            Debug.Log($"视频 {currentData.id} 设置为循环播放。");
        }
        else
        {
            videoPlayer.isLooping = false;
        }

        // 设置是否启用“视频即将结束”检查（循环视频默认禁用）
        enablePreEndCheck = !currentData.isLoop;
        

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

    public void HideVideo()
    {
        videoPlayer.targetCameraAlpha = 0f; // 将视频画面完全透明
    }

    public void ShowVideo()
    {
        videoPlayer.targetCameraAlpha = 1f; // 恢复视频画面可见
    }

    /// <summary>
    /// 启用“视频即将结束”检查（用于循环视频）
    /// </summary>
    public void EnablePreEndCheck()
    {
        enablePreEndCheck = true;
        videoPlayer.isLooping = false; // 取消循环播放，启用结束检查
        Debug.Log("已启用视频即将结束检查。");
    }
}