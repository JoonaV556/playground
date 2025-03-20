using UnityEngine;

public interface ICharacterInput
{
    public Vector2 GetMove();
    public Vector2 GetLook();
    public bool GetJump();
    public bool GetSprint();
}
