using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (DeityMons deityMons) =>
                {
                    deityMons.DecreaseHP(deityMons.MaxHp / 8);
                    deityMons.StatusChanges.Enqueue($"{deityMons.Base.Name} hurt itself due to poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (DeityMons deityMons) =>
                {
                    deityMons.DecreaseHP(deityMons.MaxHp / 16);
                    deityMons.StatusChanges.Enqueue($"{deityMons.Base.Name} hurt itself due to burn");
                }
            }
        },
        {
            ConditionID.bld,
            new Condition()
            {
                Name = "Bleed",
                StartMessage = "has been bled",
                OnAfterTurn = (DeityMons deityMons) =>
                {
                    deityMons.DecreaseHP((int)(deityMons.MaxHp / (Random.Range(6f, 12f))));
                    deityMons.StatusChanges.Enqueue($"{deityMons.Base.Name} hurt itself due to bleed");
                }
            }
        },
         {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (DeityMons DeityMons) =>
                {
                    if (Random.Range(1, 5) == 3)
                    {
                        DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name}'s paralyzed and can't move");
                        return false;
                    }

                    return true;
                }
            }
         },
         {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (DeityMons DeityMons) =>
                {
                    if (Random.Range(1, 5) == 3)
                    {
                        DeityMons.CureStatus();
                        DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name}'s is not frozen anymore");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (DeityMons DeityMons) =>
                {
                    DeityMons.StatusTime = Random.Range(1, 4);
                    DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name} will be asleep for {DeityMons.StatusTime} moves");
                },
                OnBeforeMove = (DeityMons DeityMons) =>
                {
                    if (DeityMons.StatusTime <= 0)
                    {
                        DeityMons.CureStatus();
                        DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name} woke up!");
                        return true;
                    }

                    DeityMons.StatusTime--;
                    DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name} is sleeping");
                    return false;
                }
            }
        },

        {
            ConditionID.cof,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (DeityMons DeityMons) =>
                {
                    DeityMons.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Will be confused for {DeityMons.VolatileStatusTime} moves");
                },
                OnBeforeMove = (DeityMons DeityMons) =>
                {
                    if (DeityMons.VolatileStatusTime <= 0)
                    {
                        DeityMons.CureVolatileStatus();
                        DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name} kicked out of confusion!");
                        return true;
                    }
                    DeityMons.VolatileStatusTime--;

                    if (Random.Range(1, 3) == 1)
                        return true;

                    DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name} is confused");
                    DeityMons.DecreaseHP(DeityMons.MaxHp / 8);
                    DeityMons.StatusChanges.Enqueue($"It hurt itself due to confusion");
                    return false;
                }
            }
        },
        { ConditionID.brk,
            new Condition()
            {
                Name = "Broken",
                StartMessage = "has been broken",
                OnBeforeMove = (DeityMons deityMons) =>
                {
                    deityMons.damageBonus = 0.6f;
                    deityMons.StatusChanges.Enqueue($"{deityMons.Base.Name}'s damage decreased due to broken");
                    return true;
                }
            }
        },
        {
            ConditionID.sun,
            new Condition()
            {
                Name = "Sunburn",
                StartMessage = "has been burned by the sun",

                OnStart = (DeityMons DeityMons) =>
                {
                    DeityMons.StatusTime = 3;
                    DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name} will be affected by sun burn for 3 moves.");
                },

                OnBeforeAttack = (DeityMons deityMons) =>
                {
                     deityMons.damageReduce = 0.75f * deityMons.damageReduce;
                     Debug.Log("DamageReduce: " + deityMons.damageReduce);
                },

                OnBeforeMove = (DeityMons deityMons) =>
                {
                    if (deityMons.StatusTime <= 0)
                    {
                        deityMons.CureStatus();
                        deityMons.StatusChanges.Enqueue($"{deityMons.Base.Name} is no longer burned!");
                        return true;
                    }

                    deityMons.damageBonus = 0.75f * deityMons.damageBonus;
                    deityMons.StatusChanges.Enqueue($"{deityMons.Base.Name}'s damage decreased by 25%, and defending ability decreased 25%");
                    deityMons.StatusTime--;
                    Debug.Log("StatusTime: " + deityMons.StatusTime);
                    return true;
                },

                OnAfterTurn = (DeityMons deityMons) =>
                {
                    if(deityMons.Base.typ1 == DeityMonsType.Water || deityMons.Base.typ2 == DeityMonsType.Water)
                    {
                        deityMons.DecreaseHP(deityMons.MaxHp / 16);
                    }else
                    {
                    deityMons.DecreaseHP(deityMons.MaxHp / 8);
                    }
                    
                    deityMons.StatusChanges.Enqueue($"{deityMons.Base.Name} hurt itself due to sun burn");
                }
            }
        },
        {
            ConditionID.snb,
            new Condition()
            {
                Name = "Sun Bless",
                StartMessage = "has been blessed by the sun",

                OnStart = (DeityMons DeityMons) =>
                {
                    DeityMons.StatusTime = 3;
                    DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name} will be blessed for 3 turns");
                    DeityMons.TempCrit = 40;
                },

                OnBeforeMove = (DeityMons deityMons) =>
                {
                    if (deityMons.StatusTime <= 0)
                    {
                        deityMons.CureStatus();
                        deityMons.StatusChanges.Enqueue($"{deityMons.Base.Name} is no longer blessed!");
                         return true;
                    }

                    deityMons.StatusChanges.Enqueue($"{deityMons.Base.Name}'s crit increased 40%");

                    deityMons.StatusTime--;
                    return true;
                },
            }
        },

        // Weather Conditions
        {
            ConditionID.sunny,
            new Condition()
            {
                Name = "Harsh Sunlight",
                StartMessage = "The weather has changed to Harsh Sunlight",
                EffectMessage = "the sulight is harsh",
                OnDamageModify = (DeityMons source, DeityMons target, Moves move) =>
                {
                    if (move.Base.Type == DeityMonsType.Fire)
                        return 1.5f;
                    else if (move.Base.Type == DeityMonsType.Water)
                        return 0.5f;

                    return 1f;
                }
            }
        },
        {
            ConditionID.rain,
            new Condition()
            {
                Name = "Heavy Rain",
                StartMessage = "It started raining heavily",
                EffectMessage = "It's raining heavily",
                OnDamageModify = (DeityMons source, DeityMons target, Moves move) =>
                {
                    if (move.Base.Type == DeityMonsType.Water)
                        return 1.5f;
                    else if (move.Base.Type == DeityMonsType.Fire)
                        return 0.5f;

                    return 1f;
                }
            }
        },
        {
            ConditionID.sandstorm,
            new Condition()
            {
                Name = "Sandstorm",
                StartMessage = "A sandstrom is raging",
                EffectMessage = "The sandstorm rages",
                OnWeather = (DeityMons DeityMons) =>
                {
                    DeityMons.DecreaseHP(Mathf.RoundToInt((float)DeityMons.MaxHp / 16f));
                    DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name} has been buffeted by sandstorm");
                }
            }
        },
        {
            ConditionID.Ultimate_Boundless_Occean,
            new Condition()
            {
                Name = "Ultimate Boundless Occean",
                StartMessage = "A surge of oceanic power sweeps the battle field,the Ultimate Boundless Ocean rose",
                EffectMessage = "The Ultimate Boundless Ocean envelops the battlefield",
                OnDamageModify = (DeityMons source, DeityMons target, Moves move) =>
                {
                    if (move.Base.Type == DeityMonsType.Water || move.Base.Type == DeityMonsType.Light)
                    {
                        return 2f;
                    }
                    else
                    {
                        return 0.75f;
                    }
                },
                OnWeather = (DeityMons DeityMons) =>
                {
                    if(DeityMons.Base.AbilityID == AbilityID.Marine_Supremacy)
                    {
                        DeityMons.IncreaseHP(Mathf.RoundToInt((float)DeityMons.MaxHp / 8f));
                        DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name} has restored some health in it's realm");
                    }else
                    {
                        DeityMons.DecreaseHP(Mathf.RoundToInt((float)DeityMons.MaxHp / 16f));
                        DeityMons.StatusChanges.Enqueue($"{DeityMons.Base.Name} has been affeted by the harsh climate of Ultimate Boundless Occean");
                    }
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
        {
            return 1f;
        }
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
        {
            return 2f;
        }
        else if (condition.Id == ConditionID.bld || condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brk || condition.Id == ConditionID.cof)
        {
            return 1.5f;
        }

        return 1f;
    }
}

public enum ConditionID
{
    none, psn, brn, bld, slp, par, frz, cof, brk, sun, snb,

    sunny, rain, sandstorm, Ultimate_Boundless_Occean
}

