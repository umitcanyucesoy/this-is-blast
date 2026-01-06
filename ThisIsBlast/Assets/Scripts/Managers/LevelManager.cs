using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Managers
{
    public class LevelManager : MonoSingleton<LevelManager>
    {
        [SerializeField] private TextAsset levelJson;

        public void LoadLevel()
        {
            var data = JsonUtility.FromJson<LevelJson>(levelJson.text);
            
            ApplyGrid(data.grid);
            ApplyShooter(data.shooter);
        }

        private void ApplyGrid(GridJson grid)
        {
            var rows = new List<List<CubeColor>>(10);
            
            for (int y = 0; y < 10; y++)
            {
                var row = grid.rows[y];
                var list = new List<CubeColor>(10);
                for (int x = 0; x < 10; x++)
                    list.Add(ParseColor(row.cells[x]));
                
                rows.Add(list);
            }
            
            GridManager.Instance.ApplyColorsFromRows(rows);
        }

        private void ApplyShooter(ShooterBoardJson shooter)
        {
            var target = ShooterManager.Instance.shooters;
            target.Clear();

            foreach (var srcRow in shooter.rows)
            {
                var dstRow = new ShooterManager.ShooterRowData();

                foreach (var col in srcRow.columns)
                {
                    var dest = new ShooterManager.ShooterData
                    {
                        color = ParseColor(col.color),
                        bulletCount = col.bulletCount,
                        hidden = col.hidden
                    };
                    
                    dstRow.columns.Add(dest);
                }
                
                target.Add(dstRow);
            }
            
            ShooterManager.Instance.GenerateShooters();
        }

        private CubeColor ParseColor(string cell)
        {
            if (string.IsNullOrEmpty(cell)) return CubeColor.Red;
            
            if (cell == "R") return CubeColor.Red;
            if (cell == "B") return CubeColor.Blue;
            if (cell == "G") return CubeColor.Green;
            if (cell == "Y") return CubeColor.Yellow;
            if (cell == "O") return CubeColor.Orange;

            return CubeColor.Red;
        }
    }
}