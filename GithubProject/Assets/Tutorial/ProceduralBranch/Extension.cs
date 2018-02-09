using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extension 
{     
     public static Vector2 Rotate(this Vector2 v, float degrees) 
	 {
         float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
         float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
         
         float tx = v.x;
         float ty = v.y;
         v.x = (cos * tx) - (sin * ty);
         v.y = (sin * tx) + (cos * ty);
         
         return v;
     }
 }

 public static class Vector3Extension 
{     
     public static Vector3 SmoothStep(this Vector3 a, Vector3 b, float t) 
	 {
         return new Vector3(Mathf.SmoothStep(a.x, b.x, t), Mathf.SmoothStep(a.y, b.y, t), Mathf.SmoothStep(a.z, b.z, t));
     }
 }