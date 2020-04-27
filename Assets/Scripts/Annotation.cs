using System.Collections; 
using System.Collections.Generic;
using Unity.Mathematics;


public abstract class Annotation
{
    public AnnotationProperties AnnotationProperties { get; set; }

    public Annotation(AnnotationProperties annotationProperties) =>
        (AnnotationProperties) = (annotationProperties);

    public Annotation(bool scaleWithCameraDistance, bool scaleBySelection, float3 pointingDirection) => 
        (AnnotationProperties) = (new AnnotationProperties(scaleWithCameraDistance, scaleBySelection, pointingDirection));

    public override string ToString()
    {
        return AnnotationProperties.ToString();
    }
}

public class AnnotationProperties
{
    public bool ScaleWithCameraDistance { get; set; }
    public bool ScaleBySelection { get; set; }
    public float3 PointingDirection { get; set; }

    public AnnotationProperties(bool scaleWithCameraDistance, bool scaleBySelection, float3 pointingDirection) =>
        (ScaleWithCameraDistance, ScaleBySelection, PointingDirection) =
        (scaleWithCameraDistance, scaleBySelection, pointingDirection);

    public override string ToString()
    {
        return "\n--AnnotationProperties----------------------------------"
            + "\nScaleWithCameraDistance: " + ScaleWithCameraDistance
            + "\nScaleBySelection: " + ScaleBySelection
            + "\nPointingDirection " + PointingDirection;
    }
}


/*
public class SimpleTextAnnotation
{
    public AnnotationTextComponent TextAnnotationComponent { get; set; }

    public SimpleTextAnnotation(, string annotationText, float localScale) :
        this(scaleWithCameraDistance, scaleBySelection, pointingDirection, new AnnotationTextComponent(annotationText, localScale)) {}

    public SimpleTextAnnotation(AnnotationTextComponent textAnnotationComponent)
}
*/


public class AnnotationComponent { }

public class TextAnnotationComponent : AnnotationComponent
{
    public string Text { get; set; }
    public float TextSize { get; set; }


    public TextAnnotationComponent(string text, float textSize) => 
        (Text, TextSize) = (text, textSize);

    public override string ToString()
    {
        return "\n--AnnotationTextComponent----------------------------------"
            + "\nText: " + Text
            + "\nLocalScale: " + TextSize;
    }
}


public class BuildingAnnotation : Annotation
{
    public const int meterAboveBuilding = 1;

    /// <summary>
    /// Core component for anchoraging the BuildingAnnotation
    /// </summary>
    public Building AssociatedBuilding { get; set; }

    public AnnotationComponent AnnotationComponent { get; set; }

    public BuildingAnnotation(Building associatedBuilding, AnnotationComponent annotationComponent, AnnotationProperties annotationProperties) :
        this(associatedBuilding, annotationComponent, annotationProperties.ScaleWithCameraDistance, annotationProperties.ScaleBySelection, annotationProperties.PointingDirection)
    { }

    public BuildingAnnotation(Building associatedBuilding, AnnotationComponent annotationComponent, bool scaleWithCameraDistance, bool scaleBySelection, float3 pointingDirection)  :
        base(scaleWithCameraDistance, scaleBySelection, pointingDirection) =>
        (AssociatedBuilding, AnnotationComponent) = (associatedBuilding, annotationComponent);

    public override string ToString()
    {
        return "BuildingAnnotation associated to building " + AssociatedBuilding.CityGMLID
            + base.ToString()
            + AnnotationComponent.ToString()
            + "\n--associated Building----------------------------------"
            + "\n\n";
    }
}


public class SurfaceAnnotation : Annotation
{
    public const float SurfaceOffset = 0.01f;

    /// <summary>
    /// Core component for anchoraging the SurfaceAnnotation
    /// </summary>
    public Surface AssociatedSurface { get; set; }


// TODO: Ankerpunkte durch Referenzierung der Double Werte aus 
    /// <summary>
    /// Indexes of two adjacent points of the AssociatedSurface's polygon for positioning the SurfaceAnnotation in between
    /// </summary>
    public int AnnotationAnchorPointIndex { get; set; }

    /// <summary>
    /// Double for relative placement between two adjacent points of the surface polygon specified by the AnnotationAnchorPointIndexes
    /// </summary>
    public double RelativePositionBetweenBasePoints { get; set; }

    /// <summary>
    /// Distance of the anchoring of the surface annotation perpendicular to the base line spanned by two points of the AssociatedSurface's polygon and perpendicular to the Surface's normal vector. 
    /// </summary>
    public double HeightAboveBaseLine { get; set; }

    public AnnotationComponent AnnotationComponent { get; set; }

    public SurfaceAnnotation(Surface associatedSurface, int annotationAnchorPointIndex, double relativePositionBetweenBasePoints, double heightAboveBaseLine, AnnotationComponent annotationComponent, AnnotationProperties annotationProperties) :
        this(associatedSurface, annotationAnchorPointIndex, relativePositionBetweenBasePoints, heightAboveBaseLine, annotationComponent, annotationProperties.ScaleWithCameraDistance, annotationProperties.ScaleBySelection, annotationProperties.PointingDirection)
    { }

    public SurfaceAnnotation(Surface associatedSurface, int annotationAnchorPointIndex, double relativePositionBetweenBasePoints, double heightAboveBaseLine, AnnotationComponent annotationComponent, bool scaleWithCameraDistance, bool scaleBySelection, float3 pointingDirection) :
        base(scaleWithCameraDistance, scaleBySelection, pointingDirection) =>
        (AssociatedSurface, AnnotationAnchorPointIndex, RelativePositionBetweenBasePoints, HeightAboveBaseLine, AnnotationComponent) = (associatedSurface, annotationAnchorPointIndex, relativePositionBetweenBasePoints, heightAboveBaseLine, annotationComponent);

    public override string ToString()
    {
        return "SurfaceAnnotation associated to surface " + AssociatedSurface.CityGMLID
            + base.ToString()
            + AnnotationComponent.ToString()
            + "\nwith AnnotationAnchorPointIndex " + AnnotationAnchorPointIndex
            + "\nwith RelativePositionBetweenBasePoints " + RelativePositionBetweenBasePoints
            + "\nwith HeightAboveBaseLine " + HeightAboveBaseLine
            + "\n\n";
    }
}



public class WorldCoordinateAnnotation : Annotation
{
    /// <summary>
    /// Core component for anchoraging the SurfaceAnnotation
    /// </summary>
    public double3 AnnotationUMLCoordinates { get; set; }

    public AnnotationComponent AnnotationComponent { get; set; }

    public WorldCoordinateAnnotation(double3 realWorldCoordinates, AnnotationComponent annotationComponent, AnnotationProperties annotationProperties) :
        this(realWorldCoordinates, annotationComponent, annotationProperties.ScaleWithCameraDistance, annotationProperties.ScaleBySelection, annotationProperties.PointingDirection)
    { }

    public WorldCoordinateAnnotation(double3 realWorldCoordinates, AnnotationComponent annotationTextComponent, bool scaleWithCameraDistance, bool scaleBySelection, float3 pointingDirection) :
        base(scaleWithCameraDistance, scaleBySelection, pointingDirection) =>
        (AnnotationUMLCoordinates, AnnotationComponent) = (realWorldCoordinates, annotationTextComponent);

    public override string ToString()
    {
        return "WorldCoordinateAnnotation associated to coordinate " + AnnotationUMLCoordinates.ToString()
            + base.ToString()
            + AnnotationComponent.ToString()
            + "\n\n";
    }
}