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
            var containersGroups = new List<SPTContainersGroup>();

            Object.FindObjectsOfType<LootableContainersGroup>().ExecuteForEach(containersGroup =>
            {
                var sptContainersGroup = new SPTContainersGroup { groupId = containersGroup.Id, minContainers = containersGroup.Min, maxContainers = containersGroup.Max, containerList = new List<string>() };

                foreach (LootableContainer container in containersGroup.GetComponentsInChildren<LootableContainer>())
                {
                    sptContainersGroup.containerList.Add(container.Id);
                }

                containersGroups.Add(sptContainersGroup);
            });

            string jsonString = JsonConvert.SerializeObject(containersGroups, Formatting.Indented);
            Logger.LogInfo(jsonString);
        }
    }

    public class SPTContainersGroup
    {
        public string groupId;
        public int minContainers;
        public int maxContainers;
        public List<string> containerList;
    }
}
