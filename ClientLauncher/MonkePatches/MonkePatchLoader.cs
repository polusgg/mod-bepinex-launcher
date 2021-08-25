using HarmonyLib;

namespace ClientLauncher.MonkePatches
{
    public class MonkePatchLoader
    {
        private Harmony _harmony;

        public MonkePatchLoader()
        {
            _harmony = new Harmony("com.polusgg.clientlauncher");
        }

        public void LoadMonkePatches() => _harmony.PatchAll();
    }
}