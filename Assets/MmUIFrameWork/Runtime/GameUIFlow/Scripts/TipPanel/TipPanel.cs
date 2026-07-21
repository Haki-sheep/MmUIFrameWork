/// <summary>
/// TipPanel Logic层 - 用户编写
/// </summary>

using MieMieUIFrameWork;
using MieMieUIFrameWork.UI;
using UnityEngine;
using UnityEngine.UI;

internal class TipPanel : UIWindowBase
{
    internal TipPanelGen View { get; private set; }

    internal protected override void OnAwake()
    {
        base.OnAwake();
        View = UIContent.GetComponent<TipPanelGen>();
    }

    internal protected override void OnShow()
    {
        base.OnShow();
    }

    internal protected override void OnHide()
    {
        base.OnHide();
    }

    internal protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void ShowTip(string tip)
    {
        View.InfoTextMeshProUGUI.text = tip;
        View.ImageDOTweenSequence.DOPlay().onComplete = () => {
            UICoreMgr.Instance.HideWindow<TipPanel>();
        };
    }

}
