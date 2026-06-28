using UnityEngine;
using UnityEngine.UI;

public class CanvasLose : UICanvas
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

        UIManager.Instance.CloseUI<CanvasLose>();
        UIManager.Instance.OpenUI<HomeUI>();
    }
}