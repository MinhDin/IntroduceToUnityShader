using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(Camera))]
    [AddComponentMenu ("Image Effects/Bloom and Glow/Bloom (Optimized)")]
    public class BloomOptimized : PostEffectsBase
    {
        public enum Resolution
		{
            Low = 0,
            High = 1,
        }

        public enum BlurType
		{
            Standard = 0,
            Sgx = 1,
        }

        [Range(0.0f, 1.5f)]
        public float threshold = 0.25f;
        [Range(0.0f, 2.5f)]
        public float intensity = 0.75f;

        [Range(0.25f, 5.5f)]
        public float blurSize = 1.0f;

        Resolution resolution = Resolution.Low;
        [Range(1, 4)]
        public int blurIterations = 1;

        public BlurType blurType= BlurType.Standard;

        public Shader fastBloomShader = null;
        public Material Mat;// {get{return fastBloomMaterial;}}
        public Vector4 Parameter{get{return _parameter;}}        

        //public bool IsActive;
        //Material fastBloomMaterial = null;
        Vector4 _parameter;
        
        void Awake()        
        {
            
            //IsActive = false;
        }

        //public override bool CheckResources ()
		//{
            //return true;
            //CheckSupport (false);

            //fastBloomMaterial = CheckShaderAndCreateMaterial (fastBloomShader, fastBloomMaterial);
            //if (!isSupported)
            //    ReportAutoDisable ();
            //return isSupported;
        //}

        //void OnDisable ()
		//{
            //if (fastBloomMaterial)
            //    DestroyImmediate (fastBloomMaterial);
        //}

        void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
            //if (!IsActive)
			//{
            //    Graphics.Blit (source, destination, Mat, 6);
            //    return;
            //}

            //Graphics.SetRenderTarget(source.colorBuffer, source.depthBuffer);

            int divider = 5;//resolution == Resolution.Low ? 4 : 4;
            float widthMod = resolution == Resolution.Low ? 0.5f : 1.0f;
            _parameter = new Vector4 (blurSize * widthMod, 0.0f, threshold, intensity);
            Mat.SetVector ("_Parameter", _parameter);
            source.filterMode = FilterMode.Bilinear;

            var rtW= source.width/divider;
            var rtH= source.height/divider;

            // downsample
            RenderTexture rt = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
            rt.filterMode = FilterMode.Bilinear;

            Graphics.Blit(source, rt, Mat, 1);
            //Graphics.Blit(rt, destination);
            //return;
            var passOffs= blurType == BlurType.Standard ? 0 : 2;

            for(int i = 0; i < blurIterations; i++)
			{
                Mat.SetVector ("_Parameter", new Vector4 (blurSize * widthMod + (i*1.0f), 0.0f, threshold, intensity));

                // vertical blur
                RenderTexture rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
                rt2.filterMode = FilterMode.Bilinear;
                Graphics.Blit (rt, rt2, Mat, 2 + passOffs);
                RenderTexture.ReleaseTemporary (rt);
                rt = rt2;

                // horizontal blur
                rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
                rt2.filterMode = FilterMode.Bilinear;
                Graphics.Blit (rt, rt2, Mat, 3 + passOffs);
                RenderTexture.ReleaseTemporary (rt);
                rt = rt2;
            }

            Mat.SetTexture ("_Bloom", rt);

            Graphics.Blit (source, destination, Mat, 0);

            RenderTexture.ReleaseTemporary (rt);
        }
    }
}
