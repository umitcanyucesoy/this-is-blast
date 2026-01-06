using System.Collections;
using UnityEngine;

namespace Managers
{
    public class VFXManager : MonoSingleton<VFXManager>
    {
        public string splashVfxPoolKey = "SplashFX";
        public string innerSplashVfxPoolKey = "InnerSplashFX";
      
        public GameObject Play(string poolKey, Transform origin)
        {
            if (string.IsNullOrEmpty(poolKey))
                return null;

            var vfx = PoolManager.Instance.Spawn(poolKey, origin.position, origin.rotation);
            StartCoroutine(DespawnRoutine(poolKey, vfx));
            return vfx;
        }

        private IEnumerator DespawnRoutine(string poolKey, GameObject vfx)
        {
            vfx.TryGetComponent<ParticleSystem>(out var ps);
            
            ps.Play();
            yield return new WaitWhile(() => ps.IsAlive(true));
            PoolManager.Instance.Despawn(poolKey, vfx);
        }
    }
}