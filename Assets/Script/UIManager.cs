using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set;}

    [Header("Zoom UI Elements")]
    [SerializeField] private GameObject cardZoomPanel;
    public Transform zoomedCardParent;
    public Transform effectLayer;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button gainButton;
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button trashButton;
    [SerializeField] private Button deckButton;
    [SerializeField] private Button handButton;
    [SerializeField] private Button asideButton;

    private CardView selectedStentryCard;
    private List<CardData> stentryData = new List<CardData>();

    private GameObject currentZoomedCard;
    private CardView currentEffectCard;

    void Awake()
    {
        Instance = this;
        HideCardDetail();
        selectionPanel.SetActive(false);
    }

    public void ShowCardDetail(CardView originalCard, CardContext context)
    {
        cardZoomPanel.SetActive(true);
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(HideCardDetail);

        switch (context)
        {
            case CardContext.Hand:
                playButton.gameObject.SetActive(true);
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(() => {
                    StartCoroutine(GameManager.Instance.PlayCard(originalCard));
                    HideCardDetail();
                });
                break;

            case CardContext.Supply:
                buyButton.gameObject.SetActive(true);
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => {
                    GameManager.Instance.BuyCard(originalCard.data);
                    HideCardDetail();
                });
                break;

            case CardContext.Selection:
                selectButton.gameObject.SetActive(true);
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() =>
                {
                    GameManager.Instance.ToggleCardSelection(originalCard);
                    HideCardDetail();
                });
                break;

            case CardContext.Return:
                returnButton.gameObject.SetActive(true);
                returnButton.onClick.RemoveAllListeners();
                returnButton.onClick.AddListener(() =>
                {
                    GameManager.Instance.ReturnCard(originalCard);
                    HideCardDetail();
                });
                break;
            case CardContext.Vassal:
                closeButton.onClick.RemoveAllListeners(); 
                closeButton.onClick.AddListener(() => {
                    GameManager.Instance.BackFromVassal(originalCard); 
                    HideCardDetail();                                 
                });
                if (originalCard.data.cardtype == CardType.Action)
                {
                    playButton.gameObject.SetActive(true);
                    playButton.onClick.RemoveAllListeners();
                    playButton.onClick.AddListener(() => {
                        GameManager.Instance.PlayCardFromVassal(originalCard);
                        HideCardDetail();
                    });
                }
                break;
            case CardContext.Library:
                closeButton.enabled = false;
                handButton.gameObject.SetActive(true);
                handButton.onClick.RemoveAllListeners();
                handButton.onClick.AddListener(() => {
                    GameManager.Instance.CardMove(originalCard, 0.25f, CardLocation.Hand);
                    HideCardDetail();
                });
                if (originalCard.data.cardtype == CardType.Action)
                {
                    asideButton.gameObject.SetActive(true);
                    asideButton.onClick.RemoveAllListeners();
                    asideButton.onClick.AddListener(() => {
                        GameManager.Instance.CardMove(originalCard, 0.25f, CardLocation.Aside);
                        HideCardDetail();
                    });
                }
                break;
            case CardContext.Gain:
                gainButton.gameObject.SetActive(true);
                gainButton.onClick.RemoveAllListeners();
                gainButton.onClick.AddListener(() => {
                    GameManager.Instance.GainCard(originalCard.data);
                    HideCardDetail();
                });
                break;
            case CardContext.None:
            default:
                break;
        }

        currentZoomedCard = Instantiate(originalCard.gameObject, zoomedCardParent);
        if (currentZoomedCard.TryGetComponent<CardView>(out var view))
        {
            view.enabled = false;
        }
        RectTransform zoomedRT = currentZoomedCard.GetComponent<RectTransform>();
        zoomedRT.sizeDelta = new Vector2(354f, 546f);
        zoomedRT.anchoredPosition = Vector2.zero;
        zoomedRT.rotation = Quaternion.identity;
    }

    public void ShowSupplyDetail(CardData data, CardContext context)
    {
        currentEffectCard = GameManager.Instance.CreateCardView(data, effectLayer, CardLocation.Supply);
        ShowCardDetail(currentEffectCard, context);
    }

    public void HideCardDetail()
    {
        if (currentZoomedCard != null) Destroy(currentZoomedCard);
        if (currentEffectCard != null) Destroy(currentEffectCard.gameObject);
        cardZoomPanel.SetActive(false);
        playButton.gameObject.SetActive(false);
        buyButton.gameObject.SetActive(false);
        selectButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
        gainButton.gameObject.SetActive(false);
        discardButton.gameObject.SetActive(false);
        trashButton.gameObject.SetActive(false);
        deckButton.gameObject.SetActive(false);
        asideButton.gameObject.SetActive(false);
        handButton.gameObject.SetActive(false);
    }

    public void ShowSelectPanel(bool show)
    {
        selectionPanel.SetActive(true);
        if (show)
        {
            completeButton.gameObject.SetActive(true);
            completeButton.onClick.RemoveAllListeners();
            completeButton.onClick.AddListener(() =>
            {
                GameManager.Instance.FinishSelection();
                HideSelectPanel();
            });
        }
        else
        {
            completeButton.gameObject.SetActive(false);
        }
    }

    public void HideSelectPanel()
    {
        selectionPanel.SetActive(false);
    }

    public void StentrySelect(List<CardData> cardList)
    {
        stentryData = cardList;
        cardZoomPanel.SetActive(true);
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => {
            SetStentryButtonsActive(false);
        });
        int listSize = cardList.Count;
        for (int i = 0; i < listSize; i++)
        {
            CardView newCard = GameManager.Instance.CreateCardView(cardList[i], zoomedCardParent, CardLocation.Select);
            Button btn = newCard.GetComponent<Button>();
            newCard.enabled = false;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                selectedStentryCard = newCard;
                SetStentryButtonsActive(true);
            });
            RectTransform zoomedRT = newCard.GetComponent<RectTransform>();
            zoomedRT.sizeDelta = new Vector2(354f, 546f);
        }
    }

    private void SetStentryButtonsActive(bool isActive)
    {
        discardButton.gameObject.SetActive(isActive);
        trashButton.gameObject.SetActive(isActive);
        deckButton.gameObject.SetActive(isActive);
        if (isActive) {
            discardButton.onClick.RemoveAllListeners();
            discardButton.onClick.AddListener(() => {
                StartCoroutine(StentryEffect(CardLocation.Discard));
            });
            trashButton.onClick.RemoveAllListeners();
            trashButton.onClick.AddListener(() => {
                StartCoroutine(StentryEffect(CardLocation.Trash));
            });
            deckButton.onClick.RemoveAllListeners();
            deckButton.onClick.AddListener(() => {
                StartCoroutine(StentryEffect(CardLocation.Deck));
            });
        }
    }

    IEnumerator StentryEffect(CardLocation destination)
    {
        cardZoomPanel.SetActive(false);
        CardView drawCard = GameManager.Instance.CreateCardView(selectedStentryCard.data, selectedStentryCard.transform, CardLocation.Select);
        GameManager.Instance.CardMove(drawCard, 0.3f, destination);
        yield return new WaitForSeconds(0.35f);
        Destroy(selectedStentryCard.gameObject);
        if (zoomedCardParent.childCount == 2)
        {
            SetStentryButtonsActive(false);
            cardZoomPanel.SetActive(true);
        }
        else
        {
            HideCardDetail();
            GameManager.Instance.currentPhase = GamePhase.Action;
        }
    }
}
