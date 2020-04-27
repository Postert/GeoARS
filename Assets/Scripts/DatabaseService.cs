using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Unity.Mathematics;
using UnityEngine;



public enum DBBoolean
{
    DBFalse,
    DBTrue
}

/// <summary>
/// https://medium.com/@rizasif92/sqlite-and-unity-how-to-do-it-right-31991712190
/// </summary>
public class DatabaseService
{

    private struct TableNames
    {
        public const string Building = "Building";

        public const string Building_CityGMLID = "BuildingCityGMLID";
        public const string Building_MeasuredHeight = "MeasuredHeight";
        public const string Building_BoundingBox_UpperRightCorner_X = "BoundingBox_UpperRightCorner_X";
        public const string Building_BoundingBox_UpperRightCorner_Y = "BoundingBox_UpperRightCorner_Y";
        public const string Building_BoundingBox_UpperRightCorner_Z = "BoundingBox_UpperRightCorner_Z";
        public const string Building_BoundingBox_LowerLeftCorner_X = "BoundingBox_LowerLeftCorner_X";
        public const string Building_BoundingBox_LowerLeftCorner_Y = "BoundingBox_LowerLeftCorner_Y";
        public const string Building_BoundingBox_LowerLeftCorner_Z = "BoundingBox_LowerLeftCorner_Z";


        public const string Surface = "Surface";
        public const string Surface_CityGMLID = "SurfaceID";
        public const string Surface_Type = "Type";


        public const string SurfacePoint = "SurfacePoint";
        public const string Surface_Point_X = "X";
        public const string Surface_Point_Y = "Y";
        public const string Surface_Point_Z = "Z";


        public const string Annotation = "Annotation";
        public const string Annotation_ID = "AnnotationID";
        public const string Annotation_Text = "AnnotationText";
        public const string Annotation_LocalScale = "AnnotationLocalScale";

        public const string BuildingAnnotation = "BuildingAnnotation";

        public const string SurfaceAnnotation = "SurfaceAnnotation";
        public const string SurfaceAnnotation_RelativeGroundSurfacePosition = "RelativeGroundSurfacePosition";
        public const string SurfaceAnnotation_Height = "Height";

        public const string WorldCoordinateAnnotation = "WorldCoordinateAnnotation";
    }























    //private string DatabasePath = Application.persistentDataPath + "/"; // Application.streamingAssetsPath vs Application.persistentDataPath
    private string DatabasePath = Application.streamingAssetsPath + "/"; // Application.streamingAssetsPath vs Application.persistentDataPath
    private const string DatabaseName = "Database.db";


    private IDbConnection dbConnection;


    /// <summary>
    /// 
    /// </summary>
    public DatabaseService(out bool databaseAlreadyExists, out string databasePathWithName)
    {
        databasePathWithName = DatabasePath + DatabaseName;

        databaseAlreadyExists = File.Exists(DatabasePath + DatabaseName);
        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": Exists" + databaseAlreadyExists);
        dbConnection = new SqliteConnection("URI=file:" + DatabasePath + DatabaseName);
        dbConnection.Open();

