using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

public partial class EventManager : Manager<EventManager>
{
    private ReaderWriterLockSlim eventLock = new ReaderWriterLockSlim();

    private Dictionary<Type, LinkedList<Delegate>> dicEvent = new Dictionary<Type, LinkedList<Delegate>>();

    private Queue<IEvent> queueEvent = new Queue<IEvent>();

    public void AddEvent<T>(Action<T> listener)where T:IEvent
    {
        var typeT = typeof(T);
        if (!dicEvent.ContainsKey(typeT))
        {
            dicEvent.Add(typeT, new LinkedList<Delegate>());
        }
        dicEvent[typeT].AddLast(listener);
    }

    public void RemoveEvent<T>(Action<T> listener)where T:IEvent
    {
        var typeT = typeof(T);
        if (dicEvent.ContainsKey(typeT))
        {
            dicEvent[typeT].Remove(listener);
            if (dicEvent[typeT].Count == 0)
            {
                dicEvent.Remove(typeT);
            }
        }
    }

    public bool SendEvent<T>(T evt) where T:IEvent
    {
        if (evt == null)
            return false;
        var typeT = evt.GetType();
        if (!dicEvent.ContainsKey(typeT) || dicEvent[typeT] == null)
            return false;

        if (!eventLock.IsWriteLockHeld && !eventLock.TryEnterWriteLock(200))
            return false;
        try
        {
            queueEvent.Enqueue(evt);
        }
        finally
        {
            try
            {
                eventLock.ExitWriteLock();
            }
            catch (SynchronizationLockException e)
            {
                Debug.LogError("SynchronizationLockException : " + e.Message);
            }
        }
        return true;
    }

    public bool SendEventImmediately<T>(T evt) where T:IEvent
    {
        if (evt == null)
            return false;
        var typeT = evt.GetType();
        if (!dicEvent.ContainsKey(typeT) || dicEvent[typeT] == null)
            return false;
        var handlerArray = new Queue<Delegate>();
        foreach (var handler in dicEvent[typeT])
        {
            handlerArray.Enqueue(handler);
        }

        while (handlerArray.Count > 0)
        {
            var handler = handlerArray.Dequeue();
            try
            {
                ((Action<T>)handler)?.Invoke(evt);
            }
            catch (Exception e)
            {
                Debug.LogError("Event触发回调异常 " + e);
            }
        }
        return true;
    }

    public void ClearEvent()
    {
        dicEvent.Clear();
    }

    void Update()
    {
        if (eventLock.IsWriteLockHeld || eventLock.TryEnterWriteLock(200))
        {
            try
            {
                while (queueEvent.Count > 0)
                {
                    IEvent qEvent = queueEvent.Dequeue();
                    var typeEvt = qEvent.GetType();
                    if (dicEvent.ContainsKey(typeEvt) && dicEvent[typeEvt] != null)
                    {
                        var handlerArray = new Queue<Delegate>();
                        foreach (var handler in dicEvent[typeEvt])
                        {
                            handlerArray.Enqueue(handler);
                        }
                        while (handlerArray.Count > 0)
                        {
                            var handler = handlerArray.Dequeue();
                            try
                            {
                                handler.DynamicInvoke(qEvent);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("Event触发回调异常 " + e);
                            }
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    if(eventLock.IsWriteLockHeld)
                        eventLock.ExitWriteLock();
                }
                catch (SynchronizationLockException e)
                {
                    Debug.LogError("SynchronizationLockException : " + e.Message);
                }
            }
        }
    }
}