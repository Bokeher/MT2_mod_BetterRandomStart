using UnityEngine;
using BepInEx;
using System.Reflection;
using System.Collections.Generic;

namespace MT2_BETTER_RANDOM
{
    [BepInPlugin("com.bokeher.better_random", "Better Random", "1.0.0")]
    public class BetterRandom : BaseUnityPlugin
    {
        private readonly Dictionary<string, string> clans = new Dictionary<string, string>
        {
            { "5be08e27-c1e6-4b9d-b506-e4781e111dc8", "Banished" },
            { "ab9c9f6f-2543-4ca5-b7e5-e2eb125445b8", "Pyreborne" },
            { "0df83271-5359-48df-9365-e73b7b2d0130", "Luna Coven" },
            { "3c98c8eb-fc7c-4b35-925e-6b5ab0f69896", "Underlegion" },
            { "9aaf1009-3fbe-4ac5-9f99-a3a702ff7f27", "Lazarus League" },
            { "c595c344-d323-4cf1-9ad6-41edc2aebbd0", "Hellhorned" },
            { "fd119fcf-c2cf-469e-8a5a-e9b0f265560d", "Awoken" },
            { "9317cf9a-04ec-49da-be29-0e4ed61eb8ba", "Stygian Guard" },
            { "4fe56363-b1d9-46b7-9a09-bd2df1a5329f", "Umbra" },
            { "fda62ada-520e-42f3-aa88-e4a78549c4a2", "Melting Remnant" },
        };

        private readonly int[] championIndexes = { 0, 1 };

        private void Update()
        {
            // Find an object of type CompendiumSection
            var section = GameObject.FindObjectOfType<CompendiumSection>();
            if (section == null)
            {
                Logger.LogError("CompendiumSection not found.");
                return;
            }

            // Get the private field metagameSave
            var metagameField = typeof(CompendiumSection).GetField("metagameSave", BindingFlags.NonPublic | BindingFlags.Instance);
            if (metagameField == null)
            {
                Logger.LogError("metagameSave not found.");
                return;
            }

            var metagameSave = metagameField.GetValue(section);
            if (metagameSave == null)
            {
                Logger.LogError("Failed to get metagameSave value.");
                return;
            }

            // Get the private field wonClassCombinations
            var wonField = metagameSave.GetType().GetField("wonClassCombinations", BindingFlags.NonPublic | BindingFlags.Instance);
            if (wonField == null)
            {
                Logger.LogError("wonClassCombinations field not found.");
                return;
            }

            var listObj = wonField.GetValue(metagameSave);
            if (listObj is IEnumerable<object> wonList)
            {
                Logger.LogInfo("Found class combinations:");

                foreach (var combo in wonList)
                {
                    var comboType = combo.GetType();

                    string classID = comboType.GetField("classID")?.GetValue(combo)?.ToString();
                    string subClassID = comboType.GetField("subclassID")?.GetValue(combo)?.ToString();
                    int champIndex = (int)(comboType.GetField("championIndex")?.GetValue(combo) ?? -1);
                    int ascension = (int)(comboType.GetField("highestAscensionLevel")?.GetValue(combo) ?? -1);
                    bool tfb = (bool)(comboType.GetField("isTfbVictory")?.GetValue(combo) ?? false);

                    Logger.LogInfo($"Main: {classID}, Sub: {subClassID}, Champ: {champIndex}, Asc: {ascension}, TFB: {tfb}");
                }
            }
            else
            {
                Logger.LogError("wonClassCombinations is not a list.");
            }
        }
    }
}
