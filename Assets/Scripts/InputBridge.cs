using System.Collections.Generic;
using UnityEngine;

public class InputBridge : MonoBehaviour
{
    public Component[] InputListeners;

    private List<IInputListener> _inputListeners = new List<IInputListener>();

    void Start()
    {
        // check if listeners implemnent IInputListener and add them to the list
        foreach (var listener in InputListeners)
        {
            if (listener is IInputListener inputListener)
            {
                _inputListeners.Add(inputListener);
                Debug.Log("Added input listener: " + listener.name);
            }
        }
    }

    void Update()
    {
        if (_inputListeners == null) return;
        foreach (var listener in _inputListeners)
        {
            if (listener != null)
                listener.FeedInput(InputManager.GetInput());
        }
    }
}
