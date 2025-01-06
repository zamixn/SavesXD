
namespace FrameworksXD.SavesXD
{
    public interface ISavable<SaveDataType>
    {
        public void Save(SaveDataType saveData);
        public void Load(SaveDataType saveData);
    }
}
