using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject Icon;
    [SerializeField] Image messageBox;
    [SerializeField] Text messageText;

    DeityMons _DeityMons;
    Image image;

    public void Init(DeityMons DeityMons)
    {
        if (DeityMons.Gender == DeityMonsGender.XXX)
        {
            DeityMons.Gender = DeityMons.findGender();
        }

        _DeityMons = DeityMons;
        UpdateData();
        SetMessage("");

        _DeityMons.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        image = Icon.GetComponent<Image>();

        nameText.text = _DeityMons.Base.Name;
        levelText.text = "Lvl " + _DeityMons.Level;
        image.sprite = _DeityMons.Base.ICON;
        int HP = _DeityMons.HP;
        int Maxhp = _DeityMons.MaxHp;
        hpBar.SetHP(((float)_DeityMons.HP / _DeityMons.MaxHp), HP, Maxhp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = GlobalSettings.i.HighlightedColor;
        else
            nameText.color = Color.black;
    }

    public void SetMessage(string message)
    {
        if (message.Equals(""))
        {
            messageBox.GetComponent<Image>().enabled = false;
        }
        else
        {
            messageBox.GetComponent<Image>().enabled = true;
        }

        messageText.text = message;
    }
}
