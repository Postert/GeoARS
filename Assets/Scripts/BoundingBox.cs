using System;
using System.Collections.Generic;
using Unity.Mathematics;

/// <summary>
/// Contains the 3D points of the cuboid bounding box that best enclose the data set and the derived center point on the projected x-y plane
/// </summary>
public class BoundingBox
{
    private double3 Origin = new double3(0,0,0);

    public double3 ButtomLowerLeftCorner { get; private set; }
    public double3 TopUpperRightCorner { get; private set; }

    private bool isInitialized = false;


    public BoundingBox()
    {
        this.ButtomLowerLeftCorner = Origin;
        this.TopUpperRightCorner = Origin;
    }

    public BoundingBox(double3 ButtomLowerLeftCorner, double3 TopUpperRightCorner)
    {
        this.ButtomLowerLeftCorner = ButtomLowerLeftCorner;
        this.TopUpperRightCorner = TopUpperRightCorner;
    }


    private void UpdateWith(double3 point)
    {
        if (!isInitialized)
        {
            ButtomLowerLeftCorner = point;
            TopUpperRightCorner = point;
        }
        else
        {
            double buttomLowerLeftCorner_X = Math.Min(this.ButtomLowerLeftCorner.x, point.x);
            double buttomLowerLeftCorner_Y = Math.Min(this.ButtomLowerLeftCorner.y, point.y);
            double buttomLowerLeftCorner_Z = Math.Min(this.ButtomLowerLeftCorner.z, point.z);
            double topUpperRightCorner_X = Math.Max(this.TopUpperRightCorner.x, point.x);
            double topUpperRightCorner_Y = Math.Max(this.TopUpperRightCorner.y, point.y);
            double topUpperRightCorner_Z = Math.Max(this.TopUpperRightCorner.z, point.z);

            ButtomLowerLeftCorner = new double3(
                buttomLowerLeftCorner_X,
                buttomLowerLeftCorner_Y,
                buttomLowerLeftCorner_Z
                );
            TopUpperRightCorner = new double3(
                topUpperRightCorner_X, 
                topUpperRightCorner_Y, 
                topUpperRightCorner_Z
                );
        }

        isInitialized = true;
    }


    public void UpdateWith(double3 point, bool resetBoudingBox)
    {
        isInitialized = !resetBoudingBox;
        if (resetBoudingBox == true)
            isInitialized = false;
        this.UpdateWith(point);
    }


    public void UpdateWith(List<double3> pointList, bool resetBoudingBox)
    {
        isInitialized = !resetBoudingBox;
        foreach (double3 point in pointList)
        {
            this.UpdateWith(point);
        }
    }

    public void UpdateWith(double3[] pointArray, bool resetBoudingBox)
    {
        isInitialized = !resetBoudingBox;
        foreach (double3 point in pointArray)
        {
            this.UpdateWith(point);
        }
    }

    public void UpdateWith(BoundingBox boundingBox)
    {
        isInitialized = false;
        this.UpdateWith(boundingBox.ButtomLowerLeftCorner);
        this.UpdateWith(boundingBox.TopUpperRightCorner);
    }


    public double2 GetGroundSurfaceCenter()
    {
        return new double2(
            ButtomLowerLeftCorner.x + 0.5 * (TopUpperRightCorner.x - ButtomLowerLeftCorner.x),
            ButtomLowerLeftCorner.y + 0.5 * (TopUpperRightCorner.y - ButtomLowerLeftCorner.y)
            );
    }


    public bool HasContent()
    {
        return !ButtomLowerLeftCorner.Equals(TopUpperRightCorner);
    }


    public override string ToString()
    {
        string describtion = "Boundingbox with cordinates"
            + "\nButtomLowerLeftCorner:\n"
            + ButtomLowerLeftCorner
            + "\nTopUpperRightCorner:\n"
            + TopUpperRightCorner;

        return describtion;
    }
}