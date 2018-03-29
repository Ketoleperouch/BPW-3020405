using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

    public CanvasGroup fader;
    public int buildIndex;

    public void StartGame()
    {
        StartCoroutine(Process(false));
    }

    public void QuitGame()
    {
        StartCoroutine(Process(true));
    }

    private IEnumerator Process(bool quit)
    {
        while (!Mathf.Approximately(fader.alpha, 1))
        {
            fader.alpha = Mathf.MoveTowards(fader.alpha, 1, Time.deltaTime);
            yield return null;
        }
        if (quit)
        {
            Application.Quit();
            UnityEditor.EditorApplication.isPlaying = false;
        }
        SceneManager.LoadScene(buildIndex);
    }

}
