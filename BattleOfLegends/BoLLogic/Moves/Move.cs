namespace BoLLogic;

public abstract class Move
{
    public abstract MoveType Type { get; }

    public abstract Path MovePath { get; }

    public abstract bool Execute();

}


