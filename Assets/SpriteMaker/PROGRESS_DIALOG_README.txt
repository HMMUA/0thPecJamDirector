╔════════════════════════════════════════════════╗
║     SpritesMaker2.jsx 进度条和中断功能说明      ║
╚════════════════════════════════════════════════╝

【新增功能】

✓ 进度条显示 - 实时显示处理进度
✓ 中断功能 - 用户可随时取消操作
✓ 状态文本 - 显示当前处理的文件编号


【使用说明】

【运行脚本】
1. 在 Photoshop 中打开 PSD 文件
2. 文件 → 脚本 → 浏览 → 选择 SpritesMaker2.jsx
3. 选择 CSV 数据文件
4. 选择导出文件夹
5. 进度对话框会弹出


【进度对话框】

┌─────────────────────────────┐
│      正在处理 Sprite...      │
│                             │
│ 进度：                      │
│ ███████░░░░░░░░░░░░ 35%    │
│                             │
│ 处理中... (35/100)          │
│                             │
│      [ 取消 ]               │
└─────────────────────────────┘

字段说明：
- 进度条：显示处理进度（0-100%）
- 处理中提示：显示当前处理的行数 (当前/总数)
- 取消按钮：点击可中断操作


【中断操作】

方式 1：点击对话框中的"取消"按钮
  → 立即中止处理
  → 已导出的文件保留
  → 弹出提示信息

方式 2：按 ESC 键关闭对话框
  → 效果同上

【进度更新频率】

- 每处理一行就更新一次进度
- 对话框实时响应
- 不会因为进度更新而卡顿


【技术细节】

【新增函数】
```javascript
// 创建进度对话框
function createProgressDialog(totalCount)
```

参数：
- totalCount: 总行数

返回：
- 对话框对象（包含 bar、statusText、cancelPressed 等属性）

【进度更新逻辑】
处理循环中每次迭代：
1. 更新进度条值 (progressDialog.bar.value)
2. 更新状态文本 (progressDialog.statusText.text)
3. 检查用户是否点击取消 (progressDialog.cancelPressed)
4. 若取消，关闭对话框并返回
5. 刷新UI (app.refresh())


【对话框属性】

- bar: 进度条对象（0-totalCount）
- statusText: 状态文本显示
- cancelPressed: 取消标志 (true/false)
- show(): 显示对话框
- close(): 关闭对话框


【错误处理】

脚本包含 try-catch 异常处理：
- 发生错误时捕获并显示错误消息
- 进度对话框会自动关闭
- 脚本安全退出


【性能考虑】

【进度更新开销】
- 每行更新一次进度（相对较频繁）
- 使用 app.refresh() 刷新UI

【优化建议】
- 如果需要处理超过 10000 行数据，可考虑：
  * 每 10 行更新一次进度（修改循环条件）
  * 或增加进度更新间隔

修改示例：
```javascript
// 每 10 行更新一次
if (i % 10 === 0 && progressDialog) {
    progressDialog.bar.value = i;
    ...
}
```


【常见场景】

【场景 1】处理 50 行数据
- 打开对话框
- 快速处理并自动关闭
- 显示完成提示

【场景 2】处理 500 行数据
- 打开对话框
- 逐行更新进度
- 用户可在任何时刻点击取消

【场景 3】处理 5000 行数据
- 可能需要数分钟
- 进度对话框持续显示
- 用户可跟踪进度

【场景 4】处理失败
- 出现错误时捕获异常
- 显示错误信息
- 对话框自动关闭
- 已导出的内容保留


【对话框样式】

- 类型: palette（浮动窗口，不阻塞主窗口）
- 可移动和调整大小
- 包含进度条、文本和取消按钮


【兼容性】

✓ Adobe Photoshop 2015 及以上
✓ 基于 ExtendScript（Photoshop 原生脚本引擎）
✓ Windows 和 Mac 系统


【改进建议】

如果需要进一步改进，可以：

1. 添加预估完成时间
   ```javascript
   var elapsedTime = new Date() - startTime;
   var estimatedTotal = elapsedTime * (totalCount / i);
   ```

2. 添加处理速度显示
   ```javascript
   var speed = i / (elapsedTime / 1000); // 每秒处理行数
   ```

3. 更详细的状态信息
   ```javascript
   progressDialog.statusText.text = 
       "正在处理: " + idVal + " (" + i + "/" + totalCount + ")";
   ```

4. 暂停功能（比取消更温和）
   ```javascript
   // 添加暂停/继续按钮
   ```


【调试】

如果进度对话框不显示：
1. 检查 Photoshop 版本（需要 2015+）
2. 确保脚本执行权限正确
3. 查看 Photoshop 错误日志

如果中断不工作：
1. 检查 cancelPressed 标志逻辑
2. 确保 app.refresh() 被调用

═══════════════════════════════════════════════
