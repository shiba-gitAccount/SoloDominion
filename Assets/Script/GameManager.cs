using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public enum GamePhase
{
    Action,
    Buy,
    Cleanup,
    isSelecting,
    isGaining,
    ActionPlaying
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
    public Transform asideArea;
    public Transform selectedArea;
    public Transform choicesArea;
    public Transform trashArea;
    public Transform effectLayer;

    [SerializeField] private TextMeshProUGUI buyText;
    [SerializeField] private TextMeshProUGUI coinText;
    public TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private TextMeshProUGUI message;

    public int totalDeckCount;
    public int actions = 1;
    public int buys = 1;
    public int currentCoins = 0;
    public int totalscore = 3;
    public bool silver = false;
    public int merchant = 0;

    public GamePhase currentPhase;
    public CardLocation selectLocation;
    public int selectionLimit;
    public int selectionMin;
    public int gainCostLimit;
    public CardLocation gainDestination;




    public CardData copperData;
    public CardData estateData;
    public CardData silverData;
    public CardData goldData;
    public CardData newCardData;

    public List<CardData> deck = new List<CardData>();
    public List<CardData> hand = new List<CardData>();
    public List<CardData> choices = new List<CardData>();
    public List<CardView> selectedCards = new List<CardView>();
    public List<CardData> discardPile = new List<CardData>();
    public List<CardData> trash = new List<CardData>();
    public List<CardData> aside = new List<CardData>();

    public SupplyManager supplyManager;

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
        };
            
        deck = GetShuffledDeck(deck);
        currentDeckVisual = Instantiate(cardBackPrefab, deckPosition);
    }

    IEnumerator SetupHand()
    {
        yield return StartCoroutine(DrawCards(5));
        PhaseChange(GamePhase.Action);
    }

    public IEnumerator DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CardData drawCard = null;
            yield return StartCoroutine(RevealTopCard((cardData) => {
                drawCard = cardData;
            }));
            if (drawCard != null)
            {
                hand.Add(drawCard);
                CreateCardView(drawCard, handParent, CardLocation.Hand);
            }
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
        DrawEffect();
        yield return new WaitForSeconds(0.35f);
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

    public IEnumerator DeckAdd(CardData data, Transform spawnParent)
    {
        float moveSeconds = 0.4f;
        CardView additionalCard = CreateCardView(data, spawnParent, CardLocation.Deck);
        CardMove(additionalCard, moveSeconds, CardLocation.Deck);
        yield return new WaitForSeconds(moveSeconds);
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

    public IEnumerator PlayCard(CardView cardView)
    {
        if (actions <= 0)
        {
            yield break;
        }
        else AddAction(-1);
        hand.Remove(cardView.data);
        CardMove(cardView, 0.3f, CardLocation.PlayArea);
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(cardView.data.ExecuteEffect(this));
        if (currentPhase == GamePhase.Action && actions <= 0)
        {
            NexPhase();
        }
    }

    public void PlayCardFromVassal(CardView cardView)
    {
        StartCoroutine(VassalPlaySequence(cardView));
    }
    
    private IEnumerator VassalPlaySequence(CardView cardView)
    {
        CardMove(cardView, 0.3f, CardLocation.PlayArea);
        yield return StartCoroutine(cardView.data.ExecuteEffect(this));
        PhaseChange(GamePhase.Action);
    }
    public void BackFromVassal(CardView cardView)
    {
        CardMove(cardView, 0.2f, CardLocation.Discard);
        PhaseChange(GamePhase.Action);
    }

    public void ToggleCardSelection(CardView card)
    {
        if (selectedCards.Count >= selectionLimit) return;
        choices.Remove(card.data);
        selectedCards.Add(card);
        card.location = CardLocation.Select;
        card.transform.SetParent(selectedArea, false);
        if (selectedCards.Count >= selectionMin)
        {
           UIManager.Instance.ShowSelectPanel(true);
        }
    }

    public void ReturnCard(CardView card)
    {
        selectedCards.Remove(card);
        choices.Add(card.data);
        card.transform.SetParent(choicesArea, false);
        card.location = selectLocation;
        if (selectedCards.Count < selectionMin)
        {
            UIManager.Instance.ShowSelectPanel(false);
        }
    }

    public void StartSelection(int minCount, int maxCount, CardLocation location, System.Predicate<CardData> condition, System.Action<List<CardView>> callback)
    {
        PhaseChange(GamePhase.isSelecting);
        selectLocation = location;
        onSelectionComplete = callback;
        selectionLimit = maxCount;
        selectionMin = minCount;
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
                child.rotation = Quaternion.identity;
                child.transform.SetParent(choicesArea, false);
            }
        }
        UIManager.Instance.ShowSelectPanel(selectionMin == 0);
    }

    public void FinishSelection()
    {
        PhaseChange(GamePhase.Action);
        Transform targetArea = locationMap[selectLocation].Transform;
        for (int i = choicesArea.childCount - 1; i >= 0; i--)
        {
            RectTransform child = choicesArea.GetChild(i) as RectTransform;
            child.SetParent(targetArea, false);
        }
        
        onSelectionComplete?.Invoke(selectedCards);
        locationMap[selectLocation].DataList.AddRange(choices);
        choices.Clear();
        selectedCards.Clear();
    }

    public void BuyCard(CardData data)
    {
        if (supplyManager.supplyCounts[data] <= 0)
        {
            return;
        }
        AddBuy(-1);
        GainCoin(-data.cost);
        CardView view = CreateCardView(data, effectLayer, CardLocation.Discard);
        CardMove(view, 0.3f, CardLocation.Discard);
        totalDeckCount++;
        supplyManager.DecreaseCount(data);
        AddScore(data, true);
        if (buys <= 0)
        {
            NexPhase();
        }
    }

    public void GainCard(CardData data)
    {
        if (supplyManager.supplyCounts[data] <= 0)
        {
            return;
        }
        if (data.cost > gainCostLimit) {
            Debug.Log($"{data.cardName}は獲得できません！");
            return;
        }
        CardView view = CreateCardView(data, effectLayer, gainDestination);
        CardMove(view, 0.2f, gainDestination);
        AddScore(data, true);
        PhaseChange(GamePhase.Action);
        totalDeckCount++;
        supplyManager.DecreaseCount(data);
    }

    public void AddAction(int extraAction)
    {
        actions += extraAction;
        actionText.text = actions.ToString();
    }

    public void GainCoin(int extraCoin)
    {
        currentCoins += extraCoin;
        coinText.text = currentCoins.ToString();
    }

    public void AddBuy(int extraBuy)
    {
        buys += extraBuy;
        buyText.text = buys.ToString();
    }



    public void CardMove(CardView view, float moveSeconds, CardLocation destination)
    {
        view.transform.SetParent(canvas, true);
        view.location = destination;
        view.transform.localRotation = Quaternion.identity;
        switch (destination)
        {
            case CardLocation.Discard:
                view.GetComponent<CardMovement>().MoveTo(discardPosition, moveSeconds, false);
                discardPile.Add(view.data);
                break;
            case CardLocation.Hand:
                view.GetComponent<CardMovement>().MoveTo(handParent, moveSeconds, false);
                hand.Add(view.data);
                break;
            case CardLocation.Deck:
                view.GetComponent<CardMovement>().MoveTo(deckPosition, moveSeconds, true);
                if(deck.Count <= 0)
                {
                    StartCoroutine(CreateDeckVisual(moveSeconds));
                }
                deck.Add(view.data);
                break;
            case CardLocation.Trash:
                totalDeckCount--;
                trash.Add(view.data);
                view.GetComponent<CardMovement>().MoveTo(trashArea, moveSeconds, false);
                break;
            case CardLocation.PlayArea:
                view.GetComponent<CardMovement>().MoveTo(playArea, moveSeconds, false);
                break;
            case CardLocation.Aside:
                view.GetComponent<CardMovement>().MoveTo(asideArea, moveSeconds, false);
                break;
        }
    }

    IEnumerator CreateDeckVisual(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        currentDeckVisual = Instantiate(cardBackPrefab, deckPosition);
    }
    

    public void AddScore(CardData data, bool isGained)
    {
        if (data.cardtype == CardType.Curse || data.cardtype == CardType.Victory)
        {
            if (isGained) totalscore += GetVictoryPoints(data);
            else totalscore -= GetVictoryPoints(data);
        }
        scoreText.text = totalscore.ToString();
    }

    public void StartGain(int maxCost, System.Predicate<CardData> condition, CardLocation destination)
    {
        PhaseChange(GamePhase.isGaining);
        gainCostLimit = maxCost;
        gainDestination = destination;
    }

    public void PhaseChange(GamePhase phase)
    {
        currentPhase = phase;
        switch (phase)
        {
            case GamePhase.Action:
                message.text = "アクションカードをプレイ";
                break;
            case GamePhase.Buy:
                message.text = "カードを購入";
                break;
            case GamePhase.Cleanup:
                message.text = "次ターンの準備中";
                break;
            case GamePhase.isSelecting:
                message.text = "カードを選択してください";
                break;
            case GamePhase.isGaining:
                message.text = "カードを獲得してください";
                break;
            case GamePhase.ActionPlaying:
                message.text = "カード効果発動中";
                break;
        }
    }

    public void NexPhase()
    {
        switch (currentPhase)
        {
            case GamePhase.Action:
                int i = 0;
                while (i < handParent.childCount)
                {
                    CardView view = handParent.GetChild(i).GetComponent<CardView>();
                    if (view.data.cardtype == CardType.Treasure)
                    {
                        StartCoroutine(view.data.ExecuteEffect(this));
                        CardMove(view, 0.3f, CardLocation.PlayArea);
                    }
                    else i++;
                }
                coinText.text = currentCoins.ToString();
                PhaseChange(GamePhase.Buy);
                break;
            case GamePhase.Buy:
                StartCoroutine(CleanupPhase());
                break;
        }
    }

    private IEnumerator CleanupPhase()
    {
        yield return new WaitForSeconds(0.2f);
        PhaseChange(GamePhase.Cleanup);

        for (int i = playArea.childCount - 1; i >= 0; i--)
        {
            CardView view = playArea.GetChild(0).GetComponent<CardView>();
            CardMove(view, 0.2f, CardLocation.Discard);
        }
        for (int i = handParent.childCount - 1; i >= 0; i--)
        {
            CardView view = handParent.GetChild(0).GetComponent<CardView>();
            CardMove(view, 0.2f, CardLocation.Discard);
        }
        hand.Clear();

        yield return new WaitForSeconds(0.5f);

        ResetCondition();

        yield return StartCoroutine(DrawCards(5));

        PhaseChange(GamePhase.Action);
    }

    void ResetCondition()
    {
        totalscore = calculatedScore();
        scoreText.text = totalscore.ToString();
        actions = 1;
        actionText.text = actions.ToString();
        buys = 1;
        buyText.text = buys.ToString();
        currentCoins = 0;
        coinText.text = currentCoins.ToString();
        merchant = 0;
        silver = false;
    }

    int calculatedScore()
    {
        int temScore = 0;
        foreach (CardData data in discardPile)
        {
            if (data.cardtype == CardType.Curse || data.cardtype == CardType.Victory)
            {
                temScore += GetVictoryPoints(data);
            }
        }
        foreach (CardData data in deck)
        {
            if (data.cardtype == CardType.Curse || data.cardtype == CardType.Victory)
            {
                temScore += GetVictoryPoints(data);
            }
        }
        return temScore;
    }

    public int GetVictoryPoints(CardData data)
    {
        if (data.cardtype == CardType.Curse)
        {
            return -1;
        }
        switch(data.cardName)
        {
            case "Estate":
                return 1;
            case "Duchy":
                return 3;
            case "Province":
                return 6;
            case "Gardens":
                return totalDeckCount / 10;
            default:
                return 0;
        }
    }

    void Start()
    {
        supplyManager = Object.FindFirstObjectByType<SupplyManager>();
        SetupInitialDeck();
        totalDeckCount = deck.Count;
        PhaseChange(GamePhase.Cleanup);
        ResetCondition();
        StartCoroutine(SetupHand());
    }
}
