using UnityEngine;
using UnityEngine.Events;
using System.Collections;

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
    public abstract IEnumerator ExecuteEffect(GameManager gm);
}