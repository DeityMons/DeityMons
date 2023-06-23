using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Moves", menuName = "DeityMons/Create New move")]

public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] DeityMonsType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp;
    [SerializeField] int priority;

    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] MoveTarget target;
    [SerializeField] List<MoveFlag> flags;

    public static void InitMoves()
    {
        var movesList = Resources.LoadAll<MoveBase>("");
        foreach (var move in movesList)
        {
            move.effects.Source = EffectSource.Move;
        }
    }

    public bool HasFlag(MoveFlag flag)
    {
        if (flags != null && flags.Contains(flag))
            return true;

        return false;
    }

    public int GetHitTimes()
    {
        if (hitRange == Vector2Int.zero)
            return 1;

        int hitCount = 1;
        if (hitRange.y == 0)
        {
            hitCount = hitRange.x;
        }
        else
        {
            hitCount = Random.Range(hitRange.x, hitRange.y + 1);
        }

        return hitCount;
    }

    [SerializeField] Vector2Int hitRange;

    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public int Power
    {
        get { return power; }
    }
    public int Accuracy
    {
        get { return accuracy; }
    }
    public bool AlwaysHits
    {
        get { return alwaysHits; }
    }
    public int PP
    {
        get { return pp; }
    }
    public DeityMonsType Type
    {
        get { return type; }
    }
    public MoveCategory Category
    {
        get { return category; }
    }

    public MoveEffects Effects
    {
        get { return effects; }
    }
    public List<SecondaryEffects> Secondaries
    {
        get { return secondaries; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }
    public int Priority
    {
        get { return priority;  }
    }  
}

public class EffectData
{
    public EffectSource Source { get; set; }
    public int SourceId { get; set; }
}

[System.Serializable]
public class MoveEffects : EffectData
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;
    [SerializeField] ConditionID weather;
    [SerializeField] int weatherTurns;
    [SerializeField] bool weatherForEver;

    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }

    public ConditionID Status
    {
        get { return status; }
    }

    public ConditionID VolatileStatus
    {
        get { return volatileStatus; }
    }

    public ConditionID Weather
    {
        get { return weather; }
    }

    public int WeatherTurns
    {
        get { return weatherTurns ; }
    }

    public bool WeatherForEver
    {
        get { return weatherForEver; }
    }
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance
    {
        get { return chance; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}
public enum MoveCategory
{
    Physical, Special, Status
}
public enum MoveTarget
{
    Foe, Self
}
public enum Animation
{
    None, 
}
public enum EffectSource
{
    Ability, Item, Move, Condition
}
public enum MoveFlag
{
    Contact, Punch, Bite, Pulse, Sound
}