        //this.ConfigureDatabaseForPerformance();

























    }



    ~DatabaseService()
    {
        if (dbConnection != null)
        {
            dbConnection.Close();
        }
    }


    private void ConfigureDatabaseForPerformance()
    {
        IDbCommand dbCommand;

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText =
            "PRAGMA locking_mode = EXCLUSIVE;" +
            "PRAGMA synchronous = OFF;" +
            "PRAGMA journal_mode = OFF;";
        dbCommand.ExecuteReader();
    }


    public void PrepareTabels()
    {
        IDbCommand dbCommand;

        // Buildings ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText =
        Environment.NewLine + "DROP TABLE IF EXISTS Building;";

        dbCommand.ExecuteReader();


        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText =
        Environment.NewLine + "CREATE TABLE Building(" +
        Environment.NewLine + "[BuildingCityGMLID] VARCHAR(42) NOT NULL UNIQUE," +
        Environment.NewLine + "[MeasuredHeight] FLOAT NOT NULL," +
        Environment.NewLine + "[BoundingBox_LowerLeftCorner_X] DOUBLE NOT NULL," +
        Environment.NewLine + "[BoundingBox_LowerLeftCorner_Y] DOUBLE NOT NULL," +
        Environment.NewLine + "[BoundingBox_LowerLeftCorner_Z] DOUBLE NOT NULL," +
        Environment.NewLine + "[BoundingBox_UpperRightCorner_X] DOUBLE NOT NULL," +
        Environment.NewLine + "[BoundingBox_UpperRightCorner_Y] DOUBLE NOT NULL," +
        Environment.NewLine + "[BoundingBox_UpperRightCorner_Z] DOUBLE NOT NULL," +
        Environment.NewLine + "PRIMARY KEY(BuildingCityGMLID)" +
        Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // Surfaces ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText =
        Environment.NewLine + "DROP TABLE IF EXISTS Surface;";

        dbCommand.ExecuteReader();


        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText =
        Environment.NewLine + "CREATE TABLE Surface(" +
        Environment.NewLine + "[BuildingCityGMLID] VARCHAR(42) NOT NULL," +
        Environment.NewLine + "[SurfaceID] VARCHAR(47) NOT NULL UNIQUE," +
        Environment.NewLine + "[Type] VARCHAR," +
        Environment.NewLine + "PRIMARY KEY(SurfaceID)" +
        Environment.NewLine + "FOREIGN KEY(BuildingCityGMLID) REFERENCES Building(BuildingCityGMLID)" +
        Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // SurfacePoints ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText =
        Environment.NewLine + "DROP TABLE IF EXISTS SurfacePoint;";

        dbCommand.ExecuteReader();


        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText =
        Environment.NewLine + "CREATE TABLE SurfacePoint(" +
        Environment.NewLine + "[SurfaceID] VARCHAR(47) NOT NULL," +
        Environment.NewLine + "[X] DOUBLE NOT NULL," +
        Environment.NewLine + "[Y] DOUBLE NOT NULL," +
        Environment.NewLine + "[Z] DOUBLE NOT NULL," +
        Environment.NewLine + "PRIMARY KEY(SurfaceID, X, Y, Z)," +
        Environment.NewLine + "FOREIGN KEY(SurfaceID) REFERENCES Surface(SurfaceID)" +
        Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // Annotations ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText =
        Environment.NewLine + "DROP TABLE IF EXISTS Annotation;";

        dbCommand.ExecuteReader();


        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText =
        Environment.NewLine + "CREATE TABLE Annotation(" +
        Environment.NewLine + "[AnnotationID] INTEGER PRIMARY KEY AUTOINCREMENT," +
        Environment.NewLine + "[ScaleWithCameraDistance] INTEGER NOT NULL," +
        Environment.NewLine + "[ScaleBySelection] INTEGER NOT NULL," +
        Environment.NewLine + "[PointingDirectionX] DOUBLE NOT NULL," +
        Environment.NewLine + "[PointingDirectionY] DOUBLE NOT NULL," +
        Environment.NewLine + "[PointingDirectionZ] DOUBLE NOT NULL " +
        Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // TextAnnotationComponents ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText =
        Environment.NewLine + "DROP TABLE IF EXISTS TextAnnotationComponent;";

        dbCommand.ExecuteReader();


        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText =
        Environment.NewLine + "CREATE TABLE TextAnnotationComponent(" +
        Environment.NewLine + "[TextID] INTEGER PRIMARY KEY AUTOINCREMENT," +
        Environment.NewLine + "[Text] VARCHAR NOT NULL," +
        Environment.NewLine + "[TextSize] DOUBLE NOT NULL" +
        Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // SimpleTextAnnotation ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText =
        Environment.NewLine + "DROP TABLE IF EXISTS SimpleTextAnnotation;";

        dbCommand.ExecuteReader();


        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText =
        Environment.NewLine + "CREATE TABLE SimpleTextAnnotation(" +
        Environment.NewLine + "[AnnotationID] INTEGER NOT NULL UNIQUE," +
        Environment.NewLine + "[TextID] INTEGER NOT NULL UNIQUE," +
        Environment.NewLine + "PRIMARY KEY(AnnotationID, TextID)," +
        Environment.NewLine + "FOREIGN KEY(AnnotationID) REFERENCES Annotation(AnnotationID)" +
        Environment.NewLine + "FOREIGN KEY(TextID) REFERENCES TextAnnotationComponent(TextID)" +
        Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // BuildingAnnotation ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText =
        Environment.NewLine + "DROP TABLE IF EXISTS BuildingAnnotation;";

        dbCommand.ExecuteReader();


        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText =
        Environment.NewLine + "CREATE TABLE BuildingAnnotation(" +
        Environment.NewLine + "[BuildingCityGMLID] VARCHAR(42) NOT NULL UNIQUE," +
        Environment.NewLine + "[AnnotationID] INTEGER NOT NULL UNIQUE," +
        Environment.NewLine + "PRIMARY KEY(BuildingCityGMLID, AnnotationID)," +
        Environment.NewLine + "FOREIGN KEY(BuildingCityGMLID) REFERENCES Building(BuildingCityGMLID)" +
        Environment.NewLine + "FOREIGN KEY(AnnotationID) REFERENCES Annotation(AnnotationID)" +
        Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // SurfaceAnnotation ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText =
        Environment.NewLine + "DROP TABLE IF EXISTS SurfaceAnnotation;";

        dbCommand.ExecuteReader();


        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText =
        Environment.NewLine + "CREATE TABLE SurfaceAnnotation(" +
        Environment.NewLine + "[SurfaceID] VARCHAR(47) NOT NULL," +
        Environment.NewLine + "[AnchorPointIndex] INTEGER NOT NULL," +
        Environment.NewLine + "[HeightAboveBaseline] DOUBLE NOT NULL," +
        Environment.NewLine + "[RelativePositionBetweenBaselinePoints] DOUBLE NOT NULL," +
        Environment.NewLine + "[AnnotationID] INTEGER NOT NULL UNIQUE," +
        Environment.NewLine + "PRIMARY KEY(SurfaceID, AnnotationID)," +
        Environment.NewLine + "FOREIGN KEY(SurfaceID) REFERENCES Surface(SurfaceID)" +
        Environment.NewLine + "FOREIGN KEY(AnnotationID) REFERENCES Annotation(AnnotationID)" +
        Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // WorldCoordinateAnnotation ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText =
        Environment.NewLine + "DROP TABLE IF EXISTS WorldCoordinateAnnotation;";

        dbCommand.ExecuteReader();


        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText =
        Environment.NewLine + "CREATE TABLE WorldCoordinateAnnotation(" +
        Environment.NewLine + "[X] DOUBLE NOT NULL," +
        Environment.NewLine + "[Y] DOUBLE NOT NULL," +
        Environment.NewLine + "[Z] DOUBLE NOT NULL," +
        Environment.NewLine + "[AnnotationID] INTEGER NOT NULL UNIQUE," +
        Environment.NewLine + "PRIMARY KEY(X, Y, Z, AnnotationID)," +
        Environment.NewLine + "FOREIGN KEY(AnnotationID) REFERENCES Annotation(AnnotationID)" +
        Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();
    }


    public void PrepareTabels_OLD()
    {
        IDbCommand dbCommand;

        // Buildings ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "DROP TABLE IF EXISTS " + TableNames.Building;
        dbCommand.ExecuteReader();

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE " + TableNames.Building +
            "(" +
            "[" + TableNames.Building_CityGMLID + "] " +
            "VARCHAR(42) NOT NULL UNIQUE," +
            "[" + TableNames.Building_MeasuredHeight + "] " +
            "FLOAT NOT NULL," +
            "[" + TableNames.Building_BoundingBox_LowerLeftCorner_X + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.Building_BoundingBox_LowerLeftCorner_Y + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.Building_BoundingBox_LowerLeftCorner_Z + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.Building_BoundingBox_UpperRightCorner_X + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.Building_BoundingBox_UpperRightCorner_Y + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.Building_BoundingBox_UpperRightCorner_Z + "] " +
            "DOUBLE NOT NULL," +
            "PRIMARY KEY(" + TableNames.Building_CityGMLID + ")" +
            ")";

        Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // Surface ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "DROP TABLE IF EXISTS " + TableNames.Surface;
        dbCommand.ExecuteReader();

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE " + TableNames.Surface
            + "("
            + "[" + TableNames.Building_CityGMLID + "] "
            + "VARCHAR(42) NOT NULL,"
            + "[" + TableNames.Surface_CityGMLID + "] "
            + "VARCHAR(47) NOT NULL UNIQUE,"
            + "[" + TableNames.Surface_Type + "] "
            + "VARCHAR,"
            + "PRIMARY KEY(" + TableNames.Surface_CityGMLID + ")"
            + "FOREIGN KEY(" + TableNames.Building_CityGMLID + ") REFERENCES " + TableNames.Building + "(" + TableNames.Building_CityGMLID + ")"
            + ")";

        Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();



        // SurfacePoint ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "DROP TABLE IF EXISTS " + TableNames.SurfacePoint;
        dbCommand.ExecuteReader();

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE " + TableNames.SurfacePoint + "("
            + "[" + TableNames.Surface_CityGMLID + "] "
            + "VARCHAR(47) NOT NULL,"
            + "[" + TableNames.Surface_Point_X + "] "
            + "DOUBLE NOT NULL,"
            + "[" + TableNames.Surface_Point_Y + "] "
            + "DOUBLE NOT NULL,"
            + "[" + TableNames.Surface_Point_Z + "] "
            + "DOUBLE NOT NULL,"
            + "PRIMARY KEY(" + TableNames.Surface_CityGMLID + ", " + TableNames.Surface_Point_X + ", " + TableNames.Surface_Point_Y + ", " + TableNames.Surface_Point_Z
            + "),"
            + "FOREIGN KEY(" + TableNames.Surface_CityGMLID + ") REFERENCES " + TableNames.Surface + "(" + TableNames.Surface_CityGMLID + ")"
            + ")";

        Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // BuildingAnnotations ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "DROP TABLE IF EXISTS " + TableNames.BuildingAnnotation;
        dbCommand.ExecuteReader();

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE " + TableNames.BuildingAnnotation + "(" +
            "[" + TableNames.Building_CityGMLID + "] " +
            "VARCHAR," +
            "[" + "] " +
            "INTEGER," +
            "[" + TableNames.Annotation_Text + "] " +
            "TEXT NOT NULL," +
            "[" + TableNames.Annotation_LocalScale + "] " +
            "FLOAT NOT NULL," +
            "PRIMARY KEY(" + TableNames.Building_CityGMLID + ", " + ")," +
            "FOREIGN KEY(" + TableNames.Building_CityGMLID + ") REFERENCES " + TableNames.Building + "(" + TableNames.Building_CityGMLID + ")" +
            ")";

        Debug.Log(dbCommand.CommandText);

        //dbCommand.ExecuteNonQuery();


        // SurfaceAnnotations ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "DROP TABLE IF EXISTS " + TableNames.SurfaceAnnotation;
        dbCommand.ExecuteReader();

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE " + TableNames.SurfaceAnnotation + "(" +
            "[" + TableNames.Building_CityGMLID + "] " +
            "VARCHAR(47)," +
            "[" + TableNames.Surface_Point_X + "1] " +
            "DOUBLE," +
            "[" + TableNames.Surface_Point_Y + "1] " +
            "DOUBLE," +
            "[" + TableNames.Surface_Point_Z + "1] " +
            "DOUBLE," +
            "[" + TableNames.Surface_Point_X + "2] " +
            "DOUBLE," +
            "[" + TableNames.Surface_Point_Y + "2] " +
            "DOUBLE," +
            "[" + TableNames.Surface_Point_Z + "2] " +
            "DOUBLE," +
            "[" + "] " +
            "INTEGER," +
            "[" + TableNames.SurfaceAnnotation_RelativeGroundSurfacePosition + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.SurfaceAnnotation_Height + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.Annotation_Text + "] " +
            "TEXT NOT NULL, " +
            "[" + TableNames.Annotation_LocalScale + "] " +
            "FLOAT NOT NULL," +
            "PRIMARY KEY(" + TableNames.Building_CityGMLID + ", " + TableNames.Surface_Point_X + "1, " + TableNames.Surface_Point_Y + "1, " + TableNames.Surface_Point_Z + "1, " + TableNames.Surface_Point_X + "2, " + TableNames.Surface_Point_Y + "2, " + TableNames.Surface_Point_Z + "2, " + "), " +
            "FOREIGN KEY(" + TableNames.Building_CityGMLID + ") REFERENCES " + TableNames.Surface + "(" + TableNames.Building_CityGMLID + "), " +
            "FOREIGN KEY(" + TableNames.Surface_Point_X + "1) REFERENCES " + TableNames.Surface + "(" + TableNames.Surface_Point_X + "), " +
            "FOREIGN KEY(" + TableNames.Surface_Point_X + "2) REFERENCES " + TableNames.Surface + "(" + TableNames.Surface_Point_X + "), " +
            "FOREIGN KEY(" + TableNames.Surface_Point_Y + "1) REFERENCES " + TableNames.Surface + "(" + TableNames.Surface_Point_Y + "), " +
            "FOREIGN KEY(" + TableNames.Surface_Point_Y + "2) REFERENCES " + TableNames.Surface + "(" + TableNames.Surface_Point_Y + "), " +
            "FOREIGN KEY(" + TableNames.Surface_Point_Z + "1) REFERENCES " + TableNames.Surface + "(" + TableNames.Surface_Point_Z + "), " +
            "FOREIGN KEY(" + TableNames.Surface_Point_Z + "2) REFERENCES " + TableNames.Surface + "(" + TableNames.Surface_Point_Z + "), " +
            "CHECK(" + TableNames.SurfaceAnnotation_RelativeGroundSurfacePosition + " >= 0 " + "), " +
            "CHECK(" + TableNames.SurfaceAnnotation_RelativeGroundSurfacePosition + " <= 1 " + ")" +
            ")";

        Debug.Log(dbCommand.CommandText);

        //        dbCommand.ExecuteNonQuery();


        // WorldCoordinateAnnotations ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "DROP TABLE IF EXISTS " + TableNames.WorldCoordinateAnnotation;
        dbCommand.ExecuteReader();

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE " + TableNames.WorldCoordinateAnnotation + "(" +
            "[" + TableNames.Surface_Point_X + "] " +
            "DOUBLE," +
            "[" + TableNames.Surface_Point_Y + "] " +
            "DOUBLE," +
            "[" + TableNames.Surface_Point_Z + "] " +
            "DOUBLE," +
            "[" + "] " +
            "INTEGER," +
            "[" + TableNames.Annotation_Text + "] " +
            "TEXT NOT NULL," +
            "[" + TableNames.Annotation_LocalScale + "] " +
            "FLOAT NOT NULL," +
            "PRIMARY KEY(" + TableNames.Surface_Point_X + ", " + TableNames.Surface_Point_Y + ", " + TableNames.Surface_Point_Z + ", " + ")" +
            ")";

        Debug.Log(dbCommand.CommandText);

        //        dbCommand.ExecuteNonQuery();

    }


    private int GetLastInsertedRowID()
    {
        int autoincrementedID;

        using (IDbCommand autoincrementedIDQuery = dbConnection.CreateCommand())
        {
            IDataReader dataReader;

            autoincrementedIDQuery.CommandText = "SELECT last_insert_rowid()";

            dataReader = autoincrementedIDQuery.ExecuteReader();

            if (dataReader.Read())
            {
                int.TryParse(dataReader[0].ToString(), out autoincrementedID);
            }
            else { throw new DataException("There have not been any data insertion operations lately."); }

        }

        return autoincrementedID;
    }


    public void BuildingToDatabase(List<Building> buildings)
    {
        Execute("BEGIN TRANSACTION");

        foreach (Building building in buildings)
        {
            BuildingToDatabase(building);
        }

        Execute("COMMIT TRANSACTION");
    }

    private void Execute(string query)
    {
        IDbCommand dbCommand;

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;
        dbCommand.ExecuteNonQuery();
    }

    public void BuildingToDatabase(Building building)
    {
        try
        {
            // TODO: IsInitialized überprüfen, scheint flasch zu sein!!
            if (/*building.MeasuredHeight > 0 &&*/ /*building.BoundingBox.IsInitialized()*/true)
            {
                IDbCommand dbCommand;

                dbCommand = dbConnection.CreateCommand();

                dbCommand.CommandText = ""
                    + Environment.NewLine + "INSERT INTO Building("
                    + Environment.NewLine + "BuildingCityGMLID,"
                    + Environment.NewLine + "MeasuredHeight,"
                    + Environment.NewLine + "BoundingBox_LowerLeftCorner_X,"
                    + Environment.NewLine + "BoundingBox_LowerLeftCorner_Y,"
                    + Environment.NewLine + "BoundingBox_LowerLeftCorner_Z,"
                    + Environment.NewLine + "BoundingBox_UpperRightCorner_X,"
                    + Environment.NewLine + "BoundingBox_UpperRightCorner_Y,"
                    + Environment.NewLine + "BoundingBox_UpperRightCorner_Z "
                    + Environment.NewLine + ")"
                    + Environment.NewLine + "VALUES("
                    + Environment.NewLine + "\"" + building.CityGMLID + "\", "
                    + Environment.NewLine + building.MeasuredHeight.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                    + Environment.NewLine + building.BoundingBox.ButtomLowerLeftCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                    + Environment.NewLine + building.BoundingBox.ButtomLowerLeftCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                    + Environment.NewLine + building.BoundingBox.ButtomLowerLeftCorner.Value.z.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                    + Environment.NewLine + building.BoundingBox.TopUpperRightCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                    + Environment.NewLine + building.BoundingBox.TopUpperRightCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                    + Environment.NewLine + building.BoundingBox.TopUpperRightCorner.Value.z.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ""
                    + Environment.NewLine + ");";

                //Debug.Log(dbCommand.CommandText);
                dbCommand.ExecuteNonQuery();

                try
                {
                    SurfaceToDatabase(building);
                }
                catch (Exception e)
                {
                    // TODO: implementieren
                    //DeleteSurfaceFromDatabase(building);
                    throw new InvalidConstraintException("Surface cannot be added to database, Building is not added to database:\n" + e);
                }
            }
            else
            {
                throw new ArgumentException("Invalid Building, BoundingBox is missing:\n" + building.ToString());
            }
        }
        catch (Exception e)
        {
            throw new DataException("Building " + building.CityGMLID + " cannot be inserted into database: " + e);
        }
    }



    // TODO: Löschen implementieren
    private void DeleteSurfaceFromDatabase(Building building)
    {
        throw new NotImplementedException();
    }




    private void SurfaceToDatabase(Building building)
    {
        SurfaceToDatabase(building.CityGMLID, building.ExteriorSurfaces);
    }

    private void SurfaceToDatabase(string buildingCityGMLID, Dictionary<string, Surface> exteriorSurfaces)
    {
        foreach (KeyValuePair<string, Surface> uniqueKeyAssociatedSurface in exteriorSurfaces)
        {
            SurfaceToDatabase(buildingCityGMLID, uniqueKeyAssociatedSurface.Key, uniqueKeyAssociatedSurface.Value);
        }
    }

    private void SurfaceToDatabase(string buildingCityGMLID, string uniqueSurfaceID, Surface surface)
    {
        IDbCommand dbCommand;

        // Add Surface to Surface table
        dbCommand = dbConnection.CreateCommand();

        try
        {
            dbCommand.CommandText = ""
                + Environment.NewLine + "INSERT INTO Surface("
                + Environment.NewLine + "BuildingCityGMLID,"
                + Environment.NewLine + "SurfaceID,"
                + Environment.NewLine + "Type"
                + Environment.NewLine + ")"
                + Environment.NewLine + "VALUES("
                + Environment.NewLine + "\"" + buildingCityGMLID + "\","
                + Environment.NewLine + "\"" + uniqueSurfaceID + "\","
                + Environment.NewLine + "\"" + surface.Type + "\" "
                + Environment.NewLine + ");";

            //Debug.Log(dbCommand.CommandText);
            dbCommand.ExecuteNonQuery();
        }
        catch (Exception)
        {
            throw new ArgumentException("Surface of Building " + buildingCityGMLID + "cannot be inserted. Make sure the uniqueSurfaceID " + uniqueSurfaceID + "is unique.");
        }


        // Add SurfacePoints to SurfacePoint table
        foreach (double3 surfacePoint in surface.Polygon)
        {
            dbCommand.CommandText = ""
                + Environment.NewLine + "INSERT INTO SurfacePoint("
                + Environment.NewLine + "SurfaceID,"
                + Environment.NewLine + "X,"
                + Environment.NewLine + "Y,"
                + Environment.NewLine + "Z"
                + Environment.NewLine + ")"
                + Environment.NewLine + "VALUES("
                + Environment.NewLine + "\"" + uniqueSurfaceID + "\", "
                + Environment.NewLine + surfacePoint.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                + Environment.NewLine + surfacePoint.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                + Environment.NewLine + surfacePoint.z.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                + Environment.NewLine + ");";

            //Debug.Log(dbCommand.CommandText);
            dbCommand.ExecuteNonQuery();
        }
    }




    #region BuildingAnnotation

    public void BuildingAnnotationToDatabase(List<BuildingAnnotation> buildingAnnotations)
    {
        Execute("BEGIN TRANSACTION");

        //        try
        //        {
        foreach (BuildingAnnotation buildingAnnotation in buildingAnnotations)
        {
            BuildingAnnotationToDatabase(buildingAnnotation);
        }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError(e);
        //        }

        Execute("COMMIT TRANSACTION");
    }

    public int AnnotationToDatabase(Annotation annotation)
    {
        IDbCommand dbCommand;
        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = ""
            + Environment.NewLine + "INSERT INTO Annotation("
            + Environment.NewLine + "ScaleWithCameraDistance,"
            + Environment.NewLine + "ScaleBySelection,"
            + Environment.NewLine + "PointingDirectionX,"
            + Environment.NewLine + "PointingDirectionY,"
            + Environment.NewLine + "PointingDirectionZ"
            + Environment.NewLine + ")"
            + Environment.NewLine + "VALUES("
            + Environment.NewLine + (int)((annotation.AnnotationProperties.ScaleWithCameraDistance) ? DBBoolean.DBTrue : DBBoolean.DBFalse) + ","
            + Environment.NewLine + (int)((annotation.AnnotationProperties.ScaleBySelection) ? DBBoolean.DBTrue : DBBoolean.DBFalse) + ","
            + Environment.NewLine + annotation.AnnotationProperties.PointingDirection.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
            + Environment.NewLine + annotation.AnnotationProperties.PointingDirection.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
            + Environment.NewLine + annotation.AnnotationProperties.PointingDirection.z.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " "
            + Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();

        return GetLastInsertedRowID();
    }


    public int TextAnnotationComponentToDatabase(TextAnnotationComponent textAnnotationComponent)
    {
        IDbCommand dbCommand;
        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = ""
            + Environment.NewLine + "INSERT INTO TextAnnotationComponent("
            + Environment.NewLine + "Text,"
            + Environment.NewLine + "TextSize "
            + Environment.NewLine + ")"
            + Environment.NewLine + "VALUES("
            + Environment.NewLine + "\"" + textAnnotationComponent.Text + "\"" + ","
            + Environment.NewLine + textAnnotationComponent.TextSize.ToString("000.000", System.Globalization.CultureInfo.InvariantCulture) + " "
            + Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();

        return GetLastInsertedRowID();
    }

    public void SimpleTextAnnotationToDatabase(int annotationID, int textID)
    {
        IDbCommand dbCommand;
        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = ""
            + Environment.NewLine + "INSERT INTO SimpleTextAnnotation("
            + Environment.NewLine + "AnnotationID,"
            + Environment.NewLine + "TextID "
            + Environment.NewLine + ")"
            + Environment.NewLine + "VALUES("
            + Environment.NewLine + annotationID + ","
            + Environment.NewLine + textID + " "
            + Environment.NewLine + ");";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();
    }

    public void BuildingAnnotationToDatabase(BuildingAnnotation buildingAnnotation)
    {
        switch (buildingAnnotation.AnnotationComponent)
        {
            case TextAnnotationComponent textAnnotationComponent:

                textAnnotationComponent = (TextAnnotationComponent)buildingAnnotation.AnnotationComponent;

                int annotationID = AnnotationToDatabase(buildingAnnotation);
                int textAnnotationComponentID = TextAnnotationComponentToDatabase(textAnnotationComponent);
                SimpleTextAnnotationToDatabase(annotationID, textAnnotationComponentID);

                IDbCommand dbCommand;
                dbCommand = dbConnection.CreateCommand();

                dbCommand.CommandText = ""
                    + Environment.NewLine + "INSERT INTO BuildingAnnotation("
                    + Environment.NewLine + "BuildingCityGMLID,"
                    + Environment.NewLine + "AnnotationID"
                    + Environment.NewLine + ")"
                    + Environment.NewLine + "VALUES("
                    + Environment.NewLine + "\"" + buildingAnnotation.AssociatedBuilding.CityGMLID + "\"" + ","
                    + Environment.NewLine + "\"" + annotationID + "\"" + " "
                    + Environment.NewLine + ");";

                //Debug.Log(dbCommand.CommandText);

                dbCommand.ExecuteNonQuery();

                break;

            default: throw new NotImplementedException();
        }
    }

    public List<BuildingAnnotation> GetBuildingAnnotation(BoundingBox boundingBox, Dictionary<string,Building> buildingsWithinBoundingBox)
    {
        List<BuildingAnnotation> buildingAnnotations = new List<BuildingAnnotation>();

        using (IDbCommand worldCoordinateAnnotationQuery = dbConnection.CreateCommand())
        {
            IDataReader dataReader;

            worldCoordinateAnnotationQuery.CommandText = ""
                    + Environment.NewLine + "SELECT ScaleWithCameraDistance, ScaleBySelection, PointingDirectionX, PointingDirectionY, PointingDirectionZ, Text, Textsize, BuildingAnnotation.BuildingCityGMLID, MeasuredHeight, BoundingBox_LowerLeftCorner_X, BoundingBox_LowerLeftCorner_Y, BoundingBox_LowerLeftCorner_Z, BoundingBox_UpperRightCorner_X, BoundingBox_UpperRightCorner_Y, BoundingBox_UpperRightCorner_Z"
                    + Environment.NewLine + "FROM BuildingAnnotation"
                    + Environment.NewLine + "INNER JOIN Annotation ON BuildingAnnotation.AnnotationID = Annotation.AnnotationID"
                    + Environment.NewLine + "INNER JOIN SimpleTextAnnotation ON Annotation.AnnotationID = SimpleTextAnnotation.AnnotationID"
                    + Environment.NewLine + "INNER JOIN TextAnnotationComponent ON SimpleTextAnnotation.TextID = TextAnnotationComponent.TextID"
                    + Environment.NewLine + "INNER JOIN Building ON BuildingAnnotation.BuildingCityGMLID = Building.BuildingCityGMLID"
                    + Environment.NewLine + "WHERE BoundingBox_LowerLeftCorner_X >= " + boundingBox.ButtomLowerLeftCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + Environment.NewLine + "AND BoundingBox_LowerLeftCorner_Y >= " + boundingBox.ButtomLowerLeftCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + Environment.NewLine + "AND BoundingBox_UpperRightCorner_X <= " + boundingBox.TopUpperRightCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + Environment.NewLine + "AND BoundingBox_UpperRightCorner_Y <= " + boundingBox.TopUpperRightCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);

            //Debug.Log(worldCoordinateAnnotationQuery.CommandText);

            dataReader = worldCoordinateAnnotationQuery.ExecuteReader();

            while (dataReader.Read())
            {
                // -- AnnotationProperties ----------
                int.TryParse(dataReader[0].ToString(), out int scaleWithCameraDistanceInt);
                bool scaleWithCameraDistance = (bool)((scaleWithCameraDistanceInt == (int)DBBoolean.DBTrue) ? true : false);
                int.TryParse(dataReader[1].ToString(), out int scaleBySelectionInt);
                bool scaleBySelection = (bool)((scaleBySelectionInt == (int)DBBoolean.DBTrue) ? true : false);
                float.TryParse(dataReader[2].ToString(), out float pointingX);
                float.TryParse(dataReader[3].ToString(), out float pointingY);
                float.TryParse(dataReader[4].ToString(), out float pointingZ);
                // -- TextAnnotationComponent ----------
                string annotationText = dataReader[5].ToString();
                float.TryParse(dataReader[6].ToString(), out float textSize);

                // -- BuildingAnnotationProperties
                string buildingCityGMLID = dataReader[7].ToString();
                float.TryParse(dataReader[8].ToString(), out float measuredHeight);

                if (!buildingsWithinBoundingBox.TryGetValue(buildingCityGMLID, out Building associatedBuilding))
                {
                    Debug.LogWarning("BuildingAnnotation cannot be associated with a Building. Creating dummy Building object.");

                    associatedBuilding = new Building(buildingCityGMLID, measuredHeight);

                    double.TryParse(dataReader[9].ToString(), out double ButtomLowerLeftCornerX);
                    double.TryParse(dataReader[10].ToString(), out double ButtomLowerLeftCornerY);
                    double.TryParse(dataReader[11].ToString(), out double TopUpperRightCornerX);
                    double.TryParse(dataReader[12].ToString(), out double TopUpperRightCornerY);

                    double3 ButtomLowerLeftCorner = new double3(ButtomLowerLeftCornerX, ButtomLowerLeftCornerY, 0);
                    double3 TopUpperRightCorner = new double3(TopUpperRightCornerX, TopUpperRightCornerY, 0);

                    BoundingBox buildingPropertiesBoundingBox = new BoundingBox(ButtomLowerLeftCorner, TopUpperRightCorner);
                    associatedBuilding.BoundingBox = buildingPropertiesBoundingBox;
                }

                buildingAnnotations.Add(
                    new BuildingAnnotation(associatedBuilding,
                                                    new TextAnnotationComponent(annotationText, textSize),
                                                    new AnnotationProperties(scaleWithCameraDistance, scaleBySelection, new float3(pointingX, pointingY, pointingZ))));
            }
        }

        return buildingAnnotations;
    }



    #endregion


    #region SurfaceAnnotation

    public void SurfaceAnnotationToDatabase(List<SurfaceAnnotation> surfaceAnnotations)
    {
        Execute("BEGIN TRANSACTION");

        //        try
        //        {
        foreach (SurfaceAnnotation surfaceAnnotation in surfaceAnnotations)
        {
            //Debug.Log(surfaceAnnotation.ToString());
            SurfaceAnnotationToDatabase(surfaceAnnotation);
        }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError(e);
        //        }

        Execute("COMMIT TRANSACTION");
    }

    public void SurfaceAnnotationToDatabase(SurfaceAnnotation surfaceAnnotation)
    {
        switch (surfaceAnnotation.AnnotationComponent)
        {
            case TextAnnotationComponent textAnnotationComponent:

                textAnnotationComponent = (TextAnnotationComponent)surfaceAnnotation.AnnotationComponent;

                int annotationID = AnnotationToDatabase(surfaceAnnotation);
                int textAnnotationComponentID = TextAnnotationComponentToDatabase(textAnnotationComponent);
                SimpleTextAnnotationToDatabase(annotationID, textAnnotationComponentID);

                IDbCommand dbCommand;
                dbCommand = dbConnection.CreateCommand();

                dbCommand.CommandText = ""
                    + Environment.NewLine + "INSERT INTO SurfaceAnnotation("
                    + Environment.NewLine + "SurfaceID,"
                    + Environment.NewLine + "AnchorPointIndex,"
                    + Environment.NewLine + "HeightAboveBaseline,"
                    + Environment.NewLine + "RelativePositionBetweenBaselinePoints,"
                    + Environment.NewLine + "AnnotationID "
                    + Environment.NewLine + ")"
                    + Environment.NewLine + "VALUES("
                    + Environment.NewLine + "\"" + surfaceAnnotation.AssociatedSurface.CityGMLID + "\"" + ","
                    + Environment.NewLine + surfaceAnnotation.AnnotationAnchorPointIndex + ","
                    + Environment.NewLine + surfaceAnnotation.HeightAboveBaseLine.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                    + Environment.NewLine + surfaceAnnotation.RelativePositionBetweenBasePoints.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                    + Environment.NewLine + annotationID + " "
                    + Environment.NewLine + ");";

                //Debug.Log(dbCommand.CommandText);

                dbCommand.ExecuteNonQuery();

                break;

            default: throw new NotImplementedException();
        }
    }



    public List<SurfaceAnnotation> GetSurfaceAnnotation(BoundingBox boundingBox, Dictionary<string,Surface> surfacesWithinBoundingBox)
    {
        List<SurfaceAnnotation> surfaceAnnotations = new List<SurfaceAnnotation>();

        using (IDbCommand worldCoordinateAnnotationQuery = dbConnection.CreateCommand())
        {
            IDataReader dataReader;

            worldCoordinateAnnotationQuery.CommandText = ""
                    + Environment.NewLine + "SELECT ScaleWithCameraDistance, ScaleBySelection, PointingDirectionX, PointingDirectionY, PointingDirectionZ, Text, Textsize, SurfaceAnnotation.SurfaceID, AnchorPointIndex, RelativePositionBetweenBaselinePoints, HeightAboveBaseline"
                    + Environment.NewLine + "FROM SurfaceAnnotation"
                    + Environment.NewLine + "INNER JOIN Annotation ON SurfaceAnnotation.AnnotationID = Annotation.AnnotationID"
                    + Environment.NewLine + "INNER JOIN SimpleTextAnnotation ON Annotation.AnnotationID = SimpleTextAnnotation.AnnotationID"
                    + Environment.NewLine + "INNER JOIN TextAnnotationComponent ON SimpleTextAnnotation.TextID = TextAnnotationComponent.TextID"
                    + Environment.NewLine + "INNER JOIN Surface ON SurfaceAnnotation.SurfaceID = Surface.SurfaceID"
                    + Environment.NewLine + "INNER JOIN Building ON Surface.BuildingCityGMLID = Building.BuildingCityGMLID"
                    + Environment.NewLine + "WHERE BoundingBox_LowerLeftCorner_X >= " + boundingBox.ButtomLowerLeftCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + Environment.NewLine + "AND BoundingBox_LowerLeftCorner_Y >= " + boundingBox.ButtomLowerLeftCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + Environment.NewLine + "AND BoundingBox_UpperRightCorner_X <= " + boundingBox.TopUpperRightCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + Environment.NewLine + "AND BoundingBox_UpperRightCorner_Y <= " + boundingBox.TopUpperRightCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);

            //Debug.Log(worldCoordinateAnnotationQuery.CommandText);

            dataReader = worldCoordinateAnnotationQuery.ExecuteReader();

            //Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": finishes SurfaceAnnotation query, parsing results");

            while (dataReader.Read())
            {
                // -- AnnotationProperties ----------
                int.TryParse(dataReader[0].ToString(), out int scaleWithCameraDistanceInt);
                bool scaleWithCameraDistance = (bool)((scaleWithCameraDistanceInt == (int)DBBoolean.DBTrue) ? true : false);
                int.TryParse(dataReader[1].ToString(), out int scaleBySelectionInt);
                bool scaleBySelection = (bool)((scaleBySelectionInt == (int)DBBoolean.DBTrue) ? true : false);
                float.TryParse(dataReader[2].ToString(), out float pointingX);
                float.TryParse(dataReader[3].ToString(), out float pointingY);
                float.TryParse(dataReader[4].ToString(), out float pointingZ);
                // -- TextAnnotationComponent ----------
                string annotationText = dataReader[5].ToString();
                float.TryParse(dataReader[6].ToString(), out float textSize);

                // -- SurfaceAnnotationProperties
                string surfaceID = dataReader[7].ToString();
                int.TryParse(dataReader[8].ToString(), out int anchorPointIndex);
                double.TryParse(dataReader[9].ToString(), out double relativePositionBetweenBaselinePoints);
                double.TryParse(dataReader[10].ToString(), out double heightAboveBaseline);

                if (!surfacesWithinBoundingBox.TryGetValue(surfaceID, out Surface associatedSurface))
                {
                    Debug.LogWarning("SurfaceAnnotation cannot be associated with a Surface within the BoundingBox. Creating dummy Surface object.");
                    continue;
                }

                surfaceAnnotations.Add(
                    new SurfaceAnnotation(associatedSurface, anchorPointIndex, relativePositionBetweenBaselinePoints, heightAboveBaseline,
                                                    new TextAnnotationComponent(annotationText, textSize),
                                                    new AnnotationProperties(scaleWithCameraDistance, scaleBySelection, new float3(pointingX, pointingY, pointingZ))));
            }
        }

        return surfaceAnnotations;
    }

    #endregion



    #region WorldCoordinateAnnotation

    public void WorldCoordinateAnnotationToDatabase(List<WorldCoordinateAnnotation> worldCoordinateAnnotations)
    {
        Execute("BEGIN TRANSACTION");

        //        try
        //        {
        foreach (WorldCoordinateAnnotation worldCoordinateAnnotation in worldCoordinateAnnotations)
        {
            WorldCoordinateAnnotationToDatabase(worldCoordinateAnnotation);
        }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError(e);
        //        }

        Execute("COMMIT TRANSACTION");
    }

    public void WorldCoordinateAnnotationToDatabase(WorldCoordinateAnnotation worldCoordinateAnnotation)
    {
        switch (worldCoordinateAnnotation.AnnotationComponent)
        {
            case TextAnnotationComponent textAnnotationComponent:

                textAnnotationComponent = (TextAnnotationComponent)worldCoordinateAnnotation.AnnotationComponent;

                int annotationID = AnnotationToDatabase(worldCoordinateAnnotation);
                int textAnnotationComponentID = TextAnnotationComponentToDatabase(textAnnotationComponent);
                SimpleTextAnnotationToDatabase(annotationID, textAnnotationComponentID);

                IDbCommand dbCommand;
                dbCommand = dbConnection.CreateCommand();

                dbCommand.CommandText = ""
                    + Environment.NewLine + "INSERT INTO WorldCoordinateAnnotation("
                    + Environment.NewLine + "X,"
                    + Environment.NewLine + "Y,"
                    + Environment.NewLine + "Z,"
                    + Environment.NewLine + "AnnotationID "
                    + Environment.NewLine + ")"
                    + Environment.NewLine + "VALUES("
                    + Environment.NewLine + worldCoordinateAnnotation.AnnotationUMLCoordinates.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                    + Environment.NewLine + worldCoordinateAnnotation.AnnotationUMLCoordinates.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                    + Environment.NewLine + worldCoordinateAnnotation.AnnotationUMLCoordinates.z.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ","
                    + Environment.NewLine + annotationID + " "
                    + Environment.NewLine + ");";

                //Debug.Log(dbCommand.CommandText);

                dbCommand.ExecuteNonQuery();

                break;

            default: throw new NotImplementedException();
        }
    }



    public List<WorldCoordinateAnnotation> GetWorldCoordinateAnnotation(BoundingBox boundingBox)
    {
        List<WorldCoordinateAnnotation> worldCoordinateAnnotations = new List<WorldCoordinateAnnotation>();

        using (IDbCommand worldCoordinateAnnotationQuery = dbConnection.CreateCommand())
        {
            IDataReader dataReader;

            worldCoordinateAnnotationQuery.CommandText = ""
                    + Environment.NewLine + "SELECT ScaleWithCameraDistance, ScaleBySelection, PointingDirectionX, PointingDirectionY, PointingDirectionZ, Text, Textsize, X, Y, Z"
                    + Environment.NewLine + "FROM WorldCoordinateAnnotation"
                    + Environment.NewLine + "INNER JOIN Annotation ON WorldCoordinateAnnotation.AnnotationID = Annotation.AnnotationID"
                    + Environment.NewLine + "INNER JOIN SimpleTextAnnotation ON Annotation.AnnotationID = SimpleTextAnnotation.AnnotationID"
                    + Environment.NewLine + "INNER JOIN TextAnnotationComponent ON SimpleTextAnnotation.TextID = TextAnnotationComponent.TextID"
                    + Environment.NewLine + "WHERE X >= " + boundingBox.ButtomLowerLeftCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + Environment.NewLine + "AND Y >= " + boundingBox.ButtomLowerLeftCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + Environment.NewLine + "AND X <= " + boundingBox.TopUpperRightCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + Environment.NewLine + "AND Y <= " + boundingBox.TopUpperRightCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);

            //Debug.Log(worldCoordinateAnnotationQuery.CommandText);

            dataReader = worldCoordinateAnnotationQuery.ExecuteReader();

            //Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": finishes WordCoordinateAnnotation query, parsing results");

            while (dataReader.Read())
            {
                // -- AnnotationProperties ----------
                int.TryParse(dataReader[0].ToString(), out int scaleWithCameraDistanceInt);
                bool scaleWithCameraDistance = (bool)((scaleWithCameraDistanceInt == (int)DBBoolean.DBTrue) ? true : false);
                int.TryParse(dataReader[1].ToString(), out int scaleBySelectionInt);
                bool scaleBySelection = (bool)((scaleBySelectionInt == (int)DBBoolean.DBTrue) ? true : false);
                float.TryParse(dataReader[2].ToString(), out float pointingX);
                float.TryParse(dataReader[3].ToString(), out float pointingY);
                float.TryParse(dataReader[4].ToString(), out float pointingZ);
                // -- TextAnnotationComponent ----------
                string annotationText = dataReader[5].ToString();
                float.TryParse(dataReader[6].ToString(), out float textSize);

                // -- SurfaceAnnotationProperties
                double.TryParse(dataReader[7].ToString(), out double coordinateX);
                double.TryParse(dataReader[8].ToString(), out double coordinateY);
                double.TryParse(dataReader[9].ToString(), out double coordinateZ);

                worldCoordinateAnnotations.Add(
                    new WorldCoordinateAnnotation(new double3(coordinateX, coordinateY, coordinateZ),
                                                    new TextAnnotationComponent(annotationText, textSize),
                                                    new AnnotationProperties(scaleWithCameraDistance, scaleBySelection, new float3(pointingX, pointingY, pointingZ))));
            }
        }

        return worldCoordinateAnnotations;
    }


    #endregion






    public Dictionary<string, Building> GetBuildings(BoundingBox boundingBox)
    {
        /// Dictionary with Building values and its CityGMLID as its associated dictionary key
        Dictionary<string, Building> buildings = new Dictionary<string, Building>();

        using (IDbCommand buildingQuery = dbConnection.CreateCommand())
        {
            IDataReader dataReader;

//            string SelectedSurfaces = "SelectedSurfaces";
//            string SelectedBuildings = "SelectedBuildings";

            buildingQuery.CommandText = "SELECT  SelectedSurfaces.BuildingCityGMLID,"
                + Environment.NewLine + "        SelectedSurfaces.SurfaceID,"
                + Environment.NewLine + "        SelectedSurfaces.Type,"
                + Environment.NewLine + "        SurfacePoint.X,"
                + Environment.NewLine + "        SurfacePoint.Y,"
                + Environment.NewLine + "        SurfacePoint.Z,"
                + Environment.NewLine + "        SelectedSurfaces.MeasuredHeight,"
                + Environment.NewLine + "        SelectedSurfaces.BoundingBox_LowerLeftCorner_X,"
                + Environment.NewLine + "        SelectedSurfaces.BoundingBox_LowerLeftCorner_Y,"
                + Environment.NewLine + "        SelectedSurfaces.BoundingBox_LowerLeftCorner_Z,"
                + Environment.NewLine + "        SelectedSurfaces.BoundingBox_UpperRightCorner_X,"
                + Environment.NewLine + "        SelectedSurfaces.BoundingBox_UpperRightCorner_Y,"
                + Environment.NewLine + "        SelectedSurfaces.BoundingBox_UpperRightCorner_Z"
                + Environment.NewLine + "FROM  SurfacePoint"
                + Environment.NewLine + "        INNER JOIN (SELECT SelectedBuildings.BuildingCityGMLID,"
                + Environment.NewLine + "                Surface.SurfaceID,"
                + Environment.NewLine + "                Surface.Type,"
                + Environment.NewLine + "                SelectedBuildings.MeasuredHeight,"
                + Environment.NewLine + "                SelectedBuildings.BoundingBox_LowerLeftCorner_X,"
                + Environment.NewLine + "                SelectedBuildings.BoundingBox_LowerLeftCorner_Y,"
                + Environment.NewLine + "                SelectedBuildings.BoundingBox_LowerLeftCorner_Z,"
                + Environment.NewLine + "                SelectedBuildings.BoundingBox_UpperRightCorner_X,"
                + Environment.NewLine + "                SelectedBuildings.BoundingBox_UpperRightCorner_Y,"
                + Environment.NewLine + "                SelectedBuildings.BoundingBox_UpperRightCorner_Z"
                + Environment.NewLine + "                FROM Surface"
                + Environment.NewLine + "                        INNER JOIN ( SELECT"
                + Environment.NewLine + "                        Building.BuildingCityGMLID,"
                + Environment.NewLine + "                        Building.MeasuredHeight,"
                + Environment.NewLine + "                        Building.BoundingBox_LowerLeftCorner_X,"
                + Environment.NewLine + "                        Building.BoundingBox_LowerLeftCorner_Y,"
                + Environment.NewLine + "                        Building.BoundingBox_LowerLeftCorner_Z,"
                + Environment.NewLine + "                        Building.BoundingBox_UpperRightCorner_X,"
                + Environment.NewLine + "                        Building.BoundingBox_UpperRightCorner_Y,"
                + Environment.NewLine + "                        Building.BoundingBox_UpperRightCorner_Z"
                + Environment.NewLine + "                        FROM Building"
                + Environment.NewLine + "                         WHERE BoundingBox_LowerLeftCorner_X >= " + boundingBox.ButtomLowerLeftCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                + Environment.NewLine + "                         AND BoundingBox_LowerLeftCorner_Y >= " + boundingBox.ButtomLowerLeftCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                + Environment.NewLine + "                         AND BoundingBox_UpperRightCorner_X <= " + boundingBox.TopUpperRightCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                + Environment.NewLine + "                         AND BoundingBox_UpperRightCorner_Y <= " + boundingBox.TopUpperRightCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                + Environment.NewLine + "                        ) AS SelectedBuildings ON SelectedBuildings.BuildingCityGMLID = Surface.BuildingCityGMLID"
                + Environment.NewLine + "                        ) AS SelectedSurfaces ON SelectedSurfaces.SurfaceID = SurfacePoint.SurfaceID";




                /*
                + SelectedSurfaces + "." + TableNames.Building_CityGMLID + ",\n        "
                + SelectedSurfaces + "." + TableNames.Building_MeasuredHeight + ",\n        "
                + SelectedSurfaces + "." + TableNames.Building_BoundingBox_LowerLeftCorner_X + ",\n        "
                + SelectedSurfaces + "." + TableNames.Building_BoundingBox_LowerLeftCorner_Y + ",\n        "
                + SelectedSurfaces + "." + TableNames.Building_BoundingBox_LowerLeftCorner_Z + ",\n        "
                + SelectedSurfaces + "." + TableNames.Building_BoundingBox_UpperRightCorner_X + ",\n        "
                + SelectedSurfaces + "." + TableNames.Building_BoundingBox_UpperRightCorner_Y + ",\n        "
                + SelectedSurfaces + "." + TableNames.Building_BoundingBox_UpperRightCorner_Z + ",\n        "
                */
            /*
            + SelectedSurfaces + "." + TableNames.Surface_CityGMLID + ",\n        "
            + SelectedSurfaces + "." + TableNames.Surface_Type + ",\n        "
            + TableNames.SurfacePoint + "." + TableNames.Surface_Point_X + ",\n        "
            + TableNames.SurfacePoint + "." + TableNames.Surface_Point_Y + ",\n        "
            + TableNames.SurfacePoint + "." + TableNames.Surface_Point_Z + "\n"
            + "FROM  "
            + TableNames.SurfacePoint + "\n"
            + "        INNER JOIN "
            + "(SELECT "
            + SelectedBuildings + "." + TableNames.Building_CityGMLID + ",\n                "
            + SelectedBuildings + "." + TableNames.Building_MeasuredHeight + ",\n                "
            + SelectedBuildings + "." + TableNames.Building_BoundingBox_LowerLeftCorner_X + ",\n                "
            + SelectedBuildings + "." + TableNames.Building_BoundingBox_LowerLeftCorner_Y + ",\n                "
            + SelectedBuildings + "." + TableNames.Building_BoundingBox_LowerLeftCorner_Z + ",\n                "
            + SelectedBuildings + "." + TableNames.Building_BoundingBox_UpperRightCorner_X + ",\n                "
            + SelectedBuildings + "." + TableNames.Building_BoundingBox_UpperRightCorner_Y + ",\n                "
            + SelectedBuildings + "." + TableNames.Building_BoundingBox_UpperRightCorner_Z + ",\n                "
            */
            /*
            + TableNames.Surface + "." + TableNames.Surface_CityGMLID + ",\n                "
            + TableNames.Surface + "." + TableNames.Surface_Type + "\n                "
            + "FROM " + TableNames.Surface + "\n                        "
            + "INNER JOIN ( "
            + "SELECT\n                        "
            + TableNames.Building + "." + TableNames.Building_CityGMLID + /*",\n                        "*/
/*                                                                                                                "\n                        "
                + TableNames.Building + "." + TableNames.Building_MeasuredHeight + ",\n                        "
                + TableNames.Building + "." + TableNames.Building_BoundingBox_LowerLeftCorner_X + ",\n                        "
                + TableNames.Building + "." + TableNames.Building_BoundingBox_LowerLeftCorner_Y + ",\n                        "
                + TableNames.Building + "." + TableNames.Building_BoundingBox_LowerLeftCorner_Z + ",\n                        "
                + TableNames.Building + "." + TableNames.Building_BoundingBox_UpperRightCorner_X + ",\n                        "
                + TableNames.Building + "." + TableNames.Building_BoundingBox_UpperRightCorner_Y + ",\n                        "
                + TableNames.Building + "." + TableNames.Building_BoundingBox_UpperRightCorner_Z + "\n                        "
                + "FROM "
                + TableNames.Building + "\n                        "
                + " WHERE " + TableNames.Building_BoundingBox_LowerLeftCorner_X + " >= " + boundingBox.ButtomLowerLeftCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "\n                        "
                + " AND " + TableNames.Building_BoundingBox_LowerLeftCorner_Y + " >= " + boundingBox.ButtomLowerLeftCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "\n                        "
                + " AND " + TableNames.Building_BoundingBox_UpperRightCorner_X + " <= " + boundingBox.TopUpperRightCorner.Value.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "\n                        "
                + " AND " + TableNames.Building_BoundingBox_UpperRightCorner_Y + " <= " + boundingBox.TopUpperRightCorner.Value.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "\n                        "
                + ") AS " + SelectedBuildings + " ON " + SelectedBuildings + "." + TableNames.Building_CityGMLID + " = " + TableNames.Surface + "." + TableNames.Building_CityGMLID + "\n                        "
                + ") AS " + SelectedSurfaces + " ON " + SelectedSurfaces + "." + TableNames.Surface_CityGMLID + " = " + TableNames.SurfacePoint + "." + TableNames.Surface_CityGMLID; ;
                */

            Debug.Log(buildingQuery.CommandText);

            dataReader = buildingQuery.ExecuteReader();

            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": finishes query, parsing results");


            while (dataReader.Read())
            {
                string buildingCityGMLID = dataReader[0].ToString();
                string uniqueSurfaceID = dataReader[1].ToString();

                SurfaceType surfaceType;
                switch (dataReader[2].ToString())
                {
                    case "GroundSurface":
                        surfaceType = SurfaceType.GroundSurface;
                        break;
                    case "WallSurface":
                        surfaceType = SurfaceType.WallSurface;
                        break;
                    case "RoofSurface":
                        surfaceType = SurfaceType.RoofSurface;
                        break;
                    case "UNDEFINED":
                        surfaceType = SurfaceType.UNDEFINED;
                        break;
                    default:
                        surfaceType = SurfaceType.UNDEFINED;
                        break;
                }

                double.TryParse(dataReader[3].ToString(), out double x);
                double.TryParse(dataReader[4].ToString(), out double y);
                double.TryParse(dataReader[5].ToString(), out double z);

                Building currentSurfacePointAssociatedBuilding;

                if (buildings.TryGetValue(buildingCityGMLID, out Building cityGMLIDAssociatedBuilding))
                {
                    currentSurfacePointAssociatedBuilding = cityGMLIDAssociatedBuilding;
                }
                else
                {
                    // Create new Building and parse additional buonding box and measuredHeight parameter
                    float.TryParse(dataReader[6].ToString(), out float measuredHeight);
                    
                    currentSurfacePointAssociatedBuilding = new Building(buildingCityGMLID, measuredHeight);
                    buildings.Add(buildingCityGMLID, currentSurfacePointAssociatedBuilding);

                    double.TryParse(dataReader[7].ToString(), out double buttomLowerLeftCornerX);
                    double.TryParse(dataReader[8].ToString(), out double buttomLowerLeftCornerY);
                    double.TryParse(dataReader[9].ToString(), out double buttomLowerLeftCornerZ);
                    double.TryParse(dataReader[10].ToString(), out double topUpperRightCornerX);
                    double.TryParse(dataReader[11].ToString(), out double topUpperRightCornerY);
                    double.TryParse(dataReader[12].ToString(), out double topUpperRightCornerZ);

                    double3 buttomLowerLeftCorner = new double3(buttomLowerLeftCornerX, buttomLowerLeftCornerY, buttomLowerLeftCornerZ);
                    double3 topUpperRightCorner = new double3(topUpperRightCornerX, topUpperRightCornerY, topUpperRightCornerZ);

                    currentSurfacePointAssociatedBuilding.BoundingBox = new BoundingBox(buttomLowerLeftCorner, topUpperRightCorner);

                }
                currentSurfacePointAssociatedBuilding.AddSurfacePoint(uniqueSurfaceID, surfaceType, new double3(x, y, z));

            }
        }

        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": finishes parsing, transforming Dictionary to List");

        return buildings;
    }

    /* !!!!!! Laufzeitkosten für Vergleich auf String-Ebene zu groß, Suche nach BuildingCityGMLID benötigt > 4s
    private List<Surface> GetSurfaces(Building building)
    {
        Dictionary<string, Surface> surfaces = new Dictionary<string, Surface>();

        using (IDbCommand groundSurfacePointQuery = dbConnection.CreateCommand())
        {
            IDataReader dataReader;

            groundSurfacePointQuery.CommandText =
                "SELECT " + TableNames.Surface_Point_X + ", " + TableNames.Surface_Point_Y + ", " + TableNames.Surface_Point_Z + " " +
                "FROM " + TableNames.Surface + " " +
                "WHERE(" + TableNames.Building_CityGMLID + " == " + building.CityGMLID + ")";

            Debug.Log(groundSurfacePointQuery.CommandText);

            dataReader = groundSurfacePointQuery.ExecuteReader();

            while (dataReader.Read())
            {
                double.TryParse(dataReader[0].ToString(), out double x);
                double.TryParse(dataReader[1].ToString(), out double y);
                double.TryParse(dataReader[2].ToString(), out double z);

                double3 newPoint = new double3(x, y, z);

                groundSurfacePoints.Add(newPoint);
            }

            return groundSurfacePoints;
        }
    }
    */
}