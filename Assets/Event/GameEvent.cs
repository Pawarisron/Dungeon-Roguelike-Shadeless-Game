using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "GameEvent")]
public class GameEvent : ScriptableObject
{
    public List<GameEventListener> listeners = new List<GameEventListener>();
    
    //Raise event through different methods signatures
    public void Raise(Component sender, object data)
    {
        if (listeners == null)
        {
            Debug.LogError("Listeners list is null!");
            return;
        }

        for (int i = 0; i < listeners.Count; i++)
        {
            if (listeners[i] != null)
            {
                listeners[i].OnEventRaised(sender, data);
            }
        }
    }

    //Manage Listeners
    public void RegisterListener(GameEventListener listener)
    {
        if (listener != null && !listeners.Contains(listener))
            listeners.Add(listener);
    }
    public void UnregisterListener(GameEventListener listener)
    {
        if (listeners.Contains(listener))
            listeners.Remove(listener);
    }
}
