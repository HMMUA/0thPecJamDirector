using System.Collections;
using UnityEngine;

/// <summary>
/// 直播进度上报接口 - 测试用脚本
/// </summary>
public class LiveProgressTester : MonoBehaviour
{
    [Tooltip("拖入你刚刚写好的 LiveProgressReporter 脚本")]
    public LiveProgressReporter reporter;

    void Start()
    {
        // 如果没有手动拖拽，尝试在同一个物体上获取
        if (reporter == null)
        {
            reporter = GetComponent<LiveProgressReporter>();
        }

        if (reporter != null)
        {
            // 在这里自动填入你发我的 Authorization Token
            reporter.authorizationHeader = "Bearer u4H3JVukwthOOZbDMD9jlkmbi2o1q1v7M134cT";
            
            // 注意：如果你还需要填 Cookie，请在这里取消注释并填入，或者在 Inspector 面板里填
            // reporter.cookieHeader = "你的测试环境Cookie";
            
            Debug.Log("【测试工具】初始化成功！请在运行状态下按 1、2、3 键进行测试。");
        }
        else
        {
            Debug.LogError("【测试工具】找不到 LiveProgressReporter 脚本，请检查是否挂载！");
        }
    }

    void Update()
    {
        if (reporter == null) return;

        // 按键盘 1 键：单独测试“上报正在播放”
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            string testId = "yjtJPFfER5PVy2O8";
            Debug.Log($"hmmua正在上报稿件ID: {testId}");
            reporter.ReportNowPlaying(testId);
        }

        // 按键盘 2 键：单独测试“移除播放状态”
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("hmm正在Delete");
            reporter.ClearNowPlaying();
        }

        // 按键盘 3 键：测试“完整流程” (上报 -> 等待5秒 -> 移除)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(TestFullFlowRoutine());
        }
    }

    /// <summary>
    /// 自动化完整流程测试
    /// </summary>
    private IEnumerator TestFullFlowRoutine()
    {
        Debug.Log("【测试工具】>>> 开始执行完整自动化测试流程 <<<");

        string testEntryId = "auto_test_12345";
        
        Debug.Log($"【测试工具】第一步：上报稿件 [{testEntryId}]");
        reporter.ReportNowPlaying(testEntryId);

        Debug.Log("【测试工具】第二步：等待 5 秒钟，模拟正在播放...");
        yield return new WaitForSeconds(5f);

        Debug.Log("【测试工具】第三步：移除当前播放稿件...");
        reporter.ClearNowPlaying();

        Debug.Log("【测试工具】>>> 自动化测试流程结束 <<<");
    }
}