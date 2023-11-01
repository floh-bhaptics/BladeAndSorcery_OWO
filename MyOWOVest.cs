using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MelonLoader;
using OWOGame;
using System.Net;

namespace MyOWOVest
{
    public class TactsuitVR
    {
        /* A class that contains the basic functions for the bhaptics Tactsuit, like:
         * - A Heartbeat function that can be turned on/off
         * - A function to read in and register all .tact patterns in the bHaptics subfolder
         * - A logging hook to output to the Melonloader log
         * - 
         * */
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        // Event to start and stop the heartbeat thread
        public Dictionary<String, Sensation> FeedbackMap = new Dictionary<String, Sensation>();


        /*
        //public static ISensation Explosion => new Sensation(100, 1f, 80, 100f, 500f, 0f);
        public static Sensation Explosion = Sensation.Create(100, 1f, 80, 100f, 500f, 0f);
        public static ISensation ExplosionBelly = Sensation.CreateWithMuscles(Explosion, Muscle.Lumbar_L, Muscle.Lumbar_R, Muscle.Abdominal_L, Muscle.Abdominal_R);
        //public static OWOSensationWithMuscles ExplosionBelly = new OWOSensationWithMuscles(Explosion, OWOMuscle.Abdominal_Left, OWOMuscle.Abdominal_Right, OWOMuscle.Lumbar_Left, OWOMuscle.Lumbar_Right);

        public static Sensation Healing = Sensation.Create(70, 0.5f, 65, 300f, 200f, 0f);
        public static ISensation HealingBody = Sensation.CreateWithMuscles(Healing, Muscle.AllMuscles);

        
        public static Sensation Reload1 = Sensation.Create(100, 0.3f, 50, 100f, 100f, 0f);
        public static Sensation Reload2 = Sensation.Create(100, 0.2f, 40, 0f, 100f, 0f);
        public static ISensation Reloading = Reload1.ContinueWith(Reload2);
        */

        public TactsuitVR()
        {
            RegisterAllTactFiles();
            InitializeOWO();
        }

        private async void InitializeOWO()
        {
            LOG("Initializing suit");

            // New auth.
            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("0");

            OWO.Configure(gameAuth);
            string myIP = getIpFromFile("OWO_Manual_IP.txt");
            if (myIP == "") await OWO.AutoConnect();
            else
            {
                LOG("Found manual IP address: " + myIP);
                await OWO.Connect(myIP);
            }

            if (OWO.ConnectionState == ConnectionState.Connected)
            {
                suitDisabled = false;
                LOG("OWO suit connected.");
            }
            if (suitDisabled) LOG("Owo is not enabled?!?!");
        }

        public string getIpFromFile(string filename)
        {
            string ip = "";
            string filePath = Directory.GetCurrentDirectory() + "\\Mods\\" + filename;
            if (File.Exists(filePath))
            {
                string fileBuffer = File.ReadAllText(filePath);
                IPAddress address;
                if (IPAddress.TryParse(fileBuffer, out address)) ip = fileBuffer;
            }
            return ip;
        }

        private BakedSensation[] AllBakedSensations()
        {
            var result = new List<BakedSensation>();

            foreach (var sensation in FeedbackMap.Values)
            {
                if (sensation is BakedSensation baked)
                {
                    LOG("Registered baked sensation: " + baked.name);
                    result.Add(baked);
                }
                else
                {
                    LOG("Sensation not baked? " + sensation);
                    continue;
                }
            }
            return result.ToArray();
        }


        ~TactsuitVR()
        {
            LOG("Destructor called");
            DisconnectOwo();
        }

        public void DisconnectOwo()
        {
            LOG("Disconnecting Owo skin.");
            OWO.Disconnect();
        }

        public void LOG(string logStr)
        {
#pragma warning disable CS0618 // remove warning that the logger is deprecated
            MelonLogger.Msg(logStr);
#pragma warning restore CS0618
        }

        void RegisterAllTactFiles()
        {

            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\OWO";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.owo", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                // LOG("Trying to register: " + prefix + " " + fullName);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    Sensation test = Sensation.Parse(tactFileStr);
                    FeedbackMap.Add(prefix, test);
                }
                catch (Exception e) { LOG(e.ToString()); }

            }

            systemInitialized = true;
        }


        public void PlayBackHit(string effect, float xzAngle, float yShift)
        {
            string pattern = effect;
            // two parameters can be given to the pattern to move it on the vest:
            // 1. An angle in degrees [0, 360] to turn the pattern to the left
            // 2. A shift [-0.5, 0.5] in y-direction (up and down) to move it up or down
            if ((xzAngle < 90f))
            {
                pattern += "_LF";
            }
            if ((xzAngle > 90f) && (xzAngle < 180f))
            {
                pattern += "_LB";
            }
            if ((xzAngle > 180f) && (xzAngle < 270f))
            {
                pattern += "_RB";
            }
            if ((xzAngle > 270f))
            {
                pattern += "_RF";
            }

            PlayBackFeedback(pattern);
        }

        public void Recoil(bool isRightHand, bool isTwoHanded = false)
        {
            if (isTwoHanded)
            {
                PlayBackFeedback("Melee_both");
                //OWO.Send(Sensation.GunRecoil, Muscle.Arm_R.WithIntensity(70), Muscle.Arm_L.WithIntensity(70));
                return;
            }
            if (isRightHand) PlayBackFeedback("Melee_R");
            else PlayBackFeedback("Melee_L");
        }

        public void CastSpell(bool isRightHand, bool isTwoHanded = false)
        {
            if (isTwoHanded)
            {
                PlayBackFeedback("CastSpell_both");
                //OWO.Send(Sensation.GunRecoil, Muscle.Arm_R.WithIntensity(70), Muscle.Arm_L.WithIntensity(70));
                return;
            }
            if (isRightHand) PlayBackFeedback("CastSpell_R");
            else PlayBackFeedback("CastSpell_L");
        }

        public void PlayBackFeedback(string feedback, float intensity = 1.0f)
        {
            LOG("Feedback: " + feedback);
            return;
            if (FeedbackMap.ContainsKey(feedback))
            {
                OWO.Send(FeedbackMap[feedback]);
            }
            else LOG("Feedback not registered: " + feedback);
        }

    }
}
