using UnityEngine;
using UnityEngine.Serialization;

public class CardAppearance : MonoBehaviour
{
    public Card card;
    
    [Header("References")]
    [SerializeField]private SpriteRenderer artSprite;
    
    [Header("Color")]
    [SerializeField] private Color personColor;
    [SerializeField] private Color enemyTintColor;
    
    private void Start()
    {
        var type = card.cardData.cardType;
        artSprite.color = (type == CardType.Enemy) ? enemyTintColor : personColor;
    }
}
