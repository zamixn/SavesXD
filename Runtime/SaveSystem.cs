using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json;

namespace FrameworksXD.SavesXD
{
    public abstract class SaveSystem<SaveDataType, ConfigFileDataType> : MonoBehaviour
    where SaveDataType : SaveData
    where ConfigFileDataType : ConfigFileData
    {
        [System.Serializable]
        public class SaveSystemConfig
        {
            public int SaveSlotCount = SaveSystemConstants.DefaultSaveSlotCount;
            public string SaveFileExtension = SaveSystemConstants.DefaultSaveFileExtension;
            public string SaveFileNamePrefix = SaveSystemConstants.DefaultSaveFileNamePrefix;
            public string SaveHeaderFileNamePrefix = SaveSystemConstants.DefaultSaveHeaderFileNamePrefix;
            public string ConfigFileName = SaveSystemConstants.DefaultConfigFileName;

            public int MaxSavablesToProcessPerFrame = SaveSystemConstants.MaxSavablesToProcessPerFrame;
        }

        public static SaveSystem<SaveDataType, ConfigFileDataType> Instance { get; private set; }

        [SerializeField] private SaveSystemConfig Config;

        private IFileIOHandler FileSaver;
        private bool IsInitted => FileSaver != null;

        private List<ISavable<SaveDataType>> RegisteredSavables;
        public SaveDataType CurrentSaveData { get; protected set; }
        public ConfigFileDataType CurrentConfigFileData { get; protected set; }

        private bool CurrentlySavingData;
        private bool CurrentlyLoadingData;

        private bool CurrentlySavingConfig;
        private bool CurrentlyLoadingConfig;

        protected void Initialize(IFileIOHandler fileIOHandler)
        {
            if (Instance != null)
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} Duplicate instance of SaveeSystem MonoBehaviour");
                return;
            }
            Instance = this;
            FileSaver = fileIOHandler;
            RegisteredSavables = new List<ISavable<SaveDataType>>();
            Debug.Log($"{SaveSystemConstants.LOG_PREFIX} Initialized");
        }

        public void RegisterSavable(ISavable<SaveDataType> savable)
        {
            RegisteredSavables.Add(savable);
        }

        public void UnregisterSavable(ISavable<SaveDataType> savable)
        {
            RegisteredSavables.Remove(savable);
        }

        #region save data handling
        public bool IsCurrentlySavingData() => CurrentlySavingData;
        public bool IsCurrentlyLoadingData() => CurrentlyLoadingData;

        public void SetCurrentSaveData(SaveDataType saveData)
        {
            CurrentSaveData = saveData;
        }

        public void SaveCurrentSaveData(bool saveToFile, System.Action<bool> callback)
        {
            if (IsCurrentlySavingData())
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} saving is already in progress, can't start another saving");
                callback?.Invoke(false);
                return;
            }
            if (IsCurrentlyLoadingData())
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} loading is in progress, can't start saving");
                callback?.Invoke(false);
                return;
            }
            if (CurrentSaveData == null)
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} save file not loaded or created, cannot save game");
                callback?.Invoke(false);
                return;
            }

            CurrentlySavingData = true;
            StartCoroutine(SaveCurrentSaveDataRoutine(saveToFile,
                (bool success) =>
                {
                    CurrentlySavingData = false;
                    callback?.Invoke(success);
                }));
        }

        private IEnumerator SaveCurrentSaveDataRoutine(bool saveToFile, System.Action<bool> callback)
        {
            OnPreSave();
            yield return null;

            var maxToProcess = Config.MaxSavablesToProcessPerFrame;
            var processedTemp = 0;

            var count = RegisteredSavables.Count;
            for (int i = 0; i < count; i++)
            {
                var savable = RegisteredSavables[i];
                savable.Save(CurrentSaveData);
                processedTemp++;

                if(processedTemp > maxToProcess)
                {
                    processedTemp = 0;
                    yield return null;
                }
            }
            yield return null;

            if (saveToFile)
                SaveToFile(CurrentSaveData, callback);
            else
                callback?.Invoke(true);
        }

        protected abstract void OnPreSave();

        public void LoadSaveFile(int saveSlotIndex, System.Action<bool, SaveDataType> callback)
        {
            if (IsCurrentlySavingData())
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} saving is already in progress, can't start another saving");
                callback?.Invoke(false, null);
                return;
            }
            if (IsCurrentlyLoadingData())
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} loading is in progress, can't start saving");
                callback?.Invoke(false, null);
                return;
            }

            CurrentlyLoadingData = true;
            LoadFromFile(saveSlotIndex,
                (SaveDataType saveData) =>
                {
                    CurrentlyLoadingData = false;
                    callback?.Invoke(saveData != null, saveData);
                });
        }

        public void LoadCurrentSaveData(System.Action<bool> callback)
        {
            if (IsCurrentlySavingData())
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} saving is already in progress, can't start another saving");
                callback?.Invoke(false);
                return;
            }
            if (IsCurrentlyLoadingData())
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} loading is in progress, can't start saving");
                callback?.Invoke(false);
                return;
            }
            if (CurrentSaveData == null)
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} save file not loaded or created, cannot load game");
                callback?.Invoke(false);
                return;
            }

            CurrentlyLoadingData = true;
            StartCoroutine(LoadCurrentSaveDataRoutine(
                (bool success) =>
                {
                    CurrentlyLoadingData = false;
                    callback?.Invoke(success);
                }));
        }

        private IEnumerator LoadCurrentSaveDataRoutine(System.Action<bool> callback)
        {
            OnPreLoad();
            yield return null;

            var maxToProcess = Config.MaxSavablesToProcessPerFrame;
            var processedTemp = 0;

            var count = RegisteredSavables.Count;
            for (int i = 0; i < count; i++)
            {
                var savable = RegisteredSavables[i];
                savable.Load(CurrentSaveData);

                processedTemp++;
                if (processedTemp > maxToProcess)
                {
                    processedTemp = 0;
                    yield return null;
                }
            }
            yield return null;
            callback.Invoke(true);
        }

        protected abstract void OnPreLoad();
        #endregion

        #region config handling

        public bool IsCurrentlySavingConfig() => CurrentlySavingConfig;
        public bool IsCurrentlyLoadingConfig() => CurrentlyLoadingConfig;

        public void DoesConfigExist(System.Action<bool> callback)
        {
            if (!IsInitted)
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} SaveSystem is not initialized");
                callback.Invoke(false);
            }

            FileSaver.FileExists(Config.ConfigFileName,
                (existsData) =>
                {
                    if (!existsData.Success)
                    {
                        Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} Loading '{Config.ConfigFileName}' failed, file does not exist");
                        callback.Invoke(false);
                        return;
                    }

                    callback.Invoke(existsData.FileExists);
                });
        }

        public void SetCurrentConfigData(ConfigFileDataType configData)
        {
            CurrentConfigFileData = configData;
        }

        public void SaveCurrentConfig(bool saveToFile, System.Action<bool> callback)
        {
            if (IsCurrentlySavingConfig())
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} config saving is already in progress, can't start another saving");
                callback?.Invoke(false);
                return;
            }
            if (IsCurrentlyLoadingConfig())
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} config loading is in progress, can't start saving");
                callback?.Invoke(false);
                return;
            }
            if (CurrentConfigFileData == null)
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} config data not loaded or created, cannot save it");
                callback?.Invoke(false);
                return;
            }

            CurrentlySavingConfig = true;
            if (saveToFile)
                SaveConfig(CurrentConfigFileData, 
                    (bool success) =>
                    {
                        CurrentlySavingConfig = false;
                        callback.Invoke(success);
                    });
            else
                callback?.Invoke(true);
        }

        public void LoadCurrentConfig(System.Action<bool> callback)
        {
            if (IsCurrentlySavingConfig())
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} config saving is already in progress, can't start to load");
                callback?.Invoke(false);
                return;
            }
            if (IsCurrentlyLoadingConfig())
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} config loading is in progress, can't start saving");
                callback?.Invoke(false);
                return;
            }

            CurrentlyLoadingConfig = true;
            LoadConfigFile((ConfigFileDataType config) =>
                {
                    CurrentlyLoadingConfig = false;
                    CurrentConfigFileData = config;
                    callback.Invoke(config != null);
                });
        }
        #endregion

        #region saving/loading to file
        private string GetSaveFileName(int index)
        {
            return $"{Config.SaveFileNamePrefix}{index}.{Config.SaveFileExtension}";
        }
        private string GetSaveHeaderName(int index)
        {
            return $"{Config.SaveHeaderFileNamePrefix}{index}.{Config.SaveFileExtension}";
        }

        private void LoadFromFile(int index, System.Action<SaveDataType> callback)
        {
            if (!IsInitted)
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} SaveSystem is not initialized");
                callback.Invoke(null);
                return;
            }

            var fileName = GetSaveFileName(index);
            FileSaver.FileExists(fileName, 
                (existsData) => 
                {
                    if(!existsData.Success)
                    {
                        Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} Loading '{fileName}' failed, file does not exist");
                        callback.Invoke(null);
                        return;
                    }

                    FileSaver.LoadFile(fileName,
                        (loadData) =>
                        {
                            if (!loadData.Success)
                            {
                                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} Loading '{fileName}' failed");
                                callback.Invoke(null);
                                return;
                            }

                            var bytes = loadData.FileBytes;
                            var stringData = System.Text.Encoding.UTF8.GetString(bytes);
                            var saveData = JsonConvert.DeserializeObject<SaveDataType>(stringData);
                            callback.Invoke(saveData);
                        });
                });
        }

        private void SaveToFile(SaveDataType saveData, System.Action<bool> callback)
        {
            if (!IsInitted)
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} SaveSystem is not initialized");
                callback.Invoke(false);
                return;
            }

            var fileName = GetSaveFileName(saveData.SaveIndex);
            var stringData = JsonConvert.SerializeObject(saveData);
            var bytes = System.Text.Encoding.UTF8.GetBytes(stringData);
            FileSaver.SaveFile(fileName, bytes, 
                (saveData) =>
                {
                    if (!saveData.Success)
                    {
                        Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} Saving '{fileName}' failed");
                        callback.Invoke(false);
                        return;
                    }
                    callback.Invoke(saveData.Success);
                });
        }

        private void TryGetNextSaveIndex(System.Action<bool, int> callback)
        {
            if (!IsInitted)
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} SaveSystem is not initialized");
                callback.Invoke(false, -1);
                return;
            }


            int i = 0;
            System.Action<FileExistsCallbackData> existsCallback = null;
            existsCallback = (FileExistsCallbackData existsData) =>
            {
                if (!existsData.Success)
                {
                    Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} failed to check if a file exists");
                }

                if (existsData.Success && !existsData.FileExists)
                    callback.Invoke(true, i);
                else if(i == Config.SaveSlotCount)
                    callback.Invoke(false, -1);
                else
                {
                    i++;
                    FileSaver.FileExists(GetSaveFileName(i), existsCallback);
                }

            };

            FileSaver.FileExists(GetSaveFileName(i), existsCallback);
        }

        private void LoadConfigFile(System.Action<ConfigFileDataType> callback)
        {
            if (!IsInitted)
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} SaveSystem is not initialized");
                callback.Invoke(null);
            }

            FileSaver.FileExists(Config.ConfigFileName,
                (existsData) =>
                {
                    if (!existsData.Success)
                    {
                        Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} Loading '{Config.ConfigFileName}' failed, file does not exist");
                        callback.Invoke(null);
                        return;
                    }

                    FileSaver.LoadFile(Config.ConfigFileName,
                        (loadData) =>
                        {
                            if (!loadData.Success)
                            {
                                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} Loading '{Config.ConfigFileName}' failed");
                                callback.Invoke(null);
                                return;
                            }

                            var bytes = loadData.FileBytes;
                            var stringData = System.Text.Encoding.UTF8.GetString(bytes);
                            var configData = JsonConvert.DeserializeObject<ConfigFileDataType>(stringData);
                            callback.Invoke(configData);
                        });
                });
        }

        private void SaveConfig(ConfigFileDataType config, System.Action<bool> callback)
        {
            if (!IsInitted)
            {
                Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} SaveSystem is not initialized");
                return;
            }

            var stringData = JsonConvert.SerializeObject(config);
            var bytes = System.Text.Encoding.UTF8.GetBytes(stringData);
            FileSaver.SaveFile(Config.ConfigFileName, bytes,
                (saveData) =>
                {
                    if (!saveData.Success)
                    {
                        Debug.LogError($"{SaveSystemConstants.LOG_PREFIX} Saving '{Config.ConfigFileName}' failed");
                        callback.Invoke(false);
                        return;
                    }
                    callback.Invoke(saveData.Success);
                });
        }
        #endregion
    }
}