using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ParticleEffectManager : MonoBehaviour
{
    //TODO: Rework this to use scriptable object based event channels when more particle events are needed.

    //  Structure for each of the particle pools
    [System.Serializable]
    struct ParticleEffect
    {
        public string id;   // e.g. "ProjectileHit", "PlayerDeath"
        public ParticleSystem particlePrefab;
        public int initialPoolSize;
        public int maxPoolSize;
    }

    // List of particle effects to pool and a disctionary for quick access
    [SerializeField] private ParticleEffect[] particleEffects;
    private Dictionary<string, ObjectPool<ParticleSystem>> particlePools;

    public static ParticleEffectManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitialisePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitialisePools()
    {
        //  Initialise a separate pool for each particle effect we have, create a separete gameobject to act as a parent for each pool.
        particlePools = new Dictionary<string, ObjectPool<ParticleSystem>>();

        foreach (ParticleEffect particleEffect in particleEffects)
        {
            GameObject poolParent = new GameObject(particleEffect.id + " Particle Pool");

            ObjectPool<ParticleSystem> pool = new ObjectPool<ParticleSystem>(
                () => CreatePooledItem(particleEffect, poolParent.transform),
                OnGetFromPool,
                OnReturnToPool,
                OnDestroyPoolObject,
                false,
                particleEffect.initialPoolSize,
                particleEffect.maxPoolSize
            );

            particlePools.Add(particleEffect.id, pool);
        }
    }

    ParticleSystem CreatePooledItem(ParticleEffect particleEffect, Transform parent)
    {
        ParticleSystem newParticle = Instantiate(particleEffect.particlePrefab, parent);

        // Attacch the poolreleaser to the particle system prefab. Done here since I will probably forget to attach it in the editor.
        ParticleEffectRelease poolReleaser = newParticle.gameObject.AddComponent<ParticleEffectRelease>();
        if (particlePools.TryGetValue(particleEffect.id, out ObjectPool<ParticleSystem> sourcePool))
        { 
            poolReleaser.SetPool(sourcePool);
        }

        newParticle.gameObject.SetActive(false);
        return newParticle;
    }

    void OnGetFromPool(ParticleSystem particleSystem)
    { 
        particleSystem.gameObject.SetActive(true);
    }

    void OnReturnToPool(ParticleSystem particleSystem)
    { 
        particleSystem.gameObject.SetActive(false);
        particleSystem.Stop();
    }

    void OnDestroyPoolObject(ParticleSystem particleSystem)
    {
        Destroy(particleSystem.gameObject);
    }

    public void PlayEffect(string id, Vector3 position, Quaternion rotation)
    {
        if (!particlePools.TryGetValue(id, out ObjectPool<ParticleSystem> pool))
        {
            Debug.LogError($"Particle effect ID '{id}' not found");
            return;
        }

        ParticleSystem effectToPlay = pool.Get();
        effectToPlay.transform.SetPositionAndRotation(position, rotation);
        effectToPlay.Play();
    }
}