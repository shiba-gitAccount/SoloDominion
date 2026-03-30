using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardEffectLibrary", menuName = "Dominion/Effect")]
public class CardEffectLibrary : ScriptableObject
{
    public static IEnumerator Copper(GameManager gm)
    {
        gm.currentCoins += 1;
        yield break;
    }

    public static IEnumerator Silver(GameManager gm)
    {
        gm.currentCoins += 2;
        if (!gm.silver)
        {
            gm.silver = true;
            gm.currentCoins += gm.merchant;
        }
        yield break;
    }

    public static IEnumerator Gold(GameManager gm)
    {
        gm.currentCoins += 3;
        yield break;
    }

    public static IEnumerator Cellar(GameManager gm)
    {
        gm.actions += 1;
        bool isEffectFinished = false;

        gm.StartSelection(0, 99, CardLocation.Hand, null, (list) => 
        {
            gm.StartCoroutine(ExecuteCellarProcess(gm, list, () => isEffectFinished = true));
        });

        while (!isEffectFinished) yield return null;
    }

    private static IEnumerator ExecuteCellarProcess(GameManager gm, List<CardView> selectedList, System.Action onComplete)
    {
        int count = selectedList.Count;

        foreach (var card in selectedList)
        {
            gm.discardPile.Add(card.data);
            gm.hand.Remove(card.data);
            card.location = CardLocation.Discard;
            card.transform.SetParent(gm.canvas, true);
            card.GetComponent<CardMovement>().MoveTo(gm.discardPosition, 0.3f, false);
        }

        if (count > 0)
        {
            yield return new WaitForSeconds(0.35f);
            yield return gm.StartCoroutine(gm.DrawCards(count));
        }

        onComplete?.Invoke();
    }

    public static IEnumerator Moat(GameManager gm)
    {
        yield return gm.StartCoroutine(gm.DrawCards(2));
    }

    public static IEnumerator Chapel(GameManager gm)
    {
        gm.StartSelection(0, 4, CardLocation.Hand, null, (selectedList) => 
        {
            foreach (var card in selectedList)
            {
                gm.hand.Remove(card.data);
                gm.trash.Add(card.data);
                card.location = CardLocation.Trash;
                card.transform.SetParent(gm.canvas, true);
                card.GetComponent<CardMovement>().MoveTo(gm.trashArea, 0.3f, false);
            }
        });
        while (gm.currentPhase == GamePhase.isSelecting) yield return null;
        yield return new WaitForSeconds(0.3f);
    }

    public static IEnumerator Vassal(GameManager gm)
    {
        gm.currentCoins += 2;
        gm.isVassalEffectProcessing = true;
        yield return null;
        yield return gm.StartCoroutine(gm.RevealTopCard((cardData) => 
        {
            CardView view = gm.CreateCardView(cardData, gm.deckPosition, CardLocation.Discard);
            view.GetComponent<CardMovement>().MoveTo(gm.discardPosition, 0.3f, false);
            CardContext context = CardContext.Vassal;
            UIManager.Instance.ShowCardDetail(view, context);
        }));
        while (gm.isVassalEffectProcessing) yield return null;
    }

    public static IEnumerator Workshop(GameManager gm)
    {
        gm.StartGain(4, null, CardLocation.Discard);
        while (gm.currentPhase == GamePhase.isGaining) yield return null;
    }

    public static IEnumerator Merchant(GameManager gm)
    {
        gm.actions += 1;
        gm.merchant += 1;
        yield return gm.StartCoroutine(gm.DrawCards(1));
    }

    public static IEnumerator Harbinger(GameManager gm)
    {
        gm.actions += 1;
        yield return gm.StartCoroutine(gm.DrawCards(1));
        yield return gm.StartCoroutine(HarbingerSequence(gm));
    }

