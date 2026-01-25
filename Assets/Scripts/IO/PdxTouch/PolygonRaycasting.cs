using UnityEngine;

namespace MajdataPlay.IO.PdxTouch;

public static class PolygonRaycasting
{
    /// <summary>
    /// 检查点是否在多边形的顶点内
    /// </summary>
    /// <returns></returns>
    public static bool InPointInInternal(Vector2[] polygon, Vector2 localInputPoint)
    {
        bool isInsidePolygon = false;
        int num = polygon.Length;
        float x = localInputPoint.x;
        float y = localInputPoint.y;
        Vector2 prevVertex = polygon[num - 1];
        float prevX = prevVertex.x;
        float prevY = prevVertex.y;
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 currentVertex = polygon[i];
            float currentX = currentVertex.x;
            float currentY = currentVertex.y;


            // 判断点是否在边的左右交替
            if ((currentY > y ^ prevY > y) && (x < (prevX - currentX) * (y - currentY) / (prevY - currentY) + currentX))
            {
                isInsidePolygon = !isInsidePolygon;
            }
            prevX = currentX;
            prevY = currentY;
        }
        return isInsidePolygon;
    }
    /// <summary>
    /// 检查顶点是否在触摸点的范围
    /// </summary>
    /// <param name="polygon"></param>
    /// <param name="circleCenter"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static bool IsVertDistance(Vector2[] polygon, Vector2 circleCenter, float radius)
    {
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 currentVertex = polygon[i];
            if (Vector2.Distance(circleCenter, currentVertex) < radius)
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 检查圆是否与多边形的边相交
    /// </summary>
    /// <param name="polygon"></param>
    /// <param name="circleCenter"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static bool IsCircleIntersectingPolygonEdges(Vector2[] polygon, Vector2 circleCenter, float radius)
    {
        int vertexCount = polygon.Length;

        for (int i = 0; i < vertexCount; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % vertexCount];

            // 检查圆心到边的最短距离是否小于等于半径
            if (DistanceFromPointToSegment(circleCenter, a, b) <= radius)
            {
                return true;
            }
        }

        return false;
    }
    // 计算点到线段的最短距离
    private static float DistanceFromPointToSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 segment = segmentEnd - segmentStart;
        float segmentLengthSquared = segment.sqrMagnitude;

        if (segmentLengthSquared == 0f)
        {
            return Vector2.Distance(point, segmentStart); // 退化为一个点
        }

        float t = Mathf.Clamp(Vector2.Dot(point - segmentStart, segment) / segmentLengthSquared, 0f, 1f);
        Vector2 projection = segmentStart + t * segment;
        return Vector2.Distance(point, projection);
    }
}
