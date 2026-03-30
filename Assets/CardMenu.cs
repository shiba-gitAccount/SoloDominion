using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewCard", menuName = "Dominion/NewCard")]
public class CustomCard : CardData
{
    [Header("カードプレイ時の効果をここにドラッグ")]
    public UnityEvent<GameManager> onPlayEffect;

    public override void ExecuteEffect(GameManager gm)
    {
        onPlayEffect?.Invoke(gm);
    }
}
