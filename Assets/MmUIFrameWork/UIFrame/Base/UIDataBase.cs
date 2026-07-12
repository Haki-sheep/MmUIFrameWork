namespace MieMieFrameWork.UI
{
    using UnityEngine;
    using DG.Tweening;
    using UnityEngine.UI;

    /// <summary>
    /// 所有UI的基类
    /// </summary>
    public abstract class UIDataBase
    {
        #region 属性
        public GameObject UIGameObject { get; protected set; }
        public bool UIIsShow => UICanvasGroup.alpha > 0;
        public Canvas UICanvas { get; protected set; }
        public Transform UIContent { get; protected set; }
        public CanvasGroup UICanvasGroup { get; protected set; }
        public Image UIMask { get; protected set; }
        public bool ApplyAniamtion { get; set; } = false;
        #endregion

        #region 生命周期
        public abstract void BindGameObject(GameObject uiPrefab, Camera uiCamera);
        internal protected abstract void OnAwake();
        internal protected abstract void OnShow();
        internal protected abstract void OnHide();
        internal protected abstract void OnDestroy();
        #endregion

        #region 全局动画效果 
        protected virtual void GlobalAnimationShow()
        {
            this.UIContent.localScale = Vector3.one * 0.8f;
            this.UIContent.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                this.UICanvasGroup.DOFade(1f, 0.15f);
            });
        }

        protected virtual void GlobalAnimationHide()
        {
            this.UIContent.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
            {
                this.UICanvasGroup.DOFade(0f, 0.15f);
            });
        }
        #endregion
    }
}
