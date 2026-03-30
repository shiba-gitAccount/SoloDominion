using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[CreateAssetMenu(fileName = "NewCard", menuName = "Dominion/NewCard")]
public class CustomCard : CardData
{
    [Header("カードプレイ時の効果をここにドラッグ")]
    public UnityEvent<GameManager> onPlayEffect;

    public override IEnumerator ExecuteEffect(GameManager gm)
    {
        switch (this.cardName)
        {
            case "Copper":     yield return CardEffectLibrary.Copper(gm); break;
            case "Silver":     yield return CardEffectLibrary.Silver(gm); break;
            case "Gold":     yield return CardEffectLibrary.Gold(gm); break;
            case "Cellar":     yield return CardEffectLibrary.Cellar(gm); break;
            case "Moat":     yield return CardEffectLibrary.Moat(gm); break;
            case "Chapel":     yield return CardEffectLibrary.Chapel(gm); break;
            case "Vassal":     yield return CardEffectLibrary.Vassal(gm); break;
            case "Workshop":   yield return CardEffectLibrary.Workshop(gm); break;
            case "Merchant":     yield return CardEffectLibrary.Merchant(gm); break;
            case "Harbinger":     yield return CardEffectLibrary.Harbinger(gm); break;
            case "Village":     yield return CardEffectLibrary.Village(gm); break;
            case "Remodel":    yield return CardEffectLibrary.Remodel(gm); break;
            case "Smithy":    yield return CardEffectLibrary.Smithy(gm); break;
            case "Moneylender":    yield return CardEffectLibrary.Moneylender(gm); break;
            case "ThroneRoom": yield return CardEffectLibrary.ThroneRoom(gm); break;
            
            
            
            default:
                Debug.LogWarning($"{cardName} の効果が定義されていません");
                yield break;
        }
    }
}
