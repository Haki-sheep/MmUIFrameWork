#if UNITY_EDITOR
using MieMieUITools.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 一键生成 StandUIPrefabs 暗色控件 圆角/直角双版本
/// </summary>
public static class StandUIPrefabFactory
{
    private static string RootFolder => PackagePaths.StandUIPrefabsRoot;
    private static string PresetFolder => PackagePaths.DoTweenPresetsRoot;

    private static readonly Color ColorPanel = new Color(0.12f, 0.12f, 0.15f, 0.96f);
    private static readonly Color ColorButton = new Color(0.22f, 0.22f, 0.28f, 1f);
    private static readonly Color ColorButtonHighlight = new Color(0.30f, 0.30f, 0.38f, 1f);
    private static readonly Color ColorAccent = new Color(0.31f, 0.79f, 0.69f, 1f);
    private static readonly Color ColorText = new Color(0.91f, 0.91f, 0.93f, 1f);
    private static readonly Color ColorTextDim = new Color(0.60f, 0.60f, 0.66f, 1f);
    private static readonly Color ColorTrack = new Color(0.08f, 0.08f, 0.10f, 1f);
    private static readonly Color ColorHandleCore = new Color(0.95f, 0.96f, 0.98f, 1f);

    [MenuItem("Tools/MieMieFrameWork/TestAndCreat/生成标准控件预制体")]
    public static void GenerateAll()
    {
        EnsureAllFolders();
        // 确保动画预设存在
        DOTweenPresetFactory.GenerateClassicPresetsSilent();

        var sprites = StandUISpriteUtil.RebuildAllSprites();
        if (sprites.RoundPanel == null || sprites.SharpPanel == null || sprites.HandleKnob == null)
        {
            EditorUtility.DisplayDialog("失败", "Sprite 生成失败 请查看 Console", "确定");
            return;
        }

        // 圆角版
        GenerateVariant(true, sprites);
        // 直角版
        GenerateVariant(false, sprites);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[StandUIPrefabFactory] StandUIPrefabs 圆角/直角双版本已生成");
        EditorUtility.DisplayDialog("完成", "已生成圆角 + 直角 Stand 控件\n并尝试绑定动画预设\n" + RootFolder, "确定");
    }

    /// <summary>
    /// 生成某一风格全套
    /// </summary>
    private static void GenerateVariant(bool rounded, StandUISpriteUtil.SpriteSet sprites)
    {
        string suffix = rounded ? "" : "_Sharp";
        Sprite panel = rounded ? sprites.RoundPanel : sprites.SharpPanel;
        Sprite buttonBg = rounded ? sprites.Capsule : sprites.SharpPanel;
        Image.Type imageType = Image.Type.Sliced;

        string buttonPath = $"{RootFolder}/Button/StandButton{suffix}.prefab";
        string sliderPath = $"{RootFolder}/Slider/StandSlider{suffix}.prefab";
        string togglePath = $"{RootFolder}/Toggle/StandToggle{suffix}.prefab";
        string imagePath = $"{RootFolder}/Image/StandImage{suffix}.prefab";
        string tmpPath = $"{RootFolder}/Tmp/StandTmpText{suffix}.prefab";
        string panelPath = $"{RootFolder}/Panel/StandPanel{suffix}.prefab";

        SavePrefab(BuildButton(buttonBg, imageType, $"StandButton{suffix}"), buttonPath);
        SavePrefab(BuildSlider(panel, sprites.HandleKnob, imageType, $"StandSlider{suffix}"), sliderPath);
        SavePrefab(BuildToggle(panel, sprites.Circle, imageType, $"StandToggle{suffix}"), togglePath);
        SavePrefab(BuildImage(panel, imageType, $"StandImage{suffix}"), imagePath);
        SavePrefab(BuildTmpLabel($"StandTmpText{suffix}"), tmpPath);
        SavePrefab(BuildPanel(panel, imageType, $"StandPanel{suffix}"), panelPath);

        BindPreset(buttonPath, "UI_ButtonPunch_Scale.asset", true);
        BindPreset(panelPath, "UI_PopupShow_ScaleFade.asset", false);
        BindPreset(imagePath, "UI_FadeIn.asset", false);
        BindPreset(sliderPath, "UI_Progress_Fill.asset", false);
    }

