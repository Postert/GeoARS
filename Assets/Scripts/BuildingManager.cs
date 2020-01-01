using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Unity.Mathematics;
using UnityEngine;



/// <summary>
/// Deserializes a CityGML file and manages and displays the derived Building
/// </summary>
public class BuildingManager : MonoBehaviour
{
    private const int boudingBoxDimension = 300;


    private BuildingMeshFactory BuildingMeshFactory = new BuildingMeshFactory();
    private DatabaseService _DatabaseService;

    private MeshFilter _MeshFilter;


    public void CreateGameObjectsAroundTarget(double3 targetRealWorldCoordinates)
    {
            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Generating BoundinBox around detected target");
            double3 lowerLeftCorner = new double3(targetRealWorldCoordinates.x - (double)(0.5 * boudingBoxDimension), targetRealWorldCoordinates.y - (double)(0.5 * boudingBoxDimension), 0);
            double3 upperRightCorner = new double3(targetRealWorldCoordinates.x + (double)(0.5 * boudingBoxDimension), targetRealWorldCoordinates.y + (double)(0.5 * boudingBoxDimension), 0);
            BoundingBox boundingBoxAroundTarget = new BoundingBox(lowerLeftCorner, upperRightCorner);

            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Querying buildings within " + boundingBoxAroundTarget.ToString());
            List<Building> BuildingRenderingList = new List<Building>(); ;
            BuildingRenderingList.AddRange(_DatabaseService.GetBuildings(boundingBoxAroundTarget));


            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Buildings added to temporary BuildingManagerList");


            _MeshFilter.mesh = BuildingMeshFactory.CreateMesh(BuildingRenderingList, targetRealWorldCoordinates, gameObject);

            Debug.Log(MyTimer.GetSecondsSiceStartAsString() +  ": Building Mesh was created");
    }


    public void UpdateMeshPosition(float3 trackedImagePosition, Quaternion trackedImageRotation)
    {
        _MeshFilter.transform.position = trackedImagePosition;
        _MeshFilter.transform.rotation = trackedImageRotation;
    }









    private void Awake()
    {
        _MeshFilter = gameObject.GetComponent<MeshFilter>();
        _DatabaseService = GameObject.Find("AR Session Origin").GetComponent<TargetDetector>()._DatabaseService;
    }
}






