using System;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ScarletTeleports;

public static class ECSExtensions {
  public static EntityManager EntityManager => Core.Server.EntityManager;

  public static unsafe void Write<T>(this Entity entity, T componentData) where T : struct {
    var componentType = new ComponentType(Il2CppType.Of<T>());

    byte[] byteArray = StructureToByteArray(componentData);

    int size = Marshal.SizeOf<T>();

    fixed (byte* p = byteArray) {
      EntityManager.SetComponentDataRaw(entity, componentType.TypeIndex, p, size);
    }
  }

  public static byte[] StructureToByteArray<T>(T structure) where T : struct {
    int size = Marshal.SizeOf(structure);
    byte[] byteArray = new byte[size];
    IntPtr ptr = Marshal.AllocHGlobal(size);

    Marshal.StructureToPtr(structure, ptr, true);
    Marshal.Copy(ptr, byteArray, 0, size);
    Marshal.FreeHGlobal(ptr);

    return byteArray;
  }

  public static unsafe T Read<T>(this Entity entity) where T : struct {
    var componentType = new ComponentType(Il2CppType.Of<T>());

    void* rawPointer = EntityManager.GetComponentDataRawRO(entity, componentType.TypeIndex);

    T componentData = Marshal.PtrToStructure<T>(new IntPtr(rawPointer));

    return componentData;
  }

  public static DynamicBuffer<T> ReadBuffer<T>(this Entity entity) where T : struct {
    return EntityManager.GetBuffer<T>(entity);
  }

  public static bool Has<T>(this Entity entity) {
    var componentType = new ComponentType(Il2CppType.Of<T>());
    return EntityManager.HasComponent(entity, componentType);
  }

  public static void Add<T>(this Entity entity) {
    var componentType = new ComponentType(Il2CppType.Of<T>());
    EntityManager.AddComponent(entity, componentType);
  }

  public static void Remove<T>(this Entity entity) {
    var componentType = new ComponentType(Il2CppType.Of<T>());
    EntityManager.RemoveComponent(entity, componentType);
  }

  public static float3 GetPosition(this Entity entity) {
    if (!EntityManager.HasComponent<Translation>(entity)) return float3.zero;

    return EntityManager.GetComponentData<Translation>(entity).Value;
  }
}
