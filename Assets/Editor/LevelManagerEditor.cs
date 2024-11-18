using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default Inspector interface
        DrawDefaultInspector();

        // Reference the levelManager instance
        LevelManager levelManager = (LevelManager)target;

        // Button to start the game
        if (GUILayout.Button("Start Dungeon"))
        {
            levelManager.StartDungeon();
        }

    }
}
