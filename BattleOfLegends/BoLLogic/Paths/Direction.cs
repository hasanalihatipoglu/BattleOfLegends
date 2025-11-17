namespace BoLLogic;

public class Direction
{
    public readonly static Direction OddRowUpperLeft = new Direction(-1,0);
    public readonly static Direction OddRowUpperRight = new Direction(-1,1);
    public readonly static Direction OddRowLowerLeft = new Direction(+1, 0);
    public readonly static Direction OddRowLowerRight = new Direction(+1,+1);
    public readonly static Direction OddRowLeft = new Direction(0, -1);
    public readonly static Direction OddRowRight = new Direction(0, +1);

    public readonly static Direction EvenRowUpperLeft = new Direction(-1, -1);
    public readonly static Direction EvenRowUpperRight = new Direction(-1, 0);
    public readonly static Direction EvenRowLowerLeft = new Direction(+1, -1);
    public readonly static Direction EvenRowLowerRight = new Direction(+1, 0);
    public readonly static Direction EvenRowLeft = new Direction(0, -1);
    public readonly static Direction EvenRowRight = new Direction(0, +1);

    public readonly static Direction None = new Direction(0, 0);


    public int RowDelta {  get;  }
    public int ColumnDelta { get;  }

    public Direction(int rowDelta, int columnDelta)
    { 
        RowDelta = rowDelta; 
        ColumnDelta = columnDelta; 
    }

    public static Direction operator + (Direction dir1, Direction dir2) 
    { 
        return new Direction(dir1.RowDelta + dir2.RowDelta, dir1.ColumnDelta + dir2.ColumnDelta);
    }

    public static Direction operator * (int scalar, Direction dir) 
    { 
        return new Direction(scalar * dir.RowDelta, scalar * dir.ColumnDelta);
    }

}
