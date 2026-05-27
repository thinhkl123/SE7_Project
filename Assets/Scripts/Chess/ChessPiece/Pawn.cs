using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetValidMoves(ChessPiece[] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        int direction = (team == Team.White) ? 1 : -1;

        int nextY = currentY + direction;
        if (nextY >= 0 && nextY < tileCountY)
        {
            if (board[currentX + nextY * ChessBoard.Instance.BoardSize.x] == null)
            {
                moves.Add(new Vector2Int(currentX, nextY));

                int startRow = (team == Team.White) ? 1 : 6; 
                if (currentY == startRow)
                {
                    int nextY2 = currentY + (direction * 2);
                    if (board[currentX + nextY2 * ChessBoard.Instance.BoardSize.x] == null)
                    {
                        moves.Add(new Vector2Int(currentX, nextY2));
                    }
                }
            }
        }

        int[] diagonalX = { currentX - 1, currentX + 1 };
        foreach (int x in diagonalX)
        {
            if (x >= 0 && x < tileCountX && nextY >= 0 && nextY < tileCountY)
            {
                ChessPiece targetPiece = board[x + nextY * ChessBoard.Instance.BoardSize.x];
                if (targetPiece != null && targetPiece.team != team)
                {
                    moves.Add(new Vector2Int(x, nextY));
                }
            }
        }

        return moves;
    }
}