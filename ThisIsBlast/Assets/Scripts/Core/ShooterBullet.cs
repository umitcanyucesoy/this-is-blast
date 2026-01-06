using System.Collections;
using DG.Tweening;
using Managers;
using UnityEngine;

namespace Core
{
    public class ShooterBullet : MonoBehaviour
    {
        [SerializeField] private float speed = 20f;

        private BlastCube _target;
        private string _poolKey;
        private Tween _moveTween;
        private Coroutine _flyRoutine;
        private bool _hasHitTarget;

        public void Init(BlastCube target, string poolKey)
        {
            _target = target;
            _poolKey = poolKey;
            _hasHitTarget = false;

            if (_flyRoutine != null)
                StopCoroutine(_flyRoutine);

            _moveTween?.Kill();
            _moveTween = null;

            _flyRoutine = StartCoroutine(FlyToTargetRoutine());
        }

        private IEnumerator FlyToTargetRoutine()
        {
            if (!_target || !_target.gameObject.activeInHierarchy)
            {
                Despawn();
                yield break;
            }

            Vector3 startPos  = transform.position;
            Vector3 targetPos = _target.transform.position;
            float distance    = Vector3.Distance(startPos, targetPos);

            if (distance <= 0.001f)
            {
                if (!_hasHitTarget)
                {
                    var dir = targetPos - startPos;
                    _target.ReactHit(dir, true);
                    _hasHitTarget = true;
                }

                Despawn();
                yield break;
            }

            float duration = distance / speed;

            _moveTween = transform.DOMove(targetPos, duration)
                                  .SetEase(Ease.Linear);

            yield return _moveTween.WaitForCompletion();

            if (!_hasHitTarget && _target != null && _target.gameObject.activeInHierarchy)
            {
                var dir = _target.transform.position - transform.position;
                _target.ReactHit(dir, true);
                _hasHitTarget = true;
            }

            Despawn();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<BlastCube>(out var cube))
                return;

            if (!cube.gameObject.activeInHierarchy)
                return;

            var dir = cube.transform.position - transform.position;

            bool isMainTarget = cube == _target;

            cube.ReactHit(dir, isMainTarget);

            if (isMainTarget)
                _hasHitTarget = true;
        }

        private void Despawn()
        {
            _moveTween?.Kill();
            _moveTween = null;

            if (_flyRoutine != null)
            {
                StopCoroutine(_flyRoutine);
                _flyRoutine = null;
            }

            var key = _poolKey;
            _target = null;
            _poolKey = null;

            if (!string.IsNullOrEmpty(key))
                PoolManager.Instance.Despawn(key, gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
