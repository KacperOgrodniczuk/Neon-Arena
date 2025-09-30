using System.Collections;
using UnityEngine.Pool;
using UnityEngine;

public class ParticleEffectRelease : MonoBehaviour
{
    ObjectPool<ParticleSystem> pool;

    ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void SetPool(ObjectPool<ParticleSystem> pool)
    { 
        this.pool = pool;
    }

    private void OnEnable()
    {
        StartCoroutine(ReleaseAfterDuration());

    }
    IEnumerator ReleaseAfterDuration()
    {
        // Calculate the duration of the particle effect and wait for said duration.
        float duration = _particleSystem.main.duration + _particleSystem.main.startLifetime.constantMax;
        yield return new WaitForSeconds(duration);

        pool.Release(_particleSystem);
    }
}
