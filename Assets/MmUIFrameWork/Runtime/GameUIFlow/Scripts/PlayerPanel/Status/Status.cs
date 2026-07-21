using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MieMieUIFrameWork.UI
{
    /// <summary>
    /// 环形数值条表现 外界传值 Fill快跳 Ghost延迟追 Icon心跳 Number同步
    /// </summary>
    public class Status : MonoBehaviour
    {
        #region 引用

        [SerializeField]
        private Image GhostFill;

        [SerializeField]
        private Image Fill;

        [SerializeField]
        private TextMeshProUGUI Number;

        [SerializeField]
        private Image Icon;

        #endregion

        #region 动效参数

        /// <summary> Fill 追值时长 </summary>
        [FoldoutGroup("动效")]
        [LabelText("Fill时长")]
        [SerializeField]
        private float fillDuration = 0.15f;

        /// <summary> Ghost 延迟多久再追 </summary>
        [FoldoutGroup("动效")]
        [LabelText("Ghost延迟")]
        [SerializeField]
        private float ghostDelay = 0.25f;

        /// <summary> Ghost 追值时长 </summary>
        [FoldoutGroup("动效")]
        [LabelText("Ghost时长")]
        [SerializeField]
        private float ghostDuration = 0.45f;

        /// <summary> 心跳基础幅度 </summary>
        [FoldoutGroup("动效")]
        [LabelText("心跳基础幅度")]
        [SerializeField]
        private float heartBasePunch = 0.08f;

        /// <summary> 心跳最大额外幅度 </summary>
        [FoldoutGroup("动效")]
        [LabelText("心跳最大幅度")]
        [SerializeField]
        private float heartMaxPunch = 0.28f;

        /// <summary> 心跳单次时长 </summary>
        [FoldoutGroup("动效")]
        [LabelText("心跳时长")]
        [SerializeField]
        private float heartDuration = 0.35f;

        #endregion

        #region 运行时状态

        /// <summary> 当前显示数值 </summary>
        private float current;

        /// <summary> 最大数值 </summary>
        private float max = 100f;

        /// <summary> 当前归一化进度 </summary>
        private float normalized = 1f;

        /// <summary> Fill tween </summary>
        private Tween fillTween;

        /// <summary> Ghost tween </summary>
        private Tween ghostTween;

        /// <summary> Icon 心跳 tween </summary>
        private Tween iconTween;

        /// <summary> Icon 初始缩放 </summary>
        private Vector3 iconBaseScale = Vector3.one;

        #endregion

        #region 属性

        public float Current => current;

        public float Max => max;

        public float Normalized => normalized;

        #endregion

        #region 生命周期

        private void Awake()
        {
            EnsureRefs();
            if (Icon != null)
            {
                iconBaseScale = Icon.transform.localScale;
            }
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        #endregion

        #region 对外接口

        /// <summary>
        /// 立即设置数值无动效
        /// </summary>
        public void SetValueInstant(float current, float max)
        {
            EnsureRefs();
            KillTweens();

            float safeMax = Mathf.Max(0.0001f, max);
            this.max = safeMax;
            this.current = Mathf.Clamp(current, 0f, safeMax);
            normalized = this.current / safeMax;

            if (Fill != null) Fill.fillAmount = normalized;
            if (GhostFill != null) GhostFill.fillAmount = normalized;
            ApplyNumber();

            if (Icon != null)
            {
                Icon.transform.localScale = iconBaseScale;
            }
        }

        /// <summary>
        /// 设置数值并播放 Fill Ghost 延迟与 Icon 心跳
        /// </summary>
        public void SetValue(float current, float max = 100f)
        {
            EnsureRefs();

            float safeMax = Mathf.Max(0.0001f, max);
            float nextCurrent = Mathf.Clamp(current, 0f, safeMax);
            float nextNormalized = nextCurrent / safeMax;
            float delta01 = Mathf.Abs(nextNormalized - normalized);

            this.max = safeMax;
            this.current = nextCurrent;
            ApplyNumber();

            if (delta01 <= 0.0001f)
            {
                normalized = nextNormalized;
                if (Fill != null) Fill.fillAmount = normalized;
                if (GhostFill != null) GhostFill.fillAmount = normalized;
                return;
            }

            bool isDecrease = nextNormalized < normalized;
            AnimateFill(nextNormalized, isDecrease);
            PlayHeartBeat(delta01);
            normalized = nextNormalized;
        }

        /// <summary>
        /// 按归一化 0到1 设置
        /// </summary>
        public void SetNormalized(float normalized01, float maxForNumber = -1f)
        {
            float useMax = maxForNumber > 0f ? maxForNumber : max;
            float clamped = Mathf.Clamp01(normalized01);
            SetValue(clamped * useMax, useMax);
        }

        /// <summary>
        /// 在当前值上增减
        /// </summary>
        public void AddValue(float delta)
        {
            SetValue(current + delta, max);
        }

        #endregion

        #region 测试

#if UNITY_EDITOR
        [FoldoutGroup("测试")]
        [Button("满值 100")]
        private void TestFull()
        {
            if (!Application.isPlaying) return;
            SetValue(100f, 100f);
        }

        [FoldoutGroup("测试")]
        [Button("扣血 -20")]
        private void TestDamage20()
        {
            if (!Application.isPlaying) return;
            if (max <= 0f) SetValueInstant(100f, 100f);
            AddValue(-20f);
        }

        [FoldoutGroup("测试")]
        [Button("扣血 -50")]
        private void TestDamage50()
        {
            if (!Application.isPlaying) return;
            if (max <= 0f) SetValueInstant(100f, 100f);
            AddValue(-50f);
        }

        [FoldoutGroup("测试")]
        [Button("回血 +30")]
        private void TestHeal30()
        {
            if (!Application.isPlaying) return;
            if (max <= 0f) SetValueInstant(40f, 100f);
            AddValue(30f);
        }

        [FoldoutGroup("测试")]
        [Button("设为 10")]
        private void TestCritical()
        {
            if (!Application.isPlaying) return;
            SetValue(10f, 100f);
        }

        [FoldoutGroup("测试")]
        [Button("瞬间重置满血")]
        private void TestResetInstant()
        {
            if (!Application.isPlaying) return;
            SetValueInstant(100f, 100f);
        }
#endif

        #endregion

        #region 内部实现

        /// <summary>
        /// Fill 与 Ghost 的延迟追赶
        /// </summary>
        private void AnimateFill(float target01, bool isDecrease)
        {
            if (Fill == null || GhostFill == null) return;

            fillTween?.Kill();
            ghostTween?.Kill();

            if (isDecrease)
            {
                // 掉血 Fill 先掉 Ghost 延迟再追下来
                fillTween = Fill.DOFillAmount(target01, fillDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);

                ghostTween = GhostFill.DOFillAmount(target01, ghostDuration)
                    .SetDelay(ghostDelay)
                    .SetEase(Ease.InOutQuad)
                    .SetUpdate(true);
            }
            else
            {
                // 回血 Ghost 先顶上去 Fill 再追
                ghostTween = GhostFill.DOFillAmount(target01, fillDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);

                fillTween = Fill.DOFillAmount(target01, ghostDuration)
                    .SetDelay(ghostDelay * 0.5f)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true);
            }
        }

        /// <summary>
        /// Icon 心跳 幅度随变化量放大
        /// </summary>
        private void PlayHeartBeat(float delta01)
        {
            if (Icon == null) return;

            iconTween?.Kill();
            Icon.transform.localScale = iconBaseScale;

            float punch = Mathf.Lerp(heartBasePunch, heartMaxPunch, Mathf.Clamp01(delta01));
            iconTween = Icon.transform
                .DOPunchScale(Vector3.one * punch, heartDuration, 6, 0.6f)
                .SetUpdate(true)
                .OnKill(() =>
                {
                    if (Icon != null)
                    {
                        Icon.transform.localScale = iconBaseScale;
                    }
                });
        }

        /// <summary>
        /// 同步 Number 文本
        /// </summary>
        private void ApplyNumber()
        {
            if (Number == null) return;
            Number.text = Mathf.RoundToInt(current).ToString();
        }

        /// <summary>
        /// 停掉所有表现 tween
        /// </summary>
        private void KillTweens()
        {
            fillTween?.Kill();
            ghostTween?.Kill();
            iconTween?.Kill();
            fillTween = null;
            ghostTween = null;
            iconTween = null;
        }

        /// <summary>
        /// 补齐序列化引用
        /// </summary>
        private void EnsureRefs()
        {
            if (GhostFill == null)
            {
                var t = transform.Find("GhostFill");
                if (t != null) GhostFill = t.GetComponent<Image>();
            }

            if (Fill == null)
            {
                var t = transform.Find("Fill");
                if (t != null) Fill = t.GetComponent<Image>();
            }

            if (Number == null)
            {
                var t = transform.Find("Number");
                if (t != null) Number = t.GetComponent<TextMeshProUGUI>();
            }

            if (Icon == null)
            {
                var t = transform.Find("Icon");
                if (t != null) Icon = t.GetComponent<Image>();
            }
        }

        #endregion
    }
}
