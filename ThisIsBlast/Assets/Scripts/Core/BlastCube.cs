using System.Collections.Generic;
using Enums;
using Managers;
using UnityEngine;
using DG.Tweening;

namespace Core
{
    public class BlastCube : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform visualTransform; 
        [SerializeField] private List<Renderer> renderers = new();
        
        [Header("Animation Settings")]
        [SerializeField] private float punchScaleAmount = 0.3f; 
        [SerializeField] private float punchRotateAmount = 45f;
        [SerializeField] private float animDuration = 0.5f;    
        [SerializeField] private int vibrato = 10;             
        [SerializeField] private float elasticity = 1f;  
        
        [Header("Wave Settings")]
        [SerializeField] private float waveDelayPerCube = 0.03f;
        [SerializeField] private float waveAmplitudeMultiplier = 0.7f;
        [SerializeField] private float waveAmplitudeDecay = 0.5f;
        [SerializeField] private float wavePunchStrength = 0.5f;

        public Vector2Int coordinates;
        public CubeColor color;
        public bool isTargeted;

        public void Register(int x, int y)
        {
            coordinates = new Vector2Int(x, y);
            name = $"Cube_{x},{y}";
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

        public void SetColor(CubeColor cubeColor)
        {
            color = cubeColor;
            isTargeted = false;
        }

        public void ReactHit(Vector3 hitDirection, bool isMainTarget, float amplitudeMultiplier = 1f)
        {
            visualTransform.DOKill(true); 
            visualTransform.localScale   = Vector3.one;
            visualTransform.localRotation = Quaternion.identity;

            float scalePunch   = punchScaleAmount  * amplitudeMultiplier;
            float rotatePunch  = punchRotateAmount * amplitudeMultiplier;

            hitDirection.y = 0f;
            if (hitDirection.sqrMagnitude < 0.0001f)
                hitDirection = Vector3.forward;
            hitDirection.Normalize();

            Vector3 rotationAxis = Vector3.Cross(Vector3.up, hitDirection).normalized;

            visualTransform.DOPunchScale(
                new Vector3(-scalePunch, scalePunch, -scalePunch), 
                animDuration, 
                vibrato, 
                elasticity
            );

            visualTransform.DOPunchRotation(
                rotationAxis * rotatePunch, 
                animDuration, 
                vibrato, 
                elasticity
            );

            if (isMainTarget)
            {
                TriggerWave(hitDirection);
                VFXManager.Instance.Play(VFXManager.Instance.innerSplashVfxPoolKey, transform);

                Sequence seq = DOTween.Sequence();
                seq.AppendInterval(animDuration * 0.01f);
                seq.Append(transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.OutQuad));
                seq.OnComplete(() => { GridManager.Instance.HandleCubeHit(this); });
            }
        }

        private void TriggerWave(Vector3 hitDirection)
        {
            var grid   = GridManager.Instance;
            var column = grid.GetColumn(coordinates.x);
            if (column == null) return;

            float currentDelay     = waveDelayPerCube;
            float currentAmplitude = waveAmplitudeMultiplier;

            for (int y = coordinates.y + 1; y < column.Count; y++)
            {
                var cube = column[y];
                if (!cube || !cube.gameObject.activeInHierarchy)
                    continue;

                var localCube = cube;
                float delay = currentDelay;
                float amp   = currentAmplitude;

                DOTween.Sequence()
                    .AppendInterval(delay)
                    .AppendCallback(() =>
                    {
                        if (localCube && localCube.gameObject.activeInHierarchy)
                            localCube.ReactHit(hitDirection, false, amp * wavePunchStrength);
                    });

                currentDelay     += waveDelayPerCube;
                currentAmplitude *= waveAmplitudeDecay;

                if (currentAmplitude < 0.05f)
                    break;
            }
        }
        
        
    }
}