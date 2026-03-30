using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum GamePhase
{
    Action,
    Buy,
    Cleanup,
    isSelecting,
    isGaining
}

public class CardZone
{
    public List<CardData> DataList;
    public Transform Transform;

    public CardZone(List<CardData> dataList, Transform transform)
    {
        DataList = dataList;
        Transform = transform;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;



    public GameObject cardPrefab;
    public GameObject cardBackPrefab;
    public GameObject currentDeckVisual;
    public Transform canvas;
    public Transform handParent;
    public Transform deckPosition;
    public Transform drawPosition;
    public Transform discardPosition;
    public Transform playArea;
    public Transform selectedArea;
    public Transform choicesArea;
    public Transform trashArea;
    public Transform zoomedCardParent;
    public int actions = 1;
    public int buys = 1;
    public int currentCoins = 0;
    public int victory = 0;
    public bool silver = false;
    public int merchant = 0;

    public GamePhase currentPhase;
    public CardLocation selectLocation;
    public int selectionLimit;
    public int gainCostLimit;
    public CardLocation gainDestination;




    public CardData copperData;
    public CardData estateData;
    public CardData villageData;
    public CardData smithyData;
    public CardData silverData;
    public CardData newCardData;

    public List<CardData> deck = new List<CardData>();
    public List<CardData> hand = new List<CardData>();
    public List<CardData> choices = new List<CardData>();
    public List<CardView> selectedCards = new List<CardView>();
    public List<CardData> discardPile = new List<CardData>();
    public List<CardData> trash = new List<CardData>();
    public List<CardData> aside = new List<CardData>();

    private Dictionary<CardLocation, CardZone> locationMap;


    private System.Action<List<CardView>> onSelectionComplete;

    void Awake()
    {
        Instance = this;
        locationMap = new Dictionary<CardLocation, CardZone>
        {
            { CardLocation.Hand,    new CardZone(hand, handParent) },
            { CardLocation.Choices, new CardZone(choices, choicesArea) },
            { CardLocation.Discard, new CardZone(discardPile, discardPosition) },
            { CardLocation.PlayArea,new CardZone(null, playArea) },
            { CardLocation.Select,  new CardZone(null, selectedArea) } 
        };
    }

    void SetupInitialDeck()
    {
        deck = new List<CardData> {
            copperData, copperData, copperData, copperData, copperData, copperData, copperData, estateData, estateData, estateData, 
            newCardData,
        };
            
        deck = GetShuffledDeck(deck);
        currentDeckVisual = Instantiate(cardBackPrefab, deckPosition);
    }

    public IEnumerator DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(RevealTopCard((cardData) => {
                hand.Add(cardData);
                DrawEffect();
                CreateCardView(cardData, handParent, CardLocation.Hand);
            }));
            yield return new WaitForSeconds(0.3f);
        }
    }

    public IEnumerator RevealTopCard(System.Action<CardData> onRevealed)
    {
        if (deck.Count == 0)
        {
            if (discardPile.Count == 0) yield break;
            yield return StartCoroutine(ShuffleDeck());
        }

        CardData card = deck[deck.Count - 1];
        deck.RemoveAt(deck.Count - 1);

        onRevealed?.Invoke(card);
    }

    private IEnumerator ShuffleDeck()
    {
        Transform cardTrigger = discardPosition.GetChild(discardPosition.childCount - 1);
        currentDeckVisual = Instantiate(cardBackPrefab, discardPosition);
        currentDeckVisual.transform.SetParent(canvas);
        currentDeckVisual.GetComponent<CardMovement>().MoveTo(deckPosition, 0.3f, false);
        Destroy(cardTrigger.gameObject);
        yield return new WaitForSeconds(0.1f);
        for (int i = discardPosition.childCount - 1; i >= 0; i--)
        {
            cardTrigger = discardPosition.GetChild(i);
            GameObject cardBack = Instantiate(cardBackPrefab, discardPosition);
            cardBack.transform.SetParent(canvas);
            cardBack.GetComponent<CardMovement>().MoveTo(deckPosition, 0.3f, true);
            Destroy(cardTrigger.gameObject);
            yield return new WaitForSeconds(0.1f);
        }
        deck.AddRange(discardPile);
        discardPile.Clear();
        deck = GetShuffledDeck(deck);
        yield return new WaitForSeconds(0.3f);
    }

    void DrawEffect()
    {
        if (deck.Count == 0)
        {
            currentDeckVisual.GetComponent<CardMovement>().MoveTo(drawPosition, 0.3f, true);
        }
        else 
        {
            GameObject cardBack = Instantiate(cardBackPrefab, deckPosition);
            cardBack.GetComponent<CardMovement>().MoveTo(drawPosition, 0.3f, true);
        }
    }

    public CardView CreateCardView(CardData data, Transform place, CardLocation location)
    {
        GameObject newCard = Instantiate(cardPrefab, place);
        CardView view = newCard.GetComponent<CardView>();
        view.SetCard(data, location);
        return view;
    }

    List<CardData> GetShuffledDeck(List<CardData> source)
    {
        var result = new List<CardData>(source);

        for (int i = result.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            var tmp = result[i];
            result[i] = result[r];
            result[r] = tmp;
        }
        return result;
    }

    public void PlayCard(CardView cardView)
    {
        if(cardView.data.cardtype == CardType.Action)
        {
            if (actions <= 0)
            {
                Debug.Log($"{cardView.data.cardName}はプレイできません！");
                return;
            }
            else
            {
                actions--;
            }
        }
        cardView.location = CardLocation.PlayArea;
        hand.Remove(cardView.data);
        cardView.transform.SetParent(canvas, true);
        cardView.GetComponent<CardMovement>().MoveTo(playArea, 0.3f, false);
        cardView.data.ExecuteEffect(this);
    }

    public void PlayCardFromVassal(CardView cardView)
    {
        cardView.location = CardLocation.PlayArea;
        discardPile.Remove(cardView.data);
        cardView.transform.SetParent(canvas, true);
        cardView.GetComponent<CardMovement>().MoveTo(playArea, 0.3f, false);
        cardView.data.ExecuteEffect(this);
    }

    public void ToggleCardSelection(CardView card)
    {
        if (selectedCards.Count >= selectionLimit)
        {
            Debug.Log($"これ以上選べません（上限: {selectionLimit}枚）");
            return;
        }
        choices.Remove(card.data);
        selectedCards.Add(card);
        card.location = CardLocation.Select;
        card.transform.SetParent(selectedArea, false);
        Debug.Log($"{card.data.cardName} を確定エリアへ移動しました。");
    }

    public void ReturnCard(CardView card)
    {
        selectedCards.Remove(card);
        choices.Add(card.data);
        card.transform.SetParent(choicesArea, false);
        card.location = selectLocation;
        Debug.Log($"{card.data.cardName} を選択候補に戻しました。");
    }

    public void StartSelection(int maxCount, CardLocation location, System.Predicate<CardData> condition, System.Action<List<CardView>> callback)
    {
        currentPhase = GamePhase.isSelecting;
        selectLocation = location;
        onSelectionComplete = callback;
        selectionLimit = maxCount;
        var allCardsInLocation = locationMap[selectLocation].DataList;
        for (int i = allCardsInLocation.Count - 1; i >= 0; i--)
        {
            if (condition == null || condition(allCardsInLocation[i]))
            {
                choices.Add(allCardsInLocation[i]);
                allCardsInLocation.RemoveAt(i);
            }
        }

        Transform sourceArea = locationMap[location].Transform;
        for (int i = sourceArea.childCount - 1; i >= 0; i--)
        {
            Transform child = sourceArea.GetChild(i);
            if (choices.Contains(child.GetComponent<CardView>().data))
            {
                child.transform.SetParent(choicesArea, false);
            }
        }
        UIManager.Instance.ShowSelectPanel();
    }

    public void FinishSelection()
    {
        
        Transform targetArea = locationMap[selectLocation].Transform;
        for (int i = choicesArea.childCount - 1; i >= 0; i--)
        {
            choicesArea.GetChild(i).SetParent(targetArea, false);
        }
        onSelectionComplete?.Invoke(selectedCards);
        currentPhase = GamePhase.Action;
        locationMap[selectLocation].DataList.AddRange(choices);
        choices.Clear();
        selectedCards.Clear();
    }

    public void BuyCard(CardData data)
    {
        SupplyManager supplyManager = Object.FindFirstObjectByType<SupplyManager>();
        if (supplyManager.supplyCounts[data] <= 0)
        {
            Debug.Log($"{data.cardName}は在庫切れです！");
            return;
        }
        if (data.cost > currentCoins || buys <= 0) {
            Debug.Log($"{data.cardName}は購入できません！");
            return;
        }
        buys--;
        currentCoins -= data.cost;
        CardView view = CreateCardView(data, zoomedCardParent, CardLocation.Discard);
        view.GetComponent<CardMovement>().MoveTo(discardPosition, 0.2f, false);
        Debug.Log($"{data.cardName}を購入しました！捨て札に追加します。");
        discardPile.Add(data);
        supplyManager.supplyCounts[data]--;
        if (buys <= 0)
        {
            StartCoroutine(CleanupPhase());
        }
    }

    public void GainCard(CardData data)
    {
        SupplyManager supplyManager = Object.FindFirstObjectByType<SupplyManager>();
        if (supplyManager.supplyCounts[data] <= 0)
        {
            Debug.Log($"{data.cardName}は在庫切れです！");
            return;
        }
        if (data.cost > gainCostLimit) {
            Debug.Log($"{data.cardName}は獲得できません！");
            return;
        }
        CardView view = CreateCardView(data, zoomedCardParent, gainDestination);
        switch (gainDestination)
        {
            case CardLocation.Discard:
                view.GetComponent<CardMovement>().MoveTo(discardPosition, 0.2f, false);
                discardPile.Add(data);
                break;
            case CardLocation.Hand:
                view.GetComponent<CardMovement>().MoveTo(handParent, 0.2f, false);
                hand.Add(data);
                break;
            case CardLocation.Deck:
                view.GetComponent<CardMovement>().MoveTo(deckPosition, 0.2f, true);
                deck.Add(data);
                break;
        }
        currentPhase = GamePhase.Action;
        supplyManager.supplyCounts[data]--;
    }

    public void StartGain(int maxCost, System.Predicate<CardData> condition, CardLocation destination)
    {
        currentPhase = GamePhase.isGaining;
        gainCostLimit = maxCost;
        gainDestination = destination;
    }

    public void NexPhase()
    {
        switch (currentPhase)
        {
            case GamePhase.Action:
                currentPhase = GamePhase.Buy;
                var treasures = hand.Where(c => c.cardtype == CardType.Treasure).ToList();
                foreach (var data in treasures)
                {
                    CardView view = FindViewByData(data); 
                    if (view != null) PlayCard(view);
                }
                Debug.Log($"購入フェイズ開始：現在の合計金量 = {currentCoins}");
                break;
            case GamePhase.Buy:
                StartCoroutine(CleanupPhase());
                break;
        }
    }

    public CardView FindViewByData(CardData data)
    {
        foreach (Transform child in handParent)
        {
            CardView view = child.GetComponent<CardView>();
            if (view != null && view.data == data)
            {
                return view;
            }
        }
        return null;
    }

    private IEnumerator CleanupPhase()
    {
        yield return new WaitForSeconds(0.2f);
        currentPhase = GamePhase.Cleanup;
        Debug.Log("クリーンアップ開始");

        foreach (Transform cardTrigger in playArea)
        {
            CardView view = cardTrigger.GetComponent<CardView>();
            if (view != null)
            {
                discardPile.Add(view.data);
                view.GetComponent<CardMovement>().MoveTo(discardPosition, 0.2f, false);
                view.location = CardLocation.Discard;
            }
        }

        foreach (Transform cardTrigger in handParent)
        {
            CardView view = cardTrigger.GetComponent<CardView>();
            if (view != null)
            {
                discardPile.Add(view.data);
                view.GetComponent<CardMovement>().MoveTo(discardPosition, 0.2f, false);
                view.location = CardLocation.Discard;
            }
        }

        yield return new WaitForSeconds(0.5f);

        actions = 1;
        buys = 1;
        currentCoins = 0;
        merchant = 0;
        silver = false;

        yield return StartCoroutine(DrawCards(5));

        currentPhase = GamePhase.Action;
        Debug.Log("新しいターン:アクションフェーズ");


    }

    void Start()
    {
        SetupInitialDeck();
        StartCoroutine(DrawCards(5));
        currentPhase = GamePhase.Action;
    }
}
