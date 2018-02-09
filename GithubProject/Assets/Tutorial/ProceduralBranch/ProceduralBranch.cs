using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class BranchData
{
	Vector2 startPos1;
	Vector2 startPos2;
	Vector2 endPos1;
	Vector2 endPos2;
	int startIndex;
	int endIndex;
	int step;

	public List<BranchData> subBranches;
	public List<GameObject> leaves;
	public List<GameObject> flowers;

	MeshData mesh;
	ProceduralBranch root;
	int lastFlowerGrow;
	int lastLeafGrow;
	float minFlowerSize;
	float maxFlowerSize;
	float minLeafSize;
	float maxLeafSize;

	float length;
	Vector2 norAngle;
	static int sideCorrector = 0;
	static int maxSide = 2;
	public BranchGenerateData GenData;
	LeafGenerateData leafData;
	float endWidth;
	bool isExpand;
	public BranchData(ProceduralBranch root)
	{
		this.root = root;
		subBranches = new List<BranchData>();
		leaves = new List<GameObject>();
		flowers = new List<GameObject>();	
		lastFlowerGrow = 0;	
		lastLeafGrow = 0;
	}

	public void GenerateBranch(BranchGenerateData data, LeafGenerateData leaf, MeshData mesh, int meshStartIndex, bool callForward = true)
	{
		startPos1 = data.StartPos1;
		startPos2 = data.StartPos2;
		this.mesh = mesh;
		step = data.Step;
		startIndex = meshStartIndex;
		norAngle = data.NorAngle;

		//caculate this branch
		
		if(data.ForceLength > 0)
		{
			length = data.ForceLength;
			endWidth = data.Width - (1 - Random.Range(data.ReduceWidthRateMin, data.ReduceWidthRateMax)) * data.Width * length;
			data.ForceLength = -1;
		}
		else
		{
			length = Random.Range(data.BranchLengthMin, data.BranchLengthMax);
			endWidth = data.Width - (1 - Random.Range(data.ReduceWidthRateMin, data.ReduceWidthRateMax)) * data.Width * length;

			if(endWidth < data.MinBranchWidth)
			{
				length = (data.Width - data.MinBranchWidth) / ((1 - Random.Range(data.ReduceWidthRateMin, data.ReduceWidthRateMax)) * data.Width);
				endWidth = data.MinBranchWidth;
			}
		}
		
		Vector2 startMiddlePoint = ((data.StartPos1 + data.StartPos2) / 2);
		Vector2 endMiddlePoint = startMiddlePoint + data.NorAngle * length;
		Vector2 dir = data.StartPos2 - data.StartPos1;
		Vector2 endDir;
		if(step == 1)
		{
			endDir = dir.normalized;
		}
		else
		{
			endDir = dir.Rotate(Random.Range(-data.RandomAngleEndBranch / 2, data.RandomAngleEndBranch / 2)).normalized;
		}
		endPos1 = endMiddlePoint - endDir * endWidth / 2;
		endPos2 = endMiddlePoint + endDir * endWidth / 2;

		//add leaves
		int numLeaves = Mathf.RoundToInt(Random.Range(leaf.LeafAmountMin, leaf.LeafAmountMax) * length);
		float stepLeaves = (1.0f - leaf.NonLeafPercent) / numLeaves;
		Vector2 dirLeft = endPos2 - startPos2;
		Vector2 dirRight = endPos1 - startPos1;
		Vector2 perpenLeft = new Vector2(-dirLeft.y, dirLeft.x);
		Vector2 perpenRight = new Vector2(dirRight.y, -dirRight.x);

		for(int i = 0; i < numLeaves; ++i)
		{
			bool side;//true = left
			if(sideCorrector >= maxSide)
			{
				side = false;
			}
			else if(sideCorrector <= -maxSide)
			{
				side = true;
			}
			else
			{
				side = Random.Range(0, 2) == 0;
			}

			if(side)
			{
				sideCorrector++;
			}
			else
			{
				sideCorrector--;
			}

			float posPercent = leaf.NonLeafPercent + Random.Range(stepLeaves * i, stepLeaves * (i + 1));


			GameObject aLeaf = QuickTestPrefabStorage.Instance.GetLeaf();


			if(side)
			{
				aLeaf.transform.localPosition = Vector2.Lerp(startPos2, endPos2, posPercent);
				aLeaf.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(perpenLeft.y, perpenLeft.x) * Mathf.Rad2Deg);
			}
			else
			{
				aLeaf.transform.localPosition = Vector2.Lerp(startPos1, endPos1, posPercent);
				aLeaf.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(perpenRight.y, perpenRight.x) * Mathf.Rad2Deg);
			}
			//float scale = Random.Range(leaf.SizeMin, leaf.SizeMax);
			minLeafSize = leaf.SizeMin;
			maxLeafSize = leaf.SizeMax;
#if UNITY_EDITOR
			if(Application.isPlaying)
			{
				aLeaf.transform.localScale = new Vector3(0, 0, 0);	
			}
#else
			aLeaf.transform.localScale = new Vector3(0, 0, 0);
#endif
			aLeaf.transform.localRotation = Quaternion.Euler(0, 0, aLeaf.transform.localRotation.eulerAngles.z + Random.Range(-10.0f, 10.0f));
			aLeaf.GetComponent<SpriteRenderer>().color = leaf.Color;
			leaves.Add(aLeaf);
#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				TransformData trans = new TransformData();
				trans.Position = aLeaf.transform.localPosition;
				trans.RotateZ = aLeaf.transform.localRotation.eulerAngles.z;
				trans.Step = step;
				mesh.Leaves.Add(trans);
				leaves.Clear();
				GameObject.DestroyImmediate(aLeaf);
			}
