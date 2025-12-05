using System;

namespace BoLLogic;


public enum CombatType
{
    Melee,
    Ranged
}


public sealed class CombatManager
{

    private static readonly Lazy<CombatManager> instance = new Lazy<CombatManager>(() => new CombatManager());

    public static CombatManager Instance => instance.Value;

    // Combat constants
    private const int DICE_MIN = 1;
    private const int DICE_MAX = 6;

    public AttackType Type { get; set; }
    public Unit Attacker { get; set; }
    public Unit Target { get; set; }
    public Path AttackPath { get; set; }
    public Path OriginalAttackPath { get; set; }
    public UnitState AttackerState { get; set; }
    public UnitState TargetState { get; set; }
    public CombatType CurrentCombatType { get; set; }

    readonly Random roller = new();
    readonly List<int> attackDice = new();
    readonly List<int> defenseDice = new();
    int dieResult;

    public int DiceModifier { get; set; }
    public int NumberOfHits { get; set; }
    public int NumberOfWounds { get; set; }
    public int NumberOfRetreats { get; set; }
    public int NumberOfRetreatSpaces { get; set; }

    public event EventHandler<StateChangedEventArgs> ChangeUnitState;
    public event EventHandler<MoraleEventArgs> ChangeMorale;



    public bool DeclareCombat(AttackType type, Path attackPath)
    {

        Type = type;
        AttackPath = attackPath;
        if (AttackPath?.TilesInPath == null || AttackPath.TilesInPath.Count == 0)
        {
            MessageController.Instance.Show("Invalid attack path!");
            return false;
        }

        Attacker = AttackPath.TilesInPath.First().Unit;
        Target = AttackPath.TilesInPath.Last().Unit;

        if (Attacker == null || Target == null)
        {
            MessageController.Instance.Show("Invalid attacker or target!");
            return false;
        }

        if (CheckCombat(type) == false)
            return false;

        if (Type == AttackType.Normal)
        {
            OriginalAttackPath = AttackPath;
        }

        AttackerState = Attacker.State;
        TargetState = Target.State;


        ChangeUnitState?.Invoke(this, new StateChangedEventArgs(Attacker, UnitState.Attacking));
        ChangeUnitState?.Invoke(this, new StateChangedEventArgs(Target, UnitState.Defending));

        if (TurnManager.Instance.CurrentTurnPhase == TurnPhase.Move
            || TurnManager.Instance.CurrentTurnPhase == TurnPhase.Advance)
        {
            TurnManager.Instance.CurrentTurnPhase = TurnPhase.Attack;

            TurnManager.Instance.AdvanceTurnPhase();
        }



        if (Attacker.Tile.Adjacents.Contains(Target.Tile))
        {
            CurrentCombatType = CombatType.Melee;
        }
        else
        {
            CurrentCombatType = CombatType.Ranged;
        }


        MessageController.Instance.Show($"{Attacker} {Type}({CurrentCombatType}) attacks {Target}");

        return true;

    }


    public void Combat(AttackType type)
    {

        if (type == AttackType.Normal)
        {
            if (OriginalAttackPath?.TilesInPath == null || OriginalAttackPath.TilesInPath.Count == 0)
            {
                MessageController.Instance.Show("Invalid combat state!");
                return;
            }

            AttackPath = OriginalAttackPath;
            Attacker = AttackPath.TilesInPath.First().Unit;
            Target = AttackPath.TilesInPath.Last().Unit;

            if (Attacker == null || Target == null)
            {
                MessageController.Instance.Show("Attacker or target is missing!");
                return;
            }

            // Validate combat state - check for stale references
            if (!ValidateCombatState())
            {
                MessageController.Instance.Show("Combat state is invalid - units may have moved or died!");
                ClearCombat(type);
                return;
            }
        }


        PathFinder.Instance.AssignPath(OriginalAttackPath, PathType.Attack);
   

        SoundController.Instance.PlaySound("shake_dice");

        if (CheckCombat(type) == false)
            return;
        ResetCombat();
        RollAttackDice(type);
        DetermineHits();
        RollDefenseDice();
        DetermineWounds();
        DetermineRetreats();
        CalculateCombat(type);
        EndCombat(type);
    }


