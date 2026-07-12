#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 程序生成 StandUI 共用 Sprite
/// </summary>
internal static class StandUISpriteUtil
{
    /// <summary>
    /// 共享美术目录
    /// </summary>
    public const string SharedArtFolder = "Assets/MmUIFrameWork/StandUIPrefabs/_Shared/Art";

    public const string RoundPanelPath = SharedArtFolder + "/Stand_RoundPanel.png";
    public const string CapsulePath = SharedArtFolder + "/Stand_Capsule.png";
    public const string CirclePath = SharedArtFolder + "/Stand_Circle.png";
    public const string SharpPanelPath = SharedArtFolder + "/Stand_SharpPanel.png";
    public const string HandleKnobPath = SharedArtFolder + "/Stand_HandleKnob.png";

    /// <summary>
    /// Stand 共用图集
    /// </summary>
    public struct SpriteSet
    {
        public Sprite RoundPanel;
        public Sprite Capsule;
        public Sprite Circle;
        public Sprite SharpPanel;
        public Sprite HandleKnob;
    }

    /// <summary>
    /// 强制重建全部共用图
    /// </summary>
    public static SpriteSet RebuildAllSprites()
    {
        EnsureFolder(SharedArtFolder);
        var set = new SpriteSet
        {
            RoundPanel = WriteRoundedSprite(RoundPanelPath, 64, 12, 14),
            Capsule = WriteRoundedSprite(CapsulePath, 64, 28, 24),
            Circle = WriteCircleSprite(CirclePath, 64),
            SharpPanel = WriteSolidSprite(SharpPanelPath, 32, 1),
            HandleKnob = WriteHandleKnobSprite(HandleKnobPath, 64)
        };
        AssetDatabase.Refresh();
        return set;
    }

    /// <summary>
    /// 圆角九宫格
    /// </summary>
    private static Sprite WriteRoundedSprite(string assetPath, int size, int radius, int border)
    {
        var tex = DrawRoundedRect(size, radius);
        WritePng(assetPath, tex);
        Object.DestroyImmediate(tex);
        ConfigureSpriteImporter(assetPath, border);
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    /// <summary>
    /// 直角实心九宫格
    /// </summary>
    private static Sprite WriteSolidSprite(string assetPath, int size, int border)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, Color.white);
            }
        }

        tex.Apply();
        WritePng(assetPath, tex);
        Object.DestroyImmediate(tex);
        ConfigureSpriteImporter(assetPath, border);
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    /// <summary>
    /// 实心圆
    /// </summary>
    private static Sprite WriteCircleSprite(string assetPath, int size)
    {
        var tex = DrawCircle(size);
        WritePng(assetPath, tex);
        Object.DestroyImmediate(tex);
        ConfigureSpriteImporter(assetPath, 0);
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    /// <summary>
    /// 滑条手柄 外环+实心芯
    /// </summary>
    private static Sprite WriteHandleKnobSprite(string assetPath, int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) * 0.5f;
        float outerR = center - 1f;
        float ringInner = outerR * 0.72f;
        float coreR = outerR * 0.55f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = 0f;
                float shade = 1f;

                if (dist <= coreR)
                {
                    alpha = 1f;
                    shade = 1f;
                }
                else if (dist <= ringInner)
                {
                    // 芯与环之间略透
                    alpha = 0.15f;
                    shade = 1f;
                }
                else if (dist <= outerR)
                {
                    alpha = 1f;
                    shade = 0.92f;
                }
                else
                {
                    alpha = Mathf.Clamp01(outerR - dist + 1f);
                    shade = 0.85f;
                }

                tex.SetPixel(x, y, new Color(shade, shade, shade, alpha));
            }
        }

        tex.Apply();
        WritePng(assetPath, tex);
        Object.DestroyImmediate(tex);
        ConfigureSpriteImporter(assetPath, 0);
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static Texture2D DrawRoundedRect(int size, int radius)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = radius;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float alpha = SampleRoundedRectAlpha(x, y, size, r);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        return tex;
    }

    private static Texture2D DrawCircle(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) * 0.5f;
        float radius = center - 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        return tex;
    }

    private static float SampleRoundedRectAlpha(int x, int y, int size, float radius)
    {
        float max = size - 1;
        float px = x;
        float py = y;

        if (px >= radius && px <= max - radius) return 1f;
        if (py >= radius && py <= max - radius) return 1f;

        float cx;
        float cy;
        if (px < radius && py < radius)
        {
            cx = radius;
            cy = radius;
        }
        else if (px > max - radius && py < radius)
        {
            cx = max - radius;
            cy = radius;
        }
        else if (px < radius && py > max - radius)
        {
            cx = radius;
            cy = max - radius;
        }
        else
        {
            cx = max - radius;
            cy = max - radius;
        }

        float dx = px - cx;
        float dy = py - cy;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        return Mathf.Clamp01(radius - dist + 0.5f);
    }

    private static void WritePng(string assetPath, Texture2D tex)
    {
        string abs = ToAbsolutePath(assetPath);
        File.WriteAllBytes(abs, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }

    private static void ConfigureSpriteImporter(string assetPath, int border)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.spriteBorder = border > 0
            ? new Vector4(border, border, border, border)
            : Vector4.zero;

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }

    private static string ToAbsolutePath(string assetPath)
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
    }

    public static void EnsureFolder(string folderPath)
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
