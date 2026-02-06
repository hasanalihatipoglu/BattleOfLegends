namespace BoLLogic;

/// <summary>
/// Represents a combat action including attack declaration and results
/// </summary>
public class CombatAction : GameAction
{
    public UnitType AttackerType { get; set; }
    public Position AttackerPosition { get; set; }
    public UnitType DefenderType { get; set; }
    public Position DefenderPosition { get; set; }
    public int AttackerHealthBefore { get; set; }
    public int DefenderHealthBefore { get; set; }
    public int AttackerHealthAfter { get; set; }
    public int DefenderHealthAfter { get; set; }
    public UnitState AttackerStateBefore { get; set; }
    public UnitState DefenderStateBefore { get; set; }
    public UnitState AttackerStateAfter { get; set; }
    public UnitState DefenderStateAfter { get; set; }

    // Faction of attacker and defender for unit lookup after deserialization
    public PlayerType AttackerFaction { get; set; }
    public PlayerType DefenderFaction { get; set; }

    // Track units that were locked by this attack (for undo/redo)
    // Key: "Faction-UnitType" to uniquely identify units
    // Value: Previous state before locking (Idle or Ready)
    public Dictionary<string, UnitState> LockedUnits { get; set; } = new Dictionary<string, UnitState>();

    // Keep references to the actual units for resurrection (not serialized, used for runtime only)
    private readonly Unit _attacker;
    private readonly Unit _defender;

    public CombatAction(PlayerType player, Unit attacker, Unit defender,
        int attackerHealthBefore, int defenderHealthBefore,
        int attackerHealthAfter, int defenderHealthAfter,
        UnitState attackerStateBefore, UnitState defenderStateBefore)
        : base(player)
    {
        _attacker = attacker;
        _defender = defender;
        AttackerType = attacker.Type;
        AttackerFaction = attacker.Faction;
        AttackerPosition = attacker.Position;
        DefenderType = defender.Type;
        DefenderFaction = defender.Faction;
        DefenderPosition = defender.Position;
        AttackerHealthBefore = attackerHealthBefore;
        DefenderHealthBefore = defenderHealthBefore;
        AttackerHealthAfter = attackerHealthAfter;
        DefenderHealthAfter = defenderHealthAfter;
        AttackerStateBefore = attackerStateBefore;
        DefenderStateBefore = defenderStateBefore;
    }

    // Parameterless constructor for JSON deserialization
    public CombatAction() : base() { }

    /// <summary>
    /// Update the final states after combat is complete
    /// </summary>
    public void UpdateFinalStates(UnitState attackerStateAfter, UnitState defenderStateAfter)
    {
        AttackerStateAfter = attackerStateAfter;
        DefenderStateAfter = defenderStateAfter;
    }

    public override string GetNotation()
    {
        string playerName = Player == PlayerType.Rome ? "Rome" : "Carthage";
        int attackerDamage = AttackerHealthBefore - AttackerHealthAfter;
        int defenderDamage = DefenderHealthBefore - DefenderHealthAfter;

        return $"{playerName} attacks with {AttackerType} at ({AttackerPosition.Row},{AttackerPosition.Column}) " +
               $"against {DefenderType} at ({DefenderPosition.Row},{DefenderPosition.Column}). " +
               $"Results: {AttackerType} {attackerDamage} damage, {DefenderType} {defenderDamage} damage";
    }

