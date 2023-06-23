using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : EffectData
{
    public AbilityID Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public Action<Dictionary<Stat, int>, DeityMons, DeityMons> OnBoost { get; set; }
    public Func<float, DeityMons, DeityMons, Moves, float> OnBasePower { get; set; }
    public Action<float, DeityMons, DeityMons, Moves> OnDamagingHit { get; set; }
    public Action<float, DeityMons, DeityMons, Moves> OnDamageDid { get; set; }

    //stats ability
    public Func<float, DeityMons, DeityMons, Moves, float> OnModifyAtk { get; set; }
    public Func<float, DeityMons, DeityMons, Moves, float> OnModifyDef { get; set; }
    public Func<float, DeityMons, DeityMons, Moves, float> OnModifySpAtk { get; set; }
    public Func<float, DeityMons, DeityMons, Moves, float> OnModifySpDef { get; set; }
    public Func<float, DeityMons, DeityMons, Moves, float> OnModifySpd { get; set; }
    public Func<float, DeityMons, DeityMons, Moves, float> OnModifyAcc { get; set; }
    public Func<DeityMons, Moves, float, Condition, float> OnModifyDamageBonus { get; set; }
    public Func<DeityMons, Moves, float, Condition, float> OnModifyDamageReduce { get; set; }

    //Status ability
    public Func<ConditionID, DeityMons, EffectData, bool> OnTrySetVolatile { get; set; }
    public Func<ConditionID, DeityMons, EffectData, bool> OnTrySetStatus { get; set; }

    //Weathers ability
    public ConditionID OnEnterBattle { get; set; }
    public int weatherDuration { get; set; }
}
