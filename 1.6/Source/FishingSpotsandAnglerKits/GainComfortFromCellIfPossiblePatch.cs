using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FishingSpotsandAnglerKits
{
    [HarmonyPatch(typeof(JobDriver_Fish), "MakeNewToils")]
    public static class Patch_JobDriver_Fish_AddComfort
    {
        [HarmonyPostfix]
        public static void AddComfortToilTick(ref IEnumerable<Toil> __result, JobDriver_Fish __instance)
        {
            __result = AddComfortToToils(__result, __instance.pawn);
        }

        private static IEnumerable<Toil> AddComfortToToils(IEnumerable<Toil> toils, Pawn pawn)
        {
            foreach (var toil in toils)
            {
                if (toil.defaultCompleteMode == ToilCompleteMode.Delay)
                {
                    if (toil.tickIntervalAction != null)
                    {
                        var oldTickIntervalAction = toil.tickIntervalAction;
                        toil.tickIntervalAction = (int delta) =>
                        {
                            oldTickIntervalAction(delta); // 调用原有逻辑，传递准确的 delta
                            pawn.GainComfortFromCellIfPossible(delta); // 使用准确的 delta
                        };
                    }
                    else if (toil.tickAction != null)
                    {
                        // 如果没有tickIntervalAction，退化到tickAction，传1作为间隔
                        var oldTickAction = toil.tickAction;
                        toil.tickAction = () =>
                        {
                            oldTickAction();
                            pawn.GainComfortFromCellIfPossible(1);
                        };
                    }
                }
                yield return toil;
            }
        }
    }
}
