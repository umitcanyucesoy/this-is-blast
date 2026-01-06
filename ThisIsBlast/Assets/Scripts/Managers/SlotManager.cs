using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

namespace Managers
{
    public class SlotManager : MonoSingleton<SlotManager>
    {
        [SerializeField] private List<Transform> slots = new();

        private ShooterCube[] _occupied;

        public void Init() => _occupied = new ShooterCube[slots.Count];

        public bool TryPlaceToFirstEmpty(ShooterCube shooter)
        {
            if (!shooter || IsPlaced(shooter))
                return false;

            for (int i = 0; i < slots.Count; i++)
            {
                if (_occupied[i])
                    continue;

                _occupied[i] = shooter;
                shooter.MoveToSlot(slots[i]);
                GameManager.Instance.CheckLoseCondition();
                return true;
            }

            return false;
        }

        private bool IsPlaced(ShooterCube shooter)
        {
            return _occupied != null && _occupied.Any(t => t == shooter);
        }

        public void ReleaseSlot(ShooterCube shooter)
        {
            if (_occupied == null || !shooter) return;

            for (int i = 0; i < _occupied.Length; i++)
            {
                if (_occupied[i] == shooter)
                {
                    _occupied[i] = null;
                    return;
                }
            }
        }
        
        public bool AreAllSlotsFull()
        {
            if (_occupied == null || _occupied.Length == 0)
                return false;

            return _occupied.All(t => t);
        }

        public IEnumerable<ShooterCube> GetOccupiedShooters()
        {
            if (_occupied == null) yield break;
            foreach (var shooter in _occupied)
            {
                if (shooter) yield return shooter;
            }
        }
    }
}