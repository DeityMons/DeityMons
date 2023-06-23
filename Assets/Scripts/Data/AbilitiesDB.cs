using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesDB
{
    public static void Init()
    {
        foreach (var kvp in Abilities)
        {
            var abilityId = kvp.Key;
            var ability = kvp.Value;

            ability.Id = abilityId;
            ability.Source = EffectSource.Ability;
            ability.SourceId = (int)abilityId;
        }
    }

    public static Dictionary<AbilityID, Ability> Abilities { get; set; } = new Dictionary<AbilityID, Ability>()
    {
        {
            AbilityID.None,
            new Ability()
            {
                OnEnterBattle = ConditionID.none
            }
        },
        {
            AbilityID.Marine_Supremacy,
            new Ability()
            {
                Name = "Marine Supremacy",
                Description = "Every time enters the battle, starts the Ultimate Boundless Occean, in the weather, 20% damage reduction. Recovers 20% of each water type damage. ",

                OnEnterBattle = ConditionID.Ultimate_Boundless_Occean,
                weatherDuration = 99999,

                OnDamageDid = (damage, attacker, defender, move) => 
                {
                    if(move.Base.Type == DeityMonsType.Water)
                    {
                        float HPrecover = damage * 0.2f;
                        attacker.IncreaseHP((int)HPrecover);
                    }
                },

                OnModifyDamageReduce = (DeityMons d, Moves m, float damageReduce, Condition c ) =>
                {
                    return damageReduce * 1.2f;
                }

            }
        },
        {
            AbilityID.Glorious_embodiment_of_the_sun,
            new Ability()
            {
                Name = "Gloriouse embodiment of the sun",
                Description = "Every time this Deitymon deals a damge, it recovers its health by 8%. If its speed is greater than its oppenent, the base power of every move increases by 5. Else, it recover 12% of health instead.",

                OnBasePower = (damage, attacker, defender, move) =>
                {
                    if(attacker.Speed > defender.Speed){
                        return move.Power + 5;
                    }
                    else
                    {
                        return move.Power;
                    }
                },

                OnDamageDid = (damage, attacker, defender, move) =>
                {
                    if(attacker.Speed < defender.Speed){
                       attacker.IncreaseHP(Mathf.FloorToInt(attacker.MaxHp * 0.12f));
                    }
                    else
                    {
                        attacker.IncreaseHP(Mathf.FloorToInt(attacker.MaxHp * 0.08f));
                    }
                },
            }
        }
    };
}

public enum AbilityID
{
    None,Marine_Supremacy, Glorious_embodiment_of_the_sun
}
