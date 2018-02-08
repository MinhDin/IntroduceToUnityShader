using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLight : MonoBehaviour 
{
	public SpriteRenderer LightRenderer;

	void Start () 
	{
		Shader.SetGlobalColor("LightColor", LightRenderer.color);
	}

	void Update () 
	{
		Shader.SetGlobalVector("LightPos", transform.position);
	}
}
