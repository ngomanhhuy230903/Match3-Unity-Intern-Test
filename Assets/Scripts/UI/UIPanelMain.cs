using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnTimer;
    [SerializeField] private Button btnMoves;
    [SerializeField] private Button btnAutoplay;
    [SerializeField] private Button btnAutoLose;

    private UIMainManager m_mngr;

    private void Awake()
    {
        if (btnMoves != null) btnMoves.onClick.AddListener(OnClickMoves);

        if (btnTimer != null) btnTimer.onClick.AddListener(OnClickTimer);

        if (btnAutoplay != null) btnAutoplay.onClick.AddListener(OnClickAutoplay);

        if (btnAutoLose != null) btnAutoLose.onClick.AddListener(OnClickAutoLose);
    }

    private void OnDestroy()
    {
        if (btnMoves) btnMoves.onClick.RemoveAllListeners();
        if (btnTimer) btnTimer.onClick.RemoveAllListeners();
        if (btnAutoplay) btnAutoplay.onClick.RemoveAllListeners();
        if (btnAutoLose) btnAutoLose.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickTimer()
    {
        m_mngr.LoadLevelTimer();
    }

    private void OnClickMoves()
    {
        m_mngr.LoadLevelMoves();
    }

    private void OnClickAutoplay()
    {
        m_mngr.LoadLevelAutoplay(true);
    }

    private void OnClickAutoLose()
    {
        m_mngr.LoadLevelAutoplay(false);
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