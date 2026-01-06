using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Managers
{
    public class MaterialManager : MonoSingleton<MaterialManager>
    {
        [Header("Material Lists")]
        [SerializeField] private List<Material> cubeMaterials = new();
        public Material hiddenMaterial;
        public Material outlineMaterial;

        public Material Get(CubeColor color)
        {
            int idx = (int)color;
            if (idx < 0 || idx >= cubeMaterials.Count) return null;
            return cubeMaterials[idx];
        }
    }
}