using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace FishBasket.Patches
{
    internal static class ObjectPlacementPatch
    {
        internal const string ModDataTierKey = "StardewUncle.FishBasket/Tier";

        internal static readonly (string ItemId, string Tier)[] FishBasketIds =
        {
            ("StardewUncle.FishBasket_Basic", "Basic"),
            ("StardewUncle.FishBasket_Silver", "Silver"),
            ("StardewUncle.FishBasket_Gold", "Gold"),
            ("StardewUncle.FishBasket_Iridium", "Iridium"),
        };

        internal static string? GetTierForItemId(string itemId)
        {
            foreach (var entry in FishBasketIds)
                if (entry.ItemId == itemId)
                    return entry.Tier;
            return null;
        }

        internal static string? GetItemIdForTier(string tier)
        {
            foreach (var entry in FishBasketIds)
                if (entry.Tier == tier)
                    return entry.ItemId;
            return null;
        }

        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), "placementAction"),
                prefix: new HarmonyMethod(typeof(ObjectPlacementPatch), nameof(Prefix))
            );
        }

        private static bool Prefix(SObject __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            string? tier = null;
            foreach (var entry in FishBasketIds)
            {
                if (__instance.ItemId == entry.ItemId)
                {
                    tier = entry.Tier;
                    break;
                }
            }

            if (tier == null)
                return true;

            Vector2 placementTile = new Vector2(x / 64, y / 64);
            if (!CrabPot.IsValidCrabPotLocationTile(location, (int)placementTile.X, (int)placementTile.Y))
            {
                __result = false;
                return false;
            }

            var pot = new CrabPot { ItemId = "710" };
            pot.modData[ModDataTierKey] = tier;
            pot.placementAction(location, x, y, who);

            __result = true;
            return false;
        }
    }
}