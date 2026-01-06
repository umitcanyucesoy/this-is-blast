using System.Collections;
using Core;
using UnityEngine;

namespace Managers
{
    public class InputManager : MonoSingleton<InputManager>
    {
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask shooterLayerMask;

        public void Init()
        {
            cam = Camera.main;
            StartCoroutine(MouseDown());
        }

        private IEnumerator MouseDown()
        {
            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, 200f, shooterLayerMask))
                    {
                        var shooter = hit.collider.GetComponentInParent<ShooterCube>();

                        if (shooter)
                        {
                            if (shooter.coordinates.y != 0)
                            {
                                shooter.PlayLockedAnimation();
                            }
                            else
                            {
                                if (SlotManager.Instance.TryPlaceToFirstEmpty(shooter))
                                {
                                    ShooterManager.Instance.ColumnShift(shooter);
                                }
                                else
                                    Debug.Log("No empty slot.");
                            }
                        }
                    }
                }

                yield return null;
            }
        }
    }
}