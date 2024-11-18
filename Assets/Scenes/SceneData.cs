using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SceneData", menuName = "ObjectData/SceneData")]
public class SceneData : ScriptableObject
{
    public string sceneName;
    [TextArea(3, 10)]
    public string description;
    public int difficultyLevel;
}
