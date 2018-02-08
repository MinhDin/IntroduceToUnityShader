using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameFX : MonoBehaviour 
{
	public SpriteRenderer BackLight;
	public SpriteRenderer FrontLight;
	public float FXTime;
	public UnityStandardAssets.ImageEffects.BloomOptimized BloomOpt;

	Coroutine work;
	float baseIntensity;
	bool isFadeIn;
	float counter = 0;
	bool isHigh;

	public GameCamera gameCam;

	void Awake()
	{

		baseIntensity = BloomOpt.intensity;		
		isFadeIn = false;
		counter = 0;
		
	}

	public void FadeIn()
	{
		isFadeIn = true;
		if(work == null)
		{
			Active();
			work = StartCoroutine(PlayDeathFXCoroutine());
		}
	}

	void Active()
	{
		BackLight.enabled = true;
		FrontLight.enabled = true;
		if(isHigh)
		{
			gameCam.FlowerGlowCamera.enabled = true;
			gameCam.Plane.gameObject.SetActive(true);
		}
	}

	void InActive()
	{
		counter = 0;
		BackLight.color = new Color(BackLight.color.r, BackLight.color.g, BackLight.color.b, 0);
		FrontLight.color = new Color(FrontLight.color.r, FrontLight.color.g, FrontLight.color.b, 0);
		if(isHigh)
		{
			gameCam.FlowerGlowCamera.enabled = false;		
			gameCam.Plane.gameObject.SetActive(false);
		}

		if(isHigh)
		{
			BloomOpt.intensity = 0;
		}

		BackLight.enabled = false;
		FrontLight.enabled = false;	
	}

	public void FadeOut(bool instantly = true)
	{
		isFadeIn = false;
		if(instantly)
		{
			if(work != null)
			{
				StopCoroutine(work);
				work = null;
			}
			InActive();
		}
		else
		{
			if(work == null)
			{
				work = StartCoroutine(PlayDeathFXCoroutine());
			}
		}
	}

	IEnumerator PlayDeathFXCoroutine()
	{
		while(true)
		{
			if(isFadeIn)
			{
				if(counter >= FXTime)
				{
					counter = FXTime;
					break;
				}
				counter += Time.deltaTime;
			}
			else
			{
				if(counter <= 0)
				{
					counter = 0;
					break;
				}
				counter -= Time.deltaTime;
			}
			
			float percent = Mathf.Clamp01(counter / FXTime);

			BloomOpt.intensity = percent * baseIntensity;
			
			yield return null;
		}

		if(isFadeIn)
		{
			BloomOpt.intensity = baseIntensity;			
		}
		else
		{
			InActive();
		}

		work = null;
	}
	
}
