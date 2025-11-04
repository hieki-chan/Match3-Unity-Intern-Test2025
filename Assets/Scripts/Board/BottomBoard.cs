using System;
using System.Collections.Generic;
using UnityEngine;

public class BottomBoard
{
    internal BottomCell[] BottomCell => _bottomCells;
    internal bool IsFull => _currentCellCount == _bottomCells.Length;
    internal int Count => _currentCellCount;
    private int _cellCount;
    private BottomCell[] _bottomCells;
    int _currentCellCount;

    private Transform _root;

    public BottomBoard(int cellCount, Transform root)
    {
        _cellCount = cellCount;
        _currentCellCount = 0;

        _root = root;

        CreateBoard();
    }

    private void CreateBoard()
    {
        float yOffset = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y + 1;
        Vector3 origin = new Vector3(-_cellCount * 0.5f + 0.5f, yOffset, 0f);
        GameObject bottomCell = Resources.Load<GameObject>(Constants.PREFAB_BTTOM_CELL);

        _bottomCells = new BottomCell[_cellCount];
        for (int x = 0; x < _cellCount; x++)
        {
            GameObject go = GameObject.Instantiate(bottomCell);
            go.transform.position = origin + new Vector3(x, 0f, 0f);
            go.transform.SetParent(_root);

            BottomCell cell = go.GetComponent<BottomCell>();
            //cell.Setup(x);

            _bottomCells[x] = cell;
        }
    }

    public bool TryAssign(Cell cell, Action onAssigned)
    {
        if (!cell.IsOnBoard)
            return false;

        if (_currentCellCount == 0)
        {
            _bottomCells[0].AssignCell(cell);
            _currentCellCount++;
            return true;
        }

        if (_currentCellCount == _bottomCells.Length)
        {
            onAssigned?.Invoke();
            return false;
        }

        int insertIndex = -1;
        for (int i = _currentCellCount - 1; i >= 0; i--)
        {
            if (_bottomCells[i].GetCell().IsSameType(cell))
            {
                insertIndex = i + 1;
                break;
            }
        }

        if (insertIndex == -1)
        {
            _bottomCells[_currentCellCount].AssignCell(cell);
            _currentCellCount++;
            return true;
        }

        for (int i = _currentCellCount; i > insertIndex; i--)
        {
            _bottomCells[i].AssignCell(_bottomCells[i - 1].GetCell());
        }

        _bottomCells[insertIndex].AssignCell(cell, () =>
        {
            if (CheckForClear(cell, insertIndex))
            {
                Debug.Log("clear");
                ClearCells(insertIndex - 2);
            }

            onAssigned?.Invoke();
        });
        _currentCellCount++;


        return true;
    }

    public bool CheckForLoss()
    {
        if (_currentCellCount == _bottomCells.Length)
        {
            Debug.Log("you loss");

            return true;
        }

        return false;
    }

    private bool CheckForClear(Cell cell, int index)
    {
        if (index - 2 >= 0 && _bottomCells[index - 2].GetCell().IsSameType(cell))
        {
           return true;
        }

        return false;
    }

    private void ClearCells(int startIndex)
    {
        for (int i = startIndex; i < startIndex + 3; i++)
        {
            _bottomCells[i].ClearCell();
        }

        for (int i = startIndex; i < _currentCellCount; i++)
        {
            if (i + 3 >= _bottomCells.Length)
                continue;

            if (!_bottomCells[i + 3].hasCell)
                continue;

            var cell = _bottomCells[i + 3].GetCell();
            _bottomCells[i].AssignCell(cell);
            _bottomCells[i + 3].AssignCell(null);
        }

        _currentCellCount -= 3;
    }
}
