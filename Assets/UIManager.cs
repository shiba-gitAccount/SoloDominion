using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set;}

    [Header("Zoom UI Elements")]
    [SerializeField] private GameObject cardZoomPanel;
    [SerializeField] private Transform zoomedCardParent;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button gainButton;
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Button completeButton;

    private GameObject currentZoomedCard;

    void Awake()
    {
        Instance = this;
        closeButton.onClick.AddListener(HideCardDetail);
        cardZoomPanel.SetActive(false);
        selectionPanel.SetActive(false);
    }

    public void ShowCardDetail(CardView originalCard, CardContext context)
    {
        cardZoomPanel.SetActive(true);

        buyButton.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);
        selectButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
        gainButton.gameObject.SetActive(false);

        switch (context)
        {
            case CardContext.Hand:
                playButton.gameObject.SetActive(true);
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(() => {
                    GameManager.Instance.PlayCard(originalCard);
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
                playButton.gameObject.SetActive(true);
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(() => {
                    GameManager.Instance.PlayCardFromVassal(originalCard);
                    HideCardDetail();
                });
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
        zoomedRT.sizeDelta = new Vector2(531f, 819f);
        zoomedRT.anchoredPosition = Vector2.zero;
    }

    public void HideCardDetail()
    {
        if (currentZoomedCard != null) Destroy(currentZoomedCard);
        cardZoomPanel.SetActive(false);
    }

    public void ShowSelectPanel()
    {
        selectionPanel.SetActive(true);
        completeButton.gameObject.SetActive(true);
        completeButton.onClick.RemoveAllListeners();
        completeButton.onClick.AddListener(() =>
        {
            GameManager.Instance.FinishSelection();
            HideSelectPanel();
        });
    }

    public void HideSelectPanel()
    {
        selectionPanel.SetActive(false);
    }
}
