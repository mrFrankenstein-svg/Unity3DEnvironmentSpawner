#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EnvironmentSpawnerNamespace
{
	[CustomEditor(typeof(EnvironmentSpawner))]
	[CanEditMultipleObjects]
	public class EnvironmentSpawnerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			EnvironmentSpawner script = (EnvironmentSpawner)target; 

            if (GUILayout.Button("Generate"))
			{
				script.GenerateEnvironment_Main();
            }
			if (GUILayout.Button("Destroy all in parent object"))
			{
				script.DestrouAllInParentObject();
			}
            //Debug.Log(EditorUtility.IsDirty(this)+ "  "+ EditorUtility.IsDirty(this));
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(script);
            //serializedObject.Update();    // Display a property field
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("myProperty"));    // Mark the object as dirty
            //EditorUtility.SetDirty(target); serializedObject.ApplyModifiedProperties();
            /*
			script.scatterMode = EditorGUILayout.Popup("Scatter Mode", script.scatterMode, scatterModeOption);

			if (script.scatterMode == 1)
			{
				//script.offsetInEachCell = EditorGUILayout.Toggle("Offset In Each Cell", script.offsetInEachCell);
				script.fixedGridScale = EditorGUILayout.FloatField("Grid Scale ", script.fixedGridScale);
			}

			if (GUILayout.Button("Generate"))
			{
				script.InstantiateNew();
			}
			if (GUILayout.Button("Re-Generate All In Scene"))
			{
				EnviroSpawn_CS.MassInstantiateNew();
			}

			if (script.cCheck)
				GUILayout.Box("WARNING: The prefabs may overlap! Grid cycles > 0!");

			*/
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("myProperty"));        // Mark the object as dirty
            //EditorUtility.SetDirty(this); 
            //serializedObject.ApplyModifiedProperties();
        }
		public void OnEditorGUI()
		{
			EnvironmentSpawner scriptg = (EnvironmentSpawner)target;

		}

		public void OnInspectorUpdate()
		{
			Repaint();
		}

	}
}


#endif