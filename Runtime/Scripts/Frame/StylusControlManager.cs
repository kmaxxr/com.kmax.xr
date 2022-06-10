
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
namespace KmaxXR
{
    public class StylusControlManager : MonoBehaviour
    {

        public Camera eventCamera;
        [SerializeField] StylusProvider provider;
        public float StylusBeamWidth = 0.006f;
        public float StylusBeamLength = 5f;
        [Tooltip("Use default beam if null")]
        public GameObject CustomStylusBeam;
        [Tooltip("Default direction is camera forward")]
        public bool ReverseZDirection = false;
        // UI深度的判断精度
        public float uiDepthFactor = 1000;
        public Transform pen;
        public bool allowStylus = true;

        [SerializeField, Header("Runtime status")]
        private GameObject CurHitGameobjet;
        [SerializeField]
        private GameObject LastHitGameobject;

        private bool _wasButtonPressed = false;
        private bool _wasScalePressed = false;
        private GameObject _stylusBeamObject = null;
        private LineRenderer _stylusBeamRenderer = null;
        private float _stylusBeamLength;//实际长度
        private StylusState _stylusState = StylusState.Idle;
        private GameObject _grabObject = null;
        private Vector3 _initialGrabOffset = Vector3.zero;
        private Quaternion _initialGrabRotation = Quaternion.identity;
        public float _initialGrabDistance = 0.0f;
        private GameObject _scaleObject = null;
        private float orginScaleDistance = 0;
        private InteractableObject _interactableObject;
        private Vector3 orginScale;

        private StylusPose stylusPose = new StylusPose();
        private void Awake()
        {
            var prefab = CustomStylusBeam != null ? CustomStylusBeam : Resources.Load("StylusLine");
            _stylusBeamObject = Instantiate(prefab, transform) as GameObject;
            _stylusBeamObject.name = "StylusBeam";
            _stylusBeamRenderer = _stylusBeamObject.GetComponent<LineRenderer>();
            _stylusBeamRenderer.startColor = Color.white;
            _stylusBeamRenderer.endColor = Color.white;
            _stylusBeamRenderer.generateLightingData = true;
            _stylusBeamLength = StylusBeamLength;
            GoFacade.Instance.Line = _stylusBeamObject;
            if (eventCamera == null) eventCamera = Camera.main;
            if (pen == null)
            {
                Debug.LogError("Please assign property: pen(Transform)");
            }
            if (provider == null)
            {
                Debug.LogError("Please assign property: provider(StylusProvider)");
            }
            provider.Open();
        }
        
        void OnDestroy()
        {
            provider.Close();
        }

        #region 按钮事件
        public static event Action<StylusButtonArgs> ButtonHandler;
        private void HandleMidBtnDown()
        {
            if (allowStylus)
            {
                GoFacade.Instance.ButtonPress = true;
                if (CurHitGameobjet != null)
                {
                    _interactableObject = CurHitGameobjet.transform.GetComponentInParent<InteractableObject>();
                    StylusBeamClickDown(CurHitGameobjet);
                }
            }
            ButtonHandler?.Invoke(new StylusButtonArgs{
                down = true,
                key = StylusKey.Mid,
                hitObj = CurHitGameobjet
            });
        }

        private void HandleMidBtnUp()
        {
            if (allowStylus)
            {
                GoFacade.Instance.ButtonPress = false;
                if (CurHitGameobjet != null)
                {
                    StylusBeamClick(CurHitGameobjet);
                }
                else
                {
                    StylusBeamClickNull();
                }
            }
            ButtonHandler?.Invoke(new StylusButtonArgs{
                down = false,
                key = StylusKey.Mid,
                hitObj = CurHitGameobjet
            });
        }
        private void HandleLeftBtnDown()
        {
            ButtonHandler?.Invoke(new StylusButtonArgs{
                down = true,
                key = StylusKey.Left,
                hitObj = CurHitGameobjet
            });
        }
        private void HandleLeftBtnUp()
        {
            ButtonHandler?.Invoke(new StylusButtonArgs{
                down = false,
                key = StylusKey.Left,
                hitObj = CurHitGameobjet
            });
        }
        private void HandleRightBtnDown()
        {
            if (allowStylus)
            {
                GoFacade.Instance.ButtonScalePress = true;

            }
            ButtonHandler?.Invoke(new StylusButtonArgs{
                down = true,
                key = StylusKey.Right,
                hitObj = CurHitGameobjet
            });
        }

