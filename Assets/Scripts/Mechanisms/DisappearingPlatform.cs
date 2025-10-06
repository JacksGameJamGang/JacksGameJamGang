using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Collider2D))]
public class DisappearingPlatform : MonoBehaviour
{
    [SerializeField] private float disappearDelay = 0.5f; // Delay before animation starts

    private Animator anim;
    private Collider2D col;
    private bool hasDisappeared = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasDisappeared) return;

        // Only care about the player
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Dog"))
        {
            // Check if the player is standing on this platform
            foreach (ContactPoint2D contact in collision.contacts)
            {
                //Debug.Log($"Contact normal: {contact.normal}, Contact point: {contact.point}");
                // The player's feet should be above the contact point
                if (contact.normal.y < -0.5f) // ensures contact is from above
                {
                    //Debug.Log("Player is standing on platform");
                    StartCoroutine(DisappearRoutine());
                    break;
                }
            }
        }
    }

    private IEnumerator DisappearRoutine()
    {

        anim.SetTrigger("DisappearTrigger"); // Play animation
        hasDisappeared = true;
        yield return new WaitForSeconds(disappearDelay);
        DisableCollider();
    }

    // Called by Animation Event on the last frame
    public void DisableCollider()
    {
        if (col != null)
            col.enabled = false;
    }
}
