using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;




public class AnnotationManager : MonoBehaviour //CityGMLObjectManager
{
    public GameObject AnnotationPrefab;
    public DatabaseService DatabaseService { get; set; }

    private const int boundingBoxDimension = 300;


    private void Awake()
    {
        DatabaseService = GameObject.Find("AR Session Origin").GetComponent<TargetDetector>().DatabaseService;
    }



    public void CreateGameObjectsAroundTarget(double3 targetUMLCoordinates, Dictionary<string, Building> buildingsWithinBoundingBox)
    {
        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": AnnotationManager determining BoundinBox around detected target");

        double3 lowerLeftCorner = new double3(targetUMLCoordinates.x - (double)(0.5 * boundingBoxDimension), targetUMLCoordinates.y - (double)(0.5 * boundingBoxDimension), 0);
        double3 upperRightCorner = new double3(targetUMLCoordinates.x + (double)(0.5 * boundingBoxDimension), targetUMLCoordinates.y + (double)(0.5 * boundingBoxDimension), 0);
        BoundingBox boundingBoxAroundTarget = new BoundingBox(lowerLeftCorner, upperRightCorner);

        // -- BuildingAnnatitions ----------
        List<BuildingAnnotation> buildingAnnotations = DatabaseService.GetBuildingAnnotation(boundingBoxAroundTarget, buildingsWithinBoundingBox);

        foreach (BuildingAnnotation buildingAnnotation in buildingAnnotations)
        {
            Debug.Log("BuildingAnnotation assiciated with:\n" + buildingAnnotation.AssociatedBuilding.ToString());

            // Falls die BoundingBox des Gebäudes für die BuildingAnnotation unvollständig ist, diese Annotation ignorieren
            if (buildingAnnotation.AssociatedBuilding.BoundingBox.GetGroundSurfaceCenter().HasValue)
            {
                double3 groundSurfaceCenter = buildingAnnotation.AssociatedBuilding.BoundingBox.GetGroundSurfaceCenter().Value;
                Vector3 unityCoordinates = CoordinateTransformer.GetUnityCoordinatesArroundTarget(new double3(groundSurfaceCenter.x, groundSurfaceCenter.y, groundSurfaceCenter.z + buildingAnnotation.AssociatedBuilding.MeasuredHeight + BuildingAnnotation.meterAboveBuilding), targetUMLCoordinates);

                CreateSimpleTextAnnotation(unityCoordinates, buildingAnnotation.AnnotationProperties, buildingAnnotation.AnnotationComponent);
            }
            else
            {
                Debug.Log("Cannot determin the gournd surface center of Building with CityGMLID: " + buildingAnnotation.AssociatedBuilding.CityGMLID);
            }
        }


        // -- SurfaceAnnotations ----------
        Dictionary<string, Surface> surfacesWithinBoundingBox = new Dictionary<string, Surface>();
        foreach (Building building in buildingsWithinBoundingBox.Values)
        {
            foreach (KeyValuePair<string, Surface> surface in building.ExteriorSurfaces)
            {
                surfacesWithinBoundingBox.Add(surface.Key, surface.Value);
            }
        }

        List<SurfaceAnnotation> surfaceAnnotations = DatabaseService.GetSurfaceAnnotation(boundingBoxAroundTarget, surfacesWithinBoundingBox);

        Debug.Log("Anzahl SurfaceAnnotation innerhalb der BoundingBox: " + surfaceAnnotations.Count);

        foreach (SurfaceAnnotation surfaceAnnotation in surfaceAnnotations)
        {
            double3 fristBaselinePoint = surfaceAnnotation.AssociatedSurface.Polygon[surfaceAnnotation.AnnotationAnchorPointIndex];
            double3 secondBaselinePoint = surfaceAnnotation.AssociatedSurface.Polygon[surfaceAnnotation.AnnotationAnchorPointIndex + 1];

            // Surface normal pointing behind the annotation
            Vector3 baseLineDirectionVector = (float3)(secondBaselinePoint - fristBaselinePoint);
            Vector3 unitySurfaceNormal = CoordinateTransformer.GetLeftHandedCoordinates(surfaceAnnotation.AssociatedSurface.GetSurfaceNormal());
            Vector3 goFromFirstPoint = ((Vector3)(float3)(baseLineDirectionVector) * (float)surfaceAnnotation.RelativePositionBetweenBasePoints);
            double3 realWorldAnnotationPosition = (fristBaselinePoint + (float3)goFromFirstPoint);

            realWorldAnnotationPosition = realWorldAnnotationPosition + (float3)(unitySurfaceNormal.normalized * SurfaceAnnotation.SurfaceOffset);
            realWorldAnnotationPosition.z += surfaceAnnotation.HeightAboveBaseLine;

            Vector3 unityCoordinates = CoordinateTransformer.GetUnityCoordinatesArroundTarget(realWorldAnnotationPosition, targetUMLCoordinates);

            // TODO: falls die Ausrichtung nicht in der CityGML festgelegt wurde, Ausrichtung anhand des Surface
            if (surfaceAnnotation.AnnotationProperties.PointingDirection.Equals(Vector3.zero))
            {
                surfaceAnnotation.AnnotationProperties.PointingDirection = unitySurfaceNormal * -1;
            }

            CreateSimpleTextAnnotation(unityCoordinates, surfaceAnnotation.AnnotationProperties, surfaceAnnotation.AnnotationComponent);
        }


