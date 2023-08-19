using Aki.Reflection.Patching;
using BepInEx;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DrakiaXYZ.StaticLootDumper
{
    [BepInPlugin("xyz.drakia.staticlootdumper", "DrakiaXYZ-StaticLootDumper", "1.0.0")]
    public class StaticLootDumperPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
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
            var containersData = new SPTContainersData();

            Object.FindObjectsOfType<LootableContainersGroup>().ExecuteForEach(containersGroup =>
            {
                var sptContainersGroup = new SPTContainersGroup { minContainers = containersGroup.Min, maxContainers = containersGroup.Max };
                containersData.containersGroups.Add(containersGroup.Id, sptContainersGroup);
            });

            Object.FindObjectsOfType<LootableContainer>().ExecuteForEach(container =>
            {
                containersData.containers.Add(container.Id, new SPTContainer { groupId = container.LootableContainersGroupId });
            });

            string jsonString = JsonConvert.SerializeObject(containersData, Formatting.Indented);
            Logger.LogInfo(jsonString);
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
