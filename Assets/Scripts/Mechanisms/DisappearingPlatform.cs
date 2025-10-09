using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Collider2D))]
public class DisappearingPlatform : MechanismListener
{
    [SerializeField] private float disappearDelay = 1f; // Delay before animation starts

    private bool hasDisappeared = false;

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
                    StartCoroutine(DisappearPlatform());
                    break;
                }
            }
        }
    }

	private IEnumerator DisappearPlatform()
	{

		_Animator.Play("fadingPlatform");
		hasDisappeared = true;
		yield return new WaitForSeconds(disappearDelay);
	}

	protected override void HandleMechanismTrigger(IMechanism sender, bool isActive)
	{
		MechanismStates mechanism = null;

		foreach (var linkedMechanism in linkedMechanisms)
		{
			if (linkedMechanism.mechanism != sender) continue;
			linkedMechanism.isActive = isActive;
			mechanism = linkedMechanism;
		}

		if (MechanismShouldOpen(mechanism, isActive))
			StartCoroutine(ReactivatePlatform(true));
		else
			StartCoroutine(ReactivatePlatform(false));
	}

    private IEnumerator ReactivatePlatform(bool shouldReactivate)
    {
        if (shouldReactivate)
        {
			_Animator.Play("FadingPlatformIdle");
			hasDisappeared = false;
			_Collider.enabled = true;

            yield return new WaitForSeconds(0.2f);

			foreach (MechanismStates mechanism in linkedMechanisms)
				mechanism.mechanism.Deactivate();
		}
    }

	// Called by Animation Event on the last frame
	public void DisableCollider()
    {
        if (_Collider != null)
			_Collider.enabled = false;
    }
}
