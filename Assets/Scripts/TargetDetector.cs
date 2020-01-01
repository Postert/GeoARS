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
        string formatedSeconds = seconds.PadLeft(5,'0');
        return "TimeStamp: " + formatedSeconds;
    }


}


[RequireComponent(typeof(ARTrackedImageManager))]
public class TargetDetector : MonoBehaviour
{
    ARTrackedImageManager TrackedImageManager;


    private BuildingManager _BuildingManager;
    private AnnotationManager _AnnotationManager;
    public DatabaseService _DatabaseService { get; private set; }

    public bool isInitialized { get; private set; } = false;
    private string LastDetectedTarget;


    Dictionary<string, double3> MyTargets = new Dictionary<string, double3>()
    {
        { "Target1", new double3(33310555.001, 5995791.728, 31.356) },
        { "Target2", new double3(33310215.085, 5995811.903, 37.5f) }
    };



    async void Awake()
    {
        TrackedImageManager = GetComponent<ARTrackedImageManager>();
        _BuildingManager = GameObject.Find("BuildingManagementGameObject").GetComponent<BuildingManager>();
        _AnnotationManager = GameObject.Find("AnnotationManagementGameObject").GetComponent<AnnotationManager>();

        MyTimer.Start();
        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Unity initialized after " + Time.realtimeSinceStartup);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;


        _DatabaseService = new DatabaseService(out bool databaseAlreadyExists, out string databasePathWithName);

        bool dbWasPreviouslyInitialized = false;

        if (PlayerPrefs.HasKey("isInitialized_BuildingDatabase")) // Player was initialized at some time
        {
            bool.TryParse(PlayerPrefs.GetString("isInitialized_BuildingDatabase"), out dbWasPreviouslyInitialized);
        }

        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Database was once initialized: " + dbWasPreviouslyInitialized);
        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Database already exists " + databaseAlreadyExists + "(" + databasePathWithName + ")");


        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Initialization of the database required:" +
            ((!databaseAlreadyExists) ? "\nDatabase has not yet been created or was deleted" : (!dbWasPreviouslyInitialized ? "\nDatabase has not been initialized (completely) yet" : "  not required")));


