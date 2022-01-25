﻿using UnityEngine;

public class GridController:MonoBehaviour
{
    public Vector2Int gridSize;
    public float cellRadius = 0.5f;
    public FlowField curFlowField;
    public GridDebug gridDebug;

    private void InitializeFlowField()
    {
        curFlowField = new FlowField(cellRadius, gridSize);
        curFlowField.CreateGrid();
        gridDebug.SetFlowField(curFlowField);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            

            InitializeFlowField();
            curFlowField.CreateCostField();
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Cell destinationCell = curFlowField.GetCellFromWorldPos(worldMousePos);

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            curFlowField.CreateIntegrationFiled(destinationCell);
            curFlowField.CreateFlowField();

            stopwatch.Stop();
            UnityEngine.Debug.Log("用时=" + stopwatch.ElapsedMilliseconds + "ms");

            gridDebug.DrawFlowField();
        }
    }
}