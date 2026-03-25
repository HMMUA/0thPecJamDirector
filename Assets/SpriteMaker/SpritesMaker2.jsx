#target photoshop

// 强制设置单位为像素
app.preferences.rulerUnits = Units.PIXELS;

function main() {
    if (app.documents.length === 0) {
        alert("请先打开一个包含对应图层的 PSD 文件！");
        return;
    }

    var doc = app.activeDocument;

    // 选择 CSV 文件
    var csvFile = File.openDialog("请选择包含数据的 CSV 文件", "*.csv");
    if (csvFile === null) return;

    // 选择输出文件夹
    var outFolder = Folder.selectDialog("请选择 PNG 图片导出文件夹");
    if (outFolder === null) return;

    // 读取 CSV 内容
    csvFile.encoding = "UTF8";
    csvFile.open("r");
    var csvContent = csvFile.read();
    csvFile.close();

    // 解析 CSV 行（处理 \r\n 或 \n）
    var lines = csvContent.split(/\r\n|\n|\r/);
    if (lines.length < 2) {
        alert("CSV 文件内容为空或没有数据行！");
        return;
    }

    // 解析表头，获取列索引（使用正确的 CSV 解析）
    var headers = parseCSVLine(lines[0]);
    var colMap = {};
    for (var i = 0; i < headers.length; i++) {
        colMap[headers[i]] = i;
    }

    // 检查必要的列是否存在
    if (colMap["ID"] === undefined || colMap["Title"] === undefined || 
        colMap["Composers"] === undefined || colMap["Designers"] === undefined) {
        alert("CSV表头必须包含 ID, Title, Composers, Designers 这四列！\n当前表头为: " + headers.join(", "));
        return;
    }

    // 记录初始历史状态，以便每次循环后恢复（防止连续缩放导致字体大小失真，也会恢复点文本到原有的段落文本状态）
    var savedState = doc.activeHistoryState;

    // 创建进度对话框
    var progressDialog = createProgressDialog(lines.length - 1);
    
    // 遍历数据行
    for (var i = 1; i < lines.length; i++) {
        var line = lines[i];
        if (line.replace(/^\s+|\s+$/g, '') === "") continue; // 跳过空行

        // 更新进度
        if (progressDialog) {
            progressDialog.bar.value = i;
            progressDialog.statusText.text = "处理中... (" + i + "/" + (lines.length - 1) + ")";
            
            // 检查用户是否点击了取消
            if (progressDialog.cancelPressed) {
                alert("操作已被用户取消。");
                progressDialog.close();
                return;
            }

            // 处理事件，允许UI更新
            app.refresh();
        }

        // 使用正确的 CSV 解析方法（支持包含逗号的字段）
        var data = parseCSVLine(line);
        var idVal = data[colMap["ID"]];
        var titleVal = data[colMap["Title"]];
        var artistVal = data[colMap["Composers"]];
        var designerVal = data[colMap["Designers"]];

        // 定义图层名和对应的内容映射
        var layerMappings =[
            { name: "T", content: titleVal },
            { name: "T_O", content: titleVal },
            { name: "A", content: artistVal },
            { name: "A_O", content: artistVal },
            { name: "D", content: designerVal },
            { name: "D_O", content: designerVal }
        ];

        // 依次处理 6 个图层
        for (var j = 0; j < layerMappings.length; j++) {
            var mapping = layerMappings[j];
            var targetLayer = findLayerByName(doc, mapping.name);

            if (targetLayer !== null && targetLayer.kind === LayerKind.TEXT) {
                // 1. 替换文字（进行最后的数据清理）
                var newContent = mapping.content;
                if (newContent === null || newContent === undefined) {
                    newContent = "";
                } else {
                    newContent = newContent.toString().replace(/^\s+|\s+$/g, '');
                }
                targetLayer.textItem.contents = newContent;

                // 若文字不为空，则进行自适应缩放
                if (newContent !== "") {
                    
                    // 【修复核心1】：如果是段落文本（文本框），bounds不会随字数减少而缩小。将其临时强转为点文本。
                    if (targetLayer.textItem.kind === TextType.PARAGRAPHTEXT) {
                        targetLayer.textItem.kind = TextType.POINTTEXT;
                    }

                    // 【修复核心2】：复制一个临时层并读取，这是强迫PS立即刷新缓存并计算出最真实的文字物理像素边界
                    var tempLayer = targetLayer.duplicate();
                    var bounds = tempLayer.bounds;
                    tempLayer.remove();

                    // 2. 约束尺寸在最大宽度 2000 300 以内 (等比缩放)
                    var w = bounds[2].value - bounds[0].value;
                    var h = bounds[3].value - bounds[1].value;

                    if (w > 0 && h > 0) {
                        var MAX_W = 2000; 
                        var MAX_H = 300;
                        var scaleW = MAX_W / w;
                        var scaleH = MAX_H / h;
                        
                        // 取长宽缩放比例中较小的一个。
                        // 由于获取到了文字真实尺寸，当输入文字很少时，w 和 h 都很小，scaleRatio 就会大于 100，实现等比放大
                        var scaleRatio = Math.min(scaleW, scaleH) * 100; 

                        // 【可选限制】：如果你担心个别极短文字（例如仅1个字母）被放大得过于夸张，
                        // 可以解除下面这行代码的注释，限制最高放大倍率（例如最高放大为原来的 2.5 倍）
                        // if (scaleRatio > 250) scaleRatio = 250;

                        // 执行缩放
                        targetLayer.resize(scaleRatio, scaleRatio, AnchorPosition.MIDDLECENTER);
                    }
                }

                // 3. 导出类似“快速导出PNG”的单独图层
                var exportFileName = idVal + "_" + mapping.name;
                exportLayerAsPNG(targetLayer, outFolder, exportFileName);
            }
        }

        // 恢复初始状态，准备处理下一行（这也会把上方的点文本转换安全地撤销恢复为原始模板状态）
        doc.activeHistoryState = savedState;
    }

    // 关闭进度对话框
    if (progressDialog) {
        progressDialog.close();
    }

    alert("处理完成！所有图片已导出至: " + outFolder.fsName);
}

