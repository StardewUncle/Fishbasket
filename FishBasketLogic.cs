using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace FishBasket
{
    internal static class FishBasketLogic
    {
        private static readonly MethodInfo? NeedsBaitMethod =
            typeof(CrabPot).GetMethod("NeedsBait", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly MethodInfo? GetFishFromLocationDataMethod =
            typeof(GameLocation).GetMethod("GetFishFromLocationData", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly Dictionary<string, (int[] Qualities, double[] Weights)> TierSettings =
            new()
            {
                ["Basic"] = (new[] { 0 }, new[] { 1.0 }),
                ["Silver"] = (new[] { 0, 1 }, new[] { 0.8, 0.2 }),
                ["Gold"] = (new[] { 0, 1, 2 }, new[] { 0.65, 0.25, 0.10 }),
                ["Iridium"] = (new[] { 0, 1, 2, 4 }, new[] { 0.55, 0.25, 0.15, 0.05 }),
            };

        internal static void RunCatchLogic(CrabPot pot, GameLocation location, string tier)
        {
            IMonitor monitor = ModEntry.StaticMonitor;

            Farmer ownedByPlayer = Game1.GetPlayer(pot.owner.Value) ?? Game1.MasterPlayer;

            bool needsBait = ModEntry.Config.RequiresBait
                && NeedsBaitMethod != null
                && (bool)NeedsBaitMethod.Invoke(pot, new object[] { ownedByPlayer })!;

            monitor?.Log($"[FishBasket] RunCatchLogic gestartet. Tier={tier}, needsBait={needsBait}, GetFishFromLocationDataMethod gefunden={GetFishFromLocationDataMethod != null}", LogLevel.Info);

            if (needsBait || pot.heldObject.Value != null)
                return;

            if (!TierSettings.TryGetValue(tier, out var settings))
                return;

            pot.readyForHarvest.Value = true;
            pot.tileIndexToShow = 714;

            Random r = Utility.CreateDaySaveRandom(
                pot.TileLocation.X * 1000f,
                pot.TileLocation.Y * 255f,
                pot.directionOffset.X * 1000f + pot.directionOffset.Y);

            if (!location.TryGetFishAreaForTile(pot.TileLocation, out _, out var fishArea))
                fishArea = null;

            double baseJunkChance = fishArea?.CrabPotJunkChance ?? 0.2;

            double junkMultiplier = tier switch
            {
                "Basic" => ModEntry.Config.BasicJunkMultiplier,
                "Silver" => ModEntry.Config.SilverJunkMultiplier,
                "Gold" => ModEntry.Config.GoldJunkMultiplier,
                "Iridium" => ModEntry.Config.IridiumJunkMultiplier,
                _ => 1.0
            };

            double chanceForJunk = baseJunkChance * junkMultiplier;

            if (pot.bait.Value?.QualifiedItemId == "(O)DeluxeBait" || pot.bait.Value?.QualifiedItemId == "(O)774")
                chanceForJunk /= 2.0;

            bool junkRolled = r.NextBool(chanceForJunk);
            monitor?.Log($"[FishBasket] chanceForJunk={chanceForJunk}, junkRolled={junkRolled}", LogLevel.Info);

            SObject? caughtFish = null;

            if (!junkRolled && GetFishFromLocationDataMethod != null)
            {
                const int assumedWaterDepth = 3;

                try
                {
                    object? result = GetFishFromLocationDataMethod.Invoke(null, new object?[]
                    {
                        location.Name,
                        pot.TileLocation,
                        assumedWaterDepth,
                        ownedByPlayer,
                        false,
                        true,
                        location,
                        null
                    });

                    caughtFish = result as SObject;
                    monitor?.Log($"[FishBasket] GetFishFromLocationData Ergebnis: {(caughtFish != null ? caughtFish.QualifiedItemId : "null")}", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    monitor?.Log($"[FishBasket] Fehler beim Aufruf von GetFishFromLocationData: {ex}", LogLevel.Error);
                }
            }

            if (caughtFish != null)
            {
                int quality = RollQuality(r, settings.Qualities, settings.Weights);
                caughtFish.Quality = quality;
                pot.heldObject.Value = caughtFish;
                monitor?.Log($"[FishBasket] Fisch gefangen: ItemId={caughtFish.ItemId}, Quality={quality}, Qualities verfügbar=[{string.Join(",", settings.Qualities)}], Weights=[{string.Join(",", settings.Weights)}]", LogLevel.Info);
            }
            else
            {
                pot.heldObject.Value = ItemRegistry.Create<SObject>("(O)" + r.Next(168, 173));
                pot.heldObject.Value.Quality = RollQuality(r, settings.Qualities, settings.Weights);
                monitor?.Log($"[FishBasket] Kein Fisch -> Müll mit Qualität {pot.heldObject.Value.Quality} vergeben.", LogLevel.Info);
            }
        }

        private static int RollQuality(Random r, int[] allowedQualities, double[] weights)
        {
            double roll = r.NextDouble();
            double cumulative = 0.0;
            for (int i = 0; i < allowedQualities.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                    return allowedQualities[i];
            }
            return allowedQualities[allowedQualities.Length - 1];
        }
    }
}