# OtterGui

由 Ottermandias 開發的 ImGui 輔助函式庫，提供表格（`Table`）、篩選組合框、檔案系統瀏覽器
（`Filesystem`）、Raii 風格的 ImGui 包裝、文字排版擴充等元件，是艦隊裡最多插件共用的 UI 底層。

## 台服 fork 的目的

跟隨艦隊釘 API13.1 / net9（上游已上到 API15），並補了三處相容性修正：

- `OtterGui.csproj` 的 `Dalamud.NET.Sdk` 版本從 `15.0.0` 降回 `13.1.0`。
- `ObjectKind.Mount` 改為 `ObjectKind.MountType`——API13 的枚舉命名。
- `FrameworkManager.Dispose()` 的 `Task.WaitAll(tasks)` 加上 5 秒逾時，避免無限等待卡死。

## 與上游的差異

- 上述三處 API13 相容性修正。
- `.gitmodules` 的 `OtterGuiInternal` 指向艦隊自己的 fork。
- **降低記憶體用量**：表格、彈出選單、下拉選單、分頁、樹狀節點與 `Disabled`／`Enabled` 包裝在「沒有開起來、沒東西要還原」時，不再每次配置結束物件。

## 誰在用它

目前 5 個插件 repo 以子模組使用：`Artisan`、`AutoRetainer`、`GatherBuddyReborn`、`InventoryTools`、`Lifestream`。

---

上游原始碼：<https://github.com/Ottermandias/OtterGui>
