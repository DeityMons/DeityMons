using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DeityMonsBase", menuName = "DeityMons/Create DeityMons")]

public class DeityMonsBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] Sprite Icon;

    [SerializeField] DeityMonsType type1;
    [SerializeField] DeityMonsType type2;

    [SerializeField] DeityMonsRarity Rarity;

    [SerializeField] int Maxhp;
    [SerializeField] int NormalATK;
    [SerializeField] int SpecialATK;
    [SerializeField] int NormalDefense;
    [SerializeField] int SpecialDefense;
    [SerializeField] int speed;
    [SerializeField] int critRate;

    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;

    [SerializeField] AbilityID ability;

    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> learnableByItems;

    public static int MaxNumOfMoves { get; set; } = 4;

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 3 * (level * level * level) / 4;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }
        else if (growthRate == GrowthRate.Medium)
        {
            return 5 * (level * level * level) /4;
        }
        else if (growthRate == GrowthRate.MediumSlow)
        {
            return 6 * (level * level * level) /4;
        }
        else if (growthRate == GrowthRate.Slow)
        {
            return 2 * level * level * level;
        }

        return -1;
    }

    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public Sprite Frontsprite
    {
        get { return frontSprite; }
    }
    public Sprite Backsprite
    {
        get { return backSprite; }
    }
    public Sprite ICON
    {
        get { return Icon; }
    }
    public DeityMonsType typ1
    {
        get { return type1; }
    }
    public DeityMonsType typ2
    {
        get { return type2; }
    }
    public AbilityID AbilityID
    {
        get { return ability; }
    }
    public int MaxHp
    {
        get { return Maxhp; }
    }
    public int Attack
    {
        get { return NormalATK; }
    }
    public int SpAttack
    {
        get { return SpecialATK; }
    }
    public int Defense
    {
        get { return NormalDefense; }
    }
    public int SpDefense
    {
        get { return SpecialDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }
    public int Crit
    {
        get { return critRate; }
    }
    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }

    public DeityMonsRarity rarity
    {
        get { return Rarity;  }
    }

    public List<MoveBase> LearnableByItems => learnableByItems;

    public int CatchRate => catchRate;

    public int ExpYield => expYield;
    public GrowthRate GrowthRate => growthRate;
}

[System.Serializable] 

public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

public enum DeityMonsType
{
    None,
    Normal,
    Fire,
    Water,
    Grass,
    Wind,
    Electric,
    Ice,
    Metal,
    Light,
    Dark,
    Evil,
    Dragon,
    Chaos,
    Holy,
}

public enum DeityMonsGender
{
    XXX,
    None,
    Male,
    Female,
}

public enum DeityMonsRarity
{
    Mythical,
    Epic,
    Superior,
    Rare,
    Common,
}

public enum GrowthRate
{
    Fast, MediumFast, Medium, MediumSlow, Slow
}


public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    Crit,
    Accuracy,
    Evasion,
}
public class TypeChart
{
    static float[][] chart =
    {
        //                  NOR  FIR  WAT  GRS  WND  ELE  ICE  MET  LHT  DRK  EVL  DRG  CHA  HOL            
        /*NOR*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 1f, 1f, 1f },
        /*FIR*/ new float[] { 1f, 1f, 0.5f, 2f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 0.5f, 1f, 1f },
        /*WAT*/ new float[] { 1f, 2f, 1f, 0.5f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f, 0.5f, 1f, 1f },
        /*GRS*/ new float[] { 1f, 0.5f, 2f, 1f, 0.5f, 1f, 1f, 0.5f, 1f, 1f, 1f, 0.5f, 1f, 1f },
        /*WND*/ new float[] { 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 0.5f, 1f, 1f, 1f, 1f, 1f, 1f },
        /*ELE*/ new float[] { 1f, 1f, 2f, 0.5f, 2f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f },
        /*ICE*/ new float[] { 1f, 0.5f, 0.5f, 2f, 2f, 1f, 1f, 0.5f, 1f, 1f, 1f, 2f, 1f, 1f },
        /*MET*/ new float[] { 2f, 0.5f, 1f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0.5f },
        /*LHT*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f, 0.5f, 2f, 1f, 1f, 2f },
        /*DRK*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 2f, 1f, 0.5f, 1f, 2f, 0.5f },
        /*EVL*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0.5f, 2f, 1f, 1f, 2f, 0.5f },
        /*DRG*/ new float[] { 1f, 2f, 1f, 2f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 1f, 1f, 1f, 1f },
        /*CHA*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 0.5f, 0.5f, 0.5f, 2f, 1f, 2f },
        /*HOL*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 0.5f, 2f, 2f, 1f, 0.5f, 1f },
    };

    public static float GetEffectiveness(DeityMonsType attackType, DeityMonsType defenseType)
    {
        if (attackType == DeityMonsType.None || defenseType == DeityMonsType.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}

