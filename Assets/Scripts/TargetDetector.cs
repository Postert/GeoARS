using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;



public static class MyTimer
{
    private static long StartTime;

    public static void Start()
    {
        StartTime = DateTime.Now.Ticks;
    }

    public static float GetSecondsSiceStart()
    {
        float milliseconds = (DateTime.Now.Ticks - StartTime) / TimeSpan.TicksPerMillisecond;
        float seconds = milliseconds / 1000;
        return seconds;
    }

    public static string GetSecondsSiceStartAsString()
    {
        string seconds = "" + MyTimer.GetSecondsSiceStart();
        string formatedSeconds = seconds.PadLeft(5, '0');
        return "TimeStamp: " + formatedSeconds;
    }


}


[RequireComponent(typeof(ARTrackedImageManager))]
public class TargetDetector : MonoBehaviour
{
    ARTrackedImageManager TrackedImageManager;


    private BuildingManager BuildingManagerLoD1;
    private BuildingManager BuildingManagerLoD2;
    private AnnotationManager AnnotationManager;
    public DatabaseService DatabaseService { get; private set; }

    public bool isInitialized { get; private set; } = false;
    private string LastDetectedTarget;


    Dictionary<string, double3> MyTargets = new Dictionary<string, double3>()
    {
        // Bib
        { "ar_marker0", new double3(33310550.604, 5995765.951, 30.500) },
        { "ar_marker1", new double3(33310555.001, 5995791.728, 31.356) },
        { "ar_marker2", new double3(33310557.696, 5995819.134, 31.356) },
        { "ar_marker3", new double3(33310550.143, 5995766.570, 30.500) },
        { "ar_marker4", new double3(33310550.143, 5995766.570, 31.356) },

        { "ar_marker5", new double3(33311031.093, 5996128.408, 23.126) },
        { "ar_marker6", new double3(33311053.576, 5996128.715, 23.126) },
        { "ar_marker7", new double3(33310555.001, 5995791.728, 31.356) },
        /*
        { "ar_marker8", new double3(33310555.001, 5995791.728, 31.356) },
        */
        // Grill
        { "ar_marker9", new double3(33310215.085, 5995811.903, 37.5f) }
    };



