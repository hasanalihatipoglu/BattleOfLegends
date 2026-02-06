namespace BoLLogic;

public abstract class Unit : IDisposable
{
    private bool _disposed = false;

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
    public UnitState StateBeforeCombat { get; set; } // Tracks state before entering Defending/Attacking
    public UnitState StateBeforeLocked { get; set; } // Tracks state before being Locked (Idle/Ready/Active)
    public UnitState StateBeforeActive { get; set; } // Tracks state before becoming Active (Idle/Ready/Retreated)



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
                    // No need to update counter - it will be recalculated from actual unit states
                    break;

                case UnitState.Idle:
                    // Count actual ready units instead of using delta-tracked counter
                    int currentReadyCount = OrderManager.Instance.CountReadyUnits(this.Faction, board);

                    if(currentReadyCount < OrderManager.Instance.OrderLimit)
                    {
                        TurnManager.Instance.SelectedUnit.State = UnitState.Ready;
                        // No need to update counter - it will be recalculated from actual unit states
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
                        // Check if activating this unit would exceed max action limit
                        var player = board.Players.FirstOrDefault(p => p.Type == this.Faction);
                        if (player != null && player.Action.ActionValue >= player.Action.MaxAction)
                        {
                            MessageController.Instance.Show($"Cannot activate unit: Max action limit ({player.Action.MaxAction}) reached!");
                            break;
                        }

                        // Reset TurnPhase to Move when activating a Ready unit (only in Turn phase)
                        // Player must manually transition from Order → Turn phase before activating Ready units
                        TurnPhase previousTurnPhase = TurnManager.Instance.CurrentTurnPhase;
                        if (previousTurnPhase != TurnPhase.Move)
                        {
                            TurnManager.Instance.CurrentTurnPhase = TurnPhase.Move;
                            TurnManager.Instance.AdvanceTurnPhase();

                            // Record turn phase change in history
                            HistoryManager.Instance.RecordAction(
                                new PhaseChangeAction(TurnManager.Instance.CurrentPlayer, previousTurnPhase, TurnPhase.Move)
                            );
                        }

                        foreach (Unit u in board.Units)
                        {
                            if (u.Faction == TurnManager.Instance.CurrentPlayer && u.State == UnitState.Active)
                            {
                                u.State = UnitState.Idle;
                            }
                        }
                        TurnManager.Instance.SelectedUnit.StateBeforeActive = UnitState.Ready;  // Track state before activation
                        TurnManager.Instance.SelectedUnit.State = UnitState.Active;
                        PathFinder.Instance.FindPaths(this, this.Tile, PathType.Move);
                        break;

                    case UnitState.Retreated:
                        // Retreated units can be activated like Idle units (forced retreat during opponent's turn)
                        // During Order phase, block normal activation (count actual ready units)
                        int retreatedReadyCount = OrderManager.Instance.CountReadyUnits(this.Faction, board);
                        if(retreatedReadyCount > 0)
                        {
                            break;
                        }

                        // Check if activating this unit would exceed max action limit
                        var retreatedPlayer = board.Players.FirstOrDefault(p => p.Type == this.Faction);
                        if (retreatedPlayer != null && retreatedPlayer.Action.ActionValue >= retreatedPlayer.Action.MaxAction)
                        {
                            MessageController.Instance.Show($"Cannot activate unit: Max action limit ({retreatedPlayer.Action.MaxAction}) reached!");
                            break;
                        }

                        foreach (Unit u in board.Units)
                        {
                            if (u.Faction == TurnManager.Instance.CurrentPlayer && u.State == UnitState.Active)
                            {
                                u.State = UnitState.Idle;
                            }
                        }
                        TurnManager.Instance.SelectedUnit.StateBeforeActive = UnitState.Retreated;  // Track state before activation
                        TurnManager.Instance.SelectedUnit.State = UnitState.Active;
                        PathFinder.Instance.FindPaths(this, this.Tile, PathType.Move);
                        break;

                    case UnitState.Idle:
                        // During Order phase, block normal activation (count actual ready units)
                        int idleReadyCount = OrderManager.Instance.CountReadyUnits(this.Faction, board);
                        if(idleReadyCount > 0)
                        {
                            break;
                        }

                        // Check if activating this unit would exceed max action limit
                        var idlePlayer = board.Players.FirstOrDefault(p => p.Type == this.Faction);
                        if (idlePlayer != null && idlePlayer.Action.ActionValue >= idlePlayer.Action.MaxAction)
                        {
                            MessageController.Instance.Show($"Cannot activate unit: Max action limit ({idlePlayer.Action.MaxAction}) reached!");
                            break;
                        }

                        foreach (Unit u in board.Units)
                        {
                            if (u.Faction == TurnManager.Instance.CurrentPlayer && u.State == UnitState.Active)
                            {
                                u.State = UnitState.Idle;
                            }
                        }
                        TurnManager.Instance.SelectedUnit.StateBeforeActive = UnitState.Idle;  // Track state before activation
                        TurnManager.Instance.SelectedUnit.State = UnitState.Active;
                        PathFinder.Instance.FindPaths(this, this.Tile, PathType.Move);
                        break;

                    case UnitState.Active:
                        // Return to the state before activation (Ready, Idle, or Retreated)
                        TurnManager.Instance.SelectedUnit.State = TurnManager.Instance.SelectedUnit.StateBeforeActive;
                        TurnManager.Instance.SelectedUnit = null;
                        PathFinder.Instance.ResetAll();
                        break;

                    case UnitState.Locked:
                        // Locked units cannot be activated because another friendly unit has already moved
                        MessageController.Instance.Show("Unit locked - another friendly unit has already moved this turn");
                        break;

                    case UnitState.Moved:
                    case UnitState.Marched:
                    case UnitState.Attacked:
                    case UnitState.PushedBack:  // PushedBack units can't act (already spent action during own turn)
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

        // Validate state transition before applying
        if (!UnitStateValidator.IsValidTransition(State, e.State))
        {
            string error = UnitStateValidator.GetTransitionError(State, e.State);
            MessageController.Instance.Show($"Invalid state transition for {this}: {error}");
            return; // Reject invalid transition
        }

        PreviousState = State;

        State = e.State;

        switch (State)
        {
            case UnitState.Retreating:
                // Automatic retreat from combat - find and use first available path
                PathFinder.Instance.FindPaths(this, this.Tile, PathType.Retreat);

                // Determine retreat type:
                // - Retreated: Forced retreat during OPPONENT'S turn (defending) - can act on own turn
                // - PushedBack: Retreat during OWN turn (after attacking, counter-attack) - already spent action
                bool isOwnTurn = (this.Faction == TurnManager.Instance.CurrentPlayer);
                UnitState finalState = isOwnTurn ? UnitState.PushedBack : UnitState.Retreated;

                if (PathFinder.Instance.CurrentSpaces.Count > 0)
                {
                    TurnManager.Instance.MakeMove(new NormalMove(PathFinder.Instance.CurrentSpaces.Values.First()));
                    this.State = finalState;
                    System.Diagnostics.Debug.WriteLine($"[Retreat] {this} retreated to {finalState} (isOwnTurn={isOwnTurn})");
                }
                else
                {
                    MessageController.Instance.Show($"{this} has no valid retreat path");
                    this.State = finalState;
                }
                PathFinder.Instance.Reset(PathType.Move);
                break;
        }
    }

    // IDisposable implementation to clean up event subscriptions
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Unsubscribe from events to prevent memory leaks
            CombatManager.Instance.ChangeUnitState -= On_StateChanged;
        }

        _disposed = true;
    }

}