public static class Deserializer
{
    /// <summary>
    /// Returns a list with at least one Building object depending on the occurrence of BuildingPart nodes in the building node. 
    /// The XML node is closed after it has been processed in this method.
    /// </summary>
    /// <param name="xmlSurfaceNode"></param>
    /// <returns>List of Building objects for each part of a building</returns>
    private static List<Building> GetBuildingWithParts(XmlReader buildingReader, Building newBuilding)
    {
        List<Building> buildingWithParts = new List<Building>();

        /// Reading the needed building properties
        buildingReader.ReadToFollowing("bldg:measuredHeight");
        if (!float.TryParse(buildingReader.ReadElementContentAsString(), NumberStyles.Any, CultureInfo.InvariantCulture, out float measuredHeight))
        {
            Debug.LogError("Building does not contain a measuredHeight");
            return null;
        }

        /// Building has GroundSurface: create Building object and add ground surface points.
        newBuilding.MeasuredHeight = measuredHeight;

        /// Determine the level of detail of the current building
        buildingReader.Read();
        switch (buildingReader.Name)
        {
            /// LOD1: Surfaces are directly stored in the "bldg:lod1Solid"-tag.
            /// The ground surface polygon must be geometrically determined.
            case "bldg:lod1Solid":
                /// For LOD1 buildings, the ground surface is not declared using a separate day. 
                /// Thus, the base area is identified by determining the surface with the lowest average height of all spanning vertices. 

                List<double3> lowestPoints = null;
                double lowestAggregatedHeight = double.MaxValue;

                XmlReader allSurfacesReader = buildingReader.ReadSubtree();
                allSurfacesReader.ReadToFollowing("gml:surfaceMember");

                do
                {
                    XmlReader currentSurfaceReader = allSurfacesReader.ReadSubtree();

                    List<double3> currentPoints = Deserializer.DeserializePoints(currentSurfaceReader);

                    double currentAggregatedHeight = 0;
                    foreach (double3 point in currentPoints)
                    {
                        currentAggregatedHeight += point.z;
                    }
                    currentAggregatedHeight = (currentAggregatedHeight / currentPoints.ToArray().Length);

                    if (currentAggregatedHeight < lowestAggregatedHeight)
                    {
                        lowestAggregatedHeight = currentAggregatedHeight;
                        lowestPoints = currentPoints;
                    }

                    currentSurfaceReader.Close();

                } while (allSurfacesReader.ReadToNextSibling("gml:surfaceMember"));

                allSurfacesReader.Close();
                newBuilding.SetGroundSurfacePoints(lowestPoints);
                buildingWithParts.Add(newBuilding);
                break;



            /// LOD2: the "bldg:lod2Solid"-tag contains references to all building surfaces. 
            /// The surfaces themselves are defined individually in the following tags.
            /// The ground surface is marked with the "GroundSurface"-tag. 
            case "bldg:lod2Solid":
                /// Navigate to first point of the groundsurface using a new reader instance with the child notes of the current building node.
                /// Process only the subtree to ensure that just GroundSurface points are considered.
                /// If there is no GroundSurface node in the current building node: skip this building.
                if (!buildingReader.ReadToFollowing("bldg:GroundSurface"))
                {
                    Debug.LogError("Building does not contain a GroundSurface");
                    return null;
                }


                XmlReader groundSurfaceReader = buildingReader.ReadSubtree();
                groundSurfaceReader.ReadToFollowing("gml:surfaceMember");
                newBuilding.SetGroundSurfacePoints(Deserializer.DeserializePoints(groundSurfaceReader));
                groundSurfaceReader.Close();
                buildingWithParts.Add(newBuilding);
                break;

            default: throw new ArgumentException("Unknown LOD: the LOD-tag could not be found.");
        }

        if (buildingReader.ReadToFollowing("bldg:consistsOfBuildingPart"))
        {
            while (buildingReader.ReadToFollowing("bldg:BuildingPart"))
            {
                string currentBuildingID = buildingReader.GetAttribute("gml:id");

                /// Creating new XMLReader instance to process only the inner nodes of the current building node. 
                /// This enshures, that the BuildingPropertyReader only processes the nodes of the current building.
                XmlReader buildingPartReader = buildingReader.ReadSubtree();

                buildingWithParts.AddRange(Deserializer.GetBuildingWithParts(buildingPartReader, new Building(currentBuildingID)));

                buildingPartReader.Close();
            }
        }

        buildingReader.Close();

        return buildingWithParts;
    }



    /// <summary>
    /// Deserializes XML section with point elements. 
    /// </summary>
    /// <param name="pointReader">XmlReader as subtree of the "bldg:Building"-tag with the reader position set to the "gml:surfaceMeber"-tag</param>
    /// <returns></returns>
    private static List<double3> DeserializePoints(XmlReader pointReader)
    {
        List<double3> surfacePoints = new List<double3>();

        /// Iterate over all points of the current surface. 
        pointReader.ReadToFollowing("gml:pos");
        do
        {
            try
            {
                //pointReader.MoveToContent();
                string pointCoordinates = pointReader.ReadElementContentAsString();
                double3 parsedPoint = Deserializer.ParseCoordinates(pointCoordinates);

                if (!surfacePoints.Exists(existingPoint => (existingPoint.x == parsedPoint.x && existingPoint.y == parsedPoint.y && existingPoint.z == parsedPoint.z)))
                {
                    surfacePoints.Add(parsedPoint);
                }
            }
            catch (ArgumentException e)
            {
                Debug.LogError(e);
            }

        } while (pointReader.ReadToNextSibling("gml:pos"));

        pointReader.Close();

        return surfacePoints;
    }




