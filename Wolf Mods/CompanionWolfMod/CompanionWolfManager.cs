﻿using UnityEngine;
using Il2CppInterop.Runtime;
using MelonLoader.TinyJSON;
using Il2CppTLD.AddressableAssets;
using static ExpandedAiFramework.Utility;



namespace ExpandedAiFramework.CompanionWolfMod
{
    public class CompanionWolfManager : ISubManager
    {
        public const string WolfPrefabString = "WILDLIFE_Wolf";

        protected EAFManager mManager;
        protected GameObject mWolfPrefab;
        protected Transform mTamedSpawnTransform;
        protected CompanionWolf mInstance;
        protected CompanionWolfData mData;
        protected bool mInitialized = false;
        protected bool mShouldCheckForSpawnTamedCompanion = false;
        protected float mLastTriggeredCheckForSpawnTamedCompanionTime = 0.0f;
        protected bool mShouldShowInfoScreen = false;

        public CompanionWolfData Data { get { return mData; } set { mData = value; } }
        public CompanionWolf Instance { get { return mInstance; } set { mInstance = value; } }
        public Type SpawnType { get { return typeof(CompanionWolf); } }
        public GameObject WolfPrefab { get { return mWolfPrefab; } set { mWolfPrefab = value; } }
        public bool ShouldShowInfoScreen { get { return mShouldShowInfoScreen; } }



        public void Initialize(EAFManager manager)
        {
            mManager = manager;
            mInitialized = true;
            LogDebug("CompanionWolfManager initialized!");
        }


        public bool ShouldInterceptSpawn(BaseAi baseAi, SpawnRegion region)
        {
            SpawnCompanion();
            if (mData == null)
            {
                LogDebug($"No data setup, will not intercept spawn. How the fuck did we get here before data loading anyways?");
                return false;
            }
            if (!mData.Connected)
            {
                LogDebug($"No connected instance, will not intercept spawn");
                return false;
            }
            if (mInstance != null)
            {
                LogDebug($"Active instance, will not intercept spawn");
                return false;
            }
            if (baseAi == null)
            {
                LogDebug($"Null baseAi, will not intercept spawn");
                return false;
            }
            if (region == null)
            {
                LogDebug($"Null SpawnRegion, will not intercept spawn");
                return false;
            }
            if (mData.SpawnRegionModDataProxy == null)
            {
                LogDebug($"Null proxy, will not intercept spawn");
                return false;
            }
            if (mData.SpawnRegionModDataProxy.Scene != GameManager.m_ActiveScene
                || Vector3.Distance(mData.SpawnRegionModDataProxy.Position, region.transform.position) > 0.001f
                || mData.SpawnRegionModDataProxy.AiType != baseAi.m_AiType
                || mData.SpawnRegionModDataProxy.AiSubType != baseAi.m_AiSubType)
            {
                LogDebug($"Proxy mismatch, will not intercept spawn");
                return false;
            }

            LogDebug($"Proxy match to connected CompanionWolf data found, overriding WeightedTypePicker and spawning companionwolf where it first spawned {GetCurrentTimelinePoint() - Data.SpawnDate} hours ago!");
            return true;
        }


        public void Shutdown()
        {
            mData = null;
        }


        public void OnStartNewGame()
        {
            mData = new CompanionWolfData();
            //OnSaveGame();
        }


        public void OnLoadGame()
        {
            if (mData == null)
            {
                mData = new CompanionWolfData();
            }

            string json = mManager.ModData.Load("CompanionWolfMod");
            if (json != null)
            {
                Variant variant = JSON.Load(json);
                if (variant != null)
                {
                    LogDebug($"Successfully loaded previously saved CompanionWolfData!");
                    JSON.Populate(variant, mData);
                }
            }

            LogDebug($"Tamed: {mData.Tamed} | Calories: {mData.CurrentCalories} | Affection: {mData.CurrentAffection} | Outdoors: {GameManager.m_ActiveSceneSet.m_IsOutdoors}");
        }


        public void OnLoadScene()
        {
            if (mInstance != null) //this is too late, by now system has serialized the wolf for later. gotta stop it there
            {
                GameObject.Destroy(mInstance.gameObject.transform.parent.gameObject); //destroy the whole thing
            }
        }


        public void OnInitializedScene()
        {
            if (GameManager.m_ActiveSceneSet != null && GameManager.m_ActiveSceneSet.m_IsOutdoors && mData != null && mData.Tamed && mInstance == null)
            {
                mShouldCheckForSpawnTamedCompanion = true;
                mLastTriggeredCheckForSpawnTamedCompanionTime = Time.time;
            }
        }


        public void OnSaveGame()
        {
            mData.LastDespawnTime = GetCurrentTimelinePoint();
            string json = JSON.Dump(mData);
            if (json != null)
            {
                mManager.ModData.Save(json, "CompanionWolfMod");
            }
        }


