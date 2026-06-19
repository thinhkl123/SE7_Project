using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class SkipResponseUI : UICanvas
{
    public Button SkipButton;
    public void OnEnable()
    {
        SkipButton.onClick.AddListener(() =>
        {
            PressSkipButton();
        });
    }
    public void OnDisable()
    {
        SkipButton.onClick.RemoveListener(() =>
        {
            PressSkipButton();
        });
    }

    public void PressSkipButton()
    {
        if (!UnoManager.Instance.IsResponseWindowOpen) return;
        if (ChessManager.Instance.IsPlayerTurn()) return;
        ChessManager.Instance.Rpc_PressSkipUI();
    }
}
