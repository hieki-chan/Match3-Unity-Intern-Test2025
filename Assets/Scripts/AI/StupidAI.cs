using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StupidAI
{
    GameManager gameManager;
    BoardController boardController;
    BottomBoardController bottomBoardController;

    WaitForSeconds delay = new WaitForSeconds(.25f);

    public StupidAI(GameManager gameManager,
        BoardController boardController, BottomBoardController bottomBoardController)
    {
        this.gameManager = gameManager;
        this.boardController = boardController;
        this.bottomBoardController = bottomBoardController;

        if (gameManager.GameMode == GameManager.eGameMode.AUTO_PLAY)
        {
            gameManager.StartCoroutine(AutoPlay());
        }
        else if (gameManager.GameMode == GameManager.eGameMode.AUTO_PLAY_LOSE)
        {
            gameManager.StartCoroutine(AutoLose());
        }
    }

    public IEnumerator AutoPlay()
    {
        var boards = boardController.Boards;
        Dictionary<Cell, int> bCellMap = new Dictionary<Cell, int>();

        while (!bottomBoardController.BottomBoard.IsFull && gameManager.State != GameManager.eStateGame.GAME_WON)
        {
            CalculateBMap(bCellMap);
            bool picked = false;
            Cell target = null;

            var pairCells = bCellMap.Where(k => k.Value == 2).Select(k => k.Key).ToList();
            if (pairCells.Count > 0)
            {
                foreach (var board in boards)
                {
                    foreach (var cell in board.Cells)
                    {
                        if (cell == null || !cell.IsOnBoard) continue;

                        if (pairCells.Any(pc => pc.IsSameType(cell)))
                        {
                            target = cell;
                            picked = true;
                            break;
                        }
                    }
                    if (picked) break;
                }
            }

            if (!picked)
            {
                var singleCells = bCellMap.Where(k => k.Value == 1).Select(k => k.Key).ToList();
                if (singleCells.Count > 0)
                {
                    foreach (var board in boards)
                    {
                        foreach (var cell in board.Cells)
                        {
                            if (cell == null || !cell.IsOnBoard) continue;
                            if (singleCells.Any(sc => sc.IsSameType(cell)))
                            {
                                target = cell;
                                picked = true;
                                break;
                            }
                        }
                        if (picked) break;
                    }
                }
            }

            if (!picked)
            {
                var validCells = boards
                     .SelectMany(b => b.Cells.Cast<Cell>())
                     .Where(c => c != null && c.IsOnBoard)
                     .ToList();

                if (validCells.Count > 0)
                {
                    target = validCells[Random.Range(0, validCells.Count)];
                    picked = true;
                }
            }

            if (picked && target != null)
            {
                if (boardController.TryMoveToBottomBoard(target))
                {
                    yield return delay;
                    CalculateBMap(bCellMap);
                }
            }

            yield return null;
        }

        void CalculateBMap(Dictionary<Cell, int> bCellMap)
        {
            bCellMap.Clear();
            foreach (var bottomCell in bottomBoardController.BottomBoard.BottomCell)
            {
                if (bottomCell == null || !bottomCell.hasCell) continue;
                var c = bottomCell.GetCell();
                if (c == null) continue;
                if (bCellMap.ContainsKey(c))
                    bCellMap[c]++;
                else
                    bCellMap[c] = 1;
            }
        }
    }



    public IEnumerator AutoLose()
    {
        Dictionary<Cell, int> bCellMap = new Dictionary<Cell, int>();

        var boards = boardController.Boards;

        while (!bottomBoardController.BottomBoard.IsFull)
        {
            foreach (var bottomCell in bottomBoardController.BottomBoard.BottomCell)
            {
                if (bottomCell == null || !bottomCell.hasCell)
                    break;

                if (bCellMap.ContainsKey(bottomCell.GetCell()))
                {
                    bCellMap[bottomCell.GetCell()]++;
                }
                else
                {
                    bCellMap[bottomCell.GetCell()] = 1;
                }
            }

            Cell randomCell = null;
            bool picked = false;

            foreach (var board in boards)
            {
                for (int i = 0; i < board.BoardSizeX * board.BoardSizeY; i++)
                {
                    int x = i / board.BoardSizeY;
                    int y = i % board.BoardSizeY;
                    Cell cell = board.Cells[x, y];

                    if (!cell.IsOnBoard) continue;


                    if (bCellMap.ContainsKey(cell))
                    {
                        if (bCellMap[cell] == 1)
                        {
                            if (boardController.TryMoveToBottomBoard(cell))
                                yield return delay;
                            picked = true;
                            if (IsGameOver()) yield break;
                        }

                        if (randomCell == null)
                            randomCell = cell;
                        else if (Random.Range(0, 5) == 0)
                            randomCell = cell;
                    }
                    else
                    {
                        if (boardController.TryMoveToBottomBoard(cell))
                            yield return delay;
                        picked = true;
                        if (IsGameOver()) yield break;
                    }
                }

                yield return null;
            }

            if (!picked)
            {
                boardController.TryMoveToBottomBoard(randomCell);
                if (IsGameOver()) yield break;
            }

            yield return delay;
        }

        bool IsGameOver() => gameManager.State == GameManager.eStateGame.GAME_OVER;
    }
}