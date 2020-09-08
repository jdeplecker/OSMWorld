using UnityEngine;
using System;
using System.Collections.Generic;

public class OverpassHouse
{

    private GameObject gameObject;
    private Vector3[] vertices;
    private int[] triangles;

    private Vector2 position;
    private float scale;

    public OverpassHouse(List<OverpassGeometry> geometry, Vector2 position, float scale)
    {
        this.position = position;
        this.scale = scale;
        createHouse(geometry);
    }



    void createHouse(List<OverpassGeometry> geometry)
    {
        int polygonLength = geometry.Count;
        vertices = new Vector3[polygonLength * 3];
        int[] extrudedTriangles = new int[polygonLength * 6];
        Vector2[] polygon = new Vector2[polygonLength];

        for (int i = 0; i < polygonLength; i++)
        {
            float lat = (geometry[i].lat - position.y) * scale;
            float lon = (geometry[i].lon - position.x) * scale;

            vertices[i] = new Vector3(lon, 10f, lat);
            vertices[i + polygonLength] = new Vector3(lon, 10f, lat); // duplicated for use of hard edges
            vertices[i + 2 * polygonLength] = new Vector3(lon, 0f, lat);
            if (i < polygonLength - 1)
            {
                polygon[i] = new Vector2(lon, lat);

                extrudedTriangles[6 * i] = i + polygonLength;
                extrudedTriangles[6 * i + 1] = i + 2 * polygonLength;
                extrudedTriangles[6 * i + 2] = i + 1 + polygonLength;
                extrudedTriangles[6 * i + 3] = i + 1 + polygonLength;
                extrudedTriangles[6 * i + 4] = i + 2 * polygonLength;
                extrudedTriangles[6 * i + 5] = i + 1 + 2 * polygonLength;
            }
        }

        int[] polygonTriangles = new Triangulator(polygon).Triangulate();
        Debug.Log("Polygon length: " + polygonLength + " polygonTriangles: " + polygonTriangles.Length + " extrudeTriangles: " + extrudedTriangles.Length);
        triangles = merge(polygonTriangles, extrudedTriangles);

        gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.transform.position = new Vector3(meanPosition().x, 0, meanPosition().y);
    }

    Vector2 meanPosition()
    {
        if (vertices.Length == 0)
        {
            return new Vector2(0, 0);
        }

        float totX = 0;
        float totY = 0;

        for (int i = 0; i < vertices.Length; i++)
        {
            totX += vertices[i].x;
            totY += vertices[i].y;
        }

        return new Vector2(totX / vertices.Length, totY / vertices.Length);
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
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}