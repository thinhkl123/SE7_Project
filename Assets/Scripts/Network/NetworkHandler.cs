using CustomUtils;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkHandler Instance { get; private set; }

    public List<GameObject> NetworkObjList;
    public List<Transform> NetworkObjParentList;

    [SerializeField] private float clientReconnectTimeout = 30f; // Thời gian chờ Client kết nối lại

    private Coroutine waitClientReconnectCoroutine;
    private PlayerRef disconnectedClientRef;

    private void Awake()
    {
        Instance = this;
        CreateNetworkObject();
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
        GameObject runnerObj = new GameObject("FusionRunner");
        //runnerObj.transform.SetParent(this.transform);

        _runner = runnerObj.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        //CreateNetworkObject();

        _runner.AddCallbacks(this);

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
            SceneManager = runnerObj.AddComponent<NetworkSceneManagerDefault>()
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

    private void CreateNetworkObject()
    {
        for (int i = 0; i < NetworkObjList.Count; i++)
        {
            if (NetworkObjList[i] != null)
            {
                GameObject obj = Instantiate(NetworkObjList[i], NetworkObjParentList[i]);
                //DontDestroyOnLoad(obj);
            }
        }
    }

    private void HandleFusionError(ShutdownReason reason, string roomName)
    {
        string errorMessage = "Error! Please try again";

        switch (reason)
        {
            case ShutdownReason.ServerInRoom:
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
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (!runner.IsServer)
        {
            Debug.Log($"[Client] Kết nối tới Host bị đứt đột ngột. Lý do: {shutdownReason}");
            ChessManager.Instance.Local_HandleHostDisconnected();
        }
        else
        {
            Debug.Log($"[Host] Chính bạn (Host) đã bị mất kết nối Internet/Cloud: {shutdownReason}");
            ChessManager.Instance.Local_HandleSelfHostDisconnected();
        }
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }
}