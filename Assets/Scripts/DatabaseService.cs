using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Unity.Mathematics;
using UnityEngine;


public class DatabaseService
{
    struct TableNames
    {
        public const string Building = "Building";

        public const string Building_BuildingID = "BuildingID";
        public const string Building_BuildingSurfaceID = "BuildingSurfaceID";
        public const string Building_CityGMLID = "CityGMLID";
        public const string Building_MeasuredHeight = "MeasuredHeight";
        public const string Building_boundingBox_UpperRightCorner_X = "boundingBox_UpperRightCorner_X";
        public const string Building_boundingBox_UpperRightCorner_Y = "boundingBox_UpperRightCorner_Y";
        public const string Building_boundingBox_LowerLeftCorner_X = "boundingBox_LowerLeftCorner_X";
        public const string Building_boundingBox_LowerLeftCorner_Y = "boundingBox_LowerLeftCorner_Y";


        public const string GroundSurfacePoint = "GroundSurfacePoint";

        public const string Point_X = "X";
        public const string Point_Y = "Y";
        public const string Point_Z = "Z";




        public const string Annotation_ComponentID = "ComponentID";
        public const string Annotation_Text = "AnnotationText";
        public const string Annotation_LocalScale = "AnnotationLocalScale";


        public const string BuildingAnnotation = "BuildingAnnotation";

        public const string SurfaceAnnotation = "SurfaceAnnotation";
        public const string SurfaceAnnotation_RelativeGroundSurfacePosition = "RelativeGroundSurfacePosition";
        public const string SurfaceAnnotation_Height = "Height";

        public const string FreeWorldAnnotation = "FreeWorldAnnotation";
    }


    private string DatabasePath = Application.persistentDataPath + "/"; // Application.streamingAssetsPath vs Application.persistentDataPath
    private const string DatabaseName = "Database.db";

