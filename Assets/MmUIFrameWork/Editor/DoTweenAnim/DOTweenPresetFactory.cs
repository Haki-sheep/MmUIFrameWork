#if UNITY_EDITOR
using DG.Tweening;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 生成经典 UI DOTween 预设资源
/// </summary>
public static class DOTweenPresetFactory
{
    /// <summary>
    /// 输出目录
    /// </summary>
    private const string OutputFolder = "Assets/MmUIFrameWork/UIFrame/DoTweenAnim/Presets";

    [MenuItem("Tools/MieMieFrameWork/TestAndCreat/生成经典 UI 预设")]
    public static void GenerateClassicPresets()
    {
        GenerateClassicPresetsInternal(true);
    }

    /// <summary>
    /// 静默生成 供其他工具调用
    /// </summary>
    public static void GenerateClassicPresetsSilent()
    {
        GenerateClassicPresetsInternal(false);
    }

    [InitializeOnLoadMethod]
    private static void EnsureClassicPresetsExist()
    {
        // 工程首次打开时补齐缺失预设 已有则跳过
        string probePath = $"{OutputFolder}/UI_FadeIn.asset";
        if (AssetDatabase.LoadAssetAtPath<DOTweenSequencePreset>(probePath) != null) return;
        GenerateClassicPresetsInternal(false);
    }

    /// <summary>
    /// 生成经典预设
    /// </summary>
    private static void GenerateClassicPresetsInternal(bool showDialog)
    {
        EnsureFolder(OutputFolder);

        CreateOrUpdate("UI_PopupShow_ScaleFade", BuildPopupShow());
        CreateOrUpdate("UI_PopupHide_ScaleFade", BuildPopupHide());
        CreateOrUpdate("UI_FadeIn", BuildFadeIn());
        CreateOrUpdate("UI_FadeOut", BuildFadeOut());
        CreateOrUpdate("UI_SlideIn_FromBottom", BuildSlideFromBottom());
        CreateOrUpdate("UI_ButtonPunch_Scale", BuildButtonPunch());
        CreateOrUpdate("UI_Progress_Fill", BuildProgressFill());

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[DOTweenPresetFactory] 已生成经典预设到 {OutputFolder}");
        if (showDialog)
        {
            EditorUtility.DisplayDialog("完成", $"经典 UI 预设已生成\n{OutputFolder}", "确定");
        }
    }

    /// <summary>
    /// 弹窗显示 缩放+透明并行
    /// </summary>
    private static DOTweenSequencePreset BuildPopupShow()
    {
        var preset = ScriptableObject.CreateInstance<DOTweenSequencePreset>();
        var scale = MakeStep(
            AddType.Append,
            DOTweenType.DOScale,
            useFrom: true,
            from: new Vector4(0.8f, 0.8f, 0.8f, 0f),
            to: new Vector4(1f, 1f, 1f, 0f),
            duration: 0.3f,
            eEase: Ease.OutBack);
        var fade = MakeStep(
            AddType.Join,
            DOTweenType.DOCanvasGroupFade,
            useFrom: true,
            from: new Vector4(0f, 0f, 0f, 0f),
            to: new Vector4(1f, 0f, 0f, 0f),
            duration: 0.25f,
            eEase: Ease.OutQuad);
        preset.EditorSetData(new[] { scale, fade }, 0f, Ease.OutQuad, 1, LoopType.Restart, UpdateType.Normal, true);
        return preset;
    }

    /// <summary>
    /// 弹窗隐藏
    /// </summary>
    private static DOTweenSequencePreset BuildPopupHide()
    {
        var preset = ScriptableObject.CreateInstance<DOTweenSequencePreset>();
        var scale = MakeStep(
            AddType.Append,
            DOTweenType.DOScale,
            useFrom: true,
            from: new Vector4(1f, 1f, 1f, 0f),
            to: new Vector4(0.8f, 0.8f, 0.8f, 0f),
            duration: 0.2f,
            eEase: Ease.InBack);
        var fade = MakeStep(
            AddType.Join,
            DOTweenType.DOCanvasGroupFade,
            useFrom: true,
            from: new Vector4(1f, 0f, 0f, 0f),
            to: new Vector4(0f, 0f, 0f, 0f),
            duration: 0.2f,
            eEase: Ease.InQuad);
        preset.EditorSetData(new[] { scale, fade }, 0f, Ease.InQuad, 1, LoopType.Restart, UpdateType.Normal, true);
        return preset;
    }

    /// <summary>
    /// 淡入
    /// </summary>
    private static DOTweenSequencePreset BuildFadeIn()
    {
        var preset = ScriptableObject.CreateInstance<DOTweenSequencePreset>();
        var fade = MakeStep(
            AddType.Append,
            DOTweenType.DOFade,
            useFrom: true,
            from: new Vector4(0f, 0f, 0f, 0f),
            to: new Vector4(1f, 0f, 0f, 0f),
            duration: 0.25f,
            eEase: Ease.OutQuad);
        preset.EditorSetData(new[] { fade }, 0f, Ease.OutQuad, 1, LoopType.Restart, UpdateType.Normal, true);
        return preset;
    }

