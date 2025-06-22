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
        private readonly int[] championIndexes = { 0, 1 };
        private IReadOnlyList<(string mainClan, string subClan, int champ)>? uncompleted;
        private IReadOnlyList<ClassData>? allClassDatas;
        private AllGameManagers? allGameManagers;
        private List<String>? clanIds;

        private void Awake()
        {
            Instance = this;
            var harmony = new Harmony("com.bokeher.better_random");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
            
        public void onRunSetupScreenOpened()
        {
            allGameManagers = GameObject.FindObjectOfType<AllGameManagers>();
            if (allGameManagers == null)
            {
                Logger.LogError("AllGameManagers is null");
                return;
            }

            allClassDatas = getAllClans();
            if (allClassDatas == null)
            {
                Logger.LogError("AllClassDatas is null");
                return;
            }

            clanIds = getClanIds();
            if (clanIds == null)
            {
                Logger.LogError("ClanIds is null");
                return;
            }

            uncompleted = GetUncompletedCombinations() ?? new();
            AddCustomButton();
        }

        private List<string> getClanIds()
        {
            List<String> list = new();
            foreach (ClassData clan in allClassDatas!)
            {
                list.Add(clan.GetID());
            }
            return list;
        }

        private IReadOnlyList<ClassData>? getAllClans()
        {
            var allGameData = allGameManagers!.GetAllGameData();
            if (allGameData == null)
            {
                Logger.LogError("ClassManager not found.");
                return null;
            }

            IReadOnlyList<ClassData> list = allGameData.GetAllClassDatas();
            foreach (ClassData classData in list)
            {
                Logger.LogInfo(classData.GetID());
                Logger.LogInfo(classData.GetTitle());
            }

            return list;
        }

        public List<(string mainClan, string subClan, int champ)>? GetUncompletedCombinations()
        {
            var saveManager = allGameManagers!.GetSaveManager();
            if (saveManager == null)
            {
                Logger.LogError("SaveManager is null.");
                return null;
            }

            var metagameSave = saveManager.GetMetagameSave();
            if (metagameSave == null)
            {
                Logger.LogError("MetagameSaveData is null.");
                return null;
            }

            if (allClassDatas == null)
            {
                Logger.LogError("AllClassDatas is null.");
                return null;
            }

            var uncompleted = new List<(string mainClan, string subClan, int champ)>();
            foreach (String mainClan in clanIds!)
            {
                foreach (String subClan in clanIds)
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

            Logger.LogInfo($"Uncompleted combinations updated: {uncompleted.Count}");
            return uncompleted;
        }

        private int getClanLevel(string classId)
        {
            var saveManager = allGameManagers!.GetSaveManager();
            if (saveManager == null)
            {
                Logger.LogError("SaveManager is null.");
                return 1; // 1 is in game code
            }

            var metagameSave = saveManager.GetMetagameSave();
            if (metagameSave == null)
            {
                Logger.LogError("MetagameSaveData is null.");
                return 1; 
            }

            return metagameSave.GetLevel(classId);
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
                Logger.LogError("SettingsButton not found");
                return;
            }

            var canvas = settingsButton.gameObject.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Logger.LogError("Canvas not found");
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
            if (uncompleted.IsNullOrEmpty())
            {
                Logger.LogError("'uncompleted' is null or empty!");
                return;
            }

            if (uncompleted!.Count == 0)
            {
                Logger.LogError("All combinations are completed!");
                return;
            }

            var rng = new System.Random();
            var chosen = uncompleted[rng.Next(uncompleted.Count)];

            ClassData? mainClan = allClassDatas?.FirstOrDefault(cd => cd.GetID() == chosen.mainClan);
            ClassData? subClan = allClassDatas?.FirstOrDefault(cd => cd.GetID() == chosen.subClan);

            if (mainClan == null || subClan == null)
            {
                Logger.LogError("MainClan or SubClan is null");
                return;
            }

            string mainName = mainClan.GetTitle();
            string subName = subClan.GetTitle();

            Logger.LogInfo($"Main Clan: {mainName}, Sub Clan: {subName}, Champion: {chosen.champ}");

            var classInfos = UnityEngine.Object.FindObjectsOfType<RunSetupClassLevelInfoUI>();
            if(classInfos == null || classInfos.Length < 2)
            {
                Logger.LogError("RunSetupClassLevelInfoUI is null or doesnt contain main and sub clans");
                return;
            }

            var mainClassInfo = classInfos[0];
            var subClassInfo = classInfos[1];

            // TODO: CHECK IF THIS WORKS CORRECTLY AND NOT OVERRIDE CLAN LEVELS
            int mainClanLevel = getClanLevel(mainClan.GetID());
            int subClanLevel = getClanLevel(subClan.GetID());

            // pick here since it doesnt matter for the logbook which sub clan variant is chosen
            int subClassChamp = UnityEngine.Random.Range(0, 2);

            mainClassInfo.SetClass(mainClan, mainClanLevel, chosen.champ);
            subClassInfo.SetClass(subClan, subClanLevel, subClassChamp);

            var runSetupScreen = GameObject.FindObjectOfType<RunSetupScreen>();

            var refreshCharacterMethod = typeof(RunSetupScreen).GetMethod("RefreshCharacters", BindingFlags.Instance | BindingFlags.NonPublic);
            refreshCharacterMethod?.Invoke(runSetupScreen, new object[] { false });

            var RefreshClanCovenantUIMethod = typeof(RunSetupScreen).GetMethod("RefreshClanCovenantUI", BindingFlags.Instance | BindingFlags.NonPublic);
            RefreshClanCovenantUIMethod?.Invoke(runSetupScreen, null);

            mainClassInfo.ShowCardPreview();
            subClassInfo.ShowCardPreview();

            // TODO: check if clans are unlocked, change UI of button, tests
        }
    }

    [HarmonyPatch(typeof(RunSetupScreen), "Start")]
    public class RunSetupScreen_Start_Patch
    {
        static void Postfix()
        {
            BetterRandom.Instance?.onRunSetupScreenOpened();
        }
    }
}
