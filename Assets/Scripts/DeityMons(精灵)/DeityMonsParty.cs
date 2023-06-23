using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeityMonsParty : MonoBehaviour
{
    [SerializeField] List<DeityMons> deityMons;

    public event Action OnUpdated;

    public List<DeityMons> DeityMons
    {
        get
        {
            return deityMons;
        }
        set
        {
            deityMons = value;
            OnUpdated?.Invoke();
        }
    }

    private void Start()
    {
        foreach (var deityMon in deityMons)
        {
            deityMon.Init();
        }
    }

    public DeityMons GetHealthyDeityMons()
    {
        return deityMons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddDeityMons(DeityMons newDeityMons)
    {
        if (DeityMons.Count < 6)
        {
            DeityMons.Add(newDeityMons);
            
        }
        else
        {
            // TODO: Add to the PC once that's implemented
        }
    }

    public static DeityMonsParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<DeityMonsParty>();
    }
}