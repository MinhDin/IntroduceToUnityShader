using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HeatDistortionValueSet : MonoBehaviour
{
	public SpriteRenderer BGTrans;
	public SpriteRenderer ThisTex;
	Vector2 bgSize;
	Vector2 thisSize;
	public Material HeatDistortMat;
	
	int bgTexID;
	void Awake()
	{
		bgTexID = Shader.PropertyToID("_BGTex");
		bgSize = new Vector2(BGTrans.sprite.bounds.size.x * BGTrans.transform.localScale.x, BGTrans.sprite.bounds.size.y * BGTrans.transform.localScale.y);
	}

	void Update()
	{
		SetData();
	}

	void OnDestroy()
	{
		HeatDistortMat.SetTextureScale(bgTexID, new Vector2(1, 1));
		HeatDistortMat.SetTextureOffset(bgTexID, Vector2.zero);
	}
	
	void SetData()
	{
		Vector2 thisSize = new Vector2(ThisTex.sprite.bounds.size.x * transform.localScale.x, ThisTex.sprite.bounds.size.y * transform.localScale.y);
		HeatDistortMat.SetTextureScale(bgTexID, new Vector2(thisSize.x / bgSize.x, thisSize.y / bgSize.y));
		HeatDistortMat.SetTextureOffset(bgTexID, new Vector2((bgSize.x - thisSize.x) / bgSize.x / 2 + (transform.position.x - BGTrans.transform.position.x) / bgSize.x,
				(bgSize.y - thisSize.y) / bgSize.y / 2 + (transform.position.y - BGTrans.transform.position.y) / bgSize.y));
	}
}
