using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetValidMoves(ChessPiece[] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        int[] dx = { 1, -1, 1, -1 };
        int[] dy = { 1, 1, -1, -1 };

        for (int i = 0; i < 4; i++)
        {
            int nextX = currentX + dx[i];
            int nextY = currentY + dy[i];

            while (nextX >= 0 && nextX < tileCountX && nextY >= 0 && nextY < tileCountY)
            {
                if (board[nextX + nextY * ChessBoard.Instance.BoardSize.x] == null)
                {
                    moves.Add(new Vector2Int(nextX, nextY));
                }
                else
                {
                    if (board[nextX + nextY * ChessBoard.Instance.BoardSize.x].team != team)
                        moves.Add(new Vector2Int(nextX, nextY));

                    break;
                }
                nextX += dx[i];
                nextY += dy[i];
            }
        }
        return moves;
    }
}