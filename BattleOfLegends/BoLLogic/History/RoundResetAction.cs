namespace BoLLogic;

/// <summary>
/// Represents the automatic state resets that happen at the start of certain rounds
/// </summary>
public class RoundResetAction : GameAction
{
    public int Round { get; set; }
    public bool ResetUnitStates { get; set; }
    public bool ResetActionValues { get; set; }

    // Store previous states for undo (keyed by position to handle multiple units of same type)
    public Dictionary<string, UnitState> PreviousUnitStates { get; set; } = new Dictionary<string, UnitState>();
    public Dictionary<PlayerType, int> PreviousActionValues { get; set; } = new Dictionary<PlayerType, int>();
    public PlayerType PreviousPlayer { get; set; }

    public RoundResetAction(PlayerType player, int round, bool resetUnitStates, bool resetActionValues)
        : base(player)
    {
        Round = round;
        ResetUnitStates = resetUnitStates;
        ResetActionValues = resetActionValues;
        PreviousPlayer = TurnManager.Instance.CurrentPlayer;
    }

    // Parameterless constructor for JSON deserialization
    public RoundResetAction() : base() { }

    public override string GetNotation()
    {
        return $"Round {Round} begins - states reset";
    }

    public override bool Execute(Board board)
    {
        System.Diagnostics.Debug.WriteLine($"[RoundResetAction.Execute] Round {Round}, ResetUnitStates={ResetUnitStates}, PreviousUnitStates.Count={PreviousUnitStates.Count}");

        // Save previous states before resetting (only on first execution)
        if (ResetUnitStates && PreviousUnitStates.Count == 0)
        {
            foreach (Unit unit in board.Units)
            {
                string key = $"{unit.Position.Row},{unit.Position.Column}";
                PreviousUnitStates[key] = unit.State;
                System.Diagnostics.Debug.WriteLine($"[RoundResetAction.Execute] Saved {unit.Type} at {key}: {unit.State}");
            }
        }

        if (ResetActionValues && PreviousActionValues.Count == 0)
        {
            foreach (Player player in board.Players)
            {
                PreviousActionValues[player.Type] = player.Action.ActionValue;
            }
        }

        // Apply resets
        if (ResetUnitStates)
        {
            System.Diagnostics.Debug.WriteLine($"[RoundResetAction.Execute] Resetting all units to Idle");
            foreach (Unit unit in board.Units)
            {
                System.Diagnostics.Debug.WriteLine($"[RoundResetAction.Execute] Setting {unit.Type} from {unit.State} to Idle");
                unit.State = UnitState.Idle;
            }
        }

        if (ResetActionValues)
        {
            foreach (Player player in board.Players)
            {
                player.Action.ActionValue = 0;
            }
        }

        // Set current player based on round number (every 2 rounds, player switches)
        // Rounds 1-2: InitialPlayer, Rounds 3-4: Opponent, etc.
        if (Round % 4 == 1 || Round % 4 == 2)
        {
            TurnManager.Instance.CurrentPlayer = board.InitialPlayer;
        }
        else
        {
            TurnManager.Instance.CurrentPlayer = board.InitialPlayer.Opponent();
        }

        return true;
    }

    public override bool Undo(Board board)
    {
        // Restore previous unit states
        if (ResetUnitStates)
        {
            foreach (Unit unit in board.Units)
            {
                string key = $"{unit.Position.Row},{unit.Position.Column}";
                if (PreviousUnitStates.TryGetValue(key, out UnitState previousState))
                {
                    unit.State = previousState;
                    System.Diagnostics.Debug.WriteLine($"[RoundResetAction.Undo] Restored {unit.Type} at {key} state to {previousState}");
                }
            }
        }

        // Restore previous action values
        if (ResetActionValues)
        {
            foreach (Player player in board.Players)
            {
                if (PreviousActionValues.TryGetValue(player.Type, out int previousValue))
                {
                    player.Action.ActionValue = previousValue;
                }
            }
        }

        // Restore previous player
        TurnManager.Instance.CurrentPlayer = PreviousPlayer;

        return true;
    }
}
