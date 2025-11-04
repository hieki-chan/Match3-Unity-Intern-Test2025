using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }
    internal Board[] Boards => m_boards;

    private Board[] m_boards;

    private GameManager m_gameManager;

    private BottomBoardController m_bottomBoardController;

    private Camera m_cam;

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private bool isHitting;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    private bool m_gameOver;

    public void StartGame(GameManager gameManager, GameSettings gameSettings,
        BottomBoardController bottomBoardController)
    {
        m_gameManager = gameManager;
        m_bottomBoardController = bottomBoardController;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        int totalBoardSize = CreateBoards(gameSettings);

        Fill(totalBoardSize);
    }

    private int CreateBoards(GameSettings gameSettings)
    {
        int totalBoardSize = 0;

        m_boards = new Board[gameSettings.BoardNum];
        for (int i = 0; i < gameSettings.BoardNum; i++)
        {
            var board = new Board(this.transform,
                gameSettings.MatchesMin,
                gameSettings.BoardSizeX - i,
                gameSettings.BoardSizeY - i,
                i);
            totalBoardSize += board.BoardSize;
            m_boards[i] = board;
        }

        return totalBoardSize;
    }

    private void Fill(int totalBoardSize)
    {
        var itemList = Utils.GetItemList(totalBoardSize);

        foreach (var item in itemList)
        {
            Debug.Log(item.Item1 + " " + item.Item2);
        }

        foreach (var board in m_boards)
        {
            board.Fill(itemList);

            //FindMatchesAndCollapse();
        }
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                //StopHints();
                break;
        }
    }


    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;
        if (m_gameManager.GameMode != GameManager.eGameMode.PLAY) return;

        //if (!m_hintIsShown)
        //{
        //    m_timeAfterFill += Time.deltaTime;
        //    if (m_timeAfterFill > m_gameSettings.TimeForHint)
        //    {
        //        m_timeAfterFill = 0f;
        //        ShowHint();
        //    }
        //}

        if (Input.GetMouseButtonDown(0) && !isHitting)
        {
            HandleHit();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetRayCast();
        }
    }

    private void HandleHit()
    {
        var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider == null)
        {
            return;
        }
        if (hit.transform.TryGetComponent(out Cell cell))
        {
            TryMoveToBottomBoard(cell);
        }

        isHitting = true;
    }

    public bool TryMoveToBottomBoard(Cell cell)
    {
        if(cell == null) return false;
        if (cell.IsActive && m_bottomBoardController.TryAssign(cell))
        {
            HandleOnMoveToBottom(cell.BoardZ);
            return true;
        }

        return false;
    }

    private void ResetRayCast()
    {
        isHitting = false;
    }

    private void HandleOnMoveToBottom(int level)
    {
        if (level < 0) return;
        if (level == 0)
        {
            Board board = m_boards[0];
            for (int i = 0; i < board.BoardSizeX * board.BoardSizeY; i++)
            {
                int x = i / board.BoardSizeY;
                int y = i % board.BoardSizeY;
                Cell lowerCell = board.Cells[x, y];
                if (lowerCell.IsOnBoard) return;
            }

            m_gameManager.SetState(GameManager.eStateGame.GAME_WON);
        }

        SetActiveUnCoveredCells(level);
    }

    private void SetActiveUnCoveredCells(int level)
    {

        Board currentBoard = m_boards[level];
        Board lowerBoard = m_boards[level - 1];

        for (int i = 0; i < lowerBoard.BoardSizeX * lowerBoard.BoardSizeY; i++)
        {
            int x = i / lowerBoard.BoardSizeY;
            int y = i % lowerBoard.BoardSizeY;
            Cell lowerCell = lowerBoard.Cells[x, y];

            Debug.Log("onboard" + lowerCell.IsOnBoard);
            if (!lowerCell.IsOnBoard) continue;
            Debug.Log("soonboard" + lowerCell.IsOnBoard);
            bool covered = false;

            for (int j = 0; j < currentBoard.BoardSizeX * currentBoard.BoardSizeY; j++)
            {
                int xc = j / currentBoard.BoardSizeY;
                int yc = j % currentBoard.BoardSizeY;
                Cell upperCell = currentBoard.Cells[xc, yc];
                if (!upperCell.IsOnBoard) continue;

                Debug.Log(IsOverlapping(lowerCell, upperCell) + "cell" + x + " " + y + "/" + xc + " " + yc);

                if (IsOverlapping(lowerCell, upperCell))
                {
                    covered = true;
                    break;
                }
            }

            Debug.Log(x + " " + y + "is:" + covered);

            if (!covered)
            {
                lowerCell.SetActive(true);
            }
        }
    }

    bool IsOverlapping(Cell lower, Cell upper)
    {
        if (lower.Item == null || upper.Item == null) return false;

        return Item.IsOverlapping(lower.Item, upper.Item);
    }


    //private void FindMatchesAndCollapse(Cell cell1, Cell cell2)
    //{
    //    if (cell1.Item is BonusItem)
    //    {
    //        cell1.ExplodeItem();
    //        StartCoroutine(ShiftDownItemsCoroutine());
    //    }
    //    else if (cell2.Item is BonusItem)
    //    {
    //        cell2.ExplodeItem();
    //        StartCoroutine(ShiftDownItemsCoroutine());
    //    }
    //    else
    //    {
    //        List<Cell> cells1 = GetMatches(cell1);
    //        List<Cell> cells2 = GetMatches(cell2);

    //        List<Cell> matches = new List<Cell>();
    //        matches.AddRange(cells1);
    //        matches.AddRange(cells2);
    //        matches = matches.Distinct().ToList();

    //        if (matches.Count < m_gameSettings.MatchesMin)
    //        {
    //            m_boards.Swap(cell1, cell2, () =>
    //            {
    //                IsBusy = false;
    //            });
    //        }
    //        else
    //        {
    //            OnMoveEvent();

    //            CollapseMatches(matches, cell2);
    //        }
    //    }
    //}

    //private void FindMatchesAndCollapse()
    //{
    //    List<Cell> matches = m_boards.FindFirstMatch();

    //    if (matches.Count > 0)
    //    {
    //        CollapseMatches(matches, null);
    //    }
    //    else
    //    {
    //        m_potentialMatch = m_boards.GetPotentialMatches();
    //        if (m_potentialMatch.Count > 0)
    //        {
    //            IsBusy = false;

    //            m_timeAfterFill = 0f;
    //        }
    //        else
    //        {
    //            //StartCoroutine(RefillBoardCoroutine());
    //            StartCoroutine(ShuffleBoardCoroutine());
    //        }
    //    }
    //}

    //private List<Cell> GetMatches(Cell cell)
    //{
    //    List<Cell> listHor = m_boards.GetHorizontalMatches(cell);
    //    if (listHor.Count < m_gameSettings.MatchesMin)
    //    {
    //        listHor.Clear();
    //    }

    //    List<Cell> listVert = m_boards.GetVerticalMatches(cell);
    //    if (listVert.Count < m_gameSettings.MatchesMin)
    //    {
    //        listVert.Clear();
    //    }

    //    return listHor.Concat(listVert).Distinct().ToList();
    //}

    private void CollapseMatches(List<Cell> matches, Cell cellEnd)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ExplodeItem();
        }

        if (matches.Count > m_gameSettings.MatchesMin)
        {
            foreach (var board in m_boards)
            {
                board.ConvertNormalToBonus(matches, cellEnd);
            }
        }

        StartCoroutine(ShiftDownItemsCoroutine());
    }

    private IEnumerator ShiftDownItemsCoroutine()
    {
        foreach (var board in m_boards)
        {
            board.ShiftDownItems();
        }

        yield return new WaitForSeconds(0.2f);

        //m_board.FillGapsWithNewItems();

        yield return new WaitForSeconds(0.2f);

        //FindMatchesAndCollapse();
    }

    //private IEnumerator RefillBoardCoroutine()
    //{
    //    foreach (var board in m_boards)
    //    {
    //        board.ExplodeAllItems();
    //    }

    //    yield return new WaitForSeconds(0.2f);

    //    foreach (var board in m_boards)
    //    {
    //        //board.Fill();
    //    }

    //    yield return new WaitForSeconds(0.2f);

    //    //FindMatchesAndCollapse();
    //}

    //private IEnumerator ShuffleBoardCoroutine()
    //{
    //    foreach (var board in m_boards)
    //    {
    //        board.Shuffle();
    //    }

    //    yield return new WaitForSeconds(0.3f);

    //    //FindMatchesAndCollapse();
    //}


    private void SetSortingLayer(Cell cell1, Cell cell2)
    {
        if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
        if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    }

    private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    {
        return cell1.IsNeighbour(cell2);
    }

    internal void Clear()
    {
        foreach (var board in m_boards)
        {
            board.Clear();
        }
    }

    private void ShowHint()
    {
        m_hintIsShown = true;
        foreach (var cell in m_potentialMatch)
        {
            cell.AnimateItemForHint();
        }
    }

    private void StopHints()
    {
        m_hintIsShown = false;
        foreach (var cell in m_potentialMatch)
        {
            cell.StopHintAnimation();
        }

        m_potentialMatch.Clear();
    }
}
