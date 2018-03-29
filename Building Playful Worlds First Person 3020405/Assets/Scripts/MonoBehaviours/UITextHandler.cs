using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITextHandler : MonoBehaviour {

    private static UITextHandler uiText;
    private Text text;

    private void Awake()
    {
        uiText = this;
        text = GetComponent<Text>();
    }

    public static void LogText(string text)
    {
        uiText.StartCoroutine(Process(text, 0));
    }

    public static void LogText(string text, float delay)
    {
        uiText.StartCoroutine(Process(text, delay));
    }

    //For use through UnityEvents
    public void LogTextNonStatic(string text)
    {
        StartCoroutine(Process(text, 0));
    }

    private static IEnumerator Process(string displayText, float delay)
    {
        yield return new WaitForSeconds(delay);
        uiText.text.text = displayText;
        float multiplier = 0;
        if (displayText.Length < 5)
        {
            multiplier = 2;
        }
        else if (displayText.Length < 20)
        {
            multiplier = 8;
        }
        else
        {
            multiplier = 16;
        }
        yield return new WaitForSeconds(displayText.Length / multiplier);
        uiText.text.text = "";
    }
}