    private static void EnsureAllFolders()
    {
        StandUISpriteUtil.EnsureFolder(RootFolder);
        string[] folders = { "Button", "Slider", "Toggle", "Image", "Tmp", "Panel", "_Shared" };
        for (int i = 0; i < folders.Length; i++)
        {
            StandUISpriteUtil.EnsureFolder(RootFolder + "/" + folders[i]);
        }
    }

    private static GameObject BuildButton(Sprite bgSprite, Image.Type imageType, string name)
    {
        var root = CreateUiObject(name, null, new Vector2(180f, 48f));
        var image = root.AddComponent<Image>();
        image.sprite = bgSprite;
        image.type = imageType;
        image.color = ColorButton;
        image.raycastTarget = true;

        var button = root.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = ColorButton;
        colors.highlightedColor = ColorButtonHighlight;
        colors.pressedColor = ColorAccent;
        colors.selectedColor = ColorButtonHighlight;
        colors.disabledColor = new Color(0.2f, 0.2f, 0.22f, 0.5f);
        button.colors = colors;
        button.targetGraphic = image;

        var labelGo = CreateUiObject("Label", root.transform, Vector2.zero);
        StretchFull(labelGo.GetComponent<RectTransform>());
        ApplyTmp(labelGo.AddComponent<TextMeshProUGUI>(), "Button", 24f, TextAlignmentOptions.Center, ColorText);

        var seq = root.AddComponent<DOTweenSequence>();
        seq.DefaultTarget = root.transform;
        return root;
    }

    private static GameObject BuildSlider(Sprite panelSprite, Sprite handleSprite, Image.Type imageType, string name)
    {
        var root = CreateUiObject(name, null, new Vector2(240f, 32f));
        var slider = root.AddComponent<Slider>();

        var bg = CreateUiObject("Background", root.transform, Vector2.zero);
        StretchFull(bg.GetComponent<RectTransform>(), new Vector2(0f, 10f), new Vector2(0f, -10f));
        var bgImage = bg.AddComponent<Image>();
        bgImage.sprite = panelSprite;
        bgImage.type = imageType;
        bgImage.color = ColorTrack;

        var fillArea = CreateUiObject("Fill Area", root.transform, Vector2.zero);
        StretchFull(fillArea.GetComponent<RectTransform>(), new Vector2(10f, 10f), new Vector2(-10f, -10f));

        var fill = CreateUiObject("Fill", fillArea.transform, Vector2.zero);
        StretchFull(fill.GetComponent<RectTransform>());
        var fillImage = fill.AddComponent<Image>();
        fillImage.sprite = panelSprite;
        fillImage.type = imageType;
        fillImage.color = ColorAccent;

        var handleArea = CreateUiObject("Handle Slide Area", root.transform, Vector2.zero);
        StretchFull(handleArea.GetComponent<RectTransform>(), new Vector2(10f, 0f), new Vector2(-10f, 0f));

        // 必须正方形 否则圆手柄会被拉成椭圆
        var handle = CreateUiObject("Handle", handleArea.transform, new Vector2(22f, 22f));
        var handleImage = handle.AddComponent<Image>();
        handleImage.sprite = handleSprite;
        handleImage.type = Image.Type.Simple;
        handleImage.preserveAspect = true;
        handleImage.color = ColorHandleCore;

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;

        var seq = root.AddComponent<DOTweenSequence>();
        seq.DefaultTarget = fillImage;
        return root;
    }

    private static GameObject BuildToggle(Sprite panelSprite, Sprite circleSprite, Image.Type imageType, string name)
    {
        var root = CreateUiObject(name, null, new Vector2(200f, 36f));
        var toggle = root.AddComponent<Toggle>();

        var bg = CreateUiObject("Background", root.transform, new Vector2(36f, 36f));
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.5f);
        bgRt.anchorMax = new Vector2(0f, 0.5f);
        bgRt.pivot = new Vector2(0f, 0.5f);
        bgRt.anchoredPosition = Vector2.zero;
        var bgImage = bg.AddComponent<Image>();
        bgImage.sprite = panelSprite;
        bgImage.type = imageType;
        bgImage.color = ColorButton;

        var check = CreateUiObject("Checkmark", bg.transform, new Vector2(20f, 20f));
        var checkImage = check.AddComponent<Image>();
        checkImage.sprite = circleSprite;
        checkImage.preserveAspect = true;
        checkImage.color = ColorAccent;

