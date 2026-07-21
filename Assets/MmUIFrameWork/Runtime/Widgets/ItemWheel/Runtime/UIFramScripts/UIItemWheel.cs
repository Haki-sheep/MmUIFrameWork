/// <summary>
/// UIItemWheel Logic层 - 用户编写
/// </summary>

using MieMieUIFrameWork;
using MieMieUIFrameWork.UI;
using UnityEngine;
using UnityEngine.UI;

internal class UIItemWheel : UIWindowBase
{
    internal UIItemWheelGen View { get; private set; }

    internal protected override void OnAwake()
    {
        base.OnAwake();
        View = UIContent.GetComponent<UIItemWheelGen>();
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

}
