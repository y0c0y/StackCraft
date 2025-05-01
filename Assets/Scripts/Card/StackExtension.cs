using System.Collections.Generic;
using System.Linq;

public static class StackExtension
{
    public static bool HasCardType(this Stack stack, CardType type) =>
        stack.cards.Any(card => card.cardData.cardType == type);

    public static List<Card> GetCardTypeInStack(this Stack stack, CardType type) =>
        stack.cards.Where(card => card.cardData.cardType == type).ToList();

    public static bool IsInsideField(this Stack stack, Field field) =>
        field.IsInsideField(stack.TopCard.transform.position);

    public static Stack GetRandomStackFromSameField(this Stack stack)
    {
        return GameTableManager.Instance
            .GetAllStacksInField(stack.currentField)
            .Where(s => s != stack 
                        && s.LastCard?.cardData 
                        && BattleManager.Instance ? !BattleManager.Instance.Flag(s.TopCard) : true
                           && stack.LastCard.CanStackOn(s.LastCard))
            .ToList()
            .Random();
    }
}