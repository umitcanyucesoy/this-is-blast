using System;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using Enums;
using UnityEngine;

namespace Managers
{
    public class ShooterManager : MonoSingleton<ShooterManager>
    {
        [Serializable]
        public class ShooterData
        {
            public CubeColor color;
            public int bulletCount;
            public bool hidden;

            [NonSerialized] public ShooterCube RuntimeCube;
        }

        [Serializable]
        public class ShooterRowData
        {
            public List<ShooterData> columns = new();
        }

        [Header("Shooter Board")]
        public List<ShooterRowData> shooters = new();
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private bool centerOnRoot = true;

        [Header("References")] 
        [SerializeField] private ShooterCube shooterPrefab;
        [SerializeField] private Transform shooterRoot;
        public Transform exitPoint;
        public Transform slotExitPoint;

        [Header("Shooter Anim")]
        [SerializeField] private float shiftDuration = 0.25f;
        
        public void GenerateShooters()
        {
            ClearShooters();

            float xCenter = (shooters.Count - 1) * cellSize * 0.5f;

            for (int rowX = 0; rowX < shooters.Count; rowX++)
            {
                var row = shooters[rowX];

                for (int depthZ = 0; depthZ < row.columns.Count; depthZ++)
                {
                    var data = row.columns[depthZ];
                    var cube = Instantiate(shooterPrefab, shooterRoot);
                    cube.Register(rowX, depthZ); 

                    float posX = rowX * cellSize;
                    float posZ = -(depthZ * 2);

                    if (centerOnRoot)
                        posX -= xCenter;

                    cube.transform.localPosition = new Vector3(posX, 0, posZ);
                    data.RuntimeCube = cube;
                    
                    cube.Init(data.color, data.bulletCount);
                    cube.SetBulletCount(data.bulletCount);
                    cube.SetBulletVisible(!data.hidden);
                    
                    var mat = MaterialManager.Instance.Get(data.color);
                    if (!data.hidden) cube.ApplyMaterial(mat);
                    
                    var colorMat  = MaterialManager.Instance.Get(data.color);
                    var hiddenMat = MaterialManager.Instance.hiddenMaterial;

                    cube.ApplyMaterial(data.hidden ? hiddenMat : colorMat);
                    bool isFront = depthZ == 0;
                    
                    cube.SetOutline(MaterialManager.Instance.outlineMaterial, isFront);
                    cube.SetRunning(false);
                    cube.SetEmpty(!isFront);
                    cube.SetTextAlphaState(isFront);
                    
                }
                
                RevealFrontIfHidden(rowX);
            }
        }

        public void ColumnShift(ShooterCube shooter)
        {
            int rowX = shooter.coordinates.x;
            if (rowX < 0 || rowX >= shooters.Count) return;
    
            var row = shooters[rowX];
            if (row.columns.Count == 0) return;

            if (shooter.coordinates.y != 0 || row.columns[0]?.RuntimeCube != shooter) return;

            shooter.SetOutline(MaterialManager.Instance.outlineMaterial, false);
            row.columns.RemoveAt(0);
    
            float xCenter = (shooters.Count - 1) * cellSize * 0.5f;
            for (int depthZ = 0; depthZ < row.columns.Count; depthZ++)
            {
                var data = row.columns[depthZ];
                var cube = data.RuntimeCube;
                cube.Register(rowX, depthZ);
                
                float posX = rowX * cellSize;
                float posZ = -(depthZ * 2);

                if (centerOnRoot)
                    posX -= xCenter;

                var targetPos = new Vector3(posX, 0, posZ);

                cube.SetEmpty(false);
                cube.SetRunning(true);

                bool isFront = depthZ == 0;
                cube.SetOutline(MaterialManager.Instance.outlineMaterial, isFront);
                cube.SetTextAlphaState(isFront);

                cube.transform
                    .DOLocalMove(targetPos, shiftDuration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        cube.transform.localPosition = targetPos;
                        cube.SetRunning(false);

                        bool isFrontNow = cube.coordinates.y == 0;
                        cube.SetEmpty(!isFrontNow);
                    });
            }
    
            RevealFrontIfHidden(rowX);
        }

        private void ClearShooters()
        {
            foreach (var row in shooters)
                foreach (var data in row.columns)
                    if (data != null) data.RuntimeCube = null;

            for (int i = shooterRoot.childCount - 1; i >= 0; i--)
                Destroy(shooterRoot.GetChild(i).gameObject);
        }
        
        private void RevealFrontIfHidden(int rowX)
        {
            if (rowX < 0 || rowX >= shooters.Count) return;

            var row = shooters[rowX];
            if (row == null || row.columns == null || row.columns.Count == 0) return;
            var data = row.columns[0];
            if (!data.hidden) return;

            var cube = data.RuntimeCube;
            data.hidden = false;
            cube.SetBulletVisible(true);

            var mat = MaterialManager.Instance.Get(data.color);
            cube.ApplyMaterial(mat);
        }
    }
}
