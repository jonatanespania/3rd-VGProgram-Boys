
using UnityEngine;



public class ScenePauseController : MonoBehaviour
{
    private bool isPaused = false;
    private AudioSource[] allAudioSources;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // Asignamos la tecla P para pausar y reanudar
        {
            if (isPaused)
            {
                ResumeScene();
            }
            else
            {
                PauseScene();
            }
        }
    }

    public void PauseScene()
    {
        Time.timeScale = 0f; // Pausar la escena

        allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in allAudioSources)
        {
            audioSource.Pause(); // Pausar todos los sonidos
        }

        isPaused = true;
    }

    public void ResumeScene()
    {
        Time.timeScale = 1f; // Reanudar la escena

        foreach (var audioSource in allAudioSources)
        {
            audioSource.UnPause(); // Reanudar todos los sonidos
        }

        isPaused = false;
    }
}
