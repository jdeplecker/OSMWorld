using UnityEngine;
using System;
using System.Collections.Generic;
using mattatz.Triangulation2DSystem;

public class OverpassHouse
{

    private GameObject gameObject;
    private Vector3[] vertices;
    private int[] triangles;

    private Vector2 position;
    private float scale;

    private Dictionary<string, string> tags;

    public OverpassHouse(List<OverpassGeometry> geometry, Vector2 position, float scale, Dictionary<string, string> tags)
    {
        this.position = position;
        this.scale = scale;
        this.tags = tags;
        // createHouse(geometry, new Polygon(geometry));
        try {
            createRoof(new Polygon(geometry, position, scale));
        } catch (Exception e) {
            Debug.LogError(e);
        }
    }

    // void createHouse(List<OverpassGeometry> geometry, Polygon polygon)
    // {
    //     int polygonLength = geometry.Count;
    //     // vertices = new Vector3[polygonLength * 3];
    //     int[] extrudedTriangles = new int[polygonLength * 6];

    //     for (int i = 0; i < polygonLength; i++)
    //     {
    //         float lat = (geometry[i].lat - position.y) * scale;
    //         float lon = (geometry[i].lon - position.x) * scale;

    //         vertices[i] = new Vector3(lon, 10f, lat);
    //         vertices[i + polygonLength] = new Vector3(lon, 10f, lat); // duplicated for use of hard edges
    //         vertices[i + 2 * polygonLength] = new Vector3(lon, 0f, lat);
    //         if (i < polygonLength - 1)
    //         {
    //             extrudedTriangles[6 * i] = i + polygonLength;
    //             extrudedTriangles[6 * i + 1] = i + 2 * polygonLength;
    //             extrudedTriangles[6 * i + 2] = i + 1 + polygonLength;
    //             extrudedTriangles[6 * i + 3] = i + 1 + polygonLength;
    //             extrudedTriangles[6 * i + 4] = i + 2 * polygonLength;
    //             extrudedTriangles[6 * i + 5] = i + 1 + 2 * polygonLength;
    //         }
    //     }

    //     int[] polygonTriangles = new Triangulator(polygon.AsListOfPoints()).Triangulate();
    //     triangles = merge(polygonTriangles, extrudedTriangles);
    // }

    void createRoof(Polygon polygon) {
        var polygonMeanPosition = polygon.MeanPosition();
        gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.transform.position = new Vector3(polygonMeanPosition.x * scale, 0, polygonMeanPosition.y * scale);
        var pointsList = polygon.AsListOfPoints();
        pointsList.Reverse();
        var polygonArray = pointsList.ToArray();
        Polygon2D polygon2d = Polygon2D.Contour(polygonArray);

        // construct Triangulation2D with Polygon2D and threshold angle (18f ~ 27f recommended)
        Triangulation2D triangulation = new Triangulation2D(polygon2d, 22.5f);

        // build a mesh from triangles in a Triangulation2D instance
        Mesh mesh = triangulation.Build((Vertex2D v) => { return new Vector3((v.Coordinate.x - polygonMeanPosition.x) * scale, 0f, (v.Coordinate.y - polygonMeanPosition.y)  * scale);});
        vertices = mesh.vertices;
        triangles = mesh.triangles;
    }

    int[] merge(int[] front, int[] back)
    {
        int[] combined = new int[front.Length + back.Length];
        Array.Copy(front, combined, front.Length);
        Array.Copy(back, 0, combined, front.Length, back.Length);
        return combined;
    }

    public void UpdateMesh()
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}