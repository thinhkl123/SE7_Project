using CustomUtils;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : SingletonMono<ChessBoard>
{
    [Header("Chessboard Settings")]
    public Vector2 BoardStartPoint = new Vector2(-360f, -360f);
    public float CellSize = 90f;
    public Vector2Int BoardSize = new Vector2Int(8, 8);

    [Header("HighLight")]
    [SerializeField] private Transform _hightLightTf;

    private void Start()
    {
        foreach (Transform child in _hightLightTf)
        {
            child.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (ChessManager.Instance == null || !ChessManager.Instance.IsGameActiveForPlayer())
            return;

        if (!ChessManager.Instance.IsPlayerTurn())
        {
            //Debug.Log("Chưa đến lượt bạn!");
            return;
        }

        if (ChessManager.Instance.TurnCount <= 0)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            ProcessBoardClick();
        }
    }

    private void ProcessBoardClick()
    {
        RectTransform utilityRect = transform as RectTransform;

        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            utilityRect,
            Input.mousePosition,
            null, 
            out localPoint
        );

        //Debug.Log($"[Update Loop] Local Point in Board Rect: {localPoint}");

        Vector2Int clickedCell = CalculateCellIndex(localPoint);

        if (IsValidCell(clickedCell))
        {
            OnCellClicked(clickedCell);
        }
        else
        {
            //Debug.Log($"Click ngoài phạm vi bàn cờ: {clickedCell}");
        }
    }

    private Vector2Int CalculateCellIndex(Vector2 localPos)
    {
        int cellX = Mathf.FloorToInt((localPos.x - BoardStartPoint.x) / CellSize);
        int cellY = Mathf.FloorToInt((localPos.y - BoardStartPoint.y) / CellSize);

        return new Vector2Int(cellX, cellY);
    }

    private bool IsValidCell(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < BoardSize.x &&
               cell.y >= 0 && cell.y < BoardSize.y;
    }

    private void OnCellClicked(Vector2Int cellIndices)
    {
        //Debug.Log($"Bạn vừa click vào ô: ({cellIndices.x}, {cellIndices.y})");
        if (ChessManager.Instance.GetPlayerTeam() == Team.Black)
        {
            cellIndices = new Vector2Int(BoardSize.x - 1 - cellIndices.x, BoardSize.y - 1 - cellIndices.y);
        }
        ChessManager.Instance.ProcessPiece(cellIndices);
    }

    public Vector2 GetCellPos(int x, int y)
    {
        float posX = BoardStartPoint.x + CellSize / 2 + x * CellSize;
        float posY = BoardStartPoint.y + CellSize / 2 + y * CellSize;
        return new Vector2(posX, posY);
    }

    public void HighlightCells(List<Vector2Int> cellsToHighlight)
    {
        foreach (Transform child in _hightLightTf)
        {
            child.gameObject.SetActive(false);
        }
        for (int i = 0; i < cellsToHighlight.Count; i++)
        {
            Vector2Int cell = cellsToHighlight[i];
            {
                if (IsValidCell(cell))
                {
                    if (ChessManager.Instance.GetPlayerTeam() == Team.Black)
                    {
                        cell = new Vector2Int(BoardSize.x - 1 - cell.x, BoardSize.y - 1 - cell.y);
                    }

                    Transform highlight = _hightLightTf.GetChild((BoardSize.y - 1 - cell.y) * BoardSize.x + cell.x);
                    highlight.gameObject.SetActive(true);
                }
            }
        }
    }

    public void ClearHighlights()
    {
        foreach (Transform child in _hightLightTf)
        {
            child.gameObject.SetActive(false);
        }
    }
}