        private void HandleRightBtnUp()
        {
            if (allowStylus)
            {
                GoFacade.Instance.ButtonScalePress = false;
            }
            ButtonHandler?.Invoke(new StylusButtonArgs{
                down = false,
                key = StylusKey.Right,
                hitObj = CurHitGameobjet
            });
        }
        #endregion
        
        void Process()
        {
            provider.Process();
            if (provider.GetKeyDown(StylusKey.Mid)) HandleMidBtnDown();
            if (provider.GetKeyDown(StylusKey.Left)) HandleLeftBtnDown();
            if (provider.GetKeyDown(StylusKey.Right)) HandleRightBtnDown();
            if (provider.GetKeyUp(StylusKey.Mid)) HandleMidBtnUp();
            if (provider.GetKeyUp(StylusKey.Left)) HandleLeftBtnUp();
            if (provider.GetKeyUp(StylusKey.Right)) HandleRightBtnUp();
        }
        
        void Update()
        {
            Process();
            if (allowStylus)
            {
                GoControl(GoFacade.Instance.StylusPose);
                UiControl(GoFacade.Instance.StylusPose);
                stylusPose.Pos = pen.position;
                stylusPose.Rot = pen.rotation;
                stylusPose.Direction = pen.forward;
                GoFacade.Instance.StylusPose = stylusPose;
                this.UpdateStylusBeam(GoFacade.Instance.StylusPose.Pos, GoFacade.Instance.StylusPose.Direction);

            }
        }

        /// <summary>
        /// UI识别
        /// </summary>
        /// <param name="pose"></param>
        private void UiControl(StylusPose pose)
        {

            if (_stylusBeamObject.activeSelf && _stylusState == StylusState.Idle)
            {
                foreach (var c in UIFacade.Canvases)
                {
                    Raycast(pose, c);
                }
            }
        }

        private Vector3 FocusPoint(Transform plane, Vector3 direct)
        {

            // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn

            float d = Vector3.Dot(plane.transform.position - _stylusBeamRenderer.GetPosition(0), plane.transform.forward) / Vector3.Dot(direct, plane.transform.forward);

            return d * direct + _stylusBeamRenderer.GetPosition(0);
        }


        /// <summary>
        /// 识别射线点所在的图形
        /// </summary>
        /// <param name="pose">射线笔姿态</param>
        /// <param name="c">画布</param>
        private void Raycast(StylusPose pose, Canvas c)
        {
            var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(c);
            //  Debug.Log("ttt" + pointerPosition);
            // Necessary for the event system
            int totalCount = foundGraphics.Count;
            UIFacade.perSelectedGraphics.Clear();
            for (int i = 0; i < totalCount; ++i)
            {
                Graphic graphic = foundGraphics[i];

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1 || !graphic.raycastTarget || graphic.canvasRenderer.cull || !graphic.enabled)
                    continue;

                var p = FocusPoint(graphic.transform, pose.Direction);
                if (RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, p))
                {
                    UIFacade.perSelectedGraphics.Add(graphic);

                }
            }

