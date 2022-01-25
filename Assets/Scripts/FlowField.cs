using System.Collections.Generic;
using UnityEngine;

public class FlowField
{
    public Cell[,] grid { get; private set; }
    public Vector2Int gridSize { get; private set; }
    public float cellRadius { get; private set; }
    public Cell destinationCell;

    private float cellDiameter;

    public FlowField(float cellRadius, Vector2Int gridSize)
    {
        this.cellRadius = cellRadius;
        this.gridSize = gridSize;
        cellDiameter = cellRadius * 2f;
    }

    public void CreateGrid()
    {
        grid = new Cell[gridSize.x, gridSize.y];
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                Vector3 worldPos = new Vector3(cellDiameter * i + cellRadius, 0, cellDiameter * j + cellRadius);
                grid[i, j] = new Cell(worldPos, new Vector2Int(i, j));
            }
        }
    }

    public void CreateCostField()
    {
        Vector3 cellHalfExtents = Vector3.one * cellRadius;
        int terrainMask = LayerMask.GetMask("Collision");
        foreach (var curCell in grid)
        {
            Collider[] obstacles = Physics.OverlapBox(curCell.worldPos, cellHalfExtents, Quaternion.identity, terrainMask);
            foreach (var col in obstacles)
            {
                if (col.gameObject.layer == 8)
                {
                    curCell.IncreaseCost(255);
                    continue;
                }
            }
        }
    }

    public void CreateIntegrationFiled(Cell destinationCell)
    {
        this.destinationCell = destinationCell;
        destinationCell.cost = 0;
        destinationCell.bestCost = 0;

        Queue<Cell> cellsToCheck = new Queue<Cell>();
        cellsToCheck.Enqueue(destinationCell);

        while (cellsToCheck.Count > 0)
        {
            Cell curCell = cellsToCheck.Dequeue();
            List<Cell> curNeighbors = GetNeighborCells(curCell.gridIndex, GridDirection.CardinalDirections);
            foreach (var curNeighbor in curNeighbors)
            {
                if (curNeighbor.cost == byte.MaxValue)
                {
                    continue;
                }
                if (curNeighbor.cost + curCell.bestCost < curNeighbor.bestCost)
                {
                    curNeighbor.bestCost = (ushort)(curNeighbor.cost + curCell.bestCost);
                    cellsToCheck.Enqueue(curNeighbor);
                }
            }
        }
    }

    public void CreateFlowField()
    {
        foreach (var curCell in grid)
        {
            List<Cell> curNeighbors = GetNeighborCells(curCell.gridIndex, GridDirection.AllDirections);
            int bestCost = curCell.bestCost;
            foreach (var curNeighbor in curNeighbors)
            {
                if (curNeighbor.bestCost<bestCost)
                {
                    bestCost = curNeighbor.bestCost;
                    curCell.bestDirection = GridDirection.GetDirectionFromV2I(curNeighbor.gridIndex - curCell.gridIndex);
                }
            }
        }
    }

    private List<Cell> GetNeighborCells(Vector2Int nodeIndex,List<GridDirection> directions)
    {
        List<Cell> neighborCells = new List<Cell>();

        foreach (var curDirection in directions)
        {
            Cell newNeighbor = GetCellAtRelativePos(nodeIndex, curDirection);
            if (newNeighbor!=null)
            {
                neighborCells.Add(newNeighbor);
            }
        }
        return neighborCells;
    }

    private Cell GetCellAtRelativePos(Vector2Int orignPos, Vector2Int relativePos)
    {
        Vector2Int finalPos = orignPos + relativePos;

        if (finalPos.x < 0 || finalPos.x >= gridSize.x || finalPos.y < 0 || finalPos.y >= gridSize.y)
        {
            return null;
        }
        else
        {
            return grid[finalPos.x, finalPos.y];
        }
    }

    public Cell GetCellFromWorldPos(Vector3 worldPos)
    {
        float percentX = worldPos.x / (gridSize.x * cellDiameter);
        float percentY = worldPos.z / (gridSize.y * cellDiameter);

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.Clamp(Mathf.FloorToInt(gridSize.x * percentX), 0, gridSize.x - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(gridSize.y * percentY), 0, gridSize.y - 1);

        return grid[x, y];
    }
}