        public void Update()
        {
            if (mShouldCheckForSpawnTamedCompanion && Time.time - mLastTriggeredCheckForSpawnTamedCompanionTime > 2.0f)
            {
                mShouldCheckForSpawnTamedCompanion = false;
                SpawnCompanion();
            }
            if (mInstance != null)
            {
                mInstance.UpdateStatusText();
            }
        }


        public void SpawnCompanion()
        {
            if (Data == null)
            {
                LogDebug("No data found, cannot spawn companion!");
                return;
            }
            if (!Data.Tamed)
            {
                LogDebug("Companion is not tamed, go find and tame one!");
                return;
            }
            if (mInstance != null)
            {
                LogDebug("Companion is already here!");
                return;
            }
            GameObject wolfContainer = new GameObject("CompanionWolfContainer");
            Vector3 playerPos = GameManager.m_PlayerManager.m_LastPlayerPosition;
            AiUtils.GetClosestNavmeshPos(out Vector3 validPos, playerPos, playerPos);
            GameObject newWolf = AssetHelper.SafeInstantiateAssetAsync(WolfPrefabString).WaitForCompletion();
            newWolf.transform.position = validPos;
            LogDebug("Successfully instantiated: " + newWolf.name);
            if (newWolf == null)
            {
                LogWarning("Couldn't instantiate new wolf prefab!");
                return;
            }
            newWolf.transform.position = validPos;
            BaseAi baseAi = newWolf.GetComponentInChildren<BaseAi>();
            if (baseAi == null)
            {
                LogError("Coult not find BaseAi script attached to wolf prefab!");
                return;
            }
            LogDebug($"Creating move agent...");
            baseAi.CreateMoveAgent(wolfContainer.transform);
            LogDebug($"Reparenting...");
            baseAi.ReparentBaseAi(wolfContainer.transform);
            LogDebug($"Wrapping...");
            if (!mManager.TryInjectCustomAi(baseAi, Il2CppType.From(typeof(CompanionWolf)), null))
            {
                return;
            }
            LogDebug($"re-grabbing wrapper..");
            if (!mManager.CustomAis.TryGetValue(baseAi.GetHashCode(), out ICustomAi wrapper))
            {
                LogError("Did not find new wrapper for new base ai!");
                return;
            }
            LogDebug($"Grabbing Instance..");
            mInstance = wrapper as CompanionWolf;
            if (mInstance == null)
            {
                LogError("Instantiated companion wolf but script is not correct!");
                return;
            }
            wrapper.BaseAi.m_MoveAgent.transform.position = validPos;
            wrapper.BaseAi.m_MoveAgent.Warp(validPos, 5.0f, true, -1);
            mShouldCheckForSpawnTamedCompanion = false;
            BaseAiManager.Remove(wrapper.BaseAi);
            LogDebug($"Companion wolf loaded!");
        }



        #region console commands

        public const string CWolfCommandString = "cwolf";
        public const string CWolfCommandString_Tamed = "tamed";
        public const string CWolfCommandString_Untamed = "untamed";

        public const string CWoldCommandString_OnCommandSupportedTypes =
                                                $"{CommandString_Help}" +
                                                $"{CommandString_Create} " +
                                                $"{CommandString_Delete} " +
                                                $"{CommandString_GoTo} " +
                                                $"{CommandString_Spawn}" + 
                                                $"{CommandString_Info} ";


        public const string CWoldCommandString_HelpSupportedTypes =
                                         $"{CommandString_Create} " +
                                         $"{CommandString_Delete} " +
                                         $"{CommandString_GoTo} " +
                                         $"{CommandString_Spawn}" +
                                         $"{CommandString_Info} ";


        public const string CWoldCommandString_CreateTypes =
                         $"{CWolfCommandString_Tamed} " +
                         $"{CWolfCommandString_Untamed} ";





        internal static void Console_OnCommand()
        {
            if (!Manager.SubManagers.TryGetValue(typeof(CompanionWolf), out ISubManager subManager))
            {
                LogError("Could not fetch CompanionWolfManager instance!");
                return;
            }

            if (subManager is not CompanionWolfManager instance)
            {
                LogError("Could not fetch CompanionWolfManager instance!");
                return;
            }

            string command = uConsole.GetString().ToLowerInvariant();
            if (command == null)
            {
                LogAlways($"Available commands: {CWoldCommandString_OnCommandSupportedTypes}");
            }
            switch (command)
            {
                case CommandString_Help: instance.Console_Help(); break;
                case CommandString_Create: instance.Console_Create(); break;
                case CommandString_Delete: instance.Console_Delete(); break;
                case CommandString_GoTo: instance.Console_GoTo(); break;
                case CommandString_Spawn: instance.Console_Spawn(); break;
                case CommandString_Info: instance.Console_Info(); break;
                default: LogWarning($"Unknown command: {command}"); break;
            }
        }


