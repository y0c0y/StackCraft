using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    [Serializable]
    public struct CustomStackingRule
    {
        public StackingRuleFlags customIncludeStackingOnMe;
        public StackingRuleFlags customExcludeStackingOnMe;
        public StackingRuleFlags customIncludeICanStackOn;
        public StackingRuleFlags customExcludeICanStackOn;
    }
    
    public string cardName;
    public string description;
    public Sprite sprite;
    public CardType cardType;
    public bool isConsumable = true;
    public bool isStatic = false;

    public CustomStackingRule customStackingRule;
}
