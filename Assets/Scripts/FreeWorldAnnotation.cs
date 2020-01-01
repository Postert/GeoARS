using Unity.Mathematics;


public class FreeWorldAnnotation
{
    public double3 RealWorldCoordinate { get; set; }
    public int ComponentID { get; set; }
    public string AnnotationText { get; set; }
    public float LocalScale { get; set; }


    public FreeWorldAnnotation(double3 realWorldCoordinate, int componentID, string annotationText, float localScale)
    {
        this.RealWorldCoordinate = realWorldCoordinate;
        this.ComponentID = componentID;
        this.AnnotationText = annotationText;
        this.LocalScale = localScale;
    }



    ~FreeWorldAnnotation()
    {

    }

}
