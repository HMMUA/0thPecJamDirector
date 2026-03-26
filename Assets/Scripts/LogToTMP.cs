using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LogToTMP : MonoBehaviour
{
    [Header("UI 引用")][Tooltip("拖入你的 TextMeshPro 或 TextMeshProUGUI 组件")]
    [SerializeField] private TMP_Text logTextDisplay;

    [Header("设置")][Tooltip("最多显示的日志条数，防止文本过长导致卡顿")]
    [SerializeField] private int maxLogCount = 30;

    // 用于存储日志的队列
    private Queue<string> logQueue = new Queue<string>();

    private void OnEnable()
    {
        // 订阅日志事件
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        // 取消订阅，防止内存泄漏
        Application.logMessageReceived -= HandleLog;
    }

    /// <summary>
    /// 处理接收到的日志
    /// </summary>
    /// <param name="logString">日志内容</param>
    /// <param name="stackTrace">堆栈信息</param>
    /// <param name="type">日志类型</param>
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 只筛选 Warning, Error 和 Exception
        if (type == LogType.Warning || type == LogType.Error || type == LogType.Exception)
        {
            // 为不同类型的日志设置不同颜色
            string color = type == LogType.Warning ? "yellow" : "red";
            
            // 使用 TMP 富文本格式
            string newLog = $"<color={color}>[{type}] {logString}</color>";

            // 进队
            logQueue.Enqueue(newLog);

            // 如果超出最大限制，移除最早的日志
            while (logQueue.Count > maxLogCount)
            {
                logQueue.Dequeue();
            }

            // 更新 TMP 的显示文本
            UpdateLogText();
        }
    }

    private void UpdateLogText()
    {
        if (logTextDisplay != null)
        {
            // 将队列中的所有日志用换行符连接起来
            logTextDisplay.text = string.Join("\n", logQueue);
        }
    }
    
    // （可选）提供一个公共方法用于手动清空屏幕日志
    public void ClearLogs()
    {
        logQueue.Clear();
        if (logTextDisplay != null) logTextDisplay.text = "";
    }
}