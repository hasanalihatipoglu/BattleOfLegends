using System.Text.Json.Serialization;

namespace BoLLogic;

/// <summary>
/// Base class for all reversible game actions in the history system.
/// Each action knows how to execute itself and undo itself.
/// </summary>
[JsonDerivedType(typeof(UnitMoveAction), typeDiscriminator: "UnitMove")]
[JsonDerivedType(typeof(CombatAction), typeDiscriminator: "Combat")]
[JsonDerivedType(typeof(EndTurnAction), typeDiscriminator: "EndTurn")]
[JsonDerivedType(typeof(CardPlayAction), typeDiscriminator: "CardPlay")]
[JsonDerivedType(typeof(PlayerChangeAction), typeDiscriminator: "PlayerChange")]
[JsonDerivedType(typeof(MoraleChangeAction), typeDiscriminator: "MoraleChange")]
[JsonDerivedType(typeof(ActionValueChangeAction), typeDiscriminator: "ActionValueChange")]
[JsonDerivedType(typeof(PhaseChangeAction), typeDiscriminator: "PhaseChange")]
[JsonDerivedType(typeof(GamePhaseChangeAction), typeDiscriminator: "GamePhaseChange")]
[JsonDerivedType(typeof(GameRoundChangeAction), typeDiscriminator: "GameRoundChange")]
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

    // Parameterless constructor for JSON deserialization
    protected GameAction()
    {
        Timestamp = DateTime.Now;
    }
}
