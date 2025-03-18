using UnityEngine;

public class PlayerController : MonoBehaviour, IInputListener
{
    private Rigidbody _rb;

    public void FeedInput(IPlayerInput input)
    {
        Debug.Log("Move: " + input.GetMove());
        // Debug.Log("Look: " + input.GetLook());
        // Debug.Log("Jump: " + input.GetJump());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {

    }
}
