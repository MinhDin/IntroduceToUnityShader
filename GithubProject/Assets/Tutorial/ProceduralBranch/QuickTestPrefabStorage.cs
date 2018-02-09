using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class QuickTestPrefabStorage : MonoBehaviour 
{
	public static QuickTestPrefabStorage Instance;
	public List<GameObject> Leaves;
	public List<GameObject> Flowers;

	void Awake()
	{
		
	}

	void Start () 
	{
		
	}
	
	void Update ()
	{
		Instance = this;
	}

	public GameObject GetLeaf()
	{
		return GameObject.Instantiate(Leaves[Random.Range(0, Leaves.Count)]);
	}

	public GameObject GetFlower()
	{
		return GameObject.Instantiate(Flowers[Random.Range(0, Flowers.Count)]);
	}
}
