using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public ConditionID Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public string EffectMessage { get; set; }

    public Action<DeityMons> OnBeforeAttack { get; set; }
    public Action<DeityMons> OnAfterTurn { get; set; }
    public Action<DeityMons> OnStart { get; set; }
    public Func<DeityMons, bool> OnBeforeMove { get; set; }

    public Action<DeityMons> OnWeather { get; set; }
    public Func<DeityMons, DeityMons, Moves, float> OnDamageModify { get; set; }
}
