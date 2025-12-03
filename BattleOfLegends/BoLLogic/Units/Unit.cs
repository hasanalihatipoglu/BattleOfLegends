namespace BoLLogic;

public abstract class Unit
{
    public abstract UnitType Type { get; }
    public abstract UnitClass Class { get; }
    public abstract PlayerType Faction { get; }

    public abstract int MarchMove { get; }
    public abstract int AttackMove { get; }
    public abstract int AttackRange { get; }
    public abstract int Strength { get;}
    public abstract int MeleeAttackPoint { get; }
    public abstract int MeleeDefensePoint { get; }
    public abstract int RangedAttackPoint { get; }
    public abstract int RangedDefensePoint { get; }
    public abstract bool IsLight { get; }

    public abstract List<CardType> Skills { get; set; }
    public abstract List<CardType> Abilities { get; set; }

    public Position Position { get; set; }
    public Tile Tile { get; set; }

    public HealthSystem Health { get; set; }
    public UnitState State { get; set; }
    public UnitState PreviousState { get; set; }



    //--------------------------------------HOVER------------------------------------------------
    public void OnHover(Board board)
    {

        //HOVER FRIENDLY UNIT
        if (this.Faction == TurnManager.Instance.CurrentPlayer)
        {

        }

        //HOVER ENEMY UNIT
        else
        {
            //SELECTED UNIT
            if (TurnManager.Instance.SelectedUnit == null)
            {
                return;
            }

            switch (TurnManager.Instance.SelectedUnit.State)
            {
                case UnitState.Idle:
                    break;

                case UnitState.Active:
                    if (PathFinder.Instance.CurrentTargets.ContainsKey(this.Tile))
                    {
                        PathFinder.Instance.AssignPath(PathFinder.Instance.CurrentTargets[this.Tile].MovePath, PathType.Move);
                        PathFinder.Instance.AssignPath(PathFinder.Instance.CurrentTargets[this.Tile].AttackPath, PathType.Attack);
                    }
                    break;

                case UnitState.Moved:
                    if (PathFinder.Instance.CurrentTargets.ContainsKey(this.Tile))
                    {
                        PathFinder.Instance.AssignPath(PathFinder.Instance.CurrentTargets[this.Tile].AttackPath, PathType.Attack);
                    }
                    break;
            }
          
        }

    }



