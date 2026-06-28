using CustomUtils;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkHandler : MonoBehaviour
{
    public static NetworkHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private NetworkRunner _runner;

    public async void JoinGame(GameMode mode, string roomName)
    {
        if (mode == GameMode.Host)
        {
            UIManager.Instance.OpenUI<LoadingUI>().ShowLoading("Creating room...");
        }
        else
        {
            UIManager.Instance.OpenUI<LoadingUI>().ShowLoading("Entering room...");
        }

        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name
        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            Scene = scene,
            PlayerCount = 2,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        UIManager.Instance.CloseUI<LoadingUI>();

        if (!result.Ok)
        {
            HandleFusionError(result.ShutdownReason, roomName);
            return;
        }

        if (mode == GameMode.Host)
        {
            ChessManager.Instance.SetPlayerTeam(Team.White);
        }
        else
        {
            ChessManager.Instance.SetPlayerTeam(Team.Black);
        }
    }

    private void HandleFusionError(ShutdownReason reason, string roomName)
    {
        string errorMessage = "Error! Please try again";

        switch (reason)
        {
            case ShutdownReason.GameIdAlreadyExists:
                errorMessage = $"Room has already exist";
                break;

            case ShutdownReason.GameIsFull:
                errorMessage = $"Room is full";
                break;

            case ShutdownReason.GameNotFound:
                errorMessage = $"Error Not Found Room";
                break;
        }

        UIManager.Instance.OpenUI<HomeUI>();
        NotiCanvas.Instance.ShowTutorialText(errorMessage, 2f);

        if (_runner != null)
        {
            Destroy(_runner.gameObject);
            _runner = null;
        }
    }
}