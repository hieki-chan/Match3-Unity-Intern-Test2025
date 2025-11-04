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
        Dictionary<Cell, int> bCellMap = new Dictionary<Cell, int>();
        var boards = boardController.Boards;
        int expected = 2;

        while (!bottomBoardController.BottomBoard.IsFull)
        {
            CalculateBMap(bCellMap);
            bool picked = false;
            Cell randomCell = null;

            foreach (var board in boards)
            {
                for (int i = 0; i < board.BoardSizeX * board.BoardSizeY; i++)
                {
                    int x = i / board.BoardSizeY;
                    int y = i % board.BoardSizeY;
                    Cell cell = board.Cells[x, y];
                    if (!cell.IsOnBoard) continue;

                    int count = bCellMap.ContainsKey(cell) ? bCellMap[cell] : 0;

                    if (expected == 2 && count == 2)
                    {
                        if (boardController.TryMoveToBottomBoard(cell))
                        {
                            yield return delay;
                            picked = true;
                            expected = 2;
                            CalculateBMap(bCellMap);
                        }
                        break;
                    }

                    else if (expected == 1 && count == 1)
                    {
                        if (boardController.TryMoveToBottomBoard(cell))
                        {
                            yield return delay;
                            picked = true;
                            expected = 2;
                            CalculateBMap(bCellMap);
                        }
                        break;
                    }

                    else if (expected == 0)
                    {
                        if (boardController.TryMoveToBottomBoard(cell))
                        {
                            yield return delay;
                            picked = true;
                            expected = 1;
                            CalculateBMap(bCellMap);
                        }
                        break;
                    }

                    else if (randomCell == null)
                        randomCell = cell;
                    else if (Random.Range(0, 5) == 0)
                        randomCell = cell;
                }

                if (picked) break;
            }

            if (!picked)
            {
                if (!bCellMap.Any(k => k.Value == 2))
                    expected = 1;
                else if (!bCellMap.Any(k => k.Value == 1))
                    expected = 0;

                if (randomCell != null && boardController.TryMoveToBottomBoard(randomCell))
                {
                    yield return delay;
                    expected = 2;
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