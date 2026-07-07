using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;

namespace FishBasket.Patches
{
    internal static class CrabPotDrawPatch
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(CrabPot), "draw", new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                postfix: new HarmonyMethod(typeof(CrabPotDrawPatch), nameof(Postfix))
            );
        }

        private static Point QualityStar(string? tier)
        {
            return tier switch
            {
                "Silver" => new Point(338, 400),
                "Gold" => new Point(346, 400),
                "Iridium" => new Point(346, 392),
                _ => new Point(0, 0),
            };
        }

        private static void Postfix(CrabPot __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!__instance.modData.TryGetValue(ObjectPlacementPatch.ModDataTierKey, out string? tier))
                return;

            float depth = (y * 64 + 64) / 10000f + 0.002f;

            // Widerhaken (item 691) from Maps/springobjects
            try
            {
                int idx = 691;
                Texture2D objects = Game1.content.Load<Texture2D>("Maps/springobjects");
                Rectangle hookRect = new(idx % 24 * 16, idx / 24 * 16, 16, 16);
                Vector2 hookPos = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 48, y * 64 + 12));
                spriteBatch.Draw(objects, hookPos, hookRect, Color.White * alpha, 0f, Vector2.Zero, 2f, SpriteEffects.None, depth);
            }
            catch { }

            // Quality star centered (origin half-size to keep alignment)
            Point pt = QualityStar(tier);
            if (pt.X > 0)
            {
                Vector2 starPos = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 4));
                spriteBatch.Draw(Game1.mouseCursors, starPos, new Rectangle(pt.X, pt.Y, 8, 8), Color.White * alpha, 0f, new Vector2(4f, 4f), 4f, SpriteEffects.None, depth + 0.001f);
            }
        }
    }
}
