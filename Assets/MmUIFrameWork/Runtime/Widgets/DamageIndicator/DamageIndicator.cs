using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MieMieUIFrameWork.UI
{
    /// <summary>
    /// 生存向受伤屏幕反馈 全走 URP Volume 后处理
    /// 外部传入 0到1 强度 组件不负责算血量
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/MmUI/DamageIndicator")]
    public class DamageIndicator : MonoBehaviour
    {
        /// <summary> 残血最大去饱和 0为不变 100为全灰 </summary>
        [FoldoutGroup("残血 Post")]
        [LabelText("最大去饱和")]
        [PropertyRange(0f, 100f)]
        [SerializeField]
        private float m_MaxDesaturate = 55f;

        /// <summary> 残血冷色滤镜 </summary>
        [FoldoutGroup("残血 Post")]
        [LabelText("冷色滤镜")]
        [SerializeField]
        private Color m_ColdFilter = new Color(0.72f, 0.82f, 0.95f, 1f);

        /// <summary> 残血额外压暗 </summary>
        [FoldoutGroup("残血 Post")]
        [LabelText("残血压暗")]
        [PropertyRange(-2f, 0f)]
        [SerializeField]
        private float m_GrayExposure = -0.25f;

        /// <summary> 濒死 vignette 最大强度 </summary>
        [FoldoutGroup("濒死 Vignette")]
        [LabelText("最大压暗强度")]
        [PropertyRange(0f, 1f)]
        [SerializeField]
        private float m_MaxVignette = 0.5f;

        /// <summary> 濒死 vignette 颜色 </summary>
        [FoldoutGroup("濒死 Vignette")]
        [LabelText("边缘颜色")]
        [SerializeField]
        private Color m_VignetteColor = Color.black;

        /// <summary> 挨打边缘红色 </summary>
        [FoldoutGroup("挨打闪红")]
        [LabelText("边缘红色")]
        [SerializeField]
        private Color m_HitVignetteColor = Color.red;

        /// <summary> 挨打边缘闪红强度 </summary>
        [FoldoutGroup("挨打闪红")]
        [LabelText("边缘强度")]
        [PropertyRange(0f, 1f)]
        [SerializeField]
        private float m_HitVignette = 0.3f;

        /// <summary> 挨打 vignette 平滑 越大越靠四角 </summary>
        [FoldoutGroup("挨打闪红")]
        [LabelText("边缘收敛")]
        [PropertyRange(0.01f, 1f)]
        [SerializeField]
        private float m_HitVignetteSmoothness = 0.1f;

        /// <summary> 闪红淡入时长 </summary>
        [FoldoutGroup("挨打闪红")]
        [LabelText("淡入秒")]
        [SerializeField]
        private float m_HitFadeIn = 0.04f;

        /// <summary> 闪红淡出时长 </summary>
        [FoldoutGroup("挨打闪红")]
        [LabelText("淡出秒")]
        [SerializeField]
        private float m_HitFadeOut = 3f;

        /// <summary> 可选外部 Volume 为空则运行时自建 </summary>
        [FoldoutGroup("引用")]
        [LabelText("Volume 可空")]
        [SerializeField]
        private Volume m_Volume;

        /// <summary> 残血强度 0到1 </summary>
        private float m_LowHp;

        /// <summary> 濒死强度 0到1 </summary>
        private float m_Critical;

        /// <summary> 当前闪红权重 0到1 </summary>
        private float m_HitWeight;

        /// <summary> 是否由本组件创建了 VolumeProfile </summary>
        private bool m_OwnsProfile;

        /// <summary> 运行时 Profile </summary>
        private VolumeProfile m_RuntimeProfile;

        /// <summary> 色彩调整 </summary>
        private ColorAdjustments m_ColorAdjustments;

        /// <summary> 边缘压暗 </summary>
        private Vignette m_Vignette;

        /// <summary> 闪红 Tween </summary>
        private Tween m_HitTween;

        public float LowHp => m_LowHp;

        public float Critical => m_Critical;

        private void Awake()
        {
            EnsureVolume();
            ApplyVisual();
        }

        private void OnDestroy()
        {
            m_HitTween?.Kill();
            if (m_OwnsProfile && m_RuntimeProfile != null)
            {
                Destroy(m_RuntimeProfile);
                m_RuntimeProfile = null;
            }
        }

        private void OnValidate()
        {
            if (m_Volume == null)
            {
                m_Volume = null;
            }
        }

        /// <summary>
        /// 设置残血灰冷强度 0无效果 1满强度
        /// </summary>
        public void SetLowHp(float amount01)
        {
            m_LowHp = Mathf.Clamp01(amount01);
            ApplyVisual();
        }

        /// <summary>
        /// 设置濒死边缘压暗 0无效果 1满强度
        /// </summary>
        public void SetCritical(float amount01)
        {
            m_Critical = Mathf.Clamp01(amount01);
            ApplyVisual();
        }

        /// <summary>
        /// 同时设置残血与濒死强度
        /// </summary>
        public void SetState(float lowHp01, float critical01)
        {
            m_LowHp = Mathf.Clamp01(lowHp01);
            m_Critical = Mathf.Clamp01(critical01);
            ApplyVisual();
        }

        /// <summary>
        /// 播放挨打边缘闪红
        /// </summary>
        public void PlayHit()
        {
            PlayHit(1f);
        }

        /// <summary>
        /// 播放挨打边缘闪红 可调强度
        /// </summary>
        public void PlayHit(float intensity)
        {
            EnsureVolume();
            float peak = Mathf.Clamp01(intensity);

            m_HitTween?.Kill();
            m_HitWeight = 0f;
            m_HitTween = DOTween.Sequence()
                .Append(DOTween.To(() => m_HitWeight, w =>
                {
                    m_HitWeight = w;
                    ApplyVisual();
                }, peak, Mathf.Max(0.01f, m_HitFadeIn)))
                .Append(DOTween.To(() => m_HitWeight, w =>
                {
                    m_HitWeight = w;
                    ApplyVisual();
                }, 0f, Mathf.Max(0.01f, m_HitFadeOut)))
                .SetUpdate(true)
                .SetTarget(this);
        }

        /// <summary>
        /// 清除残血与濒死效果
        /// </summary>
        public void ClearState()
        {
            SetState(0f, 0f);
        }

#if UNITY_EDITOR
        [FoldoutGroup("调试")]
        [Button("测试挨打闪红")]
        private void EditorTestHit()
        {
            if (!Application.isPlaying) return;
            PlayHit();
        }

        [FoldoutGroup("调试")]
        [Button("测试残血")]
        private void EditorTestLowHp()
        {
            if (!Application.isPlaying) return;
            SetLowHp(0.7f);
        }

        [FoldoutGroup("调试")]
        [Button("测试濒死")]
        private void EditorTestCritical()
        {
            if (!Application.isPlaying) return;
            SetState(1f, 0.8f);
        }

        [FoldoutGroup("调试")]
        [Button("清除状态")]
        private void EditorTestClear()
        {
            if (!Application.isPlaying) return;
            ClearState();
        }
#endif

        /// <summary>
        /// 按外部强度写入 Volume
        /// </summary>
        private void ApplyVisual()
        {
            EnsureVolume();
            if (m_ColorAdjustments == null || m_Vignette == null) return;

            float grayT = Mathf.Clamp01(m_LowHp);
            float sat = Mathf.Lerp(0f, -m_MaxDesaturate, grayT);
            float exposure = Mathf.Lerp(0f, m_GrayExposure, grayT);
            Color filter = Color.Lerp(Color.white, m_ColdFilter, grayT);

            m_ColorAdjustments.active = true;
            m_ColorAdjustments.saturation.Override(sat);
            m_ColorAdjustments.postExposure.Override(exposure);
            m_ColorAdjustments.colorFilter.Override(filter);

            float hitT = Mathf.Clamp01(m_HitWeight);
            float vigFromCritical = m_MaxVignette * Mathf.Clamp01(m_Critical);
            float vigFromHit = m_HitVignette * hitT;
            float vigIntensity = Mathf.Max(vigFromCritical, vigFromHit);

            Color vigColor = m_VignetteColor;
            float vigSmooth = 0.45f;
            if (hitT > 0.001f)
            {
                float hitBlend = vigFromHit >= vigFromCritical
                    ? 1f
                    : Mathf.Clamp01(vigFromHit / Mathf.Max(0.001f, vigFromCritical));
                vigColor = Color.Lerp(m_VignetteColor, m_HitVignetteColor, Mathf.Max(hitT, hitBlend));
                vigSmooth = Mathf.Lerp(0.45f, m_HitVignetteSmoothness, hitT);
            }

            m_Vignette.active = true;
            m_Vignette.color.Override(vigColor);
            m_Vignette.intensity.Override(vigIntensity);
            m_Vignette.smoothness.Override(vigSmooth);
            m_Vignette.rounded.Override(true);
        }

        /// <summary>
        /// 确保 Volume 与覆盖项存在
        /// </summary>
        private void EnsureVolume()
        {
            if (m_ColorAdjustments != null && m_Vignette != null)
            {
                if (m_Volume != null)
                {
                    m_Volume.enabled = true;
                    m_Volume.weight = 1f;
                    m_Volume.isGlobal = true;
                }

                return;
            }

            if (m_Volume == null)
            {
                m_Volume = GetComponent<Volume>();
                if (m_Volume == null)
                {
                    m_Volume = gameObject.AddComponent<Volume>();
                }
            }

            m_Volume.enabled = true;
            m_Volume.isGlobal = true;
            m_Volume.weight = 1f;
            m_Volume.priority = Mathf.Max(m_Volume.priority, 50f);

            if (m_Volume.sharedProfile == null && m_Volume.profile == null)
            {
                m_RuntimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                m_RuntimeProfile.name = "DamageIndicator_RuntimeProfile";
                m_Volume.profile = m_RuntimeProfile;
                m_OwnsProfile = true;
            }
            else
            {
                m_RuntimeProfile = m_Volume.profile != null ? m_Volume.profile : m_Volume.sharedProfile;
            }

            if (m_RuntimeProfile == null) return;

            if (!m_RuntimeProfile.TryGet(out m_ColorAdjustments))
            {
                m_ColorAdjustments = m_RuntimeProfile.Add<ColorAdjustments>(true);
            }

            if (!m_RuntimeProfile.TryGet(out m_Vignette))
            {
                m_Vignette = m_RuntimeProfile.Add<Vignette>(true);
            }

            m_ColorAdjustments.active = true;
            m_Vignette.active = true;
        }
    }
}
