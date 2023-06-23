using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status Conditions")]
    [SerializeField] List<ConditionID> status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(DeityMons deityMon)
    {
        bool Used = false;
        // Revive
        if (revive || maxRevive)
        {
            if (deityMon.HP < 0)
            {

                if (revive)
                    deityMon.IncreaseHP(deityMon.MaxHp / 2);
                else if (maxRevive)
                    deityMon.IncreaseHP(deityMon.MaxHp);

                deityMon.CureStatus();

                Used = true;
            }
        }

        // No other items can be used on fainted deityMon
        if (deityMon.HP == 0)
            return false;

        // Restore HP
        if (restoreMaxHP || hpAmount > 0)
        {
            if (deityMon.HP < deityMon.MaxHp)
            {
                if (restoreMaxHP)
                    deityMon.IncreaseHP(deityMon.MaxHp);
                else
                    deityMon.IncreaseHP(hpAmount);

                Used = true;
            }
        }

        // Recover Status
        if (recoverAllStatus || status.Count > 0)
        {
            if (deityMon.Status != null | deityMon.VolatileStatus != null)
            {
                if (recoverAllStatus)
                {
                    deityMon.CureStatus();
                    deityMon.CureVolatileStatus();
                }
                else
                {
                    bool curedStatus = false;
                    foreach (ConditionID condition in status)
                    {
                        if (deityMon.Status.Id == condition)
                        {
                            deityMon.CureStatus();
                            curedStatus = true;
                        }
                        else if (deityMon.VolatileStatus.Id == condition)
                        {
                            deityMon.CureVolatileStatus();
                            curedStatus = true;
                        }
                    }
                    if (curedStatus)
                    {
                        Used = true;
                    }
                }
            }
        }

        // Restore PP
        if (restoreMaxPP)
        {
            deityMon.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
            Used = true;
        }
        else if (ppAmount > 0)
        {
            deityMon.Moves.ForEach(m => m.IncreasePP(ppAmount));
            Used = true;
        }

        return Used;
    }
}

