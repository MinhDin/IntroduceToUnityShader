using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PrespawnedBranchDef : ScriptableObject
{
    public List<Vector3> Vertices;
    public int[] Indices;
    public List<Vector2> Uvs;
    public List<int> ParentID;
    public List<int> GrowStep;
    public List<TransformData> Leaves;
    public List<TransformData> Flowers;
#if UNITY_EDITOR
    [MenuItem("Games/Create/PrespawnedBranchDef")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<PrespawnedBranchDef>();
    }
#endif
}

[System.Serializable]
public struct TransformData
{
    public Vector3 Position;
    public float RotateZ;
    public int Step;
}
