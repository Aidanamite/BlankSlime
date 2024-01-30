using HarmonyLib;
using SRML;
using SRML.Console;
using SRML.SR;
using SRML.Utils;
using SRML.SR.Translation;
using SRML.Utils.Enum;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using MoSecretStyles;
using Secret_Style_Things.Utils;
using System.Reflection.Emit;
using Console = SRML.Console.Console;
using Object = UnityEngine.Object;

namespace BlankSlime
{
    public class Main : ModEntryPoint
    {
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{Environment.CurrentDirectory}\\SRML\\Mods\\{modName}";
        internal static Sprite slimeIcon = LoadImage("slimeBlank.png").CreateSprite();
        internal static Sprite plortIcon = LoadImage("plortBlank.png").CreateSprite();
        internal static Dictionary<Identifiable.Id, Identifiable.Id> pureLargos = new Dictionary<Identifiable.Id, Identifiable.Id>();
        internal static Dictionary<Identifiable.Id, Identifiable.Id> forcedAccociation = new Dictionary<Identifiable.Id, Identifiable.Id>();
        internal static List<SlimeDiet> diets = new List<SlimeDiet>();
        internal static Transform prefabParent;
        internal static Console.ConsoleInstance Console;

        public Main()
        {
            var p = new GameObject("PrefabParent");
            p.SetActive(false);
            Object.DontDestroyOnLoad(p);
            prefabParent = p.transform;
        }

        public override void PreLoad()
        {
            Console = ConsoleInstance;
            HarmonyInstance.PatchAll();
            SlimeEat.FoodGroup.NONTARRGOLD_SLIMES.AddItem(Ids.BLANK_SLIME);
            SlimeEat.FoodGroup.PLORTS.AddItem(Ids.BLANK_PLORT);
            PediaRegistry.RegisterIdEntry(Ids2.BLANK_SLIME, slimeIcon);
            PediaRegistry.RegisterIdentifiableMapping(Ids2.BLANK_SLIME, Ids.BLANK_SLIME);
            PediaRegistry.SetPediaCategory(Ids2.BLANK_SLIME, PediaRegistry.PediaCategory.SLIMES);
            AmmoRegistry.RegisterPlayerAmmo(PlayerState.AmmoMode.DEFAULT, Ids.BLANK_SLIME);
            var plortVac = ScriptableObject.CreateInstance<VacItemDefinition>();
            plortVac.color = Color.gray;
            plortVac.id = Ids.BLANK_PLORT;
            plortVac.icon = plortIcon;
            LookupRegistry.RegisterVacEntry(plortVac);
            AmmoRegistry.RegisterPlayerAmmo(PlayerState.AmmoMode.DEFAULT, Ids.BLANK_PLORT);

            DataModelRegistry.RegisterActorModelOverride((x) => pureLargos.ContainsValue(x), (actorId, id, region, obj) =>
              {
                  foreach (var p in pureLargos)
                      if (id == p.Value)
                      {
                          var m = SceneContext.Instance.GameModel.CreateActorModel(actorId, p.Key, region, obj);
                          m.ident = p.Value;
                          return m;
                      }
                  return null;
              });

            if (SRModLoader.IsModPresent("mosecretstyles"))
                MSS.DoTheThing();
        }
        public override void Load()
        {
            EnumTranslator.RegisterFallbackHandler<Identifiable.Id>((ref string x) =>
            {
                if (x.EndsWith("_LARGO") && Enum.TryParse<Identifiable.Id>(x.RemoveAfterLast("_") + "_PURELARGO",out var e) && pureLargos.ContainsValue(e))
                {
                    x = x.RemoveAfterLast("_") + "_PURELARGO";
                    return true;
                }
                return false;
            });
            var pinkDef = GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.PINK_SLIME);
            var def = Object.Instantiate(pinkDef);
            def.AppearancesDefault = new SlimeAppearance[] { Object.Instantiate(def.AppearancesDefault[0]) };
            def.CanLargofy = false;
            def.Diet = new SlimeDiet()
            {
                AdditionalFoods = new Identifiable.Id[] { Identifiable.Id.SPICY_TOFU },
                Favorites = new Identifiable.Id[0],
                FavoriteProductionCount = 2,
                MajorFoodGroups = new SlimeEat.FoodGroup[0],
                Produces = new Identifiable.Id[] { Ids.BLANK_PLORT }
            };
            def.FavoriteToys = new Identifiable.Id[] {
                Identifiable.Id.ECHO_NOTE_01,
                Identifiable.Id.ECHO_NOTE_02,
                Identifiable.Id.ECHO_NOTE_03,
                Identifiable.Id.ECHO_NOTE_04,
                Identifiable.Id.ECHO_NOTE_05,
                Identifiable.Id.ECHO_NOTE_06,
                Identifiable.Id.ECHO_NOTE_08,
                Identifiable.Id.ECHO_NOTE_09,
                Identifiable.Id.ECHO_NOTE_10,
                Identifiable.Id.ECHO_NOTE_11,
                Identifiable.Id.ECHO_NOTE_12,
                Identifiable.Id.ECHO_NOTE_13
            };
            def.IdentifiableId = Ids.BLANK_SLIME;
            def.IsLargo = false;
            def.Name = "Blank Slime";
            def.name = "BlankSlime";
            def.PrefabScale = 1;
            def.Diet.RefreshEatMap(GameContext.Instance.SlimeDefinitions, def);
            var a = def.AppearancesDefault[0];
            a.Face = Object.Instantiate(a.Face);
            var faces = a.Face.ExpressionFaces;
            var rep = new Dictionary<Material, Material>();
            for (int i = 0; i < faces.Length; i++)
            {
                if (faces[i].Eyes)
                {
                    if (!rep.TryGetValue(faces[i].Eyes, out var eye))
                    {
                        eye = faces[i].Eyes.Clone();
                        eye.name = eye.name.Replace("(Clone)", "").Replace(" (Instance)", "") + "Blank";
                        if (eye.HasProperty("_EyeRed"))
                            eye.SetColor("_EyeRed", Color.black);
                        if (eye.HasProperty("_EyeGreen"))
                            eye.SetColor("_EyeGreen", Color.black);
                        if (eye.HasProperty("_EyeBlue"))
                            eye.SetColor("_EyeBlue", Color.black);
                        rep.Add(faces[i].Eyes, eye);
                    }
                    faces[i].Eyes = eye;
                }
                if (faces[i].Mouth)
                {
                    if (!rep.TryGetValue(faces[i].Mouth, out var mouth))
                    {
                        mouth = faces[i].Mouth.Clone();
                        mouth.name = mouth.name.Replace("(Clone)", "").Replace(" (Instance)", "") + "Blank";
                        if (mouth.HasProperty("_MouthTop"))
                            mouth.SetColor("_MouthTop", Color.black);
                        if (mouth.HasProperty("_MouthMid"))
                            mouth.SetColor("_MouthMid", Color.black);
                        if (mouth.HasProperty("_MouthBot"))
                            mouth.SetColor("_MouthBot", Color.black);
                        rep.Add(faces[i].Mouth, mouth);
                    }
                    faces[i].Mouth = mouth;
                }
            }
            a.Face.OnEnable();
            a.Structures = new SlimeAppearanceStructure[] { new SlimeAppearanceStructure(a.Structures[0]) };
            a.ColorPalette.Ammo = Color.gray;
            a.ColorPalette.Top = Color.white;
            a.ColorPalette.Middle = Color.white;
            a.ColorPalette.Bottom = Color.white;
            Material body = a.Structures[0].DefaultMaterials[0].Clone();
            body.name = "slimeBlank";
            a.Structures[0].DefaultMaterials[0] = body;
            body.SetColor("_TopColor", Color.white);
            body.SetColor("_MiddleColor", Color.white);
            body.SetColor("_BottomColor", Color.white);
            a.Icon = slimeIcon;

