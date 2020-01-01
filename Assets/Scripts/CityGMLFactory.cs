using Unity.Mathematics;
using UnityEngine;

public abstract class CityGMLFactory : MonoBehaviour
{
    protected Vector3 GetUnityCoordinatesArroundTarget(double3 realWorldCoordinate, double3 targetRealWorldCoordinate)
    {
        float x = (float)(realWorldCoordinate.x - targetRealWorldCoordinate.x);
        float y = (float)(realWorldCoordinate.y - targetRealWorldCoordinate.y);
        float z = (float)(realWorldCoordinate.z - targetRealWorldCoordinate.z);

        return GetLeftHandedCoordinates(new Vector3(x, y, z));
    }

    protected Vector3 GetLeftHandedCoordinates(Vector3 realWorldCoordiantes)
    {
        return new Vector3(realWorldCoordiantes.x, realWorldCoordiantes.z, realWorldCoordiantes.y);
    }

    protected double3 GetLeftHandedCoordinates(double3 realWorldCoordiantes)
    {
        return new double3(realWorldCoordiantes.x, realWorldCoordiantes.z, realWorldCoordiantes.y);
    }


    public abstract void CreateGameObjectsAroundTarget(double3 targetRealWorldCoordinates);

}