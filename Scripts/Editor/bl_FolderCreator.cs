using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Object = UnityEngine.Object;

public class bl_FolderCreator : EditorWindow
{

    public string parentPath;
    public string newFolderName;
    public Object parentReference;

    private Object lastObjects;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        minSize = new Vector2(300, 300);
        titleContent = new GUIContent("Folder Creator");
        parentPath = "Assets/";
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        EditorStyles.miniLabel.richText = true;
        EditorGUILayout.BeginVertical("box");
        parentReference = EditorGUILayout.ObjectField("Parent Folder", parentReference, typeof(Object), false) as Object;
        if(lastObjects != parentReference)
        {
            if (parentReference != null)
                parentPath = AssetDatabase.GetAssetPath(parentReference) + "/";
            else parentPath = "Assets/";
            lastObjects = parentReference;
        }
        EditorGUILayout.LabelField("Parent Path", parentPath);
        EditorGUILayout.EndVertical();

        newFolderName = EditorGUILayout.TextField("New Folder", newFolderName);
        GUILayout.Label($"<i>{parentPath}{newFolderName}</i>", EditorStyles.miniLabel);
        if(GUILayout.Button("Create new asset folder structure"))
        {
            CreateNewAssetFolderStructure();
        }
    }

    void CreateNewAssetFolderStructure()
    {
        string newPath = $"{parentPath}{newFolderName}";
        if (Directory.Exists(newPath)) return;

        Directory.CreateDirectory(newPath);
        newPath += "/Content/";
        Directory.CreateDirectory(newPath);
        var subPath = newPath;

        subPath = CreateSubFolder(subPath, "Scripts");
        CreateSubFolder(subPath, "Runtime");
        CreateSubFolder(subPath, "Runtime/Main");
        CreateSubFolder(subPath, "Runtime/UI");
        CreateSubFolder(subPath, "Internal");
        CreateSubFolder(subPath, "Internal/Editor");
        CreateSubFolder(subPath, "Internal/Structures");

        subPath = newPath;
        subPath = CreateSubFolder(subPath, "Art");
        CreateSubFolder(subPath, "UI");
        CreateSubFolder(subPath, "Font");
        CreateSubFolder(subPath, "Audio");

        subPath = newPath;
        subPath = CreateSubFolder(subPath, "Prefabs");
        CreateSubFolder(subPath, "Instances");
        CreateSubFolder(subPath, "Main");

        AssetDatabase.Refresh();
    }

    private string CreateSubFolder(string parentPath, string subFolder)
    {
        if (parentPath.EndsWith("/")) parentPath += subFolder;
        else parentPath += $"/{subFolder}";

        Directory.CreateDirectory(parentPath);
        return parentPath;
    }

    [MenuItem("Lovatto/Tools/Folder Creator")]
    public static void Open()
    {
        GetWindow<bl_FolderCreator>();
    }
}