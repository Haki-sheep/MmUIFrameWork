using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MieMieUIFrameWork.UI
{
    public enum ECrosshairType
    {
        Point,
        Crosshair,
        CrosshairNoPoint,
        XCrosshair
    }

    public class Crosshair : MonoBehaviour
    {
        [SerializeField]
        private Texture2D PointCrosshair;

        [SerializeField]
        private Texture2D CrosshairNoPoint;

        [SerializeField]
        private Texture2D CrosshairTexture;

        [SerializeField]
        private Texture2D XCrosshair;

        [SerializeField]
        private RawImage CrosshairRawImage;

        /// <summary> 击中放大倍率 </summary>
        [SerializeField]
        private float hitScale = 1.35f;

        /// <summary> 击中放大时长 </summary>
        [SerializeField]
        private float punchInDuration = 0.05f;

        /// <summary> 击中缓动缩回时长 </summary>
        [SerializeField]
        private float easeOutDuration = 0.22f;

        /// <summary> 移动时微扩倍率 </summary>
        [SerializeField]
        private float moveScale = 1.12f;

        /// <summary> 移动缩放切换时长 </summary>
        [SerializeField]
        private float moveDuration = 0.12f;

        /// <summary> 初始缩放 </summary>
        private Vector3 baseScale = Vector3.one;

        /// <summary> 击中动效 tween </summary>
        private Tween hitTween;

        /// <summary> 移动微扩 tween </summary>
        private Tween moveTween;

        /// <summary> 当前是否移动展开态 </summary>
        private bool isMovingExpanded;

        private void Awake()
        {
            CacheBaseScale();
        }

        private void OnDestroy()
        {
            hitTween?.Kill();
            moveTween?.Kill();
        }

        /// <summary>
        /// 切换准心样式 showAnimation 仅 Crosshair 表示移动微扩 X 表示击中
        /// </summary>
        public void SetCrosshair(ECrosshairType crosshairType, bool showAnimation)
        {
            switch (crosshairType)
            {
                case ECrosshairType.Point:
                    if (CrosshairRawImage.texture != PointCrosshair)
                    {
                        CrosshairRawImage.texture = PointCrosshair;
                    }

                    // 点准心无移动展开 复位
                    SetMoving(false);
                    break;
                case ECrosshairType.CrosshairNoPoint:
                    if (CrosshairRawImage.texture != CrosshairNoPoint)
                    {
                        CrosshairRawImage.texture = CrosshairNoPoint;
                    }

                    SetMoving(false);
                    break;
                case ECrosshairType.Crosshair:
                    if (CrosshairRawImage.texture != CrosshairTexture)
                    {
                        CrosshairRawImage.texture = CrosshairTexture;
                    }

                    SetMoving(showAnimation);
                    break;
                case ECrosshairType.XCrosshair:
                    if (CrosshairRawImage.texture != XCrosshair)
                    {
                        CrosshairRawImage.texture = XCrosshair;
                    }

                    XCrosshairAnimation(showAnimation);
                    break;
            }

            CrosshairRawImage.SetNativeSize();
            CacheBaseScale();
        }

        /// <summary>
        /// 显示隐藏准心
        /// </summary>
        public void SetCrosshairActive(bool isActive)
        {
            CrosshairRawImage.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// FPS 移动微扩 true 微微放大 false 缓回原大小
        /// </summary>
        public void SetMoving(bool isMoving)
        {
            if (CrosshairRawImage == null) return;
            if (isMoving == isMovingExpanded && moveTween != null && moveTween.IsActive()) return;

            isMovingExpanded = isMoving;
            moveTween?.Kill();

            Transform target = CrosshairRawImage.transform;
            Vector3 toScale = isMoving ? baseScale * moveScale : baseScale;
            moveTween = target.DOScale(toScale, moveDuration)
                .SetEase(isMoving ? Ease.OutQuad : Ease.OutCubic)
                .SetUpdate(true);
        }

        /// <summary>
        /// X准心击中动效 放大后缓动缩回
        /// </summary>
        public void XCrosshairAnimation(bool showAnimation)
        {
            if (CrosshairRawImage == null) return;

            hitTween?.Kill();
            Transform target = CrosshairRawImage.transform;

            if (!showAnimation)
            {
                target.localScale = isMovingExpanded ? baseScale * moveScale : baseScale;
                return;
            }

            Vector3 restScale = isMovingExpanded ? baseScale * moveScale : baseScale;
            target.localScale = restScale;
            hitTween = DOTween.Sequence()
                .Append(target.DOScale(baseScale * hitScale, punchInDuration).SetEase(Ease.OutQuad))
                .Append(target.DOScale(restScale, easeOutDuration).SetEase(Ease.OutBack))
                .SetUpdate(true)
                .OnKill(() =>
                {
                    if (CrosshairRawImage != null)
                    {
                        CrosshairRawImage.transform.localScale =
                            isMovingExpanded ? baseScale * moveScale : baseScale;
                    }
                });
        }

        /// <summary>
        /// 缓存当前基准缩放
        /// </summary>
        private void CacheBaseScale()
        {
            if (CrosshairRawImage == null) return;
            if (!isMovingExpanded && (hitTween == null || !hitTween.IsActive()))
            {
                baseScale = CrosshairRawImage.transform.localScale;
            }
        }
    }
}