    async void Awake()
    {
        DatabaseService = new DatabaseService(out bool databaseAlreadyExists, out string databasePathWithName);


        TrackedImageManager = GetComponent<ARTrackedImageManager>();

        //BuildingManagerLoD1 = GameObject.Find("BuildingManagementGameObjectLoD1").GetComponent<BuildingManager>();
        //BuildingManagerLoD1.DatabaseService = DatabaseService;
        BuildingManagerLoD2 = GameObject.Find("BuildingManagementGameObjectLoD2").GetComponent<BuildingManager>();
        BuildingManagerLoD2.DatabaseService = DatabaseService;

        AnnotationManager = GameObject.Find("AnnotationManagementGameObject").GetComponent<AnnotationManager>();
        AnnotationManager.DatabaseService = DatabaseService;

        MyTimer.Start();
        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Unity initialized after " + Time.realtimeSinceStartup);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;



        //        /// If executed in Unity Editor: Initialize database
        //
        //        bool dbWasPreviouslyInitialized = false;
        //
        //        if (PlayerPrefs.HasKey("isInitialized_BuildingDatabase")) // Player was initialized at some time
        //        {
        //            bool.TryParse(PlayerPrefs.GetString("isInitialized_BuildingDatabase"), out dbWasPreviouslyInitialized);
        //        }
        //
        //        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Database was once initialized: " + dbWasPreviouslyInitialized);
        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Database already exists: " + databaseAlreadyExists + " Database path: " + databasePathWithName);


        //        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Initialization of the database required:" +
        //            ((!databaseAlreadyExists) ? "\nDatabase has not yet been created or was deleted" : (!dbWasPreviouslyInitialized ? "\nDatabase has not been initialized (completely) yet" : "  not required")));


#if UNITY_EDITOR
        /// In case that the database was not completely initialized at the last start of the app or meanwhile manually deleted from the file system, it is recreated with the given CItyGML files.
        if (/*!dbWasPreviouslyInitialized ||*/ !databaseAlreadyExists)
        {
            PlayerPrefs.SetString("isInitialized_BuildingDatabase", "false");

            (List<Building>, List<BuildingAnnotation>, List<SurfaceAnnotation>, List<WorldCoordinateAnnotation>) import = await ImportCityGMLFilesFromRessourcesAsync();

            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Starting Initialization of the database");

            List<Building> buildings = import.Item1;
            List<BuildingAnnotation> buildingAnnotations = import.Item2;
            List<SurfaceAnnotation> surfaceAnnotations = import.Item3;
            List<WorldCoordinateAnnotation> worldCoordinateAnnotations = import.Item4;


            //            foreach (Building building in buildings)
            //            {
            //                Debug.Log(building.ToString());
            //            }


            foreach (BuildingAnnotation buildingAnnotation in buildingAnnotations)
            {
                Debug.Log(buildingAnnotation.ToString());
            }

            foreach (SurfaceAnnotation surfaceAnnotation in surfaceAnnotations)
            {
                Debug.Log(surfaceAnnotation.ToString());
            }

            foreach (WorldCoordinateAnnotation worldCoordinateAnnotation in worldCoordinateAnnotations)
            {
                Debug.Log(worldCoordinateAnnotation.ToString());
            }



            try
            {
                DatabaseService.PrepareTabels();

                DatabaseService.BuildingToDatabase(buildings);
                DatabaseService.BuildingAnnotationToDatabase(buildingAnnotations);
                DatabaseService.SurfaceAnnotationToDatabase(surfaceAnnotations);
                DatabaseService.WorldCoordinateAnnotationToDatabase(worldCoordinateAnnotations);

                PlayerPrefs.SetString("isInitialized_BuildingDatabase", "true");

                Debug.Log(MyTimer.GetSecondsSiceStartAsString() + " Buildings and Annotations stored in database.");
            }
            catch (Exception e)
            {
                Debug.LogError(MyTimer.GetSecondsSiceStartAsString() + ": Initialization of the building database failed: " + e);
            }
        }

        //#else 
        //        if(!dbWasPreviouslyInitialized) throw new FileNotFoundException("Initialize the database by executing the App in the Unity Editor before compiling! Some entries meight be missing in the current database.");
        //        if(!databaseAlreadyExists) throw new FileNotFoundException("Initialize the database by executing the App in the Unity Editor before compiling! Database file is missing.");
#endif


        isInitialized = true;

        Debug.Log("isInitialized (Awake): " + isInitialized);
    }


    private async Task<(List<Building>, List<BuildingAnnotation>, List<SurfaceAnnotation>, List<WorldCoordinateAnnotation>)> ImportCityGMLFilesFromRessourcesAsync()
    {
        List<Building> buildings = new List<Building>();
        List<BuildingAnnotation> buildingAnnotations = new List<BuildingAnnotation>();
        List<SurfaceAnnotation> surfaceAnnotations = new List<SurfaceAnnotation>();
        List<WorldCoordinateAnnotation> worldCoordinateAnnotations = new List<WorldCoordinateAnnotation>();


        TextAsset[] cityGMLFiles = Resources.LoadAll<TextAsset>("CityGML");
        BoundingBox[] boundingBoxes = new BoundingBox[cityGMLFiles.Length];

        StringReader[] stringReaders = new StringReader[cityGMLFiles.Length];
        string[] cityGMLFileNames = new string[cityGMLFiles.Length];

        foreach (TextAsset cityGMLFile in cityGMLFiles)
        {
            int i = Array.IndexOf(cityGMLFiles, cityGMLFile);
            stringReaders[i] = new StringReader(cityGMLFiles[i].text);
            cityGMLFileNames[i] = cityGMLFiles[i].name;
        }

        (List<Building>, List<BuildingAnnotation>, List<SurfaceAnnotation>, List<WorldCoordinateAnnotation>, BoundingBox)[] buildingListTupels
            = await Deserializer.GetBuildingsAndAnnotationsAsync(cityGMLFileNames, stringReaders);


        foreach ((List<Building> newBuildings, List<BuildingAnnotation> newBuildingAnnotations, List<SurfaceAnnotation> newSurfaceAnnotations, List<WorldCoordinateAnnotation> newWorldCoordinateAnnotations, BoundingBox boundingBox) in buildingListTupels)
        {
            buildings.AddRange(newBuildings);
            buildingAnnotations.AddRange(newBuildingAnnotations);
            surfaceAnnotations.AddRange(newSurfaceAnnotations);
            worldCoordinateAnnotations.AddRange(newWorldCoordinateAnnotations);
            //Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": " + cityGMLFiles[Array.IndexOf(buildingListTupels, (newBuildings, boundingBox))].name + " deserialised:\n" + boundingBox.ToString());
        }

        return (buildings, buildingAnnotations, surfaceAnnotations, worldCoordinateAnnotations);
    }