    /// <summary>
    /// 淡出
    /// </summary>
    private static DOTweenSequencePreset BuildFadeOut()
    {
        var preset = ScriptableObject.CreateInstance<DOTweenSequencePreset>();
        var fade = MakeStep(
            AddType.Append,
            DOTweenType.DOFade,
            useFrom: true,
            from: new Vector4(1f, 0f, 0f, 0f),
            to: new Vector4(0f, 0f, 0f, 0f),
            duration: 0.2f,
            eEase: Ease.InQuad);
        preset.EditorSetData(new[] { fade }, 0f, Ease.InQuad, 1, LoopType.Restart, UpdateType.Normal, true);
        return preset;
    }

    /// <summary>
    /// 自下而上滑入
    /// </summary>
    private static DOTweenSequencePreset BuildSlideFromBottom()
    {
        var preset = ScriptableObject.CreateInstance<DOTweenSequencePreset>();
        var slide = MakeStep(
            AddType.Append,
            DOTweenType.DOAnchorPosY,
            useFrom: true,
            from: new Vector4(-400f, 0f, 0f, 0f),
            to: new Vector4(0f, 0f, 0f, 0f),
            duration: 0.35f,
            eEase: Ease.OutCubic);
        preset.EditorSetData(new[] { slide }, 0f, Ease.OutCubic, 1, LoopType.Restart, UpdateType.Normal, true);
        return preset;
    }

    /// <summary>
    /// 按钮按下回弹
    /// </summary>
    private static DOTweenSequencePreset BuildButtonPunch()
    {
        var preset = ScriptableObject.CreateInstance<DOTweenSequencePreset>();
        var press = MakeStep(
            AddType.Append,
            DOTweenType.DOScale,
            useFrom: true,
            from: new Vector4(1f, 1f, 1f, 0f),
            to: new Vector4(0.9f, 0.9f, 0.9f, 0f),
            duration: 0.08f,
            eEase: Ease.OutQuad);
        var release = MakeStep(
            AddType.Append,
            DOTweenType.DOScale,
            useFrom: false,
            from: Vector4.zero,
            to: new Vector4(1f, 1f, 1f, 0f),
            duration: 0.12f,
            eEase: Ease.OutBack);
        preset.EditorSetData(new[] { press, release }, 0f, Ease.OutQuad, 1, LoopType.Restart, UpdateType.Normal, true);
        return preset;
    }

    /// <summary>
    /// 进度填充
    /// </summary>
    private static DOTweenSequencePreset BuildProgressFill()
    {
        var preset = ScriptableObject.CreateInstance<DOTweenSequencePreset>();
        var fill = MakeStep(
            AddType.Append,
            DOTweenType.DOFillAmount,
            useFrom: true,
            from: new Vector4(0f, 0f, 0f, 0f),
            to: new Vector4(1f, 0f, 0f, 0f),
            duration: 0.8f,
            eEase: Ease.OutQuad);
        preset.EditorSetData(new[] { fill }, 0f, Ease.OutQuad, 1, LoopType.Restart, UpdateType.Normal, true);
        return preset;
    }

    /// <summary>
    /// 构造单步 无 Target
    /// </summary>
    private static SequenceAnimation MakeStep(
        AddType eAddType,
        DOTweenType eType,
        bool useFrom,
        Vector4 from,
        Vector4 to,
        float duration,
        Ease eEase)
    {
        return new SequenceAnimation
        {
            AddType = eAddType,
            AnimationType = eType,
            Target = null,
            ToValue = to,
            UseToTarget = false,
            ToTarget = null,
            UseFromValue = useFrom,
            FromValue = from,
            SpeedBased = false,
            DurationOrSpeed = duration,
            Delay = 0f,
            UpdateType = UpdateType.Normal,
            CustomEase = false,
            Ease = eEase,
            Loops = 1,
            LoopType = LoopType.Restart,
            Snapping = false
        };
    }

    /// <summary>
    /// 创建或覆盖同名预设
    /// </summary>
    private static void CreateOrUpdate(string assetName, DOTweenSequencePreset source)
    {
        string path = $"{OutputFolder}/{assetName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<DOTweenSequencePreset>(path);
        if (existing != null)
        {
            existing.EditorSetData(
                source.CloneSequence(),
                source.Delay,
                source.Ease,
                source.Loops,
                source.LoopType,
                source.UpdateType,
                source.IgnoreTimeScale);
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(source);
            return;
        }

        AssetDatabase.CreateAsset(source, path);
    }

    /// <summary>
    /// 确保目录存在
    /// </summary>
    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        string[] parts = folderPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
#endif
