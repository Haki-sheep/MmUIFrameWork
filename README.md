# MieMieUIFrameWork

MieMie UI 运行时与编辑器工具。UPM 包名：`com.hakisheep.mm-uiframe`。  
菜单入口：`Tools/MieMieUIFrameWork/`。

## 本地开发

代码在 `Assets/MmUIFrameWork/` 下直接改，报错正常可见。  
宿主插件在 `Assets/Plugins`（DOTween / Odin），**不进包**。

## 业务工程引用

开发期（可写）：

```json
"com.hakisheep.mm-uiframe": "file:C:/UnityProject/MmUIFrameWork/Assets/MmUIFrameWork"
```

稳定期（Git）：

```json
"com.hakisheep.mm-uiframe": "https://github.com/Haki-sheep/MmUIFrameWork.git?path=Assets/MmUIFrameWork#v1.1.0"
```

版本号在 `Assets/MmUIFrameWork/package.json` 与 git tag 中控制。

## 目录

```text
Assets/MmUIFrameWork/          # UPM 包根
├── MieMieUIFrameWork.UI.asmdef  # 覆盖 Core + Runtime
├── Core/                      # 窗口栈 加载器 基类
├── Runtime/Widgets/           # DOTween序列 跳字 轮盘
├── StandUIPrefabs/            # 标准控件
├── Editor/                    # 生成器 / 烘焙 / Inspector
└── Samples~/Demo/             # 可选试玩场景
Assets/Plugins/                # 本地 DOTween / Odin（不进包）
```

## 宿主依赖

**UPM（包已声明）：** Addressables、uGUI、Input System、UniTask  

**需自备：** DOTween（含 Modules）、Odin Inspector

## 框架钩子

HakiSheep `ModuleHub` 通过反射绑定 `UICoreMgr`：

```csharp
var ui = ModuleHub.Instance.GetUI<MieMieUIFrameWork.UI.UICoreMgr>();
```

## 主工程胶水（留宿主）

1. 场景挂 `UICoreMgr` / `UIRoot`
2. 写具体窗口类（继承 `UIDataBase` / `UIWindowBase`）
3. Addressables 登记与类型名对齐的预制体地址
4. （可选）配置 UI 生成器输出目录；接轮盘/跳字的具体玩法数据
