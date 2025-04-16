using UnityEngine;

public static class StackingRules
{
    public static StackingRuleFlags ToFlag(this CardType type)
    {
        switch (type)
        {
            case CardType.Person:   return StackingRuleFlags.Person;
            case CardType.Resource: return StackingRuleFlags.Resource;
            case CardType.Producer: return StackingRuleFlags.Producer;
            case CardType.None:
            default:                return StackingRuleFlags.None;
        }
    }
    
    public static StackingRuleFlags GetDefaultCanStackOnMe(CardType cardBelowType)
    {
        switch (cardBelowType)
        {
            case CardType.Person:
                return StackingRuleFlags.Person | StackingRuleFlags.Resource;
            case CardType.Resource:
                return StackingRuleFlags.Resource;
            case CardType.Producer:
                return StackingRuleFlags.Person;
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
                return StackingRuleFlags.Person | StackingRuleFlags.Producer;
            case CardType.Resource:
                return StackingRuleFlags.Resource | StackingRuleFlags.Person;
            case CardType.Producer:
            case CardType.None:
            default:
                return StackingRuleFlags.None;
        }
    }

    public static bool CanStackByType(CardType cardToPlaceType, CardType cardBelowType)
    {
        var placingCardFlag = cardToPlaceType.ToFlag();
        
        if (placingCardFlag == StackingRuleFlags.None)
            return false;

        StackingRuleFlags allowedOnBelow = GetDefaultCanStackOnMe(cardBelowType);
        bool belowAllowsPlacing = (placingCardFlag & allowedOnBelow) != 0;
        
        var cardBelowFlag = cardBelowType.ToFlag();
        
        if (cardBelowFlag == StackingRuleFlags.None)
            return false;
        
        var placeCardCanStackOn = GetDefaultICanStackOn(cardToPlaceType);
        bool placingCardAllowsBelow = (cardBelowFlag & placeCardCanStackOn) != 0;
        
        return belowAllowsPlacing && placingCardAllowsBelow;
    }
}
