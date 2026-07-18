#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ItemWheel 预制体生成窗口
/// </summary>
public class ItemWheelEditorWindow : EditorWindow
{
    private enum IconRotationMode
    {
        正放,
        顺向
    }
    private const string DefaultSaveFolder = "Assets/Scripts/Item Wheel/Prefab";
    private const string WheelPrefabName = "Item Wheel.prefab";
    private const float PreviewHeight = 300f;
    private const string PrefsPrefix = "MmItemWheel_";

    [MenuItem("Tools/Mm ItemWheel")]
    private static void Open()
    {
        var window = GetWindow<ItemWheelEditorWindow>("Mm ItemWheel");
        window.minSize = new Vector2(360f, 620f);
    }

    private string saveFolder = DefaultSaveFolder;

    private Vector2 centerPosition = Vector2.zero;
    private float circleInner = 350f;
    private float circleOuter = 500f;
    private int circleSegments = 32;
    private int innerArcSegments = 64;

    private int sectorCount = 8;
    private float arcOffset = 3f;
    private float gapRadiusOffset = 0f;

    private Color sectorColor = new Color(0.55f, 0.55f, 0.55f, 0.5f);
    private Color centerColor = new Color(0.08f, 0.08f, 0.1f, 1f);
    private Color borderColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    private float borderWidth = 10f;

    private float iconSize = 80f;
    private IconRotationMode iconRotation = IconRotationMode.顺向;
    private Sprite previewIconSprite;

    private Vector2 scrollPos;
    private GUIStyle legendStyle;

    private static readonly Color InnerCircleColor = new Color(0.25f, 0.85f, 1f, 1f);
    private static readonly Color OuterRadiusColor = new Color(1f, 0.55f, 0.15f, 1f);
    private static readonly Color GapColor = new Color(0.12f, 0.12f, 0.14f, 1f);

    private void OnEnable() => LoadSettings();

    private void OnDisable() => SaveSettings();

    private void OnGUI()
    {
        if (legendStyle == null)
        {
            legendStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 10,
                wordWrap = true,
                padding = new RectOffset(8, 8, 6, 6),
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
            };
        }

