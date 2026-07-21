using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace MieMieUIFrameWork.UI
{
    /// <summary>
    /// 体力条表现 外界传值驱动 PowerFill
    /// </summary>
    public class PowerBar : MonoBehaviour
    {
        #region 引用

        [SerializeField]
        private Image PowerFill;

        #endregion

        #region 动效参数

        /// <summary> Fill 追值时长 </summary>
        [FoldoutGroup("动效")]
        [LabelText("Fill时长")]
        [SerializeField]
        private float fillDuration = 0.2f;

        #endregion

        #region 运行时状态

        /// <summary> 当前体力 </summary>
        private float current;

        /// <summary> 最大体力 </summary>
        private float max = 100f;

        /// <summary> 当前归一化 </summary>
        private float normalized = 1f;

        /// <summary> Fill tween </summary>
        private Tween fillTween;

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
        }

        private void OnDestroy()
        {
            fillTween?.Kill();
        }

        #endregion

        #region 对外接口

        /// <summary>
        /// 立即设置体力无动效
        /// </summary>
        public void SetValueInstant(float current, float max)
        {
            EnsureRefs();
            fillTween?.Kill();

            float safeMax = Mathf.Max(0.0001f, max);
            this.max = safeMax;
            this.current = Mathf.Clamp(current, 0f, safeMax);
            normalized = this.current / safeMax;

            if (PowerFill != null)
            {
                PowerFill.fillAmount = normalized;
            }
        }

        /// <summary>
        /// 设置体力并缓动 Fill
        /// </summary>
        public void SetValue(float current, float max = 100f)
        { 
            EnsureRefs();
            if (PowerFill == null) return;

            float safeMax = Mathf.Max(0.0001f, max);
            float nextCurrent = Mathf.Clamp(current, 0f, safeMax);
            float nextNormalized = nextCurrent / safeMax;

            this.max = safeMax;
            this.current = nextCurrent;

            if (Mathf.Abs(nextNormalized - normalized) <= 0.0001f)
            {
                normalized = nextNormalized;
                PowerFill.fillAmount = normalized;
                return;
            }

            fillTween?.Kill();
            normalized = nextNormalized;
            fillTween = PowerFill.DOFillAmount(nextNormalized, fillDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
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
        /// 在当前体力上增减
        /// </summary>
        public void AddValue(float delta)
        {
            SetValue(current + delta, max);
        }

        #endregion

        #region 测试

#if UNITY_EDITOR
        [FoldoutGroup("测试")]
        [Button("满体力")]
        private void TestFull()
        {
            if (!Application.isPlaying) return;
            SetValue(100f, 100f);
        }

        [FoldoutGroup("测试")]
        [Button("消耗 -30")]
        private void TestSpend30()
        {
            if (!Application.isPlaying) return;
            if (max <= 0f) SetValueInstant(100f, 100f);
            AddValue(-30f);
        }

        [FoldoutGroup("测试")]
        [Button("消耗 -60")]
        private void TestSpend60()
        {
            if (!Application.isPlaying) return;
            if (max <= 0f) SetValueInstant(100f, 100f);
            AddValue(-60f);
        }

        [FoldoutGroup("测试")]
        [Button("恢复 +40")]
        private void TestRecover40()
        {
            if (!Application.isPlaying) return;
            if (max <= 0f) SetValueInstant(20f, 100f);
            AddValue(40f);
        }

        [FoldoutGroup("测试")]
        [Button("瞬间清空")]
        private void TestEmptyInstant()
        {
            if (!Application.isPlaying) return;
            SetValueInstant(0f, 100f);
        }
#endif

        #endregion

        #region 内部实现

        /// <summary>
        /// 补齐序列化引用
        /// </summary>
        private void EnsureRefs()
        {
            if (PowerFill != null) return;

            var t = transform.Find("PowerFill");
            if (t == null) t = transform.Find("Image/PowerFill");
            if (t != null) PowerFill = t.GetComponent<Image>();
        }

        #endregion
    }
}
