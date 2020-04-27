using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class AnnotationPrefabScript : MonoBehaviour
{
    public bool IsLookingToARCamera = false;
    public bool IsScalingWithARCameraDistance = false;


    private GameObject AnnotationGameObject;
    private Canvas AnnotationCanvas;
    private Text TextComponent;
    private GameObject TextField;
    private CanvasScaler AnnotationCanvasScaler;
    private GraphicRaycaster AnnotationGraphicRaycaster;

    private Vector3 AnnotationPosition;
    private const int FontSize = 25;
    private RectTransform RectTransform;
    private Vector3 RectTransformScaleOneMeterDistance;



    public void SetLookToARCamera(bool isLookingToARCamera)
    {
        IsLookingToARCamera = isLookingToARCamera;
        this.Rename();
    }

    public bool GetLookToARCamera() { return IsLookingToARCamera; }


    public void SetScaleWithARCameraDistance(bool isScalingWithARCameraDistance)
    {
        IsScalingWithARCameraDistance = isScalingWithARCameraDistance;
        this.Rename();
    }

    public bool GetScaleithARCameraDistance() { return IsScalingWithARCameraDistance; }


    public void Rename()
    {
        name = (IsLookingToARCamera) ? "ARCameraOrientatedAnnatation" : "FixedOrietatedAnnotation";
        name += (IsScalingWithARCameraDistance) ? "WithAutoScaling" : "WithFixedScaling";
    }


    public void SetAnnotationText(string annotationText)
    {
        TextComponent.text = annotationText;
    }

    public void SetLocalScale(float localScale)
    {
        RectTransform.localScale = new Vector3(localScale, localScale, localScale);
    }



    // Start is called before the first frame update
    void Awake()
    {
        this.Rename();
        AnnotationGameObject = new GameObject();

        RectTransformScaleOneMeterDistance = new Vector3(0.01f, 0.01f, 1f);

        // Initialize Annotation with Canvas
        AnnotationCanvas = AnnotationGameObject.AddComponent<Canvas>();
        AnnotationCanvas.renderMode = RenderMode.WorldSpace;
        AnnotationCanvasScaler = AnnotationGameObject.AddComponent<CanvasScaler>();
        AnnotationCanvasScaler.scaleFactor = 10.0f;
        AnnotationCanvasScaler.dynamicPixelsPerUnit = 50f;
        AnnotationGraphicRaycaster = AnnotationGameObject.AddComponent<GraphicRaycaster>();
        AnnotationGameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 3.0f);
        AnnotationGameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 3.0f);


        AnnotationGameObject.name = "Annotation";
        bool bWorldPosition = false;

        AnnotationGameObject.GetComponent<RectTransform>().SetParent(transform, bWorldPosition);
        AnnotationGameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        AnnotationGameObject.transform.localScale = new Vector3(1, 1, 1);
        // 1.0f / this.transform.localScale.x * 0.1f,
        // 1.0f / this.transform.localScale.y * 0.1f,
        // 1.0f / this.transform.localScale.z * 0.1f);

        // Initialize Canvas with text component
        TextField = new GameObject();
        TextField.name = "Text";
        TextField.transform.parent = AnnotationGameObject.transform;
        TextComponent = TextField.AddComponent<Text>();

        RectTransform = TextField.GetComponent<RectTransform>();
        RectTransform.localScale = RectTransformScaleOneMeterDistance;
        RectTransform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 3.0f);
        RectTransform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 3.0f);

        TextComponent.alignment = TextAnchor.MiddleCenter;
        TextComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        TextComponent.verticalOverflow = VerticalWrapMode.Overflow;
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        TextComponent.font = ArialFont;
        TextComponent.fontSize = FontSize;
        TextComponent.text = "";
        TextComponent.enabled = true;
        TextComponent.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraPosition = Camera.main.transform.position;

        if (GetLookToARCamera())
        {
            // Face the camera directly.
            transform.LookAt(cameraPosition);

            // Rotate so the visible side faces the camera.
            transform.Rotate(0, 180, 0);
        }

        if (GetScaleithARCameraDistance())
        {
            // Scale annotation for keeping the same size by calculating the scale depending on the distance of the AR camera to the annotation
            float annotationCameraDistance = Vector3.Distance(cameraPosition, AnnotationGameObject.transform.position);
            RectTransform.localScale = RectTransformScaleOneMeterDistance * annotationCameraDistance;
        }
    }
}