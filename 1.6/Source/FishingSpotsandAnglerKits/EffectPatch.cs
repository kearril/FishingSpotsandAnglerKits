using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.AI;
using Verse;

namespace FishingSpotsandAnglerKits
{
    [HarmonyPatch(typeof(JobDriver_Fish), "MakeNewToils")]
    public static class Patch_FishingEffect_HediffBased
    {
        static bool Prefix(JobDriver_Fish __instance, ref IEnumerable<Toil> __result)
        {
            Pawn pawn = __instance.pawn;
            Job job = __instance.job;

            var toils = new List<Toil>();

            // Fail condition 保留原版
            __instance.FailOn(() =>
                !(job.GetTarget(TargetIndex.A).Cell.GetZone(pawn.Map) is Zone_Fishing zone_Fishing)
                || !zone_Fishing.Allowed);

            // Goto
            Toil goTo = Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            toils.Add(goTo);

            // 钓鱼等待 Toil
            int ticks = Mathf.RoundToInt(7500f / pawn.GetStatValue(StatDefOf.FishingSpeed));
            Toil toil = Toils_General.WaitWith(TargetIndex.A, ticks, useProgressBar: false, maintainPosture: true);

            // 判断 Hediff
            EffecterDef effectToUse = pawn.health.hediffSet.HasHediff(HediffDef.Named("SeasFavor"))
                ? DefDatabase<EffecterDef>.GetNamed("GoldenFishing")
                : EffecterDefOf.Fishing;

            // 使用特效
            toil.WithEffect(effectToUse, () => job.GetTarget(TargetIndex.A));
            toil.WithProgressBarToilDelay(TargetIndex.B);

            // 加技能增长
            toil.tickAction = () =>
            {
                pawn.skills?.Learn(SkillDefOf.Animals, 0.025f);
            };

            toils.Add(toil);

            // 完成 Toil
            toils.Add(AccessTools.Method(typeof(JobDriver_Fish), "CompleteFishingToil")
                .Invoke(__instance, null) as Toil);

            __result = toils;
            return false; // 完全替换原流程
        }
    }
}
