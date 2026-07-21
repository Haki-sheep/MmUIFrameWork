# Mm UI Frame (`com.hakisheep.mm-uiframe`)

通用 UI 框架包：窗口栈、Addressable 加载、StandUI、DOTween 序列、跳字、物品轮盘。  
不含具体游戏窗口类与玩法数据。

## 引用方式

开发期（可写）：

```json
"com.hakisheep.mm-uiframe": "file:C:/UnityProject/MmUIFrameWork/Assets/MmUIFrameWork"
```

或相对路径：

```json
"com.hakisheep.mm-uiframe": "file:../../MmUIFrameWork/Assets/MmUIFrameWork"
```

稳定期（Git）：

```json
"com.hakisheep.mm-uiframe": "https://github.com/Haki-sheep/MmUIFrameWork.git?path=Assets/MmUIFrameWork#v1.1.0"
```

## 宿主必须自备（非 UPM）

| 依赖 | 说明 |
|------|------|
| DOTween（含 Modules） | Asset Store / Demigiant 插件 |
| Odin Inspector | 跳字 / 轮盘 Inspector 特性 |

## UPM 依赖（已声明）

- `com.unity.addressables`
- `com.unity.ugui`（含 TMP）
- `com.unity.inputsystem`
- `com.cysharp.unitask`

## API 入口

| 类 | 用途 |
|----|------|
| `UICoreMgr` | 窗口显示/隐藏/栈 |
| `UIDataBase` / `UIWindowBase` | 窗口基类 |
| `UIAddressable` / `UILoad` | Addressable 实例化 |
| `DOTweenSequence` | UI 动画序列组件 |
| `FloatingTextManager` / `FloatingTextWorld` | 跳字 |
| `ItemWheelController` | 物品/武器轮盘 |

## 目录

```text
com.hakisheep.mm-uiframe/
├── package.json
├── MieMieFrameWork.UI.asmdef   # 覆盖 Runtime + Widgets
├── Runtime/                    # 窗口栈 加载器
├── Widgets/                    # DOTween序列 跳字 轮盘
├── StandUIPrefabs/             # 标准控件预制体
├── Editor/                     # 生成器 烘焙 自定义 Inspector
└── Samples~/Demo/              # 可选试玩场景
```
