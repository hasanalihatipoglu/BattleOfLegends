namespace BoLLogic;

/// <summary>
/// Represents a combat action including attack declaration and results
/// </summary>
public class CombatAction : GameAction
{
    public UnitType AttackerType { get; private set; }
    public Position AttackerPosition { get; private set; }
    public UnitType DefenderType { get; private set; }
    public Position DefenderPosition { get; private set; }
    public int AttackerHealthBefore { get; private set; }
    public int DefenderHealthBefore { get; private set; }
    public int AttackerHealthAfter { get; private set; }
    public int DefenderHealthAfter { get; private set; }

    public CombatAction(PlayerType player, Unit attacker, Unit defender,
        int attackerHealthBefore, int defenderHealthBefore,
        int attackerHealthAfter, int defenderHealthAfter)
        : base(player)
    {
        AttackerType = attacker.Type;
        AttackerPosition = attacker.Position;
        DefenderType = defender.Type;
        DefenderPosition = defender.Position;
        AttackerHealthBefore = attackerHealthBefore;
        DefenderHealthBefore = defenderHealthBefore;
        AttackerHealthAfter = attackerHealthAfter;
        DefenderHealthAfter = defenderHealthAfter;
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

        return true;
    }

    public override bool Undo(Board board)
    {
        // Find attacker and defender
        var attackerTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == AttackerPosition.Row && t.Position.Column == AttackerPosition.Column);
        var defenderTile = board.Tiles.FirstOrDefault(t =>
            t.Position.Row == DefenderPosition.Row && t.Position.Column == DefenderPosition.Column);

        if (attackerTile?.Unit == null || defenderTile?.Unit == null)
            return false;

        // Restore previous health
        attackerTile.Unit.Health.SetHealth(AttackerHealthBefore);
        defenderTile.Unit.Health.SetHealth(DefenderHealthBefore);

        return true;
    }
}
