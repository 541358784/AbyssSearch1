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
    public Dictionary<Collider, GridRect> ColliderGridMaps;
    public List<Collider> TriggerColliders;
    public HashSet<(int, int)> ColliderDeduplicationSet;//碰撞去重表
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
        ColliderGridMaps = new Dictionary<Collider, GridRect>();
        TriggerColliders = new List<Collider>();
        EventManager.Instance.RemoveEvent<EventColliderLocalPositionChange>(OnColliderLocalPositionChange);
        EventManager.Instance.AddEvent<EventColliderLocalPositionChange>(OnColliderLocalPositionChange);
        IsInit = true;
        while (PreRegisterQueue.Count > 0)
        {
            Register(PreRegisterQueue.Dequeue());
        }
    }

    public GridRect GetColliderGridRect(Collider collider)
    {
        var colliderPosition = collider.LocalPosition + new Vector3(Width / 2, Height / 2);
        var minPoint = colliderPosition - new Vector3(-collider.CollideAreaRadius, -collider.CollideAreaRadius);
        var maxPoint = colliderPosition + new Vector3(-collider.CollideAreaRadius, -collider.CollideAreaRadius);
        var minX = (int)(minPoint.x / GridWidth);
        var minY = (int)(minPoint.y / GridHeight);
        var maxX = (int)(maxPoint.x / GridWidth);
        var maxY = (int)(maxPoint.y / GridHeight);
        var rect = new GridRect(minX, minY, maxX, maxY);
        return rect;
    }
    public void OnColliderLocalPositionChange(EventColliderLocalPositionChange evt)
    {
        if (!ColliderGridMaps.TryGetValue(evt.Collider,out var oldRect))
        {
            return;
        }
        var rect = GetColliderGridRect(evt.Collider);
        GetDiff(oldRect, rect, out var removeList, out var addList);
        ColliderGridMaps[evt.Collider] = rect;
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

    private Queue<Collider> PreRegisterQueue = new Queue<Collider>();
    public void Register(Collider collider)
    {
        if (!IsInit)//未初始化，暂存注册的碰撞体等初始化后再注册
        {
            PreRegisterQueue.Enqueue(collider);
            return;
        }
        if (ColliderGridMaps.ContainsKey(collider))
        {
            Debug.LogError("重复注册碰撞体");
            return;   
        }

        var rect = GetColliderGridRect(collider);
        for (var x = rect.minX; x <= rect.maxX; x++)
        {
            for (var y = rect.minY; y <= rect.maxY; y++)
            {
                Grids[x,y].Add(collider);
            }
        }
        ColliderGridMaps.Add(collider,rect);
        if (TriggerType.HasFlag(collider.Type))
        {
            TriggerColliders.Add(collider);
        }
    }

    public void UnRegister(Collider collider)
    {
        if (!ColliderGridMaps.TryGetValue(collider,out var rect))
        {
            return;   
        }
        for (var x = rect.minX; x <= rect.maxX; x++)
        {
            for (var y = rect.minY; y <= rect.maxY; y++)
            {
                Grids[x,y].Remove(collider);
            }
        }
        ColliderGridMaps.Remove(collider);
        if (TriggerType.HasFlag(collider.Type))
        {
            TriggerColliders.Remove(collider);
        }
    }


    public void CheckCollider()//检测碰撞
    {
        ColliderDeduplicationSet.Clear();
        foreach (var triggerCollider in TriggerColliders)//遍历每个触发器
        {
            var triggerTypes = ColliderHandler.Instance.GetTriggerTypes(triggerCollider.Type);
            var rect = ColliderGridMaps[triggerCollider];
            for (var x = rect.minX; x <= rect.maxX; x++)//遍历触发器占据的每个格子
            {
                for (var y = rect.minY; y <= rect.maxY; y++)
                {
                    foreach (var collider in Grids[x, y].GetContains(triggerTypes))//遍历格子内每个类型正确的碰撞体
                    {
                        if (collider == triggerCollider)
                            continue;
                        var distance = collider.LocalPosition - triggerCollider.LocalPosition;
                        var distanceValue = distance.x * distance.x + distance.y * distance.y;
                        var radius = collider.CollideAreaRadius + triggerCollider.CollideAreaRadius;
                        var radiusSquare = radius * radius;
                        if (distanceValue <= radiusSquare)//圆形碰撞体判断
                        {
                            if (ColliderDeduplicationSet.Contains((triggerCollider.Id, collider.Id)))//碰撞去重
                            {
                                continue;
                            }
                            ColliderDeduplicationSet.Add((triggerCollider.Id, collider.Id));//正反都加,避免双向检测
                            ColliderDeduplicationSet.Add((collider.Id, triggerCollider.Id));
                            EventManager.Instance.SendEvent(new EventColliderTrigger(triggerCollider, collider));//发送碰撞事件
                        }
                    }
                }
            }
        }
    }
}