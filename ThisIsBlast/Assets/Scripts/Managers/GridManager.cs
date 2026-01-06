using System.Collections.Generic;
using System.Linq;
using Core;
using DG.Tweening;
using Enums;
using UnityEngine;

namespace Managers
{
    public class GridManager : MonoSingleton<GridManager>
    {
        [Header("Grid Settings")]
        public int width = 10;
        public int height = 10;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private bool centerOnRoot = true;
        
        [Header("References")] 
        [SerializeField] private BlastCube cubePrefab;
        [SerializeField] private Transform gridRoot;
        
        private readonly List<List<BlastCube>> _columns = new();

        public void GenerateGrid()
        {
            ClearGrid();
            var xCenter = (width - 1) * cellSize * 0.5f;
            var zCenter = (height - 1) * cellSize * 0.5f;

            for (int x = 0; x < width; x++)
            {
                var col = new List<BlastCube>(height);
                _columns.Add(col);
                
                for (int y = 0; y < height; y++)
                {
                    var cube = Instantiate(cubePrefab, gridRoot);
                    cube.Register(x,y);
                    
                    var posX = x * cellSize;
                    var posZ = y * cellSize;
                    
                    if (centerOnRoot)
                    {
                        posX -= xCenter;
                        posZ -= zCenter;
                    }

                    cube.transform.localPosition = new Vector3(posX, .5f, posZ);
                    col.Add(cube);
                }
            }
        }
        
        public List<BlastCube> GetColumn(int x)
        {
            if (x < 0 || x >= width) return null;
            return _columns[x];
        }
        
        public BlastCube GetCell(int x, int y)
        {
            if (x < 0 || x >= width) return null;
            if (y < 0 || y >= height) return null;
            return _columns[x][y];
        }

        private void ClearGrid()
        {
            for (int i = gridRoot.childCount - 1; i >= 0; i--)
                Destroy(gridRoot.GetChild(i).gameObject);

            _columns.Clear();
        }
        
        private void ApplyColor(int x, int y, CubeColor color)
        {
            var cube = GetCell(x, y);
            if (cube == null) return;

            var mat = MaterialManager.Instance.Get(color);
            cube.ApplyMaterial(mat);
            cube.SetColor(color);
        }

        public void ApplyColorsFromRows(List<List<CubeColor>> rows)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ApplyColor(x, y, rows[y][x]);
                }
            }
        }
        
        public void HandleCubeHit(BlastCube cube)
        {
            var coord = cube.coordinates;
            if (coord.y == 0) CollapseColumnFromRow(coord.x, coord.y);
        }

        private void CollapseColumnFromRow(int x, int fromRow)
        {
            if (x < 0 || x >= width) return;
            if (fromRow < 0 || fromRow >= height) return;

            var column = _columns[x];
            if (column == null || column.Count != height) return;

            var removedCube = column[fromRow];
            if (!removedCube) return;

            removedCube.gameObject.SetActive(false);

            for (int y = fromRow + 1; y < height; y++)
            {
                var cube = column[y];
                if (!cube) continue;

                int newY = y - 1;
                column[newY] = cube;
                cube.Register(x, newY);

                float posX = x * cellSize;
                float posZ = newY * cellSize;

                if (centerOnRoot)
                {
                    float xCenter = (width - 1) * cellSize * 0.5f;
                    float zCenter = (height - 1) * cellSize * 0.5f;
                    posX -= xCenter;
                    posZ -= zCenter;
                }

                var targetPos = new Vector3(posX, 0.5f, posZ);

                cube.transform.DOKill();
                cube.transform.DOLocalMove(targetPos, 0.15f).SetEase(Ease.Linear);
            }

            column[height - 1] = removedCube;
            removedCube.coordinates = new Vector2Int(x, height - 1);
            GameManager.Instance.CheckWinCondition();
        }
        
        public bool IsGridCleared()
        {
            if (_columns == null || _columns.Count == 0)
                return false;

            for (int x = 0; x < _columns.Count; x++)
            {
                var column = _columns[x];
                if (column == null) 
                    continue;

                if (column.Any(cube => cube && cube.gameObject.activeSelf))
                    return false;
            }

            return true;
        }
    }
}