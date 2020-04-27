using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;




/// <summary>
/// Deserializes a CityGML file and manages and displays the derived Building
/// </summary>
public class BuildingManager : MonoBehaviour //CityGMLObjectManager<Building>
{
    private const int boundingBoxDimension = 300;

    private BuildingMeshFactory BuildingMeshFactory;
    public DatabaseService DatabaseService { get; set; }

    private MeshFilter MeshFilter;

    /// <summary>
    /// List of all building managed by the BuildingManager instance
    /// </summary>
    //public List<Building> BuildingRenderingList { get; private set; } = new List<Building>();




    /// <summary>
    /// Returns all contained buildings with its parameters
    /// </summary>
    /// <returns></returns>
    //private new string ToString()
    //{
    //    string output = "BuildingManager with\n" + BuildingRenderingList.Count + " buildings:\n";
    //
    //    foreach (Building currentBuilding in BuildingRenderingList)
    //    {
    //        output += currentBuilding.ToString();
    //    }
    //
    //    return output += "\n\n";
    //}



    public Dictionary<string, Building> CreateGameObjectsAroundTarget(double3 targetRealWorldCoordinates)
    {
        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": BuildingManager determining BoundinBox around detected target");
        double3 lowerLeftCorner = new double3(targetRealWorldCoordinates.x - (double)(0.5 * boundingBoxDimension), targetRealWorldCoordinates.y - (double)(0.5 * boundingBoxDimension), 0);
        double3 upperRightCorner = new double3(targetRealWorldCoordinates.x + (double)(0.5 * boundingBoxDimension), targetRealWorldCoordinates.y + (double)(0.5 * boundingBoxDimension), 0);
        BoundingBox boundingBoxAroundTarget = new BoundingBox(lowerLeftCorner, upperRightCorner);

        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Querying buildings within " + boundingBoxAroundTarget.ToString());
        List<Building> BuildingRenderingList = new List<Building>(); ;

        Dictionary<string, Building> buildingsWithinBoundingBox = DatabaseService.GetBuildings(boundingBoxAroundTarget);

        BuildingRenderingList.AddRange(buildingsWithinBoundingBox.Values);

        string debugString = BuildingRenderingList.Count + " added to BuildingRenderingList:\n\n";
        foreach (Building building in BuildingRenderingList)
        {
            debugString += building.ToString();
        }
        Debug.Log(debugString);

        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Buildings added to temporary BuildingManagerList");

        MeshFilter.mesh = BuildingMeshFactory.CreateMesh(BuildingRenderingList, targetRealWorldCoordinates, gameObject);

        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Building Mesh was created");

        return buildingsWithinBoundingBox;
    }


    public void UpdateMeshPosition(float3 trackedImagePosition, Quaternion trackedImageRotation)
    {
        MeshFilter.transform.position = trackedImagePosition;
        MeshFilter.transform.rotation = trackedImageRotation;
    }



    private void Awake()
    {
        BuildingMeshFactory = new BuildingMeshFactory();

        MeshFilter = gameObject.GetComponent<MeshFilter>();
    }
}













public class BuildingMeshFactory
{
    private LevelOfDetail LevelOfDetail;

    public BuildingMeshFactory()
    {

    }

    private Surface TransformToUnityCoordinates(Surface surfaceWithUTMCoordinates, double3 targetWithUTMCoordinates)
    {
        List<double3> surfacePointsWithUTMCoordinates = new List<double3>();

        try
        {
            foreach (double3 point in surfaceWithUTMCoordinates.Polygon)
            {
                surfacePointsWithUTMCoordinates.Add((float3)TransformToUnityCoordinates(point, targetWithUTMCoordinates));
            }

            return new Surface(surfaceWithUTMCoordinates.CityGMLID, surfaceWithUTMCoordinates.Type, surfacePointsWithUTMCoordinates);
        }
        catch (Exception e)
        {
            throw new Exception("Surface UTM coordinates cannot be transformed into Unity coordinates:\n" + e);
        }
    }

