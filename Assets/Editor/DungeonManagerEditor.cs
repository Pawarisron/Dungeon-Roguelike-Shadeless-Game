using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(DungeonManager))]
public class DungeonManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        DungeonManager dungeonManager = (DungeonManager)target;

        if (GUILayout.Button("Load Next Scene"))
        {
            dungeonManager.LoadNextScene();
        }

        if (GUILayout.Button("Load Previous Scene"))
        {
            dungeonManager.LoadPreviousScene();
        }

        if (GUILayout.Button("Reload Current Scene"))
        {
            dungeonManager.ReloadCurrentScene();
        }

        if (GUILayout.Button("Exit Level"))
        {
            dungeonManager.ExitLevel();
        }

        
    }
}