// 创建进度对话框
function createProgressDialog(totalCount) {
    var dlg = new Window("palette", "处理进度");

    // 标题文本
    dlg.add("statictext", undefined, "正在处理 Sprite...", {multiline: true});

    // 进度条
    var barGroup = dlg.add("group", undefined);
    barGroup.orientation = "column";
    barGroup.add("statictext", undefined, "进度：");
    var bar = barGroup.add("progressbar", undefined, 0, totalCount);
    bar.preferredSize = [400, 20];

    // 状态文本
    var statusText = dlg.add("statictext", undefined, "处理中... (0/" + totalCount + ")");
    statusText.characters = 40;

    // 按钮组
    var btnGroup = dlg.add("group", undefined);
    btnGroup.orientation = "row";
    btnGroup.add("button", undefined, "取消", { name: "cancel" });

    // 标志用来记录是否取消
    dlg.cancelPressed = false;

    btnGroup.children[0].onClick = function() {
        dlg.cancelPressed = true;
        dlg.close(1);
    };

    // 保存引用便于访问
    dlg.bar = bar;
    dlg.statusText = statusText;

    dlg.show();
    return dlg;
}

// CSV 行解析函数（支持包含逗号的字段和转义双引号）
function parseCSVLine(line) {
    var fields = [];
    var currentField = "";
    var inQuotes = false;

    for (var i = 0; i < line.length; i++) {
        var c = line[i];
        var nextChar = (i + 1 < line.length) ? line[i + 1] : "";

        if (c === '"') {
            if (inQuotes && nextChar === '"') {
                // 处理转义的双引号（""）
                currentField += '"';
                i++; // 跳过下一个引号
            } else {
                // 切换引号状态
                inQuotes = !inQuotes;
            }
        } else if (c === "," && !inQuotes) {
            // 逗号分隔符（仅在引号外）
            fields.push(currentField.replace(/^\s+|\s+$/g, '').replace(/^"|"$/g, ''));
            currentField = "";
        } else {
            currentField += c;
        }
    }

    // 添加最后一个字段
    if (currentField.length > 0 || fields.length > 0) {
        fields.push(currentField.replace(/^\s+|\s+$/g, '').replace(/^"|"$/g, ''));
    }

    return fields;
}

function findLayerByName(parent, name) {
    for (var i = 0; i < parent.layers.length; i++) {
        var layer = parent.layers[i];
        if (layer.name === name) {
            return layer;
        }
        if (layer.typename === "LayerSet") { // 如果是图层组，递归查找
            var found = findLayerByName(layer, name);
            if (found !== null) return found;
        }
    }
    return null;
}

// 导出单独图层为 PNG
function exportLayerAsPNG(layer, folder, fileName) {
    var mainDoc = app.activeDocument;
    
    // 创建一个透明的新文档，尺寸与主文档相同
    var tempDoc = app.documents.add(mainDoc.width, mainDoc.height, mainDoc.resolution, "TempExportDoc", NewDocumentMode.RGB, DocumentFill.TRANSPARENT);
    
    // 切回主文档，将图层复制到新文档
    app.activeDocument = mainDoc;
    layer.duplicate(tempDoc, ElementPlacement.PLACEATBEGINNING);
    
    // 切换到新文档进行裁切和导出
    app.activeDocument = tempDoc;
    
    // 裁切掉周围的透明像素（这就是右键快速导出的核心效果）
    tempDoc.trim(TrimType.TRANSPARENT);
    
    // 导出为 PNG-24 支持透明
    var file = new File(folder.fsName + "/" + fileName + ".png");
    var options = new ExportOptionsSaveForWeb();
    options.format = SaveDocumentType.PNG;
    options.PNG8 = false; // 保证是 PNG-24 带透明度
    options.transparency = true;
    
    tempDoc.exportDocument(file, ExportType.SAVEFORWEB, options);
    
    // 关闭临时文档且不保存
    tempDoc.close(SaveOptions.DONOTSAVECHANGES);
    
    // 切回主文档
    app.activeDocument = mainDoc;
}

// 执行主程序
try {
    main();
} catch (e) {
    alert("发生错误: " + e.toString() + "\n行号: " + e.line);
}