    private Vector3 TransformToUnityCoordinates(double3 pointWithUTMCoordinates, double3 targetWithUTMCoordinates)
    {
        try
        {
            float UnityCoordinateX = (float)(pointWithUTMCoordinates.x - targetWithUTMCoordinates.x);
            float UnityCoordinateZ = (float)(pointWithUTMCoordinates.y - targetWithUTMCoordinates.y);
            float UnityCoordinateY = (float)(pointWithUTMCoordinates.z - targetWithUTMCoordinates.z);

            return new Vector3(UnityCoordinateX, UnityCoordinateY, UnityCoordinateZ);
        }
        catch (InvalidCastException e)
        {
            throw new InvalidCastException("Double could not be typecasted to float, because the catchment area is too large:\n" +
                "\nPoint with UTM coordinates: " + pointWithUTMCoordinates.ToString() +
                "\nTarget position in real world: " + targetWithUTMCoordinates.ToString() + "\n" + e);
        }
    }



    /// <summary>
    /// Returns the vertices of the ground and roof surface. Deviating from the points in the CityGML file, y corresponds to the height coordinate and the x- and z-coordinates to the horizontal positioning.
    /// </summary>
    /// <param name="building">Building with ground surface</param>
    /// <returns>Vertices of the ground and roof surface with y as the height</returns>
    private Dictionary<string, Surface> TransformToUnityCoordinates(Building building, double3 targetRealWorldPosition)
    {
        Dictionary<string, Surface> surfacesWithUnityCoordinates = new Dictionary<string, Surface>();

        foreach (KeyValuePair<string, Surface> uniqueKeyAssociatedSurface in building.ExteriorSurfaces)
        {
            surfacesWithUnityCoordinates.Add(uniqueKeyAssociatedSurface.Key, TransformToUnityCoordinates(uniqueKeyAssociatedSurface.Value, targetRealWorldPosition));
        }

        return surfacesWithUnityCoordinates;
    }



    /// <summary>
    /// Creates meshes for each building contained in the class owned building list "Buildings"
    /// </summary>
    private Mesh GetBuildingMesh(Building building, double3 targetRealWorldPosition, GameObject gameObjectBuildingManger)
    {
        /// Assumes that the polygons are planar, but oriented arbitrarily in 3D space

        try
        {
            Dictionary<string, Surface> surfacesWithUnityCoordinates = TransformToUnityCoordinates(building, targetRealWorldPosition);

            /// The points must be present clockwise from the point of view of the desired viewing direction. 
            /// Since the points in the XML file are ordered clockwise for the top view, the point order of the roofSurfaceVerticies is correct, 
            /// whereas a reverse is required for the groundSurfaceVerticies. 

            /// Create Mesh of side surfaces:
            /// Modelling of the wall surfaces each by two adjacent points of the ground surface and two corresponding points of the roof 
            /// surface derived by using the measuredHeight of the whole building part


            //Debug.Log("Surface Dictionary mit " + surfacesWithUnityCoordinates.Count + " Einträgen");

            /*
            foreach (Surface surface in building.ExteriorSurfaces.Values)
            {
                Debug.Log(surface.ToString());
            }
            */


            List<Mesh> surfaceMeshList = new List<Mesh>();

            foreach (Surface surface in surfacesWithUnityCoordinates.Values)
            {
                //Debug.Log("SurfacesCityGMLID: " + surface.CityGMLID +  ", " + surface.Type);

                // Extract Vertices
                Mesh surfaceMesh = new Mesh();

                Vector3[] surfaceVertices = new Vector3[surface.Polygon.Count];
                try
                {
                    for (int i = 0; i < surface.Polygon.Count; i++)
                    {
                        surfaceVertices[i] = ((float3)surface.Polygon[i]);
                    }
                    surfaceMesh.vertices = surfaceVertices;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogError("Cannot determin Vector3 from double3\nAffected Building:" + building.CityGMLID + "\nBuildung will not be displayed" + e);
                    continue;
                }

                // TODO: Warum SurfacePosition angeben
                surfaceMesh.triangles = SurfaceTriangulator.GetTriangles(surfaceVertices, surface);
                surfaceMesh.RecalculateNormals();

                surfaceMeshList.Add(surfaceMesh);
            }

            return CombineMeshes(surfaceMeshList, gameObjectBuildingManger);
        }
        catch (ArgumentException e)
        {
            Debug.LogError("Error when converting the projected UTM coordinates into coordinates of the Unity coordinate system: " + e);
            return null;
        }
    }