        EditorGUI.BeginChangeCheck();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - PreviewHeight - 12f));

        EditorGUILayout.LabelField("保存路径", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        saveFolder = EditorGUILayout.TextField("文件夹", saveFolder);
        if (GUILayout.Button("选择", GUILayout.Width(48f)))
        {
            string picked = EditorUtility.OpenFolderPanel("选择预制体保存文件夹", "Assets", "");
            if (!string.IsNullOrEmpty(picked))
            {
                string dataPath = Application.dataPath.Replace('\\', '/');
                picked = picked.Replace('\\', '/');
                if (picked.StartsWith(dataPath))
                    saveFolder = "Assets" + picked.Substring(dataPath.Length);
                else
                    EditorUtility.DisplayDialog("路径无效", "请选择项目 Assets 目录内的文件夹", "确定");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("同心圆", EditorStyles.boldLabel);
        centerPosition = EditorGUILayout.Vector2Field("中心位置", centerPosition);
        circleInner = EditorGUILayout.Slider("内圆半径", circleInner, 1f, 1005f);
        circleOuter = EditorGUILayout.Slider("外圆半径", circleOuter, 1f, 1000f);
        circleSegments = EditorGUILayout.IntSlider("外弧段数", circleSegments, 2, 64);
        innerArcSegments = EditorGUILayout.IntSlider("内弧段数", innerArcSegments, 2, 128);
        gapRadiusOffset = EditorGUILayout.FloatField("扇环内径偏移", gapRadiusOffset);
        centerColor = EditorGUILayout.ColorField("内圆颜色", centerColor);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("扇环", EditorStyles.boldLabel);
        sectorCount = EditorGUILayout.IntSlider("扇环个数", sectorCount, 1, 32);
        arcOffset = EditorGUILayout.FloatField("扇环间距", arcOffset);
        sectorColor = EditorGUILayout.ColorField("扇环颜色", sectorColor);
        borderColor = EditorGUILayout.ColorField("描边颜色", borderColor);
        borderWidth = EditorGUILayout.FloatField("描边宽度", borderWidth);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("图标", EditorStyles.boldLabel);
        iconSize = EditorGUILayout.FloatField("图标尺寸", iconSize);
        iconRotation = (IconRotationMode)EditorGUILayout.EnumPopup("图标朝向", iconRotation);
        previewIconSprite = (Sprite)EditorGUILayout.ObjectField("预览图标", previewIconSprite, typeof(Sprite), false);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("尺寸与网格", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Rect  {circleOuter * 2f:0} × {circleOuter * 2f:0}  ← 外圆半径 × 2", EditorStyles.helpBox);
        EditorGUILayout.LabelField($"预估顶点  {EstimateTotalVertices():0}  ← 扇环数 × 弧段数", EditorStyles.miniLabel);

        EditorGUILayout.Space(8f);
        if (GUILayout.Button("生成 Item Wheel 预制体", GUILayout.Height(36f)))
            GenerateItemWheelPrefab();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.LabelField("实时预览", EditorStyles.boldLabel);
        Rect previewRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(PreviewHeight));
        DrawWheelPreview(previewRect);

        if (EditorGUI.EndChangeCheck())
        {
            SaveSettings();
            Repaint();
        }
    }

    /// <summary>
    /// 从 EditorPrefs 读取窗口参数
    /// </summary>
    private void LoadSettings()
    {
        saveFolder = EditorPrefs.GetString(PrefsPrefix + "SaveFolder", DefaultSaveFolder);
        centerPosition.x = EditorPrefs.GetFloat(PrefsPrefix + "CenterX", 0f);
        centerPosition.y = EditorPrefs.GetFloat(PrefsPrefix + "CenterY", 0f);
        circleInner = EditorPrefs.GetFloat(PrefsPrefix + "CircleInner", 350f);
        circleOuter = EditorPrefs.GetFloat(PrefsPrefix + "CircleOuter", 500f);
        circleSegments = EditorPrefs.GetInt(PrefsPrefix + "CircleSegments", 32);
        innerArcSegments = EditorPrefs.GetInt(PrefsPrefix + "InnerArcSegments", 64);
        sectorCount = EditorPrefs.GetInt(PrefsPrefix + "SectorCount", 8);
        arcOffset = EditorPrefs.GetFloat(PrefsPrefix + "ArcOffset", 3f);
        gapRadiusOffset = EditorPrefs.GetFloat(PrefsPrefix + "GapRadiusOffset", 0f);
        borderWidth = EditorPrefs.GetFloat(PrefsPrefix + "BorderWidth", 10f);
        iconSize = EditorPrefs.GetFloat(PrefsPrefix + "IconSize", 80f);
        iconRotation = (IconRotationMode)EditorPrefs.GetInt(PrefsPrefix + "IconRotation", (int)IconRotationMode.顺向);
        previewIconSprite = LoadSpriteFromGuid(EditorPrefs.GetString(PrefsPrefix + "PreviewIconGUID", ""));
        sectorColor = LoadColor(PrefsPrefix + "SectorColor", sectorColor);
        centerColor = LoadColor(PrefsPrefix + "CenterColor", centerColor);
        borderColor = LoadColor(PrefsPrefix + "BorderColor", borderColor);
    }

    /// <summary>
    /// 写入 EditorPrefs 保存窗口参数
    /// </summary>
    private void SaveSettings()
    {
        EditorPrefs.SetString(PrefsPrefix + "SaveFolder", saveFolder);
        EditorPrefs.SetFloat(PrefsPrefix + "CenterX", centerPosition.x);
        EditorPrefs.SetFloat(PrefsPrefix + "CenterY", centerPosition.y);
        EditorPrefs.SetFloat(PrefsPrefix + "CircleInner", circleInner);
        EditorPrefs.SetFloat(PrefsPrefix + "CircleOuter", circleOuter);
        EditorPrefs.SetInt(PrefsPrefix + "CircleSegments", circleSegments);
        EditorPrefs.SetInt(PrefsPrefix + "InnerArcSegments", innerArcSegments);
        EditorPrefs.SetInt(PrefsPrefix + "SectorCount", sectorCount);
        EditorPrefs.SetFloat(PrefsPrefix + "ArcOffset", arcOffset);
        EditorPrefs.SetFloat(PrefsPrefix + "GapRadiusOffset", gapRadiusOffset);
        EditorPrefs.SetFloat(PrefsPrefix + "BorderWidth", borderWidth);
        EditorPrefs.SetFloat(PrefsPrefix + "IconSize", iconSize);
        EditorPrefs.SetInt(PrefsPrefix + "IconRotation", (int)iconRotation);
        EditorPrefs.SetString(PrefsPrefix + "PreviewIconGUID",
            previewIconSprite != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(previewIconSprite)) : "");
        SaveColor(PrefsPrefix + "SectorColor", sectorColor);
        SaveColor(PrefsPrefix + "CenterColor", centerColor);
        SaveColor(PrefsPrefix + "BorderColor", borderColor);
    }

    private static Color LoadColor(string key, Color fallback)
    {
        if (!EditorPrefs.HasKey(key + "_R")) return fallback;
        return new Color(
            EditorPrefs.GetFloat(key + "_R"),
            EditorPrefs.GetFloat(key + "_G"),
            EditorPrefs.GetFloat(key + "_B"),
            EditorPrefs.GetFloat(key + "_A"));
    }

    private static Sprite LoadSpriteFromGuid(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;
        string path = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void SaveColor(string key, Color color)
    {
        EditorPrefs.SetFloat(key + "_R", color.r);
        EditorPrefs.SetFloat(key + "_G", color.g);
        EditorPrefs.SetFloat(key + "_B", color.b);
        EditorPrefs.SetFloat(key + "_A", color.a);
    }

    /// <summary>
    /// 估算整盘网格顶点数
    /// </summary>
    private int EstimateTotalVertices()
    {
        int strip = Mathf.Max(circleSegments, innerArcSegments);
        int perSector = (strip + 1) * 2;
        if (borderWidth > 0.001f)
            perSector += (strip + 1) * 2;
        return perSector * sectorCount + innerArcSegments + 1;
    }

    private void DrawWheelPreview(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
        if (rect.width < 40f || rect.height < 40f)
            return;

        float sectorInner = circleInner + gapRadiusOffset;
        bool paramValid = circleOuter > sectorInner && sectorCount >= 1;

        List<float> startList = new();
        List<float> sweepList = new();
        CalculateArcLayout(Mathf.Max(1, sectorCount), arcOffset, startList, sweepList);

        const float pad = 8f;
        const float legendW = 120f;

        float wheelZoneW = rect.width - legendW - pad * 3f;
        float wheelZoneH = rect.height - pad * 2f;
        float drawSize = Mathf.Min(wheelZoneW, wheelZoneH);
        Vector2 center = new Vector2(rect.x + pad + wheelZoneW * 0.5f, rect.y + pad + wheelZoneH * 0.5f);
        float scale = drawSize * 0.5f / Mathf.Max(1f, circleOuter);

        if (Event.current.type == EventType.Repaint && paramValid)
        {
            Handles.BeginGUI();

            float lineInner = circleOuter - Mathf.Max(0f, borderWidth);
            bool hasBorder = borderWidth > 0.001f && lineInner > sectorInner + 0.001f;
            float fillOuter = hasBorder ? lineInner : circleOuter;

            for (int i = 0; i < sectorCount; i++)
            {
                DrawSectorRing(center, scale, sectorInner, fillOuter, startList[i], sweepList[i],
                    sectorColor, circleSegments, innerArcSegments);
            }

            if (arcOffset > 0.01f)
            {
                float singleAngle = 360f / sectorCount;
                for (int i = 0; i < sectorCount; i++)
                {
                    float gapCenter = i * singleAngle;
                    DrawSectorRing(center, scale, sectorInner, circleOuter, gapCenter - arcOffset * 0.5f, arcOffset,
                        GapColor, circleSegments, innerArcSegments);
                }
            }

            if (hasBorder)
            {
                for (int i = 0; i < sectorCount; i++)
                {
                    DrawSectorRing(center, scale, lineInner, circleOuter, startList[i], sweepList[i],
                        borderColor, circleSegments, circleSegments);
                }
            }

            DrawFilledCircle(center, scale, circleInner, centerColor, innerArcSegments);
            DrawCircleRing(center, scale, circleInner, InnerCircleColor, innerArcSegments);

            DrawRadiusTick(center, scale, circleInner, 200f, InnerCircleColor);
            DrawRadiusTick(center, scale, circleOuter, 340f, OuterRadiusColor);

            Handles.EndGUI();

            DrawSectorIconPreviews(center, scale, sectorInner, fillOuter, startList, sweepList);
        }

        float sweep = sectorCount > 0 ? Mathf.Max(0.001f, 360f / sectorCount - arcOffset) : 0f;
        Rect legendRect = new Rect(rect.xMax - legendW - pad, rect.y + pad, legendW, wheelZoneH);
        EditorGUI.DrawRect(legendRect, new Color(0.08f, 0.08f, 0.1f, 1f));

        string legendText = paramValid
            ? "图例\n\n" +
              "青圈  内圆\n" +
              "橙线  外圆半径\n" +
              "色块  扇环颜色\n" +
              "外缘  圆弧描边\n" +
              "白框  图标占位\n\n" +
              $"图标 {iconSize:0}  {(iconRotation == IconRotationMode.顺向 ? "顺向" : "正放")}\n" +
              $"外弧段 {circleSegments}\n" +
              $"内弧段 {innerArcSegments}\n" +
              $"扇环 {sectorCount}  间距 {arcOffset:0.#}°\n" +
              $"描边 {borderWidth:0}  单扇 {sweep:0.#}°"
            : "参数无效";
        GUI.Label(legendRect, legendText, legendStyle);
    }

    /// <summary>
    /// 预览各扇环 Icon 位置与朝向
    /// </summary>
    private void DrawSectorIconPreviews(Vector2 center, float scale, float inner, float outer,
        List<float> startList, List<float> sweepList)
    {
        float drawSize = iconSize * scale;
        if (drawSize < 2f) return;

        for (int i = 0; i < sectorCount; i++)
        {
            float startDeg = startList[i];
            float sweepDeg = sweepList[i];
            float midDeg = startDeg + sweepDeg * 0.5f;
            float rad = midDeg * Mathf.Deg2Rad;
            float r = (inner + outer) * 0.5f;
            Vector2 local = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * r;
            Vector2 guiCenter = new Vector2(center.x + local.x * scale, center.y - local.y * scale);

            float rotZ = iconRotation == IconRotationMode.顺向 ? midDeg - 90f : 0f;
            Rect iconRect = new Rect(guiCenter.x - drawSize * 0.5f, guiCenter.y - drawSize * 0.5f, drawSize, drawSize);

            Matrix4x4 backup = GUI.matrix;
            GUIUtility.RotateAroundPivot(rotZ, guiCenter);

            if (previewIconSprite != null && previewIconSprite.texture != null)
                DrawSprite(iconRect, previewIconSprite);
            else
            {
                EditorGUI.DrawRect(iconRect, new Color(1f, 1f, 1f, 0.22f));
                DrawRectOutline(iconRect, new Color(1f, 1f, 1f, 0.75f));
            }

            GUI.matrix = backup;
        }
    }

    /// <summary>
    /// 绘制 Sprite 到预览区
    /// </summary>
    private static void DrawSprite(Rect dest, Sprite sprite)
    {
        Texture2D tex = sprite.texture;
        Rect r = sprite.rect;
        Rect uv = new Rect(r.x / tex.width, r.y / tex.height, r.width / tex.width, r.height / tex.height);
        GUI.DrawTextureWithTexCoords(dest, tex, uv, true);
    }

    /// <summary>
    /// 绘制矩形描边
    /// </summary>
    private static void DrawRectOutline(Rect rect, Color color)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), color);
    }

    /// <summary>
    /// 预览扇环 与 SectorBuilder 相同采样 按条带绘制规避 DrawAAConvexPolygon 64 顶点上限
    /// </summary>
    private static void DrawSectorRing(Vector2 center, float scale, float inner, float outer,
        float startDeg, float sweepDeg, Color fill, int outerParts, int innerParts)
    {
        int stripCount = Mathf.Max(Mathf.Max(1, outerParts), Mathf.Max(1, innerParts));
        Handles.color = fill;

        for (int i = 0; i < stripCount; i++)
        {
            float u0 = i / (float)stripCount;
            float u1 = (i + 1) / (float)stripCount;

            Vector3 o0 = GuiPoint(center, scale, SectorBuilder.SampleArc(startDeg, sweepDeg, outer, u0, outerParts));
            Vector3 o1 = GuiPoint(center, scale, SectorBuilder.SampleArc(startDeg, sweepDeg, outer, u1, outerParts));
            Vector3 i1 = GuiPoint(center, scale, SectorBuilder.SampleArc(startDeg, sweepDeg, inner, u1, innerParts));
            Vector3 i0 = GuiPoint(center, scale, SectorBuilder.SampleArc(startDeg, sweepDeg, inner, u0, innerParts));

            Handles.DrawAAConvexPolygon(o0, o1, i1, i0);
        }
    }

    private static Vector3 GuiPoint(Vector2 center, float scale, Vector2 local)
    {
        return new Vector3(center.x + local.x * scale, center.y - local.y * scale, 0f);
    }

    private static void DrawRadiusTick(Vector2 center, float scale, float radius, float angleDeg, Color color)
    {
        Vector2 edge = DegToGuiPoint(center, scale, radius, angleDeg);
        Vector2 dir = (edge - center).normalized;
        Vector2 tickEnd = edge + dir * 14f;

        Handles.color = color;
        Handles.DrawLine(new Vector3(center.x, center.y, 0f), new Vector3(edge.x, edge.y, 0f));
        Handles.DrawLine(new Vector3(edge.x, edge.y, 0f), new Vector3(tickEnd.x, tickEnd.y, 0f));
        Handles.DrawSolidDisc(new Vector3(edge.x, edge.y, 0f), Vector3.forward, 3f);
    }

    private static Vector2 DegToGuiPoint(Vector2 center, float scale, float radius, float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        return center + new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad)) * radius * scale;
    }

    private static void DrawFilledCircle(Vector2 center, float scale, float radius, Color color, int segments)
    {
        int seg = Mathf.Max(8, segments);
        Vector3 c = new Vector3(center.x, center.y, 0f);
        Handles.color = color;

        for (int i = 0; i < seg; i++)
        {
            Vector2 p0 = DegToGuiPoint(center, scale, radius, i * 360f / seg);
            Vector2 p1 = DegToGuiPoint(center, scale, radius, (i + 1) * 360f / seg);
            Handles.DrawAAConvexPolygon(c, new Vector3(p0.x, p0.y, 0f), new Vector3(p1.x, p1.y, 0f));
        }
    }

    private static void DrawCircleRing(Vector2 center, float scale, float radius, Color color, int segments)
    {
        int seg = Mathf.Max(8, segments);
        Handles.color = color;
        Vector3 prev = Vector3.zero;
        for (int i = 0; i <= seg; i++)
        {
            Vector2 p = DegToGuiPoint(center, scale, radius, i * 360f / seg);
            Vector3 cur = new Vector3(p.x, p.y, 0f);
            if (i > 0)
                Handles.DrawLine(prev, cur);
            prev = cur;
        }
    }

    private void GenerateItemWheelPrefab()
    {
        if (sectorCount < 1)
        {
            EditorUtility.DisplayDialog("参数错误", "扇环个数至少为 1", "确定");
            return;
        }

        if (circleOuter <= circleInner + gapRadiusOffset)
        {
            EditorUtility.DisplayDialog("参数错误", "外圆半径需大于内圆半径加内径偏移", "确定");
            return;
        }

        if (!EnsureSaveFolder(saveFolder))
            return;

        List<float> startAngleList = new();
        List<float> sweepAngleList = new();
        CalculateArcLayout(sectorCount, arcOffset, startAngleList, sweepAngleList);

        float sectorInner = circleInner + gapRadiusOffset;
        string prefabPath = $"{saveFolder}/{WheelPrefabName}".Replace('\\', '/');

        ClearExistingWheelPrefabs(saveFolder);

        GameObject root = CreateWheelRoot();
        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < sectorCount; i++)
            {
                GameObject sector = CreateSectorObject(i, startAngleList[i], sweepAngleList[i], sectorInner, circleOuter);
                sector.transform.SetParent(root.transform, false);
            }

            CreateCenterCircleObject(root.transform);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            DestroyImmediate(root);
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("完成", $"已生成 {WheelPrefabName}\n路径: {prefabPath}", "确定");
    }

    private static void ClearExistingWheelPrefabs(string folder)
    {
        folder = folder.Replace('\\', '/').TrimEnd('/');

        string wheelPath = $"{folder}/{WheelPrefabName}";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(wheelPath) != null)
            AssetDatabase.DeleteAsset(wheelPath);

        for (int i = 0; i < 64; i++)
        {
            string sectorPath = $"{folder}/Sector_{i}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(sectorPath) != null)
                AssetDatabase.DeleteAsset(sectorPath);
        }
    }

    private GameObject CreateWheelRoot()
    {
        GameObject root = new GameObject("Item Wheel", typeof(RectTransform));
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = centerPosition;
        rt.sizeDelta = new Vector2(circleOuter * 2f, circleOuter * 2f);
        rt.localScale = Vector3.one;

        ItemWheelController controller = root.AddComponent<ItemWheelController>();
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("wheelRoot").objectReferenceValue = rt;
        so.ApplyModifiedPropertiesWithoutUndo();

        return root;
    }

    private GameObject CreateSectorObject(int index, float startDeg, float sweepDeg, float inner, float outer)
    {
        GameObject go = new GameObject($"Sector_{index}", typeof(RectTransform));
        go.AddComponent<CanvasRenderer>();

        RingDraw ring = go.AddComponent<RingDraw>();
        ring.raycastTarget = true;
        ring.InitSector(startDeg, sweepDeg, inner, outer, circleSegments, innerArcSegments, borderWidth, borderColor);
        ring.color = sectorColor;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(outer * 2f, outer * 2f);
        rt.localScale = Vector3.one;

        CreateSectorIcon(go.transform, startDeg, sweepDeg, inner, outer);
        go.AddComponent<RingController>();
        return go;
    }

    /// <summary>
    /// 创建扇环下图标 Image
    /// </summary>
    private void CreateSectorIcon(Transform sector, float startDeg, float sweepDeg, float inner, float outer)
    {
        GameObject iconGo = new GameObject("Icon", typeof(RectTransform));
        iconGo.transform.SetParent(sector, false);
        iconGo.AddComponent<CanvasRenderer>();

        Image image = iconGo.AddComponent<Image>();
        image.raycastTarget = false;
        image.color = Color.white;

        float midDeg = startDeg + sweepDeg * 0.5f;
        float rad = midDeg * Mathf.Deg2Rad;
        float r = (inner + outer) * 0.5f;

        RectTransform rt = iconGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(iconSize, iconSize);
        rt.anchoredPosition = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * r;
        rt.localRotation = iconRotation == IconRotationMode.顺向
            ? Quaternion.Euler(0f, 0f, midDeg - 90f)
            : Quaternion.identity;
    }

    private void CreateCenterCircleObject(Transform parent)
    {
        GameObject go = new GameObject("CenterCircle", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.AddComponent<CanvasRenderer>();

        CenterCircleDraw cc = go.AddComponent<CenterCircleDraw>();
        cc.raycastTarget = true;
        cc.InitCenter(circleInner, innerArcSegments);
        cc.color = centerColor;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(circleInner * 2f, circleInner * 2f);
        rt.localScale = Vector3.one;
    }

    private static void CalculateArcLayout(int count, float gap, List<float> startList, List<float> sweepList)
    {
        startList.Clear();
        sweepList.Clear();

        float singleAngle = 360f / count;
        float sweep = Mathf.Max(0.001f, singleAngle - gap);

        for (int i = 0; i < count; i++)
        {
            startList.Add(i * singleAngle + gap * 0.5f);
            sweepList.Add(sweep);
        }
    }

    private static bool EnsureSaveFolder(string folder)
    {
        folder = folder.Replace('\\', '/').TrimEnd('/');
        if (string.IsNullOrEmpty(folder) || !folder.StartsWith("Assets"))
        {
            EditorUtility.DisplayDialog("路径无效", "保存路径必须在 Assets 目录下", "确定");
            return false;
        }

        if (AssetDatabase.IsValidFolder(folder))
            return true;

        string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
        string folderName = Path.GetFileName(folder);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
        {
            EditorUtility.DisplayDialog("路径无效", "无法解析文件夹路径", "确定");
            return false;
        }

        if (!EnsureSaveFolder(parent))
            return false;

        AssetDatabase.CreateFolder(parent, folderName);
        return AssetDatabase.IsValidFolder(folder);
    }
}
#endif