    private IDbConnection dbConnection;


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
            dbConnection.Close();
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
        dbCommand.CommandText = "DROP TABLE IF EXISTS " + TableNames.Building;
        dbCommand.ExecuteReader();

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE " + TableNames.Building + 
            "(" +
            "[" + TableNames.Building_BuildingID +
            "] INTEGER," +
            "[" + TableNames.Building_CityGMLID + "] " +
            "VARCHAR(42) NOT NULL UNIQUE," +
            "[" + TableNames.Building_MeasuredHeight + "] " +
            "FLOAT NOT NULL," +
            "[" + TableNames.Building_boundingBox_LowerLeftCorner_X + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.Building_boundingBox_LowerLeftCorner_Y + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.Building_boundingBox_UpperRightCorner_X + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.Building_boundingBox_UpperRightCorner_Y + "] " +
            "DOUBLE NOT NULL," +
            "PRIMARY KEY(" + TableNames.Building_BuildingID + ")" + 
            ")";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // GroundSUrfacePoints ---------------------------------------------------
               
        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "DROP TABLE IF EXISTS " + TableNames.GroundSurfacePoint;
        dbCommand.ExecuteReader();

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE " + TableNames.GroundSurfacePoint + "(" +
            "[" + TableNames.Building_BuildingID + "] " +
            "INTEGER," +
            "[" + TableNames.Building_BuildingSurfaceID + "] " +
            "VARCHAR," +
            "[" + TableNames.Point_X + "] " +
            "DOUBLE," +
            "[" + TableNames.Point_Y + "] " +
            "DOUBLE," +
            "[" + TableNames.Point_Z + "] " +
            "DOUBLE," +
            "PRIMARY KEY(" + TableNames.Building_BuildingID + ", " + TableNames.Building_BuildingSurfaceID + ", " + TableNames.Point_X + ", " + TableNames.Point_Y + ", " + TableNames.Point_Z +  ")," +
            "FOREIGN KEY(" + TableNames.Building_BuildingID + ") REFERENCES " + TableNames.Building + "(" + TableNames.Building_BuildingID + ")" +
            ")";

        //Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();


        // BuildingAnnotations ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "DROP TABLE IF EXISTS " + TableNames.BuildingAnnotation;
        dbCommand.ExecuteReader();

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE " + TableNames.BuildingAnnotation + "(" +
            "[" + TableNames.Building_BuildingID + "] " +
            "INTEGER," +
            "[" + TableNames.Annotation_ComponentID + "] " +
            "INTEGER," +
            "[" + TableNames.Annotation_Text + "] " +
            "TEXT NOT NULL," +
            "[" + TableNames.Annotation_LocalScale + "] " +
            "FLOAT NOT NULL," +
            "PRIMARY KEY(" + TableNames.Building_BuildingID + ", " + TableNames.Annotation_ComponentID + ")," + 
            "FOREIGN KEY(" + TableNames.Building_BuildingID + ") REFERENCES " + TableNames.Building + "(" + TableNames.Building_BuildingID + ")" +
            ")";

        Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();

        
        // SurfaceAnnotations ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "DROP TABLE IF EXISTS " + TableNames.SurfaceAnnotation;
        dbCommand.ExecuteReader();

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE " + TableNames.SurfaceAnnotation + "(" +
            "[" + TableNames.Building_BuildingID + "] " +
            "INTEGER," +
            "[" + TableNames.Point_X + "1] " +
            "DOUBLE," +
            "[" + TableNames.Point_Y + "1] " +
            "DOUBLE," +
            "[" + TableNames.Point_Z + "1] " +
            "DOUBLE," +
            "[" + TableNames.Point_X + "2] " +
            "DOUBLE," +
            "[" + TableNames.Point_Y + "2] " +
            "DOUBLE," +
            "[" + TableNames.Point_Z + "2] " +
            "DOUBLE," +
            "[" + TableNames.Annotation_ComponentID + "] " +
            "INTEGER," +
            "[" + TableNames.SurfaceAnnotation_RelativeGroundSurfacePosition + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.SurfaceAnnotation_Height + "] " +
            "DOUBLE NOT NULL," +
            "[" + TableNames.Annotation_Text + "] " +
            "TEXT NOT NULL, " +
            "[" + TableNames.Annotation_LocalScale + "] " +
            "FLOAT NOT NULL," +
            "PRIMARY KEY(" + TableNames.Building_BuildingID + ", " + TableNames.Point_X + "1, " + TableNames.Point_Y + "1, " + TableNames.Point_Z + "1, " + TableNames.Point_X + "2, " + TableNames.Point_Y + "2, " + TableNames.Point_Z + "2, " + TableNames.Annotation_ComponentID + "), " +
            "FOREIGN KEY(" + TableNames.Building_BuildingID + ") REFERENCES " + TableNames.GroundSurfacePoint + "(" + TableNames.Building_BuildingID + "), " +
            "FOREIGN KEY(" + TableNames.Point_X + "1) REFERENCES " + TableNames.GroundSurfacePoint + "(" + TableNames.Point_X + "), " +
            "FOREIGN KEY(" + TableNames.Point_X + "2) REFERENCES " + TableNames.GroundSurfacePoint + "(" + TableNames.Point_X + "), " +
            "FOREIGN KEY(" + TableNames.Point_Y + "1) REFERENCES " + TableNames.GroundSurfacePoint + "(" + TableNames.Point_Y + "), " +
            "FOREIGN KEY(" + TableNames.Point_Y + "2) REFERENCES " + TableNames.GroundSurfacePoint + "(" + TableNames.Point_Y + "), " +
            "FOREIGN KEY(" + TableNames.Point_Z + "1) REFERENCES " + TableNames.GroundSurfacePoint + "(" + TableNames.Point_Z + "), " +
            "FOREIGN KEY(" + TableNames.Point_Z + "2) REFERENCES " + TableNames.GroundSurfacePoint + "(" + TableNames.Point_Z + "), " +
            "CHECK(" + TableNames.SurfaceAnnotation_RelativeGroundSurfacePosition + " >= 0 " + "), " +
            "CHECK(" + TableNames.SurfaceAnnotation_RelativeGroundSurfacePosition + " <= 1 " + ")" +
            ")";

        Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();

        
        // FreeWorldAnnotations ---------------------------------------------------

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "DROP TABLE IF EXISTS " + TableNames.FreeWorldAnnotation;
        dbCommand.ExecuteReader();

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE " + TableNames.FreeWorldAnnotation + "(" +
            "[" + TableNames.Point_X + "] " +
            "DOUBLE," +
            "[" + TableNames.Point_Y + "] " +
            "DOUBLE," +
            "[" + TableNames.Point_Z + "] " +
            "DOUBLE," +
            "[" + TableNames.Annotation_ComponentID + "] " +
            "INTEGER," +
            "[" + TableNames.Annotation_Text + "] " +
            "TEXT NOT NULL," +
            "[" + TableNames.Annotation_LocalScale + "] " +
            "FLOAT NOT NULL," +
            "PRIMARY KEY(" + TableNames.Point_X + ", " + TableNames.Point_Y + ", " + TableNames.Point_Z + ", " + TableNames.Annotation_ComponentID + ")" +
            ")";

        Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();
    }



    public void BuidlingToDatabase(List<Building> buildings)
    {
        this.Execute("BEGIN TRANSACTION");

        foreach (Building building in buildings)
        {
            this.BuidlingToDatabase(building);
        }

        this.Execute("COMMIT TRANSACTION");
    }

    private void Execute(string query)
    {
        IDbCommand dbCommand;

        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;
        dbCommand.ExecuteNonQuery();
    }

    public void BuidlingToDatabase(Building building)
    {
        if (/*building.MeasuredHeight > 0 &&*/ building.BoundingBox.HasContent())
        {
            IDbCommand dbCommand;

            dbCommand = dbConnection.CreateCommand();

            dbCommand.CommandText = "INSERT INTO " + TableNames.Building + "(" +
                TableNames.Building_CityGMLID + ", " +
                TableNames.Building_BuildingID + ", " +
                TableNames.Building_MeasuredHeight + ", " +
                TableNames.Building_boundingBox_LowerLeftCorner_X + ", " +
                TableNames.Building_boundingBox_LowerLeftCorner_Y + ", " +
                TableNames.Building_boundingBox_UpperRightCorner_X + ", " +
                TableNames.Building_boundingBox_UpperRightCorner_Y + ")\n" +
                "VALUES(\"" +
                building.CityGMLID + "\", " +
                building.BuildingID.ToString() + "," +
                building.MeasuredHeight.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "," +
                building.BoundingBox.ButtomLowerLeftCorner.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "," +
                building.BoundingBox.ButtomLowerLeftCorner.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "," +
                building.BoundingBox.TopUpperRightCorner.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "," +
                building.BoundingBox.TopUpperRightCorner.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ")";

            dbCommand.ExecuteNonQuery();

            this.GroundSurfacePointToDatabase(building.GetGroundSurfacePoints(), building.BuildingID);
        }
        else
        {
            throw new ArgumentException("Invalid Building:\n" + building.ToString());
        }
    }


    private void GroundSurfacePointToDatabase(List<double3> pointList, int buildingID)
    {
        foreach (double3 point in pointList)
        {
            this.GroundSurfacePointToDatabase(point, buildingID);
        }
    }



    private void GroundSurfacePointToDatabase(double3 point, int buildingID)
    {
        IDbCommand dbCommand;
        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = "INSERT INTO " + TableNames.GroundSurfacePoint + "(" +
            TableNames.Building_BuildingID + ", " +
            TableNames.Point_X + ", " +
            TableNames.Point_Y + ", " +
            TableNames.Point_Z + ")\n" +
            "VALUES(" +
            buildingID + ", " +
            point.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "," +
            point.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "," +
            point.z.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ")";

        dbCommand.ExecuteNonQuery();
    }


    #region BuildingAnnotation

    public void BuildingAnnotationToDatabase(List<BuildingAnnotation> buildingAnnotations)
    {
        this.Execute("BEGIN TRANSACTION");

        foreach (BuildingAnnotation buildingAnnotation in buildingAnnotations)
        {
            this.BuildingAnnotationToDatabase(buildingAnnotation);
        }

        this.Execute("COMMIT TRANSACTION");
    }


    public void BuildingAnnotationToDatabase(BuildingAnnotation buildingAnnotation)
    {
        IDbCommand dbCommand;
        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = "INSERT INTO " + TableNames.BuildingAnnotation + "(" +
            TableNames.Building_BuildingID + ", " +
            TableNames.Annotation_ComponentID + ", " +
            TableNames.Annotation_Text + ", " + 
            TableNames.Annotation_LocalScale + ")\n" +
            "VALUES(" +
            buildingAnnotation.BuildingID + ", " +
            buildingAnnotation.ComponentID + ", " +
            "\"" + buildingAnnotation.AnnotationText + "\", " +
            buildingAnnotation.LocalScale.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + 
            ")";


        Debug.Log(dbCommand.CommandText);


        dbCommand.ExecuteNonQuery();
    }

    public List<BuildingAnnotation> GetBuildingAnnotation(BoundingBox boundingBox)
    {
        List<BuildingAnnotation> buildingAnnotations = new List<BuildingAnnotation>();

        using (IDbCommand buildingAnnotationQuery = dbConnection.CreateCommand())
        {
            IDataReader dataReader;

            string buildingsWithAnnotations = "BuildingsWithAnnotations";
            string groundSurfaceLevel = "GroundSurfaceLevel";
            string selectedBuildings = "SelectedBuildings";

            buildingAnnotationQuery.CommandText =
                " SELECT" +
                "*" +
                "FROM (" +
                "SELECT " +
                buildingsWithAnnotations + "." + TableNames.Building_BuildingID + ", " + 
                TableNames.Building_MeasuredHeight + ", " + 
                TableNames.Building_boundingBox_LowerLeftCorner_X + ", " + 
                TableNames.Building_boundingBox_LowerLeftCorner_Y + ", " + 
                TableNames.Building_boundingBox_UpperRightCorner_X + ", " + 
                TableNames.Building_boundingBox_UpperRightCorner_Y + ", " + 
                "MAX(" + TableNames.Point_Z + ") AS " + groundSurfaceLevel + ", " +
                TableNames.Annotation_ComponentID + ", " +
                TableNames.Annotation_Text + ", " +
                TableNames.Annotation_LocalScale + "\n" +
                " FROM " + TableNames.GroundSurfacePoint + "\n" +
                " INNER JOIN ( " +
                " SELECT " +
                selectedBuildings + "." + TableNames.Building_BuildingID + ", " +
                TableNames.Building_MeasuredHeight + ", " +
                TableNames.Building_boundingBox_LowerLeftCorner_X + ", " +
                TableNames.Building_boundingBox_LowerLeftCorner_Y + ", " +
                TableNames.Building_boundingBox_UpperRightCorner_X + ", " +
                TableNames.Building_boundingBox_UpperRightCorner_Y + ", " +
                TableNames.Annotation_ComponentID + ", " +
                TableNames.Annotation_Text + ", " +
                TableNames.Annotation_LocalScale + "\n" +
                " FROM " + TableNames.BuildingAnnotation + "\n" +
                " INNER JOIN ( " +
                " SELECT " +
                TableNames.Building_BuildingID + ", " +
                TableNames.Building_MeasuredHeight + ", " +
                TableNames.Building_boundingBox_LowerLeftCorner_X + ", " +
                TableNames.Building_boundingBox_LowerLeftCorner_Y + ", " +
                TableNames.Building_boundingBox_UpperRightCorner_X + ", " +
                TableNames.Building_boundingBox_UpperRightCorner_Y + "\n" +
                " FROM " + TableNames.Building + " " +
                " WHERE " + TableNames.Building_boundingBox_LowerLeftCorner_X + " >= " + boundingBox.ButtomLowerLeftCorner.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Building_boundingBox_LowerLeftCorner_Y + " >= " + boundingBox.ButtomLowerLeftCorner.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Building_boundingBox_UpperRightCorner_X + " <= " + boundingBox.TopUpperRightCorner.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Building_boundingBox_UpperRightCorner_Y + " <= " + boundingBox.TopUpperRightCorner.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                ") AS " + selectedBuildings + " ON " + selectedBuildings+ "." + TableNames.Building_BuildingID + " = " + TableNames.BuildingAnnotation + "." + TableNames.Building_BuildingID +
            ") AS " + buildingsWithAnnotations + " ON " + buildingsWithAnnotations + "." + TableNames.Building_BuildingID + " = " + TableNames.GroundSurfacePoint + "." + TableNames.Building_BuildingID
            + ")";

            Debug.Log(buildingAnnotationQuery.CommandText);

            dataReader = buildingAnnotationQuery.ExecuteReader();

            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": finishes query, parsing results");

            while (dataReader.Read())
            {
                int.TryParse(dataReader[0].ToString(), out int buildingID);
                double.TryParse(dataReader[1].ToString(), out double buildingHeight);
                double.TryParse(dataReader[2].ToString(), out double Building_boundingBox_LowerLeftCorner_X);
                double.TryParse(dataReader[3].ToString(), out double Building_boundingBox_LowerLeftCorner_Y);
                double.TryParse(dataReader[4].ToString(), out double Building_boundingBox_UpperRightCorner_X);
                double.TryParse(dataReader[5].ToString(), out double Building_boundingBox_UpperRightCorner_Y);
                double.TryParse(dataReader[6].ToString(), out double groundSurfaceHeightLevel);
                int.TryParse(dataReader[7].ToString(), out int componentID);
                string annotationText = dataReader[8].ToString();
                float.TryParse(dataReader[9].ToString(), out float localScale);

                BoundingBox buildingBoundingBox = new BoundingBox(
                    new double3(Building_boundingBox_LowerLeftCorner_X, Building_boundingBox_LowerLeftCorner_Y, 0),
                    new double3(Building_boundingBox_UpperRightCorner_X, Building_boundingBox_UpperRightCorner_Y, 0));

                buildingAnnotations.Add(new BuildingAnnotation(buildingID, buildingBoundingBox, buildingHeight + groundSurfaceHeightLevel, componentID, annotationText, localScale));
            }
        }

        return buildingAnnotations;
    }



    #endregion


    #region SurfaceAnnotation

    public void SurfaceAnnotationToDatabase(List<SurfaceAnnotation> surfaceAnnotations)
    {
        this.Execute("BEGIN TRANSACTION");

        foreach (SurfaceAnnotation surfaceAnnotation in surfaceAnnotations)
        {
            this.SurfaceAnnotationToDatabase(surfaceAnnotation);
        }

        this.Execute("COMMIT TRANSACTION");
    }

    public void SurfaceAnnotationToDatabase(SurfaceAnnotation surfaceAnnotation)
    {
        IDbCommand dbCommand;
        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = "INSERT INTO " + TableNames.SurfaceAnnotation + "(" +
            TableNames.Building_BuildingID + ", " +
            TableNames.Point_X + "1, " +
            TableNames.Point_Y + "1, " +
            TableNames.Point_Z + "1, " +
            TableNames.Point_X + "2, " +
            TableNames.Point_Y + "2, " +
            TableNames.Point_Z + "2, " +
            TableNames.Annotation_ComponentID + ", " +
            TableNames.SurfaceAnnotation_RelativeGroundSurfacePosition + ", " +
            TableNames.SurfaceAnnotation_Height + ", " +
            TableNames.Annotation_Text + ", " +
            TableNames.Annotation_LocalScale + ")\n" +
            "VALUES(" +
            surfaceAnnotation.BuildingID + ", " +
            surfaceAnnotation.GroundSurfacePoints[0].x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            surfaceAnnotation.GroundSurfacePoints[0].y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            surfaceAnnotation.GroundSurfacePoints[0].z.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            surfaceAnnotation.GroundSurfacePoints[1].x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            surfaceAnnotation.GroundSurfacePoints[1].y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            surfaceAnnotation.GroundSurfacePoints[1].z.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            surfaceAnnotation.ComponentID + ", " +
            surfaceAnnotation.RelativeGroundSurfacePosition.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            surfaceAnnotation.Height.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            "\"" + surfaceAnnotation.AnnotationText + "\", " +
            surfaceAnnotation.LocalScale.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
            ")";

        Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();
    }



    public List<SurfaceAnnotation> GetSurfaceAnnotation(BoundingBox boundingBox)
    {
        List<SurfaceAnnotation> surfaceAnnotations = new List<SurfaceAnnotation>();

        using (IDbCommand buildingAnnotationQuery = dbConnection.CreateCommand())
        {
            IDataReader dataReader;

            buildingAnnotationQuery.CommandText =
                " SELECT " +
                TableNames.Building_BuildingID + ", " +
                TableNames.Point_X + "1, " +
                TableNames.Point_Y + "1, " +
                TableNames.Point_Z + "1, " +
                TableNames.Point_X + "2, " +
                TableNames.Point_Y + "2, " +
                TableNames.Point_Z + "2, " +
                TableNames.Annotation_ComponentID + ", " +
                TableNames.SurfaceAnnotation_RelativeGroundSurfacePosition + ", " +
                TableNames.SurfaceAnnotation_Height + ", " +
                TableNames.Annotation_Text + ", " +
                TableNames.Annotation_LocalScale + "\n" +
                " FROM " + TableNames.SurfaceAnnotation + "\n" +
                " WHERE " + TableNames.Point_X + "1 >= " + boundingBox.ButtomLowerLeftCorner.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Point_Y + "1 >= " + boundingBox.ButtomLowerLeftCorner.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Point_X + "2 <= " + boundingBox.TopUpperRightCorner.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Point_Y + "2 <= " + boundingBox.TopUpperRightCorner.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);

            Debug.Log(buildingAnnotationQuery.CommandText);

            dataReader = buildingAnnotationQuery.ExecuteReader();

            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": finishes query, parsing results");

            while (dataReader.Read())
            {
                int.TryParse(dataReader[0].ToString(), out int buildingID);
                double.TryParse(dataReader[1].ToString(), out double X1);
                double.TryParse(dataReader[2].ToString(), out double Y1);
                double.TryParse(dataReader[3].ToString(), out double Z1);
                double.TryParse(dataReader[4].ToString(), out double X2);
                double.TryParse(dataReader[5].ToString(), out double Y2);
                double.TryParse(dataReader[6].ToString(), out double Z2);
                int.TryParse(dataReader[7].ToString(), out int componentID);
                double.TryParse(dataReader[8].ToString(), out double relativeGroundSurfacePosition);
                double.TryParse(dataReader[9].ToString(), out double height);
                string annotationText = dataReader[10].ToString();
                float.TryParse(dataReader[11].ToString(), out float localScale);


                surfaceAnnotations.Add(new SurfaceAnnotation(buildingID, new double3[] { new double3(X1, Y1, Z1), new double3(X2, Y2, Z2)}, relativeGroundSurfacePosition, height, componentID, annotationText, localScale));
            }
        }

        return surfaceAnnotations;
    }

