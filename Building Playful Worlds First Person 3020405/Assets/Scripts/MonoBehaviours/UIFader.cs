using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFader : MonoBehaviour
{
    [SerializeField] float fadeSpeed = 1f;              
    [SerializeField] CanvasGroup groupToFade;  

    private void Reset()
    {
        groupToFade = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (groupToFade.alpha > 0f)
            groupToFade.alpha -= Time.deltaTime / 2 * fadeSpeed;
    }

    public void Flash()
    {
        groupToFade.alpha += 0.5f;
    }
}