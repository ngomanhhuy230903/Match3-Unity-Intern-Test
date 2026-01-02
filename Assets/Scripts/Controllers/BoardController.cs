using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    private Board m_board;
    private BottomCellsController m_bottomCells;
    private GameManager m_gameManager;
    private GameSettings m_gameSettings;
    private bool m_isBusy = false;

    private bool m_isAutoplay = false;
    private bool m_autoplayToWin = true;
    private Coroutine m_autoplayCoroutine;

    public bool IsBusy => m_isBusy;

    public void StartGame(GameManager manager, GameSettings settings, bool isAutoplay, bool autoplayToWin)
    {
        m_gameManager = manager;
        m_gameSettings = settings;
        m_isAutoplay = isAutoplay;
        m_autoplayToWin = autoplayToWin;

        m_board = new Board(this.transform, settings);
        m_board.FillWithDivisibleByThree();

        m_bottomCells = gameObject.AddComponent<BottomCellsController>();
        m_bottomCells.Setup(this.transform);

        m_bottomCells.OnCellsFullEvent += OnGameLose;
        m_bottomCells.OnMatchClearedEvent += OnMatchCleared;

        if (m_isAutoplay)
        {
            m_autoplayCoroutine = StartCoroutine(AutoplayRoutine());
        }
    }

    public void Update()
    {
        if (m_isBusy) return;
        if (m_isAutoplay) return;
        if (m_gameManager.State != GameManager.eStateGame.GAME_STARTED) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null && !cell.IsEmpty)
                {
                    OnCellClicked(cell);
                }
            }
        }
    }

    private IEnumerator AutoplayRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (m_gameManager.State == GameManager.eStateGame.GAME_STARTED)
        {
            if (m_isBusy)
            {
                yield return null;
                continue;
            }

            if (!m_bottomCells.CanAddItem())
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            Cell cellToClick = null;

            if (m_autoplayToWin)
            {
                cellToClick = GetBestCellForWin();
            }
            else
            {
                cellToClick = GetBestCellForLose();
            }

            if (cellToClick != null)
            {
                OnCellClicked(cellToClick);
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return null;
            }
        }
    }

    private Cell GetBestCellForWin()
    {
        Dictionary<string, int> bottomItemCounts = new Dictionary<string, int>();

        for (int i = 0; i < m_bottomCells.GetItemCount(); i++)
        {
            Item item = m_bottomCells.GetItemAt(i);
            if (item != null)
            {
                string typeKey = GetItemTypeString(item);
                if (!bottomItemCounts.ContainsKey(typeKey))
                    bottomItemCounts[typeKey] = 0;
                bottomItemCounts[typeKey]++;
            }
        }

        Dictionary<string, List<Cell>> boardItemsByType = new Dictionary<string, List<Cell>>();
        List<Cell> allCells = GetAllNonEmptyCells();

        foreach (var cell in allCells)
        {
            string typeKey = GetItemTypeString(cell.Item);
            if (!boardItemsByType.ContainsKey(typeKey))
                boardItemsByType[typeKey] = new List<Cell>();
            boardItemsByType[typeKey].Add(cell);
        }

        foreach (var kvp in bottomItemCounts)
        {
            if (kvp.Value == 2)
            {
                if (boardItemsByType.ContainsKey(kvp.Key) && boardItemsByType[kvp.Key].Count > 0)
                {
                    return boardItemsByType[kvp.Key][0];
                }
            }
        }

        foreach (var kvp in bottomItemCounts)
        {
            if (kvp.Value == 1)
            {
                if (boardItemsByType.ContainsKey(kvp.Key) && boardItemsByType[kvp.Key].Count > 0)
                {
                    return boardItemsByType[kvp.Key][0];
                }
            }
        }

        var mostCommonType = boardItemsByType.OrderByDescending(x => x.Value.Count).FirstOrDefault();
        if (mostCommonType.Value != null && mostCommonType.Value.Count > 0)
        {
            return mostCommonType.Value[0];
        }

        return allCells.Count > 0 ? allCells[0] : null;
    }

    private Cell GetBestCellForLose()
    {
        Dictionary<string, int> bottomItemCounts = new Dictionary<string, int>();

        for (int i = 0; i < m_bottomCells.GetItemCount(); i++)
        {
            Item item = m_bottomCells.GetItemAt(i);
            if (item != null)
            {
                string typeKey = GetItemTypeString(item);
                if (!bottomItemCounts.ContainsKey(typeKey))
                    bottomItemCounts[typeKey] = 0;
                bottomItemCounts[typeKey]++;
            }
        }

        Dictionary<string, List<Cell>> boardItemsByType = new Dictionary<string, List<Cell>>();
        List<Cell> allCells = GetAllNonEmptyCells();

        foreach (var cell in allCells)
        {
            string typeKey = GetItemTypeString(cell.Item);
            if (!boardItemsByType.ContainsKey(typeKey))
                boardItemsByType[typeKey] = new List<Cell>();
            boardItemsByType[typeKey].Add(cell);
        }

        List<string> typesToAvoid = new List<string>();
        foreach (var kvp in bottomItemCounts)
        {
            if (kvp.Value == 2)
            {
                typesToAvoid.Add(kvp.Key);
            }
        }

        foreach (var kvp in bottomItemCounts)
        {
            if (kvp.Value == 1)
            {
                typesToAvoid.Add(kvp.Key);
            }
        }

        foreach (var kvp in boardItemsByType)
        {
            if (!typesToAvoid.Contains(kvp.Key) && kvp.Value.Count > 0)
            {
                return kvp.Value[0];
            }
        }

        var leastCommonType = boardItemsByType
            .Where(x => !typesToAvoid.Contains(x.Key))
            .OrderBy(x => x.Value.Count)
            .FirstOrDefault();

        if (leastCommonType.Value != null && leastCommonType.Value.Count > 0)
        {
            Debug.Log($"Selecting least common type: {leastCommonType.Key}");
            return leastCommonType.Value[0];
        }

        if (allCells.Count > 0)
        {
            return allCells[UnityEngine.Random.Range(0, allCells.Count)];
        }

        return null;
    }

    private List<Cell> GetAllNonEmptyCells()
    {
        List<Cell> cells = new List<Cell>();

        for (int x = 0; x < m_gameSettings.BoardSizeX; x++)
        {
            for (int y = 0; y < m_gameSettings.BoardSizeY; y++)
            {
                if (!m_board.IsCellEmpty(x, y))
                {
                    Cell cell = m_board.GetCell(x, y);
                    if (cell != null)
                    {
                        cells.Add(cell);
                    }
                }
            }
        }

        return cells;
    }

    private string GetItemTypeString(Item item)
    {
        if (item is NormalItem normalItem)
        {
            return "NormalItem_" + normalItem.ItemType.ToString();
        }
        else if (item is BonusItem bonusItem)
        {
            return "BonusItem_" + bonusItem.ItemType.ToString();
        }
        return item.GetType().FullName;
    }

    private void OnCellClicked(Cell cell)
    {
        if (!m_bottomCells.CanAddItem())
        {
            return;
        }

        m_isBusy = true;
        Item item = cell.Item;
        cell.Free();

        m_bottomCells.AddItem(item, () =>
        {
            m_isBusy = false;
            CheckWinCondition();
        });
    }

    private void CheckWinCondition()
    {
        bool boardEmpty = true;
        for (int x = 0; x < m_gameSettings.BoardSizeX; x++)
        {
            for (int y = 0; y < m_gameSettings.BoardSizeY; y++)
            {
                if (!m_board.IsCellEmpty(x, y))
                {
                    boardEmpty = false;
                    break;
                }
            }
            if (!boardEmpty) break;
        }

        if (boardEmpty && m_bottomCells.GetItemCount() == 0)
        {
            OnGameWin();
        }
    }

    private void OnMatchCleared(int count)
    {
        CheckWinCondition();
    }

    private void OnGameWin()
    {
        if (m_autoplayCoroutine != null)
        {
            StopCoroutine(m_autoplayCoroutine);
        }
        m_isBusy = false;
        m_gameManager.GameOver(true);
    }

    private void OnGameLose()
    {
        if (m_autoplayCoroutine != null)
        {
            StopCoroutine(m_autoplayCoroutine);
        }
        m_isBusy = false;
        m_gameManager.GameOver(false);
    }

    public void Clear()
    {
        if (m_autoplayCoroutine != null)
        {
            StopCoroutine(m_autoplayCoroutine);
        }

        if (m_bottomCells != null)
        {
            m_bottomCells.OnCellsFullEvent -= OnGameLose;
            m_bottomCells.OnMatchClearedEvent -= OnMatchCleared;
            m_bottomCells.Clear();
        }

        if (m_board != null)
        {
            m_board.Clear();
        }
    }
}