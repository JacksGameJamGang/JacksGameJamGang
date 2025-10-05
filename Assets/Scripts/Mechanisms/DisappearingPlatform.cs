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

    private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {
        if (hasDisappeared) return;

        // Trigger only if the collision is the ground check object
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DisappearRoutine());
        }
    }

    private IEnumerator DisappearRoutine()
    {
        yield return new WaitForSeconds(disappearDelay);

        anim.SetTrigger("DisappearTrigger"); // Play animation
        hasDisappeared = true;
    }

    // Called by Animation Event on the last frame
    public void DisableCollider()
    {
        if (col != null)
            col.enabled = false;
    }
}