#endif

		}

		leaf.LeafAmountMin *= leaf.IncreaseAmountPercentPerStep;
		leaf.LeafAmountMax *= leaf.IncreaseAmountPercentPerStep;
		leaf.IncreaseAmountPercentPerStep = Mathf.Sqrt(leaf.IncreaseAmountPercentPerStep);

		//update data for subbranch
		data.StartPos1 = endPos1;
		data.StartPos2 = endPos2;
		
		int index = mesh.Vertices.Count;

#if UNITY_EDITOR
		if(Application.isPlaying)
		{
			if(step >= data.CollapseStep)
			{
				mesh.Vertices.Add(mesh.Vertices[startIndex]);
				mesh.Vertices.Add(mesh.Vertices[startIndex + 1]);
				isExpand = false;
			}
			else
			{
				mesh.Vertices.Add(endPos1);
				mesh.Vertices.Add(endPos2);
				isExpand = true;
			}
		}
		else
		{
			mesh.Vertices.Add(endPos1);
			mesh.Vertices.Add(endPos2);
			isExpand = true;
		}
#else
		if(step >= data.CollapseStep)
		{
			mesh.Vertices.Add(mesh.Vertices[startIndex]);
			mesh.Vertices.Add(mesh.Vertices[startIndex + 1]);			
			isExpand = false;
		}
		else
		{
			mesh.Vertices.Add(endPos1);
			mesh.Vertices.Add(endPos2);
			isExpand = true;
		}
#endif
		mesh.Colors.Add(mesh.BranchColor);		
		mesh.Colors.Add(mesh.BranchColor);

		if(mesh.IsLeft)
		{
			mesh.Uvs.Add(new Vector2(0, 0));
			mesh.Uvs.Add(new Vector2(1, 0));
		}
		else
		{
			mesh.Uvs.Add(new Vector2(1, 0));
			mesh.Uvs.Add(new Vector2(0, 0));
		}

		mesh.Indices.Add(startIndex);
		mesh.Indices.Add(startIndex + 1);	
		mesh.Indices.Add(index + 1);
		
		mesh.Indices.Add(startIndex);
		mesh.Indices.Add(index + 1);
		mesh.Indices.Add(index);
		
		//save state
#if UNITY_EDITOR
		if(!Application.isPlaying)
		{
			mesh.ParentID.Add(meshStartIndex);
			mesh.ParentID.Add(meshStartIndex + 1);
			mesh.GrowStep.Add(step);
			mesh.GrowStep.Add(step);
		}