    #endregion



    #region FreeWorldAnnotation

    public void FreeWorldAnnotationToDatabase(List<FreeWorldAnnotation> freeWorldAnnotations)
    {
        this.Execute("BEGIN TRANSACTION");

        foreach (FreeWorldAnnotation freeWorldAnnotation in freeWorldAnnotations)
        {
            this.FreeWorldAnnotationToDatabase(freeWorldAnnotation);
        }

        this.Execute("COMMIT TRANSACTION");
    }

    public void FreeWorldAnnotationToDatabase(FreeWorldAnnotation freeWorldAnnotation)
    {
        IDbCommand dbCommand;
        dbCommand = dbConnection.CreateCommand();

        dbCommand.CommandText = "INSERT INTO " + TableNames.FreeWorldAnnotation + "(" +
            TableNames.Point_X + ", " +
            TableNames.Point_Y + ", " +
            TableNames.Point_Z + ", " +
            TableNames.Annotation_ComponentID + ", " +
            TableNames.Annotation_Text + ", " +
            TableNames.Annotation_LocalScale + ")\n" +
            "VALUES(" +
            freeWorldAnnotation.RealWorldCoordinate.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            freeWorldAnnotation.RealWorldCoordinate.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            freeWorldAnnotation.RealWorldCoordinate.z.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            freeWorldAnnotation.ComponentID + ", " +
            "\"" + freeWorldAnnotation.AnnotationText + "\", " +
            freeWorldAnnotation.LocalScale.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
            ")";

        Debug.Log(dbCommand.CommandText);

        dbCommand.ExecuteNonQuery();
    }







