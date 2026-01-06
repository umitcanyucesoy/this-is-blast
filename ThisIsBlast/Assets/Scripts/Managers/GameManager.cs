using System;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class GameManager : MonoSingleton<GameManager>
    {
        public enum GameState
        {
            Playing,
            Won,
            Lost
        }

        public GameState currentState;
        public event UnityAction<GameState> OnGameStateChanged;
        
        private bool _isLoseCheckRunning;
        
        private void Start()
        {
            DOTween.SetTweensCapacity(1250, 100);
            currentState = GameState.Playing;
            
            PoolManager.Instance.InitializePools();
            GridManager.Instance.GenerateGrid();
            LevelManager.Instance.LoadLevel();
            SlotManager.Instance.Init();
            InputManager.Instance.Init();
        }

        public void CheckWinCondition()
        {
            if (currentState != GameState.Playing) return;
            if (GridManager.Instance.IsGridCleared()) SetState(GameState.Won);
        }

        public void CheckLoseCondition() => StartCoroutine(CheckLoseConditionRoutine());
        
        private IEnumerator CheckLoseConditionRoutine()
        {
            if (_isLoseCheckRunning || currentState != GameState.Playing)
                yield break;

            _isLoseCheckRunning = true;
            var wait = new WaitForSeconds(0.3f);

            while (currentState == GameState.Playing)
            {
                if (SlotManager.Instance.AreAllSlotsFull() &&
                    !HasAnyPossibleShot())
                {
                    SetState(GameState.Lost);
                    break;
                }

                yield return wait;
            }

            _isLoseCheckRunning = false;
        }
        
        private void SetState(GameState newState)
        {
            if (currentState == newState)
                return;

            currentState = newState;

            switch (currentState)
            {
                case GameState.Won:
                    LevelWon();
                    break;
                case GameState.Lost:
                    LevelLost();
                    break;
            }

            OnGameStateChanged?.Invoke(currentState);
        }

        private void LevelWon()
        {
            InputManager.Instance.enabled = false;
            UIManager.Instance.ShowWinPanel();
        }
        
        private void LevelLost()
        {
            InputManager.Instance.enabled = false;
            UIManager.Instance.ShowLosePanel();
        }
        
        public void RestartLevel()
        {
            DOTween.KillAll();
            
            currentState = GameState.Playing;
            InputManager.Instance.enabled = true;
            UIManager.Instance.HideWinPanel();
            UIManager.Instance.HideLosePanel();
            
            GridManager.Instance.GenerateGrid();   
            LevelManager.Instance.LoadLevel();    
            SlotManager.Instance.Init(); 
            InputManager.Instance.Init(); 
        }
        
        private bool HasAnyPossibleShot()
        {
            var grid = GridManager.Instance;

            foreach (var shooter in SlotManager.Instance.GetOccupiedShooters())
            {
                if (!shooter) 
                    continue;
                if (shooter.currentBulletCount <= 0)
                    continue;
                if (shooter.HasAnyPotentialTarget(grid))
                    return true;
            }

            return false;
        }
    }
}