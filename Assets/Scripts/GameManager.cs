
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        Playing,
        Paused,
        Won,
        Lost
    }

    [Header("Game State")]
    public GameState currentState = GameState.Playing;

    public bool IsPlaying => currentState == GameState.Playing;

    void Awake()
    {
        // Make sure only one GameManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        LockCursor();
        Debug.Log("Game Started");
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // Pause and resume
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        // Temporary testing controls (Editor only)
#if UNITY_EDITOR
        // Test winning
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            WinGame();
        }

        // Test losing
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            LoseGame();
        }
#endif

        // Restart after winning or losing
        if (currentState == GameState.Won ||
            currentState == GameState.Lost)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                RestartGame();
            }
        }
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;

            UnlockCursor();

            Debug.Log("Game Paused");
        }
        else if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;

            LockCursor();

            Debug.Log("Game Resumed");
        }
    }

    public void WinGame()
    {
        if (!IsPlaying)
            return;

        currentState = GameState.Won;

        EndGame();

        Debug.Log("YOU WIN!");
    }

    public void LoseGame()
    {
        if (!IsPlaying)
            return;

        currentState = GameState.Lost;

        EndGame();

        Debug.Log("GAME OVER!");
    }

    void EndGame()
    {
        Time.timeScale = 0f;
        UnlockCursor();

        Debug.Log("Press R to Restart");
    }

    public void RestartGame()
    {
        Debug.Log("Restarting Game...");

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
