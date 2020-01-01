using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class SurfaceAnnotation
{
    public float SurfaceOffset { get; }  = 0.01f;


    public int BuildingID { get; set; }
    public double3[] GroundSurfacePoints { get; set; } = new double3[2];
    public int ComponentID { get; set; }
    public double RelativeGroundSurfacePosition { get; set; }
    public double Height { get; set; }
    public string AnnotationText { get; set; }
    public float LocalScale { get; set; }


    public SurfaceAnnotation(int buildingID, double3[] groundSurfacePoints, double relativeGroundSurfacePosition, double height, int componentID, string annotationText, float localScale)
    {
        this.BuildingID = buildingID;
        this.GroundSurfacePoints = groundSurfacePoints;
        this.ComponentID = componentID;
        this.RelativeGroundSurfacePosition = relativeGroundSurfacePosition;
        this.Height = height;
        this.AnnotationText = annotationText;
        this.LocalScale = localScale;
    }
}
