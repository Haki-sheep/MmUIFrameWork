/// <summary>
/// GameStopPanel Logic层 - 用户编写
/// </summary>

using MieMieUIFrameWork;
using MieMieUIFrameWork.UI;
using UnityEngine;
using UnityEngine.UI;

internal class GameStopPanel : UIWindowBase
{
    internal GameStopPanelGen View { get; private set; }

    internal protected override void OnAwake()
    {
        base.OnAwake();
        View = UIContent.GetComponent<GameStopPanelGen>();
        InitView();
    }

    internal protected override void OnShow()
    {
        Time.timeScale = 0;
        base.OnShow();
    }

    internal protected override void OnHide()
    {
        Time.timeScale = 1;
        base.OnHide();
    }

    internal protected override void OnDestroy()
    {
        Time.timeScale = 1;
        base.OnDestroy();
    }

    private void InitView(){
        View.ContinueButton.onClick.AddListener(OnContinueButtonClick);
        View.SettingButton.onClick.AddListener(OnSettingButtonClick);
        View.SaveGameButton.onClick.AddListener(OnSaveGameButtonClick);
        View.LoadGameButton.onClick.AddListener(OnLoadGameButtonClick);
        View.ExitToMenuButton.onClick.AddListener(OnExitToMenuButtonClick);
        View.ExitToDeskTopButton.onClick.AddListener(OnExitToDeskTopButtonClick);
    }
    
    private void OnContinueButtonClick(){
        // 关闭此窗口
        UICoreMgr.Instance.CloseWindow<GameStopPanel>();
    }
    private void OnSettingButtonClick(){
        // 打开设置窗口
        UICoreMgr.Instance.ShowWindow<SettingPanel>();
    }
    private void OnSaveGameButtonClick(){
        // 保存游戏
        // GameManager.Instance.SaveGame();
    }
    private void OnLoadGameButtonClick(){
        // 加载游戏
        // GameManager.Instance.LoadGame();
    }
    private void OnExitToMenuButtonClick(){
        // 退出到菜单
        UICoreMgr.Instance.ShowWindow<GameStartPanel>(() => {
            UICoreMgr.Instance.CloseWindow<GameStopPanel>();
        });
    }
    private void OnExitToDeskTopButtonClick(){
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

}
