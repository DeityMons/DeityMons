using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject statusImage;
    [SerializeField] GameObject expBar;


    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color bldColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;
    [SerializeField] Color brkColor;
    [SerializeField] Color cofColor;

    DeityMons _DeityMons;
    Dictionary<ConditionID, Color> statusColors;

 

    public void SetData(DeityMons deityMons )
    {
        if (_DeityMons != null)
        {
            _DeityMons.OnHPChanged -= UpdateHP;
            _DeityMons.OnStatusChanged -= SetStatusText;
        }

        _DeityMons = deityMons;

        nameText.text = deityMons.Base.Name;
        levelText.text = "Lvl." + deityMons.Level;
        int HP = deityMons.HP;
        int Maxhp = deityMons.MaxHp;
        hpBar.SetHP((float) deityMons.HP / deityMons.MaxHp, HP, Maxhp);
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.bld, bldColor },
            {ConditionID.slp, slpColor },
            {ConditionID.par, parColor },
            {ConditionID.frz, frzColor },
            {ConditionID.brk, brkColor },
            {ConditionID.cof, cofColor }
        };

        SetStatusText();
        _DeityMons.OnStatusChanged += SetStatusText;
        _DeityMons.OnHPChanged += UpdateHP;
    }

    void SetStatusText()
    {
        if (_DeityMons.Status == null)
        {
            statusText.text = "";
            statusImage.SetActive(false);
        }
        else
        {
            statusText.text = _DeityMons.Status.Id.ToString().ToUpper();
            statusText.color = Color.white;
            statusImage.SetActive(true);
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _DeityMons.Level;
    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) yield break;

        if (reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {
        int currLevelExp = _DeityMons.Base.GetExpForLevel(_DeityMons.Level);
        int nextLevelExp = _DeityMons.Base.GetExpForLevel(_DeityMons.Level + 1);

        float normalizedExp = (float)(_DeityMons.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_DeityMons.HP / _DeityMons.MaxHp, _DeityMons);
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }
}
