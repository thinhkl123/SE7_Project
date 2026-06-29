using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class SkipResponseUI : UICanvas
{
    public Button SkipButton;
    private bool isPressSkipButton;
    public void OnEnable()
    {
        isPressSkipButton = false;
        SkipButton.onClick.AddListener(() =>
        {
            if(isPressSkipButton) return;
            PressSkipButton();
            isPressSkipButton = true;
        });
    }
    public void OnDisable()
    {
        SkipButton.onClick.RemoveListener(() =>
        {
            if (isPressSkipButton) return;
            PressSkipButton();
            isPressSkipButton = true;
        });
    }

    public void PressSkipButton()
    {
        if (!UnoManager.Instance.IsResponseWindowOpen) return;
        if (ChessManager.Instance.IsPlayerTurn()) return;
        ChessManager.Instance.Rpc_PressSkipUI();
    }

}