    private void Start()
    {

#if UNITY_EDITOR
        if (isInitialized)
        {
            try
            {
                // TODO: LOD1 Test

                Dictionary<string, Building> buildingsWithinBoundingBox;
                //buildingsWithinBoundingBox = BuildingManagerLoD1.CreateGameObjectsAroundTarget(MyTargets["ar_marker0"]);
                buildingsWithinBoundingBox = BuildingManagerLoD2.CreateGameObjectsAroundTarget(MyTargets["ar_marker6"]);
                AnnotationManager.CreateGameObjectsAroundTarget(MyTargets["ar_marker6"], buildingsWithinBoundingBox);
            }
            catch (InvalidOperationException e)
            {
                Debug.LogError(MyTimer.GetSecondsSiceStartAsString() + "Target detection simulation not available: " + e);
            }
        }
        else
        {
            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Database has not initialized yet. No querying possible.");
        }
#endif

    }

    void OnEnable()
    {
        TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        Debug.Log("isInitialized (OnTrackedImagesChanged): " + isInitialized);

        //        if (isInitialized)
        //        {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": new Target detected");
            RepositionCityGMLObjects(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            RepositionCityGMLObjects(trackedImage);
        }
        //        }
        //        else
        //        {
        //            throw new InvalidOperationException("Database not initialized");
        //        }
    }



    /// <summary>
    /// https://stackoverflow.com/questions/57037040/unity-arfoundation-image-tracking-prefab-location
    /// </summary>
    /// <param name="trackedImage"></param>
    private void RepositionCityGMLObjects(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            string trackedTargetName = trackedImage.referenceImage.name;

            if (!MyTargets.ContainsKey(trackedTargetName))
            {
                throw new KeyNotFoundException("Target dictionary does not contain the target " + trackedImage.referenceImage.name);
            }
            else
            {
                if (!trackedTargetName.Equals(LastDetectedTarget))
                {
                    float3 targetUnityCoordinates = new float3(trackedImage.transform.position.x, trackedImage.transform.position.y, 0);

                    try
                    {
                        Dictionary<string, Building> buildingsWithinBoundingBox;
                        //buildingsWithinBoundingBox = BuildingManagerLoD1.CreateGameObjectsAroundTarget(MyTargets["ar_marker0"]);
                        buildingsWithinBoundingBox = BuildingManagerLoD2.CreateGameObjectsAroundTarget(MyTargets[trackedTargetName]);
                        AnnotationManager.CreateGameObjectsAroundTarget(MyTargets[trackedTargetName], buildingsWithinBoundingBox);
                    }
                    catch (InvalidOperationException e)
                    {
                        Debug.LogError("Could not query database:\n" + e);
                    }
                    catch (ArgumentException e)
                    {
                        Debug.LogError("Building Mesh cannot be created:\n" + e);
                    }
                    LastDetectedTarget = trackedTargetName;
                }
            }

            BuildingManagerLoD2.UpdateMeshPosition(trackedImage.transform.position, trackedImage.transform.rotation);
            AnnotationManager.UpdateAnnotationAnchor(trackedImage.transform.position, trackedImage.transform.rotation);
        }

    }
}