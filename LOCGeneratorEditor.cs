using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class LOCGeneratorWindow : EditorWindow
{
    #region Data Model

    public class LOCFileStat
    {
        public string Path;
        public int Lines;
    }

    public class LOCResult
    {
        public List<LOCFileStat> Files = new();
        public int TotalLines;
    }

    #endregion

    #region Generator

    public static class LOCGenerator
    {
        public static LOCResult Generate(List<string> includeFolders, List<string> ignoreFolders)
        {
            var result = new LOCResult();

            var files = includeFolders
                .SelectMany(folder =>
                {
                    if (!Directory.Exists(folder)) return new string[0];
                    return Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);
                })
                .Where(f => !IsIgnored(f, ignoreFolders));

            foreach (var file in files)
            {
                int lines = File.ReadAllLines(file).Length;

                result.Files.Add(new LOCFileStat
                {
                    Path = file.Replace("\\", "/"),
                    Lines = lines
                });

                result.TotalLines += lines;
            }

            return result;
        }

        private static bool IsIgnored(string path, List<string> ignoreFolders)
        {
            path = path.Replace("\\", "/");
            return ignoreFolders.Any(ignore => path.StartsWith(ignore));
        }
    }

    #endregion

    #region Exporter

    public static class LOCExporter
    {
        public static void ExportToFile(LOCResult result, string outputPath)
        {
            if (result == null) return;

            StringBuilder sb = new();

            foreach (var f in result.Files)
            {
                sb.AppendLine($"{f.Path} - {f.Lines} lines");
            }

            sb.AppendLine($"Total lines of code: {result.TotalLines}");

            File.WriteAllText(outputPath, sb.ToString());
        }
    }

    #endregion

    #region Importer

    public static class LOCImporter
    {
        public static LOCResult ImportFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("File not found");
                return null;
            }

            var result = new LOCResult();
            var lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("Total")) continue;

                int splitIndex = line.LastIndexOf(" - ");
                if (splitIndex < 0) continue;

                string filePath = line.Substring(0, splitIndex);
                string right = line.Substring(splitIndex + 3);

                int lineCount = ParseLineCount(right);

                result.Files.Add(new LOCFileStat
                {
                    Path = filePath,
                    Lines = lineCount
                });

                result.TotalLines += lineCount;
            }

            return result;
        }

        private static int ParseLineCount(string text)
        {
            var number = new string(text.TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(number, out int value) ? value : 0;
        }
    }

    #endregion

    #region Window

    private List<string> includeFolders = new();
    private List<string> ignoreFolders = new();

    private LOCResult currentResult;

    private Vector2 scroll;
    private Vector2 resultScroll;

    private const string PREF_INCLUDE = "LOC_INCLUDE";
    private const string PREF_IGNORE = "LOC_IGNORE";

    private string outputPath = "LOC_Report.txt";

    private enum SortMode
    {
        LinesDesc,
        LinesAsc,
        Path
    }

    private SortMode sortMode = SortMode.LinesDesc;

    [MenuItem("Tools/LOC Generator")]
    public static void ShowWindow()
    {
        GetWindow<LOCGeneratorWindow>("LOC Generator");
    }

    private void OnEnable()
    {
        includeFolders = LoadList(PREF_INCLUDE);
        ignoreFolders = LoadList(PREF_IGNORE);

        if (includeFolders.Count == 0)
            includeFolders.Add("Assets");
    }

    private void OnDisable()
    {
        SaveList(PREF_INCLUDE, includeFolders);
        SaveList(PREF_IGNORE, ignoreFolders);
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        DrawFolderList("Include Folders", includeFolders);
        GUILayout.Space(10);
        DrawFolderList("Ignore Folders", ignoreFolders);

        GUILayout.Space(10);

        outputPath = EditorGUILayout.TextField("Output File", outputPath);

        GUILayout.Space(10);

        sortMode = (SortMode)EditorGUILayout.EnumPopup("Sort By", sortMode);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate", GUILayout.Height(35)))
        {
            Generate();
        }

        if (GUILayout.Button("Export File", GUILayout.Height(35)))
        {
            Export();
        }

        if (GUILayout.Button("Import File", GUILayout.Height(35)))
        {
            Import();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (currentResult != null)
        {
            EditorGUILayout.LabelField($"Total LOC: {currentResult.TotalLines}", EditorStyles.boldLabel);

            if (currentResult.Files.Count > 0)
            {
                var biggest = currentResult.Files[0];
                EditorGUILayout.LabelField($"Biggest File: {biggest.Path} ({biggest.Lines})");
            }

            GUILayout.Space(10);

            DrawResults();
        }

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region Logic

    private void Generate()
    {
        currentResult = LOCGenerator.Generate(includeFolders, ignoreFolders);
        SortResults();
    }

    private void Export()
    {
        if (currentResult == null)
        {
            Debug.LogWarning("Generate first!");
            return;
        }

        LOCExporter.ExportToFile(currentResult, outputPath);
        AssetDatabase.Refresh();

        Debug.Log("LOC exported");
    }

    private void Import()
    {
        string path = EditorUtility.OpenFilePanel("Import LOC Report", "", "txt");

        if (string.IsNullOrEmpty(path))
            return;

        currentResult = LOCImporter.ImportFromFile(path);

        if (currentResult != null)
        {
            SortResults();
            Debug.Log("LOC imported");
        }
    }

    private void SortResults()
    {
        if (currentResult == null) return;

        switch (sortMode)
        {
            case SortMode.LinesDesc:
                currentResult.Files = currentResult.Files.OrderByDescending(x => x.Lines).ToList();
                break;
            case SortMode.LinesAsc:
                currentResult.Files = currentResult.Files.OrderBy(x => x.Lines).ToList();
                break;
            case SortMode.Path:
                currentResult.Files = currentResult.Files.OrderBy(x => x.Path).ToList();
                break;
        }
    }

    #endregion

    #region UI Helpers

    private void DrawResults()
    {
        resultScroll = EditorGUILayout.BeginScrollView(resultScroll, GUILayout.Height(300));

        foreach (var r in currentResult.Files)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Ping", GUILayout.Width(40)))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(r.Path);
                EditorGUIUtility.PingObject(obj);
            }

            EditorGUILayout.LabelField(r.Path, GUILayout.MaxWidth(position.width - 150));
            EditorGUILayout.LabelField($"{r.Lines}", GUILayout.Width(60));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawFolderList(string title, List<string> list)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            list[i] = EditorGUILayout.TextField(list[i]);

            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selected) && selected.Contains(Application.dataPath))
                {
                    list[i] = "Assets" + selected.Replace(Application.dataPath, "");
                }
            }

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                list.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Folder"))
        {
            list.Add("Assets");
        }
    }

    #endregion

    #region Save / Load

    private List<string> LoadList(string key)
    {
        string data = EditorPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(data)) return new List<string>();
        return data.Split('|').ToList();
    }

    private void SaveList(string key, List<string> list)
    {
        EditorPrefs.SetString(key, string.Join("|", list));
    }

    #endregion
}
