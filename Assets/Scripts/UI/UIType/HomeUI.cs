using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeUI : UICanvas
{
    [SerializeField] private Button HostBtn;
    [SerializeField] private Button ClientBtn;
    [SerializeField] private TMP_InputField RoomName;

    private bool isPressHost;
    private bool isPressClient;

    private void OnEnable()
    {
        isPressHost = false;
        isPressClient = false;
        RoomName.text = string.Empty;
    }

    private void Start()
    {
        HostBtn.onClick.AddListener(() =>
        {
            if (isPressHost) return;

            if (string.IsNullOrEmpty(RoomName.text))
            {
                NotiCanvas.Instance.ShowTutorialText("Please enter a room name.", 2f);
                return;
            }

            isPressHost = true;
            UIManager.Instance.CloseUI<HomeUI>();
            NetworkHandler.Instance.JoinGame(GameMode.Host, RoomName.text.ToLower());
        });
        ClientBtn.onClick.AddListener(() =>
        {
            if (isPressClient) return;

            if (string.IsNullOrEmpty(RoomName.text))
            {
                NotiCanvas.Instance.ShowTutorialText("Please enter a room name.", 2f);
                return;
            }

            isPressClient = true;
            UIManager.Instance.CloseUI<HomeUI>();
            NetworkHandler.Instance.JoinGame(GameMode.Client, RoomName.text.ToLower());
        });
    }
}