    bool CheckCombat(AttackType type)
    {

        if (Attacker == null)
        {
            MessageController.Instance.Show("No attacker!");
            return false;
        }

        if (Target == null)
        {
            MessageController.Instance.Show("No target!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that combat state is still valid (units haven't died/moved)
    /// </summary>
    bool ValidateCombatState()
    {
        // Check attacker is alive and at expected position
        if (Attacker == null || Attacker.State == UnitState.Dead)
        {
            return false;
        }

        // Check target is alive and at expected position
        if (Target == null || Target.State == UnitState.Dead)
        {
            return false;
        }

        // Check units are still at the positions in the attack path
        if (OriginalAttackPath?.TilesInPath == null || OriginalAttackPath.TilesInPath.Count < 2)
        {
            return false;
        }

        var attackerTile = OriginalAttackPath.TilesInPath.First();
        var targetTile = OriginalAttackPath.TilesInPath.Last();

        // Verify attacker is still at the starting position
        if (attackerTile.Unit != Attacker)
        {
            return false;
        }

        // Verify target is still at the target position
        if (targetTile.Unit != Target)
        {
            return false;
        }

        return true;
    }


    void ResetCombat()
    {
        attackDice.Clear();
        defenseDice.Clear();

        NumberOfHits = 0;
        NumberOfWounds = 0;
        NumberOfRetreats = 0;
        NumberOfRetreatSpaces = 0;
    }


    bool RollAttackDice(AttackType type)
    {

        int numberofDice;

        if (type == AttackType.Normal)
        {
            numberofDice = Attacker.Health.GetHealth() + DiceModifier;
        }
        else
        {
            numberofDice = Attacker.Health.GetHealth();
        }

        for (int n = 0; n < numberofDice; n++)
        {
            attackDice.Add(RollDie());
        }

        return true;
    }


    int DetermineHits()
    {
        // Null check for Attacker
        if (Attacker == null)
            return 0;

        int attackPoint;

        if (CurrentCombatType == CombatType.Melee)
        {
            attackPoint = Attacker.MeleeAttackPoint;

            //LEADERSHIP SKILL - null check for Tile
            if (Attacker.Tile != null)
            {
                foreach (Tile tile in Attacker.Tile.Adjacents)
                {
                    if (tile != null
                        && tile.Unit != null
                        && tile.Unit.Faction == Attacker.Faction
                        && tile.Unit.Type == UnitType.Leader)
                    {
                        attackPoint--;
                    }
                }
            }
        }
        else
        {
            attackPoint = Attacker.RangedAttackPoint;
        }

        foreach (int die in attackDice)
        {
            if (die >= attackPoint)
            {
                NumberOfHits++;
            }
        }

        return NumberOfHits;
    }

    bool RollDefenseDice()
    {
        for (int n = 0; n < NumberOfHits; n++)
        {
            defenseDice.Add(RollDie());
        }

        return true;
    }


    int DetermineWounds()
    {
        int defensePoint;


        if (CurrentCombatType == CombatType.Melee)
        {
            defensePoint = Target.MeleeDefensePoint;
        }
        else
        {
            defensePoint = Target.RangedDefensePoint;
        }


        foreach (int die in defenseDice)
        {
            if (die < defensePoint)
            {
                NumberOfWounds++;
            }
        }

        return NumberOfWounds;
    }


    int DetermineRetreats()
    {
        // Null check for Target
        if (Target == null)
            return 0;

        int defensePoint;


        if (CurrentCombatType == CombatType.Melee)
        {
            defensePoint = Target.MeleeDefensePoint;
        }
        else
        {
            defensePoint = Target.RangedDefensePoint;
        }
        foreach (int die in defenseDice)
        {
            if (die == defensePoint)
            {
                NumberOfRetreats++;
            }
        }


        //LEADERSHIP SKILL - null check for Tile
        if (Target.Tile != null)
        {
            foreach (Tile tile in Target.Tile.Adjacents)
            {
                if (tile != null
                    && tile.Unit != null
                    && tile.Unit.Faction == Target.Faction
                    && tile.Unit.Type == UnitType.Leader
                    && NumberOfRetreats > 0)
                {
                    NumberOfRetreats--;
                }
            }
        }

        //NO RETREAT SKILL
        if (Target.Type == UnitType.Leader
            && NumberOfRetreats > 0)
        {
            NumberOfRetreats--;
        }

        return NumberOfRetreats;
    }


    public bool CalculateCombat(AttackType type)
    {

        Target.Health.Damage(NumberOfWounds);


        if (Target.State != UnitState.Dead && NumberOfRetreats > 0)
        {
            NumberOfRetreatSpaces = (Target.MarchMove - 1) * NumberOfRetreats;
            ChangeUnitState?.Invoke(this, new StateChangedEventArgs(Target, UnitState.Retreating));
        }


        ChangeUnitState?.Invoke(this, new StateChangedEventArgs(Attacker, AttackerState));

        // Only restore target state if not retreating
        if (Target.State != UnitState.Retreating)
        {
            ChangeUnitState?.Invoke(this, new StateChangedEventArgs(Target, TargetState));
        }



        if (type == AttackType.Normal)
        {
            CheckAdvance();
        }


        return true;

    }


    public void CheckAdvance()
    {
            // Add null check for OriginalAttackPath
            if (OriginalAttackPath?.TilesInPath != null &&
                OriginalAttackPath.TilesInPath.Count > 0 &&
                OriginalAttackPath.TilesInPath.Last().Unit == null &&
                CurrentCombatType == CombatType.Melee &&
                Attacker != null)
            {
                ChangeUnitState?.Invoke(this, new StateChangedEventArgs(Attacker, UnitState.Advancing));
            }
    }


    public void EndCombat(AttackType type)
    {

        var attackResult = String.Join(", ", attackDice.ToArray());
        var defenseResult = String.Join(", ", defenseDice.ToArray());

        MessageController.Instance.Show($"{Attacker} attacks {Target}\n" +
                                $"AttackDice: {attackResult}, DefenceDice: {defenseResult}\n" +
              $"Hits: {NumberOfHits}, Wounds: {NumberOfWounds}  Retreats: {NumberOfRetreats}");


        MoraleCheck(Attacker, Target);

        ClearCombat(type);

    }


    void MoraleCheck(Unit attacker, Unit target)
    {
        if (target.State == UnitState.Dead && target.IsLight == false)
        {
            ChangeMorale?.Invoke(this, new MoraleEventArgs(attacker.Faction, 1));
            ChangeMorale?.Invoke(this, new MoraleEventArgs(target.Faction, -1));
        }
    }


    public void ClearCombat(AttackType type)
    {
        Attacker = null;
        Target = null;

        if (type == AttackType.Normal)
        {
            DiceModifier = 0;
        }
    }

    public void CancelCombat()
    {
        ChangeUnitState?.Invoke(this, new StateChangedEventArgs(Attacker, AttackerState));
        ChangeUnitState?.Invoke(this, new StateChangedEventArgs(Target, TargetState));
    }


    int RollDie()
    {
        dieResult = roller.Next(DICE_MIN, DICE_MAX + 1);
        return dieResult;
    }

}