            var slimePrefab = Object.Instantiate(GameContext.Instance.LookupDirector.GetPrefab(Identifiable.Id.PINK_SLIME), prefabParent, false);
            slimePrefab.name = "slimeBlank";
            var id = slimePrefab.GetComponent<Identifiable>();
            id.id = Ids.BLANK_SLIME;
            var app = slimePrefab.GetComponent<SlimeAppearanceApplicator>();
            app.SlimeDefinition = def;
            app.Appearance = a;
            var eat = slimePrefab.GetComponent<SlimeEat>();
            eat.slimeDefinition = def;
            var emo = slimePrefab.GetComponent<SlimeEmotions>();
            emo.initAgitation = new SlimeEmotions.EmotionState(SlimeEmotions.Emotion.AGITATION, 0.5f, 0.5f, 0, 0);
            emo.initFear = new SlimeEmotions.EmotionState(SlimeEmotions.Emotion.FEAR, 0.5f, 0.5f, 0, 0);
            emo.initHunger = new SlimeEmotions.EmotionState(SlimeEmotions.Emotion.HUNGER, 0.5f, 0.5f, 0, 0);
            emo.initAgitation.SetEnabled(false);
            emo.initFear.SetEnabled(false);
            LookupRegistry.RegisterIdentifiablePrefab(id);
            SlimeRegistry.RegisterSlimeDefinition(def);

            var plortPrefab = Object.Instantiate(GameContext.Instance.LookupDirector.GetPrefab(Identifiable.Id.PINK_PLORT), prefabParent, false);
            plortPrefab.name = "plortBlank";
            id = plortPrefab.GetComponent<Identifiable>();
            id.id = Ids.BLANK_PLORT;
            var rend = plortPrefab.GetComponent<Renderer>();
            rend.sharedMaterial = a.Structures[0].DefaultMaterials[0];
            LookupRegistry.RegisterIdentifiablePrefab(id);
            

