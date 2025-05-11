// Reference: part of this code was adapted from BloodyCore
// Original repository: https://github.com/oscarpedrero/BloodyCore/blob/master/BloodyCore/API/v1/CoroutineHandler.cs
// Credits to the original author: github.com/oscarpedrero

using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ProjectM.Physics;
using UnityEngine;

namespace ScarletTeleports;

public class CoroutineHandler {
  private static readonly IgnorePhysicsDebugSystem coroutineManager = new GameObject("CoroutineScarletMods").AddComponent<IgnorePhysicsDebugSystem>();

  private static IgnorePhysicsDebugSystem CoroutineManager() {
    if (coroutineManager == null) {
      throw new InvalidOperationException("CoroutineManager not initialized.");
    }

    return coroutineManager;
  }

  public static Coroutine StartRepeatingCoroutine(Action action, float delay) {
    return CoroutineManager().StartCoroutine(RepeatingCoroutine(action, delay).WrapToIl2Cpp());
  }

  private static IEnumerator RepeatingCoroutine(Action action, float delay) {
    while (true) {
      yield return new WaitForSeconds(delay);
      action?.Invoke();
    }
  }

  public static Coroutine StartRepeatingCoroutine(Action action, float delay, int repeatCount) {
    return CoroutineManager().StartCoroutine(RepeatingCoroutine(action, delay, repeatCount).WrapToIl2Cpp());
  }

  private static IEnumerator RepeatingCoroutine(Action action, float delay, int repeatCount) {
    for (int i = 0; i < repeatCount; i++) {
      yield return new WaitForSeconds(delay);
      action?.Invoke();
    }
  }
}