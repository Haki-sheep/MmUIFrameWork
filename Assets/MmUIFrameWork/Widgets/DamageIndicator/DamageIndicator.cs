using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MieMieFrameWork.UI
{
    /// <summary>
    /// 生存向受伤屏幕反馈 全走 URP Volume 后处理
    /// 挨打仅边缘闪红 残血去饱和发冷 濒死 vignette 压暗
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/MmUI/DamageIndicator")]
    public class DamageIndicator : MonoBehaviour
    {
        /// <summary> 灰化开始血量比例 高于此无灰化 </summary>
        [FoldoutGroup("血量区间")]
        [LabelText("灰化上区间")]
        [PropertyRange(0.01f, 1f)]
        [SerializeField]
        private float m_GrayUpper = 0.5f;

        /// <summary> 濒死阈值 低于此开启 vignette </summary>
        [FoldoutGroup("血量区间")]
        [LabelText("濒死下区间")]
        [PropertyRange(0.01f, 1f)]
        [SerializeField]
        private float m_CriticalLower = 0.2f;

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

        /// <summary> 当前归一化血量 </summary>
        private float m_Health01 = 1f;

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

        public float Health01 => m_Health01;

        public float GrayUpper
        {
            get => m_GrayUpper;
            set => m_GrayUpper = Mathf.Clamp(value, 0.01f, 1f);
        }

        public float CriticalLower
        {
            get => m_CriticalLower;
            set => m_CriticalLower = Mathf.Clamp(value, 0.01f, 1f);
        }

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
            if (m_CriticalLower > m_GrayUpper)
            {
                m_CriticalLower = m_GrayUpper;
            }

            // 清掉已销毁的 Volume 引用 避免 Inspector 一直 Missing
            if (m_Volume == null)
            {
                m_Volume = null;
            }
        }

        /// <summary>
        /// 设置当前与最大生命
        /// </summary>
        public void SetHealth(float current, float max)
        {
            float safeMax = Mathf.Max(0.0001f, max);
            SetHealth01(current / safeMax);
        }

        /// <summary>
        /// 设置归一化生命 0到1
        /// </summary>
        public void SetHealth01(float normalized)
        {
            m_Health01 = Mathf.Clamp01(normalized);
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
        /// 受伤便捷入口 闪红并刷新血量
        /// </summary>
        public void OnDamaged(float currentHp, float maxHp, float hitIntensity = 1f)
        {
            PlayHit(hitIntensity);
            SetHealth(currentHp, maxHp);
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
        [Button("测试残血 35%")]
        private void EditorTestGray()
        {
            if (!Application.isPlaying) return;
            SetHealth01(0.35f);
        }

        [FoldoutGroup("调试")]
        [Button("测试濒死 10%")]
        private void EditorTestCritical()
        {
            if (!Application.isPlaying) return;
            SetHealth01(0.1f);
        }

        [FoldoutGroup("调试")]
        [Button("回满血")]
        private void EditorTestFull()
        {
            if (!Application.isPlaying) return;
            SetHealth01(1f);
        }
#endif

        /// <summary>
        /// 合成血量态与闪红态后写入 Volume
        /// </summary>
        private void ApplyVisual()
        {
            EnsureVolume();
            if (m_ColorAdjustments == null || m_Vignette == null) return;

            float upper = Mathf.Max(m_GrayUpper, m_CriticalLower + 0.001f);
            float lower = Mathf.Min(m_CriticalLower, upper - 0.001f);

            float grayT = 0f;
            if (m_Health01 < upper)
            {
                grayT = Mathf.InverseLerp(upper, lower, m_Health01);
            }

            float sat = Mathf.Lerp(0f, -m_MaxDesaturate, grayT);
            float exposure = Mathf.Lerp(0f, m_GrayExposure, grayT);
            Color filter = Color.Lerp(Color.white, m_ColdFilter, grayT);

            m_ColorAdjustments.active = true;
            m_ColorAdjustments.saturation.Override(sat);
            m_ColorAdjustments.postExposure.Override(exposure);
            m_ColorAdjustments.colorFilter.Override(filter);

            float hitT = Mathf.Clamp01(m_HitWeight);

            float vigHealth = 0f;
            if (m_Health01 < lower)
            {
                vigHealth = Mathf.InverseLerp(lower, 0f, m_Health01);
            }

            float vigFromHealth = Mathf.Lerp(0f, m_MaxVignette, vigHealth);
            float vigFromHit = m_HitVignette * hitT;
            float vigIntensity = Mathf.Max(vigFromHealth, vigFromHit);

            // 挨打只用边缘红 不染全屏
            Color vigColor = m_VignetteColor;
            float vigSmooth = 0.45f;
            if (hitT > 0.001f)
            {
                float hitBlend = vigFromHit >= vigFromHealth ? 1f : Mathf.Clamp01(vigFromHit / Mathf.Max(0.001f, vigFromHealth));
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
