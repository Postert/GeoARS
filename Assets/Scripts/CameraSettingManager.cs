using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class CameraSettingManager : MonoBehaviour
{
    List<BuildingManager> BuildingManagers = new List<BuildingManager>();

    private void Awake()
    {
        //BuildingManagers.Add(GameObject.Find("BuildingManagementGameObjectLoD1").GetComponent<BuildingManager>());
        BuildingManagers.Add(GameObject.Find("BuildingManagementGameObjectLoD2").GetComponent<BuildingManager>());
    }

    public void OnIncreaseFieldOfViewClick()
    {
        Camera.main.fieldOfView += 0.1f;
    }

    public void OnDecreaseFieldOfViewClick()
    {
        Camera.main.fieldOfView -= 0.1f;
    }


    public void OnSwitchBuildingMaterialClick()
    {
        /*
        foreach (BuildingManager buildingManager in BuildingManagers)
        {
            buildingManager.IterateMaterials();
        }
        */
    }

    
    public void OnSwitchLevelOfDetailClick()
    {
        
    }    
}
