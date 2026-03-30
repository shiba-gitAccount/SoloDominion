using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "CardEffectLibrary", menuName = "Dominion/Effect")]
public class CardEffectLibrary : ScriptableObject
{
    public static void Copper(GameManager gm)
    {
        gm.currentCoins += 1;
    }

    public static void Silver(GameManager gm)
    {
        gm.currentCoins += 2;
        if (!gm.silver)
        {
            gm.silver = true;
            gm.currentCoins += gm.merchant;
        }
    }

    public static void Gold(GameManager gm)
    {
        gm.currentCoins += 3;
    }

    public void Cellar(GameManager gm)
    {
        gm.actions += 1;
        gm.StartSelection(99, CardLocation.Hand, null, (list) => {
            int count = list.Count;
            foreach (var card in list)
            {
                gm.discardPile.Add(card.data);
                card.location = CardLocation.Discard;
                card.transform.SetParent(gm.discardPosition, true);
                card.GetComponent<CardMovement>().MoveTo(gm.discardPosition, 0.3f, false);
            }
            gm.StartCoroutine(gm.DrawCards(count));
            Debug.Log($"{count}枚捨てて、{count}枚引きました。");
        });
    }

    public static void Moat(GameManager gm)
    {
        gm.StartCoroutine(gm.DrawCards(2));
    }

    public void Chapel(GameManager gm)
    {
        gm.StartSelection(4, CardLocation.Hand, null, (selectedList) => 
        {
            foreach (var card in selectedList)
            {
                card.location = CardLocation.Trash;
                card.transform.SetParent(gm.canvas, true);
                card.GetComponent<CardMovement>().MoveTo(gm.trashArea, 0.3f, false);
            }
        });
    }

    public static void Vassal(GameManager gm)
    {
        gm.currentCoins += 2;
        gm.StartCoroutine(ExecuteVassalAfterDelay(gm));
    }

    private static IEnumerator ExecuteVassalAfterDelay(GameManager gm)
    {
        yield return null; 

        yield return gm.StartCoroutine(gm.RevealTopCard((cardData) => 
        {
            CardView view = gm.CreateCardView(cardData, gm.deckPosition, CardLocation.Discard);
            view.GetComponent<CardMovement>().MoveTo(gm.discardPosition, 0.3f, false);
            CardContext context = (cardData.cardtype == CardType.Action) ? CardContext.Vassal : CardContext.None;
            UIManager.Instance.ShowCardDetail(view, context);
            if (cardData.cardtype != CardType.Action)
            {
                gm.discardPile.Add(cardData);
            }
        }));
    }

    public static void Workshop(GameManager gm)
    {
        gm.StartGain(4, null, CardLocation.Discard);
    }

    public static void Merchant(GameManager gm)
    {
        gm.actions += 1;
        gm.merchant += 1;
        gm.StartCoroutine(gm.DrawCards(1));
    }

    public static void Village(GameManager gm)
    {
        gm.actions += 2;
        gm.StartCoroutine(gm.DrawCards(1));
    }
    
    public static void Smithy(GameManager gm)
    {
        gm.StartCoroutine(gm.DrawCards(3));
    }
}