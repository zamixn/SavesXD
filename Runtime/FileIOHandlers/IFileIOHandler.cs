namespace FrameworksXD.SavesXD
{
    public struct SaveFileCallbackData
    {
        public bool Success;
    }

    public struct LoadFileCallbackData
    {
        public bool Success;
        public byte[] FileBytes;
    }

    public struct FileExistsCallbackData
    {
        public bool Success;
        public bool FileExists;
    }

    public interface IFileIOHandler
    {
        public void SaveFile(string fileName, byte[] data, System.Action<SaveFileCallbackData> callback);
        public void LoadFile(string fileName, System.Action<LoadFileCallbackData> callback);
        public void FileExists(string fileName, System.Action<FileExistsCallbackData> callback);
    }
}
