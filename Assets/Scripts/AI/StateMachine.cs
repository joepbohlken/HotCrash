using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public bool debugging = false;

    public BaseState currentState { get; private set; }

    private void Update()
    {
        if (currentState != null)
            currentState.LogicUpdate();
    }

    protected void Initialize(BaseState startingState)
    {
        currentState = startingState;
        startingState.Enter();
    }

    public void ChangeState(BaseState newState)
    {
        currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!debugging) return;
        string stateName = currentState != null ? currentState.ToString() : "No state";
        GUILayout.Label($"<color='white'><size=40>{stateName}</size></color>");
    }
#endif
}
