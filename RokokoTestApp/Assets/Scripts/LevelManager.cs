using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    
    public static Transform _player;

    public Transform rotationPoint;
    public GameObject levelMesh;
    private GameObject oculusUserAvatar;
    
    public void ParentToPivot()
    {
        if (rotationPoint == null) return;

   
        transform.SetParent(rotationPoint);
        oculusUserAvatar = GameObject.Find("Network Player_Dance(Clone)")?.transform.GetChild(0).gameObject;
        if (oculusUserAvatar != null)
        {
            oculusUserAvatar.transform.SetParent(rotationPoint);
        }
    }

    public void UnparentFromPivot()
    {
 
        transform.SetParent(null);
        if (oculusUserAvatar != null)
        {
            oculusUserAvatar.transform.SetParent(null);
        }
    }

    public void EnableMesh(bool state)
    {
        levelMesh.SetActive(state);
    }
}
