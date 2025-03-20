using UnityEngine;

public struct InputData : ICharacterInput
{
    public Vector2 Move;
    public Vector2 Look;
    public bool Jump;
    public bool Sprint;

    public bool GetSprint()
    {
        return Sprint;
    }

    bool ICharacterInput.GetJump()
    {
        return Jump;
    }

    Vector2 ICharacterInput.GetLook()
    {
        return Look;
    }

    Vector2 ICharacterInput.GetMove()
    {
        return Move;
    }
}
