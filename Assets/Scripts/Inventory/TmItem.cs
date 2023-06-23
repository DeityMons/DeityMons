using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;

    public override string Name => base.Name + $": {move.Name}";

    public override string Description => "Teaches a DeityMon to learn" + $": {move.Name}";

    public override bool Use(DeityMons deityMons)
    {
        // Learning move is handled from Inventory UI, If it was learned then return true
        return deityMons.HasMove(move);
    }

    public bool CanBeTaught(DeityMons deityMons)
    {
        return deityMons.Base.LearnableByItems.Contains(move);
    }

    public override bool IsReusable => isHM;

    public override bool CanUseInBattle => false;

    public MoveBase Move => move;
    public bool IsHM => isHM;
}
