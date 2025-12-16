using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// </summary>
/// <typeparam name="T">Controller</typeparam>
public class MachineManager<T> where T : MonoBehaviour
{
    public StateBase<T> CurrentState;
    public StateBase<T> PreviousState;

    public void LogicUpdate()
    {
        if (CurrentState != null)
            CurrentState.LogicUpdate();
    }
    public void PhysicsUpdate()
    {
        if (CurrentState != null)
            CurrentState.PhysicsUpdate();
    }
    public void Initialize(StateBase<T> startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }
    public void TransitionTo(StateBase<T> newState)
    {
        if(CurrentState != null)
        {
            CurrentState.Exit();
            PreviousState = CurrentState;
        }

        CurrentState = newState;
        CurrentState.Enter();
    }
}