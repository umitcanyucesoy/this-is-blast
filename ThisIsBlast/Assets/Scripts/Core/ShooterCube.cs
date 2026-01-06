using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Enums;
using Managers;
using TMPro;
using UnityEngine;

namespace Core
{
    public class ShooterCube : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private List<Renderer> renderers = new();
        [SerializeField] private TextMeshPro bulletText;
        [SerializeField] private float alpha = 0.4f;
        [SerializeField] private float fadeDuration = 0.15f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int IsShootingHash = Animator.StringToHash("IsShooting");
        private static readonly int IsEmptyHash = Animator.StringToHash("IsEmpty");

        [Header("Slot Path Settings")]
        [SerializeField] private float moveSpeed = 8f;         
        [SerializeField] private float xDeltaRange = 12f;      
        [SerializeField] private float standingOffsetAmount = 2f;
        [SerializeField] private float shooterOffsetAmount  = 1f;
        [SerializeField] private float exitDuration = 10f;
        
        [Header("Firing Settings")]
        [SerializeField] private float fireRate = 0.1f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private string bulletPoolKey = "Bullet";
        
        public Vector2Int coordinates;
        public CubeColor color;
        public int currentBulletCount;
        private int _scanX;
        private bool _isExiting;

        public void Init(CubeColor cubeColor, int bulletCount)
        {
            color = cubeColor;
            currentBulletCount = bulletCount;
            SetBulletCount(currentBulletCount);
        }

        public void Register(int column, int row)
        {
            coordinates = new Vector2Int(column, row);
            name = $"Shooter_{column},{row}";
        }

        public void ApplyMaterial(Material mat)
        {
            foreach (var r in renderers)
            {
                var mats = r.sharedMaterials;
                mats[0] = mat;
                r.sharedMaterials = mats;
            }
        }
        
        public void SetOutline(Material outlineMat, bool enabled)
        {
            foreach (var r in renderers)
            {
                var mats = r.sharedMaterials;

                if (enabled && (mats == null || mats.Length < 2))
                {
                    var newMats = new Material[2];
                    if (mats != null && mats.Length > 0)
                        newMats[0] = mats[0];
                    newMats[1] = outlineMat;
                    mats = newMats;
                }

                if (mats == null || mats.Length < 2)
                    continue;

                mats[1] = (enabled) ? outlineMat : null;
                r.sharedMaterials = mats;
            }
        }

        public void SetBulletCount(int count)
        {
            currentBulletCount = count;
            bulletText.text = count.ToString();
        }
        
        public void SetBulletVisible(bool visible) => bulletText.gameObject.SetActive(visible);
        public void SetRunning(bool isRunning) => animator.SetBool(IsRunningHash, isRunning);
        public void SetEmpty(bool isEmpty) => animator.SetBool(IsEmptyHash, isEmpty);
        
        public void SetTextAlphaState(bool isFront)
        {
            float targetAlpha = isFront ? 1f : alpha;
            bulletText.DOFade(targetAlpha, fadeDuration);
        }
        
        public void PlayLockedAnimation()
        {
            if (DOTween.IsTweening(transform)) return;
            transform.DOPunchRotation(Vector3.up * 25f, 0.4f, 10, 1f);
        }
        
        public void MoveToSlot(Transform slotTransform)
        {
            Transform tr = transform;
            tr.DOKill();
            var targetPositions = new List<Vector3>();

            float xDelta = tr.position.x - slotTransform.position.x;
            float t = Mathf.InverseLerp(-xDeltaRange, xDeltaRange, xDelta);

            Vector3 standingTileOffset = Vector3.Lerp(Vector3.back  * standingOffsetAmount, Vector3.back * standingOffsetAmount, t);
            Vector3 shooterTileOffset  = Vector3.Lerp(Vector3.forward * shooterOffsetAmount, Vector3.forward  * shooterOffsetAmount,  t);
            
            targetPositions.Add(tr.position + shooterTileOffset);
            targetPositions.Add(slotTransform.position + standingTileOffset);
            targetPositions.Add(slotTransform.position);

            SetRunning(true);
            Tween path = null;
            path = tr.DOPath(targetPositions.ToArray(), moveSpeed, PathType.CatmullRom, PathMode.Full3D, 50, Color.black)
                     .SetEase(Ease.Linear)
                     .SetSpeedBased()
                     .SetLookAt(0.01f) 
                     .OnUpdate(() =>
                     {
                         if (animator && path.ElapsedPercentage() >= 0.95f)
                             animator.SetBool(IsRunningHash, false);
                     })
                     .OnComplete(() =>
                     {
                         if (!tr) return;

                         tr.position = slotTransform.position;
                         tr.DOLocalRotate(Vector3.zero, 0.1f); 
                         animator.SetBool(IsRunningHash, false);
                         StartCoroutine(FireRoutine());
                     });
        }

