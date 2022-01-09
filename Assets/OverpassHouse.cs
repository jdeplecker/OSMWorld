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
        try {
            var polygon = new Polygon(geometry, position, scale);
            createGameObject(polygon);
            createHouse(polygon);
            createRoof(polygon);
        } catch (Exception e) {
            Debug.LogError(e);
        }
    }
    void createGameObject(Polygon polygon)
    {
        var polygonMeanPosition = polygon.MeanPosition();
        gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.transform.position = new Vector3(polygonMeanPosition.x, 0, polygonMeanPosition.y);
    }

    void createHouse(Polygon polygon)
    {
        var polygonMeanPosition = polygon.MeanPosition();
        var polygonList = polygon.AsListOfPoints();
        polygonList.Reverse();
        int polygonLength = polygonList.Count;
        vertices = new Vector3[polygonLength * 2];
        triangles = new int[polygonLength * 6];

        for (int i = 0; i < polygonLength; i++)
        {
            vertices[i*2] = new Vector3(polygonList[i].x - polygonMeanPosition.x, 0f, polygonList[i].y - polygonMeanPosition.y);
            vertices[i*2 + 1] = new Vector3(polygonList[i].x - polygonMeanPosition.x, 10f, polygonList[i].y - polygonMeanPosition.y);

            var currentVertex = new Vector2(polygonList[i].x, polygonList[i].y);
            var nextVertex = new Vector2(polygonList[(i + 1) % polygonLength].x, polygonList[(i + 1) % polygonLength].y);
            if (Utils2D.LeftSide(currentVertex, nextVertex, polygonMeanPosition)) {
                triangles[i*6] = i*2;
                triangles[i*6 + 1] = i*2 + 1;
                triangles[i*6 + 2] = ((i + 1) % polygonLength)*2;
                triangles[i*6 + 3] = ((i + 1) % polygonLength)*2;
                triangles[i*6 + 4] = i*2 + 1;
                triangles[i*6 + 5] = ((i + 1) % polygonLength)*2 + 1;
            } else {
                triangles[i*6] = i*2;
                triangles[i*6 + 1] = ((i + 1) % polygonLength)*2;
                triangles[i*6 + 2] = i*2 + 1;
                triangles[i*6 + 3] = ((i + 1) % polygonLength)*2;
                triangles[i*6 + 4] = ((i + 1) % polygonLength)*2 + 1;
                triangles[i*6 + 5] = i*2 + 1;
            }
        }
    }

    void createRoof(Polygon polygon) {
        var polygonMeanPosition = polygon.MeanPosition();
        Polygon2D polygon2d = new Polygon2D(polygon.AsArrayOfPoints());

        // construct Triangulation2D with Polygon2D and threshold angle (18f ~ 27f recommended)
        Triangulation2D triangulation = new Triangulation2D(polygon2d, 22.5f);

        // build a mesh from triangles in a Triangulation2D instance
        Mesh mesh = triangulation.Build((Vertex2D v) => { return new Vector3(v.Coordinate.x - polygonMeanPosition.x, 10f, v.Coordinate.y - polygonMeanPosition.y);});
        var frontVerticesCount = vertices.Length;
        vertices = mergeV3(vertices, mesh.vertices);
        triangles = mergeTriangles(triangles, mesh.triangles, frontVerticesCount);
    }

    int[] mergeTriangles(int[] front, int[] back, int frontVerticesCount)
    {
        int[] combined = new int[front.Length + back.Length];
        Array.Copy(front, combined, front.Length);
        Array.Copy(back, 0, combined, front.Length, back.Length);
        for (int i = front.Length; i < combined.Length; i++) {
            combined[i] = combined[i] + frontVerticesCount;
        }
        return combined;
    }

    Vector3[] mergeV3(Vector3[] front, Vector3[] back)
    {
        Vector3[] combined = new Vector3[front.Length + back.Length];
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