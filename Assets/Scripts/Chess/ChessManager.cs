using DG.Tweening;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    //response 
    [Networked] public float TurnTimeElapsedBeforeWindow { get; set; }
    [Networked] public bool IsSkipUIShown { get; set; } = false;
    [Networked] public bool IsSkipButtonPressed { get; set; } = false;

    //track chess pieces that have moved 
    // Track quân đã đi trong lượt hiện tại
    private List<ChessPiece> movedPiecesThisTurn = new List<ChessPiece>();

    public bool IsGameActiveForPlayer()
    {
        if (Runner == null) return false;

        if (!IsSpawned) return false;

        return IsGameActive;
    }

    [Header("Turn Timer Settings")]
    private float timePerTurn = 30f; 
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

    //public override void FixedUpdateNetwork()
    //{
    //    if (!IsGameActive) return;

    //    float elapsedTime = Runner.SimulationTime - turnStartTime;
    //    float timeRemaining = timePerTurn - elapsedTime;

    //    if (timeRemaining <= 0)
    //    {
    //        SwitchTurn();
    //    }

    //    //Debug.Log($"[FixedUpdateNetwork] Time Remaining: {timeRemaining}");
    //}

    //public override void Render()
    //{
    //    if (!IsGameActive) return;

    //    //Time update
    //    float elapsedTime = Runner.SimulationTime - turnStartTime;
    //    float timeRemaining = Mathf.Max(0, timePerTurn - elapsedTime);

    //    timerText.text = $"{(int)timeRemaining}s - {(Team)currentTurn}";

    //    //Turn update
    //    turnText.text = $"Turn: {TurnCount}";
    //}

    public override void FixedUpdateNetwork()
    {
        if (!ChessManager.Instance.IsGameActive) return;

        // Response window countdown — runs independently of turn timer
        if (UnoManager.Instance.IsResponseWindowOpen)
        {
            if (!IsSkipUIShown)
            {
                Rpc_ShowSkipUI();
                IsSkipUIShown = true;
            }
            float remaining = UnoManager.Instance.GetResponseWindowTimeRemaining();
            if (remaining <= 0f && Runner.IsServer)
            {
                UnoManager.Instance.Rpc_ResolveCard(); // auto-resolve after 7s
            }
            return; // pause turn timer while window is open
        }

        if( IsSkipUIShown ) 
        {
            Rpc_HideSkipUI();
            IsSkipUIShown = false;
        }
        float elapsedTime = Runner.SimulationTime - ChessManager.Instance.turnStartTime;
        float timeRemaining = ChessManager.Instance.timePerTurn - elapsedTime;

        if (timeRemaining <= 0)
        {
            ChessManager.Instance.SwitchTurn();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ShowSkipUI()
    {
        if (UnoManager.Instance.PendingCardTeam != myTeam )
        {
            UIManager.Instance.OpenUI<SkipResponseUI>();
        }
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_HideSkipUI()
    {
        UIManager.Instance.CloseUI<SkipResponseUI>();
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_PressSkipUI()
    {
        if(Runner.IsServer)
        {
            IsSkipButtonPressed = true;
            UnoManager.Instance.IsResponseWindowOpen = false;
            UnoManager.Instance.Rpc_ResolveCard();
        }
        UIManager.Instance.CloseUI<SkipResponseUI>();
    }

    public override void Render()
    {
        if (!ChessManager.Instance.IsGameActive) return;

        if (UnoManager.Instance.IsResponseWindowOpen)
        {
            float remaining = UnoManager.Instance.GetResponseWindowTimeRemaining();
            ChessManager.Instance.timerText.text = $"{(Team)((currentTurn == (int)Team.White) ? (int)Team.Black : (int)Team.White)}: {(int)remaining}s to response"; ;
            return;
        }

        float elapsedTime = Runner.SimulationTime - ChessManager.Instance.turnStartTime;
        float timeRemaining = Mathf.Max(0, ChessManager.Instance.timePerTurn - elapsedTime);
        ChessManager.Instance.timerText.text = $"{Mathf.Ceil(timeRemaining)}s - {(Team)currentTurn}";
        turnText.text = $"Turn: {TurnCount}";
    }



    public void SwitchTeam()
    {
        if (Runner.IsServer)
        {
            currentTurn = (currentTurn == (int)Team.White) ? (int)Team.Black : (int)Team.White;
        }

        SetPlayerTeam((myTeam == Team.White) ? Team.Black : Team.White);
        ReRenderChessBoard();
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
                    ChessPiece selected = chessPieces[hitPosition.x + hitPosition.y * ChessBoard.Instance.BoardSize.x];

                    // Nếu quân này đã đi rồi trong lượt này thì không cho chọn
                    if (movedPiecesThisTurn.Contains(selected))
                    {
                        Debug.Log("Quân này đã đi rồi trong lượt này!");
                        return;
                    }
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
                        ChessPiece selected = chessPieces[hitPosition.x + hitPosition.y * ChessBoard.Instance.BoardSize.x];
                        if (movedPiecesThisTurn.Contains(selected))
                        {
                            Debug.Log("Quân này đã đi rồi trong lượt này!");
                            // Clear drag hiện tại vì click vào quân không hợp lệ
                            currentlyDragging = null;
                            availableMoves.Clear();
                            ChessBoard.Instance.ClearHighlights();
                            return;
                        }
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
        ChessPiece capturedPiece = null;

        if (chessPieces[x + y * ChessBoard.Instance.BoardSize.x] != null)
        {
            capturedPiece = chessPieces[x + y * ChessBoard.Instance.BoardSize.x];

            if (capturedPiece.type == PieceType.King)
            {
                if (cp.team == myTeam)
                {
                    UIManager.Instance.OpenUI<CanvasWin>();
                }
                else
                {
                    UIManager.Instance.OpenUI<CanvasLose>();
                }

            }

            Destroy(capturedPiece.gameObject);
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
        if (chessPieces.Length > 4)
        {
            movedPiecesThisTurn.Add(chessPieces[x + y * ChessBoard.Instance.BoardSize.x]);
        }

        if (capturedPiece != null)
        {
            if (capturedPiece.type == PieceType.King)
            {
                RPC_EndGameWin(cp.team == Team.White ? (int)Team.White : (int)Team.Black, "Đã bắt được vua đối phương!");
            }
        }    

        if (TurnCount - 1 <= 0)
        {
            Debug.Log("Chuyển luợt!");
            SwitchTurn();
        }
        else
        {
            Debug.Log($"Còn {TurnCount - 1} nước nữa trước khi chuyển lượt!");
            SetTurnCount(TurnCount - 1);
        }
        // Làm mờ quân đã đi trong lượt này
        foreach (var movedPiece in movedPiecesThisTurn)
        {
            Image renderer = movedPiece.GetComponent<Image>();
            if (renderer != null)
            {
                Color c = renderer.color;
                c.a = 0.8f; // Giảm alpha để làm mờ
                renderer.color = c;

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
            UnoManager.Instance.Rpc_UpdateDrawCardButton();
        }
        foreach (var movedPiece in movedPiecesThisTurn)
        {
            Image renderer = movedPiece.GetComponent<Image>();
            if (renderer != null)
            {
                Color c = renderer.color;
                c.a = 1f;
                renderer.color = c;

            }
        }
        movedPiecesThisTurn.Clear();

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

    public void PlayerPressQuitButton()
    {
        Debug.Log("Người chơi chủ động nhấn thoát game.");

        if (Runner.IsServer)
        {
            Host_HandlePlayerQuit(Runner.LocalPlayer);
        }
        else
        {
            RPC_ClientRequestQuit(Runner.LocalPlayer);
        }
    }

    #region [RPCs] Điều phối giữa Host và Client

    // Client gửi yêu cầu này lên Host báo rằng mình chủ động nhấn nút Quit
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ClientRequestQuit(PlayerRef quittingPlayer)
    {
        Debug.Log($"RPC: Client {quittingPlayer.PlayerId} chủ động xin hàng.");
        Host_HandlePlayerQuit(quittingPlayer);
    }

    // Host gửi cho cả phòng thông báo đang đếm ngược chờ Reconnect
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyWaitingForReconnect(int secondsLeft)
    {
        // Hiển thị UI đếm ngược cho cả 2 bên thấy (nếu Client còn kết nối chập chờn)
        Debug.Log($"Trận đấu tạm dừng. Chờ đối thủ kết nối lại: {secondsLeft}s");
        // UI_Manager.ShowReconnectPopup(secondsLeft);
        NotiCanvas.Instance.ShowPopup($"Opponent lost connection. Waiting for reconnect... {secondsLeft}s", false, false);
    }

    // Host gửi cho cả phòng báo Reconnect thành công, tiếp tục chơi
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyReconnectSuccess()
    {
        Debug.Log("Đối thủ đã quay lại! Tiếp tục ván cờ.");
        NotiCanvas.Instance.ClosePopup();
        NotiCanvas.Instance.ShowTutorialText("Opponent reconnected! Continue the match.", 3f);
        // UI_Manager.HideReconnectPopup();
    }

    // Host gửi kết quả trận đấu cho Client còn lại khi có người bị xử thua
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_EndGameQuit(PlayerRef loserPlayer, string reason)
    {
        NotiCanvas.Instance.ClosePopup();

        if (Runner.LocalPlayer == loserPlayer)
        {
            Debug.Log($"Bạn đã THUA do: {reason}");
            UIManager.Instance.OpenUI<CanvasLose>();
        }
        else
        {
            Debug.Log($"Bạn đã THẮNG do: {reason}");
            UIManager.Instance.OpenUI<CanvasWin>();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_EndGameWin(int team, string reason)
    {
        if ((int)GetPlayerTeam() != team)
        {
            Debug.Log($"Bạn đã THUA do: {reason}");
            UIManager.Instance.OpenUI<CanvasLose>();
        }
        else
        {
            Debug.Log($"Bạn đã THẮNG do: {reason}");
            UIManager.Instance.OpenUI<CanvasWin>();
        }

        StartCoroutine(Co_HostDelayShutdown());
    }
    #endregion

    #region [Host Logic] Chỉ chạy trên máy Host

    public void Host_HandlePlayerQuit(PlayerRef loserPlayer)
    {
        if (!Runner.IsServer) return;

        RPC_EndGameQuit(loserPlayer, "Chủ động rời trận đấu (Đầu hàng)");

        StartCoroutine(Co_HostDelayShutdown());
    }

    private IEnumerator Co_HostDelayShutdown()
    {
        yield return new WaitForSeconds(0.5f);

        if (Runner != null)
        {
            Runner.Shutdown();
        }
    }

    public void Host_HandleClientReconnectTimeout(PlayerRef loserClientPlayer)
    {
        if (!Runner.IsServer) return;

        // Hết giờ kết nối lại -> Client bị xử thua
        RPC_EndGameQuit(loserClientPlayer, "Mất kết nối quá thời gian quy định");
    }

    #endregion

    #region [Local Logic] Chỉ hiển thị UI local trên máy từng người

    public void Local_ShowWaitingForClientUI(int secondsLeft)
    {
        // Gọi UI trên máy Host hiển thị: "Client mất mạng, đang chờ... X giây"
        NotiCanvas.Instance.ShowPopup($"Client lost connection. Waiting for reconnect... {secondsLeft}s", false, false);
    }

    public void Local_HandleHostDisconnected()
    {
        // Chạy trên máy Client khi nhận thấy Host sập mạng
        // Hiện UI: "Host (Chủ phòng) đã mất mạng đột ngột. Trận đấu này bị HỦY!"
        if (GetPlayerTeam() == Team.None)
            return;

        if (UIManager.Instance.IsOpened<CanvasWin>() || UIManager.Instance.IsOpened<CanvasLose>())
            return;

        NotiCanvas.Instance.ShowPopup("Host lost connection. The match is canceled.", true, false);
    }

    public void Local_HandleSelfHostDisconnected()
    {
        // Chạy trên máy Host nếu tự bản thân Host bị rớt mạng hoàn toàn khỏi internet
        // Hiện UI: "Bạn đã mất kết nối Internet. Trận đấu bị hủy."
        if (GetPlayerTeam() == Team.None)
            return;

        if (UIManager.Instance.IsOpened<CanvasWin>() || UIManager.Instance.IsOpened<CanvasLose>())
            return;

        NotiCanvas.Instance.ShowPopup("You lost connection to the Internet. The match is canceled.", true, false);
    }

    #endregion

    public void InitChessGame()
    {
        if (Runner.IsServer)
        {
            currentTurn = (int)Team.White;
            IsGameActive = true;
            TurnCount = 0;
            this.turnStartTime = Runner.SimulationTime;
            UnoManager.Instance.Rpc_UpdateDrawCardButton();
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
                IsGameActive = false;
                UIManager.Instance.OpenUI<LoadingUI>().ShowLoading("Waiting for opponent...", 0.5f);
            }
            else
            {
                UIManager.Instance.CloseUI<LoadingUI>();

                //InitChessGame();

                UnoManager.Instance.InitializeDeck();

                Debug.Log("Hai người chơi đã sẵn sàng. Trò chơi bắt đầu!");

                DOVirtual.DelayedCall(2f, () => InitChessGame());
            }
        }
    }
}