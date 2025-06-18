using UnityEngine;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using ShinyShoe;

namespace MT2_BETTER_RANDOM
{
    [BepInPlugin("com.bokeher.better_random", "Better Random", "1.0.0")]
    public class BetterRandom : BaseUnityPlugin
    {
        public static BetterRandom? Instance { get; private set; }
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
        private readonly List<(string mainClan, string subClan, int champ)> uncompleted = new();

        private void Awake()
        {
            Instance = this;
            var harmony = new Harmony("com.bokeher.better_random");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnRunSetupScreenOpened()
        {
            uncompleted.Clear();

            var allManagers = GameObject.FindObjectOfType<AllGameManagers>();
            if (allManagers == null)
            {
                Logger.LogError("AllGameManagers not found.");
                return;
            }

            var saveManager = allManagers.GetSaveManager();
            if (saveManager == null)
            {
                Logger.LogError("SaveManager is null.");
                return;
            }

            var metagameSave = saveManager.GetMetagameSave();
            if (metagameSave == null)
            {
                Logger.LogError("MetagameSaveData is null.");
                return;
            }

            foreach (var mainClan in clans.Keys)
            {
                foreach (var subClan in clans.Keys)
                {
                    if (mainClan == subClan) continue;

                    foreach (var champ in championIndexes)
                    {
                        var result = metagameSave.GetClassCombinationWinAscensionLevel(mainClan, subClan, champ);
                        if (result.highestAscensionLevel < 10 || !result.isTfbVictory)
                        {
                            uncompleted.Add((mainClan, subClan, champ));
                        }
                    }
                }
            }

            Logger.LogInfo($"Uncompleted combinations updated: {uncompleted.Count}");
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F6)) return;

            Logger.LogInfo("F6 pressed!");

            if (uncompleted.IsNullOrEmpty())
            {
                Logger.LogInfo("'uncompleted' is null or empty!");
                return;
            }

            if (uncompleted.Count == 0)
            {
                Logger.LogInfo("All combinations are completed!");
                return;
            }

            var rng = new System.Random();
            var chosen = uncompleted[rng.Next(uncompleted.Count)];

            string mainName = clans.TryGetValue(chosen.mainClan, out var mName) ? mName : chosen.mainClan;
            string subName = clans.TryGetValue(chosen.subClan, out var sName) ? sName : chosen.subClan;

            Logger.LogInfo($"Main Clan: {mainName}, Sub Clan: {subName}, Champion: {chosen.champ}");
        }

        public void AddCustomButton()
        {
            GameUISelectableButton? settingsButton = null;
            foreach (var btn in GameObject.FindObjectsOfType<GameUISelectableButton>())
            {
                if (btn.gameObject.name == "SettingsButton")
                {
                    settingsButton = btn;
                    break;
                }
            }

            if (settingsButton == null)
            {
                Logger.LogWarning("SettingsButton not found");
                return;
            }

            var canvas = settingsButton.gameObject.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Logger.LogWarning("Canvas not found");
                return;
            }

            GameObject randomButton = new GameObject("CustomRandomButton");
            randomButton.transform.SetParent(canvas.transform, false);

            var rect = randomButton.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(110, 50);

            var image = randomButton.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.red;

            var button = randomButton.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(onRandomButtonClick);

            GameObject buttonText = new GameObject("ButtonText");
            buttonText.transform.SetParent(randomButton.transform, false);
            var text = buttonText.AddComponent<UnityEngine.UI.Text>();
            text.text = "Better Random";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var settingsRect = settingsButton.GetComponent<RectTransform>();

            rect.anchorMin = settingsRect.anchorMin;
            rect.anchorMax = settingsRect.anchorMax;
            rect.pivot = settingsRect.pivot;

            Vector2 anchoredPos = settingsRect.anchoredPosition;
            anchoredPos.y -= (settingsRect.rect.height + 10);
            anchoredPos.x -= 9;
            rect.anchoredPosition = anchoredPos;
        }

        private void onRandomButtonClick()
        {
            Logger.LogInfo("Random button pressed");
        }
    }

    [HarmonyPatch(typeof(RunSetupScreen), "Start")]
    public class RunSetupScreen_Start_Patch
    {
        static void Postfix()
        {
            BetterRandom.Instance?.OnRunSetupScreenOpened();
            BetterRandom.Instance?.AddCustomButton();
        }
    }
}
