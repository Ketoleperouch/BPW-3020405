using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

    public CanvasGroup fader;
    public int buildIndex;
    public Transform cam;
    public AudioSource music;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

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
            music.volume = Mathf.MoveTowards(music.volume, 0, Time.deltaTime);
            yield return null;
        }
        if (quit)
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        SceneManager.LoadScene(buildIndex);
    }

    private void Update()
    {
        cam.Rotate(new Vector3(0, 2, 0) * Time.deltaTime);
    }

}