#endif
		//data.Step = data.Step + 1;
		GenData = data;
		endIndex = index;		
		leafData = leaf;

		if(callForward)
		{
			SpawnChildBranch();
		}
	
	}

	public void SpawnChildBranch()
	{
		//add branch
		if(endWidth >= GenData.TwoBranchWidthThreshold)
		{
			BranchGenerateData data1 = GenData;
			BranchGenerateData data2 = GenData;
			data1.Step = step + 1;
			data2.Step = step + 1;

			//seperate to 2 subBranch
			float subBranchPercent = Random.Range(GenData.SubBranchPercentMin, GenData.SubBranchPercentMax);
			data1.Width = endWidth * subBranchPercent * GenData.MultiplyWidthWhenSeperate; 
			data2.Width = endWidth * (1 - subBranchPercent) * GenData.MultiplyWidthWhenSeperate;

			float randomAngle = (Random.Range(0, 2) - 0.5f) * 2;
			float angle1 = randomAngle * Random.Range(GenData.RandomAngleBranchMin, GenData.RandomAngleBranchMax);
			float angle2 = (-randomAngle) * Random.Range(GenData.RandomAngleBranchMin, GenData.RandomAngleBranchMax);

			float sign = Mathf.Sign(randomAngle);
			if(sign < 0)
			{
				if((GenData.CurrentAngle + angle1) < -GenData.MaxAngle)
				{
					float offset = -(GenData.CurrentAngle + angle1) - GenData.MaxAngle;
					angle1 += offset;
					angle2 += offset;
				}
				else if((GenData.CurrentAngle + angle2) > GenData.MaxAngle)
				{
					float offset = GenData.CurrentAngle + angle2 - GenData.MaxAngle;	
					angle1 -= offset;
					angle2 -= offset;
				}
			}
			else if (sign > 0)
			{
				if((GenData.CurrentAngle + angle2) < -GenData.MaxAngle)
				{
					float offset = -(GenData.CurrentAngle + angle2) - GenData.MaxAngle;
					angle1 += offset;
					angle2 += offset;
				}
				else if((GenData.CurrentAngle + angle1) > GenData.MaxAngle)
				{
					float offset = GenData.CurrentAngle + angle1 - GenData.MaxAngle;	
					angle1 -= offset;
					angle2 -= offset;
				}
			}

			data1.NorAngle = GenData.NorAngle.Rotate(angle1).normalized;
			data1.CurrentAngle += angle1;
			data2.NorAngle = GenData.NorAngle.Rotate(angle2).normalized;			
			data2.CurrentAngle += angle2;
			data1.RandomAngleBranchMin *= GenData.ReduceRandomAngleRate;
			data1.RandomAngleBranchMax *= GenData.ReduceRandomAngleRate;
			data2.RandomAngleBranchMin *= GenData.ReduceRandomAngleRate;
			data2.RandomAngleBranchMax *= GenData.ReduceRandomAngleRate;

			if(data1.Width >= data1.MinBranchWidth)
			{
				BranchData branch1 = new BranchData(root);
				branch1.GenerateBranch(data1, leafData, mesh, endIndex);
				subBranches.Add(branch1);
			}

			if(data2.Width >= data2.MinBranchWidth)
			{
				BranchData branch2 = new BranchData(root);
				branch2.GenerateBranch(data2, leafData, mesh, endIndex);
				subBranches.Add(branch2);
			}
		}
		else if (endWidth >= GenData.OneBranchWidthThreshold)
		{
			BranchGenerateData data1 = GenData;
			data1.Step = step + 1;

			float subBranchPercent = Random.Range(GenData.SubBranchPercentMin, GenData.SubBranchPercentMax);
			data1.Width = endWidth * subBranchPercent * GenData.MultiplyWidthWhenSeperate;

			float randomAngle = (Random.Range(0, 2) - 0.5f) * 2;
			float angle1 = randomAngle * Random.Range(GenData.RandomAngleBranchMin, GenData.RandomAngleBranchMax);
			float sign = Mathf.Sign(angle1);
			if(sign < 0)
			{
				if(GenData.CurrentAngle + angle1 < -GenData.MaxAngle)
				{
					angle1 *= -1;
				}				
			}
			else
			{
				if(GenData.CurrentAngle + angle1 > GenData.MaxAngle)
				{
					angle1 *= -1;
				}
			}
			data1.NorAngle = GenData.NorAngle.Rotate(angle1).normalized;
			data1.CurrentAngle += angle1;
			data1.RandomAngleBranchMin *= GenData.ReduceRandomAngleRate;
			data1.RandomAngleBranchMax *= GenData.ReduceRandomAngleRate;

			if(data1.Width >= data1.MinBranchWidth)
			{
				BranchData branch1 = new BranchData(root);
				branch1.GenerateBranch(data1, leafData, mesh, endIndex);
				subBranches.Add(branch1);
			}
		}
	}

	public void AddFlower(FlowerGenerateData flower, float percentScale)
	{
		Vector2 startMiddlePoint = ((startPos1 + startPos2) / 2);
		//add flowers

		if(step >= flower.StartStep)
		{
			int numFlowers;
			float sqrNumFlowers;
			float distanceFromBranch;
			int numCol;			
			Vector2 perpenMain;
			Vector2 basePointMatrix;
			float stepX;
			float stepY;

			if(step == flower.StartStep)
			{
				
				float oneMinusStartPercent = 1 - flower.StartStepStartPercent;

				numFlowers = Mathf.RoundToInt(Random.Range(flower.FlowerAmountMin, flower.FlowerAmountMax) * length * percentScale * oneMinusStartPercent);
				sqrNumFlowers = Mathf.Sqrt(numFlowers);
				distanceFromBranch = flower.DistanceFromBranch + flower.ScaleSizeBaseSqrAmount * sqrNumFlowers;
				numCol = Mathf.CeilToInt(sqrNumFlowers);
				perpenMain = new Vector2(-norAngle.y, norAngle.x);
				basePointMatrix = (startMiddlePoint + norAngle * length * flower.StartStepStartPercent) - perpenMain * distanceFromBranch;
				stepX = (distanceFromBranch * 2) / numCol;
				stepY = length * oneMinusStartPercent / numCol;
			}
			else
			{
				numFlowers = Mathf.RoundToInt(Random.Range(flower.FlowerAmountMin, flower.FlowerAmountMax) * length * percentScale);
				sqrNumFlowers = Mathf.Sqrt(numFlowers);
				distanceFromBranch = flower.DistanceFromBranch + flower.ScaleSizeBaseSqrAmount * sqrNumFlowers;
				numCol = Mathf.CeilToInt(sqrNumFlowers);
				perpenMain = new Vector2(-norAngle.y, norAngle.x);
				basePointMatrix = startMiddlePoint - perpenMain * distanceFromBranch;
				stepX = (distanceFromBranch * 2) / numCol;
				stepY = length / numCol;
			}

			List<Vector2> matrix = new List<Vector2>(numCol * numCol);
			for(int i = 0; i < numCol; ++i)
			{
				for(int j = 0; j < numCol; ++j)
				{
					matrix.Add(new Vector2(i, j));
				}
			}

			for(int i = 0; i < numFlowers; ++i)
			{
				int matrixIndex = Random.Range(0, matrix.Count);
				Vector2 matrixCell = matrix[matrixIndex];
				matrix.RemoveAt(matrixIndex);

				Vector2 finalPos = basePointMatrix + perpenMain * Random.Range(matrixCell.x * stepX, (matrixCell.x + 1) * stepX);
				finalPos += norAngle * Random.Range(matrixCell.y * stepY, (matrixCell.y + 1) * stepY);

				GameObject flowerObj = QuickTestPrefabStorage.Instance.GetFlower();
				
				//float size = Random.Range(flower.SizeMin, flower.SizeMax);
				flowerObj.transform.localPosition = new Vector3(finalPos.x, finalPos.y, Random.Range(-2.0f, 0.0f));
#if UNITY_EDITOR
				if(Application.isPlaying)
				{
					flowerObj.transform.localScale = new Vector3(0, 0, 0);
				}
#else
				flowerObj.transform.localScale = new Vector3(0, 0, 0);
#endif
				
				flowerObj.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0.0f, 360f));

				minFlowerSize = flower.SizeMin;
				maxFlowerSize = flower.SizeMax;

				flowers.Add(flowerObj);
#if UNITY_EDITOR
				if(!Application.isPlaying)
				{
					TransformData trans = new TransformData();
					trans.Position = flowerObj.transform.localPosition;
					trans.RotateZ = flowerObj.transform.localRotation.eulerAngles.z;
					trans.Step = step;
					mesh.Flowers.Add(trans);
					flowers.Clear();
					GameObject.DestroyImmediate(flowerObj);
				}
#endif
			}
		}

		for(int i = 0; i < subBranches.Count; ++i)
		{
			subBranches[i].AddFlower(flower, percentScale);
		}
	}

	public void GrowAll(float speed, bool useParticle = true)
	{
		root.StartCoroutine(GrowAllCoroutine(speed, useParticle));
	}

	IEnumerator GrowAllCoroutine(float speed, bool useParticle)
	{
		float counter = 0;
		if(!isExpand)
		{			
			while(counter <= 1)
			{
				counter += Time.deltaTime * speed;
				SetGrowAllPercent(counter, useParticle);
				yield return new WaitForEndOfFrame();
			}
			isExpand = true;
		}
		else
		{
			while(counter <= 1)
			{
				counter += Time.deltaTime * speed;
				SetGrowFlower(counter, useParticle);
				yield return new WaitForEndOfFrame();
			}
		}

		counter = 0;
		for(int i = 0; i < subBranches.Count; ++i)
		{
			subBranches[i].GrowAll(speed, useParticle);
		}
	}

	void SetGrowAllPercent(float percent, bool useParticle)
	{
		percent = Mathf.Clamp01(percent);
		mesh.Vertices[endIndex] = Vector2.Lerp(startPos1, endPos1, percent);
		mesh.Vertices[endIndex + 1] = Vector2.Lerp(startPos2, endPos2, percent);
		mesh.IsVertexDirty = true;

		SetGrowFlower(percent, useParticle);

		int leafAmount = Mathf.RoundToInt(percent * leaves.Count);
		for(int i = lastLeafGrow; i < leafAmount; ++i)
		{
			lastLeafGrow = i + 1;
			root.StartCoroutine(GrowLeaf(i, Random.Range(minLeafSize, maxLeafSize)));
		}
		
	}

	void SetGrowFlower(float percent, bool useParticle)
	{
		int flowerAmount = Mathf.RoundToInt(percent * flowers.Count);
		for(int i = lastFlowerGrow; i < flowerAmount; ++i)
		{
			lastFlowerGrow = i + 1;
			//root.StartCoroutine(GrowFlower(i, );
			float randomSize = Random.Range(minFlowerSize, maxFlowerSize);
			flowers[i].transform.localScale = new Vector3(randomSize, randomSize, randomSize);			
		}
	}

	IEnumerator GrowLeaf(int index, float targetSize)
	{
		float counter = 0;
		while(counter < targetSize)
		{
			counter += Time.deltaTime;
			if(leaves.Count == 0)
			{
				yield break;
			}

			leaves[index].transform.localScale = new Vector3(counter, counter, counter);
			yield return new WaitForEndOfFrame();
		}
		leaves[index].transform.localScale = new Vector3(targetSize, targetSize, targetSize);
	}

	IEnumerator GrowFlower(int index, float targetSize)
	{
		float counter = 0;
		while(counter < targetSize)
		{
			counter += Time.deltaTime;
			if(flowers.Count == 0)
			{
				yield break;
			}

			flowers[index].transform.localScale = new Vector3(counter, counter, counter);
			yield return new WaitForEndOfFrame();
		}
		flowers[index].transform.localScale = new Vector3(targetSize, targetSize, targetSize);
	}
	
	public void ReturnToPool()
	{
		for(int i = 0; i < leaves.Count; ++i)
		{
			GameObject.DestroyImmediate(leaves[i]);
		}
		leaves.Clear();

		for(int i = 0; i < flowers.Count; ++i)
		{
			GameObject.DestroyImmediate(flowers[i]);
		}
		flowers.Clear();		

		for(int j = 0; j < subBranches.Count; ++j)
		{
			subBranches[j].ReturnToPool();
		}

		lastFlowerGrow = 0;
		lastLeafGrow = 0;
	}
}

