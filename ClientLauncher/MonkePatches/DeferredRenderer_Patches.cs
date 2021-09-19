using System;
using System.Reflection;
using Avalonia.Rendering;
using HarmonyLib;

namespace ClientLauncher.MonkePatches
{
    [HarmonyPatch(typeof(DeferredRenderer), "Avalonia.Rendering.IRenderLoopTask.Render")]
    public static class DeferredRenderer_IRenderLoopTask_Patch
    {
        private static MethodInfo? _trueRenderMethod;
        public static void Prepare()
        {
            _trueRenderMethod = AccessTools.Method(AccessTools.TypeByName(nameof(Avalonia.Rendering.DeferredRenderer)), "Render", new []{ typeof(bool) });
        }

        public static bool Prefix(DeferredRenderer __instance)
        {
            if (_trueRenderMethod is not null)
            {
                _trueRenderMethod.Invoke(__instance, new object[] { true });
            }

            return _trueRenderMethod is null;
        }
    }
    
    [HarmonyPatch]
    public static class DeferredRenderer_Render_Patch
    {
        public static MethodBase TargetMethod() => AccessTools.Method(
            AccessTools.TypeByName(nameof(Avalonia.Rendering.DeferredRenderer)), "Render", new[] { typeof(bool) });
        public static Exception Finalizer(Exception __exception)
        {
            if (__exception.Message.Contains("Invalid create info - no Canvas provided"))
                return null;
            
            return __exception;
        }
    }
}