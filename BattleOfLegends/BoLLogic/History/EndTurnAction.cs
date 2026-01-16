namespace BoLLogic;

/// <summary>
/// Represents an END TURN action that increases action value, switches player, and resets phase
/// </summary>
public class EndTurnAction : GameAction
{
    public PlayerType PreviousPlayer { get; set; }
    public PlayerType NewPlayer { get; set; }
    public int PreviousActionValue { get; set; }
    public int NewActionValue { get; set; }
    public TurnPhase PreviousPhase { get; set; }

    // Store unit states that need to be reset (Retreated/Advancing units)
    // Key format: "Faction-UnitType" to handle position changes during retreat
    public Dictionary<string, (UnitState CurrentState, UnitState TargetState)> UnitStatesToReset { get; set; } = new Dictionary<string, (UnitState, UnitState)>();

    public EndTurnAction(PlayerType previousPlayer, PlayerType newPlayer, int previousActionValue)
        : base(previousPlayer) // The player ending their turn
    {
        PreviousPlayer = previousPlayer;
        NewPlayer = newPlayer;
        PreviousActionValue = previousActionValue;
        NewActionValue = previousActionValue + 1;
        PreviousPhase = TurnManager.Instance.CurrentTurnPhase; // Store current phase for undo
    }

    // Parameterless constructor for JSON deserialization
    public EndTurnAction() : base() { }

    public override string GetNotation()
    {
        string playerName = PreviousPlayer == PlayerType.Rome ? "Rome" : "Carthage";
        return $"{playerName} ends turn (Action: {PreviousActionValue} -> {NewActionValue})";
    }

    public override bool Execute(Board board)
    {
        // Save and reset unit states that are Retreated or Advancing (only on first execution)
        if (UnitStatesToReset.Count == 0)
        {
            foreach (var unit in board.Units)
            {
                if (unit.State == UnitState.Retreated || unit.State == UnitState.Advancing)
                {
                    // Use Faction-UnitType as key since position may change during retreat
                    string key = $"{unit.Faction}-{unit.Type}";
                    // Use StateBeforeCombat to get the state before combat started (not PreviousState which might be Defending/Attacking)
                    UnitState targetState = unit.StateBeforeCombat;

                    UnitStatesToReset[key] = (unit.State, targetState);

                    // Reset Retreated/Advancing units to their state before combat
                    unit.State = targetState;
                    System.Diagnostics.Debug.WriteLine($"[EndTurnAction] Reset {unit.Type} ({unit.Faction}) from {UnitStatesToReset[key].CurrentState} to {targetState}");
                }
            }
        }
        else
        {
            // Subsequent execution (during load) - apply the saved resets
            foreach (var kvp in UnitStatesToReset)
            {
                // Key format: "Faction-UnitType"
                var parts = kvp.Key.Split('-');
                PlayerType faction = Enum.Parse<PlayerType>(parts[0]);
                UnitType unitType = Enum.Parse<UnitType>(parts[1]);

                var unit = board.Units.FirstOrDefault(u => u.Faction == faction && u.Type == unitType);
                if (unit != null)
                {
                    // Reset to the saved target state
                    unit.State = kvp.Value.TargetState;
                    System.Diagnostics.Debug.WriteLine($"[EndTurnAction] Reset {unit.Type} ({unit.Faction}) from {kvp.Value.CurrentState} to {kvp.Value.TargetState} (replay)");
                }
            }
        }

        // Increase action for the previous player
        var player = board.Players.FirstOrDefault(p => p.Type == PreviousPlayer);
        if (player != null)
        {
            player.Action.ActionValue = NewActionValue;
        }

        // Switch to new player
        TurnManager.Instance.CurrentPlayer = NewPlayer;
        TurnManager.Instance.CurrentTurn = NewPlayer; // Update whose turn it is

        // Reset turn phase to Move
        TurnManager.Instance.CurrentTurnPhase = TurnPhase.Move;

        // Trigger events
        TurnManager.Instance.TriggerPlayerChangeEvent();
        TurnManager.Instance.AdvanceTurnPhase(); // Trigger phase change event

        return true;
    }

    public override bool Undo(Board board)
    {
        // Restore unit states that were reset (Retreated/Advancing)
        foreach (var kvp in UnitStatesToReset)
        {
            // Key format: "Faction-UnitType"
            var parts = kvp.Key.Split('-');
            PlayerType faction = Enum.Parse<PlayerType>(parts[0]);
            UnitType unitType = Enum.Parse<UnitType>(parts[1]);

            var unit = board.Units.FirstOrDefault(u => u.Faction == faction && u.Type == unitType);
            if (unit != null)
            {
                // Restore to Retreated/Advancing state
                unit.State = kvp.Value.CurrentState;
                System.Diagnostics.Debug.WriteLine($"[EndTurnAction.Undo] Restored {unit.Type} ({unit.Faction}) to {kvp.Value.CurrentState}");
            }
        }

        // Switch back to previous player
        TurnManager.Instance.CurrentPlayer = PreviousPlayer;
        TurnManager.Instance.CurrentTurn = PreviousPlayer; // Restore whose turn it is

        // Restore previous phase
        TurnManager.Instance.CurrentTurnPhase = PreviousPhase;

        // Trigger events
        TurnManager.Instance.TriggerPlayerChangeEvent();
        TurnManager.Instance.AdvanceTurnPhase(); // Trigger phase change event

        // Decrease action back to previous value
        var player = board.Players.FirstOrDefault(p => p.Type == PreviousPlayer);
        if (player != null)
        {
            player.Action.ActionValue = PreviousActionValue;
        }

        return true;
    }
}
