using UnityEngine;

public interface IPlayerInput
{
    public Vector2 GetMove();
    public Vector2 GetLook();
    public bool GetJump();
    public bool GetSprint();
}
