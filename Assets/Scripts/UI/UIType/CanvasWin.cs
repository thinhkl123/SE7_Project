using UnityEngine;
using UnityEngine.UI;

public class CanvasWin : UICanvas
{
    [Header("Buttons")]
    public Button btnHome;

    public override void Setup()
    {
        base.Setup();
        btnHome.onClick.RemoveAllListeners();
        btnHome.onClick.AddListener(OnHomeClicked);
    }

    private void OnHomeClicked()
    {
        Debug.Log("Return to Menu");

        UIManager.Instance.CloseUI<CanvasWin>();
        UIManager.Instance.OpenUI<HomeUI>();
    }
}