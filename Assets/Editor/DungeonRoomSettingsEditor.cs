using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonRoomSettings))]
public class DungeonRoomSettingsEditor : Editor
{
    SerializedProperty IsSpawnEnemies;
    SerializedProperty EnemySpawnDatas;
    
    SerializedProperty IsSpawnTreasure;
    SerializedProperty TreasurePrefabs;

    SerializedProperty IsSpawnTraps;
    SerializedProperty TrapSpawnDatas;


    private void OnEnable()
    {
        IsSpawnEnemies = serializedObject.FindProperty("IsSpawnEnemies");
        EnemySpawnDatas = serializedObject.FindProperty("EnemySpawnDatas");

        IsSpawnTreasure = serializedObject.FindProperty("IsSpawnTreasure");
        TreasurePrefabs = serializedObject.FindProperty("TreasurePrefabs");

        IsSpawnTraps = serializedObject.FindProperty("IsSpawnTraps");
        TrapSpawnDatas = serializedObject.FindProperty("TrapSpawnDatas");

    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        //Hide EnemySpawnDatas depend on boolean
        if (IsSpawnEnemies.boolValue)
        {
            EditorGUILayout.PropertyField(EnemySpawnDatas, true);
        }
        //Hide TreasurePrefabs depend on boolean
        if (IsSpawnTreasure.boolValue)
        {
            EditorGUILayout.PropertyField(TreasurePrefabs, true);
        }
        //Hide TrapSpawnDatas depend on boolean
        if (IsSpawnTraps.boolValue)
        {
            EditorGUILayout.PropertyField(TrapSpawnDatas, true);
        }



        serializedObject.ApplyModifiedProperties();
    }
}
