using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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
    public GameObject connectingPost;
    public float transitionSpeed = 50.0f;

    public void switchConnectionState()
    {
        IsConnecting = !IsConnecting;
        if (IsConnecting)
        {
            StopAllCoroutines();
            StartCoroutine(SwitchPostProcess(true));
            Movement.instance.resetMovement();
            Connector.instance.setConnections();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(SwitchPostProcess(false));
            Connector.instance.resetConnections();
        }
    }

    IEnumerator SwitchPostProcess(bool on)
    {
        float weight = on ? 0 : 1;
        float target = on ? 1 : 0;

        connectingPost.GetComponent<PostProcessVolume>().weight = weight;
        
        while(Mathf.Abs(weight - target) > 0.001f)
        {
            weight = Mathf.Lerp(weight, target, transitionSpeed * Time.deltaTime);
            connectingPost.GetComponent<PostProcessVolume>().weight = weight;

            yield return null;
        }

        connectingPost.GetComponent<PostProcessVolume>().weight = target;

        yield return null;
    }
}
