using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "ScriptCreatorSettings", menuName = "Lovatto/Utils/Script Creator")]
public class bl_ScriptCreatorSettings : ScriptableObject
{
    public Template[] scriptTemplates;
    public List<string> scriptNamespaces = new List<string>();

    public void AddTemplate(Template template)
    {
        var list = new Template[scriptTemplates.Length + 1];
        for (int i = 0; i < scriptTemplates.Length; i++)
        {
            list[i] = scriptTemplates[i];
        }
        list[list.Length - 1] = template;
        scriptTemplates = list;
        EditorUtility.SetDirty(this);
    }

    public void AddNamespace(string namespa)
    {
        if (scriptNamespaces.Contains(namespa)) return;

        scriptNamespaces.Add(namespa);
        EditorUtility.SetDirty(this);
    }

    [Serializable]
    public class Template
    {
        public string Name;
        public TextAsset TextAsset;
        public TemplateParameter[] Parameters;
    }

    [Serializable]
    public class TemplateParameter
    {
        public string Name;
        public string Placeholder;
        public string Default;
        public bool AllowDragReference = false;
    }

    private static bl_ScriptCreatorSettings _data;
    public static bl_ScriptCreatorSettings Instance
    {
        get
        {
            if (_data == null)
            {
                _data = Resources.Load<bl_ScriptCreatorSettings>("ScriptCreatorSettings");
            }
            return _data;
        }
    }
}