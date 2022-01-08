using UnityEngine;
using System;
using System.Collections.Generic;

public class OverpassHouse
{

    private GameObject gameObject;
    private List<Vector3> vertices = new List<Vector3>();
    private int[] triangles;

    private Vector2 position;
    private float scale;

    public OverpassHouse(List<OverpassGeometry> geometry, Vector2 position, float scale)
    {
        this.position = position;
        this.scale = scale;
        // createHouse(geometry, new Polygon(geometry));
        createRoof(new Polygon(geometry, position, scale));
    }

    void createHouse(List<OverpassGeometry> geometry, Polygon polygon)
    {
        int polygonLength = geometry.Count;
        // vertices = new Vector3[polygonLength * 3];
        int[] extrudedTriangles = new int[polygonLength * 6];

        for (int i = 0; i < polygonLength; i++)
        {
            float lat = (geometry[i].lat - position.y) * scale;
            float lon = (geometry[i].lon - position.x) * scale;

            vertices[i] = new Vector3(lon, 10f, lat);
            vertices[i + polygonLength] = new Vector3(lon, 10f, lat); // duplicated for use of hard edges
            vertices[i + 2 * polygonLength] = new Vector3(lon, 0f, lat);
            if (i < polygonLength - 1)
            {
                extrudedTriangles[6 * i] = i + polygonLength;
                extrudedTriangles[6 * i + 1] = i + 2 * polygonLength;
                extrudedTriangles[6 * i + 2] = i + 1 + polygonLength;
                extrudedTriangles[6 * i + 3] = i + 1 + polygonLength;
                extrudedTriangles[6 * i + 4] = i + 2 * polygonLength;
                extrudedTriangles[6 * i + 5] = i + 1 + 2 * polygonLength;
            }
        }

        int[] polygonTriangles = new Triangulator(polygon.AsListOfPoints()).Triangulate();
        triangles = merge(polygonTriangles, extrudedTriangles);
    }

    void createRoof(Polygon polygon)
    {
        gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.transform.position = new Vector3(polygon.MeanPosition().x, 0, polygon.MeanPosition().y);

        polygon.AsListOfPoints().ForEach(point => vertices.Add(new Vector3(point.x, 10f, point.y)));
        triangles = new Triangulator(polygon.AsListOfPoints()).Triangulate();

        for (int triangleIndex = 0; triangleIndex < triangles.Length / 3; triangleIndex += 3)
        {
            Vector3 normal = Vector3.Cross(vertices[triangles[triangleIndex + 1]] - vertices[triangles[triangleIndex]], vertices[triangles[triangleIndex + 2]] - vertices[triangles[triangleIndex]]);
            normal.Normalize();
            if (normal != Vector3.up)
            {
                Debug.Log("Flipped because normal " + normal + " isn't " + Vector3.up);
                int temp = triangles[triangleIndex + 1];
                triangles[triangleIndex + 1] = triangles[triangleIndex + 2];
                triangles[triangleIndex + 2] = temp;
            }
        }
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