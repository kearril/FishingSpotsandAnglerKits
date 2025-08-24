using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FishingSpotsandAnglerKits
{
    [HarmonyPatch(typeof(FishingUtility), nameof(FishingUtility.GetCatchesFor))]
    public static class Patch_FishingRareMultiplier
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var chanceMethod = AccessTools.Method(typeof(Rand), nameof(Rand.Chance));

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                // 找到 Rand.Chance(0.01f) 的调用位置，插入 multiplier
                if (codes[i].opcode == System.Reflection.Emit.OpCodes.Ldc_R4 && (float)codes[i].operand == 0.01f)
                {
                    // 加载 pawn (第一个参数)
                    yield return new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0);
                    // 加载 FishingRareMultiplier stat
                    yield return CodeInstruction.Call(typeof(Patch_FishingRareMultiplier), nameof(GetMultiplier));
                    // 乘以 multiplier
                    yield return new CodeInstruction(System.Reflection.Emit.OpCodes.Mul);
                }
            }
        }

        public static float GetMultiplier(Pawn pawn)
        {
            return pawn.GetStatValue(StatDef.Named("FishingRareMultiplier"), true);
        }
    }
}
