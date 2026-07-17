using System.Collections;
using UnityEngine;

public class DamageEffects : MonoBehaviour
{
    GameObject objectToFlash;
    [SerializeField] int numberOfFlashes = 2;
    [SerializeField] float durationBetweenFlashes = 0.05f;
    [SerializeField] private float flashDuration = 0.08f;
    private SpriteRenderer spriteRenderer;

    [Header("Particles")]
    [SerializeField] private GameObject hitParticlesPrefab;

    private Coroutine flashRoutine;

    private static readonly int FlashAmount =
        Shader.PropertyToID("_FlashAmount");

    // Start is called before the first frame update
    void Start()
    {
        objectToFlash = gameObject;
        spriteRenderer = objectToFlash.GetComponent<SpriteRenderer>();
    }
    public void PlayHitEffects()
    {
        Flash();
        PlayParticles();
    }

    private void PlayParticles()
    {
        if (hitParticlesPrefab == null)
            return;

        ObjectPoolManager.SpawnObject(
            hitParticlesPrefab,
            transform.position,
            Quaternion.identity
        );
    }
    private IEnumerator flashingCoroutine;


    public void Flash()
    {
        if (spriteRenderer)
        {
            if (flashingCoroutine != null)
            {
                StopCoroutine(flashingCoroutine);
            }

            flashingCoroutine = InternalFlash();
            StartCoroutine(flashingCoroutine);
        }
    }

    private IEnumerator InternalFlash()
    {
        bool makeSpriteWhite = true;

        // Iterate twice the length of times - that way numberOfFlashes is
        // "how many times is it turned on", not "how many times does it flip"
        for (int i = 0; i < numberOfFlashes * 2; i++)
        {
            spriteRenderer.material.SetFloat("_FlashAmount", makeSpriteWhite ? 1f : 0f);
            makeSpriteWhite = !makeSpriteWhite;
            yield return new WaitForSeconds(durationBetweenFlashes);
        }
    }
}