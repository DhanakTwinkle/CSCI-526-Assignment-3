using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndLevel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (!StateManager.instance.IsConnecting)
            {
                StateManager.instance.switchEffect(true);
                Movement.instance.resetMovement();
            }

            Movement.instance.enabled = false;
            Connector.instance.enabled = false;
            //Connector.instance.showKilledPlayers();
            StateManager.instance.showInstructions(false);
            StateManager.instance.showEnd(true);

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