    /// <summary>
    /// Returns a list of all buildings or NULL in case of no buildings cloud be serialised. 
    /// </summary>
    /// <param name="cityGML">CityGML-File with Buildings</param>
    public static (List<Building>, BoundingBox) InitializeBuildings(string cityGMLFileName, StringReader cityGMLFileStream)
    {
        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": " + cityGMLFileName + " deserialization started.");
        List<Building> initializedBuildings = new List<Building>();
        BoundingBox cityGMLBoundingBox = null;

        //Debug.Log("CityGML successfully imported from Resource folder: " + cityGMLFile.name);

        /// Create XMLReader Object to parse the CityGML-File
        using (XmlReader reader = XmlReader.Create(cityGMLFileStream))
        {
            /// Read BoudningBox of the CityGML-file
            if (reader.ReadToFollowing("gml:Envelope"))
            {
                XmlReader boundingBoxReader = reader.ReadSubtree();

                try
                {
                    if (!boundingBoxReader.ReadToFollowing("gml:lowerCorner"))
                    {
                        throw new ArgumentException("Tag <gml:lowerCorner> could not be found.");
                    }

                    string lowerCornerCoordinateString = boundingBoxReader.ReadElementContentAsString();
                    double3 lowerCornerPoint = Deserializer.ParseCoordinates(lowerCornerCoordinateString);

                    if (!boundingBoxReader.ReadToFollowing("gml:upperCorner"))
                    {
                        throw new ArgumentException("Tag <gml:upperCorner> could not be found.");
                    }

                    string upperCornerCoordinateString = boundingBoxReader.ReadElementContentAsString();
                    double3 upperCornerPoint = Deserializer.ParseCoordinates(upperCornerCoordinateString);

                    cityGMLBoundingBox = new BoundingBox(lowerCornerPoint, upperCornerPoint);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException("File does not contain a boundig box:\n" + e);
                }

                boundingBoxReader.Close();
            }


            /// Read up to the first building. Cancel if no building is found in the CityGML file.
            if (!reader.ReadToFollowing("bldg:Building"))
            {
                throw new ArgumentException("File does not contain any buildings.");
            }

            /// Process subnodes for each building node with a new XMLReader instance using the ReadToSibling method to jump to objects with the same hierarchy level.
            do
            {
                string currentBuildingID = reader.GetAttribute("gml:id");

                /// Creating new XMLReader instance to process only the inner nodes of the current building node. 
                /// This enshures, that the BuildingPropertyReader only processes the nodes of the current building.
                XmlReader buildingReader = reader.ReadSubtree();

                initializedBuildings.AddRange(Deserializer.GetBuildingWithParts(buildingReader, new Building(currentBuildingID)));

                /// The buildingReader is destroyed at the end of the subtree / the end of the current loop 
                /// to set the reading position of reader to the last closing element of the processed building node.
                buildingReader.Close();

            } while (reader.ReadToFollowing("bldg:Building"));

            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": " + cityGMLFileName + " deserialization completed.");

            reader.Close();
            cityGMLFileStream.Close();

