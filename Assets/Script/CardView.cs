using UnityEngine;
using UnityEngine.UI;

public enum CardLocation
{
    Deck,
    Hand,
    PlayArea,
    Choices,
    Select,
    Supply,     
    Discard,
    Trash,
    Aside
}

public enum CardContext
{
    None,
    Hand,
    Supply,
    Selection,
    Return,
    Vassal,
    Library,
    Gain
}

public class CardView : MonoBehaviour
{
    public CardData data;
    public Image cardImageDisplay;
    public CardLocation location;
    private Button button;
    public CardContext currentContext;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClickCard);
    }

    public void SetCard(CardData newData, CardLocation newLocation)
    {
        this.data = newData;
        this.location = newLocation;
        this.cardImageDisplay.sprite = this.data.cardImage;
    }
 
    public void OnClickCard()
    {
        currentContext = CardContext.None;
        if (GameManager.Instance.currentPhase == GamePhase.Action && this.location == CardLocation.Hand && this.data.cardtype == CardType.Action)
        {
            currentContext = CardContext.Hand;
        }
        else if (GameManager.Instance.currentPhase == GamePhase.isSelecting)
        {
            if (this.location == GameManager.Instance.selectLocation && GameManager.Instance.selectedCards.Count < GameManager.Instance.selectionLimit)
            {
                currentContext = CardContext.Selection;
            }
            else if (this.location == CardLocation.Select)
            {
                currentContext = CardContext.Return;
            }
        }
        UIManager.Instance.ShowCardDetail(this, currentContext);
    }

    // public void SetSelectedVisual(bool isSelected)
    // {
    //     // 方法A: 少し上に浮かせる（Y座標を+30する）
    //     // 方法B: 画像の色を少し暗く、または青っぽくする
    //     if (isSelected)
    //     {
    //         transform.localPosition += new Vector3(0, 50, 0); 
    //         GetComponent<Image>().color = Color.cyan; // 水色にする
    //     }
    //     else
    //     {
    //         transform.localPosition -= new Vector3(0, 50, 0);
    //         GetComponent<Image>().color = Color.white; // 元に戻す
    //     }
    // }

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
