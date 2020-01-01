using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationManager : CityGMLFactory
{
    public GameObject AnnotationPrefab;
    private DatabaseService _DatabaseService;

    private const int boudingBoxDimension = 300;


    private void Awake()
    {
        _DatabaseService = GameObject.Find("AR Session Origin").GetComponent<TargetDetector>()._DatabaseService;
    }
    


    public override void CreateGameObjectsAroundTarget(double3 targetRealWorldCoordinates)
    {
        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Generating BoundinBox around detected target");
        double3 lowerLeftCorner = new double3(targetRealWorldCoordinates.x - (double)(0.5 * boudingBoxDimension), targetRealWorldCoordinates.y - (double)(0.5 * boudingBoxDimension), 0);
        double3 upperRightCorner = new double3(targetRealWorldCoordinates.x + (double)(0.5 * boudingBoxDimension), targetRealWorldCoordinates.y + (double)(0.5 * boudingBoxDimension), 0);
        BoundingBox boundingBoxAroundTarget = new BoundingBox(lowerLeftCorner, upperRightCorner);


        List<BuildingAnnotation> buildingAnnotations = _DatabaseService.GetBuildingAnnotation(boundingBoxAroundTarget);
        List<SurfaceAnnotation> surfaceAnnotations = _DatabaseService.GetSurfaceAnnotation(boundingBoxAroundTarget);
        List<FreeWorldAnnotation> freeWorldAnnotations = _DatabaseService.GetFreeWorldAnnotation(boundingBoxAroundTarget);


        foreach (BuildingAnnotation buildingAnnotation in buildingAnnotations)
        {
            double2 groundSurfaceCenter = buildingAnnotation.BuildingBoundingBox.GetGroundSurfaceCenter();
            Vector3 unityCoordinates = this.GetUnityCoordinatesArroundTarget(new double3(groundSurfaceCenter.x, groundSurfaceCenter.y, buildingAnnotation.BuildingHeight), targetRealWorldCoordinates);

            this.CreateOverviewAnnotation(unityCoordinates, buildingAnnotation.AnnotationText, buildingAnnotation.LocalScale);
        }

        foreach (SurfaceAnnotation surfaceAnnotation in surfaceAnnotations)
        {
            double3 fristPoint = surfaceAnnotation.GroundSurfacePoints[0];
            double3 secondPoint = surfaceAnnotation.GroundSurfacePoints[1];

            // Surface normal pointing behind the annotation
            Vector3 groundSurfaceBaseLineVector = (float3)(secondPoint - fristPoint);
            Vector3 annotationToSurfaceDirection = Vector3.Cross((float3) (this.GetLeftHandedCoordinates((Vector3)groundSurfaceBaseLineVector)), Vector3.down).normalized;

            Debug.Log(fristPoint);
            Debug.Log(secondPoint);

            Vector3 goFromFirstPoint = ((Vector3)(float3)(groundSurfaceBaseLineVector) * (float)surfaceAnnotation.RelativeGroundSurfacePosition);
            double3 realWorldAnnotationPosition = (fristPoint + (float3) goFromFirstPoint);
            Debug.Log("realWorldAnnotationPosition (left handed): " + realWorldAnnotationPosition.ToString());
            realWorldAnnotationPosition = realWorldAnnotationPosition + (float3) (annotationToSurfaceDirection.normalized * -1 * surfaceAnnotation.SurfaceOffset);
            Debug.Log("finalAnnotationPosition: " + realWorldAnnotationPosition.ToString());
            realWorldAnnotationPosition.z += surfaceAnnotation.Height;





            Vector3 unityCoordinates = this.GetUnityCoordinatesArroundTarget(realWorldAnnotationPosition, targetRealWorldCoordinates);


            this.CreateFocusAnnotation(unityCoordinates, annotationToSurfaceDirection, surfaceAnnotation.AnnotationText, surfaceAnnotation.LocalScale);
        }

        foreach (FreeWorldAnnotation freeWorldAnnotation in freeWorldAnnotations)
        {
            Vector3 unityCoordinates = this.GetUnityCoordinatesArroundTarget(freeWorldAnnotation.RealWorldCoordinate, targetRealWorldCoordinates);

            this.CreateOverviewAnnotation(unityCoordinates, freeWorldAnnotation.AnnotationText, freeWorldAnnotation.LocalScale);
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
                this.CreateFocusAnnotation(unityCoordinates_focus, annotationPointingDirection, "WallAnnotation");


        */

        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Creating Annotations around target succeeded");

    }


    private void CreateOverviewAnnotation(Vector3 position, string annotationText, float localScale)
    {
        (GameObject annotationGameObject, Annotation annotation) = this.CreateAnnotationGameObject(annotationText, localScale);
        annotationGameObject.transform.position = position;
        annotation.SetLookToARCamera(true);
        annotation.SetScaleWithARCameraDistance(true);
    }


    private void CreateFocusAnnotation(Vector3 position, Vector3 pointingDirection, string annotationText, float localScale)
    {
        (GameObject annotationGameObject, Annotation annotation) = this.CreateAnnotationGameObject(annotationText, localScale);
        annotationGameObject.transform.position = position;
        annotationGameObject.transform.rotation = Quaternion.LookRotation(pointingDirection);
    }


    public (GameObject, Annotation) CreateAnnotationGameObject(string annotationText, float localScale)
    {
        GameObject annotationGameObject = Instantiate(AnnotationPrefab, new Vector3(0, 0.1f, 0), Quaternion.identity);
        annotationGameObject.transform.parent = this.transform;
        Annotation annotation = (Annotation)annotationGameObject.GetComponent(typeof(Annotation));

        annotation.SetAnnotationText(annotationText);
        annotation.SetLocalScale(localScale);

        return (annotationGameObject, annotation);
    }

    public void UpdateAnnotationAnchor(Vector3 trackedImagePosition, Quaternion trackedImageRotation)
    {
        this.transform.position = trackedImagePosition;
        this.transform.rotation = trackedImageRotation;
    }
}
