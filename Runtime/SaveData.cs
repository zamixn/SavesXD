using System.Collections.Generic;

namespace FrameworksXD.SavesXD
{
    [System.Serializable]
    public class SaveData
    {
        public string SaveName;
        public int SaveIndex;
        public bool IsSaveFresh;
        public Dictionary<string, string> Data;

        public SaveData() { }

        public SaveData(int saveIndex, string saveName)
        {
            Data = new Dictionary<string, string>();
            IsSaveFresh = true;
            SaveIndex = saveIndex;
            SaveName = saveName;
        }

        public void Save(string key, string data)
        {
            if (Data.ContainsKey(key))
                Data[key] = data;
            else
                Data.Add(key, data);
        }

        public string Load(string key, string defaultData = "")
        {
            if (Data.ContainsKey(key))
                return Data[key];
            return defaultData;
        }

        public virtual string ToShortString()
        {
            return $"Save: {SaveName} ({SaveIndex})";
        }

        public virtual string ToFullString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"- SaveName: {SaveName}\n- SaveIndex: {SaveIndex}\n");
            sb.Append($"- IsSaveFresh: {IsSaveFresh}\n- SaveIndex: {SaveIndex}\n");
            sb.Append("\n- Data:\n");
            foreach (var d in Data)
            {
                sb.Append($"\t\t- {d.Key}: {d.Value}\n");
            }
            return sb.ToString();
        }
    }
}