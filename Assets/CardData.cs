using UnityEngine;
using UnityEngine.Events;

public enum CardType
{
    Action,
    Victory,
    Treasure,
    Curse
}

public abstract class CardData : ScriptableObject
{
    public string cardName;
    public CardType cardtype;
    public Sprite cardImage;
    public int cost;
    public bool isPlayable;
    public abstract void ExecuteEffect(GameManager gm);
}