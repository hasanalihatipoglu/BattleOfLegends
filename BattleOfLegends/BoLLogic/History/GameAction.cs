namespace BoLLogic;

/// <summary>
/// Base class for all reversible game actions in the history system.
/// Each action knows how to execute itself and undo itself.
/// </summary>
public abstract class GameAction
{
    /// <summary>
    /// The player who performed this action
    /// </summary>
    public PlayerType Player { get; set; }

    /// <summary>
    /// Timestamp when the action was performed
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Chess-like notation description of this action
    /// Example: "Rome Player moves Equites from (2,2) to (2,1)"
    /// </summary>
    public abstract string GetNotation();

    /// <summary>
    /// Execute this action (for redo)
    /// </summary>
    public abstract bool Execute(Board board);

    /// <summary>
    /// Undo this action
    /// </summary>
    public abstract bool Undo(Board board);

    protected GameAction(PlayerType player)
    {
        Player = player;
        Timestamp = DateTime.Now;
    }
}
