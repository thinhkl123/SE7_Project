using Fusion;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ChessManager : NetworkBehaviour, IPlayerJoined
{
    public static ChessManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    [Header("Chess Piece Settings")]
    [SerializeField] private ChessPieceSO ChessPieceSO;
    public bool IsSpawned = false;

    //Private variables
    private ChessPiece[] chessPieces = new ChessPiece[64];
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private Team myTeam = Team.None;

    [Header("Networked Variables")]
    [Networked] public int currentTurn { get; set; }
    [Networked] public bool IsGameActive { get; set; } = false;
    [Networked] public int TurnCount { get; set; }

    public bool IsGameActiveForPlayer()
    {
        if (!IsSpawned) return false;

        return IsGameActive;
    }

    [Header("Turn Timer Settings")]
    public float timePerTurn = 30f; 
    public TextMeshProUGUI timerText;
    [Networked] public float turnStartTime { get; set; }

    [Header("Turn Settings")]
    public TextMeshProUGUI turnText;

    public void SetPlayerTeam(Team team)
    {
        myTeam = team;
    }

    public Team GetPlayerTeam()
    {
        return myTeam;
    }

    public bool IsPlayerTurn()
    {
        return (int)myTeam == currentTurn;
    }

    public void SetTurnCount(int count)
    {
        if (Runner.IsServer)
            TurnCount = count;
    }

    public override void Spawned()
    {
        IsSpawned = true;
        SpawnAllPieces();

        //currentTurn = (int)Team.White; 
        //IsGameActive = true;
        //turnStartTime = Runner.SimulationTime;
    }

    public override void FixedUpdateNetwork()
    {
        if (!IsGameActive) return;

        float elapsedTime = Runner.SimulationTime - turnStartTime;
        float timeRemaining = timePerTurn - elapsedTime;

        if (timeRemaining <= 0)
        {
            if (Runner.IsServer)
                SwitchTurn();
        }

        //Debug.Log($"[FixedUpdateNetwork] Time Remaining: {timeRemaining}");
    }

    public override void Render()
    {
        if (!IsGameActive) return;

        //Time update
        float elapsedTime = Runner.SimulationTime - turnStartTime;
        float timeRemaining = Mathf.Max(0, timePerTurn - elapsedTime);

        timerText.text = $"{(int)timeRemaining}s - {(Team)currentTurn}";

        //Turn update
        turnText.text = $"Turn: {TurnCount}";
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_SwitchTeam()
    {
        if (Runner.IsServer)
        {
            currentTurn = (currentTurn == (int)Team.White) ? (int)Team.Black : (int)Team.White;
        }

        SetPlayerTeam((myTeam == Team.White) ? Team.Black : Team.White);
        ReRenderChessBoard();

        UnoManager.Instance.ReverserCard();
        SwitchTurn();
    }    

    private void ReRenderChessBoard()
    {
        for (int x = 0; x < ChessBoard.Instance.BoardSize.x; x++)
        {
            for (int y = 0; y < ChessBoard.Instance.BoardSize.y; y++)
            {
                if (chessPieces[x + y * ChessBoard.Instance.BoardSize.x] != null)
                {
                    chessPieces[x + y * ChessBoard.Instance.BoardSize.x].SetPosition(x, y);
                }
            }
        }
    }

    public void ProcessPiece(Vector2Int hitPosition)
    {
        if (currentlyDragging == null)
        {
            if (chessPieces[hitPosition.x + hitPosition.y * ChessBoard.Instance.BoardSize.x] != null)
            {
                if (chessPieces[hitPosition.x + hitPosition.y * ChessBoard.Instance.BoardSize.x].team == myTeam)
                {
                    currentlyDragging = chessPieces[hitPosition.x + hitPosition.y * ChessBoard.Instance.BoardSize.x];
                    availableMoves = currentlyDragging.GetValidMoves(chessPieces, ChessBoard.Instance.BoardSize.x, ChessBoard.Instance.BoardSize.y);
                    // Thêm code để highlight các ô có thể đi ở đây
                    List<Vector2Int> showPos = new List<Vector2Int>(availableMoves);
                    showPos.Add(hitPosition);
                    ChessBoard.Instance.HighlightCells(showPos);
                }
            }
        }
        else 
        {
            if (ContainsValidMove(ref availableMoves, new Vector2(hitPosition.x,hitPosition.y)))
            {
                Rpc_MoveTo(currentlyDragging.currentX, currentlyDragging.currentY, hitPosition.x, hitPosition.y);
                currentlyDragging = null;
                availableMoves.Clear();
            }
            else
            {
                if (chessPieces[hitPosition.x + hitPosition.y * ChessBoard.Instance.BoardSize.x] != null)
                {
                    if (chessPieces[hitPosition.x + hitPosition.y * ChessBoard.Instance.BoardSize.x].team == myTeam)
                    {
                        currentlyDragging = chessPieces[hitPosition.x + hitPosition.y * ChessBoard.Instance.BoardSize.x];
                        availableMoves = currentlyDragging.GetValidMoves(chessPieces, ChessBoard.Instance.BoardSize.x, ChessBoard.Instance.BoardSize.y);
                        // Thêm code để highlight các ô có thể đi ở đây
                        List<Vector2Int> showPos = new List<Vector2Int>(availableMoves);
                        showPos.Add(hitPosition);
                        ChessBoard.Instance.HighlightCells(showPos);
                    }
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_MoveTo(int originalX, int originalY, int x, int y)
    {
        ChessPiece cp = chessPieces[originalX + originalY * ChessBoard.Instance.BoardSize.x];

        if (chessPieces[x + y * ChessBoard.Instance.BoardSize.x] != null)
        {
            Destroy(chessPieces[x + y * ChessBoard.Instance.BoardSize.x].gameObject);
        }

        chessPieces[x + y * ChessBoard.Instance.BoardSize.x] = cp;
        chessPieces[originalX + originalY * ChessBoard.Instance.BoardSize.x] = null;
        cp.SetPosition(x, y);

        //Pawn Promotion
        if (cp.type == PieceType.Pawn)
        {
            bool isWhitePromote = (cp.team == Team.White && y == ChessBoard.Instance.BoardSize.y - 1);
            bool isBlackPromote = (cp.team == Team.Black && y == 0);

            if (isWhitePromote || isBlackPromote)
            {
                PromotePawnToQueen(x, y, cp.team);
            }
        }
        ChessBoard.Instance.ClearHighlights();

        if (Runner.IsServer)
        {
            if (TurnCount - 1 <= 0)
            {
                Debug.Log("Chuyển lượt!");
                SwitchTurn();
            }
            else
            {
                Debug.Log($"Còn {TurnCount - 1} lượt nữa trước khi chuyển lượt!");
                SetTurnCount(TurnCount - 1);
            }
        }
    }

    private void PromotePawnToQueen(int x, int y, Team team)
    {
        int boardWidth = ChessBoard.Instance.BoardSize.x;
        int targetIndex = x + y * boardWidth;

        ChessPiece pawnPiece = chessPieces[targetIndex];

        GameObject queenPrefab = ChessPieceSO.GetChessPiecePrefab(PieceType.Queen, team);
        GameObject newQueenGo = Instantiate(queenPrefab, transform);

        ChessPiece newQueen = newQueenGo.GetComponent<Queen>();
        newQueen.type = PieceType.Queen;
        newQueen.team = team;
        newQueen.SetPosition(x, y);

        if (pawnPiece != null)
        {
            Destroy(pawnPiece.gameObject);
        }
        chessPieces[targetIndex] = newQueen;
    }

    public void SwitchTurn()
    {
        if (Runner.IsServer)
        {
            Debug.Log("Switching turn...");
            currentTurn = (currentTurn == (int)Team.White) ? (int)Team.Black : (int)Team.White;
            turnStartTime = Runner.SimulationTime;
            SetTurnCount(0);
            UnoManager.Instance.SetIsReleasedCard(false);
            UnoManager.Instance.UpdateDrawCardButton();
        }
    }

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;
        return false;
    }

    public void SpawnAllPieces()
    {
        //Debug.Log("Đang spawn tất cả quân cờ...");
        chessPieces[0] = SpawnSinglePiece(PieceType.Rook, Team.White);
        chessPieces[1] = SpawnSinglePiece(PieceType.Knight, Team.White);
        chessPieces[2] = SpawnSinglePiece(PieceType.Bishop, Team.White);
        chessPieces[3] = SpawnSinglePiece(PieceType.Queen, Team.White); 
        chessPieces[4] = SpawnSinglePiece(PieceType.King, Team.White);  
        chessPieces[5] = SpawnSinglePiece(PieceType.Bishop, Team.White);
        chessPieces[6] = SpawnSinglePiece(PieceType.Knight, Team.White);
        chessPieces[7] = SpawnSinglePiece(PieceType.Rook, Team.White);
        
        for (int i = 0; i < ChessBoard.Instance.BoardSize.x; i++)
        {
            chessPieces[i + ChessBoard.Instance.BoardSize.x] = SpawnSinglePiece(PieceType.Pawn, Team.White);
        }

        chessPieces[7 * ChessBoard.Instance.BoardSize.x] = SpawnSinglePiece(PieceType.Rook, Team.Black);
        chessPieces[1 + 7 * ChessBoard.Instance.BoardSize.x] = SpawnSinglePiece(PieceType.Knight, Team.Black);
        chessPieces[2 + 7 * ChessBoard.Instance.BoardSize.x] = SpawnSinglePiece(PieceType.Bishop, Team.Black);
        chessPieces[3 + 7 * ChessBoard.Instance.BoardSize.x] = SpawnSinglePiece(PieceType.King, Team.Black); 
        chessPieces[4 + 7 * ChessBoard.Instance.BoardSize.x] = SpawnSinglePiece(PieceType.Queen, Team.Black);  
        chessPieces[5 + 7 * ChessBoard.Instance.BoardSize.x] = SpawnSinglePiece(PieceType.Bishop, Team.Black);
        chessPieces[6 + 7 * ChessBoard.Instance.BoardSize.x] = SpawnSinglePiece(PieceType.Knight, Team.Black);
        chessPieces[7 + 7 * ChessBoard.Instance.BoardSize.x] = SpawnSinglePiece(PieceType.Rook, Team.Black);

        for (int i = 0; i < ChessBoard.Instance.BoardSize.x; i++)
        {
            chessPieces[i + 6 * ChessBoard.Instance.BoardSize.x] = SpawnSinglePiece(PieceType.Pawn, Team.Black);
        }
        
        for (int x = 0; x < ChessBoard.Instance.BoardSize.x; x++)
        {
            for (int y = 0; y < ChessBoard.Instance.BoardSize.x; y++)
            {
                if (chessPieces[x + y * ChessBoard.Instance.BoardSize.x] != null)
                {
                    chessPieces[x + y * ChessBoard.Instance.BoardSize.x].SetPosition(x, y);
                }
            }
        }
    }

    private ChessPiece SpawnSinglePiece(PieceType type, Team team)
    {
        var prefab = ChessPieceSO.GetChessPiecePrefab(type, team);

        //Debug.Log($"Đang spawn quân {type} của đội {team}...");

        GameObject go = Instantiate(prefab, transform);

        ChessPiece cp = go.GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;

        return cp;
    }

    public void InitChessGame()
    {
        if (Runner.IsServer)
        {
            currentTurn = (int)Team.White;
            IsGameActive = true;
            TurnCount = 0;
            this.turnStartTime = Runner.SimulationTime;
        }
    }

    public void PlayerJoined(PlayerRef player)
    {
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

                InitChessGame();

                UnoManager.Instance.InitializeDeck();

                Debug.Log("Hai người chơi đã sẵn sàng. Trò chơi bắt đầu!");
            }
        }
    }
}