        var labelGo = CreateUiObject("Label", root.transform, Vector2.zero);
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = new Vector2(44f, 0f);
        labelRt.offsetMax = Vector2.zero;
        ApplyTmp(labelGo.AddComponent<TextMeshProUGUI>(), "Toggle", 22f, TextAlignmentOptions.MidlineLeft, ColorText);

        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        toggle.isOn = true;
        return root;
    }

    private static GameObject BuildImage(Sprite panelSprite, Image.Type imageType, string name)
    {
        var root = CreateUiObject(name, null, new Vector2(128f, 128f));
        var image = root.AddComponent<Image>();
        image.sprite = panelSprite;
        image.type = imageType;
        image.color = ColorButton;

        var seq = root.AddComponent<DOTweenSequence>();
        seq.DefaultTarget = image;
        return root;
    }

    private static GameObject BuildTmpLabel(string name)
    {
        var root = CreateUiObject(name, null, new Vector2(240f, 40f));
        ApplyTmp(root.AddComponent<TextMeshProUGUI>(), "Stand Text", 28f, TextAlignmentOptions.Center, ColorText);
        return root;
    }

    /// <summary>
    /// 面板 含 UIContent 标题区 绑弹窗显示预设
    /// </summary>
    private static GameObject BuildPanel(Sprite panelSprite, Image.Type imageType, string name)
    {
        var root = CreateUiObject(name, null, new Vector2(640f, 400f));
        var canvasGroup = root.AddComponent<CanvasGroup>();
        var bg = root.AddComponent<Image>();
        bg.sprite = panelSprite;
        bg.type = imageType;
        bg.color = ColorPanel;

        var content = CreateUiObject("UIContent", root.transform, Vector2.zero);
        StretchFull(content.GetComponent<RectTransform>(), new Vector2(24f, 24f), new Vector2(-24f, -24f));

        var title = CreateUiObject("Title", content.transform, new Vector2(0f, 40f));
        var titleRt = title.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = Vector2.zero;
        titleRt.sizeDelta = new Vector2(0f, 48f);
        ApplyTmp(title.AddComponent<TextMeshProUGUI>(), "Panel Title", 30f, TextAlignmentOptions.Center, ColorText);

        var body = CreateUiObject("Body", content.transform, Vector2.zero);
        var bodyRt = body.GetComponent<RectTransform>();
        bodyRt.anchorMin = Vector2.zero;
        bodyRt.anchorMax = Vector2.one;
        bodyRt.offsetMin = new Vector2(0f, 0f);
        bodyRt.offsetMax = new Vector2(0f, -56f);
        ApplyTmp(body.AddComponent<TextMeshProUGUI>(), "Content", 22f, TextAlignmentOptions.TopLeft, ColorTextDim);

        var seq = root.AddComponent<DOTweenSequence>();
        seq.DefaultTarget = canvasGroup;
        return root;
    }

    /// <summary>
    /// 绑定预设到预制体
    /// </summary>
    private static void BindPreset(string prefabPath, string presetFileName, bool useTransformTarget)
    {
        var preset = AssetDatabase.LoadAssetAtPath<DOTweenSequencePreset>($"{PresetFolder}/{presetFileName}");
        if (preset == null)
        {
            Debug.LogWarning($"[StandUI] 未找到预设 {presetFileName}");
            return;
        }

        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        var seq = root.GetComponent<DOTweenSequence>();
        if (seq != null)
        {
            seq.EditorSetPreset(preset);
            if (useTransformTarget)
            {
                seq.DefaultTarget = root.transform;
            }
            else if (seq.DefaultTarget == null)
            {
                seq.DefaultTarget = root.GetComponent<CanvasGroup>();
                if (seq.DefaultTarget == null) seq.DefaultTarget = root.GetComponent<Graphic>();
                if (seq.DefaultTarget == null) seq.DefaultTarget = root.transform;
            }
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static GameObject CreateUiObject(string name, Transform parent, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        if (parent != null) go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        return go;
    }

    private static void StretchFull(RectTransform rt, Vector2? offsetMin = null, Vector2? offsetMax = null)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = offsetMin ?? Vector2.zero;
        rt.offsetMax = offsetMax ?? Vector2.zero;
    }

    private static void ApplyTmp(TextMeshProUGUI tmp, string text, float fontSize, TextAlignmentOptions align, Color color)
    {
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }
    }

    private static void SavePrefab(GameObject root, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }
}
#endif
