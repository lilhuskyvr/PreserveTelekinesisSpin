using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using ThunderRoad;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming
// ReSharper disable PossibleNullReferenceException
// ReSharper disable UnusedType.Local
// ReSharper disable Unity.InefficientPropertyAccess
// ReSharper disable UnusedMember.Local

namespace PreserveTelekinesisSpin.Patch
{
    // ReSharper disable once UnusedType.Global
    public class PreserveTelekinesisSpinPatchLevelModule : LevelModule
    {
        private Harmony _harmony;

        public override IEnumerator OnLoadCoroutine(Level level)
        {
            try
            {
                _harmony = new Harmony("PreserveTelekinesisSpin");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());

                Debug.Log("Preserve Telekinesis Spin Loaded");
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }

            return base.OnLoadCoroutine(level);
        }

        [HarmonyPatch(typeof(SpellTelekinesis))]
        [HarmonyPatch("TryCatch")]
        private static class SpellTelekinesisTryCatchPatch
        {
            [HarmonyPostfix]
            private static void Postfix(SpellTelekinesis __instance)
            {
                if (__instance.catchedHandle == null)
                    return;
                var item = __instance.catchedHandle.item;

                if (__instance.catchedHandle.item != null)
                {
                    var customJointItem = item.gameObject.GetComponent<CustomJointItem>();
                    if (customJointItem != null)
                    {
                        Object.Destroy(customJointItem);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SpellTelekinesis))]
        [HarmonyPatch("TryRelease")]
        private static class SpellTelekinesisTryReleasePatch
        {
            private static Handle _catchedHandle;


            [HarmonyPrefix]
            private static void Prefix(bool tryThrow, SpellTelekinesis __instance)
            {
                if (__instance.catchedHandle == null)
                    return;
                var item = __instance.catchedHandle.item;

                if (item != null)
                {
                    var customJointItem = item.gameObject.GetComponent<CustomJointItem>();
                    if (customJointItem != null)
                    {
                        Object.Destroy(customJointItem);
                    }

                    BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                             | BindingFlags.Static;
                    FieldInfo field = __instance.GetType().GetField("joint", bindFlags);
                    ConfigurableJoint joint = field.GetValue(__instance) as ConfigurableJoint;

                    if (__instance.spinMode)
                    {
                        customJointItem = item.gameObject.AddComponent<CustomJointItem>();

                        customJointItem.joint = Object.Instantiate(joint);

                        _catchedHandle = __instance.catchedHandle;
                    }
                }
            }

            [HarmonyPostfix]
            private static void Postfix(bool tryThrow, SpellTelekinesis __instance)
            {
                if (_catchedHandle != null)
                {
                    foreach (CollisionHandler collisionHandler in _catchedHandle.item.collisionHandlers)
                        collisionHandler.SetPhysicModifier(_catchedHandle, 3, __instance.gravity ? 1f : 0.0f, 1f,
                            __instance.drag, __instance.angularDrag);
                    _catchedHandle.item.rb.sleepThreshold = 0.0f;
                    _catchedHandle.item.StopThrowing();
                    _catchedHandle.item.StopFlying();
                    _catchedHandle.item.ResetColliderCollision();
                    _catchedHandle.item.ResetRagdollCollision();
                    _catchedHandle.item.ResetObjectCollision();
                    _catchedHandle.item.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingObject));
                    _catchedHandle.item.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    _catchedHandle.item.SetCenterOfMass(_catchedHandle.transform.localPosition +
                                                        new Vector3(0.0f, _catchedHandle.GetDefaultAxisLocalPosition(),
                                                            0.0f));
                    _catchedHandle.item.isTelekinesisGrabbed = true;
                }
            }
        }
    }
}