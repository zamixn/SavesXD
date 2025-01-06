using UnityEngine;
using System.IO;
using UnityEditor;
using Newtonsoft.Json;

namespace FrameworksXD.SavesXD.Editor
{
    public static class MenuItems
    {
        private static string SaveDataLocation => Application.persistentDataPath;

        [MenuItem("SaveData/OpenSaveLocation")]
        public static void Show()
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                Arguments = SaveDataLocation,
                FileName = "explorer.exe"
            };

            System.Diagnostics.Process.Start(startInfo);
        }

        [MenuItem("SaveData/PrintSaveData")]
        public static void PrintSaveData()
        {
            var path = EditorUtility.OpenFilePanel("Select Save File", SaveDataLocation, "*");
            if (path == null || path == "")
                return;

            var outputPath = EditorUtility.OpenFilePanel("Select output file", Directory.GetCurrentDirectory(), "txt");
            if (outputPath == null || outputPath == "")
                return;

            var bytes = File.ReadAllBytes(path);
            var jsonData = System.Text.Encoding.UTF8.GetString(bytes);
            var saveData = JsonConvert.DeserializeObject<SaveData>(jsonData);

            File.WriteAllText(outputPath, saveData.ToFullString());

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C notepad++.exe {outputPath}"
            };
            System.Diagnostics.Process.Start(startInfo);
        }

        [MenuItem("SaveData/DeleteSaveData")]
        public static void EditorDeleteSaveData()
        {
            var path = EditorUtility.OpenFilePanel("Select Save File", SaveDataLocation, "*");
            if (path == null || path == "")
                return;

            System.IO.File.Delete(path);
            Debug.Log($"Save File deleted: {path}");
        }

        [MenuItem("SaveData/DeleteAllSaveData")]
        public static void EditorDeleteAllSaveData()
        {
            var files = System.IO.Directory.GetFiles(SaveDataLocation);
            var fileListString = "";
            foreach (var f in files)
            {
                fileListString += $"{f}\n";
            }
            if (EditorUtility.DisplayDialog("Files to delete", fileListString, "Delete", "Cancel"))
            {
                foreach (var f in files)
                {
                    System.IO.File.Delete(f);
                }
                Debug.Log($"Deleted files: {fileListString}");
            }
        }

        [MenuItem("SaveData/PrintSaveLocation")]
        public static void EditorPrintSaveLocation()
        {
            Debug.Log(SaveDataLocation);
        }
    }
}