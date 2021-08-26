using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.X11;
using HarmonyLib;

namespace ClientLauncher.MonkePatches
{
    [HarmonyPatch]
    public static class Xi2Manager_AddWindow_Patch
    { 
        public static MethodBase TargetMethod()
        {
            return typeof(X11PlatformOptions).Assembly.GetTypes()
                .First(x => x.FullName == "Avalonia.X11.XI2Manager")
                .GetMethod("AddWindow")!;
        }

        public static bool Prefix(ref XEventMask __result)
        {
            __result = XEventMask.NoEventMask;
            return false;
        }
    }
}