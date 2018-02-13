using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraBlit : MonoBehaviour
{
    private bool hasInited = false;
    private Camera cam;

    private bool isEnabled;

    private Vector2 startPosition;
    public Material TransitionMaterial;

	private RenderTexture renderTexture;

	private int mode;

	public SpriteRenderer[] Sprs;
	public Color ActiveColor;
	public Color DeactiveColor;
	public Transform Hero;

	int colorMode = 0;

    public void Awake()
    {		
		colorMode = 0;
        Init();
    }

    public void Init()
    {
        if (hasInited) return;

        hasInited = true;
		cam = GetComponent<Camera>();

        isEnabled = false;
    }
	
    public void Deactivate()
    {
        isEnabled = false;

		RenderTexture.ReleaseTemporary (renderTexture);
		renderTexture = null;
		mode = 0;
		if (mode == 1)
		{
			TransitionMaterial.SetFloat ("_NormalizedTime", 0.0f);
			TransitionMaterial.SetVector ("_DisplayInfo", Vector4.zero);
		}

	}

	public void ActivateFakeScreen(RenderTexture renderTexture)
	{
		isEnabled = true;
		mode = 0;

		this.renderTexture = renderTexture;
	}

	IEnumerator Transition()
	{
		float t = 0;

		while(t < 1)
		{
			t += Time.deltaTime;
			SetNormalizedTime(t);
			yield return null;
		}

		Deactivate();
	}

	public void ActivateEnvTransition(Vector2 startPosition, RenderTexture renderTexture)
    {
        isEnabled = true;
		mode = 1;

        this.startPosition = startPosition;
		this.renderTexture = renderTexture;

		Color col;
		if(colorMode == 0)
		{
			col = ActiveColor;
			colorMode = 1;
		}
		else
		{
			col = DeactiveColor;
			colorMode = 0;
		}

		for(int i = 0; i < Sprs.Length; ++i)
		{
			Sprs[i].color = col;
		}
		
		StartCoroutine(Transition());
    }

    public void SetNormalizedTime(float normalizedTime)
    {
		TransitionMaterial.SetFloat("_NormalizedTime", normalizedTime);
    }

    public void Update()
    {

        if (!isEnabled)
		{
			if(Input.GetKeyDown(KeyCode.B))
			{
				ActivateEnvTransition(Hero.position, GetScreenShot());
			}
			return;
		}

		

		if (mode == 1)
		{
			float halfScreenH = cam.orthographicSize;
			float halfScreenW = halfScreenH * cam.aspect;

			Vector2 camPos = cam.transform.position;
			Vector2 offset = startPosition - camPos;

			TransitionMaterial.SetVector ("_DisplayInfo", new Vector4 (halfScreenW, halfScreenH, offset.x, offset.y));
		}
    }

	private void OnPostRender()
	{
		if (!isEnabled) return;

		if (mode == 0) Graphics.Blit (renderTexture, (RenderTexture)null);
		else Graphics.Blit(renderTexture, null, TransitionMaterial);
	}

	public RenderTexture GetScreenShot()
    {
		int w = Screen.width;
		int h = Screen.height;

		RenderTexture renderTarget = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);        
		cam.targetTexture = renderTarget;
	
		cam.Render();

		RenderTexture.active = renderTarget;

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

		RenderTexture.active = null;
		cam.targetTexture = null;

		return renderTarget;
    }
}
