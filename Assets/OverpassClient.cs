
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

    private List<OverpassHouse> houses = new List<OverpassHouse>();

    void Start()
    {
        List<OverpassElement> elements = GetOverpassArea(position).elements;
        print(elements.Count);
        elements.ForEach(delegate (OverpassElement element)
        {
            houses.Add(new OverpassHouse(element.geometry, position, scale, element.tags));
        });

        houses.ForEach(delegate (OverpassHouse house)
        {
            house.UpdateMesh();
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
        Debug.Log(jsonResponse);
        return JsonUtility.FromJson<OverpassArea>(jsonResponse);
    }
}