            var dir = SceneContext.Instance.SlimeAppearanceDirector;
            dir.onSlimeAppearanceChanged += (x, y) =>
            {
                if (pureLargos.TryGetValue(x.IdentifiableId, out var value))
                    try
                    {
                        dir.UpdateChosenSlimeAppearance(dir.SlimeDefinitions.GetSlimeByIdentifiableId(value), y);
                    }
                    catch (Exception e)
                    {
                        LogError("An error occured updating the appearance of " + value + "\n" + e);
                    }
            };
        }


        public static void Log(string message) => Console.Log($"[{modName}]: " + message);
        public static void LogError(string message) => Console.LogError($"[{modName}]: " + message);
        public static void LogWarning(string message) => Console.LogWarning($"[{modName}]: " + message);
        public static void LogSuccess(string message) => Console.LogSuccess($"[{modName}]: " + message);

        public static Texture2D LoadImage(string filename, TextureWrapMode wrapMode = default)
        {
            var a = modAssembly;
            var spriteData = a.GetManifestResourceStream(a.GetName().Name + "." + filename);
            var rawData = new byte[spriteData.Length];
            spriteData.Read(rawData, 0, rawData.Length);
            var tex = new Texture2D(0,0);
            tex.LoadImage(rawData);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = wrapMode;
            return tex;
        }

        public static void CreateLargo(Identifiable.Id original, Identifiable.Id largo)
        {
            var oDef = GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(original);
            var prefab = GameContext.Instance.LookupDirector.GetPrefab(original);
            if (!oDef || !prefab)
                return;
            if (DebugConfig.ExtendedLogging)
                Log($"Creating pure largo for ${original}");
            SlimeDefinition nDef;
            try
            {
                nDef = Object.Instantiate(oDef);
            } catch
            {
                return;
            }
            nDef.CanLargofy = false;
            nDef.IdentifiableId = largo;
            nDef.Name = oDef.Name == null ? largo.ToName() : (oDef.Name.Contains("Slime") ? oDef.Name.Replace("Slime", "Largo") : (oDef.Name + " Largo"));
            nDef.name = oDef.name == null ? largo.ToName(false) : (oDef.name.Contains("Slime") ? oDef.name.Replace("Slime", "Largo") : (oDef.name + "Largo"));
            if (oDef.Diet == null)
            {
                if (!DebugConfig.DisableWarnings)
                    LogWarning(original + " has a null diet, " + largo + " will not eat anything as a result");
                nDef.Diet = new SlimeDiet()
                {
                    AdditionalFoods = new Identifiable.Id[0],
                    Favorites = new Identifiable.Id[0],
                    MajorFoodGroups = new SlimeEat.FoodGroup[0],
                    Produces = new Identifiable.Id[0],
                    EatMap = new List<SlimeDiet.EatMapEntry>()
                };
            }
            else
                nDef.Diet = new SlimeDiet()
                {
                    AdditionalFoods = oDef.Diet.AdditionalFoods,
                    Favorites = oDef.Diet.Favorites,
                    FavoriteProductionCount = oDef.Diet.FavoriteProductionCount,
                    MajorFoodGroups = oDef.Diet.MajorFoodGroups,
                    Produces = oDef.Diet.Produces,
                    EatMap = oDef.Diet.EatMap
                };
            diets.Add(nDef.Diet);
            nDef.BaseSlimes = new SlimeDefinition[] { oDef };

            var slimePrefab = Object.Instantiate(prefab, prefabParent, false);
            slimePrefab.name = slimePrefab.name.Replace("(Clone)", "").Replace("slime", "largo");
            slimePrefab.transform.localScale *= 2;
            var oId = prefab.GetComponent<Identifiable>();
            foreach (var c in slimePrefab.GetComponentsInChildren<SlimeAppearanceObject>())
                Object.DestroyImmediate(c);
            var applicator = slimePrefab.GetComponent<SlimeAppearanceApplicator>();
            if (applicator.recalculateBoundsHelper)
                Object.DestroyImmediate(applicator.recalculateBoundsHelper);
            foreach (var c in slimePrefab.GetComponentsInChildren<MonoBehaviour>(true))
            {
                foreach (var f in c.GetType().GetFields((BindingFlags)(-1)))
                {
                    if (f.FieldType == typeof(Identifiable.Id) && (Identifiable.Id)f.GetValue(c) == original)
                        f.SetValue(c, largo);
                    if ((f.FieldType == typeof(SlimeDefinition) || typeof(SlimeDefinition).IsSubclassOf(f.FieldType)) && (f.GetValue(c) as SlimeDefinition) == oDef)
                        f.SetValue(c, nDef);
                }
            }
            var vac = slimePrefab.GetComponent<Vacuumable>();
            if (!vac)
            {
                if (!DebugConfig.DisableWarnings)
                    LogWarning(original + " is missing a Vacuumable component on its prefab");
            }
            else if (vac.size <= Vacuumable.Size.LARGE)
                vac.size++;
            LookupRegistry.RegisterIdentifiablePrefab(slimePrefab.GetComponent<Identifiable>());
            SlimeRegistry.RegisterSlimeDefinition(nDef);
            nDef.IsLargo = true;
            if (SlimeEat.FoodGroup.NONTARRGOLD_SLIMES.Contains(original))
                SlimeEat.FoodGroup.NONTARRGOLD_SLIMES.AddItem(largo);
            if (GameContext.Instance.LookupDirector.vacItemDict.TryGetValue(original, out var oItem))
            {
                var item = Object.Instantiate(oItem);
                item.id = largo;
                LookupRegistry.RegisterVacEntry(item);
            }
        }