[System.Serializable]
public struct BranchGenerateData
{
	public float ReduceWidthRateMin;
	public float ReduceWidthRateMax;	
	public float MinBranchWidth;
	public float MultiplyWidthWhenSeperate;
	public float BranchLengthMin;
	public float BranchLengthMax;
	public float OneBranchWidthThreshold;
	public float TwoBranchWidthThreshold;
	public float RandomAngleBranchMin;
	public float RandomAngleBranchMax;
	public float ReduceRandomAngleRate;
	public float RandomAngleEndBranch;
	public float SubBranchPercentMin;
	public float SubBranchPercentMax;
	public int Step;
	public float ForceLength;
	public float Width;
	public float MaxAngle;
	public Vector2 NorAngle;
	public int CollapseStep;
	[HideInInspector]
	public Vector2 StartPos1;
	[HideInInspector]
	public Vector2 StartPos2;
	[HideInInspector]
	public float CurrentAngle;	
}

[System.Serializable]
public struct LeafGenerateData
{
	public float LeafAmountMin;
	public float LeafAmountMax;
	public float IncreaseAmountPercentPerStep;
	public float NonLeafPercent;
	public float SizeMin;
	public float SizeMax;
	[HideInInspector]
	public Color32 Color;
}

[System.Serializable]
public struct FlowerGenerateData
{
	public float DistanceFromBranch;
	public float ScaleSizeBaseSqrAmount;
	public float FlowerAmountMin;
	public float FlowerAmountMax;
	public float SizeMin;
	public float SizeMax;
	public int StartStep;
	public float StartStepStartPercent;
}

