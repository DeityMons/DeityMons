using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moves
{
   public MoveBase Base { get; set; }

    public int PP { get; set; }
    public int Power { get; set; }

    public Moves(MoveBase dBase)
    {
        Base = dBase;
        PP = dBase.PP;
        Power = dBase.Power;
    }

    public Moves(MoveSaveData saveData)
    {
        Base = MoveDB.GetMoveByName(saveData.name);
        PP = saveData.pp;
        Power = saveData.power;
    }

    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData()
        {
            name = Base.Name,
            pp = PP,
            power = Power,
        };
        return saveData;
    }

    public void IncreasePP(int amount)
    {
        PP = Mathf.Clamp(PP + amount, 0, Base.PP);
    }
}

[Serializable]
public class MoveSaveData
{
    public string name;
    public int pp;
    public int power;
}