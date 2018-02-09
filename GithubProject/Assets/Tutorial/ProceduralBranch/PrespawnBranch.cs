/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrespawnBranch : MonoBehaviour, IBranch 
{
	readonly float backupAmount = 0.2f;
	readonly float flowerStep = -0.05f;
	public LeafGenerateData LeafGenData;
	public FlowerGenerateData FlowerGenData;

	[Header("Misc")]
	public Material WaterGrow;
	public Material TransitionMat;
	public Material SuccessGrowMat;
	public MeshFilter Filter;
	public MeshRenderer Render;
	public SpriteRenderer WhiteRender;
	public ParticleSystem WhiteParticle;
	ParticleSystem.ShapeModule WhiteShape;
	ParticleSystem.MinMaxCurve emissionRate;
	ParticleSystem.EmissionModule emissionModule;
	float baseEmission;

	Mesh mesh;
	Color32 useColor;
	Vector2 branchVector;
	List<Vector3> vertices;
	float orininalTouchPercent;
	float touchPercent;
	bool isLeft;

	PrespawnedBranchDef def;
	int growStep;
	int maxStep;
	List<GameObject> leaves;
	
	List<float> leavesSize;
	List<Flower> flowers;
	List<Vector3> baseFlowers;
	List<float> flowerMagnitude;

	float maxLeavesSize;
	public int PerfectCombo{get;set;}
	static int lastIndex;
	static MeshLibraryDef libraryDef;

	bool isVisible;

	int flowerSkipToEnd;
	float flowerGrowPercent;
	bool flowerUseParticle;
	bool isGrowed;
	float perfectTouch;
	float width;

	//shader id
	int colorID;
	int touchPercentID;
	bool isGrow;
	int touchTexID;
	
	void Awake()
	{
		if(WhiteParticle != null)
		{
			WhiteShape = WhiteParticle.shape;
			emissionRate = WhiteParticle.emission.rateOverTime;
			baseEmission = emissionRate.constant;
			emissionModule = WhiteParticle.emission;
		}
		// TTin: Prevent mat change during editor ( git ignore )
//#if UNITY_EDITOR
		WaterGrow =  new Material(WaterGrow);
		SuccessGrowMat = new Material(SuccessGrowMat);
//#endif
		//Fix bug 2 transition branch at the same time
		TransitionMat = new Material(TransitionMat);
		PerfectCombo = 0;
		isVisible = false;
		colorID = Shader.PropertyToID("_Color");
		touchPercentID = Shader.PropertyToID("_TouchPercent");
		touchTexID = Shader.PropertyToID("_TouchTexture");
		vertices = new List<Vector3>(30);
		leaves = new List<GameObject>(30);
		leavesSize = new List<float>(30);
		isGrow = false;
		Init();
	}

	void Init() 
	{
		flowers = new List<Flower>();
		baseFlowers = new List<Vector3>();
		flowerMagnitude = new List<float>();
		mesh = new Mesh();		
		mesh.MarkDynamic();
		//Indicator.gameObject.SetActive(false);		
		Render.enabled =  true;
		SetBaseSortingLayer();
		Filter.mesh = mesh;
		flowerSkipToEnd = GameSetting.Instance.FlowerSkipToEnd;
		isGrowed = false;
		if(libraryDef == null)
		{
			libraryDef = Game.Instance.MeshLibrary;
		}
		if(WhiteParticle != null)
		{
			WhiteParticle.gameObject.SetActive(false);
		}
	}

	void SetBaseSortingLayer()
	{
		Render.sortingLayerName = "Branch";
		Render.sortingOrder = 10;
	}

	//void SetPlayingSortingLayer()
	//{
	//	Render.sortingLayerName = "Trunk";
	//	Render.sortingOrder = 5;
	//}

	void RefreshVertices(List<Vector3> lvertex)
	{
		mesh.SetVertices(lvertex);
		mesh.RecalculateBounds();	
	}	

	public void SetSize(float width, float height, float touchPercent, float perfectPercent, Color32 useColor, bool isLeft, bool collapseAll = false)
	{
		def = GetPrespawnedBranchDef(ref width, ref height);
		isGrowed = false;
		this.isLeft = isLeft;
		this.touchPercent = 1 - (touchPercent / ( 1 + backupAmount));
		float finalHeight = height * (1 + backupAmount);
		this.orininalTouchPercent = touchPercent;
 		WaterGrow.SetFloat(touchPercentID, 1 - touchPercent);
		this.useColor = useColor;
		this.perfectTouch = perfectPercent;
		this.width = width;
		//Indicator.gameObject.SetActive(true);
		//Indicator.transform.localPosition = new Vector3(0, 0, Indicator.transform.localPosition.z);
		//Indicator.transform.localScale = new Vector3(width, 1, 1);
		//Indicator.color = Color.white;

		branchVector = finalHeight * Vector2.up;

		//if((PerfectParticles != null) && (GreatParticles != null))
		//{
		//	PerfectParticles[0].transform.localPosition = new Vector3(0, finalHeight * 0.2f, 0);
		//	GreatParticles[0].transform.localPosition = new Vector3(0, finalHeight * 0.2f, 0);
		//}

		//set meshData
		if(collapseAll)
		{
			growStep = 1;
		}
		else
		{
			growStep = 2;
		}
		maxStep = -1;
		vertices.Clear();
		vertices.AddRange(def.Vertices);
		Vector3 start = Vector2.zero;
		int length = def.GrowStep.Count;
		for(int i = 0; i < length; ++i)
		{
			if(def.GrowStep[i] == growStep - 1)
			{
				start = vertices[i];
			}
			if(def.GrowStep[i] >= growStep)
			{
				vertices[i] = start ;
			}

			maxStep = Mathf.Max(maxStep, def.GrowStep[i]);
		}

		List<Color32> col = new List<Color32>(def.Vertices.Count);
		
		for(int i = 0; i < vertices.Count; ++i)
		{
			col.Add(useColor);
		}
		
		mesh.Clear();
		mesh.SetVertices(vertices);
		mesh.SetColors(col);
		mesh.SetIndices(def.Indices, MeshTopology.Triangles, 0);

		if(isLeft)
		{
			List<Vector2> reverseUV = new List<Vector2>(def.Uvs.Count);
			for(int i = 0; i < def.Uvs.Count; ++i)
			{
				reverseUV.Add(new Vector2((def.Uvs[i].x + 1) % 2, 0));
			}

			mesh.SetUVs(0, reverseUV);
		}
		else
		{
			mesh.SetUVs(0, def.Uvs);
		}
		mesh.RecalculateBounds();	

		//spawn leaves and flowers
		maxLeavesSize = -1;
		

		Render.enabled =  true;		
		//SetPlayingSortingLayer();

		//set white particle
		if(WhiteParticle != null)
		{
			WhiteParticle.gameObject.SetActive(true);

            float whiteParticleSize = perfectPercent * finalHeight * 0.85f;
            WhiteShape.scale = new Vector3(width * 0.7f, whiteParticleSize, 1);
            WhiteShape.position = new Vector3(0, finalHeight, 0);
            emissionRate.constant = baseEmission * whiteParticleSize / 0.2f * (width);
            emissionModule.rateOverTime = emissionRate;
		}
	}
	
	public void SetTopSprite(Sprite spr)
	{
		WaterGrow.SetTexture(touchTexID, spr.texture);
	}

	public void StartGrowing()
	{
		WaterGrow.SetFloat("_TopVertexPos", vertices[2].y);
		Render.material = WaterGrow;
	}

	public float CalculateFinalPercent(float percent)
	{
		return backupAmount + percent * (1 - backupAmount);
	}
	public void UpdateWaterPercent(float percent, bool isHighlight = false)
	{
		float finalPercent = CalculateFinalPercent(percent);
		WaterGrow.SetFloat("_WaterPercent", finalPercent);

		if(isHighlight)
		{
			WhiteRender.enabled = true;
			WhiteRender.transform.position = GetMainBranchPosition(finalPercent, 0);
		}
		else
		{
			WhiteRender.enabled = false;
		}
	}

	public void SuccessGrowingNoFlower()
	{
		Render.material = SuccessGrowMat;
		if(WhiteParticle != null)
		{
			WhiteParticle.gameObject.SetActive(false);
		}
	}

	public void SuccessGrowing(float growPercent, float branchWidthPercent, Game.TimingResult rs, bool useParticle = true, bool useEffect = true)
	{
		if(isGrowed)
		{
			return;
		}
		if(WhiteParticle != null)
		{
			WhiteParticle.gameObject.SetActive(false);
		}
		bool allowTransition = true;
		if(GraphicProfiler.GetGameOption(GraphicProfiler.GameOption.GAME_EFFECT_LEVEL) == 0)
		{
			useParticle = false;
			allowTransition = false;	

		}
		if(GraphicProfiler.GetGameOption(GraphicProfiler.GameOption.DECORATION_LEVEL) == 0)
		{
			growPercent *= GameSetting.Instance.LowDeviceFlowerReduceRate;
		}
		else
		{
			growPercent *= 0.85f;
		}
		//fx

		switch(rs)
		{
			case Game.TimingResult.PERFECT:		
				if(allowTransition)
				{		
					TransitionMat.SetColor(colorID,  Color.white);
					Render.material = TransitionMat;
					StartCoroutine(TransitionBranchColor(useColor));
				}
				else
				{
					Render.material = SuccessGrowMat;
				}
				break;
			case Game.TimingResult.GREAT:
				if(allowTransition)
				{
					TransitionMat.SetColor(colorID,  Color.white);
					Render.material = TransitionMat;
					StartCoroutine(TransitionBranchColor(useColor));
				}
				else
				{
					Render.material = SuccessGrowMat;
				}
				break;
			default :
				Render.material = SuccessGrowMat;
				break;
		}

		StartCoroutine(GrowBranch(growPercent, useParticle));		
		flowerGrowPercent = growPercent;
		flowerUseParticle = useParticle;
	}

	IEnumerator GrowBranch(float growPercent, bool useParticle)
	{
		isGrowed = true;
		leaves.Clear();
		leavesSize.Clear();
		maxLeavesSize = -1;
		int length = def.Leaves.Count;
		for(int i = 0; i < length; ++i)
		{
			GameObject aLeaf = Game.Instance.ObjectSpawner.GetARandomLeaf(transform);
			aLeaf.transform.localPosition = def.Leaves[i].Position;
			aLeaf.transform.localRotation = Quaternion.Euler(0, 0, def.Leaves[i].RotateZ);
			aLeaf.transform.localScale = Vector3.zero;
			aLeaf.GetComponent<SpriteRenderer>().color = useColor;

			leaves.Add(aLeaf);
			float size = Random.Range(LeafGenData.SizeMin, LeafGenData.SizeMax);
			maxLeavesSize = Mathf.Max(maxLeavesSize, size);
			leavesSize.Add(size);
			yield return null;
		}

		if(growStep == 2)
		{
			yield return StartCoroutine(GrowFlower(1, growPercent, useParticle));
		}

		for(int i = growStep; i <= maxStep; ++i)
		{
			float counter = 0;
			float speed = 2;
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
				RefreshVertices(vertices);
				yield return null;
			}

			StartCoroutine(GrowFlower(i, growPercent, useParticle));
			StartCoroutine(GrowLeaf(i));

			for(int j = 0; j < def.GrowStep.Count; ++j)
			{
				if(def.GrowStep[j] == i)
				{
					vertices[j] = def.Vertices[j];
				}
			}
		}
		RefreshVertices(def.Vertices);
	}

	public void GrowFlowerEnd()
	{
		if(isVisible)
		{
			if(isGrowed)
			{
				StartCoroutine(GrowFlowerEnd(flowerGrowPercent, true));
			}
			else
			{
				SuccessGrowing(GameSetting.Instance.PerfectFlowerPercent, GameSetting.Instance.BranchPerfect, Game.TimingResult.NONE, false, false);
			}
		}
		else
		{
			//SuccessGrowingNoFlower();
			//SuccessGrowing()
		}
	}

	IEnumerator GrowFlowerEnd(float growPercent, bool useParticle)
	{
		int length = def.Flowers.Count;
		for(int i = 0; i < length; ++i)
		{
			if((i % flowerSkipToEnd) != 0)
			{
				continue;
			}
			//if(def.Flowers[i].Step == step)
			//{
				float seed = Random.Range(0.0f, 1.0f);
				if(seed > growPercent)
				{
					continue;
				}

				Flower flo = Game.Instance.ObjectSpawner.GetARandomFlower(transform).GetComponent<Flower>();
				flo.transform.localPosition = new Vector3(def.Flowers[i].Position.x, def.Flowers[i].Position.y, flowerStep * i);
				flo.transform.localRotation = Quaternion.Euler(0, 0, def.Flowers[i].RotateZ);
				float scale = Random.Range(FlowerGenData.SizeMin, FlowerGenData.SizeMax);
				flo.transform.localScale = new Vector3(scale, scale, scale);
				flo.UseParticle = useParticle;
				Game.Instance.ThemeMgr.ApplyTheme(flo);
				flo.PlayGrowAnim();
				flowers.Add(flo);
				baseFlowers.Add(flo.transform.localPosition);
				flowerMagnitude.Add(((Vector2)flo.transform.localPosition).magnitude);				
				yield return null;
			//}
		}
	}
	IEnumerator GrowFlower(int step, float growPercent, bool useParticle)
	{
		int length = def.Flowers.Count;
		for(int i = 0; i < length; ++i)
		{
			if((i % flowerSkipToEnd) == 0)
			{
				continue;
			}
			if(def.Flowers[i].Step == step)
			{
				float seed = Random.Range(0.0f, 1.0f);
				if(seed > growPercent)
				{
					continue;
				}

				Flower flo = Game.Instance.ObjectSpawner.GetARandomFlower(transform).GetComponent<Flower>();
				flo.transform.localPosition = new Vector3(def.Flowers[i].Position.x, def.Flowers[i].Position.y, flowerStep * i);
				flo.transform.localRotation = Quaternion.Euler(0, 0, def.Flowers[i].RotateZ);
				float scale = Random.Range(FlowerGenData.SizeMin, FlowerGenData.SizeMax);
				flo.transform.localScale = new Vector3(scale, scale, scale);
				flo.UseParticle = useParticle;
				Game.Instance.ThemeMgr.ApplyTheme(flo);
				flo.PlayGrowAnim();
				flowers.Add(flo);
				baseFlowers.Add(flo.transform.localPosition);
				flowerMagnitude.Add(((Vector2)flo.transform.localPosition).magnitude);				
				yield return null;
			}
		}
	}

	IEnumerator GrowLeaf(int step)
	{		
		float counter = 0;
		
		int length = Mathf.Min(def.Leaves.Count, leavesSize.Count);

		while(counter < maxLeavesSize)
		{
			counter += Time.deltaTime;
			if(leaves.Count == 0)
			{
				yield break;
			}

			for(int i = 0; i < length; ++i)
			{
				if(def.Leaves[i].Step == step)
				{
					if(counter <= leavesSize[i])
					{
						leaves[i].transform.localScale = new Vector3(counter, counter, counter);
					}
					else
					{
						leaves[i].transform.localScale = new Vector3(leavesSize[i], leavesSize[i], leavesSize[i]);
					}
				}
			}
			yield return null;
		}
	}

	IEnumerator TransitionBranchColor(Color32 color)
	{
		float time = 1.0f;//35 frame on animation
		float counter = 0;
		Color col = Color.white;
		col.a = 1.0f;
		yield return null;
		while(counter < time)
		{
			counter += Time.deltaTime;
			col.a = 1.0f - counter / time;
			TransitionMat.SetColor(colorID,  col);
			yield return null;
		}

		Render.material = SuccessGrowMat;
	}

	public void ReturnToPool()
	{
		if(leaves != null)
		{
			for(int i = 0; i < leaves.Count; ++i)
			{
				Game.Instance.ObjectSpawner.ReturnALeaf(leaves[i]);
			}

			leaves.Clear();
		}
		
		if(flowers != null)
		{
			for(int i = 0; i < flowers.Count; ++i)
			{
				Game.Instance.ObjectSpawner.ReturnAFlower(flowers[i].gameObject);
			}

			flowers.Clear();
			baseFlowers.Clear();
			flowerMagnitude.Clear();
		}
		
		Render.enabled = false;		
		//Indicator.gameObject.SetActive(false);
		Game.Instance.ObjectSpawner.ReturnPrespawnBranch(this);
		PerfectCombo = 0;
		growStep = 0;
		isGrowed = false;
		if(WhiteParticle != null)
		{
			WhiteParticle.gameObject.SetActive(false);
		}
		//Game.Instance.ObjectSpawner.ReturnProceBranch(this);	
	}


	static PrespawnedBranchDef GetPrespawnedBranchDef(ref float width, ref float height)
	{
		int rs = -1;
		for(int i = 0; i < libraryDef.Books.Count; ++i)
		{
			if(width <= libraryDef.Books[i].Size.x)
			{
				rs = i;
				break;
			}
		}

		if(rs == -1)
		{
			rs = libraryDef.Books.Count - 1;
		}
		else
		{
			if(rs != 0)
			{
				float minus1 = width - libraryDef.Books[rs - 1].Size.x;
				float cur = libraryDef.Books[rs].Size.x - width;

				if(minus1 < cur)
				{
					rs = rs - 1;
				}
			}
		}

		lastIndex = (lastIndex + Random.Range(1, libraryDef.Books[rs].Pages.Count)) % libraryDef.Books[rs].Pages.Count;
		width = libraryDef.Books[rs].Size.x;
		height = libraryDef.Books[rs].Size.y;
		
		return libraryDef.Books[rs].Pages[lastIndex];
	}

	void ShowPerfectText()
	{
		return;
		// Animator anim = Game.Instance.PerfectTexBranchFX;
		// anim.transform.position = GetMainBranchPosition(1);
		// anim.gameObject.SetActive(true);
		// anim.Play("Perfect_Text", -1, 0);
	}

	public Vector3 GetMainBranchPosition(float percent, float addAmount)
	{
		float length = branchVector.magnitude * percent + addAmount;
		Vector3 pos = transform.position + (Vector3)((branchVector.normalized * length).Rotate(transform.localRotation.eulerAngles.z));
		pos.z = -5;
		return pos;
	}

	public void SuccessGrowingNoAnim(float growPercent)
	{
		//Indicator.gameObject.SetActive(false);

		Render.material = SuccessGrowMat;
		RefreshVertices(def.Vertices);

		for(int i = 0; i < def.Flowers.Count; ++i)
		{
			float seed = Random.Range(0.0f, 1.0f);
			if(seed > growPercent)
			{
				continue;
			}

			Flower flo = Game.Instance.ObjectSpawner.GetARandomFlower(transform).GetComponent<Flower>();
			flo.transform.localPosition = def.Flowers[i].Position;
			flo.transform.localRotation = Quaternion.Euler(0, 0, def.Flowers[i].RotateZ);
			float scale = Random.Range(FlowerGenData.SizeMin, FlowerGenData.SizeMax);
			flo.transform.localScale = new Vector3(scale, scale, scale);
			flo.UseParticle = false;
			Game.Instance.ThemeMgr.ApplyTheme(flo);
			flo.ShowFlowerNoAnim();
			flowers.Add(flo);
			baseFlowers.Add(flo.transform.position);
			flowerMagnitude.Add(((Vector2)flo.transform.position).magnitude);
		}
	}

	public void OnBecameVisible()
    {
		isVisible = true;
    }

    public void OnBecameInvisible()
    {
		isVisible = false;
    }

	public void Hibrate()
	{
		if(isVisible && !isGrow)
		{
			StartCoroutine(HibrateCoroutine());
			for(int i = 0; i < flowers.Count; ++i)
			{
				int seed = Random.Range(0, 25);
				if(seed == 1)
				{
					flowers[i].FallPetal();
				}
			}
		}
	}

	IEnumerator HibrateCoroutine()
	{
		float A = Random.Range(0.25f, 0.75f);
		float Duration = Random.Range(0.75f, 1.5f);
		float PhiRate = Mathf.PI;
		float W = 30;
		float time = 0;

		while(time < Duration)
		{
			time += Time.deltaTime;
			//float curA = (middleTime - Mathf.Abs(time - middleTime)) * A;
			float curA = (Duration - time) * A;
			for(int i = 0; i < def.Vertices.Count; ++i)
			{
				Vector2 basePos = def.Vertices[i];
				float angle = curA * Mathf.Sin(W * time + PhiRate * basePos.magnitude);
				vertices[i] = basePos.Rotate(angle);
			}
			for(int i = 0; i < def.Leaves.Count; ++i)
			{
				Vector2 basePos = def.Leaves[i].Position;
				float angle = curA * Mathf.Sin(W * time + PhiRate * basePos.magnitude);
				leaves[i].transform.localPosition = basePos.Rotate(angle);
			}
			for(int i = 0; i < flowers.Count; ++i)
			{
				Vector2 basePos = baseFlowers[i];				
				float angle = curA * Mathf.Sin(W * time + PhiRate * flowerMagnitude[i]);
				basePos = basePos.Rotate(angle);
				flowers[i].transform.localPosition = new Vector3(basePos.x, basePos.y, baseFlowers[i].z);
			}
			RefreshVertices(vertices);
			yield return null;
		}

		for(int i = 0; i < def.Vertices.Count; ++i)
		{
			vertices[i] = def.Vertices[i];
		}

		RefreshVertices(vertices);
	}
}
*/