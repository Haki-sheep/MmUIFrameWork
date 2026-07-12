# Mm UI FrameWork

HakiMm UI 运行时与编辑器工具。

## 本地开发

代码在 `Assets/MmUIFrame/` 下 直接改 报错正常可见。

## 业务工程 Git 引用

```json
"com.hakisheep.mm-uiframe": "git@github.com:Haki-sheep/MmUIFrameWork.git?path=Assets/MmUIFrame"
```

版本号请自行在 `Assets/MmUIFrame/package.json` 与 git tag 中控制。

## 目录

- `Assets/MmUIFrame/Runtime` UI 运行时与 DoTweenAnim
- `Assets/MmUIFrame/Editor` UI 生成器 Hierarchy 绑定 DOTweenSequence 编辑器
- `Assets/MmUIFrame/Prefabs` UIRoot / UITemple 模板
- `Assets/Plugins` 本地开发用 DOTween / Odin（不进包）

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
