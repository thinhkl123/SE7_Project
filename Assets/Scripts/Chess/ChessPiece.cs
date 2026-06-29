using DG.Tweening;
using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    public PieceType type;
    public Team team;

    public int currentX;
    public int currentY;

    private void Start()
    {
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(ChessBoard.Instance.CellSize, ChessBoard.Instance.CellSize);
    }

    public virtual List<Vector2Int> GetValidMoves(ChessPiece[] board, int tileCountX = 8, int tileCountY = 8)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        return moves;
    }

    public void SetPosition(int x, int y)
    {
        currentX = x;
        currentY = y;

        Vector2 newPos = new Vector2(0, 0);

        if (ChessManager.Instance.GetPlayerTeam() == Team.White)
        {
            newPos = ChessBoard.Instance.GetCellPos(currentX, currentY);
        }
        else
        {
            newPos = ChessBoard.Instance.GetCellPos(ChessBoard.Instance.BoardSize.x - 1 - currentX, ChessBoard.Instance.BoardSize.y - 1 - currentY);
        }

        this.GetComponent<RectTransform>().DOAnchorPos(newPos, 0.25f);
    }
}
