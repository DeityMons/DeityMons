using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;
    [SerializeField] Text HPtextA;
    [SerializeField] Text HPtextB;

    public bool IsUpdating { get; private set; }

    public void SetHP(float hpNormalized, int hp, int Maxhp)
    {
        if(hpNormalized == float.NaN)
        {
            hpNormalized = 1f;
        }

        health.transform.localScale = new Vector3(hpNormalized, 1f);

        HPtextA.text = "" + hp;
        HPtextB.text = "/" + Maxhp;

    }
    public IEnumerator SetHPSmooth(float newHp, DeityMons deityMons)
    {
        IsUpdating = true;

        float curHp = health.transform.localScale.x;
        float changeAmt = Mathf.Abs(curHp - newHp) / 1f; // Amount of HP change per second.
        int CurHp = int.Parse(HPtextA.text);

        // While the absolute difference between current HP and new HP is greater than a small value
        while (Mathf.Abs(curHp - newHp) > Mathf.Epsilon)
        {
            if (curHp < newHp) // If the HP is increasing
            {
                curHp = Mathf.Min(curHp + changeAmt * Time.deltaTime, newHp);
                if (CurHp < deityMons.HP)
                {
                    CurHp++;
                    HPtextA.text = CurHp + "";
                }
            }
            else if (curHp > newHp) // If the HP is decreasing
            {
                curHp = Mathf.Max(curHp - changeAmt * Time.deltaTime, newHp);
                if (CurHp > deityMons.HP)
                {
                    CurHp--;
                    HPtextA.text = CurHp + "";
                }
            }

            health.transform.localScale = new Vector3(curHp, 1f);
            yield return null;
        }

        health.transform.localScale = new Vector3(newHp, 1f);
        HPtextA.text = deityMons.HP + "";

        IsUpdating = false;
    }

}
