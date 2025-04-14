using UnityEngine;

public enum CardType
{
    Person,
    Resource,
    Producer
}

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    public string cardName;
    public string description;
    public Sprite sprite;
    public CardType cardType;
}
