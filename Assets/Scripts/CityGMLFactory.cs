using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;




public static class CoordinateTransformer
{
    public static Vector3 GetUnityCoordinatesArroundTarget(double3 realWorldCoordinate, double3 targetRealWorldCoordinate)
    {
        float x = (float)(realWorldCoordinate.x - targetRealWorldCoordinate.x);
        float y = (float)(realWorldCoordinate.y - targetRealWorldCoordinate.y);
        float z = (float)(realWorldCoordinate.z - targetRealWorldCoordinate.z);

        return GetLeftHandedCoordinates(new Vector3(x, y, z));
    }

    public static Vector3 GetLeftHandedCoordinates(Vector3 realWorldCoordiantes)
    {
        return new Vector3(realWorldCoordiantes.x, realWorldCoordiantes.z, realWorldCoordiantes.y);
    }

    public static double3 GetLeftHandedCoordinates(double3 realWorldCoordiantes)
    {
        return new double3(realWorldCoordiantes.x, realWorldCoordiantes.z, realWorldCoordiantes.y);
    }



}


/*
*/
public abstract class CityGMLObjectManager<T> : MonoBehaviour
{
    protected string CityGMLID;

    // TODO: hier einheitliche Distanz definieren

    public abstract Dictionary<string,T> CreateGameObjectsAroundTarget(double3 targetRealWorldCoordinates);
}