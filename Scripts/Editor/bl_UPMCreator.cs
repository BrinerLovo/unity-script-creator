using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;

public class bl_UPMCreator : EditorWindow
{
    public PackageInfo packageInfo = new PackageInfo();
    SerializedObject serializedObject;
    SerializedProperty packInfoProp;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        minSize = new Vector2(300, 300);
        titleContent = new GUIContent("UPM Creator");
        serializedObject = new SerializedObject(this);
        packInfoProp = serializedObject.FindProperty("packageInfo");
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        EditorStyles.miniLabel.richText = true;
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(packInfoProp, true);
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button("Create"))
        {
            Create();
        }
    }

    void Create()
    {
        string newPath = $"Packages/{packageInfo.name}";
        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
        }
        newPath = $"Packages/{packageInfo.name}/package.json";
        File.WriteAllText(newPath, GetPackageInfoJson());
        newPath = $"Packages/{packageInfo.name}/README.md";
        File.WriteAllText(newPath, "");

        AssetDatabase.Refresh();
        Application.OpenURL($"Packages/{packageInfo.name}/");
    }

    private string GetPackageInfoJson()
    {
        var i = packageInfo;
        var json = bl_ScriptCreatorSettings.Instance.packageJsonTemplate.text;
        json = json.Replace("#name#", i.name);
        json = json.Replace("#version#", i.version);
        json = json.Replace("#displayName#", i.displayName);
        json = json.Replace("#description#", i.description);
        json = json.Replace("#unity#", string.IsNullOrEmpty(i.unity) ? Application.unityVersion : i.unity);
        json = json.Replace("#author#", i.authorName);
        json = json.Replace("#email#", i.email);
        json = json.Replace("#url#", i.url);

        var keywords = "";
        if (i.keywords != null && i.keywords.Length > 0)
        {          
            for (int r = 0; r < i.keywords.Length; r++)
            {
                keywords += $"\"{i.keywords[r]}\"";
                keywords += r < i.keywords.Length - 1 ? ",\n" : "\n";
            }
        }
        json = json.Replace("#keywords#", keywords);

        return json;
    }

    private string CreateSubFolder(string parentPath, string subFolder)
    {
        if (parentPath.EndsWith("/")) parentPath += subFolder;
        else parentPath += $"/{subFolder}";

        Directory.CreateDirectory(parentPath);
        return parentPath;
    }

    [Serializable]
    public class PackageInfo
    {
        public string name;
        public string version = "1.0.0";
        public string displayName;
        public string description;
        public string unity;
        public string[] keywords;

        public string authorName = "Lovatto";
        public string email = "contact.lovattostudio@gmail.com";
        public string url = "https://www.lovattostudio.com";
    }

    [MenuItem("Lovatto/Tools/UPM Creator")]
    public static void Open()
    {
        GetWindow<bl_UPMCreator>();
    }
}