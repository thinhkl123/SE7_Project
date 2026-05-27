using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class HomeUI : UICanvas
{
    [SerializeField] private Button HostBtn;
    [SerializeField] private Button ClientBtn;

    private void Start()
    {
        HostBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseUI<HomeUI>();
            NetworkHandler.Instance.JoinGame(GameMode.Host);
        });
        ClientBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseUI<HomeUI>();
            NetworkHandler.Instance.JoinGame(GameMode.Client);
        });
    }
}
