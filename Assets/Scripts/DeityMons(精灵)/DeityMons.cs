using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DeityMons
{
    [SerializeField] DeityMonsBase _base;
    [SerializeField] int level;

    public DeityMons(DeityMonsBase dBase, int dLevel)
    {
        _base = dBase;
        level = dLevel;

        Init();
    }

    public DeityMonsBase Base {
        get { return _base; }
    }
    public int Level {
        get { return level; }
    }

    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Moves> Moves { get; set; }
    public Moves CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    public float damageBonus { get; set; }
    public float damageReduce { get; set; }
    public float TempCrit { get; set; }

    public DeityMonsGender Gender { get; set; }

    public Ability Ability { get; private set; }

    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
    public bool HpChanged { get; set; }
    public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;

    public void Init()
    {
        Moves = new List<Moves>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Moves(move.Base));
            }

            if (Moves.Count >= 4)
            {
                break;
            }
        }
        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatBoost();

        Ability = AbilitiesDB.Abilities[Base.AbilityID];
        Status = null;
        VolatileStatus = null;

        damageBonus = 1f;
        damageReduce = 1f;
        TempCrit = 0;
    }

    public DeityMons(DeityMonsSaveData saveData)
    {
        _base = DeityMonsDB.GetDeityMonsByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;
        Gender = saveData.gender;

        if (saveData.statusId != null)
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        else
            Status = null;

        Moves = saveData.moves.Select(s => new Moves(s)).ToList();

        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolatileStatus = null;
    }

    public DeityMonsSaveData GetSaveData()
    {
        var saveData = new DeityMonsSaveData()
        {
            name = Base.Name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            gender = Gender,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };

        return saveData;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level)));
        Stats.Add(Stat.Defense, Mathf.FloorToInt(Base.Defense * Level*3/2));
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level)));
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level)*3/2));
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level)));
        Stats.Add(Stat.Crit, 1);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level)*3/10);
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Crit, 0 },
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // Apply stat boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            else
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            return true;
        }

        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > DeityMonsBase.MaxNumOfMoves)
            return;

        Moves.Add(new Moves(moveToLearn));
    }

    public bool HasMove(MoveBase moveToCheck)
    {
        return Moves.Count(m => m.Base == moveToCheck) > 0;
    }

    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }
    public int Speed
    {
        get  {return GetStat(Stat.Speed);}
    }
    public int Crit
    {
        get {return GetStat(Stat.Crit); }
    }

    public int MaxHp { get; private set; }
   
    public DamageDetails TakeDamage(Moves move, DeityMons attacker, Condition weather)
    {
        float critical = 1f;
        float STAB = 1f;

        damageBonus = 1f;

        if (Random.value * 100f < (attacker.Crit + attacker.TempCrit)) {
            critical = 2f;
        }

        if (move.Base.Type == attacker.Base.typ1 || move.Base.Type == attacker.Base.typ2) {
            STAB = 1.75f;
        }

        float weatherMod = weather?.OnDamageModify?.Invoke(this, attacker, move) ?? 1f;

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.typ1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.typ2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false

        };

        float attack;
        float defense;

        if (move.Base.Category == MoveCategory.Special)
        {
            attack = attacker.SpAttack;
            defense = SpDefense;

            // Abilites & Held Items might modify the stats
            attack = attacker.ModifySpAtk(attack, attacker, move);
            defense = ModifySpDef(defense, attacker, move);
        }
        else
        {
            attack = attacker.Attack;
            defense = Defense;

            // Abilites & Held might modify the stats
            attack = attacker.ModifyAtk(attack, attacker, move);
            defense = ModifyDef(defense, attacker, move);
        }

        attacker.damageBonus = attacker.ModifyDamageBonus(attacker.damageBonus, attacker, weather ,move);

        damageReduce = ModifyDamageReduce(damageReduce, attacker, weather, move);

        int basePower = Mathf.FloorToInt(attacker.OnBasePower(move.Base.Power, attacker, this, move));

        OnBeforeAttack();
        attacker.OnBeforeAttack();

        float modifiers = Random.Range(0.7f, 1f) * type * critical * STAB * attacker.damageBonus;
        float a = (2 * attacker.Level + 10) / 25f;
        float d = (a * basePower * ((float)attack / defense) * weatherMod) / damageReduce;
        int damage = Mathf.FloorToInt(d * modifiers);

        Debug.Log("basePower: " + basePower + " (float)attack / defense: " + ((float)attack / defense) + " WeatherMod: " + weatherMod + " DamageReduce: " + damageReduce + " level: " + ((2 * attacker.Level + 10) / 25f) + " Type: " + type + " STAB" + STAB + " Damage Bonus" + attacker.damageBonus) ;
        Debug.Log("Damage: " + damage);

        if (damage > 0)
        {
            attacker.OnDamageDid(damage, attacker, move);
            OnDamagingHit(damage, attacker, move);
        }

        if (damage < 0)
        {
            damage *= -1;
        }

        DecreaseHP(damage);

        damageReduce = 1f;
        damageBonus = 1f;

        return damageDetails;
    }

    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
        HpChanged = true;
    }

    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
        HpChanged = true;
    }

    public void SetStatus(ConditionID conditionId, EffectData effect = null)
    {
        if (Status != null) return;

        bool canSet = Ability?.OnTrySetStatus?.Invoke(conditionId, this, effect) ?? true;
        if (!canSet) return;

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId, EffectData effect = null)
    {
        if (VolatileStatus != null) return;

        bool canSet = Ability?.OnTrySetVolatile?.Invoke(conditionId, this, effect) ?? true;
        if (!canSet) return;

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Moves GetRandomMove()
    {
        var moveWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, moveWithPP.Count);
        return moveWithPP[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
                canPerformMove = false;
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;
        }

        return canPerformMove;
    }

    public void OnBeforeAttack()
    {
        Status?.OnBeforeAttack?.Invoke(this);
        VolatileStatus?.OnBeforeAttack?.Invoke(this);
    }


    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }

    public void OnDamagingHit(float damage, DeityMons attacker, Moves move)
    {
        Ability?.OnDamagingHit?.Invoke(damage, this, attacker, move);
    }
    public void OnDamageDid(float damage, DeityMons attacker, Moves move)
    {
        Ability?.OnDamageDid?.Invoke(damage, this, attacker, move);
    }
    public void OnBoost(Dictionary<Stat, int> boosts, DeityMons source)
    {
        Ability?.OnBoost?.Invoke(boosts, this, source);
    }

    public float OnBasePower(float basePower, DeityMons attacker, DeityMons defender, Moves move)
    {
        if (Ability?.OnBasePower != null)
            basePower = Ability.OnBasePower(basePower, attacker, defender, move);

        return basePower;
    }

    public float ModifyAtk(float atk, DeityMons attacker, Moves move)
    {
        if (Ability?.OnModifyAtk != null)
            atk = Ability.OnModifyAtk(atk, attacker, this, move);

        return atk;
    }

    public float ModifySpAtk(float atk, DeityMons attacker, Moves move)
    {
        if (Ability?.OnModifySpAtk != null)
            atk = Ability.OnModifySpAtk(atk, attacker, this, move);

        return atk;
    }

    public float ModifyDef(float def, DeityMons attacker, Moves move)
    {
        if (Ability?.OnModifyDef != null)
            def = Ability.OnModifyDef(def, attacker, this, move);

        return def;
    }

    public float ModifySpDef(float def, DeityMons attacker, Moves move)
    {
        if (Ability?.OnModifySpDef != null)
            def = Ability.OnModifySpDef(def, attacker, this, move);

        return def;
    }

    public float ModifySpd(float spd, DeityMons attacker, Moves move)
    {
        if (Ability?.OnModifySpd != null)
            spd = Ability.OnModifySpd(spd, attacker, this, move);

        return spd;
    }

    public float ModifyAcc(float acc, DeityMons attacker, DeityMons defender, Moves move)
    {
        if (Ability?.OnModifyAcc != null)
            acc = Ability.OnModifyAcc(acc, attacker, defender, move);

        return acc;
    }

    public float ModifyDamageBonus(float damageBonus, DeityMons attacker, Condition weather, Moves move)
    {
        if (attacker.Ability?.OnModifyDamageBonus != null)
            damageBonus = attacker.Ability.OnModifyDamageBonus(attacker, move, damageBonus, weather);

        return damageBonus;
    }
    public float ModifyDamageReduce(float damageReduce, DeityMons attacker, Condition weather, Moves move)
    {
        if (Ability?.OnModifyDamageReduce != null)
            damageReduce = Ability.OnModifyDamageReduce(attacker, move, damageReduce, weather);

        return damageReduce;
    }
    public DeityMonsGender findGender()
    {
        float modyfier = Random.value * 100;
        Debug.Log(modyfier);
        if(modyfier <= 10)
        {
            return DeityMonsGender.None;
        }
        else if(modyfier <= 60)
        {
            return DeityMonsGender.Male;
        }
        else
        {
            return DeityMonsGender.Female;
        }
    }

}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class DeityMonsSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
    public DeityMonsGender gender;
}