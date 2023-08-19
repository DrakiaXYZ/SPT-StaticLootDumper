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
            List<SPTLootContainer> lootContainers = new List<SPTLootContainer>();

            Object.FindObjectsOfType<LootableContainer>().ExecuteForEach(container =>
            {
                lootContainers.Add(new SPTLootContainer { name = container.name, probability = (container.SpawnChance / 100f), template = container.AsLootPointParameters() });
                //Logger.LogInfo($"Container: {container.name} ({container.Id}): {container.SpawnChance} ({container.SpawnType})");
            });

            string jsonString = JsonConvert.SerializeObject(lootContainers, Formatting.Indented);
            Logger.LogInfo(jsonString);
        }
    }

    public class SPTLootContainer
    {
        public float probability;
        public string name;
        public LootPointParameters template;
    }
}
