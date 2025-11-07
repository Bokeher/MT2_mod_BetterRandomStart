using BepInEx;
using HarmonyLib;
using ShinyShoe;
using System.Reflection;
using UnityEngine;

namespace MT2_BETTER_RANDOM
{
    [BepInPlugin("com.bokeher.better_random", "Better Random", "1.0.0")]
    public class BetterRandom : BaseUnityPlugin
    {
        public static BetterRandom? Instance { get; private set; }
        private readonly int[] championIndexes = { 0, 1 };
        private IReadOnlyList<(string mainClan, string subClan, int mainChampIndex)>? uncompleted;
        private IReadOnlyList<ClassData>? allClassDatas;
        private AllGameManagers? allGameManagers;
        private List<String>? clanIds;

        private MetagameSaveData? metagameSave;

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

            return allGameData.GetAllClassDatas();
        }

        public List<(string mainClan, string subClan, int champ)>? GetUncompletedCombinations()
        {
            var saveManager = allGameManagers!.GetSaveManager();
            if (saveManager == null)
            {
                Logger.LogError("SaveManager is null.");
                return null;
            }

            metagameSave = saveManager.GetMetagameSave();
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

            FieldInfo field = typeof(MetagameSaveData).GetField("unlockedClassIDs", BindingFlags.NonPublic | BindingFlags.Instance);
            var unlockedClassIDs = field.GetValue(metagameSave) as List<string>;

            List<String> unlockedClanIds = new List<String>();
            if (unlockedClassIDs == null)
            {
                Logger.LogError("unlockedClassIDs is null");
                return null;
            }


            if (unlockedClassIDs.Count < 8) // 8 is max (10 clans and 2 are default unlocked)
            {
                foreach (var clan in allClassDatas)
                {
                    // These 2 are default unlocked
                    if (clan.GetTitle() == "Banished" || clan.GetTitle() == "Pyreborne")
                    {
                        unlockedClanIds.Add(clan.GetID());
                    }

                    if (unlockedClassIDs.Contains(clan.GetID()))
                    {
                        unlockedClanIds.Add(clan.GetID());
                    }
                }
            }

            var uncompleted = new List<(string mainClan, string subClan, int champ)>();
            foreach (String mainClanId in clanIds!)
            {
                if (!unlockedClanIds.Contains(mainClanId) && unlockedClassIDs.Count < 8) continue;
                foreach (String subClanId in clanIds)
                {
                    if (!unlockedClanIds.Contains(subClanId) && unlockedClassIDs.Count < 8) continue;
                    if (mainClanId == subClanId) continue;

                    foreach (int mainChampIndex in championIndexes) // champIndex: 0 or 1
                    {
                        // skip if this champion is not unlocked (alternate champion unlocks after level 5)
                        int mainChampLevel = metagameSave.GetLevel(mainClanId);
                        if (mainChampLevel < 5 && mainChampIndex == 1) continue;

                        var result = metagameSave.GetClassCombinationWinAscensionLevel(mainClanId, subClanId, mainChampIndex);
                        if (result.highestAscensionLevel < 10 || !result.isTfbVictory)
                        {
                            uncompleted.Add((mainClanId, subClanId, mainChampIndex));
                        }

                    }
                }
            }

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
            foreach (var btn in FindObjectsOfType<GameUISelectableButton>())
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

            GameObject border = new GameObject("ButtonBorder");
            border.transform.SetParent(canvas.transform, false);

            var borderRect = border.AddComponent<RectTransform>();
            borderRect.sizeDelta = new Vector2(110 + 2, 50 + 2);

            var settingsRect = settingsButton.GetComponent<RectTransform>();
            borderRect.anchorMin = settingsRect.anchorMin;
            borderRect.anchorMax = settingsRect.anchorMax;
            borderRect.pivot = settingsRect.pivot;

            Vector2 anchoredPos = settingsRect.anchoredPosition;
            anchoredPos.y -= (settingsRect.rect.height + 10);
            anchoredPos.x -= 9;
            borderRect.anchoredPosition = anchoredPos;

            // Border background
            var borderImage = border.AddComponent<UnityEngine.UI.Image>();
            borderImage.color = Color.white;

            // ----- BUTTON -----
            GameObject randomButton = new GameObject("CustomRandomButton");
            randomButton.transform.SetParent(border.transform, false);

            var rect = randomButton.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(110, 50);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var image = randomButton.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(49f / 255f, 54f / 255f, 55f / 255f, 1f);

            var button = randomButton.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() =>
            {
                BetterRandom.Instance?.onRandomButtonClick();
            });

            // ----- TEXT -----
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

            var classInfos = UnityEngine.Object.FindObjectsOfType<RunSetupClassLevelInfoUI>();
            if (classInfos == null || classInfos.Length < 2)
            {
                Logger.LogError("RunSetupClassLevelInfoUI is null or doesnt contain main and sub clans");
                return;
            }

            var (mainClassInfo, subClassInfo) = (classInfos[0], classInfos[1]);

            int mainClanLevel = getClanLevel(mainClan.GetID());
            int subClanLevel = getClanLevel(subClan.GetID());

            // pick secondary clan variant now since it doesnt matter for the logbook completion
            int subClassChampIndex = UnityEngine.Random.Range(0, 2);

            // check if selected champion is unlocked (alt champion is unlocked after level 5)
            int subChampLevel = metagameSave!.GetLevel(subClan.GetID());
            if (subChampLevel < 5 && subChampLevel == 1)
            {
                subClassChampIndex = 0;
            }

            mainClassInfo.SetClass(mainClan, mainClanLevel, chosen.mainChampIndex);
            subClassInfo.SetClass(subClan, subClanLevel, subClassChampIndex);

            var runSetupScreen = FindObjectOfType<RunSetupScreen>();

            var refreshCharacterMethod = typeof(RunSetupScreen).GetMethod("RefreshCharacters", BindingFlags.Instance | BindingFlags.NonPublic);
            refreshCharacterMethod?.Invoke(runSetupScreen, new object[] { false });

            var RefreshClanCovenantUIMethod = typeof(RunSetupScreen).GetMethod("RefreshClanCovenantUI", BindingFlags.Instance | BindingFlags.NonPublic);
            RefreshClanCovenantUIMethod?.Invoke(runSetupScreen, null);

            mainClassInfo.ShowCardPreview();
            subClassInfo.ShowCardPreview();

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
