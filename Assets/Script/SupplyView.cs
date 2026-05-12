using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SupplyView : MonoBehaviour
{
    public CardData data;
    public Image supplyImageDisplay;
    private Button button;
    public CardContext currentContext;

    [SerializeField] private TextMeshProUGUI countText;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClickSupply);
    }

    public void SetSupply(CardData newData, int count)
    {
        this.data = newData;
        this.supplyImageDisplay.sprite = this.data.supplyImage;
        UpdateCount(count);
    }
 
    public void OnClickSupply()
    {
        currentContext = CardContext.None;
        if (GameManager.Instance.currentPhase == GamePhase.Buy && GameManager.Instance.currentCoins >= this.data.cost)
        {
            currentContext = CardContext.Supply;
        }
        if (GameManager.Instance.currentPhase == GamePhase.isGaining && GameManager.Instance.gainCostLimit >= this.data.cost)
        {
            currentContext = CardContext.Gain;
        }
        UIManager.Instance.ShowSupplyDetail(data, currentContext);
    }

    public void UpdateCount(int count)
    {
        countText.text = count.ToString();
    }

    public void SetSupplyVisual(bool isSale)
    {
        if (isSale)
        {
            GetComponent<Image>().color = Color.white;
        }
        else
        {
            GetComponent<Image>().color = Color.gray;
        }
    }
}
