namespace BoLLogic;

/// <summary>
/// Represents a combat action including attack declaration and results
/// </summary>
public class CombatAction : GameAction
{
    public UnitType AttackerType { get; set; }
    public Position AttackerPosition { get; set; }
    public Position AttackerPositionAfter { get; set; }  // Final position after combat (may have retreated)
    public UnitType DefenderType { get; set; }
    public Position DefenderPosition { get; set; }
    public Position DefenderPositionAfter { get; set; }  // Final position after combat (may have retreated)
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
        AttackerPositionAfter = attacker.Position;  // Will be updated by UpdateFinalStates
        DefenderType = defender.Type;
        DefenderFaction = defender.Faction;
        DefenderPosition = defender.Position;
        DefenderPositionAfter = defender.Position;  // Will be updated by UpdateFinalStates
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
    /// Update the final states and positions after combat is complete (including any retreats)
    /// </summary>
    public void UpdateFinalStates(Unit attacker, Unit defender)
    {
        AttackerStateAfter = attacker.State;
        DefenderStateAfter = defender.State;
        AttackerPositionAfter = new Position(attacker.Position.Row, attacker.Position.Column);
        DefenderPositionAfter = new Position(defender.Position.Row, defender.Position.Column);
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
        // Find attacker and defender at original combat positions
        var attackerTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == AttackerPosition.Row && t.Position.Column == AttackerPosition.Column);
        var defenderTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == DefenderPosition.Row && t.Position.Column == DefenderPosition.Column);

        if (attackerTile?.Unit == null || defenderTile?.Unit == null)
            return false;

        var attacker = attackerTile.Unit;
        var defender = defenderTile.Unit;

        // Apply the damage
        attacker.Health.SetHealth(AttackerHealthAfter);
        defender.Health.SetHealth(DefenderHealthAfter);

        // Move attacker to final position if it retreated
        if (AttackerPositionAfter.Row != AttackerPosition.Row || AttackerPositionAfter.Column != AttackerPosition.Column)
        {
            var attackerFinalTile = board.Tiles.FirstOrDefault(t =>
                t.Position.Row == AttackerPositionAfter.Row && t.Position.Column == AttackerPositionAfter.Column);

            if (attackerFinalTile != null)
            {
                // Clear original tile
                attackerTile.Unit = null;
                attackerTile.Occupied = false;

                // Place unit at final position
                attackerFinalTile.Unit = attacker;
                attackerFinalTile.Occupied = true;
                attacker.Tile = attackerFinalTile;
                attacker.Position = attackerFinalTile.Position;
                System.Diagnostics.Debug.WriteLine($"[CombatAction.Execute] Moved {attacker.Type} from ({AttackerPosition.Row},{AttackerPosition.Column}) to ({AttackerPositionAfter.Row},{AttackerPositionAfter.Column})");
            }
        }

        // Move defender to final position if it retreated
        if (DefenderPositionAfter.Row != DefenderPosition.Row || DefenderPositionAfter.Column != DefenderPosition.Column)
        {
            var defenderFinalTile = board.Tiles.FirstOrDefault(t =>
                t.Position.Row == DefenderPositionAfter.Row && t.Position.Column == DefenderPositionAfter.Column);

            if (defenderFinalTile != null)
            {
                // Clear original tile
                defenderTile.Unit = null;
                defenderTile.Occupied = false;

                // Place unit at final position
                defenderFinalTile.Unit = defender;
                defenderFinalTile.Occupied = true;
                defender.Tile = defenderFinalTile;
                defender.Position = defenderFinalTile.Position;
                System.Diagnostics.Debug.WriteLine($"[CombatAction.Execute] Moved {defender.Type} from ({DefenderPosition.Row},{DefenderPosition.Column}) to ({DefenderPositionAfter.Row},{DefenderPositionAfter.Column})");
            }
        }

        // Restore the states after combat and movement
        attacker.State = AttackerStateAfter;
        defender.State = DefenderStateAfter;

        System.Diagnostics.Debug.WriteLine($"[CombatAction.Execute] Set {attacker.Type} state to {AttackerStateAfter}");
        System.Diagnostics.Debug.WriteLine($"[CombatAction.Execute] Set {defender.Type} state to {DefenderStateAfter}");

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
        // Find original tiles (where combat happened)
        var attackerOriginalTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == AttackerPosition.Row && t.Position.Column == AttackerPosition.Column);
        var defenderOriginalTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == DefenderPosition.Row && t.Position.Column == DefenderPosition.Column);

        if (attackerOriginalTile == null || defenderOriginalTile == null)
        {
            System.Diagnostics.Debug.WriteLine("[CombatAction.Undo] Failed: Could not find original tiles");
            return false;
        }

        // Find attacker at its FINAL position (after any retreat)
        var attackerFinalTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == AttackerPositionAfter.Row && t.Position.Column == AttackerPositionAfter.Column);

        Unit attacker = null;
        if (attackerFinalTile?.Unit != null)
        {
            // Attacker is still alive at final position
            attacker = attackerFinalTile.Unit;
        }
        else
        {
            // Attacker was eliminated, find by Faction+Type
            attacker = _attacker ?? board.Units.FirstOrDefault(u => u.Faction == AttackerFaction && u.Type == AttackerType);
        }

        if (attacker != null)
        {
            // Restore health first
            attacker.Health.SetHealth(AttackerHealthBefore);

            // Move attacker back to original position if it had retreated
            if (attackerFinalTile != null && attackerFinalTile != attackerOriginalTile)
            {
                // Clear final tile
                attackerFinalTile.Unit = null;
                attackerFinalTile.Occupied = false;
                System.Diagnostics.Debug.WriteLine($"[CombatAction.Undo] Moving {attacker.Type} from ({AttackerPositionAfter.Row},{AttackerPositionAfter.Column}) back to ({AttackerPosition.Row},{AttackerPosition.Column})");
            }

            // Place unit at original position
            attackerOriginalTile.Unit = attacker;
            attackerOriginalTile.Occupied = true;
            attacker.Tile = attackerOriginalTile;
            attacker.Position = attackerOriginalTile.Position;

            // Restore state last
            attacker.State = AttackerStateBefore;
            System.Diagnostics.Debug.WriteLine($"[CombatAction.Undo] Restored attacker health to {AttackerHealthBefore} and state to {AttackerStateBefore}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[CombatAction.Undo] Warning: Could not find attacker unit");
        }

        // Find defender at its FINAL position (after any retreat)
        var defenderFinalTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == DefenderPositionAfter.Row && t.Position.Column == DefenderPositionAfter.Column);

        Unit defender = null;
        if (defenderFinalTile?.Unit != null)
        {
            // Defender is still alive at final position
            defender = defenderFinalTile.Unit;
        }
        else
        {
            // Defender was eliminated, find by Faction+Type
            defender = _defender ?? board.Units.FirstOrDefault(u => u.Faction == DefenderFaction && u.Type == DefenderType);
        }

        if (defender != null)
        {
            // Restore health first
            defender.Health.SetHealth(DefenderHealthBefore);

            // Move defender back to original position if it had retreated
            if (defenderFinalTile != null && defenderFinalTile != defenderOriginalTile)
            {
                // Clear final tile
                defenderFinalTile.Unit = null;
                defenderFinalTile.Occupied = false;
                System.Diagnostics.Debug.WriteLine($"[CombatAction.Undo] Moving {defender.Type} from ({DefenderPositionAfter.Row},{DefenderPositionAfter.Column}) back to ({DefenderPosition.Row},{DefenderPosition.Column})");
            }

            // Place unit at original position
            defenderOriginalTile.Unit = defender;
            defenderOriginalTile.Occupied = true;
            defender.Tile = defenderOriginalTile;
            defender.Position = defenderOriginalTile.Position;

            // Restore state last
            defender.State = DefenderStateBefore;
            System.Diagnostics.Debug.WriteLine($"[CombatAction.Undo] Restored defender health to {DefenderHealthBefore} and state to {DefenderStateBefore}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[CombatAction.Undo] Warning: Could not find defender unit");
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
