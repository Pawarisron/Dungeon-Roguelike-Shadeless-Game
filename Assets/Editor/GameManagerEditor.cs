using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default Inspector interface for GameManager
        DrawDefaultInspector();

        // Reference the GameManager instance
        GameManager gameManager = (GameManager)target;

        // Button to start the game
        if (GUILayout.Button("Start Game"))
        {
            gameManager.StartGame();
        }

        // Button to pause the game
        if (GUILayout.Button("Pause Game"))
        {
            gameManager.PauseGame();
        }

        // Button to resume the game
        if (GUILayout.Button("Resume Game"))
        {
            gameManager.ResumeGame();
        }
        // Button Main Menu
        if (GUILayout.Button("Main Menu"))
        {
            gameManager.MainMenu();
        }
        // Button Restart Game
        if (GUILayout.Button("Restart Game"))
        {
            gameManager.RestartGame();
        }
    }
}