public class MeshData
{	
	public List<Vector3> Vertices;
	public List<int> Indices;
	public List<Color32> Colors;
	public List<Vector2> Uvs;
	public bool IsVertexDirty;
	public Color32 BranchColor;
	public bool IsLeft;//for lighting side
#if UNITY_EDITOR
	public List<int> ParentID;
	public List<int> GrowStep;
	public List<TransformData> Leaves;
	public List<TransformData> Flowers;
#endif
}

public class ProceduralBranch : MonoBehaviour, IBranch
{
	readonly float backupAmount = 0.2f;
	public BranchGenerateData GenData;
	public LeafGenerateData LeafGenData;
	public FlowerGenerateData FlowerGenData;	

	[Header("Misc")]
	public Material WaterGrow;
	public Material TransitionMat;
	public Material SuccessGrowMat;
	public MeshFilter Filter;
	public MeshRenderer Render;
	public SpriteRenderer Indicator;
	public ParticleSystem PerfectParticle;
	public ParticleSystem GreatParticle;
	public Color32 TouchColor;

	public MeshData BranchMeshData{get{return meshData;}}
	public Mesh BranchMesh{get{return mesh;}}
	public SpriteRenderer bIndicator{get{return Indicator;}}
	//public GameObject gameObject{get{return gameObject;}}
	//public Transform transform{get{return transform;}}
	List<Vector3> vertices;
	List<int> indices;
	List<Color32> colors;
	List<Vector2> uvs;
	MeshData meshData;
	BranchData branch;	
	
