using UnityEngine;
using BepInEx;

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
            // AllGameManagers -> SaveManager -> MetagameSave -> filtering -> RNG chosing -> display

            if (!Input.GetKeyDown(KeyCode.F6)) return;

            Logger.LogInfo("F6 pressed!");

            // Step 1: Find AllGameManagers
            var allManagers = GameObject.FindObjectOfType<AllGameManagers>();
            if (allManagers == null)
            {
                Logger.LogError("AllGameManagers not found.");
                return;
            }

            // Step 2: Get SaveManager
            var saveManager = allManagers.GetSaveManager();
            if (saveManager == null)
            {
                Logger.LogError("SaveManager is null.");
                return;
            }

            // Step 3: Get MetagameSaveData
            var metagameSave = saveManager.GetMetagameSave();
            if (metagameSave == null)
            {
                Logger.LogError("MetagameSaveData is null.");
                return;
            }

            // Step 4: Build all combinations and filter
            var uncompleted = new List<(string mainClan, string subClan, int champ)>();

            foreach (var mainClan in clans.Keys)
            {
                foreach (var subClan in clans.Keys)
                {
                    if (mainClan == subClan) continue;

                    foreach (int champ in championIndexes)
                    {
                        var result = metagameSave.GetClassCombinationWinAscensionLevel(mainClan, subClan, champ);
                        if (result.highestAscensionLevel < 10 || !result.isTfbVictory)
                        {
                            uncompleted.Add((mainClan, subClan, champ));
                        }
                    }
                }
            }

            if (uncompleted.Count == 0)
            {
                Logger.LogInfo("All combinations are completed!");
                return;
            }

            Logger.LogInfo($"Uncompleted count: {uncompleted.Count}");

            // Step 5: Pick a random uncompleted combo
            var rng = new System.Random();
            var chosen = uncompleted[rng.Next(uncompleted.Count)];

            string mainName = clans.TryGetValue(chosen.mainClan, out var mName) ? mName : chosen.mainClan;
            string subName = clans.TryGetValue(chosen.subClan, out var sName) ? sName : chosen.subClan;
            Logger.LogInfo($"mainClan: {mainName}, subClan: {subName}, Champ: {chosen.champ}");
        }
    }
}
