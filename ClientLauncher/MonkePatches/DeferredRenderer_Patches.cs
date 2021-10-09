using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
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
    
    // [HarmonyPatch(typeof(TopLevel), "HandlePaint")]
    // public static class TopLevel_HandlePaint_Patch
    // {
    //     public static bool Prefix(TopLevel __instance, [HarmonyArgument(0)] Rect rect)
    //     {
    //         try
    //         {
    //             __instance.Renderer?.Paint(rect);
    //         }
    //         catch (ArgumentException e)
    //         {
    //             Console.WriteLine($"Possible error creating a SkiaCanvas: {e.Message}\n{e.StackTrace}");
    //         }
    //
    //         return false;
    //     }
    // }
}