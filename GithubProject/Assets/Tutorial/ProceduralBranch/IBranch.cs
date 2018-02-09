using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBranch 
{
	GameObject gameObject{get;}
	Transform transform{get;}
	//SpriteRenderer bIndicator{get;}
	//int PerfectCombo{get;set;}
	void SetSize(float width, float height, float touchPercent, float perfectPercent, Color32 useColor, bool isLeft, bool collapseAll = false);
	void StartGrowing();
	void SuccessGrowing(float growPercent, float branchWidthPercent, bool useParticle = true, bool useEffect = true);
	void UpdateWaterPercent(float percent, bool isHightlight = false);
	//void ReturnToPool();
	void SuccessGrowingNoAnim(float growPercent);
	void Hibrate();
	//void Mirror(IBranch branch);
	void SetTopSprite(Sprite spr);
	void GrowFlowerEnd();
	Vector3 GetMainBranchPosition(float percent, float addAmount);
	float CalculateFinalPercent(float percent);
}
