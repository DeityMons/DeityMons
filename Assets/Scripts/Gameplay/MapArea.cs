using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<DeityMons> wildDeityMonS;

    public DeityMons GetRandomWildDeityMons()
    {
        var wildDeityMons = wildDeityMonS[Random.Range(0, wildDeityMonS.Count)];
        wildDeityMons.Init();
        return wildDeityMons;
    }
}
