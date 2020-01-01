using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;

public class Building
{
    private static int nextId;

    public int BuildingID { get; private set; }

    public string CityGMLID { get; set; }

    private List<double3> GroundSurfacePoints = new List<double3>();


    public float MeasuredHeight { get; set; }

    public BoundingBox BoundingBox = new BoundingBox();


    public Building(int buildingID = 0)
    {
        if (buildingID == 0)
        {
            BuildingID = Interlocked.Increment(ref nextId);
        }
        else
        {
            BuildingID = buildingID;
        }
    }


    public Building(string cityGMLID, int buildingID = 0) : this(buildingID)
    {
        this.CityGMLID = cityGMLID;
    }

    public Building(string cityGMLID, float measuredHeight, int buildingID = 0) : this(cityGMLID, buildingID)
    {
        this.MeasuredHeight = measuredHeight;
    }


    public List<double3> GetGroundSurfacePoints()
    {
        return GroundSurfacePoints;
    }

    public void SetGroundSurfacePoints(List<double3> pointList)
    {
        this.GroundSurfacePoints = pointList;
        this.BoundingBox.UpdateWith(pointList, true);
    }

    public void AddGroundSurfacePoint(double3 point)
    {
        this.GroundSurfacePoints.Add(point);
        this.BoundingBox.UpdateWith(point, false);
    }

    public override string ToString()
    {
        string buildingToString = null;
        buildingToString += "BuildingID: " + BuildingID
            + "\nCityGML building ID: " + CityGMLID
            + "\nMeasuredHeight: " + MeasuredHeight
            + "\nBuilding-BoudingBox: " + this.BoundingBox.ToString()
            + "\nGroundsurfaces consisting of the following " + GroundSurfacePoints.Count + " points:\n";

        foreach (double3 point in GroundSurfacePoints)
        {
            buildingToString += point.ToString() + "\n";
        }

        buildingToString += "\n\n";

        return buildingToString;
    }



    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {

    }

}