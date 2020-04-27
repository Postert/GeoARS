using System;
using System.Collections.Generic;
using Unity.Mathematics;




/// <summary>
/// Contains the 3D points of the cuboid bounding box that best enclose the data set and the derived center point on the projected x-y plane
/// </summary>
public class BoundingBox
{
    public double3? ButtomLowerLeftCorner { get; private set; }
    public double3? TopUpperRightCorner { get; private set; }

    /// <summary>
    /// Resetting the BoudingBox by deleting the spanning points. Existing information of the BoundingBox cannot be restored.
    /// </summary>
    public void Clear()
    {
        this.ButtomLowerLeftCorner = null;
        this.TopUpperRightCorner = null;
    }


    public BoundingBox()
    {
        this.ButtomLowerLeftCorner = null;
        this.TopUpperRightCorner = null;
    }

    public BoundingBox(double3 ButtomLowerLeftCorner, double3 TopUpperRightCorner)
    {
        this.ButtomLowerLeftCorner = ButtomLowerLeftCorner;
        this.TopUpperRightCorner = TopUpperRightCorner;
    }


    /// <summary>
    /// Updates BoundingBox with a single point of type doube3
    /// </summary>
    /// <param name="point">Point to update the BoudingBoy with</param>
    private void UpdateWith(double3 point)
    {
        if (!this.ButtomLowerLeftCorner.HasValue || !this.TopUpperRightCorner.HasValue)
        {
            ButtomLowerLeftCorner = point;
            TopUpperRightCorner = point;
        }
        else
        {
            double buttomLowerLeftCorner_X = Math.Min(this.ButtomLowerLeftCorner.Value.x, point.x);
            double buttomLowerLeftCorner_Y = Math.Min(this.ButtomLowerLeftCorner.Value.y, point.y);
            double buttomLowerLeftCorner_Z = Math.Min(this.ButtomLowerLeftCorner.Value.z, point.z);
            double topUpperRightCorner_X = Math.Max(this.TopUpperRightCorner.Value.x, point.x);
            double topUpperRightCorner_Y = Math.Max(this.TopUpperRightCorner.Value.y, point.y);
            double topUpperRightCorner_Z = Math.Max(this.TopUpperRightCorner.Value.z, point.z);

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
    }

    /// <summary>
    /// Updates BoundingBox with a list of points of type doube3
    /// </summary>
    /// <param name="points">List of points to update the BoudingBox with</param>
    public void UpdateWith(List<double3> points)
    {
        foreach (double3 point in points)
        {
            this.UpdateWith(point);
        }
    }

    /// <summary>
    /// Updates BoudingBox with an array of points of the type double3
    /// </summary>
    /// <param name="points">Array with points to update the BoudingBox</param>
    public void UpdateWith(double3[] points)
    {
        foreach (double3 point in points)
        {
            this.UpdateWith(point);
        }
    }

    /// <summary>
    /// Updates BoudingBox with a list of surfaces which are spanned by polygons of points of the type double3
    /// </summary>
    /// <param name="surfaces">Surface with </param>
    public void UpdateWith(List<Surface> surfaces)
    {
        foreach (Surface surface in surfaces)
        {
            this.UpdateWith(surface);
        }
    }

    public void UpdateWith(Surface surface)
    {
        foreach (double3 point in surface.Polygon)
        {
            this.UpdateWith(point);
        }
    }


    public void UpdateWith(BoundingBox boundingBox)
    {
        if (boundingBox.ButtomLowerLeftCorner.HasValue && boundingBox.TopUpperRightCorner.HasValue)
        {
            this.UpdateWith(boundingBox.ButtomLowerLeftCorner.Value);
            this.UpdateWith(boundingBox.TopUpperRightCorner.Value);
        }
    }

    /// <summary>
    /// Returns the center of the bouding box
    /// </summary>
    /// <returns></returns>
    public double3? GetGroundSurfaceCenter()
    {
        if (this.IsInitialized())
        {
            return new double3(
                ButtomLowerLeftCorner.Value.x + 0.5 * (TopUpperRightCorner.Value.x - ButtomLowerLeftCorner.Value.x),
                ButtomLowerLeftCorner.Value.y + 0.5 * (TopUpperRightCorner.Value.y - ButtomLowerLeftCorner.Value.y),
                ButtomLowerLeftCorner.Value.z + 0.5 * (TopUpperRightCorner.Value.z - ButtomLowerLeftCorner.Value.z)
                );
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Returns true if both points that span the BoundungBox have been initialized and are not identical. Otherwise false is returned. 
    /// </summary>
    /// <returns></returns>
    public bool IsInitialized()
    {
        if (!this.ButtomLowerLeftCorner.HasValue || !this.TopUpperRightCorner.HasValue)
        {
            return false;
        }
        else
        {
            return !ButtomLowerLeftCorner.Equals(TopUpperRightCorner);
        }
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