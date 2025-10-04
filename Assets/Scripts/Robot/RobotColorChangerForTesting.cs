using UnityEngine;

public class RobotColorChangerForTesting : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get sprite renderer if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Listen to game state changes
        GameStateManager.Instance.OnGameStateChange += OnGameStateChange;
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChange -= OnGameStateChange;
    }

    private void OnGameStateChange(GameState newState)
    {
        if (newState == GameState.RobotTempDeath)
        {
            // Robot dies - change color to red
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
            }
        }
        else if (newState == GameState.Playing)
        {
            // Robot is alive - change color back to normal
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
        }
    }
}
