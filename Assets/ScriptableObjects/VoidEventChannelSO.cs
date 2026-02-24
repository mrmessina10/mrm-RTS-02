using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder.MeshOperations;

[CreateAssetMenu(fileName = "New void event", menuName = "ScriptableObjects/VoidEventChannelSO")]
public class VoidEventChannelSO : ScriptableObject
{
    public UnityAction OnEventRaised;

    public void RaiseEvent()
    {
        if (OnEventRaised != null)
            OnEventRaised.Invoke();
        else
            Debug.LogWarning("VoidEventChannelSO: No listeners for event " + name);
    }
}
