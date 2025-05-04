using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private bool hideOnStart = true;

    [HideInInspector] public bool isActive;

    private void Start()
    {
        Global.OnGameContinue.AddListener(TurnOff);

        if (hideOnStart)
            TurnOff();
    }

    public void OpenScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void StopGame()
    {
        SetAmounts(true);
        Time.timeScale = 0f;
    }

    public void ContinueGame()
        => Global.OnGameContinue.Invoke();

    private void TurnOff()
    {
        SetAmounts(false);
        Time.timeScale = 1f;
    }

    private void SetAmounts(bool amount)
    {
        Cursor.visible = amount;
        gameObject.SetActive(amount);
        isActive = amount;
    }

    public void OpenWindow(GameObject window)
        => window.SetActive(true);

    public void CloseWindow(GameObject window)
        => window.SetActive(false);

    public void Exit()
        => Application.Quit();
}
