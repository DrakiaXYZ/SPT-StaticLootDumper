using Aki.Reflection.Patching;
using BepInEx;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace DrakiaXYZ.StaticLootDumper
{
    [BepInPlugin("xyz.drakia.staticlootdumper", "DrakiaXYZ-StaticLootDumper", "1.0.0")]
    public class StaticLootDumperPlugin : BaseUnityPlugin
    {
        public static string PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string DumpFolder = Path.Combine(PluginFolder, "StaticDumps");

        private void Awake()
        {
            Directory.CreateDirectory(DumpFolder);
            new DumpLoot().Enable();
        }
    }

    public class DumpLoot : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPrefix]
        public static void PatchPrefix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            string mapName = gameWorld.MainPlayer.Location.ToLower();

            var containersData = new SPTContainersData();

            Resources.FindObjectsOfTypeAll(typeof(LootableContainersGroup)).ExecuteForEach(obj =>
            {
                var containersGroup = (LootableContainersGroup)obj;
                var sptContainersGroup = new SPTContainersGroup { minContainers = containersGroup.Min, maxContainers = containersGroup.Max };
                if (containersData.containersGroups.ContainsKey(containersGroup.Id))
                {
                    Logger.LogError($"Container group ID {containersGroup.Id} already exists in dictionary!");
                }
                else
                {
                    containersData.containersGroups.Add(containersGroup.Id, sptContainersGroup);
                }
            });

            Resources.FindObjectsOfTypeAll(typeof(LootableContainer)).ExecuteForEach(obj =>
            {
                var container = (LootableContainer)obj;

                // Skip empty ID containers
                if (container.Id.Length == 0)
                {
                    return;
                }

                if (containersData.containers.ContainsKey(container.Id))
                {
                    Logger.LogError($"Container {container.Id} already exists in dictionary!");
                }
                else
                {
                    containersData.containers.Add(container.Id, new SPTContainer { groupId = container.LootableContainersGroupId });
                }
            });

            string jsonString = JsonConvert.SerializeObject(containersData, Formatting.Indented);

            string outputFile = Path.Combine(StaticLootDumperPlugin.DumpFolder, $"{mapName}.json");
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
            File.Create(outputFile).Dispose();
            StreamWriter streamWriter = new StreamWriter(outputFile);
            streamWriter.Write(jsonString);
            streamWriter.Flush();
            streamWriter.Close();
        }
    }

    public class SPTContainersData
    {
        public Dictionary<string, SPTContainersGroup> containersGroups = new Dictionary<string, SPTContainersGroup>();
        public Dictionary<string, SPTContainer> containers = new Dictionary<string, SPTContainer>();
    }

    public class SPTContainer
    {
        public string groupId;
    }

    public class SPTContainersGroup
    {
        public int minContainers;
        public int maxContainers;
    }
}
