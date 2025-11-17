namespace BoLLogic;

public class ClickEventArgs(Position position, Board board) : EventArgs
{
    public Position Position { get; set; } = position;
    public Board GameState { get; set; } = board;

}


public class MessageEventArgs(string message) : EventArgs
{
    public string Message { get; set; } = message;
}


public class SoundEventArgs(string sound) : EventArgs
{
    public string Sound { get; set; } = sound;
}


public class StateChangedEventArgs(Unit unit, UnitState state
    //  , Position pos, GameState gameState, PathFinder pathFinder
    ) : EventArgs
{
    public Unit Unit { get; set; } = unit;
    public UnitState State { get; set; } = state;
  //  public Position Pos { get; set; } = pos;
  //  public GameState GameState { get; set; } = gameState;
  //  public PathFinder PathFinder { get; set; } = pathFinder;

}


public class MoraleEventArgs(PlayerType player, int amount) : EventArgs
{
    public PlayerType Faction { get; set; } = player;
    public int MoraleAmount { get; set; } = amount;
}


public class HandEventArgs(PlayerType player, int amount) : EventArgs
{
    public PlayerType Faction { get; set; } = player;
    public int HandAmount { get; set; } = amount;
}

public class ActionEventArgs(PlayerType player, int amount) : EventArgs
{
    public PlayerType Faction { get; set; } = player;
    public int ActionAmount { get; set; } = amount;
}

public class AttackEventArgs(Attack attack) : EventArgs
{
    public Attack attack { get; set; } = attack;
}