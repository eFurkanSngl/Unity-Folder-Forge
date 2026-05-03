using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class UnityFolderForgeWindow : EditorWindow
{
    private const string DefaultRootPath = "Assets";

    [SerializeField] private List<FolderEntry> folders = new List<FolderEntry> { FolderEntry.CreateDefault() };
    [SerializeField] private string rootPath = DefaultRootPath;
    [SerializeField] private int selectedFolderIndex;

    private Vector2 scrollPosition;

    [MenuItem("Tools/Unity Folder Forge")]
    public static void Open()
    {
        UnityFolderForgeWindow window = GetWindow<UnityFolderForgeWindow>("Folder Forge");
        window.minSize = new Vector2(420f, 360f);
        window.Show();
    }

    private void OnGUI()
    {
        DrawHeader();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawRootSection();
        EditorGUILayout.Space(8f);
        DrawFolderNamesSection();
        EditorGUILayout.Space(8f);
        DrawSubfoldersSection();
        EditorGUILayout.EndScrollView();

        DrawCreateButton();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Unity Folder Forge", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Select a folder to edit only that folder's subfolders.");
        EditorGUILayout.Space(8f);
    }

    private void DrawRootSection()
    {
        EditorGUILayout.LabelField("Root", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            rootPath = EditorGUILayout.TextField(rootPath);

            if (GUILayout.Button("Reset", GUILayout.Width(64f)))
            {
                rootPath = DefaultRootPath;
            }
        }
    }

    private void DrawFolderNamesSection()
    {
        EditorGUILayout.LabelField("Folders", EditorStyles.boldLabel);
        EnsureFolderList();

        for (int i = 0; i < folders.Count; i++)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                bool isSelected = selectedFolderIndex == i;
                string buttonText = isSelected ? "Selected" : "Select";

                if (GUILayout.Button(buttonText, GUILayout.Width(72f)))
                {
                    selectedFolderIndex = i;
                }

                folders[i].Name = EditorGUILayout.TextField(folders[i].Name);

                using (new EditorGUI.DisabledScope(folders.Count <= 1))
                {
                    if (GUILayout.Button("-", GUILayout.Width(28f)))
                    {
                        folders.RemoveAt(i);
                        selectedFolderIndex = Mathf.Clamp(selectedFolderIndex, 0, folders.Count - 1);
                        break;
                    }
                }
            }
        }

        if (GUILayout.Button("Add Folder"))
        {
            folders.Add(new FolderEntry());
            selectedFolderIndex = folders.Count - 1;
        }
    }

    private void DrawSubfoldersSection()
    {
        EnsureFolderList();
        FolderEntry selectedFolder = folders[selectedFolderIndex];
        if (selectedFolder.Subfolders == null)
        {
            selectedFolder.Subfolders = new List<string>();
        }

        string folderLabel = string.IsNullOrWhiteSpace(selectedFolder.Name) ? "Selected Folder" : selectedFolder.Name.Trim();
        EditorGUILayout.LabelField($"Subfolders for {folderLabel}", EditorStyles.boldLabel);

        if (selectedFolder.Subfolders.Count == 0)
        {
            EditorGUILayout.HelpBox("This folder has no subfolders yet.", MessageType.Info);
        }

        for (int i = 0; i < selectedFolder.Subfolders.Count; i++)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                selectedFolder.Subfolders[i] = EditorGUILayout.TextField(selectedFolder.Subfolders[i]);

                if (GUILayout.Button("-", GUILayout.Width(28f)))
                {
                    selectedFolder.Subfolders.RemoveAt(i);
                    i--;
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Subfolder"))
            {
                selectedFolder.Subfolders.Add(string.Empty);
            }

            if (GUILayout.Button("Use Scripts + Sprites"))
            {
                selectedFolder.Subfolders = new List<string> { "Scripts", "Sprites" };
            }
        }
    }

    private void DrawCreateButton()
    {
        EditorGUILayout.Space(8f);

        using (new EditorGUI.DisabledScope(!HasValidFolderName()))
        {
            if (GUILayout.Button("Create Folders", GUILayout.Height(38f)))
            {
                CreateFolders();
            }
        }

        EditorGUILayout.Space(8f);
    }

    private bool HasValidFolderName()
    {
        EnsureFolderList();

        foreach (FolderEntry folder in folders)
        {
            if (!string.IsNullOrWhiteSpace(folder.Name))
            {
                return true;
            }
        }

        return false;
    }

    private void CreateFolders()
    {
        string normalizedRoot = NormalizeAssetPath(rootPath);

        if (string.IsNullOrWhiteSpace(normalizedRoot))
        {
            normalizedRoot = DefaultRootPath;
        }

        EnsureAssetFolder(normalizedRoot);

        int createdCount = 0;
        foreach (FolderEntry folder in folders)
        {
            string cleanFolderName = SanitizeFolderName(folder.Name);
            if (string.IsNullOrWhiteSpace(cleanFolderName))
            {
                continue;
            }

            string parentPath = CombineAssetPath(normalizedRoot, cleanFolderName);
            createdCount += EnsureAssetFolder(parentPath) ? 1 : 0;

            if (folder.Subfolders == null)
            {
                continue;
            }

            foreach (string subfolderName in folder.Subfolders)
            {
                string cleanSubfolderName = SanitizeFolderName(subfolderName);
                if (string.IsNullOrWhiteSpace(cleanSubfolderName))
                {
                    continue;
                }

                string subfolderPath = CombineAssetPath(parentPath, cleanSubfolderName);
                createdCount += EnsureAssetFolder(subfolderPath) ? 1 : 0;
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Folder Forge", $"Folder creation completed.\nCreated: {createdCount}", "OK");
    }

    private void EnsureFolderList()
    {
        if (folders == null || folders.Count == 0)
        {
            folders = new List<FolderEntry> { FolderEntry.CreateDefault() };
        }

        selectedFolderIndex = Mathf.Clamp(selectedFolderIndex, 0, folders.Count - 1);

        foreach (FolderEntry folder in folders)
        {
            if (folder.Subfolders == null)
            {
                folder.Subfolders = new List<string>();
            }
        }
    }

    private static bool EnsureAssetFolder(string assetPath)
    {
        assetPath = NormalizeAssetPath(assetPath);

        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return false;
        }

        string parentPath = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
        string folderName = Path.GetFileName(assetPath);

        if (string.IsNullOrWhiteSpace(parentPath) || string.IsNullOrWhiteSpace(folderName))
        {
            return false;
        }

        if (!AssetDatabase.IsValidFolder(parentPath))
        {
            EnsureAssetFolder(parentPath);
        }

        AssetDatabase.CreateFolder(parentPath, folderName);
        return true;
    }

    private static string NormalizeAssetPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        string normalized = path.Trim().Replace("\\", "/").Trim('/');
        return normalized.StartsWith("Assets") ? normalized : CombineAssetPath(DefaultRootPath, normalized);
    }

    private static string CombineAssetPath(string left, string right)
    {
        return $"{left.TrimEnd('/')}/{right.TrimStart('/')}";
    }

    private static string SanitizeFolderName(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
        {
            return string.Empty;
        }

        string cleanName = folderName.Trim();

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            cleanName = cleanName.Replace(invalidChar, '_');
        }

        cleanName = cleanName.Replace('/', '_').Replace('\\', '_');
        return cleanName;
    }

    [System.Serializable]
    private sealed class FolderEntry
    {
        public string Name = string.Empty;
        public List<string> Subfolders = new List<string>();

        public static FolderEntry CreateDefault()
        {
            return new FolderEntry
            {
                Subfolders = new List<string> { "Scripts", "Sprites" }
            };
        }
    }
}
