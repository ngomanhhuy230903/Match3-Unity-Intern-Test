using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using System;

public class BottomCellsController : MonoBehaviour
{
    public event Action OnCellsFullEvent = delegate { };
    public event Action<int> OnMatchClearedEvent = delegate { };

    private const int MAX_CELLS = 5;
    private List<Transform> cellPositions = new List<Transform>();
    private List<Item> itemsInCells = new List<Item>();

    private Transform root;

    public void Setup(Transform parent)
    {
        root = parent;
        CreateBottomCells();
    }

    private void CreateBottomCells()
    {
        Vector3 startPos = new Vector3(-2f, -4f, 0f);
        GameObject cellPrefab = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);

        for (int i = 0; i < MAX_CELLS; i++)
        {
            GameObject cell = GameObject.Instantiate(cellPrefab);
            cell.transform.position = startPos + new Vector3(i * 1f, 0f, 0f);
            cell.transform.SetParent(root);

            cellPositions.Add(cell.transform);
        }
    }

    public bool CanAddItem()
    {
        return itemsInCells.Count < MAX_CELLS;
    }

    public void AddItem(Item item, Action onComplete)
    {
        if (!CanAddItem())
        {
            onComplete?.Invoke();
            return;
        }

        itemsInCells.Add(item);

        int index = itemsInCells.Count - 1;
        item.View.DOMove(cellPositions[index].position, 0.3f).OnComplete(() =>
        {
            CheckForMatches();
            onComplete?.Invoke();
        });
    }

    public void RemoveItem(Item item)
    {
        if (itemsInCells.Contains(item))
        {
            itemsInCells.Remove(item);
            ReorganizeItems();
        }
    }

    public Item GetItemAtPosition(Vector3 worldPosition)
    {
        for (int i = 0; i < itemsInCells.Count; i++)
        {
            if (itemsInCells[i].View != null)
            {
                SpriteRenderer spriteRenderer = itemsInCells[i].View.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Bounds bounds = spriteRenderer.bounds;
                    if (bounds.Contains(worldPosition))
                    {
                        return itemsInCells[i];
                    }
                }
                else
                {
                    float distance = Vector3.Distance(itemsInCells[i].View.position, worldPosition);
                    if (distance < 0.5f)
                    {
                        return itemsInCells[i];
                    }
                }
            }
        }
        return null;
    }

    private void CheckForMatches()
    {
        var grouped = itemsInCells
            .GroupBy(item => GetItemTypeString(item))
            .Where(g => g.Count() >= 3)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (grouped != null)
        {
            List<Item> matchedItems = grouped.Take(3).ToList();
            ClearMatches(matchedItems);
        }
        else if (itemsInCells.Count >= MAX_CELLS)
        {
            OnCellsFullEvent?.Invoke();
        }
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

    private void ClearMatches(List<Item> matches)
    {
        foreach (var item in matches)
        {
            item.View.DOScale(0f, 0.2f).OnComplete(() =>
            {
                GameObject.Destroy(item.View.gameObject);
            });

            itemsInCells.Remove(item);
        }

        OnMatchClearedEvent?.Invoke(3);

        ReorganizeItems();
    }

    private void ReorganizeItems()
    {
        for (int i = 0; i < itemsInCells.Count; i++)
        {
            itemsInCells[i].View.DOMove(cellPositions[i].position, 0.2f);
        }
    }

    public void Clear()
    {
        foreach (var item in itemsInCells)
        {
            if (item.View != null)
            {
                GameObject.Destroy(item.View.gameObject);
            }
        }
        itemsInCells.Clear();

        foreach (var cell in cellPositions)
        {
            GameObject.Destroy(cell.gameObject);
        }
        cellPositions.Clear();
    }

    public int GetItemCount()
    {
        return itemsInCells.Count;
    }

    public Item GetItemAt(int index)
    {
        if (index < 0 || index >= itemsInCells.Count)
            return null;

        return itemsInCells[index];
    }
}