    public List<FreeWorldAnnotation> GetFreeWorldAnnotation(BoundingBox boundingBox)
    {
        List<FreeWorldAnnotation> freeWorldAnnotations = new List<FreeWorldAnnotation>();

        using (IDbCommand freeWorldAnnotationQuery = dbConnection.CreateCommand())
        {
            IDataReader dataReader;

            freeWorldAnnotationQuery.CommandText =
                " SELECT " +
                TableNames.Point_X + ", " +
                TableNames.Point_Y + ", " +
                TableNames.Point_Z + ", " +
                TableNames.Annotation_ComponentID + ", " +
                TableNames.Annotation_Text + ", " +
                TableNames.Annotation_LocalScale + "\n" +
                " FROM " + TableNames.FreeWorldAnnotation + "\n" +
                " WHERE " + TableNames.Point_X + " >= " + boundingBox.ButtomLowerLeftCorner.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Point_Y + " >= " + boundingBox.ButtomLowerLeftCorner.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Point_X + " <= " + boundingBox.TopUpperRightCorner.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Point_Y + " <= " + boundingBox.TopUpperRightCorner.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);

            Debug.Log(freeWorldAnnotationQuery.CommandText);

            dataReader = freeWorldAnnotationQuery.ExecuteReader();

            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": finishes query, parsing results");