        private void Console_Help()
        {
            string command = uConsole.GetString();
            if (command == null || command.Length == 0)
            {
                LogAlways($"Supported commands: {CWoldCommandString_HelpSupportedTypes}");
                return;
            }
            switch (command.ToLower())
            {
                case CommandString_Create:
                    LogAlways($"Attempts to create an tamed or untambed companion. Syntax: '{CWolfCommandString} {CommandString_Create} <type>'. Supported types: {CWoldCommandString_CreateTypes}");
                    return;
                case CommandString_Delete:
                    LogAlways($"Attempts to disconnect current tamed or untamed companion. Syntax: '{CWolfCommandString} {CommandString_Delete}'");
                    return;
                case CommandString_GoTo:
                    LogAlways($"Attempts to teleport current tamed or untamed companion. Syntax: '{CommandString} {CommandString_GoTo}'");
                    return;
                case CommandString_Spawn:
                    LogAlways($"Attempts to spawn current tamed or untamed companion. Syntax: '{CommandString} {CommandString_Spawn}'");
                    return;
                case CommandString_Info:
                    LogAlways($"Attempts to readout info on current tamed or untamed companion. Syntax: '{CommandString} {CommandString_Info}'");
                    return;
                default:
                    LogAlways($"Unknown comand '{command.ToLower()}'!");
                    return;
            }
        }



        public void Console_Create()
        {
            if (mData == null)
            {
                LogAlways($"No data to {CommandString_Create}!");
                return;
            }
            if (mData.Connected)
            {
                LogAlways($"Companion wolf already created! To force switch state, delete current and re-create in preferred state.");
                return;
            }

            string type = uConsole.GetString();
            if (!IsTypeSupported(type, CommandString_CreateSupportedTypes)) return;

            switch (type)
            {
                case CWolfCommandString_Untamed:
                    ForceCreateUntamedCompanionWolf();
                    return;
                case CWolfCommandString_Tamed:
                    ForceCreateTamedCompanionWolf();
                    return;
                default:
                    LogAlways($"Unknown type '{type}'!");
                    return;
            }
        }


        public void Console_Delete()
        {
            if (mData == null)
            {
                LogAlways($"No data to {CommandString_Delete}!");
                return;
            }
            if (!mData.Connected)
            {
                LogAlways($"No connected companion wolf to {CommandString_Delete}!");
                return;
            }
            if (mInstance != null)
            {
                GameObject.Destroy(mInstance);
            }
            mData.Disconnect();
            LogAlways($"{CommandString_Delete} companion wolf successful!");
        }


        public void Console_GoTo()
        {
            if (mData == null)
            {
                LogAlways($"No data to {CommandString_GoTo}!");
                return;
            }
            if (!mData.Connected)
            {
                LogAlways($"No connected companion wolf to {CommandString_GoTo}!");
                return;
            }
            if (mInstance == null)
            {
                LogAlways($"No spawned companion wolf to {CommandString_GoTo}!");
                return;
            }
            Manager.Teleport(mInstance.transform.position, mInstance.transform.rotation);
            LogAlways($"{CommandString_GoTo} companion wolf successful!");
        }


        public void Console_Spawn()
        {
            if (mData == null)
            {
                LogAlways($"No data to {CommandString_Spawn}!");
                return;
            }
            if (!mData.Connected)
            {
                LogAlways($"No connected companion wolf to {CommandString_Spawn}!");
                return;
            }
            if (mInstance != null)
            {
                LogAlways($"Companion wolf is already in scene, cannot {CommandString_Spawn}!");
                return;
            }
            SpawnCompanion();
            LogAlways($"{CommandString_Spawn} companion wolf successful!");
        }



        public void Console_Info()
        {
            if (mData == null)
            {
                LogAlways($"No data to {CommandString_Info}!");
                return;
            }
            if (!mData.Connected)
            {
                LogAlways($"No connected companion wolf to {CommandString_Info}!");
                return;
            }
            mShouldShowInfoScreen = !mShouldShowInfoScreen;
            LogAlways($"{CommandString_Info} companion wolf successful!");
        }


        private void ForceCreateUntamedCompanionWolf()
        {
            LogAlways($"Havent created this yet, set the spawn weight high and fly around. Eventually when I support custom spawn region creation this command will create one next to the player to respawn it until it disappears after settings timer.");
            return;
        }


        private void ForceCreateTamedCompanionWolf()
        {
            mData.Connected = true;
            mData.Tamed = true;
            mData.Initialize(null);
            mData.CurrentAffection = CompanionWolf.Settings.AffectionRequirement;
            mData.CurrentCalories = CompanionWolf.Settings.MaximumCalorieIntake * 0.5f;
            SpawnCompanion();
        }

        #endregion
    }
}


