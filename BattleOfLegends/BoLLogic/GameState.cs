namespace BoLLogic;

public class GameState(Board board)
{

    public Board Board { get; } = board;
    public PlayerType CurrentPlayer { get; private set; } = board.CurrentPlayer;
    public TurnPhase CurrentTurnPhase { get; private set; } = board.TurnPhase;
    public GamePhase CurrentGamePhase { get; private set; } = board.GamePhase;
    public int CurrentGameRound { get; private set; } = board.GameRound;


    public void On_PlayerChanged(object sender, EventArgs e)
    {
        CurrentPlayer = TurnManager.Instance.CurrentPlayer;
        board.CurrentPlayer = CurrentPlayer;
    }


    public void On_TurnPhaseChanged(object sender, EventArgs e)
    {
        CurrentTurnPhase = TurnManager.Instance.CurrentTurnPhase;
        board.TurnPhase = CurrentTurnPhase;
    }


    public void On_GamePhaseChanged(object sender, EventArgs e)
    {
        CurrentGamePhase = TurnManager.Instance.CurrentGamePhase;
        board.GamePhase = CurrentGamePhase;
    }


    public void On_GameRoundChanged(object sender, EventArgs e)
    {
        CurrentGameRound = TurnManager.Instance.CurrentGameRound;
        board.GameRound = CurrentGameRound;

        if (CurrentGameRound == Board.EndRound)
        {
            MessageController.Instance.Show("GAME OVER !");
        }

        if (CurrentGameRound % 2 == 1)
        {

            foreach (Unit unit in Board.Units)
            {
                unit.State = UnitState.Idle;
            }

        }

        if (CurrentGameRound % 4 == 1 || CurrentGameRound % 4 == 2)
        {
            TurnManager.Instance.CurrentPlayer = Board.CurrentPlayer;
            CurrentPlayer = TurnManager.Instance.CurrentPlayer;
        }
        else
        {
            TurnManager.Instance.CurrentPlayer = Board.CurrentPlayer.Opponent();
            CurrentPlayer = TurnManager.Instance.CurrentPlayer;
        }

        foreach (Player player in Board.Players)
        {
            player.Action.ActionValue = 0;
        }
    }

}
