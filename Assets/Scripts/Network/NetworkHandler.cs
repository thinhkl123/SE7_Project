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

    public async void JoinGame(GameMode mode)
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
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "ChessRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (mode == GameMode.Host)
        {
            ChessManager.Instance.SetPlayerTeam(Team.White);
        }
        else
        {
            ChessManager.Instance.SetPlayerTeam(Team.Black);
        }

        UIManager.Instance.CloseUI<LoadingUI>();
    }
}