        public static void SetForcedAccosiation(Identifiable.Id slimeId, Identifiable.Id plortId)
        {
            if (forcedAccociation.TryGetValue(plortId, out var old) && old != slimeId && !DebugConfig.DisableWarnings)
                LogWarning($"The slime id {old} is already accossiated with the plort id {plortId}. Value being overriden with {slimeId}");
            forcedAccociation[plortId] = slimeId;
        }
    }

    [SRML.Config.Attributes.ConfigFile("Debugging")]
    static class DebugConfig
    {
        public static bool ExtendedLogging = false;
        public static bool DisableWarnings = false;
    }

    [SRML.Config.Attributes.ConfigFile("General")]
    static class Config
    {
        public static int SpawnChance = 100;
        public static float ProbabilityOfMainSlime = 90;
    }

    [EnumHolder]
    static class Ids
    {
        public static Identifiable.Id BLANK_SLIME;
        public static Identifiable.Id BLANK_PLORT;
    }

    [EnumHolder]
    static class Ids2
    {
        public static PediaDirector.Id BLANK_SLIME;
    }

    static class ExtentionMethods
    {
        public static Sprite CreateSprite(this Texture2D texture) => Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1);

        public static Material Clone(this Material material)
        {
            var m = new Material(material);
            m.CopyPropertiesFromMaterial(material);
            return m;
        }
        public static string RemoveAfterLast(this string value, string delimeter) => !string.IsNullOrEmpty(delimeter) && !string.IsNullOrEmpty(value) && value.Contains(delimeter) ? value.Remove(value.LastIndexOf(delimeter)) : value;
        public static string RemoveBefore(this string value, string delimeter) => !string.IsNullOrEmpty(delimeter) && !string.IsNullOrEmpty(value) && value.Contains(delimeter) ? value.Remove(0, value.IndexOf(delimeter) + delimeter.Length) : value;

        public static string GetSuffix(this IEnumerable<string> a)
        {
            var m = a.Min((x) => x.Length);
            var c = "";
            while (c.Length < m)
            {
                var e = a.First().Remove(0, a.First().Length - c.Length - 1);
                if (a.All((x) => e == x.Remove(0, x.Length - c.Length - 1)))
                    c = e;
                else
                    break;
            }
            return c;
        }
        public static string GetPrefix(this IEnumerable<string> a)
        {
            var m = a.Min((x) => x.Length);
            var c = "";
            while (c.Length < m)
            {
                var e = a.First().Remove(c.Length + 1);
                if (a.All((x) => e == x.Remove(c.Length + 1)))
                    c = e;
                else
                    break;
            }
            return c;
        }
        public static string GetName(this Dictionary<string, string> c, Identifiable.Id id)
        {
            if (c.TryGetValue("l." + id.ToString().ToLowerInvariant(), out var r))
                return r;
            if (!DebugConfig.DisableWarnings)
                Main.LogWarning("Failed to find name for " + id);
            return "???";
        }

        public static string ToName(this Identifiable.Id id, bool withSpaces = true, bool lowerFirstLetter = false)
        {
            var parts = id.ToString().ToLowerInvariant().Split('_');
            for (int i = lowerFirstLetter ? 1 : 0; i < parts.Length; i++)
                if (parts[i].Length == 1)
                    parts[i] = parts[i].ToUpperInvariant();
                else if (parts[i].Length > 1)
                    parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Remove(0,1);
            return parts.Join(delimiter: withSpaces ? " " : "");
        }

        public static bool AddUnique<T>(this List<T> l, T value)
        {
            if (l.Contains(value))
                return false;
            l.Add(value);
            return true;
        }

        public static void AddRangeUnique<T>(this List<T> l, IEnumerable<T> values)
        {
            foreach (var v in values)
                l.AddUnique(v);
        }

        public static void AddItem(this SlimeEat.FoodGroup foodGroup, Identifiable.Id ident) => SlimeEat.foodGroupIds[foodGroup] = SlimeEat.foodGroupIds.TryGetValue(foodGroup, out var v) ? v.AddToArray(ident) : new Identifiable.Id[] { ident };
        public static bool Contains(this SlimeEat.FoodGroup foodGroup, Identifiable.Id ident) => SlimeEat.foodGroupIds.TryGetValue(foodGroup, out var v) ? v.Contains(ident) : false;
    }

    [HarmonyPatch(typeof(ResourceBundle), "LoadFromText")]
    class Patch_LoadResources
    {
        static void Postfix(string path, Dictionary<string, string> __result)
        {
            var lang = GameContext.Instance.MessageDirector.GetCultureLang();
            if (path == "actor")
            {
                var slime = Ids.BLANK_SLIME.ToString().ToLowerInvariant();
                var plort = Ids.BLANK_PLORT.ToString().ToLowerInvariant();
                if (lang == MessageDirector.Lang.RU)
                {
                    __result["l." + slime] = "Слайм ";
                    __result["l." + plort] = "Плорт";
                    __result["l.secret_style_" + slime] = "Чистый";
                }
                else
                {
                    __result["l." + slime] = "Slime ";
                    __result["l." + plort] = "Plort";
                    __result["l.secret_style_" + slime] = "Clear";
                }
            }
            else if (path == "pedia")
            {
                var slime = Ids2.BLANK_SLIME.ToString().ToLowerInvariant();
                if (lang == MessageDirector.Lang.RU)
                {
                    __result["t." + slime] = "Слайм";
                    __result["m.intro." + slime] = "Это... Слайм?";
                    __result["m.slimeology." + slime] = "Этот слайм не был замечен за проявлением уникального поведения за исключением отсутствия интереса к еде";
                    __result["m.plortonomics." + slime] = "В редких случаях, плорт, напоминающий слайма, был найден возле него, и считалось, что этот плорт был произведен этим слаймом. Единственным использованием этих плортов было превращение обычных слаймов в чистых ларго";
                    __result["m.favorite." + slime] = "Неизвестно";
                    __result["m.risks." + slime] = "Неизвестно";
                    __result["m.diet." + slime] = "Неизвестно";
                }
                else
                {
                    __result["t." + slime] = "Slime";
                    __result["m.intro." + slime] = "It's a.... slime?";
                    __result["m.slimeology." + slime] = "This slime has not been recorded exibiting any behaviours unique to it's kind aside from an apparent lack of interest in most food";
                    __result["m.plortonomics." + slime] = "On rare occasion a plort resembling the slime has been found near it and is assumed to be produced by the slime. These plorts only known use is in the formation of pure largos";
                    __result["m.favorite." + slime] = "Unknown";
                    __result["m.risks." + slime] = "Unknown";
                    __result["m.diet." + slime] = "Unknown";
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerState), "Reset")]
    class Patch_PlayerReset
    {
        static void Postfix(PlayerState __instance)
        {
            foreach (var a in __instance.ammoDict)
            {
                var include = new List<Identifiable.Id>();
                var exclude = new List<Identifiable.Id>();
                foreach (var i in a.Value.potentialAmmo)
                {
                    if (Main.pureLargos.TryGetValue(i, out var v))
                        include.AddUnique(v);
                    if (Main.pureLargos.ContainsValue(i))
                        exclude.AddUnique(i);
                }
                foreach (var i in include.Except(exclude))
                    a.Value.potentialAmmo.Add(i);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerState),"GetPotentialAmmo")]
    class Patch_PlayerGetAmmo
    {
        static void Postfix(HashSet<Identifiable.Id> __result)
        {
            var include = new List<Identifiable.Id>();
            var exclude = new List<Identifiable.Id>();
            foreach (var i in __result)
            {
                if (Main.pureLargos.TryGetValue(i, out var v))
                    include.AddUnique(v);
                if (Main.pureLargos.ContainsValue(i))
                    exclude.AddUnique(i);
            }
            foreach (var i in include.Except(exclude))
                __result.Add(i);
        }
    }

    [HarmonyPatch(typeof(SlimeDiet.EatMapEntry),"NumToProduce")]
    class Patch_EatProduction
    {
        static void Postfix(SlimeDiet.EatMapEntry __instance, ref int __result)
        {
            if (Main.diets.Exists((x) => x.EatMap.Contains(__instance)))
                __result *= 2;
        }
    }

    [HarmonyPatch(typeof(SlimeDiet), "RefreshEatMap")]
    class Patch_RefreshEat
    {
        static void Postfix(SlimeDiet __instance, SlimeDefinitions definitions, SlimeDefinition definition)
        {
            if (Main.pureLargos.TryGetValue(definition.IdentifiableId, out var value))
            {
                __instance.EatMap.RemoveAll((x) => x.eats == Ids.BLANK_PLORT);
                __instance.EatMap.Add(new SlimeDiet.EatMapEntry()
                {
                    becomesId = value,
                    eats = Ids.BLANK_PLORT,
                    minDrive = 1
                });
            }
            else if (Main.pureLargos.ContainsValue(definition.IdentifiableId))
                __instance.EatMap.RemoveAll((x) => x.becomesId == definition.IdentifiableId);
            else if (definition.IdentifiableId == Ids.BLANK_SLIME)
            {
                Dictionary<Identifiable.Id, HashSet<Identifiable.Id>> connections = new Dictionary<Identifiable.Id, HashSet<Identifiable.Id>>();
                void Add(Identifiable.Id slime, Identifiable.Id plort)
                {
                    if (!connections.TryGetValue(plort, out var slimes))
                        connections[plort] = slimes = new HashSet<Identifiable.Id>();
                    slimes.Add(slime);
                }
                if (DebugConfig.ExtendedLogging)
                    Main.Log("Setting up blank slime transformations");
                foreach (var s in Main.pureLargos.Keys)
                {
                    var def = definitions.GetSlimeByIdentifiableId(s);
                    if (def && def.Diet?.Produces?.Length > 0)
                        foreach (var item in def.Diet.Produces)
                            Add(s, item);
                    var p = GameContext.Instance.LookupDirector.GetPrefab(s);
                    if (p)
                        foreach (var c in p.GetComponents<Component>())
                            if (c.GetType().Name.Contains("Eat"))
                            {
                                var obj = Traverse.Create(c).Field("plort").GetValue();
                                if (obj is GameObject g && g && g.GetComponent<Identifiable>() is Identifiable id && id && Identifiable.IsPlort(id.id))
                                        Add(s, id.id);
                                else if (obj is Component c2 && c2 && c2.GetComponent<Identifiable>() is Identifiable id2 && Identifiable.IsPlort(id2.id))
                                        Add(s, id2.id);
                                else if (obj is Identifiable.Id id3 && Identifiable.IsPlort(id3))
                                        Add(s, id3);
                            }
                }
                foreach (var item in Main.forcedAccociation)
                    Add(item.Value, item.Key);
                foreach (var plort in connections)
                {
                    (Identifiable.Id id, int similarity) bestMatch = default;
                    if (Main.forcedAccociation.TryGetValue(plort.Key, out var forced))
                        bestMatch = (forced, 0);
                    else if (plort.Value.Count == 1)
                        bestMatch = (plort.Value.FirstOrDefault(), 0);
                    else
                    {
                        var pn = plort.Key.ToString().ToLowerInvariant();
                        foreach (var slime in plort.Value)
                        {
                            var sn = slime.ToString().ToLowerInvariant();
                            var n = 0;
                            for (int i = 0; i <= pn.Length && i <= sn.Length; i++)
                                if (i == pn.Length && i == sn.Length)
                                    n = i;
                                else if (pn[i] != sn[i])
                                {
                                    n = i;
                                    break;
                                }
                            if (bestMatch.id == default || n > bestMatch.similarity)
                                bestMatch = (slime, n);
                        }
                    }
                    if (DebugConfig.ExtendedLogging)
                        Main.Log($"Added transformation when eating {plort.Key} [main={bestMatch.id},all={plort.Value.Join(delimiter: ",")}]");
                    __instance.EatMap.Add(new EatMapEntry()
                    {
                        becomesId = bestMatch.id,
                        eats = plort.Key,
                        driver = SlimeEmotions.Emotion.NONE,
                        minDrive = 1,
                        possibilities = new List<Identifiable.Id>(plort.Value)
                    });
                }
            }
        }
    }

    public class EatMapEntry : SlimeDiet.EatMapEntry
    {
        public List<Identifiable.Id> possibilities;
    }

    [HarmonyPatch(typeof(MessageDirector), "LoadBundle")]
    class Patch_MessageBundleLoad
    {
        static void Postfix(MessageDirector __instance, string path, ResourceBundle __result)
        {
            if (path == "actor" && __result != null)
            {
                var dict = __result.dict;
                var a = new string[]
                {
                dict.GetName(Identifiable.Id.BOOM_SLIME),
                dict.GetName(Identifiable.Id.CRYSTAL_SLIME),
                dict.GetName(Identifiable.Id.DERVISH_SLIME),
                dict.GetName(Identifiable.Id.FIRE_SLIME),
                dict.GetName(Identifiable.Id.HONEY_SLIME),
                dict.GetName(Identifiable.Id.HUNTER_SLIME),
                dict.GetName(Identifiable.Id.MOSAIC_SLIME),
                dict.GetName(Identifiable.Id.PHOSPHOR_SLIME),
                dict.GetName(Identifiable.Id.PINK_SLIME),
                dict.GetName(Identifiable.Id.PUDDLE_SLIME),
                dict.GetName(Identifiable.Id.QUANTUM_SLIME),
                dict.GetName(Identifiable.Id.RAD_SLIME),
                dict.GetName(Identifiable.Id.ROCK_SLIME),
                dict.GetName(Identifiable.Id.TABBY_SLIME),
                dict.GetName(Identifiable.Id.TANGLE_SLIME)
                };
                var slimeSuffix = a.GetSuffix();
                var slimePrefix = a.GetPrefix();

                a = new string[]
                {
                dict.GetName(Identifiable.Id.BOOM_CRYSTAL_LARGO),
                dict.GetName(Identifiable.Id.HONEY_DERVISH_LARGO),
                dict.GetName(Identifiable.Id.MOSAIC_HUNTER_LARGO),
                dict.GetName(Identifiable.Id.PINK_PHOSPHOR_LARGO),
                dict.GetName(Identifiable.Id.QUANTUM_RAD_LARGO),
                dict.GetName(Identifiable.Id.ROCK_TABBY_LARGO)
                };
                var largoSuffix = a.GetSuffix();
                var largoPrefix = a.GetPrefix();

                if (DebugConfig.ExtendedLogging)
                    Main.Log($"Translation generation data:\nSlime Name Format: {slimePrefix}{{TYPE}}{slimeSuffix}\nLargo Name Format: {largoPrefix}{{TYPE}}{largoSuffix}");

                foreach (var p in Main.pureLargos)
                {
                    dict["l." + p.Value.ToString().ToLower()] = largoPrefix + dict.GetName(p.Key).RemoveBefore(slimePrefix).RemoveAfterLast(slimeSuffix) + largoSuffix;
                    if (DebugConfig.ExtendedLogging)
                        Main.Log($"Generated translation for \"{"l." + p.Value.ToString().ToLower()}\": \"{dict["l." + p.Value.ToString().ToLower()]}\"");
                }
            }
        }
    }

    [HarmonyPatch(typeof(IdentifiableRegistry), "CategorizeAllIds")]
    class Patch_PostCatergorize
    {
        public static void Postfix()
        {
            SRMod.ForceModContext(SRModLoader.GetModForAssembly(Main.modAssembly));
            foreach (var s in Identifiable.SLIME_CLASS)
            {
                if (Identifiable.IsLargo(s) || s == Ids.BLANK_SLIME)
                    continue;
                var nv = EnumPatcher.AddEnumValue(typeof(Identifiable.Id), s.ToString().Replace("_SLIME", "_PURELARGO"));
                if (nv != null)
                {
                    var id = (Identifiable.Id)nv;
                    Main.pureLargos.Add(s, id);
                    IdentifiableRegistry.CategorizeId(id,IdentifiableCategorization.Rule.LARGO);
                }
            }
            SRMod.ClearModContext();
        }
    }

    [HarmonyPatch(typeof(SRModLoader), "LoadMods")]
    class Patch_PostLoad
    {
        public static void Postfix()
        {
            foreach (var p in Main.pureLargos)
                Main.CreateLargo(p.Key, p.Value);
        }
    }

    [HarmonyPatch(typeof(SRModLoader), "PostLoadMods")]
    class Patch_PostPostLoad
    {
        public static void Postfix()
        {
            foreach (var p in Main.pureLargos)
            {
                if (GameContext.Instance.LookupDirector.GetPrefab(p.Value) == null)
                {
                    Main.CreateLargo(p.Key, p.Value);
                    if (GameContext.Instance.LookupDirector.GetPrefab(p.Value) == null && !DebugConfig.DisableWarnings)
                        Main.LogWarning($"Failed to create largo for " + p.Key);
                }
            }
        }
    }

    [HarmonyPatch(typeof(AweTowardsLargos), "GetSearchIds")]
    class Patch_LargoAwe
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            var ind = code.FindIndex((x) => x.opcode == OpCodes.Call && x.operand is MethodInfo && (x.operand as MethodInfo).DeclaringType == typeof(Log) && (x.operand as MethodInfo).Name == "Error") + 1;
            var label = (Label)code[code.FindIndex((x) => x.opcode == OpCodes.Call && x.operand is MethodInfo && (x.operand as MethodInfo).DeclaringType == typeof(Object) && (x.operand as MethodInfo).Name == "op_Inequality") + 1].operand;
            code.Insert(ind++, new CodeInstruction(OpCodes.Br,label));
            return code;
        }
    }

    [HarmonyPatch(typeof(SlimeEat), "EatAndTransform")]
    class Patch_Slime_EatAndTransform
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            var ind = code.FindIndex((x) => x.opcode == OpCodes.Ldfld && x.operand is FieldInfo f && f.DeclaringType == typeof(SlimeDiet.EatMapEntry) && f.Name == "becomesId");
            code.InsertRange(ind + 1, new[] {
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Slime_EatAndTransform),nameof(MaybeReplace)))
            });
            return code;
        }
        static Identifiable.Id MaybeReplace(Identifiable.Id original, SlimeDiet.EatMapEntry entry)
        {
            if (entry is EatMapEntry blankslime && !Randoms.SHARED.GetProbability(Config.ProbabilityOfMainSlime / 100))
                return Randoms.SHARED.Pick(blankslime.possibilities, original);
            return original;
        }
    }

    /*[HarmonyPatch(typeof(SlimeAppearanceApplicator), "SetExpression")]
    class Patch_ActorCreate
    {
        static bool Prefix(SlimeAppearanceApplicator __instance, SlimeFace.SlimeExpression slimeExpression)
        {
            int i = 0;
            int j = 0;
            try
            {
                var app = __instance.Appearance.Face;
                i = -1;
                SlimeExpressionFace expressionFace = app.GetExpressionFace(slimeExpression);
                i = 1;
                foreach (SlimeAppearanceApplicator.FaceRenderer faceRenderer in __instance._faceRenderers)
                {
                    i = 2;
                    Material[] sharedMaterials = faceRenderer.Renderer.sharedMaterials;
                    i = 3;
                    int num = sharedMaterials.Length - 2;
                    i = 4;
                    int num2 = sharedMaterials.Length - 1;
                    i = 5;
                    if (faceRenderer.ShowEyes != faceRenderer.ShowMouth)
                    {
                        i = 6;
                        num = num2;
                    }
                    i = 7;
                    if (faceRenderer.ShowEyes && expressionFace.Eyes != null)
                    {
                        i = 8;
                        sharedMaterials[num] = expressionFace.Eyes;
                    }
                    i = 9;
                    if (faceRenderer.ShowMouth && expressionFace.Mouth != null)
                    {
                        i = 10;
                        sharedMaterials[num2] = expressionFace.Mouth;
                    }
                    i = 11;
                    faceRenderer.Renderer.sharedMaterials = sharedMaterials;
                    i = 12;
                    j++;
                }
            } catch (Exception e) { Console.LogError($"Error at {i}:{j} - ({__instance}, {slimeExpression})\n{e}"); }
            return false;
        }
    }*/

    [HarmonyPatch(typeof(SlimeAppearanceDirector), "GetChosenSlimeAppearance", typeof(SlimeDefinition))]
    class Patch_CreateActorModel
    {
        public static void Prefix(ref SlimeDefinition slimeDefinition)
        {
            foreach (var p in Main.pureLargos)
                if (slimeDefinition.IdentifiableId == p.Value)
                    slimeDefinition = GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(p.Key);
        }
    }

    [HarmonyPatch(typeof(DirectedSlimeSpawner), "MaybeReplacePrefab")]
    class Patch_SlimeSpawnerPrefab
    {
        static void Postfix(ref GameObject __result)
        {
            if (Config.SpawnChance > 0 && Randoms.SHARED.GetProbability(1f / Config.SpawnChance))
                __result = GameContext.Instance.LookupDirector.GetPrefab(Ids.BLANK_SLIME);
        }
    }

    [HarmonyPatch(typeof(SlimeAppearanceApplicator), "Awake")]
    class Patch_SlimeA
    {
        static void Prefix(SlimeAppearanceApplicator __instance)
        {
            if (!__instance.SlimeDefinition && !DebugConfig.DisableWarnings)
                Main.LogError("Missing SlimeDefinition for " + __instance.name);
            if (!__instance.SlimeAppearanceDirector && !DebugConfig.DisableWarnings)
                Main.LogError("Missing SlimeAppearanceDirector for " + __instance.name);
        }
    }

    [HarmonyPatch(typeof(ExchangeDirector),"Awake")]
    class Patch_ExchangeStart
    {
        public static void Prefix(ExchangeDirector __instance)
        {
            __instance.values = __instance.values.AddRangeToArray(new ExchangeDirector.ValueEntry[] {
                new ExchangeDirector.ValueEntry()
                {
                    id = Ids.BLANK_SLIME,
                    value = 10
                },
                new ExchangeDirector.ValueEntry()
                {
                    id = Ids.BLANK_PLORT,
                    value = 100
                }
            });
        }
    }

    static class MSS
    {
        public static void DoTheThing()
        {
            ModSecretStyle.onSecretStylesInitialization += () =>
            {
                var ss = new ModSecretStyle(Ids.BLANK_SLIME, new Vector3(195.5f, 14.8f, -332), Quaternion.Euler(-15, 180, 0), "cellRanch_Lab", "Clear");
                ss.SecretStyle.Structures[0] = new SlimeAppearanceStructure(ss.SecretStyle.Structures[0]);
                ss.SecretStyle.Structures[0].DefaultMaterials[0] = Resources.FindObjectsOfTypeAll<Material>().First((x) => x.name == "Depth Water Ball").Clone();
                ss.SecretStyle.Structures[0].DefaultMaterials[0].SetVector("_ColorMultiply", new Vector4(10, 10, 10, 100));
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                ss.SecretStyle.Structures[0].DefaultMaterials[0].SetTexture("_ColorRamp", tex);
                ss.SecretStyle.NameXlateKey = "l.secret_style_" + Ids.BLANK_SLIME.ToString().ToLowerInvariant();
                ss.SecretStyle.Icon = Main.LoadImage("slimeBlankExotic.png").CreateSprite();
            };
            if (SRModLoader.IsModPresent("secretstylethings"))
                SST.DoTheThing();
        }
    }

    static class SST
    {
        public static void DoTheThing()
        {
            SlimeUtils.SecretStyleData.Add(Ids.BLANK_PLORT, new SecretStyleData(Main.LoadImage("plortBlankExotic.png").CreateSprite()));
        }
    }
}