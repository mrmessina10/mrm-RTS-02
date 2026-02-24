using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder.MeshOperations;

[CreateAssetMenu(fileName = "New float event", menuName = "ScriptableObjects/FloatEventChannelSO")]

public class FloatEventChannelSO : ScriptableObject
{
    public UnityAction OnEventRaised;

    public void RaiseEvent()
    {
        if (OnEventRaised != null)
            OnEventRaised.Invoke();
        else
            Debug.LogWarning("FloatEventChannelSO: No listeners for event " + name);
    }
}
