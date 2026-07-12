# Mm UI FrameWork

HakiMm UI 运行时与编辑器工具的独立 UPM 包。

## 包路径

业务工程通过 Git 引用：

```json
"com.hakisheep.mm-uiframe": "git@github.com:Haki-sheep/MmUIFrameWork.git?path=unity"
```

版本号请自行在 `unity/package.json` 与 git tag 中控制。

## 目录

- `unity/Runtime` UI 运行时与 DoTweenAnim
- `unity/Editor` UI 生成器 Hierarchy 绑定 DOTweenSequence 编辑器（随包附带 无需单独导入）
- `unity/Prefabs` UIRoot / UITemple 模板
- `Assets/Plugins` 本仓库本地开发用 DOTween / Odin（不进入 UPM 包）

## 宿主依赖

- UniTask
- DOTween（`DOTween.dll`）
- Addressables
- uGUI

## 框架钩子

HakiSheep `ModuleHub` 通过反射绑定 `UICoreMgr`：

```csharp
var ui = ModuleHub.Instance.GetUI<MieMieFrameWork.UI.UICoreMgr>();
```
