namespace BoLLogic;

public abstract class Tile
{
    public abstract TileType Type { get; }
    public Position Position { get; set; }
    public abstract bool Passable { get; }
    public abstract bool AttackPassable { get; }

    public Tile Parent { get; set; }
    public int Cost { get; set; }
    public Tile AttackParent { get; set; }
    public int AttackCost { get; set; }

    public Unit Unit { get; set; }
    public List<Tile> Adjacents { get; set; }

    public event EventHandler<ClickEventArgs> Click;
    public event EventHandler<ClickEventArgs> Hover;
    public event EventHandler<StateChangedEventArgs> ChangeUnitState;


    public bool Occupied { get; set; } = false;
    public bool CanBePassed { get { if (Unit == null) { return Passable; } else { return Passable && Unit.IsLight; }; } }
    public bool CanBeAttackPassed { get { if (Unit == null) { return AttackPassable; } else { return false; }; } }




    //--------------------------------------HOVER------------------------------------------------
    public void OnHover(Board board)
    {
        //HOVER HEX
        if (Unit == null && TurnManager.Instance.SelectedUnit != null)
        {
            //SELECTED UNIT
            switch (TurnManager.Instance.SelectedUnit.State)
            {
                case UnitState.Idle:
                    break;


                case UnitState.Active:
                case UnitState.Retreating:
                case UnitState.Advancing:
                case UnitState.Defending:
                    if (PathFinder.Instance.CurrentFrontier.Tiles.Contains(this))
                    {
                        PathFinder.Instance.AssignPath(PathFinder.Instance.CurrentSpaces[this], PathType.Move);
                    }
                    break;
            }
        }


        //HOVER UNIT
        else if (Unit != null && TurnManager.Instance.SelectedUnit != null)
        {
            Unit.OnHover(board);
        }

       // Hover?.Invoke(this, e);
    }




    //-------------------------------------CLICK---------------------------------------------------------
    public void OnClick(Board board)
    {

        //CLICK UNIT
        if (Unit != null)
        {

            Unit.OnClick(board);

        }


        //CLICK HEX
        else
        {
            if (TurnManager.Instance.SelectedUnit == null)
            {

                MessageController.Instance.Show("No active unit");

                return;
            }

            //SELECTED UNIT
            switch (TurnManager.Instance.SelectedUnit.State)
            {
                case UnitState.Idle:
                    break;

                case UnitState.Active:
                    if (PathFinder.Instance.CurrentSpaces.ContainsKey(this))
                    {

                        TurnManager.Instance.MakeMove(new NormalMove(PathFinder.Instance.CurrentSpaces[this]));

                        if (PathFinder.Instance.CurrentSpaces[this].TilesInPath.Count - 1 > TurnManager.Instance.SelectedUnit.AttackMove)
                        {
                            TurnManager.Instance.SelectedUnit.State = UnitState.Marched;
                            TurnManager.Instance.SelectedUnit = null;
                            PathFinder.Instance.ResetAll();
                          //  e.GameState.ChangeTurn();
                        }

                        else
                        {
                            TurnManager.Instance.SelectedUnit.State = UnitState.Moved;
                            PathFinder.Instance.Reset(PathType.Move);
                            PathFinder.Instance.FindPaths(TurnManager.Instance.SelectedUnit, this, PathType.Attack);


                            if (PathFinder.Instance.CurrentTargets.Count == 0)
                            {
                                TurnManager.Instance.SelectedUnit = null;
                             //   e.GameState.ChangeTurn();
                            }
                        }
                    }

                    else
                    {
                        TurnManager.Instance.SelectedUnit.State = UnitState.Idle;
                        TurnManager.Instance.SelectedUnit = null;
                        PathFinder.Instance.ResetAll();
                    }
                    break;

                case UnitState.Retreating:

                case UnitState.Defending:
                    if (PathFinder.Instance.CurrentSpaces.ContainsKey(this))
                    {
                        TurnManager.Instance.MakeMove(new NormalMove(PathFinder.Instance.CurrentSpaces[this]));
                        //TurnManager.Instance.SelectedUnit.State = UnitState.Retreated;
                        // ChangeUnitState?.Invoke(this, new StateChangedEventArgs(TurnManager.Instance.SelectedUnit, UnitState.Retreated));
                        CombatManager.Instance.CancelCombat();
                        CombatManager.Instance.CheckAdvance();
                        TurnManager.Instance.SelectedUnit = null;
                        PathFinder.Instance.Reset(PathType.Move);
                    }
                    break;

                case UnitState.Advancing:
                    if (PathFinder.Instance.CurrentSpaces.ContainsKey(this))
                    {
                        TurnManager.Instance.MakeMove(new NormalMove(PathFinder.Instance.CurrentSpaces[this]));
                        //TurnManager.Instance.SelectedUnit.State = UnitState.Advanced;
                        ChangeUnitState?.Invoke(this, new StateChangedEventArgs(TurnManager.Instance.SelectedUnit, UnitState.Advanced));
                        TurnManager.Instance.SelectedUnit = null;
                        PathFinder.Instance.Reset(PathType.Move);
                    }
                    break;

            }
        }

     //   Click?.Invoke(this, e);
    }




    //-------------------------------------RIGHT CLICK---------------------------------------------------------
    public void OnRightClick(Board board)
    {
      //  MessageController.Instance.Show("Right Click");
    }


    //-------------------------------------MIDDLE CLICK---------------------------------------------------------
    public void OnMiddleClick(Board board)
    {
       // MessageController.Instance.Show("Middle Click");
    }
}
