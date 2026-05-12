using UnityEngine;
using System.Collections.Generic;

public class SupplyManager : MonoBehaviour
{
    [Header("設定")]
    public GameObject supplyPrefab;
    public Transform actionSupply;
    public Transform victorySupply;
    public Transform treasureSupply;
    public List<CardData> cardTypes;
    public List<int> actionNumber;
    public int defaultCount = 10;

    public Dictionary<CardData, int> supplyCounts = new Dictionary<CardData, int>();
    public Dictionary<CardData, SupplyView> supplyViews = new Dictionary<CardData, SupplyView>();

    SupplyMode mode;

    void Start()
    {
        mode = GameSettings.Instance.selectedSupply;
        GenerateSupply();
    }

    void GenerateSupply()
    {
        for (int i = 0; i < 7; i++)
        {
            if (i < 3) AddSupply(cardTypes[i], treasureSupply);
            else AddSupply(cardTypes[i], victorySupply);
        }
        switch (mode)     
        {
            case SupplyMode.InitialGame:
                actionNumber = new List<int> {7, 8, 14, 12, 11, 16, 15, 21, 23, 27};
                break;
            case SupplyMode.FlexibleSize:
                actionNumber = new List<int> {9, 11, 18, 19, 22, 24, 28, 29, 31, 32};
                break;
            case SupplyMode.DeckTop:
                actionNumber = new List<int> {10, 13, 14, 17, 22, 24, 25, 26, 29, 32};
                break;
            case SupplyMode.HandTechnique:
                actionNumber = new List<int> {7, 13, 16, 18, 19, 20, 21, 25, 26, 29};
                break;
            case SupplyMode.Improvement:
                actionNumber = new List<int> {7, 8, 12, 15, 17, 20, 23, 27, 31, 32};
                break;
            case SupplyMode.GoldAndSilver:
                actionNumber = new List<int> {9, 10, 12, 13, 17, 18, 22, 26, 27, 28};
                break;
        }
        foreach (int i in actionNumber)
        {
            AddSupply(cardTypes[i], actionSupply);
        }
    }

    private void AddSupply(CardData data, Transform SupplyArea)
    {
        int count = GetInitialCount(data);
        supplyCounts.Add(data, count);
        GameObject go = Instantiate(supplyPrefab, SupplyArea);

        SupplyView view = go.GetComponent<SupplyView>();
        view.SetSupply(data, count);
        supplyViews.Add(data, view);
    }

    private int GetInitialCount(CardData data)
    {
        if (data.cardtype == CardType.Victory)
        {
            return 8;
        }
        if (data.cardName == "Copper") return 60;
        if (data.cardName == "Silver") return 40;
        if (data.cardName == "Gold") return 30;

        return 10;
    }

    public void DecreaseCount(CardData data)
    {
        supplyCounts[data]--;
        supplyViews[data].UpdateCount(supplyCounts[data]);
        if (supplyCounts[data] <= 0)
        {
            if (supplyViews.TryGetValue(data, out SupplyView view))
            {
                view.SetSupplyVisual(false);
            }
        }
    }
}
