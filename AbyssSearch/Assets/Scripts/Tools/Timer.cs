using UnityEngine;
using System;
using System.Collections.Generic;
using Framework;
using JetBrains.Annotations;
using Object = UnityEngine.Object;


public class Timer
{
    #region Public

    /// <summary>
    /// 定时器持续时间.
    /// </summary>
    public float duration { get; private set; }

    /// <summary>
    /// 是否循环.
    /// </summary>
    public bool isLooped { get; set; }

    /// <summary>
    /// 定时器是否结束，取消返回false.
    /// </summary>
    public bool isCompleted { get; private set; }


    public bool usesRealTime { get; private set; }

    /// <summary>
    /// 定时器是否暂停.
    /// </summary>
    public bool isPaused
    {
        get { return this._timeElapsedBeforePause.HasValue; }
    }

    /// <summary>
    /// 定时器是否取消.
    /// </summary>
    public bool isCancelled
    {
        get { return this._timeElapsedBeforeCancel.HasValue; }
    }

    public bool isDone
    {
        get { return this.isCompleted || this.isCancelled || this.isOwnerDestroyed; }
    }

    #endregion

    #region Public Static Methods

    /// 创建一个定时器
    public static Timer Register(float duration, Action onComplete, Action<float> onUpdate = null,
        bool isLooped = false, bool useRealTime = false, MonoBehaviour autoDestroyOwner = null)
    {
        // create a manager object to update all the timers if one does not already exist.

        Timer timer = new Timer(duration, onComplete, onUpdate, isLooped, useRealTime, autoDestroyOwner);
        Timer._manager.RegisterTimer(timer);
        return timer;
    }


    /// 取消一个定时器
    public static void Cancel(Timer timer)
    {
        if (timer != null)
        {
            timer.Cancel();
        }
    }

    /// 暂停一个定时器
    public static void Pause(Timer timer)
    {
        if (timer != null)
        {
            timer.Pause();
        }
    }


    ///唤醒一个定时器
    public static void Resume(Timer timer)
    {
        if (timer != null)
        {
            timer.Resume();
        }
    }

    public static void CancelAllRegisteredTimers()
    {
        if (Timer._manager != null)
        {
            Timer._manager.CancelAllTimers();
        }
    }

    public static void PauseAllRegisteredTimers()
    {
        if (Timer._manager != null)
        {
            Timer._manager.PauseAllTimers();
        }
    }

    public static void ResumeAllRegisteredTimers()
    {
        if (Timer._manager != null)
        {
            Timer._manager.ResumeAllTimers();
        }
    }

    #endregion

    #region Public Methods

    public void Cancel()
    {
        if (this.isDone)
        {
            return;
        }

        this._timeElapsedBeforeCancel = this.GetTimeElapsed();
        this._timeElapsedBeforePause = null;
    }

    public void Pause()
    {
        if (this.isPaused || this.isDone)
        {
            return;
        }

        this._timeElapsedBeforePause = this.GetTimeElapsed();
    }


    public void Resume()
    {
        if (!this.isPaused || this.isDone)
        {
            return;
        }

        this._timeElapsedBeforePause = null;
    }

    /// 获取定时器运行时间---循环状态为当次运行时间
    public float GetTimeElapsed()
    {
        if (this.isCompleted || this.GetWorldTime() >= this.GetFireTime())
        {
            return this.duration;
        }

        return this._timeElapsedBeforeCancel ??
               this._timeElapsedBeforePause ??
               this.GetWorldTime() - this._startTime;
    }

    /// 获取定时器剩余时间---循环状态为当次运行时间
    public float GetTimeRemaining()
    {
        return this.duration - this.GetTimeElapsed();
    }


    public float GetRatioComplete()
    {
        return this.GetTimeElapsed() / this.duration;
    }


    public float GetRatioRemaining()
    {
        return this.GetTimeRemaining() / this.duration;
    }

    #endregion

    #region Private Static Properties/Fields

    // responsible for updating all registered timers
    private static TimerManager _manager => TimerManager.Instance;

    #endregion

    #region Private Properties/Fields

    private bool isOwnerDestroyed
    {
        get { return this._hasAutoDestroyOwner && this._autoDestroyOwner == null; }
    }

    private readonly Action _onComplete;
    private readonly Action<float> _onUpdate;
    private float _startTime;
    private float _lastUpdateTime;


    private float? _timeElapsedBeforeCancel;
    private float? _timeElapsedBeforePause;


    private readonly MonoBehaviour _autoDestroyOwner;
    private readonly bool _hasAutoDestroyOwner;

    #endregion

    #region Private Constructor (use static Register method to create new timer)

    private Timer(float duration, Action onComplete, Action<float> onUpdate,
        bool isLooped, bool usesRealTime, MonoBehaviour autoDestroyOwner)
    {
        this.duration = duration;
        this._onComplete = onComplete;
        this._onUpdate = onUpdate;

        this.isLooped = isLooped;
        this.usesRealTime = usesRealTime;

        this._autoDestroyOwner = autoDestroyOwner;
        this._hasAutoDestroyOwner = autoDestroyOwner != null;

        this._startTime = this.GetWorldTime();
        this._lastUpdateTime = this._startTime;
    }

    #endregion

    #region Private Methods

    private float GetWorldTime()
    {
        return this.usesRealTime ? Time.realtimeSinceStartup : Time.time;
    }

    private float GetFireTime()
    {
        return this._startTime + this.duration;
    }

    private float GetTimeDelta()
    {
        return this.GetWorldTime() - this._lastUpdateTime;
    }

    private void Update()
    {
        if (this.isDone)
        {
            return;
        }

        if (this.isPaused)
        {
            this._startTime += this.GetTimeDelta();
            this._lastUpdateTime = this.GetWorldTime();
            return;
        }

        this._lastUpdateTime = this.GetWorldTime();

        if (this._onUpdate != null)
        {
            this._onUpdate(this.GetTimeElapsed());
        }

        if (this.GetWorldTime() >= this.GetFireTime())
        {
            if (this._onComplete != null)
            {
                this._onComplete();
            }

            if (this.isLooped)
            {
                this._startTime = this.GetWorldTime();
            }
            else
            {
                this.isCompleted = true;
            }
        }
    }

    #endregion

    #region Manager Class (implementation detail, spawned automatically and updates all registered timers)

    private class TimerManager : Manager<TimerManager>
    {
        private List<Timer> _timers = new List<Timer>();

        // buffer adding timers so we don't edit a collection during iteration
        private List<Timer> _timersToAdd = new List<Timer>();

        public void RegisterTimer(Timer timer)
        {
            this._timersToAdd.Add(timer);
        }

        public void CancelAllTimers()
        {
            foreach (Timer timer in this._timers)
            {
                timer.Cancel();
            }

            this._timers = new List<Timer>();
            this._timersToAdd = new List<Timer>();
        }

        public void PauseAllTimers()
        {
            foreach (Timer timer in this._timers)
            {
                timer.Pause();
            }
        }

        public void ResumeAllTimers()
        {
            foreach (Timer timer in this._timers)
            {
                timer.Resume();
            }
        }

        // update all the registered timers on every frame
        [UsedImplicitly]
        private void Update()
        {
            this.UpdateAllTimers();
        }

        private void UpdateAllTimers()
        {
            if (this._timersToAdd.Count > 0)
            {
                this._timers.AddRange(this._timersToAdd);
                this._timersToAdd.Clear();
            }

            foreach (Timer timer in this._timers)
            {
                timer.Update();
            }

            this._timers.RemoveAll(t => t.isDone);
        }
    }

    #endregion
}