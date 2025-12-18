using System.Linq;

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

        System.Diagnostics.Debug.WriteLine($"On_GameRoundChanged: CurrentGameRound={CurrentGameRound}, EndRound={Board.EndRound}");

        if (CurrentGameRound >= Board.EndRound)
        {
            // Determine winner based on morale
            var romePlayer = Board.Players.FirstOrDefault(p => p.Type == PlayerType.Rome);
            var carthagePlayer = Board.Players.FirstOrDefault(p => p.Type == PlayerType.Carthage);

            if (romePlayer != null && carthagePlayer != null)
            {
                int romeMorale = romePlayer.Morale.MoraleValue;
                int carthageMorale = carthagePlayer.Morale.MoraleValue;

                string gameOverMessage;
                if (romeMorale > carthageMorale)
                {
                    gameOverMessage = $"GAME OVER!\n\nROME WINS!\nRome Morale: {romeMorale}\nCarthage Morale: {carthageMorale}";
                }
                else if (carthageMorale > romeMorale)
                {
                    gameOverMessage = $"GAME OVER!\n\nCARTHAGE WINS!\nRome Morale: {romeMorale}\nCarthage Morale: {carthageMorale}";
                }
                else
                {
                    gameOverMessage = $"GAME OVER!\n\nDRAW!\nBoth armies have equal morale: {romeMorale}";
                }

                MessageController.Instance.ShowWithOkButton(gameOverMessage);
            }
            else
            {
                MessageController.Instance.ShowWithOkButton("GAME OVER!");
            }
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
