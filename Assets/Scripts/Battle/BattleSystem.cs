using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget, BattleOver }
public enum BattleAction { Move, SwitchDeityMons, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialog dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject DeityMonSContractSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    public event Action<bool> OnBattleOver;

    BattleState state;

    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;

    DeityMonsParty playerParty;
    DeityMonsParty trainerParty;
    DeityMons wildDeityMons;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    private Dictionary<DeityMons, int> deityMonsEnterCount = new Dictionary<DeityMons, int>();

    int escapeAttempts;
    MoveBase moveToLearn;

    public Field Field { get; private set; }

    public void StartBattle(DeityMonsParty playerParty, DeityMons wildDeityMons)
    {
        this.playerParty = playerParty;
        this.wildDeityMons = wildDeityMons;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(DeityMonsParty playerParty, DeityMonsParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            playerUnit.Setup(playerParty.GetHealthyDeityMons());
            enemyUnit.Setup(wildDeityMons);

            dialogBox.SetMoveNames(playerUnit.DeityMons.Moves);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.DeityMons.Base.Name} appeared.");
        }
        else
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyDeityMons = trainerParty.GetHealthyDeityMons();
            enemyUnit.Setup(enemyDeityMons);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyDeityMons.Base.Name}");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerDeityMons = playerParty.GetHealthyDeityMons();
            playerUnit.Setup(playerDeityMons);
            yield return dialogBox.TypeDialog($"Go {playerDeityMons.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.DeityMons.Moves);
        }

        deityMonsEnterCount[playerUnit.DeityMons] = 1;

        Field = new Field();

        DeityMons FirstToGo = playerUnit.DeityMons.Speed >= enemyUnit.DeityMons.Speed ? playerUnit.DeityMons : enemyUnit.DeityMons; 
        DeityMons SecondToGo = playerUnit.DeityMons.Speed < enemyUnit.DeityMons.Speed ? playerUnit.DeityMons : enemyUnit.DeityMons;

        if (FirstToGo.Ability?.OnEnterBattle != ConditionID.none)
        {
            Debug.Log("Weather" + FirstToGo.Ability.OnEnterBattle);
            Field.SetWeather(FirstToGo.Ability.OnEnterBattle);
            Field.WeatherDuration = FirstToGo.Ability.weatherDuration;

            yield return dialogBox.TypeDialog(Field.Weather.StartMessage);
        }

        if (SecondToGo.Ability?.OnEnterBattle != ConditionID.none)
        {
            Debug.Log("Weather" + SecondToGo.Ability.OnEnterBattle);
            Field.SetWeather(SecondToGo.Ability.OnEnterBattle);
            Field.WeatherDuration = SecondToGo.Ability.weatherDuration;

            yield return dialogBox.TypeDialog(Field.Weather.StartMessage);
        }

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.DeityMons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.EnableActionSelector(true);
    }

    void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(DeityMons newDeityMons)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newDeityMons.Base.Name}. Do you want to change DeityMons?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(DeityMons DeityMons, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(DeityMons.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.DeityMons.CurrentMove = playerUnit.DeityMons.Moves[currentMove];
            enemyUnit.DeityMons.CurrentMove = enemyUnit.DeityMons.GetRandomMove();

            int playerPriority = playerUnit.DeityMons.CurrentMove.Base.Priority;
            int enemyPriority = enemyUnit.DeityMons.CurrentMove.Base.Priority;

            bool playerGoesFirst = true;

            if(enemyPriority > playerPriority)
            {
                playerGoesFirst = false;
            }
            else if(enemyPriority == playerPriority){
                playerGoesFirst = playerUnit.DeityMons.Speed >= enemyUnit.DeityMons.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondDeityMons = secondUnit.DeityMons;

            yield return RunMove(firstUnit, secondUnit, firstUnit.DeityMons.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondDeityMons.HP > 0)
            {
                yield return RunMove(secondUnit, firstUnit, secondUnit.DeityMons.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchDeityMons)
            {
                var selectedDeityMons = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchDeityMons(selectedDeityMons);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelector(false);
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            var enemyMove = enemyUnit.DeityMons.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (Field.Weather != null)
        {
            yield return dialogBox.TypeDialog(Field.Weather.EffectMessage);

            Field.Weather.OnWeather?.Invoke(playerUnit.DeityMons);

            yield return ShowStatusChanges(playerUnit.DeityMons);
            if (playerUnit.DeityMons.HpChanged)
            {
                playerUnit.PlayHitAnimation();
            }

            playerUnit.Hud.UpdateHP();

            if (playerUnit.DeityMons.HP == 0)
            {
                yield return dialogBox.TypeDialog($"{playerUnit.DeityMons.Base.Name} fainted!");
                playerUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(playerUnit);
                yield break;
            }

            Field.Weather.OnWeather?.Invoke(enemyUnit.DeityMons);

            yield return ShowStatusChanges(enemyUnit.DeityMons);

            if (enemyUnit.DeityMons.HpChanged)
            {
                enemyUnit.PlayHitAnimation();
            }

            enemyUnit.Hud.UpdateHP();

            if (enemyUnit.DeityMons.HP == 0)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.DeityMons.Base.Name} fainted!");
                enemyUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(enemyUnit);
                yield break;
            }

            if (Field.WeatherDuration != null)
            {
                Field.WeatherDuration--;
                if (Field.WeatherDuration == 0)
                {
                    Field.Weather = null;
                    Field.WeatherDuration = null;
                    yield return dialogBox.TypeDialog("Weather has changed back to normal");
                }
            }
        }

        if (state != BattleState.BattleOver)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Moves move)
    {
        bool canRunMove = sourceUnit.DeityMons.OnBeforeMove();

        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.DeityMons);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.DeityMons);

        move.PP--;
        dialogBox.SetMoveNames(playerUnit.DeityMons.Moves);

        yield return dialogBox.TypeDialog($"{sourceUnit.DeityMons.Base.Name} used {move.Base.Name}. ");

        if (CheckIfMoveHits(move, sourceUnit.DeityMons, targetUnit.DeityMons))
        {
            int hitTimes = move.Base.GetHitTimes();
            int hit = 1;

            for (int i = 1; i <= hitTimes; ++i)
            {
                sourceUnit.PlayAttackAnimation();
                yield return new WaitForSeconds(1f);
                targetUnit.PlayHitAnimation();

                var damageDetails = (move.Power < 0) ? sourceUnit.DeityMons.TakeDamage(move, sourceUnit.DeityMons, Field.Weather) : targetUnit.DeityMons.TakeDamage(move, sourceUnit.DeityMons, Field.Weather);

                yield return sourceUnit.Hud.WaitForHPUpdate();
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.DeityMons, targetUnit.DeityMons, move.Base.Target);

                if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.DeityMons.HP > 0)
                {
                    foreach (var secondary in move.Base.Secondaries)
                    {
                        var rnd = UnityEngine.Random.Range(1, 101);
                        if (rnd <= secondary.Chance)
                            yield return RunMoveEffects(secondary, sourceUnit.DeityMons, targetUnit.DeityMons, secondary.Target);
                    }
                }

                hit = i;

                if (targetUnit.DeityMons.HP <= 0)
                    break;
            }

            if (hitTimes > 1)
                yield return dialogBox.TypeDialog($"Hit {hit} time(s) !");

            if (targetUnit.DeityMons.HP <= 0)
            {
                yield return HandleDeityMonsFainted(targetUnit);
            }
        }

        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.DeityMons.Base.Name}'s attack missed");
        }

    }

    IEnumerator RunMoveEffects(MoveEffects effects, DeityMons source, DeityMons target, MoveTarget moveTarget)
    {
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }
        if (effects.Status != ConditionID.none)
        {
            if(moveTarget == MoveTarget.Self)
                source.SetStatus(effects.Status);
            else
                target.SetStatus(effects.Status);
        }
        if (effects.VolatileStatus != ConditionID.none)
        {
            if (moveTarget == MoveTarget.Self)
                source.SetVolatileStatus(effects.VolatileStatus);
            else
                target.SetVolatileStatus(effects.VolatileStatus);
        }
        if (effects.Weather != ConditionID.none)
        {
            Field.SetWeather(effects.Weather);

            if (!effects.WeatherForEver)
            {
                Field.WeatherDuration = 99999;
            }
            else
            {
                Field.WeatherDuration = effects.WeatherTurns;
            }
            yield return dialogBox.TypeDialog(Field.Weather.StartMessage);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        sourceUnit.DeityMons.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.DeityMons);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.DeityMons.HP <= 0)
        {
            yield return HandleDeityMonsFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMoveHits(Moves move, DeityMons source, DeityMons target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(DeityMons DeityMons)
    {
        while (DeityMons.StatusChanges.Count > 0)
        {
            var message = DeityMons.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandleDeityMonsFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.DeityMons.Base.Name} Fainted");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            // Exp Gain
            int expYield = faintedUnit.DeityMons.Base.ExpYield;
            int enemyLevel = faintedUnit.DeityMons.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.DeityMons.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.DeityMons.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();

            // Check Level Up
            while (playerUnit.DeityMons.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.DeityMons.Base.Name} grew to level {playerUnit.DeityMons.Level}");

                // Try to learn a new Move
                var newMove = playerUnit.DeityMons.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (playerUnit.DeityMons.Moves.Count < DeityMonsBase.MaxNumOfMoves)
                    {
                        playerUnit.DeityMons.LearnMove(newMove.Base);
                        yield return dialogBox.TypeDialog($"{playerUnit.DeityMons.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.DeityMons.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.DeityMons.Base.Name} trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {DeityMonsBase.MaxNumOfMoves} moves");
                        yield return ChooseMoveToForget(playerUnit.DeityMons, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextDeityMons = playerParty.GetHealthyDeityMons();
            if (nextDeityMons != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextDeityMons = trainerParty.GetHealthyDeityMons();
                if (nextDeityMons != null)
                    StartCoroutine(AboutToUse(nextDeityMons));
                else
                    BattleOver(true);
            }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if (damageDetails.TypeEffectiveness > 2f)
            yield return dialogBox.TypeDialog("It's extremely effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective!");
        else if(damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("It's very effective!");
        }

        yield return new WaitForSeconds(1f);
        
    } 

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };

            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == DeityMonsBase.MaxNumOfMoves)
                {
                    // Don't learn the new move
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.DeityMons.Base.Name} did not learn {moveToLearn.Name}"));
                }
                else
                {
                    // Forget the selected move and learn new move
                    var selectedMove = playerUnit.DeityMons.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.DeityMons.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));

                    playerUnit.DeityMons.Moves[moveIndex] = new Moves(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
        {
            if (currentAction == 0)
            {
                // Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                // Bag
                OpenBag();
            }
            else if (currentAction == 2)
            {
                // Party
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                // Run
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMove -= 1;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMove += 1;

        currentMove = Mathf.Clamp(currentMove, 0, this.playerUnit.DeityMons.Moves.Count - 1);

            dialogBox.UpdateMoveSelection(currentMove, this.playerUnit.DeityMons.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
        {
            var move = playerUnit.DeityMons.Moves[currentMove];
            if (move.PP == 0)
            {
                return;
            }

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted deitymon");
                return;
            }
            if (selectedMember == playerUnit.DeityMons)
            {
                partyScreen.SetMessageText("You can't switch with the same deitymon");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchDeityMons));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchDeityMons(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.DeityMons.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a DeityMons to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerDeityMons());
            }
            else
                ActionSelection();

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                // Yes Option
                OpenPartyScreen();
            }
            else
            {
                // No Option
                StartCoroutine(SendNextTrainerDeityMons());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerDeityMons());
        }
    }

    IEnumerator SwitchDeityMons(DeityMons newDeityMons, bool isTrainerAboutToUse = false)
    {
        if (playerUnit.DeityMons.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.DeityMons.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newDeityMons);
        dialogBox.SetMoveNames(newDeityMons.Moves);
        yield return dialogBox.TypeDialog($"Go {newDeityMons.Base.Name}!");

        if (deityMonsEnterCount.ContainsKey(newDeityMons))
        {
            deityMonsEnterCount[newDeityMons]++;
        }
        else
        {
            deityMonsEnterCount[newDeityMons] = 1;
        }

        if (newDeityMons.Ability?.OnEnterBattle != null)
        {
            Field.SetWeather(newDeityMons.Ability.OnEnterBattle);
            Field.WeatherDuration = newDeityMons.Ability.weatherDuration;

            yield return dialogBox.TypeDialog(Field.Weather.StartMessage);
        }

        if (isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerDeityMons());
        else
            state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerDeityMons()
    {
        state = BattleState.Busy;

        var nextDeityMons = trainerParty.GetHealthyDeityMons();
        enemyUnit.Setup(nextDeityMons);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextDeityMons.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (usedItem is ContractItems)
        {
            yield return UseContract((ContractItems)usedItem);
        }

        StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    public int GetEnterCount(DeityMons deityMons)
    {
        if (deityMonsEnterCount.TryGetValue(deityMons, out int count))
        {
            return count;
        }
        else
        {
            return 0;
        }
    }

    IEnumerator UseContract(ContractItems contractItems)
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal the trainers DeityMons!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used {contractItems.Name.ToUpper()}!");

        var DeityContractObj = Instantiate(DeityMonSContractSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var DeityContract = DeityContractObj.GetComponent<SpriteRenderer>();
        DeityContract.sprite = contractItems.Icon;

        // Animations
        yield return DeityContract.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return DeityContract.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchDeityMons(enemyUnit.DeityMons, contractItems);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return DeityContract.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // DeityMons is befriended
            yield return dialogBox.TypeDialog($"{enemyUnit.DeityMons.Base.Name} was befriended with you");
            yield return DeityContract.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddDeityMons(enemyUnit.DeityMons);
            yield return dialogBox.TypeDialog($"{enemyUnit.DeityMons.Base.Name} has been added to your party");

            Destroy(DeityContract);
            BattleOver(true);
        }
        else
        {
            // DeityMons broke out
            yield return new WaitForSeconds(1f);
            DeityContract.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.DeityMons.Base.Name} broke free");
            else
                yield return dialogBox.TypeDialog($"You almost befriended it!");

            Destroy(DeityContract);
            state = BattleState.RunningTurn;
        }
    }

    int TryToCatchDeityMons(DeityMons DeityMons, ContractItems contractItems)
    {
        float a = (3 * DeityMons.MaxHp - 2 * DeityMons.HP) * DeityMons.Base.CatchRate * contractItems.CatchRateModifier * ConditionsDB.GetStatusBonus(DeityMons.Status) / (3 * DeityMons.MaxHp);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.DeityMons.Speed;
        int enemySpeed = enemyUnit.DeityMons.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
                state = BattleState.RunningTurn;
            }
        }
    }
}
