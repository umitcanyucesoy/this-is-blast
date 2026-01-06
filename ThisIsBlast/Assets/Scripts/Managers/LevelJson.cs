using System;
using UnityEngine;

namespace Managers
{
    [Serializable]
    public class LevelJson
    {
        public GridJson grid;
        public ShooterBoardJson shooter;
    }

    [Serializable]
    public class GridJson
    {
        public GridRowJson[] rows;
    }
    
    [Serializable]
    public class GridRowJson
    {
        public string[] cells;
    }

    [Serializable]
    public class ShooterBoardJson
    {
        public ShooterRowJson[] rows;
    }

    [Serializable]
    public class ShooterRowJson
    {
        public ShooterCellJson[] columns;
    }

    [Serializable]
    public class ShooterCellJson
    {
        public string color;
        public int bulletCount;
        public bool hidden;
    }
}