using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;




public enum LevelOfDetail
{
    LoD1,
    LoD2
}


public class Surface
{
    public string CityGMLID { get; private set; } = null;

    public List<double3> Polygon { get; private set; } = new List<double3>();

    public SurfaceType Type { get; set; }

    public Surface(string surfaceCityGMLID) =>
        (CityGMLID) = (surfaceCityGMLID);

    public Surface(string surfaceCityGMLID, SurfaceType surfaceType) : this(surfaceCityGMLID) =>
        (Type) = (surfaceType);

    public Surface(string surfaceCityGMLID, SurfaceType surfaceType, List<double3> polygon) : this(surfaceCityGMLID, surfaceType) =>
        (Polygon) = (polygon);

    public void AddSurfacePoint(double3 surfacePoint)
    {
        Polygon.Add(surfacePoint);
    }



    public Vector3 GetSurfaceNormal()
    {
        /// Calculate the surface normal of the planar polygon to determine its orientation in 3D space.

        // TODO: eigene Methode für Normalenbestimmung
        // SurfaceNormal auch konkave Polygone
        Vector3 firstTraverseNormal = Vector3.Cross((float3)(Polygon[1] - Polygon[0]), (float3)(Polygon[2] - Polygon[0]));
        firstTraverseNormal.Normalize();

        Vector3 inverseFirstTraverseNormal = -1 * firstTraverseNormal;

        int traverseNormalCounter = 0, inverseTraverseNormalCounter = 0;

        for (int i = 0; i < Polygon.Count - 2; i++)
        {
            Vector3 currentTraverseNormal = Vector3.Cross((float3)(Polygon[i + 1] - Polygon[i]), (float3)(Polygon[i + 2] - Polygon[i]));
            currentTraverseNormal.Normalize();

            if (currentTraverseNormal.Equals(firstTraverseNormal))
            {
                traverseNormalCounter++;
            }
            else if (currentTraverseNormal.Equals(inverseFirstTraverseNormal))
            {
                inverseTraverseNormalCounter++;
            }
            else
            {
                //throw new ArgumentException
                Debug.LogWarning(
                    "Invalid polygon: Points do not lie in one plane\nSurface:\t\t\t" + firstTraverseNormal + "\ninverseFirstTraverseNormal:\t" + inverseFirstTraverseNormal + "\ncurrentTraverseNormal:\t\t" + currentTraverseNormal + "\ntraverseNormalCounter:\t\t" + traverseNormalCounter + "\ninverseTraverseNormalCounter:\t" + inverseTraverseNormalCounter + "\n\n" + this.ToString() + "\nThis may be due to the rounding of coordinate values. A correct surface normal calculation cannot be guaranteed, but an incorrect determination of the normal direction is unlikely.");
            }
        }

        Vector3 surfaceNormal = (traverseNormalCounter > inverseTraverseNormalCounter) ? firstTraverseNormal : -1 * firstTraverseNormal;

        return surfaceNormal;
    }


    public override string ToString()
    {
        string surfaceToString =
            "CityGML Surface ID: " + CityGMLID + "\n" +
            "SurfaceType: " + Type + "\n" +
            "Polygonpunkte:\n";

        foreach (double3 point in Polygon)
        {
            surfaceToString += "   " + point.ToString() + "\n";
        }

        surfaceToString += "\n";

        return surfaceToString;
    }
}

public enum SurfaceType
{
    GroundSurface, WallSurface, RoofSurface, UNDEFINED
}


public enum SurfacePosition
{
    ExteriorSurface, InteriorSurface
}


public class Building
{
    public string CityGMLID { get; private set; }

    // private List<double3> GroundSurfacePoints = new List<double3>();

    public Dictionary<string, Surface> ExteriorSurfaces { get; private set; } = new Dictionary<string, Surface>();


    public float MeasuredHeight { get; set; }

    public BoundingBox BoundingBox = new BoundingBox();



    public Building(string cityGMLID)
    {
        CityGMLID = cityGMLID;
    }

    public Building(string cityGMLID, float measuredHeight) : this(cityGMLID)
    {
        MeasuredHeight = measuredHeight;
    }


    #region Surfaces

    public void SetSurfaces(Dictionary<string, Surface> exteriorSurfaces)
    {
        ExteriorSurfaces.Clear();
        BoundingBox.Clear();
        AddSurface(exteriorSurfaces);
    }

    public void AddSurface(Dictionary<string, Surface> exteriorSurfaces)
    {
        if (exteriorSurfaces.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<string, Surface> uniqueKeyAssociatedSurface in exteriorSurfaces)
        {
            AddSurface(uniqueKeyAssociatedSurface.Key, uniqueKeyAssociatedSurface.Value);
        }
    }

    public void AddSurface(string uniqueSurfaceID, Surface surface)
    {
        ExteriorSurfaces.Add(uniqueSurfaceID, surface);
        BoundingBox.UpdateWith(surface);
    }

    public void AddSurfacePoint(string uniqueSurfaceID, SurfaceType surfaceType, List<double3> surfacePoints)
    {
        if (!ExteriorSurfaces.TryGetValue(uniqueSurfaceID, out Surface surface))
        {
            ExteriorSurfaces.Add(uniqueSurfaceID, new Surface(uniqueSurfaceID, surfaceType, surfacePoints));
        }
        else
        {
            surface.Polygon.AddRange(surfacePoints);
        }
    }

    public void AddSurfacePoint(string uniqueSurfaceID, SurfaceType surfaceType, double3 surfacePoint)
    {
        List<double3> surfacePointList = new List<double3>();
        surfacePointList.Add(surfacePoint);
        AddSurfacePoint(uniqueSurfaceID, surfaceType, surfacePointList);
    }


    public Dictionary<string, Surface> GetSurfaces(SurfaceType surfaceType)
    {
        Dictionary<string, Surface> surfacesOfSelectedType = new Dictionary<string, Surface>();

        foreach (KeyValuePair<string, Surface> surfaceDictionaryEntry in ExteriorSurfaces)
        {
            if (surfaceDictionaryEntry.Value.Type == surfaceType)
            {
                surfacesOfSelectedType.Add(surfaceDictionaryEntry.Key, surfaceDictionaryEntry.Value);
            }
        }

        return surfacesOfSelectedType;
    }

    #endregion


    public override string ToString()
    {
        string buildingToString = null;
        buildingToString += "\nCityGML building ID: " + CityGMLID
            + "\nMeasuredHeight: " + MeasuredHeight
            + "\nBuilding-BoudingBox: " + BoundingBox.ToString();

        buildingToString += "\n\nThe Building consisting of the following " + ExteriorSurfaces.Count + " Surfaces:\n";


        int counter = 1;
        foreach (KeyValuePair<string, Surface> uniqueKeyAssociatedSurface in ExteriorSurfaces)
        {
            buildingToString += "\n " + counter++ + ". Surface:\n" + "uniqueInternalSurfaceID: " + uniqueKeyAssociatedSurface.Key + "\n" + uniqueKeyAssociatedSurface.Value.ToString();
        }

        buildingToString += "\n\n";

        return buildingToString;
    }
}