            return (initializedBuildings, cityGMLBoundingBox);
        }
    }


    /// <summary>
    /// Extract a 3D coordinate from a single string
    /// </summary>
    /// <param name="coordinates">3D coordinate as a string</param>
    /// <returns></returns>
    public static double3 ParseCoordinates(string coordinates)
    {
        /// Splitting the coordinate string the format "x y z" (e.g. "33311699.707 599549.332 23.705") with the space as separator
        string[] CoordValues = coordinates.Split(' ');

        /// Parsing the splited string values with the coordintes into doubles:
        if (
            double.TryParse(CoordValues[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
            double.TryParse(CoordValues[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double y) &&
            double.TryParse(CoordValues[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double z)
            )
        {
            return new double3(x, y, z);
        }
        else
        {
            throw new ArgumentException("Coordinates could not be parsed: " + coordinates);
        }
    }


    public static async Task<(List<Building>, BoundingBox)[]> GetBuildingsAsync(string[] cityGMLFileNames, StringReader[] cityGMLFileStreams)
    {
        List<Task<(List<Building>, BoundingBox)>> deserializationTasks = new List<Task<(List<Building>, BoundingBox)>>();

        foreach (StringReader cityGMLFileStream in cityGMLFileStreams)
        {
            try
            {
                deserializationTasks.Add(Task.Run(() => Deserializer.InitializeBuildings(cityGMLFileNames[Array.IndexOf(cityGMLFileStreams, cityGMLFileStream)], cityGMLFileStream)));
            }
            catch (Exception e)
            {
                Debug.LogError("CityGML " + cityGMLFileNames[Array.IndexOf(cityGMLFileStreams, cityGMLFileStream)] + " cannot be deserialized: " + e);
            }
        }

        return await Task.WhenAll(deserializationTasks);
    }



}






public class BuildingMeshFactory //: CityGMLFactory
{
    /// <summary>
    /// Returns the vertices of the ground and roof surface. Deviating from the points in the CityGML file, y corresponds to the height coordinate and the x- and z-coordinates to the horizontal positioning.
    /// </summary>
    /// <param name="building">Building with ground surface</param>
    /// <returns>Vertecies of the ground and roof surface with y as the height</returns>
    private (Vector3[], Vector3[]) GetVerticies(Building building, double3 targetRealWorldPosition)
    {
        /// Create Vector3 vertices from the GroundSurface

        Vector3[] GroundSurfaceVerticies = new Vector3[building.GetGroundSurfacePoints().Count];
        Vector3[] RoofSurfaceVerticies = new Vector3[building.GetGroundSurfacePoints().Count];

        for (int pointIndex = 0; pointIndex < building.GetGroundSurfacePoints().Count; pointIndex++)
        {
            double3 currentPoint = building.GetGroundSurfacePoints()[pointIndex];
            try
            {
                float UnityCoordinateX = (float)(currentPoint.x - targetRealWorldPosition.x);
                float UnityCoordinateZ = (float)(currentPoint.y - targetRealWorldPosition.y);
                float UnityCoordinateY = (float)(currentPoint.z - targetRealWorldPosition.z);

                GroundSurfaceVerticies[pointIndex] = new Vector3(UnityCoordinateX, UnityCoordinateY, UnityCoordinateZ);
                RoofSurfaceVerticies[pointIndex] = new Vector3(UnityCoordinateX, UnityCoordinateY + building.MeasuredHeight, UnityCoordinateZ);

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
    private Mesh GetBuildingMesh(Building building, double3 targetRealWorldPosition, GameObject gameObjectBuildingManger)
    {
        /// Assumes that the polygons are planar, but oriented arbitrarily in 3D space

        try
        {
            (Vector3[] groundSurfaceVerticies, Vector3[] roofSurfaceVerticies) = this.GetVerticies(building, targetRealWorldPosition);

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
            roofSurfaceMesh.triangles = SurfaceTriangulator.GetTriangles(roofSurfaceVerticies);
            roofSurfaceMesh.RecalculateNormals();


            /// Create Mesh of groundsurface
            Mesh groundSurfaceMesh = new Mesh();
            groundSurfaceMesh.vertices = groundSurfaceVerticies;
            groundSurfaceMesh.triangles = SurfaceTriangulator.GetTriangles(groundSurfaceVerticies);
            groundSurfaceMesh.RecalculateNormals();

            groundSurfaceMesh = SurfaceTriangulator.GetInvertedMesh(groundSurfaceMesh);

            return this.CombineMeshes(new List<Mesh>() {
                groundSurfaceMesh,
                roofSurfaceMesh,
                this.CombineMeshes(wallSurfaceMeshList, gameObjectBuildingManger)
            }, gameObjectBuildingManger);
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
            buildingMeshes.Add(this.GetBuildingMesh(building, targetRealWorldPosition, gameObjectBuildingManger));
        }

        Mesh allBuildingMeshes = new Mesh();
        allBuildingMeshes.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        allBuildingMeshes = this.CombineMeshes(buildingMeshes, gameObjectBuildingManger);
        allBuildingMeshes.Optimize();
        allBuildingMeshes.RecalculateNormals();

        return allBuildingMeshes;
    }
}