using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace FishBasket.Patches
{
    internal static class CrabPotPatch
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(CrabPot), "DayUpdate"),
                prefix: new HarmonyMethod(typeof(CrabPotPatch), nameof(Prefix))
            );
        }

        private static bool Prefix(CrabPot __instance)
        {
            bool hasTier = __instance.modData.TryGetValue(ObjectPlacementPatch.ModDataTierKey, out string? tier);

            ModEntry.StaticMonitor?.Log($"[FishBasket] CrabPotPatch.Prefix aufgerufen. hasTier={hasTier}, tier={tier ?? "null"}, ItemId des Korbs={__instance.ItemId}", LogLevel.Info);

            if (!hasTier || tier == null)
                return true;

            GameLocation location = __instance.Location;
            FishBasketLogic.RunCatchLogic(__instance, location, tier);
            return false;
        }
    }
}