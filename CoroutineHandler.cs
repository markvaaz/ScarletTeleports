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

  public static Coroutine StartGenericCoroutine(Action action, float delay) {
    return CoroutineManager().StartCoroutine(GenericCoroutine(action, delay).WrapToIl2Cpp());
  }

  private static IEnumerator GenericCoroutine(Action action, float delay) {
    yield return new WaitForSeconds(delay);
    action?.Invoke();
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

  public static Coroutine StartFrameCoroutine(Action action, int frameInterval) {
    return CoroutineManager().StartCoroutine(FrameCoroutine(action, frameInterval).WrapToIl2Cpp());
  }

  private static IEnumerator FrameCoroutine(Action action, int frameInterval) {
    while (true) {
      for (int i = 0; i < frameInterval; i++) {
        yield return null;
      }

      action?.Invoke();
    }
  }

  public static Coroutine StartFrameCoroutine(Action action, int frameInterval, int repeatCount) {
    return CoroutineManager().StartCoroutine(FrameCoroutine(action, frameInterval, repeatCount).WrapToIl2Cpp());
  }

  private static IEnumerator FrameCoroutine(Action action, int frameInterval, int repeatCount) {
    for (int j = 0; j < repeatCount; j++) {
      for (int i = 0; i < frameInterval; i++) {
        yield return null;
      }

      action?.Invoke();
    }
  }

  public static Coroutine StartRandomIntervalCoroutine(Action action, float minDelay, float maxDelay) {
    return CoroutineManager().StartCoroutine(RandomIntervalCoroutine(action, minDelay, maxDelay).WrapToIl2Cpp());
  }

  private static IEnumerator RandomIntervalCoroutine(Action action, float minDelay, float maxDelay) {
    while (true) {
      float seconds = UnityEngine.Random.Range(minDelay, maxDelay);
      yield return new WaitForSeconds(seconds);
      action?.Invoke();
    }
  }

  public static Coroutine StartRandomIntervalCoroutine(Action action, float minDelay, float maxDelay, int repeatCount) {
    return CoroutineManager().StartCoroutine(RandomIntervalCoroutine(action, minDelay, maxDelay, repeatCount).WrapToIl2Cpp());
  }

  private static IEnumerator RandomIntervalCoroutine(Action action, float minDelay, float maxDelay, int repeatCount) {
    for (int i = 0; i < repeatCount; i++) {
      float seconds = UnityEngine.Random.Range(minDelay, maxDelay);
      yield return new WaitForSeconds(seconds);
      action?.Invoke();
    }
  }
}