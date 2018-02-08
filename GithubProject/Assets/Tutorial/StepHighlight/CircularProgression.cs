using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircularProgression : MonoBehaviour 
{
	private float activeTime;

	public float ScaleTime;

	public float Inner;
	public float Outer;
	//public float PercentFixTop = 0.02f;
    //public float PercentBlur = 0.3f;
	public Material Mat;

    private Material matLocal;
    
    private Vector2 startPosition;
	private Vector2 displayPosition;
	private Vector2 hidePosition;

	private Vector2 transitionStart;
	private Vector2 transitionEnd;

	private float transitionTotalTime = 0.2f;
	private float transitionTime = 0f;
	private bool isInTransition = false;
	private bool isActiveGlow;


	void Awake ()
    {
        matLocal = Mat;
		matLocal.SetFloat("_Inner", Inner);
		matLocal.SetFloat("_Outer", Outer);
		//matLocal.SetFloat("_PercentFixTop", PercentFixTop);
        //matLocal.SetFloat("_PercentBlur", PercentBlur);

		SetActiveTime(1);
    }

	private Vector3 GetPositionByTime (Vector3 pos1, Vector3 pos2, float time)
	{
		return Vector3.Lerp (pos1, pos2, time);
	}

	public void Update()
	{
		activeTime -= Time.deltaTime * ScaleTime;
		if (activeTime > 0)
		{
			UpdatePercent ();
		}
	}

	public void UpdatePercent()
	{
		float percent = activeTime;
		float angle = percent * 2 * Mathf.PI;
		Vector2 pos = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
		pos = pos.normalized * (Inner + Outer) / 2;
		matLocal.SetFloat("_Percent", percent);
		matLocal.SetVector("_Head", new Vector4(pos.x, pos.y, 0, (Inner + Outer) / 2));
	}

	public void SetActiveTime (float activeTime)
	{
		this.activeTime = activeTime;
	}

}
