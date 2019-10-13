using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KinematicCharacterController.Examples
{
    public class FrameratePanel : MonoBehaviour
    {
        public float PollingRate = 1f;
        public Text PhysicsRate;
        public Text PhysicsFPS;
        public Text AvgFPS;
        public Text AvgFPSMin;
        public Text AvgFPSMax;

        public Action<float> OnPhysicsFPSReady;

        public string[] FramerateStrings = new string[0];

        private bool _isFixedUpdateThisFrame = false;
        private bool _wasFixedUpdateLastFrame = false;
        private int _physFramesCount = 0;
        private float _physFramesDeltaSum = 0;

        private int _framesCount = 0;
        private float _framesDeltaSum = 0;
        private float _minDeltaTimeForAvg = Mathf.Infinity;
        private float _maxDeltaTimeForAvg = Mathf.NegativeInfinity;
        private float _timeOfLastPoll = 0;

        private void FixedUpdate()
        {
            _isFixedUpdateThisFrame = true;
        }

        void Update()
        {
            // Regular frames
            _framesCount++;
            _framesDeltaSum += Time.deltaTime;

            // Max and min
            if (Time.deltaTime < _minDeltaTimeForAvg)
            {
                _minDeltaTimeForAvg = Time.deltaTime;
            }
            if (Time.deltaTime > _maxDeltaTimeForAvg)
            {
                _maxDeltaTimeForAvg = Time.deltaTime;
            }

            // Fixed frames
            if (_wasFixedUpdateLastFrame)
            {
                _wasFixedUpdateLastFrame = false;

                _physFramesCount++;
                _physFramesDeltaSum += Time.deltaTime;
            }
            if (_isFixedUpdateThisFrame)
            {
                _wasFixedUpdateLastFrame = true;
                _isFixedUpdateThisFrame = false;
            }

            // Polling timer
            float timeSinceLastPoll = (Time.unscaledTime - _timeOfLastPoll);
            if (timeSinceLastPoll > PollingRate)
            {
                float physicsFPS = 1f / (_physFramesDeltaSum / _physFramesCount);

                AvgFPS.text = GetNumberString(Mathf.RoundToInt(1f / (_framesDeltaSum / _framesCount)));
                AvgFPSMin.text = GetNumberString(Mathf.RoundToInt(1f / _maxDeltaTimeForAvg));
                AvgFPSMax.text = GetNumberString(Mathf.RoundToInt(1f / _minDeltaTimeForAvg));
                PhysicsFPS.text = GetNumberString(Mathf.RoundToInt(physicsFPS));

                if(OnPhysicsFPSReady != null)
                {
                    OnPhysicsFPSReady(physicsFPS);
                }

                _physFramesDeltaSum = 0;
                _physFramesCount = 0;
                _framesDeltaSum = 0f;
                _framesCount = 0;
                _minDeltaTimeForAvg = Mathf.Infinity;
                _maxDeltaTimeForAvg = Mathf.NegativeInfinity;

                _timeOfLastPoll = Time.unscaledTime;
            }

            PhysicsRate.text = GetNumberString(Mathf.RoundToInt(1f / Time.fixedDeltaTime));
        }

        public string GetNumberString(int fps)
        {
            if (fps < FramerateStrings.Length - 1 && fps >= 0)
            {
                return FramerateStrings[fps];
            }
            else
            {
                return FramerateStrings[FramerateStrings.Length - 1];
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FrameratePanel))]
    public class FrameratePanelEditor : Editor
    {
        private const int MaxFPS = 999;

        private void OnEnable()
        {
            InitStringsArray();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Init strings array"))
            {
                InitStringsArray();
            }
        }

        private void InitStringsArray()
        {
            FrameratePanel fp = target as FrameratePanel;
            fp.FramerateStrings = new string[MaxFPS + 1];

            for (int i = 0; i < fp.FramerateStrings.Length; i++)
            {
                if (i >= fp.FramerateStrings.Length - 1)
                {
                    fp.FramerateStrings[i] = i.ToString() + "+" + " (<" + (1000f / (float)i).ToString("F") + "ms)";
                }
                else
                {
                    fp.FramerateStrings[i] = i.ToString() + " (" + (1000f/(float)i).ToString("F") + "ms)" ;
                }
            }
        }
    }
#endif
}