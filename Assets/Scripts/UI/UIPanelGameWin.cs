using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelGameWin : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnOK;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnOK.onClick.AddListener(OnClickOk);
    }

    private void OnDestroy()
    {
        if (btnOK) btnOK.onClick.RemoveAllListeners();
    }

    private void OnClickOk()
    {
        m_mngr.ShowMainMenu();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

}
