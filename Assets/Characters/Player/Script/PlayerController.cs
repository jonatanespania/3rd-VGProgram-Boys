using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float runSpeed = 7;
    public float rotationSpeed = 10;
    public Animator animator;
    public GameObject deathUI; // Referencia al nuevo Canvas de la UI de muerte
    public AudioClip deathUISound; // Referencia al AudioSource de la UI de muerte
    public float deathVolume = 1f;
    public AudioClip painDeathSound;
    private bool isDead = false;    // Referencias para las cámaras
    public Camera thirdPersonCamera; // Cámara en tercera persona (la cámara principal actual)
    public Camera firstPersonCamera; // Cámara en primera persona (la cámara adicional)
    private bool isFirstPersonView = false;

    private float x, y;
    public Rigidbody rb;
    public float jumpForce = 5;
    public float rollForce = 5;
    public float evadeForce = 5;

    public Transform groundCheck;
    public float groundDistance = 0.1f;
    public LayerMask groundMask;

    private bool isGrounded;
    private bool canRoll = true;
    private bool canEvade = true;
    private bool isInvulnerable = false; // Variable para indicar si es invulnerable
    private float lastTapTime = 0f;
    private float doubleTapTime = 0.3f; // Tiempo permitido entre taps para rodar
    private bool awaitingSecondTap = false;

    public int maxHealth = 100;
    public int currentHealth;
    public Text healthText;
    public HealthBar healthBar; // Añade esta línea para la referencia a la barra de vida

    public int score = 0;
    public Text scoreText;
    public ScoreBar scoreBar; // Añadir referencia al ScoreBar
    public int pointsToWin = 10;
    private List<Renderer> renderers = new List<Renderer>();    // Referencia al modelo completo del personaje
    public GameObject characterModel;
    
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateScoreUI();
        UpdateHealthUI();
        deathUI.SetActive(false); // Asegúrate de que la UI esté desactivada al inicio
        renderers.AddRange(GetComponentsInChildren<Renderer>());
        
        // Configuración inicial de las cámaras
        if (thirdPersonCamera != null && firstPersonCamera != null)
        {
            // Asegurarse que comienza en tercera persona
            thirdPersonCamera.enabled = true;
            firstPersonCamera.enabled = false;
            isFirstPersonView = false;
        }
        else
        {
            Debug.LogWarning("Faltan referencias a las cámaras. Asigna las cámaras en el Inspector.");
        }
        
        // Si no tenemos la referencia al modelo del personaje, intentamos encontrarlo automáticamente
        if (characterModel == null)
        {
            // Buscar el primer hijo que tenga un Renderer (probablemente el modelo del personaje)
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponentInChildren<Renderer>() != null)
                {
                    characterModel = transform.GetChild(i).gameObject;
                    Debug.Log("Modelo del personaje detectado automáticamente: " + characterModel.name);
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Comprobación de tecla para cambiar vista de cámara (tecla C)
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCameraView();
        }

        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(x, 0, y).normalized;

        transform.Rotate(0, x * Time.deltaTime * rotationSpeed, 0);
        transform.Translate(0, 0, y * Time.deltaTime * runSpeed);

        animator.SetFloat("VelX", x);
        animator.SetFloat("VelY", y);

        if (Input.GetKey("f"))
        {
            animator.Play("SnakeDance");
            animator.SetBool("Other", false);
        }
        if (Input.GetKey("g"))
        {
            animator.Play("BootyDance");
            animator.SetBool("Other", false);
        }

        if (x != 0 || y != 0)
        {
            animator.SetBool("Other", true);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (awaitingSecondTap)
            {
                if (Time.time - lastTapTime < doubleTapTime)
                {
                    if (isGrounded && canRoll)
                    {
                        Roll();
                        StartCoroutine(RollCooldown());
                    }
                }
                awaitingSecondTap = false;
            }
            else
            {
                lastTapTime = Time.time;
                awaitingSecondTap = true;
                StartCoroutine(EvadeIfSingleTap());
            }
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        animator.Play("Jump");
    }

    private void Roll()
    {
        StartCoroutine(TemporaryInvulnerability(animator.GetCurrentAnimatorStateInfo(0).length));
        animator.Play("Roll");
        rb.AddForce(transform.forward * rollForce, ForceMode.Impulse);
    }

    private void Evade()
    {
        rb.linearVelocity = Vector3.zero;
        StartCoroutine(TemporaryInvulnerability(animator.GetCurrentAnimatorStateInfo(0).length));
        animator.Play("Evade");
        rb.AddForce(-transform.forward * evadeForce, ForceMode.Impulse);
    }

    private IEnumerator RollCooldown()
    {
        canRoll = false;
        yield return new WaitForSeconds(2);
        canRoll = true;
    }

    private IEnumerator EvadeIfSingleTap()
    {
        yield return new WaitForSeconds(doubleTapTime);
        if (awaitingSecondTap && isGrounded && canEvade && !IsMoving())
        {
            Evade();
        }
        awaitingSecondTap = false;
    }

    private bool IsMoving()
    {
        return x != 0 || y != 0;
    }

    private IEnumerator TemporaryInvulnerability(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageEffect());
            StartCoroutine(TemporaryInvulnerability(1f));
        }
        UpdateHealthUI();
    }

    private IEnumerator DamageEffect()
    {
        for (int i = 0; i < 5; i++)
        {
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }
            yield return new WaitForSeconds(0.1f);
            foreach (var renderer in renderers)
            {
                renderer.enabled = true;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ShowDeathUIAfterDelay(float delay)
    {
        
        yield return new WaitForSeconds(delay);
        MuteAllSounds();
        deathUI.SetActive(true);
        
        Debug.Log("Reproduciendo sonido de la UI de muerte");
    }

    private void Die()
    {
        if (!isDead)
        {
            isDead = true;
            animator.Play("Die");
            if (painDeathSound != null)
            {
                AudioSource painDeathAudioSource = gameObject.AddComponent<AudioSource>();
                painDeathAudioSource.clip = painDeathSound;
                painDeathAudioSource.spatialBlend = 0.0f;
                painDeathAudioSource.volume = deathVolume;
                painDeathAudioSource.loop = false; // Asegúrate de que no se repita
                painDeathAudioSource.Play();
                Destroy(painDeathAudioSource, painDeathSound.length); // Destruye el AudioSource después de que se complete la reproducción
            }
            
            StartCoroutine(ShowDeathUIAfterDelay(4f));
            StartCoroutine(RestartSceneAfterDelay(14f));
        }
    }

    IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void MuteAllSounds()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            audioSource.mute = true;
        }
        if (deathUISound != null)
        {
            AudioSource deathAudioSource = gameObject.AddComponent<AudioSource>();
            deathAudioSource.clip = deathUISound;
            deathAudioSource.spatialBlend = 0.0f;
            deathAudioSource.volume = deathVolume;
            deathAudioSource.loop = false; // Asegúrate de que no se repita
            deathAudioSource.Play();
            Destroy(deathAudioSource, deathUISound.length); // Destruye el AudioSource después de que se complete la reproducción
        }
      
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
        if (scoreBar != null)
        {
            scoreBar.UpdateScore(score);
        }
       /* if (score >= pointsToWin)
        {
            WinGame();
        }*/
    }
    private void WinGame()
    {
        // Mostrar la pantalla de victoria
        // Esto podría ser una nueva escena o una UI en la escena actual
        Debug.Log("¡Has ganado el juego!");
        SceneManager.LoadScene("VictoryScene"); // Asegúrate de que la escena 'VictoryScene' esté añadida en los Build Settings
    }
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Cerebros: " + score;
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Vida: " + currentHealth;
        }
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth);
        }
    }    // Método para alternar entre vista en primera y tercera persona
    private void ToggleCameraView()
    {
        if (thirdPersonCamera == null || firstPersonCamera == null)
        {
            Debug.LogWarning("Faltan referencias a las cámaras. Asigna las cámaras en el Inspector.");
            return;
        }
        
        isFirstPersonView = !isFirstPersonView;
        
        if (isFirstPersonView)
        {
            // Cambiar a primera persona
            thirdPersonCamera.enabled = false;
            firstPersonCamera.enabled = true;
            
            // Ocultar todo el modelo del personaje si está asignado
            if (characterModel != null)
            {
                characterModel.SetActive(false);
                Debug.Log("Modelo del personaje oculto: " + characterModel.name);
            }
            else
            {
                // Plan B: Desactivar todos los renderers
                foreach (var renderer in renderers)
                {
                    renderer.enabled = false;
                    Debug.Log("Ocultando: " + renderer.gameObject.name);
                }
            }
            
            Debug.Log("Activada vista en primera persona");
        }
        else
        {
            // Volver a tercera persona
            firstPersonCamera.enabled = false;
            thirdPersonCamera.enabled = true;
            
            // Mostrar todo el modelo del personaje si está asignado
            if (characterModel != null)
            {
                characterModel.SetActive(true);
            }
            else
            {
                // Plan B: Activar todos los renderers
                foreach (var renderer in renderers)
                {
                    renderer.enabled = true;
                }
            }
            
            Debug.Log("Activada vista en tercera persona");
        }
    }
}