using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public static GameCamera Instance;
   
    public SpriteRenderer Render;
    public float MaxZoomOut = 14;
    Vector3 m_defaultPosition;

    private const float TIME_MOVE = 0.5f;
    private const float TIME_ZOOM = 0.5f;
    private const float TIME_SHAKE = 0.2f;
    private const float SHAKE_DISTANCE = 0.09f;
    private const float TREE_WIDTH = 5.0f;
    private const float MAX_RESET_MOVE = 8.0f;
    Camera m_camera;
    public Camera BaseCamera { get { return m_camera; } }
    private float m_treeHeight;
    public MeshRenderer Plane;
    public Camera UICamera
    {
        get
        {
            return uiCamera;
        }
        set
        {
            uiCamera = value;
            uiCamera.orthographicSize = m_camera.orthographicSize;
            uiCamera.transform.position = m_camera.transform.position;
        }
    }
    Camera uiCamera;
    //fx
    int topRightAnimGreat;
    int topRightAnimPerfect;
    int topRightAnimBad;
    Vector2 baseFXSize;

    float baseOrthographicSize;

    RenderTexture screenShot;
    bool screenShotReady;

    bool needMove;
    bool needZoom;
    float m_nextZoom;
    float m_moveTime;
    float m_zoomTime;
    Vector3 m_nextPosition;

    private Vector3 m_beforeShakePos;
    private bool needShake;
    private float m_shakeTimer;
    Vector2 perlinNoisePosX;
    Vector2 perlinNoisePosY;
    Vector2 basePerlinPos;
    //flower grow
    public Camera FlowerGlowCamera;
    //public UnityStandardAssets.ImageEffects.BloomOptimized BloomOpt;
    RenderTexture BaseImage;

    bool isHighProfile;
    bool lastFlowerGlowState;
    bool isResult;
    int finishResultCameraCounter;

    void SetFlowerCameraSize(float size)
    {
        FlowerGlowCamera.orthographicSize = m_camera.orthographicSize;
        FlowerGlowCamera.aspect = m_camera.aspect;
        Plane.transform.localScale = new Vector3(m_camera.aspect * size * 2, size * 2, 1);
        
    }

    private void Awake()
    {
        Instance = this;
        m_camera = GetComponent<Camera>();


        screenShotReady = false;
        needMove = false;
        needZoom = false;
        m_nextPosition = Vector3.zero;

        topRightAnimGreat = Animator.StringToHash("Great");
        topRightAnimPerfect = Animator.StringToHash("Perfect");
        topRightAnimBad = Animator.StringToHash("Bad");
        //
     
        lastFlowerGlowState = false;

        if (FlowerGlowCamera != null)
        {            
            SetFlowerCameraSize(m_camera.orthographicSize);
           // isHighProfile = true;
            //BaseImage = new RenderTexture(m_camera.pixelWidth, m_camera.pixelHeight, 24, RenderTextureFormat.ARGB32);
            //m_camera.targetTexture = null;
            Plane.sortingLayerName = "Foreground";
            Plane.sortingOrder = 10000;
            //Plane.material.SetTexture("_MainTex", BaseImage);
            //FlowerGlowCamera.targetTexture = BaseImage;
            //Plane.gameObject.SetActive(false);
            //FlowerGlowCamera.transform.SetParent(null);
        }
        else
        {
            isHighProfile = false;
        }

    }
}
