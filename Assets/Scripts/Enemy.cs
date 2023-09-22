using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            if (!StateManager.instance.IsConnecting)
            {
                StateManager.instance.switchConnectionState();
                Movement.instance.resetMovement();
            }

            Movement.instance.enabled = false;
            Connector.instance.enabled = false;

            StopAllCoroutines();
            StartCoroutine(LoadNewScene());
        }
    }

    IEnumerator LoadNewScene()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