    private static IEnumerator HarbingerSequence(GameManager gm)
    {
        
        gm.StartSelection(0, 1, CardLocation.Discard, null, (selectedList) => 
        {
            foreach (var cardView in selectedList)
            {
                GameObject tempCardBack = Instantiate(gm.cardBackPrefab, cardView.transform);
                tempCardBack.transform.SetParent(gm.canvas, true);
                Destroy(cardView.gameObject);
                tempCardBack.GetComponent<CardMovement>().MoveTo(gm.deckPosition, 0.3f, gm.deck.Count != 0);
                gm.deck.Add(cardView.data);
                if (gm.deck.Count == 1)
                {
                    gm.currentDeckVisual = tempCardBack;
                }
            }
        });

        while (gm.currentPhase == GamePhase.isSelecting)  yield return null;
        yield return new WaitForSeconds(0.3f);
    }

    public static IEnumerator Village(GameManager gm)
    {
        gm.actions += 2;
        yield return gm.StartCoroutine(gm.DrawCards(1));
    }

    public static IEnumerator Remodel(GameManager gm)
    {
        if (gm.hand.Count == 0) yield break;
        gm.StartSelection(1, 1, CardLocation.Hand, null, (selectedList) => 
        {
            CardView card = selectedList[0];
            int cardCost = card.data.cost;
            card.location = CardLocation.Trash;
            card.transform.SetParent(gm.canvas, true);
            card.GetComponent<CardMovement>().MoveTo(gm.trashArea, 0.3f, false);
            gm.StartGain(cardCost + 2, null, CardLocation.Discard);
            
        });
        while (gm.currentPhase == GamePhase.isSelecting) yield return null;
        yield return new WaitForEndOfFrame();
        while (gm.currentPhase == GamePhase.isGaining) yield return null;
        yield return new WaitForSeconds(0.3f);
    }

    public static IEnumerator Smithy(GameManager gm)
    {
        yield return gm.StartCoroutine(gm.DrawCards(3));
    }

    public static IEnumerator Moneylender(GameManager gm)
    {
        gm.StartSelection(0, 1, CardLocation.Hand, data => data.cardName == "Copper", (selectedList) => 
        {
            if (selectedList.Count >= 1)
            {
                CardView copper = selectedList[0];
                copper.location = CardLocation.Trash;
                copper.transform.SetParent(gm.canvas, true);
                copper.GetComponent<CardMovement>().MoveTo(gm.trashArea, 0.3f, false);
                gm.currentCoins += 3;
            }
        });
        while (gm.currentPhase == GamePhase.isSelecting) yield return null;
    }

    public static IEnumerator ThroneRoom(GameManager gm)
    {
        bool isThroneProcessFinished = false;
        gm.StartSelection(0, 1, CardLocation.Hand, data => data.cardtype == CardType.Action, (selectedList) => 
        {
            if (selectedList.Count >= 1)
            {
                gm.StartCoroutine(ThroneRoomSequence(gm, selectedList[0], () => isThroneProcessFinished = true));
            }
            else
            {
                isThroneProcessFinished = true;
            }
        });
        while (!isThroneProcessFinished || gm.currentPhase == GamePhase.isSelecting)
        {
            yield return null;
        }
    }

    private static IEnumerator ThroneRoomSequence(GameManager gm, CardView targetCard, System.Action onComplete)
    {
        targetCard.location = CardLocation.PlayArea;
        targetCard.transform.SetParent(gm.canvas, true);
        targetCard.GetComponent<CardMovement>().MoveTo(gm.playArea, 0.3f, false);
        yield return new WaitForSeconds(0.3f); 
        Debug.Log($"{targetCard.data.cardName} の1回目を開始します"); // returnの前に出す
        yield return gm.StartCoroutine(targetCard.data.ExecuteEffect(gm));
        Debug.Log($"{targetCard.data.cardName} の1回目が完了しました");

        Debug.Log($"{targetCard.data.cardName} の2回目を開始します");
        yield return gm.StartCoroutine(targetCard.data.ExecuteEffect(gm));
        Debug.Log($"{targetCard.data.cardName} の2回目が完了しました");

        Debug.Log("玉座の間の全処理が完了しました。");

        onComplete?.Invoke();
    }
}