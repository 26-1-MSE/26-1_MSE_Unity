using UnityEngine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public abstract class GameState
    {
        public abstract void OnEnter();
        public abstract void OnExit();
        public abstract void HandleInput();
        public abstract void Update();
        public abstract string GetStateName();
    }

