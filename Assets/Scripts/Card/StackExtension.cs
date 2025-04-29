using System.Collections.Generic;
using System.Linq;

public static class StackExtension
{
    public static bool HasCardType(this Stack stack, CardType type) =>
        stack.cards.Any(card => card.cardData.cardType == type);
    public static List<Card> GetCardTypeInStack(this Stack stack, CardType type) =>
        stack.cards.Where(card => card.cardData.cardType == type).ToList();
    public static bool IsInsideField(this Stack stack, Field field)
    {
        return field.IsInsideField(stack.TopCard.transform.position);
    }
}