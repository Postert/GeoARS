public class BuildingAnnotation
{
    private const int meterAboveBuilding = 5;


    public int BuildingID { get; set; }
    public BoundingBox BuildingBoundingBox { get; set; }
    public double  BuildingHeight { get; set; }
    public int ComponentID { get; set; }
    public string AnnotationText { get; set; }
    public float LocalScale { get; set; }



    public BuildingAnnotation(int buildingID, BoundingBox buildingBoundingBox, double buildingHeight, int componentID, string annotationText, float localScale)
    {
        this.BuildingID = buildingID;
        this.BuildingBoundingBox = buildingBoundingBox;
        this.BuildingHeight = buildingHeight + meterAboveBuilding;
        this.ComponentID = componentID;
        this.AnnotationText = annotationText;
        this.LocalScale = localScale;
    }



}