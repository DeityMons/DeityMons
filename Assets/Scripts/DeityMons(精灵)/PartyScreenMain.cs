using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreenMain : MonoBehaviour
{
    [SerializeField] Image DeityMonIMG;
    [SerializeField] Text DeityMonName;
    [SerializeField] Fader fader;

    [SerializeField] Text Attack;
    [SerializeField] Text SpAttack;
    [SerializeField] Text Defense;
    [SerializeField] Text SpDefense;
    [SerializeField] Text Speed;

    [SerializeField] Text Ability;
    [SerializeField] Text Gender;
    [SerializeField] Text Rarity;

    PartyMemberUI[] memberSlots;
    List<DeityMons> DeityMonss;
    DeityMonsParty party;

    int selection = 0;

    Image image;

    public DeityMons SelectedMember => DeityMonss[selection];

    public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);

        party = DeityMonsParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        DeityMonss = party.DeityMons;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < DeityMonss.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(DeityMonss[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelection(selection);
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = selection;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++selection;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --selection;

        selection = Mathf.Clamp(selection, 0, DeityMonss.Count - 1);

        if (selection != prevSelection)
            UpdateMemberSelection(selection);

        if (Input.GetKeyDown(KeyCode.Z) | Input.GetKeyDown(KeyCode.Space))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            fader.Open(false);
            onBack?.Invoke();
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < DeityMonss.Count; i++)
        {
            if (i == selectedMember)
            {
                image = DeityMonIMG.GetComponent<Image>();
                memberSlots[i].SetSelected(true);
                image.sprite = DeityMonss[i].Base.Frontsprite;
                DeityMonName.text = DeityMonss[i].Base.Name;

                Attack.text = DeityMonss[i].Attack + "";
                SpAttack.text = DeityMonss[i].SpAttack + "";
                Defense.text = DeityMonss[i].Defense + "";
                SpDefense.text = DeityMonss[i].SpDefense + "";
                Speed.text = DeityMonss[i].Speed + "";

                Ability.text = DeityMonss[i].Ability.Name + "";
                Gender.text = DeityMonss[i].Gender + "";
                Rarity.text = DeityMonss[i].Base.rarity + "";
            }
            else
                memberSlots[i].SetSelected(false);
        }
    }
}
