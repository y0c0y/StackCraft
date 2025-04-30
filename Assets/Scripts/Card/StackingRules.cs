public static class StackingRules
{
    public static StackingRuleFlags ToFlag(this CardType type)
    {
        switch (type)
        {
            case CardType.Person:        return StackingRuleFlags.Person;
            case CardType.Resource:      return StackingRuleFlags.Resource;
            case CardType.Producer:      return StackingRuleFlags.Producer;
            case CardType.Building:      return StackingRuleFlags.Building;
            case CardType.Weapon:        return StackingRuleFlags.Weapon;
            case CardType.Construction:  return StackingRuleFlags.Construction;
            case CardType.Enemy:         return StackingRuleFlags.Enemy;
            case CardType.Portal:        return StackingRuleFlags.Portal;
            case CardType.None:
            default:                     return StackingRuleFlags.None;
        }
    }
    
    public static StackingRuleFlags GetDefaultCanStackOnMe(CardType cardBelowType)
    {
        switch (cardBelowType)
        {
            case CardType.Person:
                return StackingRuleFlags.Person | StackingRuleFlags.Resource | StackingRuleFlags.Weapon;
            case CardType.Resource:
                return StackingRuleFlags.Resource;
            case CardType.Producer:
                return StackingRuleFlags.Person;
            case CardType.Building:
                return StackingRuleFlags.Resource;
            case CardType.Construction:
                return StackingRuleFlags.Resource;
            case CardType.Enemy:
                return StackingRuleFlags.Enemy;
            case CardType.Portal:
                return StackingRuleFlags.Person;
            case CardType.Weapon:
            case CardType.None:
            default:
                return StackingRuleFlags.None;
        }
    }

    public static StackingRuleFlags GetDefaultICanStackOn(CardType cardToPlaceType)
    {
        switch (cardToPlaceType)
        {
            case CardType.Person:
                return StackingRuleFlags.Person | StackingRuleFlags.Producer | StackingRuleFlags.Portal;
            case CardType.Resource:
                return StackingRuleFlags.Resource | StackingRuleFlags.Building | StackingRuleFlags.Construction;
            case CardType.Weapon:
                return StackingRuleFlags.Person;
            case CardType.Enemy:
                return StackingRuleFlags.Enemy;
            case CardType.Producer:
            case CardType.Building:
            case CardType.Construction:
            case CardType.Portal:
            case CardType.None:
            default:
                return StackingRuleFlags.None;
        }
    }

    public static bool CanStackOn(this CardData cardToPlace, CardData cardBelow)
    {
        var placingCardFlag = cardToPlace.cardType.ToFlag();
        if (placingCardFlag == StackingRuleFlags.None)
            return false;
        
        StackingRuleFlags allowedOnBelow = GetDefaultCanStackOnMe(cardBelow.cardType);
        allowedOnBelow |= cardBelow.customStackingRule.customIncludeStackingOnMe;
        allowedOnBelow &= ~cardBelow.customStackingRule.customExcludeStackingOnMe;
        
        bool belowAllowsPlacing = (placingCardFlag & allowedOnBelow) != 0;
        
        
        var cardBelowFlag = cardBelow.cardType.ToFlag();
        if (cardBelowFlag == StackingRuleFlags.None)
            return false;
        
        var placeCardCanStackOn = GetDefaultICanStackOn(cardToPlace.cardType);
        placeCardCanStackOn |= cardToPlace.customStackingRule.customIncludeICanStackOn;
        placeCardCanStackOn &= ~cardToPlace.customStackingRule.customExcludeICanStackOn;
        
        bool placingCardAllowsBelow = (cardBelowFlag & placeCardCanStackOn) != 0;
        
        return belowAllowsPlacing && placingCardAllowsBelow;
    }

    public static bool CanStackOn(this Card cardToPlace, Card cardBelow) => cardToPlace.cardData.CanStackOn(cardBelow.cardData);
}
