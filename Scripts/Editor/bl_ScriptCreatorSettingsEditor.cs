using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngineInternal;
using UnityEditorInternal;
	
namespace Lovatto.EditorTools.ScriptCreator
{
	[CustomEditor(typeof(bl_ScriptCreatorSettings))]
	public class bl_ScriptCreatorSettingsEditor : Editor
	{
	    bl_ScriptCreatorSettings script;
	    ReorderableList list;
	
	    /// <summary>
	    /// 
	    /// </summary>
	    private void OnEnable()
	    {
	        script = (bl_ScriptCreatorSettings)target;
	        list = new ReorderableList(serializedObject, serializedObject.FindProperty("scriptTemplates"), true, true, true, true);
	        list.drawElementCallback += DrawTemplateList;
	        list.elementHeightCallback += (int index) => { return EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index)); };
	        list.drawHeaderCallback += (Rect rect) => { EditorGUI.LabelField(rect, "Script Templates"); };
	    }
	
	    /// <summary>
	    /// 
	    /// </summary>
	    public override void OnInspectorGUI()
	    {
	        EditorGUI.BeginChangeCheck();
	
	        list.DoLayoutList();
			script.packageJsonTemplate = EditorGUILayout.ObjectField("Package Json Template", script.packageJsonTemplate, typeof(TextAsset), false) as TextAsset;
	        if (EditorGUI.EndChangeCheck())
	        {
	            serializedObject.ApplyModifiedProperties();
	            EditorUtility.SetDirty(target);
	        }
	    }
	
	    void DrawTemplateList(Rect rect, int index, bool isActive, bool isFocused)
	    {
	        var element = list.serializedProperty.GetArrayElementAtIndex(index);
	        rect.x += 10;
	        rect.width -= 10;
	        EditorGUI.PropertyField(rect, element, true);
	    }
	}
}