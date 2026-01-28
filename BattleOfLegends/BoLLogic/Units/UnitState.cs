namespace BoLLogic;

public enum UnitState
{
    None,
    Idle,
    Active,
    Marched,
    Moved,
    Attacked,
    Passive,
    Dead,
    Retreating,
    Retreated,   // Forced retreat during opponent's turn (can still act on own turn)
    PushedBack,  // Retreat during own turn after counter-attack (already spent action)
    Advancing,
    Advanced,
    Attacking,
    Defending,
    Ready,
    Locked       // Locked from activation because another friendly unit has already moved
}

