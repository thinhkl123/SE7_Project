using Fusion;
using System.Linq;
using UnityEngine;

public class NetworkObjectManager : NetworkBehaviour, IPlayerJoined
{
    public static NetworkObjectManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool RunnerIsServer()
    {
        return Runner;
    }

    public float GetRunnerSimulationTime()
    {
        return Runner.SimulationTime;
    }

    public override void Spawned()
    {
        ChessManager.Instance.IsSpawned = true;
        ChessManager.Instance.SpawnAllPieces();

        //currentTurn = (int)Team.White; 
        //IsGameActive = true;
        //turnStartTime = Runner.SimulationTime;
    }

    public override void FixedUpdateNetwork()
    {
        if (!ChessManager.Instance.IsGameActive) return;
        float elapsedTime = Runner.SimulationTime - ChessManager.Instance.turnStartTime;
        float timeRemaining = ChessManager.Instance.timePerTurn - elapsedTime;
        if (timeRemaining <= 0)
        {
            ChessManager.Instance.SwitchTurn();
        }
        //Debug.Log($"[FixedUpdateNetwork] Time Remaining: {timeRemaining}");
    }
    public override void Render()
    {
        if (!ChessManager.Instance.IsGameActive) return;
        float elapsedTime = Runner.SimulationTime - ChessManager.Instance.turnStartTime;
        float timeRemaining = Mathf.Max(0, ChessManager.Instance.timePerTurn - elapsedTime);
        ChessManager.Instance.timerText.text = $"{(int)timeRemaining}s - {ChessManager.Instance.GetPlayerTeam()}";
    }

    public void PlayerJoined(PlayerRef player)
    {
        Debug.Log($"Player {player} joined the game.");

        UIManager.Instance.CloseUI<LoadingUI>();

        int playerCount = Runner.ActivePlayers.ToList().Count;

        bool isLocalMaster = Runner.IsServer;

        if (isLocalMaster)
        {
            if (playerCount < 2)
            {
                UIManager.Instance.OpenUI<LoadingUI>().ShowLoading("Waiting for opponent...", 0.5f);
            }
            else
            {
                UIManager.Instance.CloseUI<LoadingUI>();

                ChessManager.Instance.InitChessGame();

                UnoManager.Instance.InitializeDeck();

                Debug.Log("Hai người chơi đã sẵn sàng. Trò chơi bắt đầu!");
            }
        }
    }
}
