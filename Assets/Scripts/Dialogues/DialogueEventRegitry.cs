using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueEventRegistry : MonoBehaviour
{
    [Serializable]
    public class DialogueEvent
    {
        public string id;
        public UnityEvent action;
    }

    [SerializeField] private DialogueEvent[] events;

    private Dictionary<string, UnityEvent> eventMap;

    void Awake()
    {
        eventMap = new Dictionary<string, UnityEvent>();

        foreach (var e in events)
        {
            if (!string.IsNullOrEmpty(e.id))
                eventMap[e.id] = e.action;
        }
    }

    public void Trigger(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
            return;

        if (eventMap.TryGetValue(eventId, out var action))
        {
            action.Invoke();
        }
        else
        {
            Debug.LogWarning($"DialogueEvent inconnu dans cette scène : {eventId}");
        }
    }
}
