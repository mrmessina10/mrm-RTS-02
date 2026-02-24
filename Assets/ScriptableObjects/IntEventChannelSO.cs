using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder.MeshOperations;

[CreateAssetMenu(fileName = "New int event", menuName = "ScriptableObjects/IntEventChannelSO")]
public class IntEventChannelSO : ScriptableObject
{
    public UnityAction OnEventRaised;

    public void RaiseEvent()
    {
        if (OnEventRaised != null)
            OnEventRaised.Invoke();
        else
            Debug.LogWarning("IntEventChannelSO: No listeners for event " + name);
    }
}