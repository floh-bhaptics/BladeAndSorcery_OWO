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

    }
}