	Mesh mesh;
	bool isGenerated;
	Vector2 branchVector;
	BranchGenerateData baseGenData;
	Color32 useColor;
	float touchPercent;
	bool isLeft;


	void Awake()
	{
		isGenerated = false;
		TransitionMat = new Material(TransitionMat);
		baseGenData = GenData;		
		Init();
	}

	public void GrowFlowerEnd()
	{


	}
	public void SetTopSprite(Sprite spr)
	{
	}	
	
#if UNITY_EDITOR
	public void CleanUp()
	{
		if(branch != null)
		{
			branch.ReturnToPool();
		}
	}
#endif

	public void Generate()
	{
		isGenerated = true;
		Render.enabled =  true;
		Render.sortingLayerName = "Branch";
		Render.sortingOrder = 1;
		vertices.Clear();
		indices.Clear();
		colors.Clear();
		uvs.Clear();

		Vector3 firstPos = new Vector3(GenData.Width / 2, 0, 0);
		Vector3 secondPos = new Vector3(-GenData.Width / 2, 0, 0);
		vertices.Add(firstPos);
		vertices.Add(secondPos);
		colors.Add(useColor);
		colors.Add(useColor);
		if(isLeft)
		{
			uvs.Add(new Vector2(0, 0));
			uvs.Add(new Vector2(1, 0));
		}
		else
		{
			uvs.Add(new Vector2(1, 0));
			uvs.Add(new Vector2(0, 0));
		}
		GenData.StartPos1 = firstPos;
		GenData.StartPos2 = secondPos;

		meshData = new MeshData();
		meshData.Vertices = vertices;
		meshData.Indices = indices;
		meshData.Colors = colors;
		meshData.Uvs = uvs;
		meshData.IsVertexDirty = false;
		meshData.BranchColor = useColor;
		meshData.IsLeft = isLeft;

		if(branch != null)
		{
			branch.ReturnToPool();
		}

		branch = new BranchData(this);
		branch.GenerateBranch(GenData, LeafGenData, meshData, 0, false);
		
		RefreshMeshData();

		Filter.mesh = mesh;
	}

	void RefreshMeshData()
	{
		mesh.Clear();
		mesh.SetVertices(vertices);
		mesh.SetColors(colors);
		mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.RecalculateBounds();		
	}

