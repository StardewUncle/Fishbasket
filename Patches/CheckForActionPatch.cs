using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace FishBasket.Patches
{
    internal static class CheckForActionPatch
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(CrabPot), "checkForAction", new[] { typeof(Farmer), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(CheckForActionPatch), nameof(Prefix))
            );
        }

        private static bool Prefix(CrabPot __instance, Farmer who, bool justCheckingForActivity)
        {
            if (!__instance.modData.TryGetValue(ObjectPlacementPatch.ModDataTierKey, out string? tier))
                return true;

            if (justCheckingForActivity)
                return true;

            if (__instance.tileIndexToShow == 714 && __instance.heldObject.Value != null && __instance.readyForHarvest.Value)
            {
                SObject item = __instance.heldObject.Value;
                int numToCatch = 1;
                item.Stack = numToCatch;
                __instance.heldObject.Value = null;

                if (who.IsLocalPlayer && !who.addItemToInventoryBool(item))
                {
                    __instance.heldObject.Value = item;
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                    return false;
                }

                who.caughtFish(item.QualifiedItemId, 1, from_fish_pond: false, numToCatch);
                who.gainExperience(1, 5);
                __instance.readyForHarvest.Value = false;
                __instance.tileIndexToShow = 710;
                __instance.lidFlapping = true;
                __instance.lidFlapTimer = 60f;
                __instance.bait.Value = null;
                who.animateOnce(279 + who.FacingDirection);
                __instance.Location?.playSound("fishingRodBend");
                DelayedAction.playSoundAfterDelay("coin", 500);
                __instance.shake = Vector2.Zero;
                __instance.shakeTimer = 0f;
                return false;
            }

            if (!Game1.didPlayerJustClickAtAll(ignoreNonMouseHeldInput: true))
                return false;

            string? basketItemId = ObjectPlacementPatch.GetItemIdForTier(tier);
            if (basketItemId == null)
                return true;

            SObject basketItem = ItemRegistry.Create<SObject>(basketItemId);
            if (who.addItemToInventoryBool(basketItem))
            {
                if (who.isMoving())
                    Game1.haltAfterCheck = false;
                Game1.playSound("coin");
                __instance.Location?.objects.Remove(__instance.TileLocation);
                return false;
            }

            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
            return false;
        }
    }
}