    /// <summary>
    /// Combines several meshes into one mesh
    /// </summary>
    /// <param name="meshes">Meshes to be combined</param>
    /// <returns>One mesh with all meshes in the Mesh list</returns>
    private Mesh CombineMeshes(List<Mesh> meshes, GameObject gameObjectBuildingManger)
    {
        CombineInstance[] combine = new CombineInstance[meshes.Count];

        for (int i = 0; i < meshes.Count; i++)
        {
            combine[i].mesh = meshes[i];
            combine[i].transform = gameObjectBuildingManger.transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.CombineMeshes(combine);
        return mesh;
    }



    public Mesh CreateMesh(List<Building> buildings, double3 targetRealWorldPosition, GameObject gameObjectBuildingManger)
    {
        List<Mesh> buildingMeshes = new List<Mesh>();


        foreach (Building building in buildings)
        {
            Dictionary<string, Surface> selectedSurfaces = new Dictionary<string, Surface>();

            switch (LevelOfDetail)
            {
                case LevelOfDetail.LoD1:
                    // TODO: bei LoD1 Bestimmung der GS
                    selectedSurfaces = building.GetSurfaces(SurfaceType.GroundSurface);

                    // TODO: LOD1-Ableitung aus DB mit UNDEFINED SurfaceType



                    buildingMeshes.Add(GetBuildingMeshLoD1(building, targetRealWorldPosition, gameObjectBuildingManger));
                    break;
                case LevelOfDetail.LoD2:

                    break;
                default:
                    break;
            }
            buildingMeshes.Add(GetBuildingMesh(building, targetRealWorldPosition, gameObjectBuildingManger));

            //buildingMeshes.Add(GetBuildingMesh(building, targetRealWorldPosition, gameObjectBuildingManger, SurfacePosition.InteriorSurface));
        }

        Mesh allBuildingMeshes = new Mesh();
        allBuildingMeshes.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        allBuildingMeshes = CombineMeshes(buildingMeshes, gameObjectBuildingManger);
        allBuildingMeshes.Optimize();
        allBuildingMeshes.RecalculateNormals();

        return allBuildingMeshes;
    }




    /// <summary>
    /// Returns the vertices of the ground and roof surface. Deviating from the points in the CityGML file, y corresponds to the height coordinate and the x- and z-coordinates to the horizontal positioning.
    /// </summary>
    /// <param name="surface">Building with ground surface</param>
    /// <returns>Vertecies of the ground and roof surface with y as the height</returns>
    private (Vector3[], Vector3[]) GetVerticiesGroundSurfaceRoofSurfaceVerteciesPair(Surface surface, float measuredHeight, double3 targetRealWorldPosition)
    {
        /// Create Vector3 vertices from the GroundSurface
        Vector3[] GroundSurfaceVerticies = new Vector3[surface.Polygon.Count];
        Vector3[] RoofSurfaceVerticies = new Vector3[surface.Polygon.Count];

        for (int pointIndex = 0; pointIndex < surface.Polygon.Count; pointIndex++)
        {
            double3 currentPoint = surface.Polygon[pointIndex];
            try
            {
                float UnityCoordinateX = (float)(currentPoint.x - targetRealWorldPosition.x);
                float UnityCoordinateZ = (float)(currentPoint.y - targetRealWorldPosition.y);
                float UnityCoordinateY = (float)(currentPoint.z - targetRealWorldPosition.z);

                GroundSurfaceVerticies[pointIndex] = new Vector3(UnityCoordinateX, UnityCoordinateY, UnityCoordinateZ);
                RoofSurfaceVerticies[pointIndex] = new Vector3(UnityCoordinateX, UnityCoordinateY + measuredHeight, UnityCoordinateZ);

            }
            catch (ArgumentException)
            {
                throw new ArgumentException("Double could not be typecasted to float, because the catchment area is too large:\n" +
                    "\nGroundSurfacePoint: " + currentPoint.ToString() +
                    "\nTarget position in real world: " + targetRealWorldPosition.ToString());
            }
        }

        return (GroundSurfaceVerticies, RoofSurfaceVerticies);
    }



    /// <summary>
    /// Creates meshes for each buidliung contained in the class owned building list "Buildings"
    /// </summary>
    private Mesh GetBuildingMeshLoD1(Building building, double3 targetRealWorldPosition, GameObject gameObjectBuildingManger)
    {
        List<Mesh> buildingMeshes = new List<Mesh>();

        foreach (Surface surface in building.GetSurfaces(SurfaceType.GroundSurface).Values)
        {
            (Vector3[] groundSurfaceVerticies, Vector3[] roofSurfaceVerticies) groundSurfaceRoofSurfaceVerticesPair = GetVerticiesGroundSurfaceRoofSurfaceVerteciesPair(surface, building.MeasuredHeight, targetRealWorldPosition);

            try
            {
                /// Assumes that the polygons are planar, but oriented arbitrarily in 3D space

                Vector3[] groundSurfaceVerticies = groundSurfaceRoofSurfaceVerticesPair.groundSurfaceVerticies;
                Vector3[] roofSurfaceVerticies = groundSurfaceRoofSurfaceVerticesPair.roofSurfaceVerticies;

                /// The points must be present clockwise from the point of view of the desired viewing direction. 
                /// Since the points in the XML file are ordered clockwise for the top view, the point order of the roofSurfaceVerticies is correct, 
                /// whereas a reverse is required for the groundSurfaceVerticies. 


                /// Create Mesh of side surfaces:
                /// Modelling of the wall surfaces each by two adjacent points of the ground surface and two corresponding points of the roof 
                /// surface derived by using the measuredHeight of the whole building part

                Vector3[] wallSurfaceGoundVerticies = new Vector3[groundSurfaceVerticies.Length + 1];
                groundSurfaceVerticies.CopyTo(wallSurfaceGoundVerticies, 0);
                wallSurfaceGoundVerticies[groundSurfaceVerticies.Length] = wallSurfaceGoundVerticies[0];

                List<Mesh> wallSurfaceMeshList = new List<Mesh>();

                Vector3[] wallSurfaceVerticies;
                /// Iterating over all side surfaces of the building
                for (int buildingEdge = 0; buildingEdge < wallSurfaceGoundVerticies.Length - 1; buildingEdge++)
                {

                    wallSurfaceVerticies = new Vector3[] {
                    groundSurfaceVerticies[buildingEdge],                                           /// 0    2 ——— ——— 3
                    groundSurfaceVerticies[(buildingEdge + 1) % groundSurfaceVerticies.Length],     /// 1    |         |
                    roofSurfaceVerticies[(buildingEdge + 1) % groundSurfaceVerticies.Length],       /// 2    |         |
                    roofSurfaceVerticies[buildingEdge]                                              /// 3    1 ——— ——— 0
                };

                    Mesh wallSurfaceMesh = new Mesh();
                    wallSurfaceMesh.vertices = wallSurfaceVerticies;
                    wallSurfaceMesh.triangles = new int[] { 0, 1, 3, 1, 2, 3 }; // SurfaceTriangulator.GetTriangles(wallSurfaceVerticies); //
                    wallSurfaceMesh.RecalculateNormals();

                    wallSurfaceMeshList.Add(wallSurfaceMesh);
                }


                /// Create Mesh of roofsurface
                Mesh roofSurfaceMesh = new Mesh();
                roofSurfaceMesh.vertices = roofSurfaceVerticies;
                roofSurfaceMesh.triangles = SurfaceTriangulator.GetTriangles(roofSurfaceVerticies, surface);
                roofSurfaceMesh.RecalculateNormals();


                /// Create Mesh of groundsurface
                Mesh groundSurfaceMesh = new Mesh();
                groundSurfaceMesh.vertices = groundSurfaceVerticies;
                groundSurfaceMesh.triangles = SurfaceTriangulator.GetTriangles(groundSurfaceVerticies, surface);
                groundSurfaceMesh.RecalculateNormals();

                groundSurfaceMesh = SurfaceTriangulator.GetInvertedMesh(groundSurfaceMesh);

                buildingMeshes.Add(CombineMeshes(new List<Mesh>() { groundSurfaceMesh, roofSurfaceMesh, CombineMeshes(wallSurfaceMeshList, gameObjectBuildingManger) }, gameObjectBuildingManger));
            }
            catch (ArgumentException e)
            {
                Debug.LogError("Error when converting the projected UTM coordinates into coordinates of the Unity coordinate system: " + e);
                return null;
            }
        }

        return CombineMeshes(buildingMeshes, gameObjectBuildingManger);
    }




}