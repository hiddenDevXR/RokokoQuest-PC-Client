using System;
using System.Collections;
using System.Collections.Generic;
using Rokoko.Inputs;
using UnityEngine;
using UnityEditor;

public class DancerManager : EditorWindow
{
    public GameObject[] actors;
    private string profileName = "Add profile name";
    
    [MenuItem("Tools/Dancer Editor")]
    public static void ShowWindow()
    {
        GetWindow<DancerManager>("Dancer Editor");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Dancer editor", EditorStyles.boldLabel);
        profileName = EditorGUILayout.TextField("Profile Name", profileName);
        
        if (GUILayout.Button("Apply"))
        {
            actors = Selection.gameObjects;
            foreach (var actor in actors)
            {
                actor.GetComponent<Actor>().profileName = profileName;
            }
        }
    }
}
