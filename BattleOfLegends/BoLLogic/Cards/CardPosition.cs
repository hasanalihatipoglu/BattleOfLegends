namespace BoLLogic;

public readonly struct CardPosition(int x, int y)
{ 

    public int X { get; } = x;
    public int Y { get; } = y;

    public override bool Equals(object obj)
    {
        return obj is CardPosition position &&
               X == position.X &&
               Y == position.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator == (CardPosition left, CardPosition right)
    {
        return EqualityComparer<CardPosition>.Default.Equals(left, right);
    }

    public static bool operator != (CardPosition left, CardPosition right)
    {
        return !(left == right);
    }

    public static CardPosition operator + (CardPosition a, CardPosition b)
    {
        int newX = a.X + b.X;
        int newY = a.Y + b.Y;

        return new CardPosition(newX, newY);
    }

    public override string ToString()
    {
        return ($"{X},{Y}"); 
    }

}


