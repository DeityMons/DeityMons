using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new DeityMons Contract")]
public class ContractItems : ItemBase
{
    [SerializeField] float catchRateModfier = 1;

    public override bool Use(DeityMons deityMons)
    {
        return true;
    }

    public override bool CanUseOutsideBattle => false;

    public float CatchRateModifier => catchRateModfier;
}
