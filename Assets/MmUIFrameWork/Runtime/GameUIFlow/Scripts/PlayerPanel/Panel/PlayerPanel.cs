/// <summary>
/// PlayerPanel Logic层 - 用户编写
/// </summary>

using MieMieUIFrameWork;
using MieMieUIFrameWork.UI;
using UnityEngine;
using UnityEngine.UI;

internal class PlayerPanel : UIWindowBase
{
    internal PlayerPanelGen View { get; private set; }

    internal protected override void OnAwake()
    {
        base.OnAwake();
        View = UIContent.GetComponent<PlayerPanelGen>();
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


    /// <summary>
    /// 设置准心与其动画效果
    /// </summary>
    public void SetCrosshair(ECrosshairType crosshairType,bool showAnimation){
        View.CrosshairCrosshair.SetCrosshair(crosshairType,showAnimation);
    }

    /// <summary>
    /// 设置体力
    /// </summary>
    public void SetPowerBar(float power){
        View.PowerPowerBar.SetValue(power);
    }

    /// <summary>
    /// 设置饱食度
    /// </summary>
    public void SetFoodStatus(float food){
        View.FoodStatus.SetValue(food);
    }
    
    /// <summary>
    /// 设置水分
    /// </summary>
    public void SetWaterStatus(float water){
        View.WaterStatus.SetValue(water);
    }

    /// <summary>
    /// 设置饱食度
    /// </summary>
    public void SetSanStatus(float san){
        View.SanStatus.SetValue(san);
    }

    /// <summary>
    /// 设置血量
    /// </summary>
    public void SetHealthStatus(float health){
        View.HealthStatus.SetValue(health);
    }
    

}
