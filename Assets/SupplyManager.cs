using UnityEngine;
using System.Collections.Generic;

public class SupplyManager : MonoBehaviour
{
    [Header("設定")]
    public GameObject cardPrefab;
    public Transform supplyArea;
    public List<CardData> cardTypes;
    public int defaultCount = 10;

    public Dictionary<CardData, int> supplyCounts = new Dictionary<CardData, int>();

    void Start()
    {
        GenerateSupply();
    }

    void GenerateSupply()
    {
        foreach (CardData data in cardTypes)
        {
            int count = defaultCount; //あとでカードに枚数という設定をつけてそれを代入するようにする
            supplyCounts.Add(data, count);

            GameObject go = Instantiate(cardPrefab, supplyArea);
            CardView view = go.GetComponent<CardView>();
            view.SetCard(data, CardLocation.Supply);
        }
    }

    public void DecreaseCount(CardData data)
    {
        supplyCounts[data]--;
        Debug.Log($"{data.cardName}はあと、{supplyCounts[data]}枚です。");
    }
}
