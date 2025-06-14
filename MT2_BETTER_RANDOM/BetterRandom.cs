using UnityEngine;
using BepInEx;
using System.Reflection;
using System.Collections.Generic;

namespace MT2_BETTER_RANDOM
{
    [BepInPlugin("com.bokeher.better_random", "Better Random", "1.0.0")]
    public class BetterRandom : BaseUnityPlugin
    {
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