	void Init() 
	{
		vertices = new List<Vector3>();
		indices = new List<int>();
		colors = new List<Color32>();
		uvs = new List<Vector2>();
		mesh = new Mesh();		
		mesh.MarkDynamic();
		//Indicator.gameObject.SetActive(false);		
	}

#if UNITY_EDITOR
	public void EditorInit()
	{
		vertices = new List<Vector3>();
		indices = new List<int>();
		colors = new List<Color32>();
		uvs = new List<Vector2>();
		mesh = new Mesh();		
		mesh.MarkDynamic();
		//Indicator.gameObject.SetActive(false);	

		meshData = new MeshData();
		meshData.Vertices = vertices;
		meshData.Indices = indices;
		meshData.Colors = colors;
		meshData.Uvs = uvs;
		meshData.IsVertexDirty = false;
		meshData.BranchColor = useColor;
		meshData.IsLeft = isLeft;

		Filter.mesh = mesh;
	}

	public void GenerateDebug(Vector2 size, float branchWidthPercent)
	{
		baseGenData = GenData;
		Init();
		//Generate();
		//generate
		isGenerated = true;
		Render.enabled =  true;
		Render.sortingLayerName = "Branch";
		Render.sortingOrder = 1;
		vertices.Clear();
		indices.Clear();
		colors.Clear();
		uvs.Clear();
		//set size
		//GenData = baseGenData;
		float finalHeight = size.y * (1 + backupAmount);
		
		float ratio = size.x / GenData.Width;

		GenData.Width = size.x;
		GenData.MinBranchWidth *= ratio;
		GenData.OneBranchWidthThreshold *= ratio;
		GenData.TwoBranchWidthThreshold *= ratio;
		GenData.ForceLength = finalHeight;
		GenData.CurrentAngle = 0;
		GenData.CollapseStep = 100;
		
		//generate
		Vector3 firstPos = new Vector3(GenData.Width / 2, 0, 0);
		Vector3 secondPos = new Vector3(-GenData.Width / 2, 0, 0);
		vertices.Add(firstPos);
		vertices.Add(secondPos);
		colors.Add(useColor);
		colors.Add(useColor);
		if(isLeft)
		{
			uvs.Add(new Vector2(0, 0));
			uvs.Add(new Vector2(1, 0));
		}
		else
		{
			uvs.Add(new Vector2(1, 0));
			uvs.Add(new Vector2(0, 0));
		}
		GenData.StartPos1 = firstPos;
		GenData.StartPos2 = secondPos;

		meshData = new MeshData();
		meshData.Vertices = vertices;
		meshData.Indices = indices;
		meshData.Colors = colors;
		meshData.Uvs = uvs;
		meshData.IsVertexDirty = false;
		meshData.BranchColor = useColor;
		meshData.IsLeft = isLeft;
		meshData.ParentID = new List<int>();
		meshData.GrowStep = new List<int>();
		meshData.Leaves = new List<TransformData>();
		meshData.Flowers = new List<TransformData>();
		meshData.ParentID.Add(-1);
		meshData.ParentID.Add(-1);
		meshData.GrowStep.Add(0);
		meshData.GrowStep.Add(0);

		if(branch != null)
		{
			branch.ReturnToPool();
		}

		branch = new BranchData(this);
		branch.GenerateBranch(GenData, LeafGenData, meshData, 0, false);
		branch.GenData.ReduceWidthRateMin *= branchWidthPercent;
		branch.GenData.ReduceWidthRateMax *= branchWidthPercent;
		branch.SpawnChildBranch();
		branch.AddFlower(FlowerGenData, 1.0f);
		RefreshMeshData();

		Filter.mesh = mesh;
		//========
		Render.material = SuccessGrowMat;
		branch.GrowAll(2);
		GenData = baseGenData;
	}
#endif

	void LateUpdate () 
	{
		if(meshData.IsVertexDirty)
		{
			mesh.SetVertices(meshData.Vertices);
			meshData.IsVertexDirty = false;
			mesh.RecalculateBounds();
		}
	}

