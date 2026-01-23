using System.ComponentModel;

namespace BoLLogic;

public class HandSystem()
{
    public int MaxHand { get; set; }
    public int HandValue { get; set; }
    public PlayerType Faction { get; set; }
    public bool IsMaxHandReached { get; set; }

    /// <summary>
    /// Change hand value by the specified amount (legacy method, kept for compatibility)
    /// </summary>
    public void Change(int handAmount)
    {
        System.Diagnostics.Debug.WriteLine($"[HandSystem.Change] {Faction}: HandValue={HandValue}, handAmount={handAmount}, MaxHand={MaxHand}");

        int newValue = HandValue + handAmount;
        if (newValue <= MaxHand && newValue >= 0)
        {
            HandValue = newValue;
            IsMaxHandReached = (HandValue >= MaxHand);
            System.Diagnostics.Debug.WriteLine($"[HandSystem.Change] {Faction}: Hand changed to {HandValue}");
        }
        else if (newValue > MaxHand)
        {
            System.Diagnostics.Debug.WriteLine($"[HandSystem.Change] {Faction}: MAX HAND REACHED - not changing hand");
            IsMaxHandReached = true;
        }
    }

    /// <summary>
    /// Sync hand value from actual card count in the board
    /// </summary>
    public void SyncFromCardCount(Board board)
    {
        int actualCount = board.Cards.Count(c => c.Faction == Faction &&
            (c.State == CardState.InHand || c.State == CardState.ReadyToPlay));
        HandValue = actualCount;
        IsMaxHandReached = (HandValue >= MaxHand);
        System.Diagnostics.Debug.WriteLine($"[HandSystem.SyncFromCardCount] {Faction}: Synced to {HandValue} cards (MaxHand={MaxHand})");
    }
}
