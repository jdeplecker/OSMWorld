using System.Collections.Generic;
using UnityEngine;

public class Polygon
{
    List<Vector2> points = new List<Vector2>();

    public Polygon(List<OverpassGeometry> overpassPoints, Vector2 position, float scale)
    {
        overpassPoints.ForEach(pt => points.Add(asVector2(pt, position, scale)));
        // if (points[0] == points[points.Count - 1])
        // {
        //     points.RemoveAt(points.Count - 1);
        // }
    }

    Vector2 asVector2(OverpassGeometry geometry, Vector2 position, float scale)
    {
        float y = (geometry.lat - position.y) * scale;
        float x = (geometry.lon - position.x) * scale;
        return new Vector2(x, y);
    }

    public List<Vector2> AsListOfPoints()
    {
        return new List<Vector2>(points);
    }

    public Vector2[] AsArrayOfPoints()
    {
        return new List<Vector2>(points).ToArray();
    }

    public Vector2 MeanPosition()
    {
        Vector2 meanPos = new Vector2();
        points.ForEach(pt => meanPos += pt);
        return meanPos / points.Count;
    }

    public int PointCount()
    {
        return points.Count;
    }
}