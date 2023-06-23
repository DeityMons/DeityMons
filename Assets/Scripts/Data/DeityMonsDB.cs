using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeityMonsDB
{
    static Dictionary<string, DeityMonsBase> DeityMons;
    
    public static void Init()
    {
        DeityMons = new Dictionary<string, DeityMonsBase>();

        var DeityMonsArray = Resources.LoadAll<DeityMonsBase>("");
        foreach (var DeityMons in DeityMonsArray)
        {
            if (DeityMonsDB.DeityMons.ContainsKey(DeityMons.Name))
            {
                Debug.LogError($"There are two DeityMonss with the name {DeityMons.Name}");
                continue;
            }

            DeityMonsDB.DeityMons[DeityMons.Name] = DeityMons;
        }
    }

    public static DeityMonsBase GetDeityMonsByName(string name)
    {
        if (!DeityMons.ContainsKey(name))
        {
            Debug.LogError($"Pokmeon with name {name} not found in the database");
            return null;
        }

        return DeityMons[name];
    }
}