using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPortal : InteractibleObject
{
    [SerializeField] private GameObject endEffect;
    [SerializeField] private string sceneName = "MainMenu";

    private void Start()
    {
        endEffect.SetActive(false);
    }

    public override void Interact()
    {
        endEffect.SetActive(true);
        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(5f);

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