            while (dataReader.Read())
            {
                double.TryParse(dataReader[0].ToString(), out double X);
                double.TryParse(dataReader[1].ToString(), out double Y);
                double.TryParse(dataReader[2].ToString(), out double Z);
                int.TryParse(dataReader[3].ToString(), out int componentID);
                string annotationText = dataReader[4].ToString();
                float.TryParse(dataReader[5].ToString(), out float localScale);

                freeWorldAnnotations.Add(new FreeWorldAnnotation(new double3(X, Y, Z), componentID, annotationText, localScale));
            }
        }

        return freeWorldAnnotations;
    }


    #endregion






    public List<Building> GetBuildings(BoundingBox boundingBox)
    {
        Dictionary<int, Building> buildings = new Dictionary<int, Building>();

        using (IDbCommand buildingQuery = dbConnection.CreateCommand())
        {
            IDataReader dataReader;

            buildingQuery.CommandText =
                " SELECT " + TableNames.GroundSurfacePoint + "." + TableNames.Building_BuildingID + ", " + TableNames.Building_CityGMLID + ", " + TableNames.Building_MeasuredHeight + ", " + TableNames.Point_X + ", " + TableNames.Point_Y + ", " + TableNames.Point_Z + " " +
                " FROM " + TableNames.GroundSurfacePoint + " " +
                " INNER JOIN ( " +
                " SELECT " + TableNames.Building_BuildingID + ", " + TableNames.Building_CityGMLID + ", " + TableNames.Building_MeasuredHeight + " " +
                " FROM " + TableNames.Building + " " +
                " WHERE " + TableNames.Building_boundingBox_LowerLeftCorner_X + " >= " + boundingBox.ButtomLowerLeftCorner.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Building_boundingBox_LowerLeftCorner_Y + " >= " + boundingBox.ButtomLowerLeftCorner.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Building_boundingBox_UpperRightCorner_X + " <= " + boundingBox.TopUpperRightCorner.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                " AND " + TableNames.Building_boundingBox_UpperRightCorner_Y + " <= " + boundingBox.TopUpperRightCorner.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) +
                ") AS SelectedBuildings ON SelectedBuildings." + TableNames.Building_BuildingID + " = " + TableNames.GroundSurfacePoint + "." + TableNames.Building_BuildingID;

            Debug.Log(buildingQuery.CommandText);

            dataReader = buildingQuery.ExecuteReader();

            Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": finishes query, parsing results");

            while (dataReader.Read())
            {
                int.TryParse(dataReader[0].ToString(), out int buildingID);
                string cityGMLID = dataReader[1].ToString();
                float.TryParse(dataReader[2].ToString(), out float measuredHeight);
                double.TryParse(dataReader[3].ToString(), out double x);
                double.TryParse(dataReader[4].ToString(), out double y);
                double.TryParse(dataReader[5].ToString(), out double z);


                if (!buildings.ContainsKey(buildingID))
                {
                    Building newBuilding = new Building(cityGMLID, measuredHeight, buildingID);
                    buildings.Add(buildingID, newBuilding);
                }

                buildings[buildingID].AddGroundSurfacePoint(new double3(x, y, z));
            }
        }

        Debug.Log(MyTimer.GetSecondsSiceStartAsString() + ": finishes parsing, transforming Dictionary to List");

        return new List<Building>(buildings.Values);
    }


    private List<double3> GetGroundSurfacePointsFromTable(Building building)
    {
        using (IDbCommand groundSurfacePointQuery = dbConnection.CreateCommand())
        {
            List<double3> groundSurfacePoints = new List<double3>();
            IDataReader dataReader;

            groundSurfacePointQuery.CommandText =
                "SELECT " + TableNames.Point_X + ", " + TableNames.Point_Y + ", " + TableNames.Point_Z + " " +
                "FROM " + TableNames.GroundSurfacePoint + " " +
                "WHERE(" + TableNames.Building_BuildingID + " == " + building.BuildingID + ")";

            //Debug.Log(groundSurfacePointQuery.CommandText);

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
}
