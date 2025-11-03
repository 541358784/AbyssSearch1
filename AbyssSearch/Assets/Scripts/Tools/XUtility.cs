using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Framework;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public static partial class XUtility
{
    public static bool IsPartiallyVisible(this RectTransform child, RectTransform viewport)
    {
        // 获取视口在 Canvas 空间中的边界
        Rect viewportRect = GetWorldRect(viewport);
        
        // 获取子节点在 Canvas 空间中的边界
        Rect childRect = GetWorldRect(child);
        
        // 检查两个矩形是否有重叠
        return viewportRect.Overlaps(childRect);
    }
    public static string GetTimeString(this long time)
    {
        DateTime utcTime = DateTimeOffset.FromUnixTimeMilliseconds(time).UtcDateTime;
        string utcTimeString = utcTime.ToString("yyyy-MM-dd HH:mm:ss");
        return utcTimeString;
    }
    public static string ToLogString<T1, T2>(this Dictionary<T1, T2> dic)
    {
        var str = "{";
        foreach (var item in dic)
        {
            str += item.Key + "=" + item.Value;
            str += " , ";
        }
        str += "}";
        return str;
    }
    public static string ToLogString<T>(this List<T> list)
    {
        var str = "{";
        foreach (var item in list)
        {
            str += item.ToString();
            str += " , ";
        }
        str += "}";
        return str;
    }
    public static int RandomIndexByWeightF(this List<float> weightList)
    {
        var maxWeight = 0f;
        for (var i = 0; i < weightList.Count; i++)
        {
            maxWeight += weightList[i];
        }

        var randomWeight = Random.Range(0, maxWeight);
        var curWeight = 0f;
        for (var i = 0; i < weightList.Count; i++)
        {
            curWeight += weightList[i];
            if (curWeight >= randomWeight)
                return i;
        }
        return weightList.Count - 1;
    }
    public static float GetDistanceToScreenSide(this Camera mainCamera,Vector3 worldStartPoint,Vector2 direction)
    {
        Vector3 screenStartPoint = mainCamera.WorldToScreenPoint(worldStartPoint);

        // 设置一个大距离以确保射线能穿过整个屏幕
        float maxDistance = 10000f;
        Vector3 rayDirection = new Vector3(direction.x, direction.y, 0).normalized * maxDistance;

        // 计算射线终止点
        Vector3 endPoint = screenStartPoint + rayDirection;

        // 初始化最小距离为正无穷
        float minDistance = Mathf.Infinity;

        // 检查射线与屏幕边界的交点并计算距离
        if (rayDirection.x != 0)
        {
            // 与垂直边界的交点
            float t1 = -screenStartPoint.x / rayDirection.x;
            float t2 = (Screen.width - screenStartPoint.x) / rayDirection.x;

            if (t1 > 0) minDistance = Mathf.Min(minDistance, t1);
            if (t2 > 0) minDistance = Mathf.Min(minDistance, t2);
        }

        if (rayDirection.y != 0)
        {
            // 与水平边界的交点
            float t3 = -screenStartPoint.y / rayDirection.y;
            float t4 = (Screen.height - screenStartPoint.y) / rayDirection.y;

            if (t3 > 0) minDistance = Mathf.Min(minDistance, t3);
            if (t4 > 0) minDistance = Mathf.Min(minDistance, t4);
        }

        // 使用计算出的最短时间来计算到边缘的实际距离
        float distanceToEdge = minDistance * rayDirection.magnitude / maxDistance;
        Debug.LogError("Distance to screen edge: " + distanceToEdge);
        return distanceToEdge;
    }
    public static Bounds To2D(this Bounds bounds)
    {
        Vector3 newCenter = new Vector3(bounds.center.x, bounds.center.y, 0);
        Vector3 newSize = new Vector3(bounds.size.x, bounds.size.y, 0);
        Bounds bounds2D = new Bounds(newCenter, newSize);
        return bounds2D;
    }
    public static List<T> DeepCopy<T>(this List<T> list)
    {
        var clone = new List<T>();
        for (var i = 0; i < list.Count; i++)
        {
            clone.Add(list[i]);
        }
        return clone;
    }
    public static Dictionary<T1,T2> DeepCopy<T1,T2>(this Dictionary<T1,T2> dic)
    {
        var clone = new Dictionary<T1,T2>();
        foreach (var pair in dic)
        {
            clone.Add(pair.Key,pair.Value);
        }
        return clone;
    }
    //查找交点
    static Vector2[] FindIntersectionPoints(Vector2 a, Vector2 b, Vector2 p, float r)
    {
        Vector2 d = b - a;
        Vector2 f = a - p;

        float aCoeff = Vector2.Dot(d, d);
        float bCoeff = 2 * Vector2.Dot(f, d);
        float cCoeff = Vector2.Dot(f, f) - r * r;

        float discriminant = bCoeff * bCoeff - 4 * aCoeff * cCoeff;
        
        if (discriminant < 0)
        {
            // No intersection
            return new Vector2[0];
        }
        
        discriminant = Mathf.Sqrt(discriminant);

        float t1 = (-bCoeff - discriminant) / (2 * aCoeff);
        float t2 = (-bCoeff + discriminant) / (2 * aCoeff);

        Vector2[] results = new Vector2[2];
        results[0] = a + t1 * d;
        results[1] = a + t2 * d;

        return results;
    }
    //线段上找点
    public static Vector3 CalculatePointsOnLineSegment(this Vector3 referencePoint,Vector3 pointA,Vector3 pointB,float fixedDistance)
    {
        var samePointDistance = 0.001f;
        if (Vector3.Distance(referencePoint, pointA) < samePointDistance &&
            Vector3.Distance(pointA, pointB) < samePointDistance &&
            Vector3.Distance(pointB, referencePoint) < samePointDistance)
        {
            return referencePoint;
        }
        var points = FindIntersectionPoints(pointA, pointB, referencePoint, fixedDistance);
        foreach (var point in points)
        {
            if (IsPointOnLineSegment(pointA, pointB, point))
            {
                return point;
            }
        }
        throw new Exception("未找到点");
    }
    //点是否在线段上
    static bool IsPointOnLineSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        var minX = Math.Min(a.x, b.x);
        var maxX = Math.Max(a.x, b.x);
        var minY = Math.Min(a.y, b.y);
        var maxY = Math.Max(a.y, b.y);
        if (p.x >= minX && p.x <= maxX && p.y >= minY && p.y <= maxY)
        {
            return true;
        }
        return false;
    }
    
    //圆与碰撞体是否重叠
    public static bool IsCircleOverlapping(this CircleCollider2D circle, Collider2D collider)
    {
        if (collider is BoxCollider2D)
        {
            return circle.IsCircleOverlappingBox(collider as BoxCollider2D);
        }

        if (collider is CircleCollider2D)
        {
            return circle.IsCirclesOverlappingCircle(collider as CircleCollider2D);
        }

        if (collider is PolygonCollider2D)
        {
            return circle.IsCircleOverlappingPolygon(collider as PolygonCollider2D);
        }
        return false;
    }
    //圆与多边形是否重叠
    public static bool IsCircleOverlappingPolygon(this CircleCollider2D circle, PolygonCollider2D polygon)
    {
        if (circle == null || polygon == null)
            return false;

        // 获取圆心的世界坐标及其缩放的实际半径
        Vector2 circleCenter = (Vector2)circle.transform.position + circle.offset;
        float circleRadius = circle.radius * Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);

        // 检查圆心是否在多边形内部
        if (IsPointInsidePolygon(circleCenter, polygon))
        {
            return true;
        }

        // 检查每条边是否与圆重叠
        for (int i = 0; i < polygon.pathCount; i++)
        {
            Vector2[] path = polygon.GetPath(i);

            for (int j = 0; j < path.Length; j++)
            {
                Vector2 pointA = polygon.transform.TransformPoint(Vector2.Scale(path[j], polygon.transform.lossyScale));
                Vector2 pointB = polygon.transform.TransformPoint(Vector2.Scale(path[(j + 1) % path.Length], polygon.transform.lossyScale));

                // 检查圆是否与多边形的边重叠
                if (IsCircleIntersectingLine(circleCenter, circleRadius, pointA, pointB))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // 检查点是否在多边形内
    private static bool IsPointInsidePolygon(Vector2 point, PolygonCollider2D polygon)
    {
        int intersections = 0;
        Vector2[] path = polygon.GetPath(0);

        for (int i = 0; i < path.Length; i++)
        {
            Vector2 vertex1 = polygon.transform.TransformPoint(Vector2.Scale(path[i], polygon.transform.lossyScale));
            Vector2 vertex2 = polygon.transform.TransformPoint(Vector2.Scale(path[(i + 1) % path.Length], polygon.transform.lossyScale));

            // 判断水平线是否与多边形的边相交
            if (((vertex1.y > point.y) != (vertex2.y > point.y)) &&
                (point.x < (vertex2.x - vertex1.x) * (point.y - vertex1.y) / (vertex2.y - vertex1.y) + vertex1.x))
            {
                intersections++;
            }
        }

        // 奇数为内，偶数为外
        return (intersections % 2) != 0;
    }

    // 检查圆与线段是否相交
    private static bool IsCircleIntersectingLine(Vector2 circleCenter, float radius, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 d = lineEnd - lineStart;
        Vector2 f = lineStart - circleCenter;

        float a = Vector2.Dot(d, d);
        float b = 2 * Vector2.Dot(f, d);
        float c = Vector2.Dot(f, f) - radius * radius;

        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            return false; // 不相交
        }
        else
        {
            discriminant = Mathf.Sqrt(discriminant);

            float t1 = (-b - discriminant) / (2 * a);
            float t2 = (-b + discriminant) / (2 * a);

            // 检查线段上是否有交点
            if (t1 >= 0 && t1 <= 1)
            {
                return true;
            }

            if (t2 >= 0 && t2 <= 1)
            {
                return true;
            }

            return false;
        }
    }
    public static bool IsCircleOverlappingBox(this CircleCollider2D circle, BoxCollider2D box)
    {
        if (circle == null || box == null)
            return false;

        // 获取圆心位置（相对于对象的中心）及其缩放的实际半径
        Vector2 circleCenter = (Vector2)circle.transform.position + circle.offset;
        float circleRadius = circle.radius * Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);

        // 获取矩形的长宽和中心点，考虑 lossyScale
        Bounds boxBounds = box.bounds;
        Vector2 boxCenter = (Vector2)box.transform.position;
        Vector3 boxScale = box.transform.lossyScale;
        Vector2 boxHalfSize = new Vector2(box.size.x * 0.5f * boxScale.x, box.size.y * 0.5f * boxScale.y);

        // 确定圆心到矩形中心的最近点
        float closestX = Mathf.Clamp(circleCenter.x, boxCenter.x - boxHalfSize.x, boxCenter.x + boxHalfSize.x);
        float closestY = Mathf.Clamp(circleCenter.y, boxCenter.y - boxHalfSize.y, boxCenter.y + boxHalfSize.y);

        // 计算最近点到圆心的距离
        float distanceX = circleCenter.x - closestX;
        float distanceY = circleCenter.y - closestY;

        // 判断距离是否小于或等于半径
        return (distanceX * distanceX + distanceY * distanceY) <= (circleRadius * circleRadius);
    }
    public static bool IsCirclesOverlappingCircle(this CircleCollider2D circleA, CircleCollider2D circleB)
    {
        if (circleA == null || circleB == null)
            return false;

        // 获取圆心的世界坐标及其缩放的实际半径
        Vector2 centerA = (Vector2)circleA.transform.position + circleA.offset;
        float radiusA = circleA.radius * Mathf.Max(circleA.transform.lossyScale.x, circleA.transform.lossyScale.y);

        Vector2 centerB = (Vector2)circleB.transform.position + circleB.offset;
        float radiusB = circleB.radius * Mathf.Max(circleB.transform.lossyScale.x, circleB.transform.lossyScale.y);

        // 计算两个圆心之间的距离
        float distance = Vector2.Distance(centerA, centerB);

        // 计算两个圆半径之和
        float radiusSum = radiusA + radiusB;

        // 判断是否重叠
        return distance <= radiusSum;
    }
   
    public enum ShowTimeStrLevel
    {
        Second,
        Minute,
        Hour,
        Day,
    }
    public static string FormatLongToTimeStr(long l, ShowTimeStrLevel showTimeStrLevel)
    {
        int hour = 0;
        int minute = 0;
        int second = 0;
        int day = 0;

        l = l < 0 ? 0 : l;
        second = (int) (l / 1000);
        if (showTimeStrLevel == ShowTimeStrLevel.Second)
        { 
            return $"{second:D2}";
        }
        if (second >= 60)
        {
            minute = second / 60;
            second = second % 60;
        }
        if (showTimeStrLevel == ShowTimeStrLevel.Minute)
        { 
            return $"{minute:D2}:{second:D2}";
        }

        if (minute >= 60)
        {
            hour = minute / 60;
            minute = minute % 60;
        }
        if (showTimeStrLevel == ShowTimeStrLevel.Hour)
        { 
            return $"{hour:D2}:{minute:D2}:{second:D2}";
        }

        if (hour >= 24)
        {
            day = hour / 24;
            hour = hour % 24;
        }
        return $"{day}d {hour:D2}:{minute:D2}:{second:D2}";
    }

    public static bool Near(this Vector3 a, Vector3 b,float threshold)
    {
        var distance = a - b;
        if (Math.Abs(distance.x) < threshold &&
            Math.Abs(distance.y) < threshold &&
            Math.Abs(distance.z) < threshold)
            return true;
        return false;
    }

    public static bool Near(this Color a, Color b, float threshold)
    {
        var distance = a - b;
        if (Math.Abs(distance.r) < threshold &&
            Math.Abs(distance.g) < threshold &&
            Math.Abs(distance.b) < threshold)
            return true;
        return false;
    }
    public static void Shuffle<T>(List<T> myList)
    {
        for (int i = 0; i < myList.Count; i++)
        {
            int randomIndex = Random.Range(0, myList.Count);
            (myList[i], myList[randomIndex]) = (myList[randomIndex], myList[i]);
        }
    }
    public static float GetDistance(this Vector2 vec2)
    {
        var distance = Mathf.Sqrt(vec2.x * vec2.x + vec2.y * vec2.y);
        return distance;
    }
    public static Vector2 GetUnit(this Vector2 vec2)
    {
        var distance = Mathf.Sqrt(vec2.x * vec2.x + vec2.y * vec2.y);
        return vec2 / distance;
    }
    public static Vector3 GetUnit(this Vector3 vec3)
    {
        var distance = Mathf.Sqrt(vec3.x * vec3.x + vec3.y * vec3.y + vec3.z*vec3.z);
        return vec3 / distance;
    }

    public static Vector3 MultiVec3(this Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }
    public static T GetRandomEnumValue<T>()
    {
        // 获取枚举类型
        Type enumType = typeof(T);

        // 确保传入的类型是枚举
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("T must be an enumerated type");
        }

        // 获取枚举的所有值
        Array enumValues = Enum.GetValues(enumType);

        // 生成随机索引
        int randomIndex = Random.Range(0, enumValues.Length);

        // 获取随机的枚举值
        T randomEnumValue = (T)enumValues.GetValue(randomIndex);

        return randomEnumValue;
    }
    public const ulong Second = 1000;
    public const ulong Min = 60 * Second;
    public const ulong Hour = 60 * Min;
    public const ulong DayTime = 24 * Hour;
    public static RectTransform  SetSizeWidth(this RectTransform selfRectTrans, float sizeWidth)
    {
        var sizeDelta = selfRectTrans.sizeDelta;
        sizeDelta.x = sizeWidth;
        selfRectTrans.sizeDelta = sizeDelta;
        return selfRectTrans;
    }
    public static RectTransform  SetSizeHeight(this RectTransform selfRectTrans, float sizeHeight)
    {
        var sizeDelta = selfRectTrans.sizeDelta;
        sizeDelta.y = sizeHeight;
        selfRectTrans.sizeDelta = sizeDelta;
        return selfRectTrans;
    }
    //百分比检测
    public static bool RateCheck(int rate,int maxRate = 100)
    {
        return Random.Range(0, maxRate) < rate;
    }
    public static Task WaitSeconds(float timeToDelay)
    {
        if (timeToDelay <= 0)
            return Task.CompletedTask;
        // await Task.Delay((int)timeToDelay * 1000);
        TaskCompletionSource<bool> waitTask = new TaskCompletionSource<bool>();
        WaitSeconds(timeToDelay, () => waitTask.SetResult(true));
        return waitTask.Task;
    } 
    public static Task WaitFrames(int frameCount)
    {
        if (frameCount <= 0)
            return Task.CompletedTask;
        TaskCompletionSource<bool> waitTask = new TaskCompletionSource<bool>();
        WaitFrames(frameCount,()=>waitTask.SetResult(true));
        return waitTask.Task;
    }
    public static IEnumerator WaitSecondsIEnumerator(float seconds, Action callback)
    {
        // float elapsedTime = 0;
        // while (elapsedTime < seconds)
        // {
        //     elapsedTime += Time.deltaTime;
        //     yield return null;
        // }
        yield return new WaitForSeconds(seconds);
        callback();
    }
    public static IEnumerator WaitFramesIEnumerator(int delayFrame, Action action)
    {
        if (action != null)
        {
            for (int i = 0; i < delayFrame; i++)
                yield return new WaitForEndOfFrame();
            action();
        }
    }
    public static void WaitSeconds(float timeToDelay,Action callback)
    {
        if (timeToDelay <= 0)
        {
            callback();
            return;   
        }
        Coroutine coroutine = CoroutineManager.Instance.StartCoroutine(WaitSecondsIEnumerator(timeToDelay, callback));
    }
    public static void WaitFrames(int frameCount, Action callback)
    {
        if (frameCount <= 0)
        {
            callback();
            return;   
        }
        Coroutine coroutine = CoroutineManager.Instance.StartCoroutine(WaitFramesIEnumerator(frameCount, callback));
    }
    public static async void WrapErrors(this Task task)
    {
        await task;
    }
    public static async Task AddCallBack(this Task task,Action action)
    {
        await task;
        action?.Invoke();
    }
    public static async Task<bool> AddBoolCallBack(this Task<bool> task,Action<bool> action)
    {
        var result = await task;
        action?.Invoke(result);
        return result;
    }

    public static Task PlaySkeletonAnimationAsync(this SkeletonGraphic skeletonGraphic,string animationName,bool loop = false,bool ignoreRepeatPlaying = false)
    {
        if (skeletonGraphic.AnimationState is null)
            return Task.CompletedTask;
        if (ignoreRepeatPlaying)
        {
            TrackEntry trackEntryLast = skeletonGraphic.AnimationState.GetCurrent(0);
            if(trackEntryLast != null && trackEntryLast.Animation != null && trackEntryLast.Animation.Name == animationName)
                return Task.CompletedTask;
        }
        skeletonGraphic.AnimationState.SetAnimation(0, animationName, loop);
        skeletonGraphic.Update(0);
        TrackEntry trackEntry = skeletonGraphic.AnimationState.GetCurrent(0);
        return WaitSeconds(trackEntry.AnimationEnd);
    }
    public static void PlaySkeletonAnimation(this SkeletonGraphic skeletonGraphic,string animationName,bool loop = false,bool ignoreRepeatPlaying = false)
    {
        if (skeletonGraphic.AnimationState is null)
            return;
        if (ignoreRepeatPlaying)
        {
            TrackEntry trackEntry = skeletonGraphic.AnimationState.GetCurrent(0);
            if(trackEntry != null && trackEntry.Animation != null && trackEntry.Animation.Name == animationName)
                return;
        }
        skeletonGraphic.AnimationState.SetAnimation(0, animationName, loop);
        skeletonGraphic.Update(0);
    }
    public static bool HasState(this Animator anim, string stateName, int layer = 0) {
 
        int stateID = Animator.StringToHash(stateName);
        return anim.HasState(layer, stateID);
    }

    public static Task PlayAnimationAsync(this Animator animator, string stateName)
    {
        var waitTask = new TaskCompletionSource<bool>();
        animator.PlayAnimation(stateName, ()=>waitTask.SetResult(true));
        return waitTask.Task;
    }
    public static async void PlayAnimation(this Animator animator,string stateName,Action finishCallback = null)
    {
        if (animator == null)
        {
            finishCallback?.Invoke();
            return;
        }
        if (!animator.HasState(stateName))
        {
            finishCallback?.Invoke();
            return;
        }
        if (finishCallback == null)
        {
            if (animator.HasState(stateName))
            {
                animator.speed = 1;
                animator.Play(stateName, -1, 0);
            }

            return;
        }

        if (!animator.gameObject.activeInHierarchy)
        {
            finishCallback?.Invoke();
            return;
        }
        
        animator.speed = 1;
        animator.Play(stateName, -1, 0);
        var clipLength = animator.GetAnimTime(stateName);
        try
        {
            if (clipLength > 0)
                await WaitSeconds(clipLength);
            finishCallback.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    public static float GetAnimTime(this Animator animator, string playAnimName)
    {
        float length = 0f;
        AnimationClip[] clips = animator.runtimeAnimatorController?.animationClips;
        if (clips != null)
        {
            foreach (AnimationClip clip in clips)
            {
                if (clip.name.Equals(playAnimName))
                {
                    length = clip.length;
                    break;
                }
            }
        }

        return length;
    }
    
    //点在多边形内
    public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        int polygonLength = polygon.Count, i = 0;
        var inside = false;

        float pointX = point.x, pointY = point.y;

        float startX, startY, endX, endY;
        var endPoint = polygon[polygonLength - 1];
        endX = endPoint.x;
        endY = endPoint.y;
        while (i < polygonLength)
        {
            startX = endX;
            startY = endY;
            endPoint = polygon[i++];
            endX = endPoint.x;
            endY = endPoint.y;
            inside ^= (endY > pointY ^ startY > pointY) &&
                      ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
        }

        return inside;
    }
    
    
    
    
    public static Rect GetWorldRect(this RectTransform rectTransform)
    {
        var lossyScale = rectTransform.lossyScale;
        var localScale = rectTransform.localScale;
        localScale = Vector3.one;
        var position = rectTransform.position;
        var rect = rectTransform.rect;
        var minY = rect.yMin * lossyScale.y * localScale.y + position.y;
        var minX = rect.xMin * lossyScale.x * localScale.x + position.x;
        var width = rect.width * lossyScale.x * localScale.x;
        var height = rect.height * lossyScale.y * localScale.y;
        return new Rect(minX, minY, width, height);
    }
    
    public static Vector3 SetRectTransformInScreen(RectTransform rectTransform,
        float topDistance = 0,
        float bottomDistance = 0,
        float leftDistance = 0,
        float rightDistance = 0)
    {
        if (!rectTransform)
            return Vector3.zero;
        Rect screenEdgeRect = UIRoot.Instance.mUICamera.GetCameraWorldRect();
        screenEdgeRect.xMin += leftDistance;
        screenEdgeRect.yMin += bottomDistance;
        screenEdgeRect.width -= rightDistance;
        screenEdgeRect.height -= topDistance;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        var windowEdgeRect = rectTransform.GetWorldRect();
        var scale = Math.Min(screenEdgeRect.height / windowEdgeRect.height, screenEdgeRect.width / windowEdgeRect.width);
        scale = Math.Min(1, scale);
        if (scale < 1)
        {
            rectTransform.localScale *= scale;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            windowEdgeRect = rectTransform.GetWorldRect();
        }

        Vector3 moveDistance = Vector3.zero;
        if (windowEdgeRect.xMax > screenEdgeRect.xMax)
            moveDistance.x = screenEdgeRect.xMax - windowEdgeRect.xMax;
        if (windowEdgeRect.xMin < screenEdgeRect.xMin)
            moveDistance.x = screenEdgeRect.xMin - windowEdgeRect.xMin;
        if (windowEdgeRect.yMax > screenEdgeRect.yMax)
            moveDistance.y = screenEdgeRect.yMax - windowEdgeRect.yMax;
        if (windowEdgeRect.yMin < screenEdgeRect.yMin)
            moveDistance.y = screenEdgeRect.yMin - windowEdgeRect.yMin;
        rectTransform.position += moveDistance;
        return moveDistance;
    }
    
    public static Rect GetCameraWorldRect(this Camera camera)
    {
        var mainCamera = camera;
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 topLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));
        Vector3 bottomRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, mainCamera.nearClipPlane));
        Rect screenEdgeRect = new Rect(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
        return screenEdgeRect;
    }
    
    public static float GetScreenRatio()
    {
        float maxR = Mathf.Max(Screen.width, Screen.height);
        float minR = Mathf.Min(Screen.width, Screen.height);
        var ratio = maxR / minR;
        //DebugUtil.LogError($"IsPad {Screen.width} {Screen.height} {isiPad}");
        return ratio;
    }
}