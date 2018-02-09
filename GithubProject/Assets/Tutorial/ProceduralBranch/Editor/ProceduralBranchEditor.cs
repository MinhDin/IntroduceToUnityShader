using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralBranch))]
public class ProceduralBranchEditor : Editor
{
	ProceduralBranch branch;
	QuickTestPrefabStorage storage;
	static PrespawnedBranchDef def;
	static Vector2 size;
	static float branchWidthPercent;
	static string baseName;
	static int genAmount;

	void OnEnable()
	{
		branch = (ProceduralBranch)target;
		storage = GameObject.Find("Storage").GetComponent<QuickTestPrefabStorage>();
	}
	
	void OnSceneGUI()
	{
		branch = (ProceduralBranch)target;
		Handles.BeginGUI();
		def = EditorGUILayout.ObjectField("Prespawn Data", def, typeof(PrespawnedBranchDef), false) as PrespawnedBranchDef;
		size = EditorGUILayout.Vector2Field("Branch Size", size);
		branchWidthPercent = EditorGUILayout.FloatField("Branch Width Percent", branchWidthPercent);
		baseName = EditorGUILayout.TextField("Base Name", baseName);
		genAmount = EditorGUILayout.IntField("Generate Amount", genAmount);

		GUI.backgroundColor = Color.green;
		if(GUI.Button(new Rect(50, 120, 100, 50), "Generate"))
		{			
			branch.GenerateDebug(size, branchWidthPercent);
		}
		if(GUI.Button(new Rect(50, 170, 150, 50), "GenerateAndSave"))
		{			
			GenerateMany(size, branchWidthPercent, baseName, genAmount);
		}
		GUI.backgroundColor = Color.red;
		if(GUI.Button(new Rect(65, 220, 70, 20), "CleanUp"))
		{
			branch.CleanUp();
		}
		GUI.backgroundColor = Color.blue;
		if(GUI.Button(new Rect(50, 240, 100, 50), "Save"))
		{
			SaveNewBranch();
		}		
		
		if(GUI.Button(new Rect(50, 290, 100, 50), "Grow"))
		{
			if(def != null && storage != null)
			{
				branch.EditorInit();
				storage.StartCoroutine(GrowPrespawnedBranch(def));
			}
		}
		Handles.EndGUI();
	}

	void GenerateMany(Vector2 size, float branchWidth, string baseName, int amount)
	{
		for(int i = 0; i < amount; ++i)
		{
			branch.GenerateDebug(size, branchWidth);
			SaveNewBranch(baseName + "_" + (i + 1).ToString());
		}
	}

	void SaveNewBranch(string name = "NewPrespawnedBranch")
	{
		PrespawnedBranchDef asset = ScriptableObject.CreateInstance<PrespawnedBranchDef>();

		string path = "Assets/Tutorial/ProceduralBranch/Branches";
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".asset");
		//set data
		asset.Vertices = branch.BranchMeshData.Vertices;
		asset.Indices = branch.BranchMeshData.Indices.ToArray();
		asset.Uvs = branch.BranchMeshData.Uvs;
		asset.ParentID = branch.BranchMeshData.ParentID;
		asset.GrowStep = branch.BranchMeshData.GrowStep;
		asset.Leaves = branch.BranchMeshData.Leaves;
		asset.Flowers = branch.BranchMeshData.Flowers;
		//==============================
        AssetDatabase.CreateAsset(asset, assetPathAndName);
		
        AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();
        //EditorUtility.FocusProjectWindow();		
        Selection.activeObject = asset;
	}

	public IEnumerator GrowPrespawnedBranch(PrespawnedBranchDef def)
	{
		List<Vector3> vertices = new List<Vector3>();
		vertices.AddRange(def.Vertices);
		int growStep = 1;
		int maxStep = -1;

		for(int i = 0; i < def.GrowStep.Count; ++i)
		{
			if(def.GrowStep[i] > growStep)
			{
				vertices[i] = Vector3.zero;
			}

			maxStep = Mathf.Max(maxStep, def.GrowStep[i]);
		}

		SetMeshData(vertices);
		yield return null;
		for(int i = growStep; i <= maxStep; ++i)
		{
			float counter = 0;
			float speed = 200;
			while(counter < 1.0f)
			{
				for(int j = 0; j < def.GrowStep.Count; ++j)
				{
					if(def.GrowStep[j] == i)
					{
						Vector3 parent = def.Vertices[def.ParentID[j]];
						Vector3 mine = def.Vertices[j];

						vertices[j] = Vector3.Lerp(parent, mine, counter);
					}
				}

				counter += Time.deltaTime * speed;
				SetMeshData(vertices);
				yield return null;
			}

			for(int j = 0; j < def.GrowStep.Count; ++j)
			{
				if(def.GrowStep[j] == i)
				{
					vertices[j] = def.Vertices[j];
				}
			}
		}
		SetMeshData(vertices);
	}

	void SetMeshData(List<Vector3> vertices)
	{
		branch.BranchMesh.SetVertices(vertices);
		//branch.BranchMesh.SetColors(def.);
		branch.BranchMesh.SetIndices(def.Indices, MeshTopology.Triangles, 0);
		branch.BranchMesh.SetUVs(0, def.Uvs);
		branch.BranchMesh.RecalculateBounds();	
	}
	
	
}
