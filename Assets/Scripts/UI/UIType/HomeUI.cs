using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class HomeUI : UICanvas
{
    [SerializeField] private Button HostBtn;
    [SerializeField] private Button ClientBtn;

    private bool isPressHost;
    private bool isPressClient;

    private void OnEnable()
    {
        isPressHost = false;
        isPressClient = false;
    }

    private void Start()
    {
        HostBtn.onClick.AddListener(() =>
        {
            if (isPressHost) return;
            isPressHost = true;
            UIManager.Instance.CloseUI<HomeUI>();
            NetworkHandler.Instance.JoinGame(GameMode.Host);
        });
        ClientBtn.onClick.AddListener(() =>
        {
            if (isPressClient) return;
            isPressClient = true;
            UIManager.Instance.CloseUI<HomeUI>();
            NetworkHandler.Instance.JoinGame(GameMode.Client);
        });
    }
}