	public void SetSize(float width, float height, float touchPercent, float perfectPercent,Color32 useColor, bool isLeft, bool collapseAll = false)
	{
		this.isLeft = isLeft;
		this.touchPercent = 1 - (touchPercent / ( 1 + backupAmount));
		float finalHeight = height * (1 + backupAmount);
 		WaterGrow.SetFloat("_TouchPercent", 1 - touchPercent);
		this.useColor = useColor;

		GenData = baseGenData;
		//size
		float ratio = width / GenData.Width;

		GenData.Width = width;
		GenData.MinBranchWidth *= ratio;
		GenData.OneBranchWidthThreshold *= ratio;
		GenData.TwoBranchWidthThreshold *= ratio;
		GenData.ForceLength = finalHeight;
		GenData.CurrentAngle = 0;
		
		if(collapseAll)
		{
			GenData.CollapseStep = 1;
		}
		//SuccessGrowLeft.SetColor("_Color",  useColor);
		LeafGenData.Color = useColor;

		Generate();	

		//indicator
		//if(collapseAll)
		//{
		//	Indicator.gameObject.SetActive(false);
		//}
		//else
		//{
			Indicator.gameObject.SetActive(true);
			Indicator.transform.localPosition = new Vector3(0, 0, Indicator.transform.localPosition.z);
			Indicator.transform.localScale = new Vector3(width, 1, 1);
			Indicator.color = Color.white;
		//}
		branchVector = finalHeight * GenData.NorAngle;

		//fx
		//OverWhite.transform.localScale = new Vector3(width, finalHeight, 1);
		//RedCut.localPosition = new Vector3(RedCut.localPosition.x, finalHeight * RedCut.localPosition.y, RedCut.localPosition.z);
		PerfectParticle.transform.localPosition = new Vector3(0, finalHeight * 0.2f, 0);
		GreatParticle.transform.localPosition = new Vector3(0, finalHeight * 0.2f, 0);
	}

	public void StartGrowing()
	{
		if(!isGenerated)
		{
			Generate();
		}
		WaterGrow.SetFloat("_TopVertexPos", vertices[2].y);
		Render.material = WaterGrow;		
	}

	public void SuccessGrowing(float growPercent, float branchWidthPercent, bool useParticle = true, bool useEffect = true)
	{
		//rs = Game.TimingResult.NORMAL;
		//fx
			
		Render.material = TransitionMat;
		StartCoroutine(TransitionBranchColor(useColor));
	

		//spawn branch late
		branch.GenData.ReduceWidthRateMin *= branchWidthPercent;
		branch.GenData.ReduceWidthRateMax *= branchWidthPercent;
		branch.SpawnChildBranch();

		RefreshMeshData();
		//===================
		branch.AddFlower(FlowerGenData, growPercent);
		branch.GrowAll(2, useParticle);
		Indicator.gameObject.SetActive(false);	
		
	}
	
	
	//public void OnEnable()
	//{
	//	Anim.enabled = true;
	//}

	IEnumerator TransitionBranchColor(Color32 color)
	{
		float time = 1.0f;//35 frame on animation
		float counter = 0;
		Color col = Color.white;
		col.a = 1.0f;
		yield return new WaitForEndOfFrame();
		while(counter < time)
		{
			counter += Time.deltaTime;
			col.a = 1.0f - counter / time;
			TransitionMat.SetColor("_Color",  col);
			yield return new WaitForEndOfFrame();
		}

		Render.material = SuccessGrowMat;
	}

	public Vector3 GetMainBranchPosition(float percent, float addAmount)
	{
		return Vector3.zero;
	}
	
	public void SuccessGrowingNoFlower()
	{
		Render.material = SuccessGrowMat;
	}
	
	public float CalculateFinalPercent(float percent)
	{
		return percent;
	}

	public void UpdateWaterPercent(float percent, bool isHightlight = false)
	{
		float finalPercent = backupAmount + percent * (1 - backupAmount);
		WaterGrow.SetFloat("_WaterPercent", finalPercent);
		if(finalPercent >= touchPercent)
		{
			Indicator.color = TouchColor;
		}
		Vector2 pos = branchVector * finalPercent;
		Indicator.transform.localPosition = new Vector3(pos.x, pos.y, Indicator.transform.localPosition.z);
	}

	public Vector3 GetMainBranchPosition(float addAmount)
	{
		float length = branchVector.magnitude + addAmount;
		Vector3 pos = transform.position + (Vector3)((branchVector.normalized * length).Rotate(transform.localRotation.eulerAngles.z));
		pos.z = -5;
		return pos;
	}

	public void SuccessGrowingNoAnim(float growPercent)
	{
		//blah blah
		throw new System.NotImplementedException();
	}

    public void Hibrate()
    {

    }

	public void Mirror(IBranch target)
	{
		
	}
}