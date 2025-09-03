using UnityEngine;

/// <summary>
/// Handles explosion effects for enemies and defenders.
/// Attach to any GameObject that can die (e.g., Enemy, Defender).
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    [Header("Explosion Settings")]
    [Tooltip("Assign in Inspector: Prefab for the explosion effect.")]
    public GameObject explosionPrefab;

    [Tooltip("Assign in Inspector: Audio clip for the explosion sound.")]
    public AudioClip explosionSound;

    [Tooltip("Assign in Inspector: AudioSource for playing sounds.")]
    public AudioSource audioSource;

    /// <summary>
    /// Plays an explosion effect at the object's position.
    /// </summary>
    public void PlayExplosion()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2f); // Destroy after effect completes
        }

        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
    }
}
