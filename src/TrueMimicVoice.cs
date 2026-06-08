using System;
using System.Reflection;
using HarmonyLib;
using MelonLoader;

namespace TrueMimicVoice
{
    public class TrueMimicVoiceMod : MelonMod
    {
        private const string HarmonyId = "com.mimicvoicefix.fixed";

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("TrueMimicVoice loaded.");

            try
            {
                Type voiceEffecterType =
                    Type.GetType("Mimic.Actors.ProtoActor+VoiceEffecter, Assembly-CSharp");

                if (voiceEffecterType == null)
                {
                    MelonLogger.Error("ProtoActor+VoiceEffecter not found.");
                    return;
                }

                MethodInfo applyEffectPreset =
                    voiceEffecterType.GetMethod(
                        "ApplyEffectPreset",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                if (applyEffectPreset == null)
                {
                    MelonLogger.Error("ApplyEffectPreset not found.");
                    return;
                }

                HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(HarmonyId);

                MethodInfo postfix =
                    typeof(TrueMimicVoiceMod).GetMethod(
                        nameof(ApplyEffectPresetPostfix),
                        BindingFlags.Static | BindingFlags.NonPublic);

                harmony.Patch(
                        applyEffectPreset,
                        postfix: new HarmonyLib.HarmonyMethod(postfix));

                MelonLogger.Msg("ApplyEffectPreset patched.");
                }
                catch (Exception ex)
                {
                MelonLogger.Error("Init error: " + ex);
                }
             }

        private static void ApplyEffectPresetPostfix(object inType, object inEffectData)
        {
              try
              {
        if (inType == null || inEffectData == null)
            return;

        if (inType.ToString() == "Transmitter")
            return;

        object preset = GetMemberValue(inEffectData, "Preset");

        if (preset == null)
            return;

        DisableFiltersOnObject(preset);
    }
    catch (Exception ex)
    {
        MelonLogger.Error("Patch error: " + ex);
    }
}

        private static object GetMemberValue(object target, string name)
        {
            Type type = target.GetType();

            PropertyInfo prop =
                type.GetProperty(
                    name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            if (prop != null)
                return prop.GetValue(target, null);

            FieldInfo field =
                type.GetField(
                    name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            if (field != null)
                return field.GetValue(target);

            return null;
        }

        private static void DisableFiltersOnObject(object target)
        {
            if (target == null)
                return;

            Type type = target.GetType();

            DisableFilter(target, type, "LowPass");
            DisableFilter(target, type, "HighPass");
            DisableFilter(target, type, "Distortion");
            DisableFilter(target, type, "Chorus");
            DisableFilter(target, type, "Reverb");
            DisableFilter(target, type, "Echo");
            DisableFilter(target, type, "Amplifier");
        }

        private static void DisableFilter(
            object filterRef,
            Type filterType,
            string propertyName)
        {
            try
            {
                PropertyInfo property =
                    filterType.GetProperty(propertyName);

                if (property == null)
                    return;

                object value =
                    property.GetValue(filterRef, null);

                if (value == null)
                    return;

                PropertyInfo enabledProperty =
                    value.GetType().GetProperty("enabled");

                if (enabledProperty != null)
                {
                    enabledProperty.SetValue(
                        value,
                        false,
                        null);
                }
            }
            catch
            {
            }
        }
    }
}