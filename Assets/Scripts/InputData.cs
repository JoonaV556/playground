using UnityEngine;

public struct InputData : IPlayerInput
{
    public Vector2 Move;
    public Vector2 Look;
    public bool Jump;
    public bool Sprint;

    public bool GetSprint()
    {
        return Sprint;
    }

    bool IPlayerInput.GetJump()
    {
        return Jump;
    }

    Vector2 IPlayerInput.GetLook()
    {
        return Look;
    }

    Vector2 IPlayerInput.GetMove()
    {
        return Move;
    }
}
