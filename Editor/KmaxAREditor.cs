using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace KmaxXR
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(KmaxAR))]
    public class KmaxAREditor : Editor
    {
        private KmaxAR script;
        public override void OnInspectorGUI()
        {

            base.DrawDefaultInspector();
            //获取脚本对象
            if (script == null)
                script = target as KmaxAR;


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Connect AR"))
            {
                ConnectAR();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open AR"))
            {
                OpenARCamera();
            }

            EditorGUILayout.EndHorizontal();
           

        }

        private void ConnectAR()
        {
            if (Application.isPlaying)
            {
                script.ConnectAR();
            }
        }

        public void OpenARCamera()
        {
            if (Application.isPlaying)
            {
                script.OpenAR();
            }
        }
    }
}
