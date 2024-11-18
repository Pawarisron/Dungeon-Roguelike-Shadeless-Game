using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Data : MonoBehaviour
{
    public List<Transform> targets = null;
    public Collider2D[] obsticles = null;

    public Transform currentTarget;

    public int GetTargetsCount() => targets == null ? 0 : targets.Count;
}
