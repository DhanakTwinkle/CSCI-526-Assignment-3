using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static StateManager instance = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public bool IsConnecting = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            switchConnectionState();
        }
    }

    public void switchConnectionState()
    {
        IsConnecting = !IsConnecting;
        if (IsConnecting)
        {
            Movement.instance.resetMovement();
            Connector.instance.setConnections();
        }
        else
        {
            Connector.instance.resetConnections();
        }
    }
}
