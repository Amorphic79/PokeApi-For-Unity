using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MrAmorphic
{
    public enum PokemonDataType
    { Pokemon, Species, Evolution, }

    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1),
            };
    }

    public class CoroutineWithData
    {
        public object result;
        private IEnumerator target;

        public CoroutineWithData(IEnumerator target)
        {
            this.target = target;
            PokeApi pokeApi = ScriptableObject.CreateInstance<PokeApi>();
            this.Coroutine = EditorCoroutineUtility.StartCoroutine(Run(), pokeApi);
        }

        public EditorCoroutine Coroutine { get; private set; }

        private IEnumerator Run()
        {
            while (target.MoveNext())
            {
                result = target.Current;
                yield return result;
            }
        }
    }

    [CustomEditor(typeof(MoveBase))]
    [ExecuteInEditMode]
    public class MoveBaseEditor : Editor
    {
        private MoveBase move;

        public override void OnInspectorGUI()
        {
            this.move = (MoveBase)this.target;
            PokeApi pokeApi;

            if (GUILayout.Button("Update Data", GUILayout.Height(40)))
            {
                pokeApi = CreateInstance<PokeApi>();
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetMoveData(this.move.Id), this);
            }

            base.OnInspectorGUI();

            if (GUILayout.Button("Clear Data", GUILayout.Height(40)))
            {
                this.ClearMoveData();
            }
        }

        private void ClearMoveData()
        {
            this.move.Accuracy = 0;
            this.move.Description = string.Empty;
            this.move.Type = PokemonType.None;
            this.move.Power = 0;
            this.move.PP = 0;
            this.move.Name = string.Empty;
            this.move.Secondaries.Clear();
            this.move.Effects.Boosts.Clear();
            this.move.Effects.Status = ConditionID.none;
            this.move.Effects.VolatileStatus = ConditionID.none;
        }
    }

    [CustomEditor(typeof(PokemonBase))]
    [ExecuteInEditMode]
    public class PokemonBaseEditor : Editor
    {
        private PokemonBase pokemon;
        private PokeApi pokeApi;

        public override void OnInspectorGUI()
        {
            this.pokemon = (PokemonBase)this.target;

            if (GUILayout.Button("Update Data", GUILayout.Height(40)))
            {
                pokeApi = CreateInstance<PokeApi>();
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetPokemonData(this.pokemon.Id), this);
            }

            base.OnInspectorGUI();

            if (GUILayout.Button("Clear Data", GUILayout.Height(40)))
            {
                this.ClearData();
            }
        }

        private void ClearData()
        {
            this.pokemon.ExpYield = 0;
            this.pokemon.Description = string.Empty;
            this.pokemon.Type1 = PokemonType.None;
            this.pokemon.Type2 = PokemonType.None;
            this.pokemon.CatchRate = 0;
            this.pokemon.GrowthRate = GrowthRate.MediumFast;
            this.pokemon.LearnableMoves.Clear();
            this.pokemon.LearnableByItems.Clear();
            this.pokemon.Evolutions.Clear();
            this.pokemon.MaxHp = 0;
            this.pokemon.Attack = 0;
            this.pokemon.Defense = 0;
            this.pokemon.SpAttack = 0;
            this.pokemon.SpDefense = 0;
            this.pokemon.Speed = 0;
            this.pokemon.Name = string.Empty;
            this.pokemon.CatchRate = 0;
            this.pokemon.FrontSprite = null;
            this.pokemon.BackSprite = null;
        }
    }

    [CustomEditor(typeof(ItemBase))]
    [ExecuteInEditMode]
    public class ItemBaseEditor : Editor
    {
        private ItemBase item;
        private PokeApi pokeApi;

        public override void OnInspectorGUI()
        {
            this.item = (ItemBase)this.target;

            if (GUILayout.Button("Update Data", GUILayout.Height(40)))
            {
                pokeApi = CreateInstance<PokeApi>();
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetItemData(this.item.Id), this);
            }

            base.OnInspectorGUI();

            if (GUILayout.Button("Clear Data", GUILayout.Height(40)))
            {
                this.ClearData();
            }
        }

        private void ClearData()
        {
            this.item.CanUseInBattle = false;
            this.item.CanUseOutsideBattle = false;
            this.item.Name = string.Empty;
        }
    }

    public class PokeApi : Editor
    {
        private static PokeApi instance;
        private static bool fetchError = false;

        ///SETTINGS ///////////////////////////////////////////////////////////////////////
        private static string pathToResources = "/Game/Resources/";
        private static string pathToMoveAssets = "Moves/Test/";
        private static string pathToItemAssets = "Items/Test/";
        private static string pathToPokemonAssets = "Pokemons/Test/";
        private static string cacheFolder = "_Cache/";
        private static string spritesFolder = "_Sprites/";
        private static string pokeballsFolder = "Poke Balls/";
        private static string healingFolder = "Medicine/";
        private static string itemsFolder = "Items/";
        private static string battleItemsFolder = "Battle Items/";
        private static string machinesFolder = "Machines (TM&HM)/";
        private static string keyItemsFolder = "Key Items/";
        private static string heldItemsFolder = "Held Items/";
        private static string evolutionItemsFolder = "Evolution Items/";
        private static string language = "en";
        private static string version_group = "firered-leafgreen";
        private static string version_group_secondary = "sword-shield";
        private static string version = "firered";
        private static string version_secondary = "shield";
        private static bool fetchSprites = true;
        private static bool createPokemonStubIfNotExistForEvolution = true;
        private static bool createItemStubIfNotExistForEvolution = true;
        private static bool createMoveStubIfNotExistForPokemonMoves = true;
        ///SETTINGS END////////////////////////////////////////////////////////////////////
        public static string FormatJson(string json, string indent = "    ")
        {
            var indentation = 0;
            var quoteCount = 0;
            var escapeCount = 0;

            var result =
                from ch in json ?? string.Empty
                let escaped = (ch == '\\' ? escapeCount++ : escapeCount > 0 ? escapeCount-- : escapeCount) > 0
                let quotes = ch == '"' && !escaped ? quoteCount++ : quoteCount
                let unquoted = quotes % 2 == 0
                let colon = ch == ':' && unquoted ? ": " : null
                let nospace = char.IsWhiteSpace(ch) && unquoted ? string.Empty : null
                let lineBreak = ch == ',' && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, indentation)) : null
                let openChar = (ch == '{' || ch == '[') && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, ++indentation)) : ch.ToString()
                let closeChar = (ch == '}' || ch == ']') && unquoted ? Environment.NewLine + string.Concat(Enumerable.Repeat(indent, --indentation)) + ch : ch.ToString()
                select colon ?? nospace ?? lineBreak ?? (
                    openChar.Length > 1 ? openChar : closeChar
                );

            return string.Concat(result);
        }

        public IEnumerator GetMoveData(int index)
        {
            string file = Application.dataPath + $"{pathToResources}{pathToMoveAssets}{cacheFolder}{index}.json";
            fetchError = false;

            if (File.Exists(file))
            {
                var data = File.ReadAllText(file);
                ProcessMoveData(data);
            }
            else
            {
                UnityWebRequest www = UnityWebRequest.Get("https://pokeapi.co/api/v2/move/" + index);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    fetchError = true;
                }
                else
                {
                    var data = www.downloadHandler.text;
                    File.WriteAllText(file, FormatJson(data));
                    ProcessMoveData(data);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        public IEnumerator GetItemData(int index)
        {
            string file = Application.dataPath + $"{pathToResources}{pathToItemAssets}{cacheFolder}{index}.json";
            fetchError = false;

            if (File.Exists(file))
            {
                var data = File.ReadAllText(file);
                yield return ProcessItemData(data);
            }
            else
            {
                UnityWebRequest www = UnityWebRequest.Get("https://pokeapi.co/api/v2/item/" + index);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    fetchError = true;
                }
                else
                {
                    var data = www.downloadHandler.text;
                    File.WriteAllText(file, FormatJson(data));
                    yield return ProcessItemData(data);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        public IEnumerator GetPokemonSubData(string url, PokemonDataType dataType)
        {
            fetchError = false;

            var index = Int32.Parse(url.TrimEnd('/').Split('/').Last());

            string folder = dataType switch
            {
                PokemonDataType.Species => "Species/",
                PokemonDataType.Evolution => "Evolution/",
                _ => string.Empty,
            };

            string file = Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}{folder}{index}.json";

            if (File.Exists(file))
            {
                var data = File.ReadAllText(file);
                yield return data;
            }
            else
            {
                UnityWebRequest www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    fetchError = true;
                }
                else
                {
                    var data = www.downloadHandler.text;
                    File.WriteAllText(file, FormatJson(data));
                    yield return data;
                }
            }
        }

        public IEnumerator GetPokemonData(int index)
        {
            string file = Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}Pokemon/{index}.json";
            fetchError = false;

            if (File.Exists(file))
            {
                var data = File.ReadAllText(file);
                yield return ProcessPokemonData(data);
            }
            else
            {
                UnityWebRequest www = UnityWebRequest.Get("https://pokeapi.co/api/v2/pokemon/" + index);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    fetchError = true;
                }
                else
                {
                    var data = www.downloadHandler.text;
                    File.WriteAllText(file, FormatJson(data));
                    yield return ProcessPokemonData(data);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        [MenuItem("MrAmorphic/PokeAPI/Create Folders #&f")]
        private static void CreateFolders()
        {
            instance = CreateInstance<PokeApi>();
            instance.CreateItemFolders();
            instance.CreatePokemonFolders();
            instance.CreateMoveFolders();
        }

        [MenuItem("MrAmorphic/PokeAPI/ALL/Fetch All")]
        private static void FetchEverything()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.FetchEverythingCO(), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Moves/Fetch All Moves #&m")]
        private static void FetchAllMoves()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetMoves(), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Items/Fetch All Items #&i")]
        private static void FetchAllItems()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetItems(), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch All Pokemons #&p")]
        private static void FetchAllPokemons()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 1 #&1")]
        private static void FetchAllPokemonsGen1()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(151, 1), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 2 #&2")]
        private static void FetchAllPokemonsGen2()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(100, 152), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 3 #&3")]
        private static void FetchAllPokemonsGen3()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(135, 252), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 4 #&4")]
        private static void FetchAllPokemonsGen4()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(107, 387), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 5 #&5")]
        private static void FetchAllPokemonsGen5()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(156, 494), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 6 #&6")]
        private static void FetchAllPokemonsGen6()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(72, 650), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 7 #&7")]
        private static void FetchAllPokemonsGen7()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(88, 722), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 8 #&8")]
        private static void FetchAllPokemonsGen8()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(89, 810), instance);
        }

        private static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        private IEnumerator FetchEverythingCO()
        {
            DateTime time = DateTime.Now;
            yield return GetMoves();
            yield return GetItems();
            yield return GetPokemons();
            AssetDatabase.Refresh();
            TimeSpan t = DateTime.Now.Subtract(time);
            string answer = string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);

            Debug.Log($"Fetched everything in {answer}.");
        }

        private IEnumerator DownloadSprite(string url, string fileToSave)
        {
            if (url?.Length == 0)
                yield break;

            if (!File.Exists(fileToSave))
            {
                using WebClient client = new WebClient();
                client.DownloadFile(new Uri(url), fileToSave);
            }

            yield return new WaitForEndOfFrame();
        }

        private void CreateItemFolders()
        {
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{cacheFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{pokeballsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{healingFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{itemsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{battleItemsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{machinesFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{keyItemsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{heldItemsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{evolutionItemsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{spritesFolder}");
        }

        private void CreatePokemonFolders()
        {
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}Pokemon/");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}Species/");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}Evolution/");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{spritesFolder}");
        }

        private void CreateMoveFolders()
        {
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToMoveAssets}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToMoveAssets}{cacheFolder}");
        }

        private IEnumerator GetItems(int count = 749, int start = 1)
        {
            DateTime time = DateTime.Now;
            int countFetched = 0;
            CreateItemFolders();

            List<int> missingItemsIDs = new List<int>() { 667, 672, 680, };

            for (int i = start; i < (start + count); i++)
            {
                if (missingItemsIDs.Contains(i))// Item is missing, skip to next
                    continue;

                yield return instance.GetItemData(i);

                if (fetchError)
                    break;

                countFetched++;
            }

            TimeSpan t = DateTime.Now.Subtract(time);
            string answer = string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);

            Debug.Log($"{countFetched} items fetched in {answer}.");
        }

        private IEnumerator GetMoves(int count = 826, int start = 1)
        {
            DateTime time = DateTime.Now;

            CreateMoveFolders();

            int countFetched = 0;
            for (int i = start; i < (start + count); i++)
            {
                yield return instance.GetMoveData(i);

                if (fetchError)
                    break;

                countFetched++;
            }

            TimeSpan t = DateTime.Now.Subtract(time);
            string answer = string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);

            Debug.Log($"{countFetched} moves fetched in {answer}.");
        }

        private IEnumerator GetPokemons(int count = 898, int start = 1)
        {
            DateTime time = DateTime.Now;

            CreatePokemonFolders();

            if (createItemStubIfNotExistForEvolution)
                CreateItemFolders();

            if (createMoveStubIfNotExistForPokemonMoves)
                CreateMoveFolders();

            int countFetched = 0;
            for (int i = start; i < (start + count); i++)
            {
                yield return instance.GetPokemonData(i);

                if (fetchError)
                    break;

                countFetched++;
            }

            TimeSpan t = DateTime.Now.Subtract(time);
            string answer = string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);

            Debug.Log($"{countFetched} pokemons fetched in {answer}.");
        }

        private IEnumerator ProcessItemData(string data)
        {
            // Replace is a hack because "default" can't be used.
            PokeApiItem item = JsonUtility.FromJson<PokeApiItem>(data.Replace("\"default\":", "\"defaul\":"));

            List<string> categoryPokeball = new List<string>() { "standard-balls", "special-balls", "apricorn-balls" };
            List<string> categoryMedicine = new List<string>() { "healing", "status-cures", "revival", "pp-recovery", "vitamins", "flutes", "medicine", "picky-healing", "baking-only", "effort-drop", "type-protection", "in-a-pinch", "other", };
            List<string> categoryItem = new List<string>() { "stat-boosts", "spelunking", "collectibles", "loot", "dex-completion", "mulch", "all-mail", "species-specific", "apricorn-box", "data-cards", };
            List<string> categoryEvolution = new List<string>() { "evolution", "mega-stones", };
            List<string> categoryHeld = new List<string>() { "held-items", "choice", "type-enhancement", "effort-training", "training", "scarves", "bad-held-items", "plates", "jewels", };
            List<string> categoryMachines = new List<string>() { "all-machines", };
            List<string> categoryKey = new List<string>() { "gameplay", "plot-advancement", "event-items", };
            List<string> categoryBattle = new List<string>() { "miracle-shooter", };
            List<string> categoryUnused = new List<string>() { "unused", };

            // Abort for unused items
            if (categoryUnused.Contains(item.category.name))
            {
                yield break;
            }

            ItemBase itemToAdd;
            bool isNew = false;

            var folder = item.category.name switch
            {
                var x when categoryPokeball.Contains(x) => pokeballsFolder,
                var x when categoryMedicine.Contains(x) => healingFolder,
                var x when categoryItem.Contains(x) => itemsFolder,
                var x when categoryEvolution.Contains(x) => evolutionItemsFolder,
                var x when categoryHeld.Contains(x) => heldItemsFolder,
                var x when categoryMachines.Contains(x) => machinesFolder,
                var x when categoryKey.Contains(x) => keyItemsFolder,
                var x when categoryBattle.Contains(x) => battleItemsFolder,
                _ => itemsFolder,
            };

            if (File.Exists(Application.dataPath + $"{pathToResources}{pathToItemAssets}{folder}{item.name}.asset"))
            {
                itemToAdd = AssetDatabase.LoadAssetAtPath<ItemBase>($"Assets/{pathToResources}{pathToItemAssets}{item.name}.asset");
                EditorUtility.SetDirty(itemToAdd);
            }
            else
            {
                isNew = true;

                itemToAdd = item.category.name switch
                {
                    var x when categoryPokeball.Contains(x) => CreateInstance<PokeballItem>(),
                    var x when categoryMedicine.Contains(x) => CreateInstance<RecoveryItem>(),
                    var x when categoryItem.Contains(x) => CreateInstance<ItemBase>(),
                    var x when categoryEvolution.Contains(x) => CreateInstance<EvolutionItem>(),
                    var x when categoryHeld.Contains(x) => CreateInstance<ItemBase>(),
                    var x when categoryMachines.Contains(x) => CreateInstance<TmItem>(),
                    var x when categoryKey.Contains(x) => CreateInstance<ItemBase>(),
                    var x when categoryBattle.Contains(x) => CreateInstance<ItemBase>(),
                    _ => null,
                };
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = itemToAdd;

            if (itemToAdd == null)
            {
                Debug.LogWarning($"Category {item.category.name} not found for item {item.name}");
                yield break;
            }

            itemToAdd.Name = item.names.First(n => n.language.name == language).name;

            if (fetchSprites)
            {
                if (!File.Exists(Application.dataPath + $"{pathToResources}{pathToItemAssets}{spritesFolder}{item.name}.png"))
                {
                    yield return instance.DownloadSprite(item.sprites.defaul, Application.dataPath + $"{pathToResources}{pathToItemAssets}{spritesFolder}{item.name}.png");
                    yield return new WaitForSeconds(0.15f);
                    AssetDatabase.Refresh();
                }

                yield return null;

                Sprite ic = Resources.Load<Sprite>($"{pathToItemAssets}{spritesFolder}{item.name}");
                itemToAdd.Icon = ic;
            }

            itemToAdd.Description = item.flavor_text_entries switch
            {
                var x when x.Count(n => n.language.name == language && n.version_group.name == version_group) > 0 => x.First(n => n.language.name == language && n.version_group.name == version_group).text,
                var x when x.Count(n => n.language.name == language && n.version_group.name == version_group_secondary) > 0 => x.First(n => n.language.name == language && n.version_group.name == version_group_secondary).text,
                var x when x.Count(n => n.language.name == language) > 0 => x.First(n => n.language.name == language).text,
                _ => string.Empty,
            };

            itemToAdd.Cost = item.cost;
            itemToAdd.Id = item.id;

            if (item.attributes.Any(a => a.name == "usable-in-battle"))
            {
                itemToAdd.CanUseInBattle = true;
            }

            if (item.attributes.Any(a => a.name == "usable-overworld"))
            {
                itemToAdd.CanUseOutsideBattle = true;
            }

            itemToAdd.PokeApiItem = item;

            EditorUtility.FocusProjectWindow();

            if (isNew)
                AssetDatabase.CreateAsset(itemToAdd, $"Assets/{pathToResources}{pathToItemAssets}{folder}{item.name}.asset");

            AssetDatabase.SaveAssets();
        }

        private void ProcessMoveData(string data)
        {
            PokeApiMove move = JsonUtility.FromJson<PokeApiMove>(data);

            MoveBase moveToAdd;
            bool isNew = false;

            if (File.Exists(Application.dataPath + $"{pathToResources}{pathToMoveAssets}{move.name}.asset"))
            {
                moveToAdd = AssetDatabase.LoadAssetAtPath<MoveBase>($"Assets/{pathToResources}{pathToMoveAssets}{move.name}.asset");
                EditorUtility.SetDirty(moveToAdd);
            }
            else
            {
                isNew = true;
                moveToAdd = CreateInstance<MoveBase>();
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = moveToAdd;

            moveToAdd.Id = move.id;
            moveToAdd.Name = move.names.First(n => n.language.name == language).name;
            moveToAdd.Accuracy = move.accuracy;
            moveToAdd.Description = move.flavor_text_entries switch
            {
                var x when x.Count(n => n.language.name == language && n.version_group.name == version_group) > 0 => x.First(n => n.language.name == language && n.version_group.name == version_group).flavor_text,
                var x when x.Count(n => n.language.name == language && n.version_group.name == version_group_secondary) > 0 => x.First(n => n.language.name == language && n.version_group.name == version_group_secondary).flavor_text,
                var x when x.Count(n => n.language.name == language) > 0 => x.First(n => n.language.name == language).flavor_text,
                _ => string.Empty,
            };
            moveToAdd.Power = move.power;
            moveToAdd.PP = move.pp;
            moveToAdd.Priority = move.priority;
            moveToAdd.Type = (PokemonType)Enum.Parse(typeof(PokemonType), move.type.name.FirstCharToUpper());
            moveToAdd.Category = (MoveCategory)Enum.Parse(typeof(MoveCategory), move.damage_class.name.FirstCharToUpper());
            moveToAdd.Effects = new MoveEffects();
            moveToAdd.Effects.Boosts = new List<StatBoost>();
            moveToAdd.Secondaries = new List<SecondaryEffects>();

            foreach (var stat_change in move.stat_changes)
            {
                StatBoost effectToAdd = new StatBoost();
                effectToAdd.stat = (Stat)Enum.Parse(typeof(Stat), stat_change.stat.name.FirstCharToUpper().Replace("-", "_"));
                effectToAdd.boost = stat_change.change;
                moveToAdd.Effects.Boosts.Add(effectToAdd);
            }

            if (move.meta.ailment.name != "none")
            {
                SecondaryEffects effects = new SecondaryEffects();
                effects.Status = (ConditionID)Enum.Parse(typeof(ConditionID), move.meta.ailment.name.Replace("-", "_"));
                effects.Chance = move.meta.ailment_chance;
                moveToAdd.Secondaries.Add(effects);
            }

            moveToAdd.PokeApiMove = move;

            if (isNew)
                AssetDatabase.CreateAsset(moveToAdd, $"Assets/{pathToResources}{pathToMoveAssets}{move.name}.asset");

            AssetDatabase.SaveAssets();
        }

        private IEnumerator ProcessPokemonData(string data)
        {
            PokeApiPokemon pokemon = JsonUtility.FromJson<PokeApiPokemon>(data);
            PokemonBase pokemonToAdd;
            bool isNew = false;

            if (File.Exists(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{pokemon.name}.asset"))
            {
                pokemonToAdd = AssetDatabase.LoadAssetAtPath<PokemonBase>($"Assets/{pathToResources}{pathToPokemonAssets}{pokemon.name}.asset");
                EditorUtility.SetDirty(pokemonToAdd);
            }
            else
            {
                isNew = true;
                pokemonToAdd = CreateInstance<PokemonBase>();
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = pokemonToAdd;

            CoroutineWithData speciesCO = new CoroutineWithData(GetPokemonSubData(pokemon.species.url, PokemonDataType.Species));
            yield return speciesCO.Coroutine;
            PokeApiSpecies species = JsonUtility.FromJson<PokeApiSpecies>(speciesCO.result.ToString());
            pokemon.species_data = species;

            CoroutineWithData evolutionCO = new CoroutineWithData(GetPokemonSubData(species.evolution_chain.url, PokemonDataType.Evolution));
            yield return evolutionCO.Coroutine;
            PokeApiEvolution evolutions = JsonUtility.FromJson<PokeApiEvolution>(evolutionCO.result.ToString());
            pokemon.species_data.evolution_data = evolutions;

            pokemonToAdd.ExpYield = pokemon.base_experience;
            pokemonToAdd.CatchRate = species.capture_rate;

            pokemonToAdd.Description = pokemon.species_data.flavor_text_entries switch
            {
                var x when x.Count(n => n.language.name == language && n.version.name == version) > 0 => x.First(n => n.language.name == language && n.version.name == version).flavor_text,
                var x when x.Count(n => n.language.name == language && n.version.name == version_secondary) > 0 => x.First(n => n.language.name == language && n.version.name == version_secondary).flavor_text,
                var x when x.Count(n => n.language.name == language) > 0 => x.First(n => n.language.name == language).flavor_text,
                _ => string.Empty,
            };

            pokemonToAdd.Name = Array.Find(pokemon.species_data.names, n => n.language.name == language).name;
            pokemonToAdd.Id = pokemon.id;
            pokemonToAdd.PokeApiPokemon = pokemon;

            foreach (var stat in pokemon.stats)
            {
                switch (stat.stat.name)
                {
                    case "hp":
                        pokemonToAdd.MaxHp = stat.base_stat;
                        break;

                    case "attack":
                        pokemonToAdd.Attack = stat.base_stat;
                        break;

                    case "defense":
                        pokemonToAdd.Defense = stat.base_stat;
                        break;

                    case "special-attack":
                        pokemonToAdd.SpAttack = stat.base_stat;
                        break;

                    case "special-defense":
                        pokemonToAdd.SpDefense = stat.base_stat;
                        break;

                    case "speed":
                        pokemonToAdd.Speed = stat.base_stat;
                        break;
                }
            }

            foreach (var type in pokemon.types)
            {
                switch (type.slot)
                {
                    case 1:
                        pokemonToAdd.Type1 = (PokemonType)Enum.Parse(typeof(PokemonType), type.type.name.FirstCharToUpper());
                        break;

                    case 2:
                        pokemonToAdd.Type2 = (PokemonType)Enum.Parse(typeof(PokemonType), type.type.name.FirstCharToUpper());
                        break;
                }
            }

            pokemonToAdd.LearnableByItems = new List<MoveBase>();

            foreach (var moves in pokemon.moves)
            {
                foreach (var details in moves.version_group_details)
                {
                    if (details.move_learn_method.name == "machine")
                    {
                        if (details.version_group.name == version_group)
                        {
                            MoveBase move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");

                            if (move == null && createMoveStubIfNotExistForPokemonMoves && moves.move.name?.Length > 0)
                            {
                                MoveBase moveBase = CreateInstance<MoveBase>();
                                AssetDatabase.CreateAsset(moveBase, $"Assets{pathToResources}{pathToMoveAssets}{moves.move.name}.asset");
                                yield return null;
                                move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");
                            }

                            if (move != null)
                            {
                                pokemonToAdd.LearnableByItems.Add(move);
                                break;
                            }
                        }

                        if (details.version_group.name == version_group_secondary)
                        {
                            MoveBase move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");

                            if (move == null && createMoveStubIfNotExistForPokemonMoves && moves.move.name?.Length > 0)
                            {
                                MoveBase moveBase = CreateInstance<MoveBase>();
                                AssetDatabase.CreateAsset(moveBase, $"Assets{pathToResources}{pathToMoveAssets}{moves.move.name}.asset");
                                yield return null;
                                move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");
                            }

                            if (move != null)
                            {
                                pokemonToAdd.LearnableByItems.Add(move);
                                break;
                            }
                        }
                    }
                }
            }

            pokemonToAdd.LearnableMoves = new List<LearnableMove>();

            foreach (var moves in pokemon.moves)
            {
                foreach (var details in moves.version_group_details)
                {
                    if (details.move_learn_method.name == "level-up" && details.version_group.name == version_group)
                    {
                        MoveBase move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");

                        if (move == null && createMoveStubIfNotExistForPokemonMoves && moves.move.name?.Length > 0)
                        {
                            MoveBase moveBase = CreateInstance<MoveBase>();
                            AssetDatabase.CreateAsset(moveBase, $"Assets{pathToResources}{pathToMoveAssets}{moves.move.name}.asset");
                            yield return null;
                            move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");
                        }

                        if (move != null)
                        {
                            var lm = new LearnableMove
                            {
                                Base = move,
                                Level = details.level_learned_at
                            };

                            pokemonToAdd.LearnableMoves.Add(lm);
                        }
                        break;
                    }

                    if (details.move_learn_method.name == "level-up" && details.version_group.name == version_group_secondary)
                    {
                        MoveBase move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");

                        if (move == null && createMoveStubIfNotExistForPokemonMoves && moves.move.name?.Length > 0)
                        {
                            MoveBase moveBase = CreateInstance<MoveBase>();
                            AssetDatabase.CreateAsset(moveBase, $"Assets{pathToResources}{pathToMoveAssets}{moves.move.name}.asset");
                            yield return null;
                            move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");
                        }

                        if (move != null)
                        {
                            var lm = new LearnableMove
                            {
                                Base = move,
                                Level = details.level_learned_at
                            };

                            pokemonToAdd.LearnableMoves.Add(lm);
                        }
                        break;
                    }
                }
            }

            pokemonToAdd.Evolutions = new List<Evolution>();

            List<PokeApiEvolvesTo> evolvesTo = new List<PokeApiEvolvesTo>();

            if (pokemon.species_data.evolution_data.chain.species.name == pokemon.name)
            {
                evolvesTo = pokemon.species_data.evolution_data.chain.evolves_to.ToList();
            }
            else
            {
                foreach (var evolution in pokemon.species_data.evolution_data.chain.evolves_to)
                {
                    if (evolution.species.name == pokemon.name)
                    {
                        foreach (var evolution2 in evolution.evolves_to)
                        {
                            var evo = new PokeApiEvolvesTo
                            {
                                evolution_details = evolution2.evolution_details,
                                species = evolution2.species,
                                is_baby = evolution2.is_baby
                            };

                            evolvesTo.Add(evo);
                        }
                    }
                }
            }

            foreach (var evolution in evolvesTo)
            {
                foreach (var detail in evolution.evolution_details)
                {
                    switch (detail.trigger.name)
                    {
                        case "level-up":
                            if (detail.min_level == 0)
                                break;

                            Evolution evolutionLevelUp = new Evolution();
                            evolutionLevelUp.RequiredLevel = detail.min_level;
                            evolutionLevelUp.EvolvesInto = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{evolution.species.name}");

                            if (evolutionLevelUp.EvolvesInto == null && createPokemonStubIfNotExistForEvolution)
                            {
                                PokemonBase pokemonBase = CreateInstance<PokemonBase>();
                                AssetDatabase.CreateAsset(pokemonBase, $"Assets/{pathToResources}{pathToPokemonAssets}{evolution.species.name}.asset");
                                yield return null;
                                evolutionLevelUp.EvolvesInto = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{evolution.species.name}");
                            }

                            if (evolutionLevelUp.EvolvesInto != null && pokemonToAdd.Evolutions.Where(e => e.RequiredItem == null && e.RequiredLevel == detail.min_level && e.EvolvesInto.name == evolution.species.name).Count() == 0)
                                pokemonToAdd.Evolutions.Add(evolutionLevelUp);
                            break;

                        case "use-item":
                            Evolution evolutionItem = new Evolution();
                            evolutionItem.RequiredItem = Resources.Load<EvolutionItem>($"{pathToItemAssets}{detail.item.name}");
                            evolutionItem.EvolvesInto = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{evolution.species.name}");

                            if (evolutionItem.EvolvesInto == null && createPokemonStubIfNotExistForEvolution)
                            {
                                PokemonBase pokemonBase = CreateInstance<PokemonBase>();
                                AssetDatabase.CreateAsset(pokemonBase, $"Assets{pathToResources}{pathToPokemonAssets}{evolution.species.name}.asset");
                                yield return null;
                                evolutionItem.EvolvesInto = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{evolution.species.name}");
                            }

                            if (evolutionItem.RequiredItem == null && createItemStubIfNotExistForEvolution && detail.item.name?.Length > 0)
                            {
                                EvolutionItem itemBase = CreateInstance<EvolutionItem>();
                                AssetDatabase.CreateAsset(itemBase, $"Assets{pathToResources}{pathToItemAssets}{evolutionItemsFolder}{detail.item.name}.asset");
                                evolutionItem.RequiredItem = Resources.Load<EvolutionItem>($"{pathToItemAssets}{evolutionItemsFolder}{detail.item.name}");
                            }

                            if (evolutionItem.EvolvesInto != null && evolutionItem.RequiredItem != null && !pokemonToAdd.Evolutions.Contains(evolutionItem))
                            {
                                pokemonToAdd.Evolutions.Add(evolutionItem);
                            }
                            break;
                    }
                }
            }

            if (fetchSprites)
            {
                // Front Sprite
                if (!File.Exists(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{spritesFolder}{pokemon.id}.png"))
                {
                    instance = CreateInstance<PokeApi>();
                    yield return instance.DownloadSprite(pokemon.sprites.front_default, Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{spritesFolder}{pokemon.id}.png");
                    yield return new WaitForEndOfFrame();
                }

                // Back Sprite
                if (!File.Exists(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{spritesFolder}{pokemon.id}-back.png"))
                {
                    instance = ScriptableObject.CreateInstance<PokeApi>();
                    yield return instance.DownloadSprite(pokemon.sprites.back_default, Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{spritesFolder}{pokemon.id}-back.png");
                    yield return new WaitForEndOfFrame();
                }

                AssetDatabase.Refresh();
                yield return null;
                Sprite front = Resources.Load<Sprite>($"{pathToPokemonAssets}{spritesFolder}{pokemon.id}");
                Sprite back = Resources.Load<Sprite>($"{pathToPokemonAssets}{spritesFolder}{pokemon.id}-back");
                pokemonToAdd.FrontSprite = front;
                pokemonToAdd.BackSprite = back;
            }

            if (isNew)
                AssetDatabase.CreateAsset(pokemonToAdd, $"Assets/{pathToResources}{pathToPokemonAssets}{pokemon.name}.asset");

            AssetDatabase.SaveAssets();

            yield return null;
        }
    }
}
