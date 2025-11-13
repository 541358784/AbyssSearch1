using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColliderCheckController:Singleton<ColliderCheckController>
{
    public float Width;//区域宽度
    public float Height;//区域高度
    public int ColumnCount;//列数，X方向上的格子数
    public int RowCount;//行数，Y方向上的格子数
    public float GridWidth => Width / ColumnCount;//格子宽度
    public float GridHeight => Height / RowCount;//格子高度
    public ColliderCheckGrid[,] Grids;
    public Dictionary<ICollider, GridRect> IColliderGridMaps;
    public List<ICollider> TriggerIColliders;
    public HashSet<(ICollider, ICollider)> IColliderDeduplicationSet;//碰撞去重表
    public ColliderType TriggerType = ColliderType.PlayerBullet | ColliderType.Player;//用来遍历的触发器类型，玩家的子弹数量比较可控
    private bool IsInit = false;
    public void Init()
    {
        Grids = new ColliderCheckGrid[ColumnCount,RowCount];
        for (var i = 0; i < ColumnCount; i++)
        {
            for (var j = 0; j < RowCount; j++)
            {
                var grid = new ColliderCheckGrid(i, j);
                Grids[i,j] = grid;
            }
        }
        IColliderGridMaps = new Dictionary<ICollider, GridRect>();
        TriggerIColliders = new List<ICollider>();
        EventManager.Instance.RemoveEvent<EventColliderPositionChange>(OnIColliderLocalPositionChange);
        EventManager.Instance.AddEvent<EventColliderPositionChange>(OnIColliderLocalPositionChange);
        IsInit = true;
        while (PreRegisterQueue.Count > 0)
        {
            Register(PreRegisterQueue.Dequeue());
        }
    }

    public GridRect GetIColliderGridRect(ICollider ICollider)
    {
        var IColliderPosition = ICollider.ColliderPosition + new Vector3(Width / 2, Height / 2);
        var minPoint = IColliderPosition - new Vector3(-ICollider.CollideAreaRadius, -ICollider.CollideAreaRadius);
        var maxPoint = IColliderPosition + new Vector3(-ICollider.CollideAreaRadius, -ICollider.CollideAreaRadius);
        var minX = (int)(minPoint.x / GridWidth);
        var minY = (int)(minPoint.y / GridHeight);
        var maxX = (int)(maxPoint.x / GridWidth);
        var maxY = (int)(maxPoint.y / GridHeight);
        var rect = new GridRect(minX, minY, maxX, maxY);
        return rect;
    }
    public void OnIColliderLocalPositionChange(EventColliderPositionChange evt)
    {
        if (!IColliderGridMaps.TryGetValue(evt.Collider,out var oldRect))
        {
            return;
        }
        var rect = GetIColliderGridRect(evt.Collider);
        GetDiff(oldRect, rect, out var removeList, out var addList);
        IColliderGridMaps[evt.Collider] = rect;
        if (removeList.Count > 0)
        {
            foreach (var pair in removeList)
            {
                Grids[pair.Item1,pair.Item2].Remove(evt.Collider);
            }
        }
        if (addList.Count > 0)
        {
            foreach (var pair in addList)
            {
                Grids[pair.Item1,pair.Item2].Add(evt.Collider);
            }
        }
    }

    #region GridDiff
    public struct GridRect
    {
        public int minX, minY, maxX, maxY;
        public GridRect(int minX, int minY, int maxX, int maxY)
        {
            this.minX = minX; this.minY = minY;
            this.maxX = maxX; this.maxY = maxY;
        }
    }

    public static void GetDiff(GridRect oldR, GridRect newR,
        out List<(int x, int y)> removed,
        out List<(int x, int y)> added)
    {
        removed = new();
        added = new();

        int interMinX = Math.Max(oldR.minX, newR.minX);
        int interMaxX = Math.Min(oldR.maxX, newR.maxX);
        int interMinY = Math.Max(oldR.minY, newR.minY);
        int interMaxY = Math.Min(oldR.maxY, newR.maxY);

        bool hasOverlap = interMinX <= interMaxX && interMinY <= interMaxY;

        // ==============
        // 1️⃣ 处理新增
        // ==============
        if (hasOverlap)
        {
            // top band
            for (int y = interMaxY + 1; y <= newR.maxY; y++)
                for (int x = newR.minX; x <= newR.maxX; x++)
                    added.Add((x, y));

            // bottom band
            for (int y = newR.minY; y < interMinY; y++)
                for (int x = newR.minX; x <= newR.maxX; x++)
                    added.Add((x, y));

            // left band
            for (int y = interMinY; y <= interMaxY; y++)
                for (int x = newR.minX; x < interMinX; x++)
                    added.Add((x, y));

            // right band
            for (int y = interMinY; y <= interMaxY; y++)
                for (int x = interMaxX + 1; x <= newR.maxX; x++)
                    added.Add((x, y));
        }
        else
        {
            // 没有交集：全部新增
            for (int y = newR.minY; y <= newR.maxY; y++)
                for (int x = newR.minX; x <= newR.maxX; x++)
                    added.Add((x, y));
        }

        // ==============
        // 2️⃣ 处理移除
        // ==============
        if (hasOverlap)
        {
            // top band (old region's top part removed)
            for (int y = interMaxY + 1; y <= oldR.maxY; y++)
                for (int x = oldR.minX; x <= oldR.maxX; x++)
                    removed.Add((x, y));

            // bottom band
            for (int y = oldR.minY; y < interMinY; y++)
                for (int x = oldR.minX; x <= oldR.maxX; x++)
                    removed.Add((x, y));

            // left band
            for (int y = interMinY; y <= interMaxY; y++)
                for (int x = oldR.minX; x < interMinX; x++)
                    removed.Add((x, y));

            // right band
            for (int y = interMinY; y <= interMaxY; y++)
                for (int x = interMaxX + 1; x <= oldR.maxX; x++)
                    removed.Add((x, y));
        }
        else
        {
            // 没有交集：全部移除
            for (int y = oldR.minY; y <= oldR.maxY; y++)
                for (int x = oldR.minX; x <= oldR.maxX; x++)
                    removed.Add((x, y));
        }
    }
    #endregion

    private Queue<ICollider> PreRegisterQueue = new Queue<ICollider>();
    
    public ColliderCheckController()
    {
        EventManager.Instance.AddEvent<EventColliderRegister>(RegisterEvent);
        EventManager.Instance.AddEvent<EventColliderUnRegister>(UnRegisterEvent);
    }

    public void RegisterEvent(EventColliderRegister evt)
    {
        Register(evt.Obj);
    }
    public void UnRegisterEvent(EventColliderUnRegister evt)
    {
        UnRegister(evt.Obj);
    }
    public void Register(ICollider ICollider)
    {
        if (!IsInit)//未初始化，暂存注册的碰撞体等初始化后再注册
        {
            PreRegisterQueue.Enqueue(ICollider);
            return;
        }
        if (IColliderGridMaps.ContainsKey(ICollider))
        {
            Debug.LogError("重复注册碰撞体");
            return;   
        }

        var rect = GetIColliderGridRect(ICollider);
        for (var x = rect.minX; x <= rect.maxX; x++)
        {
            for (var y = rect.minY; y <= rect.maxY; y++)
            {
                Grids[x,y].Add(ICollider);
            }
        }
        IColliderGridMaps.Add(ICollider,rect);
        if (TriggerType.HasFlag(ICollider.Type))
        {
            TriggerIColliders.Add(ICollider);
        }
    }

    public void UnRegister(ICollider ICollider)
    {
        if (!IColliderGridMaps.TryGetValue(ICollider,out var rect))
        {
            return;   
        }
        for (var x = rect.minX; x <= rect.maxX; x++)
        {
            for (var y = rect.minY; y <= rect.maxY; y++)
            {
                Grids[x,y].Remove(ICollider);
            }
        }
        IColliderGridMaps.Remove(ICollider);
        if (TriggerType.HasFlag(ICollider.Type))
        {
            TriggerIColliders.Remove(ICollider);
        }
    }


    public void CheckICollider()//检测碰撞
    {
        IColliderDeduplicationSet.Clear();
        foreach (var triggerICollider in TriggerIColliders)//遍历每个触发器
        {
            var triggerTypes = ColliderHandler.Instance.GetTriggerTypes(triggerICollider.Type);
            var rect = IColliderGridMaps[triggerICollider];
            for (var x = rect.minX; x <= rect.maxX; x++)//遍历触发器占据的每个格子
            {
                for (var y = rect.minY; y <= rect.maxY; y++)
                {
                    foreach (var ICollider in Grids[x, y].GetContains(triggerTypes))//遍历格子内每个类型正确的碰撞体
                    {
                        if (ICollider == triggerICollider)
                            continue;
                        var distance = ICollider.ColliderPosition - triggerICollider.ColliderPosition;
                        var distanceValue = distance.x * distance.x + distance.y * distance.y;
                        var radius = ICollider.CollideAreaRadius + triggerICollider.CollideAreaRadius;
                        var radiusSquare = radius * radius;
                        if (distanceValue <= radiusSquare)//圆形碰撞体判断
                        {
                            if (IColliderDeduplicationSet.Contains((triggerICollider, ICollider)))//碰撞去重
                            {
                                continue;
                            }
                            IColliderDeduplicationSet.Add((triggerICollider, ICollider));//正反都加,避免双向检测
                            IColliderDeduplicationSet.Add((ICollider, triggerICollider));
                            EventManager.Instance.SendEvent(new EventColliderTrigger(triggerICollider, ICollider));//发送碰撞事件
                        }
                    }
                }
            }
        }
    }
}