using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using HarmonyLib;
using StardewValley;

namespace FishBasket
{
    public class ModEntry : Mod
    {
        internal static IMonitor StaticMonitor { get; private set; } = null!;
        internal static ModConfig Config { get; private set; } = null!;

        private Dictionary<string, string>? _defaultTranslations;
        private Dictionary<string, string>? _localeTranslations;
        private string _lastLoadedLocale = "";

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();

            StaticMonitor = this.Monitor;
            var harmony = new Harmony(this.ModManifest.UniqueID);
            Patches.CrabPotPatch.Apply(harmony);
            Patches.ObjectPlacementPatch.Apply(harmony);
            Patches.ObjectCanBePlacedHerePatch.Apply(harmony);
            Patches.CraftingRecipePatch.Apply(harmony);
            Patches.CheckForActionPatch.Apply(harmony);
            Patches.CrabPotDrawPatch.Apply(harmony);
            Patches.ObjectDrawInMenuPatch.Apply(harmony);

            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            Monitor.Log("Fish Basket Mod geladen!", LogLevel.Info);
        }

        internal string T(string key)
        {
            string locale = Config.Locale;
            if (locale == "auto")
                locale = LocalizedContentManager.CurrentLanguageCode.ToString();

            if (_lastLoadedLocale != locale)
            {
                _defaultTranslations = Helper.Data.ReadJsonFile<Dictionary<string, string>>(Path.Combine("i18n", "default.json"));
                _localeTranslations = locale != "default"
                    ? Helper.Data.ReadJsonFile<Dictionary<string, string>>(Path.Combine("i18n", $"{locale}.json"))
                    : null;
                _lastLoadedLocale = locale;
            }

            if (_localeTranslations != null && _localeTranslations.TryGetValue(key, out var val))
                return val;

            if (_defaultTranslations != null && _defaultTranslations.TryGetValue(key, out var defaultVal))
                return defaultVal;

            return key;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            string[] locales = { "auto", "default", "de", "fr", "es", "pt", "zh", "ja", "ko", "it", "tr", "hu" };

            configMenu.AddTextOption(
                mod: ModManifest,
                getValue: () => Config.Locale,
                setValue: val => Config.Locale = val,
                name: () => T("config.locale.name"),
                tooltip: () => T("config.locale.tooltip"),
                allowedValues: locales,
                formatAllowedValue: val => T($"config.locale.{val}")
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.RequiresBait,
                setValue: val => Config.RequiresBait = val,
                name: () => T("config.requiresBait.name"),
                tooltip: () => T("config.requiresBait.tooltip")
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => T("config.junkMultipliers")
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.BasicJunkMultiplier,
                setValue: val => Config.BasicJunkMultiplier = val,
                name: () => T("config.basicJunk.name"),
                tooltip: () => T("config.junkMultiplier.tooltip"),
                min: 0f, max: 5f, interval: 0.05f
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.SilverJunkMultiplier,
                setValue: val => Config.SilverJunkMultiplier = val,
                name: () => T("config.silverJunk.name"),
                tooltip: () => T("config.junkMultiplier.tooltip"),
                min: 0f, max: 5f, interval: 0.05f
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.GoldJunkMultiplier,
                setValue: val => Config.GoldJunkMultiplier = val,
                name: () => T("config.goldJunk.name"),
                tooltip: () => T("config.junkMultiplier.tooltip"),
                min: 0f, max: 5f, interval: 0.05f
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.IridiumJunkMultiplier,
                setValue: val => Config.IridiumJunkMultiplier = val,
                name: () => T("config.iridiumJunk.name"),
                tooltip: () => T("config.junkMultiplier.tooltip"),
                min: 0f, max: 5f, interval: 0.05f
            );
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("StardewUncle.FishBasket.animations"))
            {
                e.LoadFromModFile<Texture2D>("Assets/fishbasket_animation_16x16.png", AssetLoadPriority.Medium);
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, StardewValley.GameData.Objects.ObjectData>().Data;

                    data["StardewUncle.FishBasket_Basic"] = CreateFishBasketData(
                        id: "StardewUncle.FishBasket_Basic",
                        price: 40);

                    data["StardewUncle.FishBasket_Silver"] = CreateFishBasketData(
                        id: "StardewUncle.FishBasket_Silver",
                        price: 80);

                    data["StardewUncle.FishBasket_Gold"] = CreateFishBasketData(
                        id: "StardewUncle.FishBasket_Gold",
                        price: 150);

                    data["StardewUncle.FishBasket_Iridium"] = CreateFishBasketData(
                        id: "StardewUncle.FishBasket_Iridium",
                        price: 300);
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;

                    data["StardewUncle.FishBasket_Basic"] =
                        "388 40 335 3/Home/StardewUncle.FishBasket_Basic 1/false/s Fishing 3/Fischkorb/";

                    data["StardewUncle.FishBasket_Silver"] =
                        "388 40 335 3/Home/StardewUncle.FishBasket_Silver 1/false/s Fishing 3/Silberner Fischkorb/";

                    data["StardewUncle.FishBasket_Gold"] =
                        "388 40 335 3/Home/StardewUncle.FishBasket_Gold 1/false/s Fishing 3/Goldener Fischkorb/";

                    data["StardewUncle.FishBasket_Iridium"] =
                        "388 40 335 3/Home/StardewUncle.FishBasket_Iridium 1/false/s Fishing 3/Iridium-Fischkorb/";
                });
            }

        }

        private StardewValley.GameData.Objects.ObjectData CreateFishBasketData(string id, int price)
        {
            string shortId = id.Replace("StardewUncle.", "");
            return new StardewValley.GameData.Objects.ObjectData
            {
                Name = id,
                DisplayName = T($"item.{shortId}.name"),
                Description = T($"item.{shortId}.description"),
                Type = "Crafting",
                Category = -8,
                Price = price,
                Texture = "Maps\\springobjects",
                SpriteIndex = 710
            };
        }
    }
}