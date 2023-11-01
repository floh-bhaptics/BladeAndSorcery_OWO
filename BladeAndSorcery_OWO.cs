using MelonLoader;
using MyOWOVest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using ThunderRoad;

namespace BladeAndSorcery_OWO
{
    public class BladeAndSorcery_OWO : MelonMod
    {
        public static TactsuitVR tactsuitVr;


        public override void OnInitializeMelon()
        {
            tactsuitVr = new TactsuitVR();

        }

        [HarmonyPatch(typeof(BhapticsHandler), "PlayHaptic", new Type[] { typeof(float), typeof(float), typeof(BhapticsHandler.FeedbackType), typeof(bool), typeof(float), typeof(BhapticsHandler.FeedbackType), typeof(bool), typeof(float) })]
        public class bhaptics_PlayBhapticsEffect
        {
            [HarmonyPostfix]
            public static void Postfix(BhapticsHandler __instance, float locationAngle, float locationHeight, BhapticsHandler.FeedbackType effect, bool reflected)
            {
                string pattern = effect.ToString();
                if (pattern.Contains("Right"))
                    if (reflected) pattern = pattern.Replace("Right", "Left");
                if ((locationAngle == 0f) && (locationHeight == 0f)) tactsuitVr.PlayBackFeedback(pattern);
                else tactsuitVr.PlayBackHit(pattern, locationAngle, locationHeight);
            }
        }


        /*
        [HarmonyPatch(typeof(SpellCaster), "Fire", new Type[] { })]
        public class bhaptics_CastFire
        {
            [HarmonyPostfix]
            public static void Postfix(SpellCaster __instance)
            {
                bool isRight = (__instance.side == Side.Right);
                bool twoHanded = (__instance.isMerging);
                tactsuitVr.CastSpell(isRight, twoHanded);
            }
        }
        */


        /*
         * TimeManager.SetSlowMotion
         * SpellMergeData.Merge
         * CameraEffects.RefreshHealth
            * Player.local.creature.state != Creature.State.Dead && !Player.local.creature.isKilled && Player.local.creature.currentHealth <= Player.local.creature.maxHealth * 0.1f && Player.local.creature.currentHealth > 0.01f
         * SpellCastCharge.OnCrystalSlam
         * SpellMergeFire.Explosion
         * Holder.Snap
         * Holder.UnSnap
            * if (__instance.drawSlot == Holder.DrawSlot.BackRight)
            * else if (__instance.drawSlot == Holder.DrawSlot.BackLeft)
         * SpellCastGravity.PushPlayer
         * 
        */

    }
}
