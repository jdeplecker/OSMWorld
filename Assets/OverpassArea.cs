using System;
using System.Collections.Generic;

[Serializable]
public class OverpassArea
{
    public List<OverpassElement> elements;
}

[Serializable]
public class OverpassElement
{
    public Dictionary<string, string> tags;
    public List<OverpassGeometry> geometry;
}

[Serializable]
public class OverpassGeometry
{
    public float lat;
    public float lon;
}