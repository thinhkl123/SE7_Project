using DG.Tweening;
using Fusion;
using SoundManager;
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

    public void SetPosition(int x, int y, bool isPlaySound = true)
    {
        if (isPlaySound)
            SoundsManager.Instance.PlaySFX(SoundType.Unit_Move_Chess);

        currentX = x;
        currentY = y;

        //check special position for king and queen
        if (type == PieceType.King)
        {
            if (team == Team.White)
            {
                if (x == 0 && y == 4)
                {
                    currentX = 0;
                    currentY = 3;
                }
            }
            else
            {
                if (x == 7 && y == 4)
                {
                    currentX = 0;
                    currentY = 3;
                }
            }
        }
        else if (type == PieceType.Queen)
        {
            if (team == Team.White)
            {
                if (x == 0 && y == 3)
                {
                    currentX = 0;
                    currentY = 4;
                }
            }
            else
            {
                if (x == 7 && y == 3)
                {
                    currentX = 0;
                    currentY = 4;
                }
            }
        }

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
