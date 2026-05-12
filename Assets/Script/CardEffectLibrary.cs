using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardEffectLibrary", menuName = "Dominion/Effect")]
public class CardEffectLibrary : ScriptableObject
{
    public static IEnumerator Copper(GameManager gm)
    {
        gm.currentCoins++;
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
        gm.AddAction(1);
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
                gm.CardMove(card, 0.3f, CardLocation.Trash);
                if (card.data.cardtype == CardType.Curse || card.data.cardtype == CardType.Victory)
                {
                    gm.totalscore -= gm.GetVictoryPoints(card.data);
                }
            }
            gm.scoreText.text = gm.totalscore.ToString();
        });
        while (gm.currentPhase == GamePhase.isSelecting) yield return null;
        yield return new WaitForSeconds(0.3f);
    }

    public static IEnumerator Vassal(GameManager gm)
    {
        gm.GainCoin(2);
        gm.PhaseChange(GamePhase.ActionPlaying);
        yield return null;
        yield return gm.StartCoroutine(gm.RevealTopCard((cardData) => 
        {
            CardView view = gm.CreateCardView(cardData, gm.effectLayer, CardLocation.Select);
            UIManager.Instance.ShowCardDetail(view, CardContext.Vassal);
        }));
        while (gm.currentPhase == GamePhase.ActionPlaying) yield return null;
    }

    public static IEnumerator Workshop(GameManager gm)
    {
        gm.StartGain(4, null, CardLocation.Discard);
        while (gm.currentPhase == GamePhase.isGaining) yield return null;
    }

    public static IEnumerator Merchant(GameManager gm)
    {
        gm.AddAction(1);
        gm.merchant += 1;
        yield return gm.StartCoroutine(gm.DrawCards(1));
    }

    public static IEnumerator Harbinger(GameManager gm)
    {
        gm.AddAction(1);
        yield return gm.StartCoroutine(gm.DrawCards(1));
        yield return gm.StartCoroutine(HarbingerSequence(gm));
    }

    private static IEnumerator HarbingerSequence(GameManager gm)
    {
        gm.StartSelection(0, 1, CardLocation.Discard, null, (selectedList) => 
        {
            foreach (var cardView in selectedList)
            {
                gm.StartCoroutine(gm.DeckAdd(cardView.data, cardView.transform));
                Destroy(cardView.gameObject);
            }
        });

        while (gm.currentPhase == GamePhase.isSelecting)  yield return null;
    }

    public static IEnumerator Village(GameManager gm)
    {
        gm.AddAction(2);
        yield return gm.StartCoroutine(gm.DrawCards(1));
    }

    public static IEnumerator Remodel(GameManager gm)
    {
        if (gm.hand.Count == 0) yield break;
        gm.StartSelection(1, 1, CardLocation.Hand, null, (selectedList) => 
        {
            CardView card = selectedList[0];
            int cardCost = card.data.cost;
            if (card.data.cardtype == CardType.Curse || card.data.cardtype == CardType.Victory)
            {
                gm.AddScore(card.data, false);
            }
            gm.CardMove(card, 0.3f, CardLocation.Trash);
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
                gm.CardMove(copper, 0.3f, CardLocation.Trash);
                gm.GainCoin(3);
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
        yield return gm.StartCoroutine(targetCard.data.ExecuteEffect(gm));
        yield return gm.StartCoroutine(targetCard.data.ExecuteEffect(gm));
        onComplete?.Invoke();
    }

    public static IEnumerator Poacher(GameManager gm)
    {
        gm.AddAction(1);
        gm.GainCoin(1);
        yield return(gm.DrawCards(1));
        int emptySupplyCount = 0;
        foreach (var count in gm.supplyManager.supplyCounts.Values)
        {
            if (count <= 0) emptySupplyCount++;
        }
        int discardCount = Mathf.Min(emptySupplyCount, gm.hand.Count);
        if (discardCount == 0) yield break;
        bool isSelectionFinished = false;

        gm.StartSelection(discardCount, discardCount, CardLocation.Hand, null, (selectedList) => 
        {
            foreach (var card in selectedList)
            {
                gm.discardPile.Add(card.data);
                card.location = CardLocation.Discard;
                card.transform.SetParent(gm.canvas, true);
                card.GetComponent<CardMovement>().MoveTo(gm.discardPosition, 0.3f, false);
            }
            isSelectionFinished = true;
        });
        while (!isSelectionFinished)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.35f);
    }


    public static IEnumerator Militia(GameManager gm)
    {
        gm.GainCoin(2);
        yield break;
    }

    public static IEnumerator Bureaucrat(GameManager gm)
    {
        if (gm.supplyManager.supplyCounts[gm.silverData] <= 0) yield break;
        Transform startPos = gm.supplyManager.supplyViews[gm.silverData].transform;
        gm.totalDeckCount++;
        gm.supplyManager.DecreaseCount(gm.silverData);
        yield return gm.StartCoroutine(gm.DeckAdd(gm.silverData, startPos));
    }

    public static IEnumerator Market(GameManager gm)
    {
        gm.AddAction(1);
        gm.AddBuy(1);
        gm.GainCoin(1);
        yield return gm.StartCoroutine(gm.DrawCards(1));
    }

    public static IEnumerator Stentry(GameManager gm)
    {
        gm.PhaseChange(GamePhase.ActionPlaying);
        gm.AddAction(1);
        yield return gm.StartCoroutine(gm.DrawCards(1));
        List<CardData> drawList = new List<CardData>();
        yield return gm.StartCoroutine(gm.RevealTopCard((cardData) => {
                drawList.Add(cardData);
        }));
        yield return gm.StartCoroutine(gm.RevealTopCard((cardData) => {
                drawList.Add(cardData);
        }));
        UIManager.Instance.StentrySelect(drawList);
        while (gm.currentPhase == GamePhase.ActionPlaying)
        {
            yield return null;
        }
    }

    public static IEnumerator CouncilRoom(GameManager gm)
    {
        gm.AddBuy(1);
        yield return gm.StartCoroutine(gm.DrawCards(4));
    }

    public static IEnumerator Laboratory(GameManager gm)
    {
        gm.AddAction(1);
        yield return gm.StartCoroutine(gm.DrawCards(2));
    }

    public static IEnumerator Mine(GameManager gm)
    {
        if (gm.hand.Count == 0) yield break;
        gm.StartSelection(0, 1, CardLocation.Hand, data => data.cardtype == CardType.Treasure, (selectedList) => 
        {
            CardView card = selectedList[0];
            int cardCost = card.data.cost;
            gm.CardMove(card, 0.3f, CardLocation.Trash);
            gm.StartGain(cardCost + 3, data => data.cardtype == CardType.Treasure, CardLocation.Hand);
        });
        while (gm.currentPhase == GamePhase.isSelecting) yield return null;
        yield return new WaitForEndOfFrame();
        while (gm.currentPhase == GamePhase.isGaining) yield return null;
        yield return new WaitForSeconds(0.3f);
    }

    public static IEnumerator Bandit(GameManager gm)
    {
        if (gm.supplyManager.supplyCounts[gm.goldData] <= 0) yield break;
        Transform startPos = gm.supplyManager.supplyViews[gm.goldData].transform;
        CardView goldCard = gm.CreateCardView(gm.goldData, startPos, CardLocation.Supply);
        gm.totalDeckCount++;
        gm.supplyManager.DecreaseCount(gm.goldData);
        gm.CardMove(goldCard, 0.3f, CardLocation.Discard);
        yield return new WaitForSeconds(0.3f);
    }

    public static IEnumerator Festival(GameManager gm)
    {
        gm.AddAction(2);
        gm.AddBuy(1);
        gm.GainCoin(2);
        yield break;
    }

    public static IEnumerator Library(GameManager gm)
    {
        gm.PhaseChange(GamePhase.ActionPlaying);
        while(gm.hand.Count < 7 && gm.deck.Count + gm.discardPile.Count > 0)
        {
            CardData revealedData = null;
            yield return gm.StartCoroutine(gm.RevealTopCard((cardData) => 
            {
                revealedData = cardData;
            }));
            CardView view = gm.CreateCardView(revealedData, gm.effectLayer, CardLocation.Select);
            UIManager.Instance.ShowCardDetail(view, CardContext.Library);
            while (UIManager.Instance.zoomedCardParent.childCount > 0){
                yield return null;
            }
        }
        for (int i = gm.asideArea.childCount - 1; i >= 0; i--)
        {
            CardView view = gm.asideArea.GetChild(0).GetComponent<CardView>();
            gm.CardMove(view, 0.2f, CardLocation.Discard);
        }
        gm.PhaseChange(GamePhase.Action);
        while(gm.currentPhase == GamePhase.ActionPlaying)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
    }

    public static IEnumerator Witch(GameManager gm)
    {
        yield return gm.StartCoroutine(gm.DrawCards(2));
    }

    public static IEnumerator Artisan(GameManager gm)
    {
        gm.StartGain(5, null, CardLocation.Hand);
        while (gm.currentPhase == GamePhase.isGaining) yield return null;
        yield return new WaitForSeconds(0.3f);
        gm.StartSelection(1, 1, CardLocation.Hand, null, (selectedList) => 
        {
            CardView card = selectedList[0];
            gm.CardMove(card, 0.3f, CardLocation.Deck);
        });
        while (gm.currentPhase == GamePhase.isSelecting) yield return null;
        yield return new WaitForSeconds(0.3f);
    }
}