        // -- WorldCoordinateAnnotations ----------
        List<WorldCoordinateAnnotation> worldCoordinateAnnotations = DatabaseService.GetWorldCoordinateAnnotation(boundingBoxAroundTarget);

        foreach (WorldCoordinateAnnotation worldCoordinateAnnotation in worldCoordinateAnnotations)
        {
            Vector3 unityCoordinates = CoordinateTransformer.GetUnityCoordinatesArroundTarget(worldCoordinateAnnotation.AnnotationUMLCoordinates, targetUMLCoordinates);
            CreateSimpleTextAnnotation(unityCoordinates, worldCoordinateAnnotation.AnnotationProperties, worldCoordinateAnnotation.AnnotationComponent);
        }

        /*
                // Overview Annotation
                Vector3 unityCoordinates_Seminarraum1 = this.GetUnityCoordinatesArroundTarget(new double3(33310196.666, 5995821.237, 49), targetRealWorldCoordinates);
                this.CreateOverviewAnnotation(unityCoordinates_Seminarraum1, "↓ Seminarraum 1");

                Vector3 unityCoordinates_JustusVLNr8 = this.GetUnityCoordinatesArroundTarget(new double3(33310096.354, 5995790.708, 55), targetRealWorldCoordinates);
                this.CreateOverviewAnnotation(unityCoordinates_JustusVLNr8, "↓ J.-v.-L Weg 8");

                // Focus Annotation
                Vector3 unityCoordinates_focus = this.GetUnityCoordinatesArroundTarget(targetRealWorldCoordinates, targetRealWorldCoordinates);
                unityCoordinates_focus = new Vector3(unityCoordinates_focus.x, unityCoordinates_focus.y + 0.1f, unityCoordinates_focus.z);
                Vector3 annotationPointingDirection = new Vector3(1,0,0);
                this.CreateFocusAnnotation(unityCoordinates_focus, annotationPointingDirection, "WallAnnotation
        */

        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Creating Annotations around target succeeded");

    }



    private void CreateSimpleTextAnnotation(Vector3 position, AnnotationProperties annotationProperties, AnnotationComponent annotationComponent)
    {
        switch (annotationComponent)
        {
            case TextAnnotationComponent annotationTextComponent:

                annotationTextComponent = (TextAnnotationComponent)annotationComponent;

                // Create new SimpleTextAnnotation GameObject from AnnotationPrefab
                GameObject annotationGameObject = Instantiate(AnnotationPrefab, new Vector3(0, 0.1f, 0), Quaternion.identity);
                annotationGameObject.transform.parent = transform;
                AnnotationPrefabScript annotationPrefabScript = (AnnotationPrefabScript)annotationGameObject.GetComponent(typeof(AnnotationPrefabScript));

                annotationPrefabScript.SetAnnotationText(annotationTextComponent.Text);
                annotationPrefabScript.SetLocalScale(annotationTextComponent.TextSize);

                // Apply AnnotationProperties
                annotationGameObject.transform.position = position;
                if (annotationProperties.PointingDirection.Equals(Vector3.zero))
                {
                    annotationPrefabScript.SetLookToARCamera(true);
                }
                else
                {
                    annotationPrefabScript.SetLookToARCamera(false);
                    annotationGameObject.transform.rotation = Quaternion.LookRotation(new Vector3((float)annotationProperties.PointingDirection.x, (float)annotationProperties.PointingDirection.y, (float)annotationProperties.PointingDirection.z));
                }
                annotationPrefabScript.SetScaleWithARCameraDistance(annotationProperties.ScaleWithCameraDistance);


                break;

            default: throw new NotImplementedException();
        }





    }


    private void CreateOverviewAnnotation(Vector3 position, string annotationText, float localScale)
    {
        (GameObject annotationGameObject, AnnotationPrefabScript annotationPrefabScript) = CreateAnnotationGameObject(annotationText, localScale);
        annotationGameObject.transform.position = position;
        annotationPrefabScript.SetLookToARCamera(true);
        annotationPrefabScript.SetScaleWithARCameraDistance(true);
    }


    private void CreateFocusAnnotation(Vector3 position, Vector3 pointingDirection, string annotationText, float localScale)
    {
        (GameObject annotationGameObject, AnnotationPrefabScript annotationPrefabScript) = CreateAnnotationGameObject(annotationText, localScale);
        annotationGameObject.transform.position = position;
        annotationGameObject.transform.rotation = Quaternion.LookRotation(pointingDirection);
    }


    public (GameObject, AnnotationPrefabScript) CreateAnnotationGameObject(string annotationText, float localScale)
    {
        GameObject annotationGameObject = Instantiate(AnnotationPrefab, new Vector3(0, 0.1f, 0), Quaternion.identity);
        annotationGameObject.transform.parent = transform;
        AnnotationPrefabScript nnotationPrefabScript = (AnnotationPrefabScript)annotationGameObject.GetComponent(typeof(AnnotationPrefabScript));

        nnotationPrefabScript.SetAnnotationText(annotationText);
        nnotationPrefabScript.SetLocalScale(localScale);

        return (annotationGameObject, nnotationPrefabScript);
    }

    public void UpdateAnnotationAnchor(Vector3 trackedImagePosition, Quaternion trackedImageRotation)
    {
        transform.position = trackedImagePosition;
        transform.rotation = trackedImageRotation;
    }
}