using Fusion;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ChessPieceData", menuName = "ScriptableObjects/ChessPieceData", order = 1)]
public class ChessPieceSO : ScriptableObject
{
    public ChessPieceData[] pieces;

    public GameObject GetChessPiecePrefab(PieceType type, Team team)
    {
        foreach (ChessPieceData data in pieces)
        {
            if (data.type == type && data.team == team)
            {
                return data.prefab;
            }
        }
        return null; // Return null if no matching piece is found
    }
}

[Serializable]
public class ChessPieceData
{
    public PieceType type;
    public Team team;
    public GameObject prefab;
}
