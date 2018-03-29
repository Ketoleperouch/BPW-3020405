using UnityEngine;

public class MusicHandler : MonoBehaviour
{
    public AudioSource[] audioSources;
    public float maxVolume = 0.8f;
    public float resetTimer = 10;
    public bool forceSwitch = false;
    public bool lockClip = false;
    public int currentPlayingSource = 1;

    private float timer;

    public void Update()
    {
        if (forceSwitch)
        {
            forceSwitch = false;
            SwitchAudio(currentPlayingSource == 1 ? 1 : 0, 1);
        }
    }

    public void LockClip(bool state)
    {
        lockClip = state;
    }

    public void SwitchAudio(int source, float fadeSpeed)
    {
        if (source == currentPlayingSource - 1 || lockClip)
        {
            if (!lockClip)
                Debug.LogWarning("Audio Source switch cancelled. Destination source is already playing.");
            return;
        }
        if (Time.time > timer)
        {
            audioSources[source].Play();
            timer = Time.time + resetTimer;
        }
        StartCoroutine(Switch(source, fadeSpeed));
    }

    private System.Collections.IEnumerator Switch(int toSource, float fadeSpeed)
    {
        while (!Mathf.Approximately(audioSources[currentPlayingSource - 1].volume, 0) && !Mathf.Approximately(audioSources[toSource].volume, maxVolume))
        {
            audioSources[currentPlayingSource - 1].volume = Mathf.MoveTowards(audioSources[currentPlayingSource - 1].volume, 0, Time.deltaTime * fadeSpeed);
            audioSources[toSource].volume = Mathf.MoveTowards(audioSources[toSource].volume, maxVolume, Time.deltaTime * fadeSpeed);
            yield return null;
        }
        timer = Time.time + resetTimer;
        currentPlayingSource = toSource + 1;
    }
}