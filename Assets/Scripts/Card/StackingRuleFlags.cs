using System;

[Flags]
public enum StackingRuleFlags
{
    None      = 0,
    Person    = 1 << 0,
    Resource  = 1 << 1,
    Producer  = 1 << 2
}
