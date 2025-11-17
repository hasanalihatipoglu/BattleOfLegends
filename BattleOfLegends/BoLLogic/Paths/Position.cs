namespace BoLLogic;

public readonly struct Position(int row, int column)
{ 
    public int Row { get; } = row;
    public int Column { get; } = column;



    public override bool Equals(object obj)
    {
        return obj is Position position &&
               Row == position.Row &&
               Column == position.Column;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Column);
    }


    public static Direction GetDirection(Position from, Position to)
    {
        int rowDiff = to.Row - from.Row;
        int colDiff= to.Column - from.Column;
        bool isEvenRow = (to.Row % 2 == 0);


        if (!isEvenRow)
        {
            if (rowDiff == 0 && colDiff == 1) return Direction.EvenRowRight;
            if (rowDiff == 1 && colDiff == 1) return Direction.EvenRowLowerRight;
            if (rowDiff == 1 && colDiff == 0) return Direction.EvenRowLowerLeft;
            if (rowDiff == 0 && colDiff == -1) return Direction.EvenRowLeft;
            if (rowDiff == -1 && colDiff == 0) return Direction.EvenRowUpperLeft;
            if (rowDiff == -1 && colDiff == 1) return Direction.EvenRowUpperRight;
        }
        else
        {
            if (rowDiff == 0 && colDiff == 1) return Direction.OddRowRight;
            if (rowDiff == 1 && colDiff == 0) return Direction.OddRowLowerRight;
            if (rowDiff == 1 && colDiff == -1) return Direction.OddRowLowerLeft;
            if (rowDiff == 0 && colDiff == -1) return Direction.OddRowLeft;
            if (rowDiff == 1 && colDiff == 1) return Direction.OddRowUpperLeft;
            if (rowDiff == -1 && colDiff == 0) return Direction.OddRowUpperRight;
        }

        return Direction.None;

    }

    public static bool operator ==(Position left, Position right)
    {
        return EqualityComparer<Position>.Default.Equals(left, right);
    }

    public static bool operator !=(Position left, Position right)
    {
        return !(left == right);
    }

    public static Position operator +(Position pos, Direction dir) 
    { 
        return new Position(pos.Row + dir.RowDelta, pos.Column + dir.ColumnDelta);
    }

    
    public static Position operator -(Position a, Position b)
    {
        int rowDiff = a.Row - b.Row;
        int colDiff = a.Column - b.Column;

        bool isEvenRow = (a.Row % 2 == 0);
        if (!isEvenRow)
        {
            if (rowDiff == -1 && colDiff == 1) colDiff = 0; // Adjust UpRight for odd rows
            if (rowDiff == 1 && colDiff == -1) colDiff = 0; // Adjust DownLeft for odd rows
        }

        return new Position(rowDiff, colDiff);
    }

    public static Position operator +(Position a, Position b)
    {
        int newRow = a.Row + b.Row;
        int newCol = a.Column + b.Column;

        bool isEvenRow = (a.Row % 2 == 0);
        if (!isEvenRow)
        {
            if (b.Row == -1 && b.Column == 0) newCol++; // Adjust UpRight for odd rows
            if (b.Row == 1 && b.Column == 0) newCol--; // Adjust DownLeft for odd rows
        }

        return new Position(newRow, newCol);
    }


    public override string ToString()
    {
        return ($"{Row},{Column}"); 
    }

}


