using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Analytics;

public class BottomCell : MonoBehaviour
{
    private Cell cell;

    public bool hasCell => cell != null;

    public Cell GetCell()
    {
        return cell;
    }

    public void AssignCell(Cell cell, Action onComplete = null)
    {
        if (cell == null)
        {
            this.cell = null;
            return;
        }

        this.cell = cell;
        cell.IsOnBoard = false;

        cell.transform.DOMove(transform.position, .2f).SetEase(Ease.InSine).onComplete = () => onComplete?.Invoke();
        cell.Item.View.transform.DOMove(transform.position, .2f).SetEase(Ease.InSine);
    }

    public void ClearCell()
    {
        cell.transform.DOScale(Vector3.zero, .1f);
        if(cell.Item != null && cell.Item.View != null)
            cell.Item.View.transform.DOScale(Vector3.zero, .1f);
        cell.Clear();
        cell = null;
    }
}