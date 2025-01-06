namespace FrameworksXD.SavesXD
{
    [System.Serializable]
    public class ConfigFileData
    {
        public int PreviousLoadedSaveFile;

        public ConfigFileData()
        {
            PreviousLoadedSaveFile = -1;
        }

        public bool IsPreviousSaveFileSet() => PreviousLoadedSaveFile != -1;
        public int GetPreviousSaveFile() => PreviousLoadedSaveFile;
        public void SetPreviousSaveFile(int saveIndex) => PreviousLoadedSaveFile = saveIndex;
    }
}
