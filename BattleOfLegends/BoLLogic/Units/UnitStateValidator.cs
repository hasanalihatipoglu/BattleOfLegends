namespace BoLLogic;

/// <summary>
/// Validates unit state transitions to prevent invalid states
/// </summary>
public static class UnitStateValidator
{
    /// <summary>
    /// Checks if a state transition is valid
    /// </summary>
    public static bool IsValidTransition(UnitState currentState, UnitState newState)
    {
        // Dead units cannot transition to any other state
        if (currentState == UnitState.Dead)
            return newState == UnitState.Dead;

        // Any state can transition to Dead
        if (newState == UnitState.Dead)
            return true;

        // Cannot transition to None
        if (newState == UnitState.None)
            return false;

        // Define valid transitions
        return (currentState, newState) switch
        {
            // From Idle
            (UnitState.Idle, UnitState.Active) => true,
            (UnitState.Idle, UnitState.Defending) => true,
            (UnitState.Idle, UnitState.Locked) => true,

            // From Active
            (UnitState.Active, UnitState.Moved) => true,
            (UnitState.Active, UnitState.Marched) => true,
            (UnitState.Active, UnitState.Attacking) => true,
            (UnitState.Active, UnitState.Idle) => true,
            (UnitState.Active, UnitState.Passive) => true,
            (UnitState.Active, UnitState.Locked) => true,  // Locked when another unit acts during ordered activation
            (UnitState.Active, UnitState.Ordered) => true,  // Leader returns to Ordered when deselected

            // From Moved
            (UnitState.Moved, UnitState.Attacked) => true,
            (UnitState.Moved, UnitState.Attacking) => true,
            (UnitState.Moved, UnitState.Passive) => true,
            (UnitState.Moved, UnitState.Defending) => true,
            (UnitState.Moved, UnitState.Advancing) => true,  // After move+attack, can advance if eligible

            // From Marched
            (UnitState.Marched, UnitState.Passive) => true,
            (UnitState.Marched, UnitState.Defending) => true,

            // From Attacked
            (UnitState.Attacked, UnitState.Passive) => true,
            (UnitState.Attacked, UnitState.Advancing) => true,
            (UnitState.Attacked, UnitState.Retreating) => true,
            (UnitState.Attacked, UnitState.Defending) => true,

            // From Attacking
            (UnitState.Attacking, UnitState.Attacked) => true,
            (UnitState.Attacking, UnitState.Idle) => true,
            (UnitState.Attacking, UnitState.Moved) => true,
            (UnitState.Attacking, UnitState.Marched) => true,

            // From Defending
            (UnitState.Defending, UnitState.Retreating) => true,
            (UnitState.Defending, UnitState.Idle) => true,
            (UnitState.Defending, UnitState.Moved) => true,
            (UnitState.Defending, UnitState.Marched) => true,
            (UnitState.Defending, UnitState.Attacked) => true,

            // From Retreating
            (UnitState.Retreating, UnitState.Retreated) => true,
            (UnitState.Retreating, UnitState.PushedBack) => true,

            // From Retreated (forced retreat during opponent's turn - can act on own turn)
            (UnitState.Retreated, UnitState.Active) => true,  // Can be activated on own turn
            (UnitState.Retreated, UnitState.Idle) => true,  // Reset at round/turn changes
            (UnitState.Retreated, UnitState.Passive) => true,  // If clicked, becomes passive

            // From PushedBack (retreat during own turn after counter-attack - already spent action)
            (UnitState.PushedBack, UnitState.Passive) => true,
            (UnitState.PushedBack, UnitState.Idle) => true,  // Reset at round/turn changes

            // From Advancing
            (UnitState.Advancing, UnitState.Advanced) => true,

            // From Advanced
            (UnitState.Advanced, UnitState.Passive) => true,
            (UnitState.Advanced, UnitState.Attacked) => true,

            // From Passive - can reset to Idle at turn end
            (UnitState.Passive, UnitState.Idle) => true,
            (UnitState.Passive, UnitState.Defending) => true,

            // From Ready
            (UnitState.Ready, UnitState.Active) => true,
            (UnitState.Ready, UnitState.Idle) => true,
            (UnitState.Ready, UnitState.Locked) => true,  // Locked when another unit acts

            // From Locked
            (UnitState.Locked, UnitState.Idle) => true,  // Reset at turn end
            (UnitState.Locked, UnitState.Ready) => true,  // Restored when action is undone
            (UnitState.Locked, UnitState.Active) => true,  // Restored when action is undone (during ordered activation)
            (UnitState.Locked, UnitState.Defending) => true,  // Can be attacked by opponent

            // From Ordered (leader has given an order)
            (UnitState.Ordered, UnitState.Ready) => true,  // Leader orders itself
            (UnitState.Ordered, UnitState.Active) => true,  // Leader is activated in Turn phase
            (UnitState.Ordered, UnitState.Idle) => true,  // Reset at turn/round end
            (UnitState.Ordered, UnitState.Defending) => true,  // Can be attacked
            (UnitState.Ordered, UnitState.Passive) => true,  // No actions left (max action reached)

            // To Ordered
            (UnitState.Idle, UnitState.Ordered) => true,  // Leader plays an order card
            (UnitState.Ready, UnitState.Ordered) => true,  // Leader removes self from order

            // Default - invalid transition
            _ => false
        };
    }

    /// <summary>
    /// Gets a human-readable reason why a transition is invalid
    /// </summary>
    public static string GetTransitionError(UnitState currentState, UnitState newState)
    {
        if (currentState == UnitState.Dead && newState != UnitState.Dead)
            return $"Dead units cannot transition from {currentState} to {newState}";

        if (newState == UnitState.None)
            return "Cannot transition to None state";

        return $"Invalid state transition from {currentState} to {newState}";
    }
}
