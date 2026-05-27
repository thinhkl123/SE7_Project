using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetValidMoves(ChessPiece[] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        int[] dx = { 1, 2, 2, 1, -1, -2, -2, -1 };
        int[] dy = { 2, 1, -1, -2, -2, -1, 1, 2 };

        for (int i = 0; i < 8; i++)
        {
            int nextX = currentX + dx[i];
            int nextY = currentY + dy[i];

            if (nextX >= 0 && nextX < tileCountX && nextY >= 0 && nextY < tileCountY)
            {
                if (board[nextX + nextY * ChessBoard.Instance.BoardSize.x] == null || board[nextX + nextY * ChessBoard.Instance.BoardSize.x].team != team)
                {
                    moves.Add(new Vector2Int(nextX, nextY));
                }
            }
        }
        return moves;
    }
}