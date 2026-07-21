/// <summary>
/// GameStartPanel Logic层 - 用户编写
/// </summary>

using MieMieUIFrameWork.UI;
using UnityEngine;

internal class GameStartPanel : UIWindowBase
{
    internal GameStartPanelGen View { get; private set; }

    internal protected override void OnAwake()
    {
        base.OnAwake();
        View = UIContent.GetComponent<GameStartPanelGen>();
        InitCompents();
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
        View.StartGameButton.onClick.RemoveListener(StartGame);
        View.ContinueGameButton.onClick.RemoveListener(ContinueGame);
        View.SettingGameButton.onClick.RemoveListener(SettingGame);
        View.DLCButton.onClick.RemoveListener(DLC);
        View.ProgramersButton.onClick.RemoveListener(Programers);
        View.ExitButton.onClick.RemoveListener(Exit);
    }

    private void InitCompents(){
        View.StartGameButton.onClick.AddListener(StartGame);
        View.ContinueGameButton.onClick.AddListener(ContinueGame);
        View.SettingGameButton.onClick.AddListener(SettingGame);
        View.DLCButton.onClick.AddListener(DLC);
        View.ProgramersButton.onClick.AddListener(Programers);
        View.ExitButton.onClick.AddListener(Exit);
    }


    private void StartGame()
    {
        // 跳转到中间界面 
    }

    private void ContinueGame()
    {
        // 如果存在存档 则直接读取存档开始游戏
    }

    private void SettingGame()
    {
        // 跳转到设置界面
    }

    private void DLC()
    {
        // TODO: 跳转到DLC界面
    }

    private void Programers()
    {
        // 跳转到开发者界面
    }

    private void Exit()
    {
        // 退出游戏
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

}
