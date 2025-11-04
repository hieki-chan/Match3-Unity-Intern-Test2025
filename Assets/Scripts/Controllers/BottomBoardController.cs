using System;
using UnityEngine;
using UnityEngine.Analytics;

public class BottomBoardController : MonoBehaviour
{
    internal BottomBoard BottomBoard => _bottomBoard;

    GameManager gameManager;
    GameSettings _gameSettings;
    BottomBoard _bottomBoard;


    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        this.gameManager = gameManager;
        _gameSettings = gameSettings;
        _bottomBoard = new BottomBoard(_gameSettings.bottomBoardCellCount, this.transform);

        gameManager.StateChangedAction += OnGameStateChange;
    }

    public bool TryAssign(Cell cell)
    {
        bool val = _bottomBoard.TryAssign(cell, () =>
        {
            if (_bottomBoard.CheckForLoss())
            {
                gameManager.SetState(GameManager.eStateGame.GAME_OVER);
            }
        });

        return val;
    }


    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                break;
            case GameManager.eStateGame.PAUSE:
                break;
            case GameManager.eStateGame.GAME_OVER:
                break;
        }
    }
}