        private void MoveToExit()
        {
            if (_isExiting) return;
            _isExiting = true;

            SlotManager.Instance.ReleaseSlot(this);
           
            animator.SetBool(IsShootingHash, false);
            animator.SetBool(IsRunningHash, true);
            
            var exit = ShooterManager.Instance.exitPoint;
            var slotExit = ShooterManager.Instance.slotExitPoint; 
            
            var points = new List<Vector3>
            {
                transform.position
            };

            var midPoint = new Vector3(
                transform.position.x,
                transform.position.y,
                slotExit.position.z
            );
            
            points.Add(midPoint);
            points.Add(exit.position);

            transform.DOPath(points.ToArray(), exitDuration, PathType.CatmullRom)
                .SetEase(Ease.Linear)
                .SetSpeedBased()
                .SetLookAt(0.01f) 
                .OnComplete(() =>
                {
                    animator.SetBool(IsRunningHash, false);
                    gameObject.SetActive(false);
                });
        }

        private IEnumerator FireRoutine()
        {
            var grid = GridManager.Instance;

            while (currentBulletCount > 0)
            {
                var target = FindNextTarget(grid);
                if (!target)
                {
                    if (animator)
                        animator.SetBool(IsShootingHash, false);

                    transform.DOKill();
                    transform.DOLocalRotate(Vector3.zero, 0.15f)
                        .SetEase(Ease.OutQuad);

                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

               
                animator.SetBool(IsShootingHash, true);
                target.isTargeted = true;

                var lookPos = target.transform.position;
                transform.DOKill();
                transform.DOLookAt(lookPos, 0.1f)
                    .SetEase(Ease.OutQuad);

                Vector3 origin    = firePoint.position;
                Vector3 targetPos = target.transform.position;
                Vector3 direction = (targetPos - origin).normalized;
                var rotation      = Quaternion.LookRotation(direction, Vector3.up);

                var bulletObj = PoolManager.Instance.Spawn(bulletPoolKey, origin, rotation);
                bulletObj.TryGetComponent<ShooterBullet>(out var bullet);
                bullet.Init(target, bulletPoolKey);
                VFXManager.Instance.Play(VFXManager.Instance.splashVfxPoolKey, firePoint);
                SoundManager.Instance.PlayPitch("SplashFX", .35f);

                currentBulletCount--;
                SetBulletCount(currentBulletCount);

                yield return new WaitForSeconds(fireRate);
            }

            animator.SetBool(IsShootingHash, false);
            transform.DOKill();
            transform.DOLocalRotate(Vector3.zero, 0.15f)
                .SetEase(Ease.OutQuad);

            if (currentBulletCount <= 0)
                MoveToExit();
        }

        private BlastCube FindNextTarget(GridManager grid)
        {
            int width = grid.width;
            int startX = _scanX;

            for (int step = 0; step < width; step++)
            {
                int x = (startX + step) % width;
                var cube = grid.GetCell(x, 0);
                if (!cube
                    || !cube.gameObject.activeInHierarchy || cube.color != color || cube.isTargeted)
                    continue;
                
                _scanX = (x + 1) % width;
                return cube;
            }

            return null;
        }
        
        public bool HasAnyPotentialTarget(GridManager grid)
        {
            int width = grid.width;

            for (int x = 0; x < width; x++)
            {
                var cube = grid.GetCell(x, 0);
                if (!cube) 
                    continue;

                if (!cube.gameObject.activeInHierarchy)
                    continue;

                if (cube.color == color)
                    return true;
            }

            return false;
        }
    }
}
