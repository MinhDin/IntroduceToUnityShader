using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct WaterNodeData
{
	public float Velocity;
	public float LeftHeight;
	public float RightHeight;
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WaterDistord : MonoBehaviour 
{
	[Header("Setting")]
	public int Density = 100;
	public float Stiffness = 0.3f;
	public float DampeningFactor = 0.025f;
	public int NeighbourPass = 8;
	public float SpreadFactor = 0.2f;	
	public Color32 EdgeColor;
	public Color32 BtmColor;
	//public float EdgeThick = 0.1f;
	

	[Header("Misc")]
	public Material WaterMat;	
	[Tooltip("If water goes beyond 1.0f , it will go beyond camera")]
	public float WaterPercent = 0.65f;

	float targetHeight = 0.5f;
	Mesh mesh;
	float step;

	List<Vector3> vertices;
	List<int> indices;
	List<Vector2> uvs;
	List<Color32> colors;
	MeshRenderer render;
	WaterNodeData[] nodeData;

	void Awake()
	{
		vertices = new List<Vector3>();
		indices = new List<int>();
		uvs = new List<Vector2>();
		colors = new List<Color32>();
		nodeData = new WaterNodeData[Density];
		targetHeight = WaterPercent - 0.5f;

		mesh = new Mesh();
		mesh.MarkDynamic();
		
		GetComponent<MeshFilter>().mesh = mesh;
		render = GetComponent<MeshRenderer>();
		//WaterMat = new Material(WaterMat);
		render.sortingLayerName = "Foreground";
		render.sortingOrder = 10000;
		step = 1.0f / (Density - 1);
		//renderTex = new RenderTexture((int)(transform.localScale.x * 100), (int)(transform.localScale.y * 100), 24, RenderTextureFormat.ARGB32);
		//cam.orthographicSize = transform.localScale.y / 2;		
		//cam.aspect  = transform.localScale.x / transform.localScale.y;
		//cam.targetTexture = renderTex;

		//WaterMat.mainTexture = renderTex;
		//render.material = WaterMat;
	}

	void Start () 
	{
		ConstructVertex();
	}

	#if UNITY_EDITOR
	void OnValidate()
	{
		if(!Application.isPlaying && nodeData != null)
		{
			ConstructVertex();
		}
	}
	#endif

	void ConstructVertex()
	{
		vertices.Clear();
		colors.Clear();
		uvs.Clear();
		indices.Clear();
		
		//water edge first
		for(int i = 0; i < Density; ++i)
		{
			vertices.Add(new Vector3(-0.5f + (i * step), 0.5f, 0));
			uvs.Add(new Vector2(i * step, 1));
			colors.Add(EdgeColor);			
		}

		//btm
		for(int i = 0; i < Density; ++i)
		{
			vertices.Add(new Vector3(-0.5f + (i * step), -0.5f, 0));
			uvs.Add(new Vector2(i * step, 0));
			colors.Add(BtmColor);
		}

		//indices
		for(int i = 0; i < Density - 1; ++i)
		{
			indices.Add(i);			
			indices.Add(Density + i + 1);
			indices.Add(Density + i);
			
			indices.Add(i);
			indices.Add(i + 1);
			indices.Add(Density + i + 1);
		}
		
		mesh.SetVertices(vertices);
		mesh.SetUVs(0, uvs);
		mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
		mesh.SetColors(colors);
		
		//node data
		for(int i = 0; i < Density; ++i)
		{
			nodeData[i].Velocity = 0;
			nodeData[i].LeftHeight = 0;
			nodeData[i].RightHeight = 0;
		}

		for(int i = 0; i < Density; ++i)
		{
			SetNodeHeight(i, targetHeight);
		}
	}

	void Update () 
	{
		/*
		#if UNITY_EDITOR
		if(!Application.isPlaying)
		{
			if((renderTex.width != (int)(transform.localScale.x * 100)) ||
				(renderTex.height != (int)(transform.localScale.y * 100)))
				{
					renderTex = new RenderTexture((int)(transform.localScale.x * 100), (int)(transform.localScale.y * 100), 24, RenderTextureFormat.ARGB32);
					cam.targetTexture = renderTex;
					WaterMat.mainTexture = renderTex;
				}			
			cam.orthographicSize = transform.localScale.y / 2;		
			cam.aspect  = transform.localScale.x / transform.localScale.y;
			cam.Render();
		}
		#endif
		*/
		//physic
		for(int i = 0; i < Density; ++i)
		{
			float x = vertices[i].y - targetHeight;
			float acceleration = -Stiffness * x - DampeningFactor * nodeData[i].Velocity;

			nodeData[i].Velocity += acceleration;
			SetNodeHeight(i, vertices[i].y + nodeData[i].Velocity);
		}

		//neighbours		
		for(int n = 0; n < NeighbourPass; ++n)
		{
			for(int i = 0; i < Density; ++i)
			{
				if(i > 0)
				{
					nodeData[i].LeftHeight = SpreadFactor * (vertices[i].y - vertices[i - 1].y);
					nodeData[i - 1].Velocity += nodeData[i].LeftHeight;					
				}

				if(i < Density - 1)
				{
					nodeData[i].RightHeight = SpreadFactor * (vertices[i].y - vertices[i + 1].y);
					nodeData[i + 1].Velocity += nodeData[i].RightHeight;
				}
			}

			for(int i = 0; i < Density; ++i)
			{
				if(i > 0)
				{
					SetNodeHeight(i - 1, vertices[i - 1].y + nodeData[i].LeftHeight);
				}

				if(i < Density - 1)
				{
					SetNodeHeight(i + 1, vertices[i + 1].y + nodeData[i].RightHeight);
				}
			}
		}

		//reapply mesh
		mesh.SetVertices(vertices);
		mesh.SetUVs(0, uvs);

		//splash
		if(Input.GetMouseButtonDown(0))
		{
			Splash(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, -1);
		}
	}

	void SetNodeHeight(int index, float y)
	{
		vertices[index] = new Vector3(vertices[index].x, y, vertices[index].z);
		uvs[index] = new Vector2(uvs[index].x, (y + 0.5f));
	}

	public void Splash(float position, float speed)
	{
		position = (position - (transform.position.x -(transform.localScale.x / 2.0f))) / transform.localScale.x;
		if((position >= 0) && (position <= 1))
		{
			nodeData[Mathf.RoundToInt(position / step)].Velocity = speed;
		}
	}
}