        /// In case that the database was not completely initialized at the last start of the app or meanwhile manually deleted from the file system, it is recreated with the given CItyGML files.
        if (!dbWasPreviouslyInitialized || !databaseAlreadyExists)
        {
            PlayerPrefs.SetString("isInitialized_BuildingDatabase", "false");

            (List<Building>, List<Annotation>) import = await ImportCityGMLFilesFromRessourcesAsync();

            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Starting Initialization of the database");

            List<Building> buildings = import.Item1;
            List<Annotation> annotations = import.Item2;

            try
            {
                _DatabaseService.PrepareTabels();
                _DatabaseService.BuidlingToDatabase(buildings);
                PlayerPrefs.SetString("isInitialized_BuildingDatabase", "true");


                // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------
                BoundingBox BuildingAnnotationBoundingBox;
                BuildingAnnotation testBuildingAnnotation;
                double3[] groundSurfacePoints;
                SurfaceAnnotation testSurfaceAnnotation;

                // JvL-Weg

                // JvL8: DEMV__44454d56-5f52-3030-5956-ff414b000000 - 11224 - 10856
                BuildingAnnotationBoundingBox = new BoundingBox(new double3(33310071.7, 5995771.481, 0), new double3(33310123.54, 5995811.573, 0));
                testBuildingAnnotation = new BuildingAnnotation(11224, BuildingAnnotationBoundingBox, 18.561f, 1, "↓ J.-v.-L Weg 8", 1);
                _DatabaseService.BuildingAnnotationToDatabase(testBuildingAnnotation);

                // JvL6: DEMV_0000f342-0000-2000-d264-000001436c59 - 10723 - 10131
                BuildingAnnotationBoundingBox = new BoundingBox(new double3(33310166.623, 5995765.097, 0), new double3(33310203.593, 5995808.594, 0));
                testBuildingAnnotation = new BuildingAnnotation(10723, BuildingAnnotationBoundingBox, 18.561f, 1, "↓ J.-v.-L Weg 6", 1);
                _DatabaseService.BuildingAnnotationToDatabase(testBuildingAnnotation);

                // Abfallwirtschaft: DEMV__44454d56-5f52-3032-3446-ff3855000000 - 11294 - 10955
                BuildingAnnotationBoundingBox = new BoundingBox(new double3(33310215.899, 5995757.384, 0), new double3(33310259.593, 5995791.41, 0));
                testBuildingAnnotation = new BuildingAnnotation(11294, BuildingAnnotationBoundingBox, 18.561f, 1, "↓ Abfallwirtschaft", 1);
                _DatabaseService.BuildingAnnotationToDatabase(testBuildingAnnotation);


                groundSurfacePoints = new double3[] { new double3(33310223.182, 5995827.444, 36.555), new double3(33310191.698, 5995807.732, 36.555) };
                testSurfaceAnnotation = new SurfaceAnnotation(8919, groundSurfacePoints, 0.5, 1.5f, 1, "Seminarräume SR1 und SR2", 0.28f);
                _DatabaseService.SurfaceAnnotationToDatabase(testSurfaceAnnotation);

                FreeWorldAnnotation testFreeWorldAnnotation = new FreeWorldAnnotation(new double3(33310215.085, 5995811.903, 38f), 0, "Grill", 1);
                _DatabaseService.FreeWorldAnnotationToDatabase(testFreeWorldAnnotation);



                // Bib

                // Bib: DEMV__44454d56-5f52-3032-3437-ff5345000000 - 5918 - 5429
                BuildingAnnotationBoundingBox = new BoundingBox(new double3(33310481.617, 5995752.509, 0), new double3(33310550.143, 5995851.811, 0));
                testBuildingAnnotation = new BuildingAnnotation(5918, BuildingAnnotationBoundingBox, 18.561f, 1, "↓ Bib", 1);
                _DatabaseService.BuildingAnnotationToDatabase(testBuildingAnnotation);

                groundSurfacePoints = new double3[] { new double3(33310545.79, 5995834.377, 31.356), new double3(33310516.312, 5995809.547, 31.356) };
                testSurfaceAnnotation = new SurfaceAnnotation(00003, groundSurfacePoints, 0.5, 16f, 1, "Ingenieurwissenschaften", 0.28f);
                _DatabaseService.SurfaceAnnotationToDatabase(testSurfaceAnnotation);

                groundSurfacePoints = new double3[] { new double3(33310545.79, 5995834.377, 31.356), new double3(33310516.312, 5995809.547, 31.356) };
                testSurfaceAnnotation = new SurfaceAnnotation(00003, groundSurfacePoints, 0.5, 11f, 2, "Politikwissenschaften", 0.28f);
                _DatabaseService.SurfaceAnnotationToDatabase(testSurfaceAnnotation);



                // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------

                Debug.Log(MyTimer.GetSecondsSiceStartAsString() + " Buildings and Annotations stored in database.");
            }
            catch (Exception e)
            {
                Debug.LogError(MyTimer.GetSecondsSiceStartAsString() + ": Initialization of the building database failed: " + e);
            }
        }

        isInitialized = true;
    }


    private async Task<(List<Building>, List<Annotation>)> ImportCityGMLFilesFromRessourcesAsync()
    {
        List<Building> buildings = new List<Building>();

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

        (List<Building> buildingList, BoundingBox boundingBox)[] buildinglistTupels = await Deserializer.GetBuildingsAsync(cityGMLFileNames, stringReaders);


        foreach ((List<Building> buildingList, BoundingBox boundingBox) in buildinglistTupels)
        {
            buildings.AddRange(buildingList);
            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": " + cityGMLFiles[Array.IndexOf(buildinglistTupels, (buildingList, boundingBox))].name + " deserialised:\n" + boundingBox.ToString());
        }

        return (buildings, null);
    }





    private void Start()
    {

#if UNITY_EDITOR
        if (isInitialized)
        {
            try
            {
                _BuildingManager.CreateGameObjectsAroundTarget(MyTargets["Grill"]);
                _AnnotationManager.CreateGameObjectsAroundTarget(MyTargets["Grill"]);
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
        if (isInitialized)
        {
            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": new Target detected");
                RepositionCityGMLObjects(trackedImage);
            }

            foreach (ARTrackedImage trackedImage in eventArgs.updated)
            {
                RepositionCityGMLObjects(trackedImage);
            }
        }
        else
        {
            throw new InvalidOperationException("Database not initialized");
        }
    }



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
                        _BuildingManager.CreateGameObjectsAroundTarget(MyTargets[trackedTargetName]);
                        _AnnotationManager.CreateGameObjectsAroundTarget(MyTargets[trackedTargetName]);
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

            _BuildingManager.UpdateMeshPosition(trackedImage.transform.position, trackedImage.transform.rotation);
            _AnnotationManager.UpdateAnnotationAnchor(trackedImage.transform.position, trackedImage.transform.rotation);
        }

    }
}