    public override bool Execute(Board board)
    {
        // Find attacker and defender
        var attackerTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == AttackerPosition.Row && t.Position.Column == AttackerPosition.Column);
        var defenderTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == DefenderPosition.Row && t.Position.Column == DefenderPosition.Column);

        if (attackerTile?.Unit == null || defenderTile?.Unit == null)
            return false;

        // Apply the damage
        attackerTile.Unit.Health.SetHealth(AttackerHealthAfter);
        defenderTile.Unit.Health.SetHealth(DefenderHealthAfter);

        // Restore the states after combat
        attackerTile.Unit.State = AttackerStateAfter;
        defenderTile.Unit.State = DefenderStateAfter;

        System.Diagnostics.Debug.WriteLine($"[CombatAction.Execute] Set {attackerTile.Unit.Type} state to {AttackerStateAfter}");
        System.Diagnostics.Debug.WriteLine($"[CombatAction.Execute] Set {defenderTile.Unit.Type} state to {DefenderStateAfter}");

        // Lock the units that were recorded as locked by this attack (during redo/load)
        if (LockedUnits.Count > 0)
        {
            foreach (var kvp in LockedUnits)
            {
                string key = kvp.Key;
                var parts = key.Split('-');
                PlayerType faction = Enum.Parse<PlayerType>(parts[0]);
                UnitType unitType = Enum.Parse<UnitType>(parts[1]);

                var lockedUnit = board.Units.FirstOrDefault(u => u.Faction == faction && u.Type == unitType);
                if (lockedUnit != null && lockedUnit != attackerTile.Unit)
                {
                    lockedUnit.StateBeforeLocked = kvp.Value;  // Set previous state for EndTurnAction to restore
                    lockedUnit.State = UnitState.Locked;
                    System.Diagnostics.Debug.WriteLine($"[CombatAction.Execute] Locked {lockedUnit.Type} ({lockedUnit.Faction}) from {kvp.Value} (replay)");
                }
            }
        }

        return true;
    }

    public override bool Undo(Board board)
    {
        // Find tiles
        var attackerTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == AttackerPosition.Row && t.Position.Column == AttackerPosition.Column);
        var defenderTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == DefenderPosition.Row && t.Position.Column == DefenderPosition.Column);

        if (attackerTile == null || defenderTile == null)
        {
            System.Diagnostics.Debug.WriteLine("[CombatAction.Undo] Failed: Could not find tiles");
            return false;
        }

        // Restore attacker
        if (attackerTile.Unit != null)
        {
            // Attacker is still alive, just restore health and state
            attackerTile.Unit.Health.SetHealth(AttackerHealthBefore);
            attackerTile.Unit.State = AttackerStateBefore;
            System.Diagnostics.Debug.WriteLine($"[CombatAction.Undo] Restored alive attacker health to {AttackerHealthBefore}");
        }
        else
        {
            // Attacker was eliminated, resurrect it
            // Use _attacker if available (runtime), otherwise find by Faction+Type (after deserialization)
            var attackerUnit = _attacker ?? board.Units.FirstOrDefault(u => u.Faction == AttackerFaction && u.Type == AttackerType);
            if (attackerUnit != null)
            {
                // Step 1: Restore health first (so unit is no longer "dead")
                attackerUnit.Health.SetHealth(AttackerHealthBefore);

                // Step 2: Place unit back on the tile BEFORE changing state
                // This ensures the unit has a valid tile reference
                attackerUnit.Tile = attackerTile;
                attackerUnit.Position = attackerTile.Position;
                attackerTile.Unit = attackerUnit;
                attackerTile.Occupied = true;

                // Step 3: Change state last (after unit is properly positioned)
                attackerUnit.State = AttackerStateBefore;
                System.Diagnostics.Debug.WriteLine($"[CombatAction.Undo] Resurrected attacker with health {AttackerHealthBefore}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CombatAction.Undo] Warning: Could not find attacker unit for resurrection");
            }
        }

        // Restore defender
        if (defenderTile.Unit != null)
        {
            // Defender is still alive, just restore health and state
            defenderTile.Unit.Health.SetHealth(DefenderHealthBefore);
            defenderTile.Unit.State = DefenderStateBefore;
            System.Diagnostics.Debug.WriteLine($"[CombatAction.Undo] Restored alive defender health to {DefenderHealthBefore}");
        }
        else
        {
            // Defender was eliminated, resurrect it
            // Use _defender if available (runtime), otherwise find by Faction+Type (after deserialization)
            var defenderUnit = _defender ?? board.Units.FirstOrDefault(u => u.Faction == DefenderFaction && u.Type == DefenderType);
            if (defenderUnit != null)
            {
                // Step 1: Restore health first (so unit is no longer "dead")
                defenderUnit.Health.SetHealth(DefenderHealthBefore);

                // Step 2: Place unit back on the tile BEFORE changing state
                // This ensures the unit has a valid tile reference
                defenderUnit.Tile = defenderTile;
                defenderUnit.Position = defenderTile.Position;
                defenderTile.Unit = defenderUnit;
                defenderTile.Occupied = true;

                // Step 3: Change state last (after unit is properly positioned)
                defenderUnit.State = DefenderStateBefore;
                System.Diagnostics.Debug.WriteLine($"[CombatAction.Undo] Resurrected defender with health {DefenderHealthBefore}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CombatAction.Undo] Warning: Could not find defender unit for resurrection");
            }
        }

        // Unlock units that were locked by this attack and restore their previous state
        foreach (var kvp in LockedUnits)
        {
            string key = kvp.Key;
            UnitState previousState = kvp.Value;
            var parts = key.Split('-');
            PlayerType faction = Enum.Parse<PlayerType>(parts[0]);
            UnitType unitType = Enum.Parse<UnitType>(parts[1]);

            var lockedUnit = board.Units.FirstOrDefault(u => u.Faction == faction && u.Type == unitType);
            if (lockedUnit != null && lockedUnit.State == UnitState.Locked)
            {
                lockedUnit.State = previousState;  // Restore to Idle or Ready
                System.Diagnostics.Debug.WriteLine($"[CombatAction.Undo] Unlocked {lockedUnit.Type} ({lockedUnit.Faction}) to {previousState}");
            }
        }

        return true;
    }
}