    //-------------------------------------CLICK---------------------------------------------------------
    public void OnClick(Board board)
    {
        
        SoundController.Instance.PlaySound("click_button");            
        
       //SELECT PHASE
        if (TurnManager.Instance.CurrentGamePhase == GamePhase.Select)
        {
            TurnManager.Instance.SelectedUnit = this;

            switch (TurnManager.Instance.SelectedUnit.State)
            {
                case UnitState.None:
                    break;

                case UnitState.Idle:
                    break;
            }
        }


        //ORDER PHASE
        else if (TurnManager.Instance.CurrentGamePhase == GamePhase.Order)
        {
            TurnManager.Instance.SelectedUnit = this;

            switch (TurnManager.Instance.SelectedUnit.State)
            {
                case UnitState.Ready:
                    TurnManager.Instance.SelectedUnit.State = UnitState.Idle;
                    OrderManager.Instance.NumberOfOrderedUnits--;
                    break;

                case UnitState.Idle:
                    if(OrderManager.Instance.NumberOfOrderedUnits < OrderManager.Instance.OrderLimit)
                    {
                        TurnManager.Instance.SelectedUnit.State = UnitState.Ready;
                        OrderManager.Instance.NumberOfOrderedUnits++;
                    }
                    else
                    {
                        MessageController.Instance.Show("Order Limit Reached");
                    }

                    break;
            }
        }

        //TURN PHASE
        else
        {
     
            //CLICK FRIENDLY UNIT
            if (this.Faction == TurnManager.Instance.CurrentPlayer)
            {

                TurnManager.Instance.SelectedUnit = this;

                switch (TurnManager.Instance.SelectedUnit.State)
                {
                    case UnitState.Ready:
                        foreach (Unit u in board.Units)
                        {
                            if (u.Faction == TurnManager.Instance.CurrentPlayer && u.State == UnitState.Active)
                            {
                                u.State = UnitState.Idle;
                            }
                        }
                        TurnManager.Instance.SelectedUnit.State = UnitState.Active;
                        PathFinder.Instance.FindPaths(this, this.Tile, PathType.Move);
                        break;


                    case UnitState.Idle:
                        if(OrderManager.Instance.NumberOfOrderedUnits>0)
                        {
                            break;
                        }
                        foreach (Unit u in board.Units)
                        {
                            if (u.Faction == TurnManager.Instance.CurrentPlayer && u.State == UnitState.Active)
                            {
                                u.State = UnitState.Idle;
                            }
                        }
                        TurnManager.Instance.SelectedUnit.State = UnitState.Active;
                        PathFinder.Instance.FindPaths(this, this.Tile, PathType.Move);
                        break;

                    case UnitState.Active:
                        TurnManager.Instance.SelectedUnit.State = UnitState.Idle;
                        TurnManager.Instance.SelectedUnit = null;
                        PathFinder.Instance.ResetAll();
                        break;

                    case UnitState.Moved:
                    case UnitState.Marched:
                    case UnitState.Attacked:
                    case UnitState.Retreated:
                    case UnitState.Advanced:
                    case UnitState.Attacking:
                    case UnitState.Defending:
                        TurnManager.Instance.SelectedUnit.State = UnitState.Passive;
                        foreach (Unit u in board.Units)
                        {
                            if (u.Faction == TurnManager.Instance.CurrentPlayer && u.State == UnitState.Active)
                            {
                                u.State = UnitState.Idle;
                            }
                        }
                        TurnManager.Instance.SelectedUnit = null;
                        PathFinder.Instance.ResetAll();
                        MessageController.Instance.Show("No actions left");
                        break;

                    case UnitState.Passive:
                        break;

                    case UnitState.Advancing:
                        if(TurnManager.Instance.CurrentTurnPhase == TurnPhase.Advance)
                        {
                            PathFinder.Instance.FindPaths(this, this.Tile, PathType.Advance);
                        }

                        //PathFinder.Instance.Reset(PathType.Move);
                        break;
                }
            }


            //CLICK ENEMY UNIT
            else
            {
                if (TurnManager.Instance.SelectedUnit == null)
                {
                    MessageController.Instance.Show("No active unit");
                    return;
                }

                if (!PathFinder.Instance.CurrentTargets.ContainsKey(this.Tile))
                {
                    MessageController.Instance.Show("Out of Range");
                    return;
                }


                switch (TurnManager.Instance.SelectedUnit.State)
                {
                    case UnitState.Idle:
                        break;

                    case UnitState.Active:
                        TurnManager.Instance.SelectedUnit.State = UnitState.Attacked;
                        TurnManager.Instance.MakeMove(new NormalMove(PathFinder.Instance.CurrentTargets[this.Tile].MovePath));
                        TurnManager.Instance.MakeAttack(new NormalAttack(PathFinder.Instance.CurrentTargets[this.Tile].AttackPath));
                        PathFinder.Instance.Reset(PathType.Move);
                        TurnManager.Instance.SelectedUnit = null;
                        break;

                    case UnitState.Moved:
                        TurnManager.Instance.SelectedUnit.State = UnitState.Attacked;
                        TurnManager.Instance.MakeAttack(new NormalAttack(PathFinder.Instance.CurrentTargets[this.Tile].AttackPath));
                        PathFinder.Instance.Reset(PathType.Move);
                        TurnManager.Instance.SelectedUnit = null;
                        break;

                    case UnitState.Marched:
                    case UnitState.Attacked:
                        PathFinder.Instance.ResetAll();
                        break;
                }
            }

        }

    }



    public void On_StateChanged(object sender, StateChangedEventArgs e)
    {
        if (e.Unit != this) return;

        PreviousState = State;
        
        State = e.State;

        switch (State)
        {                   
            case UnitState.Retreating:
                // Automatic retreat from combat - find and use first available path
                PathFinder.Instance.FindPaths(this, this.Tile, PathType.Retreat);
                if (PathFinder.Instance.CurrentSpaces.Count > 0)
                {
                    TurnManager.Instance.MakeMove(new NormalMove(PathFinder.Instance.CurrentSpaces.Values.First()));
                    this.State = UnitState.Retreated;
                }
                else
                {
                    MessageController.Instance.Show($"{this} has no valid retreat path");
                    this.State = UnitState.Retreated;
                }
                PathFinder.Instance.Reset(PathType.Move);
                break;            
        }
    }

}





