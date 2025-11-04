using UnityEngine;
using UnityEngine.UI;

public class UIPanelLoss : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnPlayAgain;
    [SerializeField] private Button btnBackToHome;

    private UIMainManager m_mngr;


    private void Awake()
    {
        btnPlayAgain.onClick.AddListener(OnClickPlayAgain);
        btnBackToHome.onClick.AddListener(OnClickBackToHome);
    }

    private void OnDestroy()
    {
        if (btnBackToHome) btnBackToHome.onClick.RemoveAllListeners();
        if (btnPlayAgain) btnPlayAgain.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickPlayAgain()
    {
        
    }

    private void OnClickBackToHome()
    {
        m_mngr.ShowGameMenu();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
