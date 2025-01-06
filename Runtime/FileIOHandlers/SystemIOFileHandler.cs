using System.IO;
using UnityEngine;

namespace FrameworksXD.SavesXD
{
    public class SystemIOFileHandler : IFileIOHandler
    {
        private static string SaveDataLocation => Application.persistentDataPath;

        private string GetFilePath(string fileName) => $"{SaveDataLocation}/{fileName}";

        public void SaveFile(string fileName, byte[] data, System.Action<SaveFileCallbackData> callback)
        {
            File.WriteAllBytes(GetFilePath(fileName), data);
            callback.Invoke(new SaveFileCallbackData() { Success = true });
        }

        public void LoadFile(string fileName, System.Action<LoadFileCallbackData> callback)
        {
            callback.Invoke(new LoadFileCallbackData()
            {
                Success = true,
                FileBytes = File.ReadAllBytes(GetFilePath(fileName))
            });
        }

        public void FileExists(string fileName, System.Action<FileExistsCallbackData> callback)
        {
            callback.Invoke(new FileExistsCallbackData()
            { 
                Success = true,
                FileExists = File.Exists(GetFilePath(fileName))
            });
        }
    }
}