            if (UIFacade.perSelectedGraphics.Count > 0)
            {
                GetZGraphic(UIFacade.perSelectedGraphics, pose.Direction);
            }
            else
            {
                UIFacade.Ray2dPoint = new Vector2(9999, 9999);
                UIFacade.Ray3dPoint = new Vector3(9999, 9999, 9999);
            }

        }

        private float GetD(Transform plane, Vector3 direct)
        {
            float d = Vector3.Dot(plane.transform.position - _stylusBeamRenderer.GetPosition(0), plane.transform.forward) / Vector3.Dot(direct, plane.transform.forward);
            return d;
        }
        /// <summary>
        /// 给识别到的图形进行层级排序
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="direct"></param>
        private void GetZGraphic(List<Graphic> graphics, Vector3 direct)
        {
            var m = graphics.Count;
            for (int i = 0; i < m - 1; i++)
            {
                for (int j = 0; j < m - 1 - i; j++)
                {
                    float d = GetD(graphics[j].transform, direct);
                    float dd = GetD(graphics[j + 1].transform, direct);
                    if (!ReverseZDirection)
                    {
                        if (d > dd)
                        {
                            var g = graphics[j + 1];
                            graphics[j + 1] = graphics[j];
                            graphics[j] = g;
                        }
                    }
                    else
                    {

                        if (d < dd)
                        {
                            var g = graphics[j + 1];
                            graphics[j + 1] = graphics[j];
                            graphics[j] = g;
                        }
                    }
                }

            }

            List<Graphic> gData = new List<Graphic>();

            for (int ii = 0; ii < m; ii++)
            {
                // Debug.Log(graphics[ii]);
                float d = Mathf.Floor(GetD(graphics[0].transform, direct) * uiDepthFactor);
                float dd = Mathf.Floor(GetD(graphics[ii].transform, direct) * uiDepthFactor);

                if (d == dd)
                {
                    gData.Add(graphics[ii]);

                }

            }

            var p = FocusPoint(gData[0].transform, direct);
            var length = GetD(graphics[0].transform, direct);



            if ((GoFacade.Instance.StoGoLength != 0 && length > GoFacade.Instance.StoGoLength) || _stylusState != StylusState.Idle)
            {
                UIFacade.Ray2dPoint = new Vector2(9999, 9999);
                UIFacade.Ray3dPoint = new Vector3(9999, 9999, 9999);
            }
            else
            {
                if (length > _stylusBeamLength)
                {
                    UIFacade.Ray2dPoint = new Vector2(9999, 9999);
                    UIFacade.Ray3dPoint = new Vector3(9999, 9999, 9999);
                }
                else
                {
                    UIFacade.Ray2dPoint = eventCamera.WorldToScreenPoint(p);
                    UIFacade.Ray3dPoint = p;
                    _stylusBeamLength = length;
                }

            }

        }


        /// <summary>
        /// 3D物体控制
        /// </summary>
        /// <param name="pose"></param>
        private void GoControl(StylusPose pose)
        {

            bool isButtonPressed = GoFacade.Instance.ButtonPress;
            bool isScalePressed = GoFacade.Instance.ButtonScalePress;

            switch (_stylusState)
            {
                case StylusState.Idle:
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(pose.Pos, pose.Direction, out hit, StylusBeamLength))
                        {

                            _stylusBeamLength = hit.distance;
                            GoFacade.Instance.StoGoLength = _stylusBeamLength;
                            CurHitGameobjet = hit.collider.gameObject;


                            if (isButtonPressed && !_wasButtonPressed)
                            {
                                if (_interactableObject != null)
                                {

                                    this.BeginGrab(hit.collider.gameObject, hit.distance, pose.Pos, pose.Rot);
                                    _stylusState = StylusState.Grab;

                                }
                            }

                            if (isScalePressed)
                            {

                                if (_interactableObject != null)
                                {
                                    _scaleObject = hit.collider.gameObject;
                                    orginScaleDistance = Vector3.Distance(_scaleObject.transform.position, pose.Pos);
                                    orginScale = _scaleObject.transform.localScale;
                                    _stylusState = StylusState.Scale;
                                }
                            }

                        }
                        else
                        {
                            _stylusBeamLength = StylusBeamLength;
                            _interactableObject = null;
                            CurHitGameobjet = null;
                            GoFacade.Instance.StoGoLength = 0;
                        }
                        GoFacade.Instance.CurHitGameObject = CurHitGameobjet;
                        if (CurHitGameobjet != null)
                        {
                            if (LastHitGameobject != CurHitGameobjet)
                            {
                                if (LastHitGameobject != null)
                                {

                                    StylusBeamExit(LastHitGameobject);
                                }
                                StylusBeamEnter(CurHitGameobjet);
                                LastHitGameobject = CurHitGameobjet;
                            }

                        }
                        else
                        {
                            if (LastHitGameobject != null)
                            {

                                StylusBeamExit(LastHitGameobject);
                                LastHitGameobject = null;

                            }
                        }

                    }
                    break;

                case StylusState.Grab:
                    {

                        this.UpdateGrab(pose.Pos, pose.Rot);

                        if (!isButtonPressed && _wasButtonPressed)
                        {
                            _stylusState = StylusState.Idle;


                        }


                    }
                    break;
                case StylusState.Scale:
                    {
                        ScaleObject(_scaleObject, pose.Pos);
                        if (!isScalePressed && _wasScalePressed)
                        {
                            _stylusState = StylusState.Idle;
                        }
                    }
                    break;
                default:
                    break;
            }


            _wasButtonPressed = isButtonPressed;
            _wasScalePressed = isScalePressed;
        }

        private void ScaleObject(GameObject go, Vector3 inputPos)
        {
            float hitdistance = Vector3.Distance(go.transform.position, inputPos);
            float offset = hitdistance / orginScaleDistance;

            go.transform.localScale = orginScale * offset;
        }

        private void BeginGrab(GameObject hitObject, float hitDistance, Vector3 inputPosition, Quaternion inputRotation)
        {

            Vector3 inputEndPosition = inputPosition + (inputRotation * (Vector3.forward * hitDistance));

            // Cache the initial grab state.
            _grabObject = hitObject;


            _initialGrabDistance = hitDistance;
            if (_interactableObject.IsWhole)
            {
                _initialGrabOffset = Quaternion.Inverse(hitObject.transform.root.rotation) * (hitObject.transform.root.position - inputEndPosition);
                _initialGrabRotation = Quaternion.Inverse(inputRotation) * hitObject.transform.root.rotation;

            }
            else
            {
                _initialGrabOffset = Quaternion.Inverse(inputRotation) * (hitObject.transform.position - inputEndPosition);
                _initialGrabRotation = Quaternion.Inverse(inputRotation) * hitObject.transform.rotation;
            }
        }

        private void UpdateGrab(Vector3 inputPosition, Quaternion inputRotation)
        {
            Vector3 inputEndPosition = inputPosition + (inputRotation * (Vector3.forward * _initialGrabDistance));


            Quaternion objectRotation = inputRotation * _initialGrabRotation;
            if (_interactableObject.IsWhole)
            {

                _grabObject.transform.root.rotation = objectRotation;
                Vector3 objectPosition = inputEndPosition + (objectRotation * _initialGrabOffset);
                _grabObject.transform.root.position = objectPosition;

            }
            else
            {

                _grabObject.transform.rotation = objectRotation;
                Vector3 objectPosition = inputEndPosition + (inputRotation * _initialGrabOffset);
                _grabObject.transform.position = objectPosition;
            }
        }

        private void UpdateStylusBeam(Vector3 stylusPosition, Vector3 stylusDirection)
        {
            if (_stylusBeamRenderer != null)
            {
                float stylusBeamWidth = StylusBeamWidth;
                float stylusBeamLength = _stylusBeamLength;

#if UNITY_EDITOR
                _stylusBeamRenderer.startWidth = stylusBeamWidth;
                _stylusBeamRenderer.endWidth = stylusBeamWidth;

#else
                _stylusBeamRenderer.startWidth = StylusBeamWidth;
                _stylusBeamRenderer.endWidth = StylusBeamWidth;
            
#endif
                _stylusBeamRenderer.SetPosition(0, stylusPosition);

                _stylusBeamRenderer.SetPosition(1, stylusPosition + (stylusDirection * stylusBeamLength));

            }
        }

        private enum StylusState
        {
            Idle = 0,
            Grab = 1,
            Scale = 2,
        }

        void StylusBeamClickDown(GameObject go)
        {
            StylusBeamEvent.StylusBeamClickDown?.Invoke(go);
        }


        void StylusBeamClick(GameObject go)
        {
            StylusBeamEvent.StylusBeamClick?.Invoke(go);
        }

        void StylusBeamClickNull()
        {
            StylusBeamEvent.StylusBeamClickNull?.Invoke();
        }

        public static event Action<GameObject> BeamEnter;
        public static event Action<GameObject> BeamExit;
        void StylusBeamEnter(GameObject go) => BeamEnter?.Invoke(go);
        void StylusBeamExit(GameObject go) => BeamExit?.Invoke(go);
    }

}