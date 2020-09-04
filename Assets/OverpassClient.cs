using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.IO;

public class OverpassClient : MonoBehaviour
{

    private string overpassUrl = "https://overpass-api.de/api/interpreter?data=%5Bout%3Ajson%5D%5Btimeout%3A25%5D%3Bway%5B%22building%22%5D({0}%2C{1}%2C{2}%2C{3})%3Bout+geom%3B";
    public float scale = 1000;
    public float downloadArea = 0.005f;

    public Vector2 position;

    void Start()
    {
        List<OverpassElement> elements = GetOverpassArea(position).elements;
        print(elements.Count);
        elements.ForEach(delegate (OverpassElement element)
        {
            createCube(element);
        });
    }

    OverpassArea GetOverpassArea(Vector2 position)
    {
        float x1 = position.x - downloadArea;
        float y1 = position.y - downloadArea;
        float x2 = position.x + downloadArea;
        float y2 = position.y + downloadArea;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format(overpassUrl, y1, x1, y2, x2).Replace(",", "."));
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string jsonResponse = reader.ReadToEnd();
        return JsonUtility.FromJson<OverpassArea>(jsonResponse);
    }

    void createCube(OverpassElement element)
    {
        int polygonLength = element.geometry.Count - 1;
        Vector3[] vertices = new Vector3[polygonLength];
        // int[] triangles = new int[polygonLength * 6];
        Vector2[] polygon = new Vector2[vertices.Length];

        for (int i = 0; i < polygonLength; i++)
        {
            float lat = (element.geometry[i].lat - position.y) * scale;
            float lon = (element.geometry[i].lon - position.x) * scale;

            vertices[i] = new Vector3(lon, 10f, lat);
            // vertices[i + polygonLength] = new Vector3(lon, 0f, lat);
            polygon[i] = new Vector2(lon, lat);
            // if (i < polygonLength - 1)
            // {
            //     triangles[i] = i;
            //     triangles[i + 1] = i + polygonLength;
            //     triangles[i + 2] = i + 1;
            //     triangles[i + 3] = i + 1;
            //     triangles[i + 4] = i + polygonLength;
            //     triangles[i + 5] = i + polygonLength + 1;
            // }
        }


        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = new Triangulator(polygon).Triangulate();//merge(triangles, new Triangulator(polygon).Triangulate());
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, 0, 0);
        cube.GetComponent<MeshFilter>().mesh = mesh;
    }

    int[] merge(int[] front, int[] back)
    {
        int[] combined = new int[front.Length + back.Length];
        Array.Copy(front, combined, front.Length);
        Array.Copy(back, 0, combined, front.Length, back.Length);
        return combined;
    }
}
