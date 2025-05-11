using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ScarletTeleports.Services;

public static class Database {
  private static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, "ScarletTeleports");

  public static void Initialize() {
    if (!Directory.Exists(CONFIG_PATH)) {
      Directory.CreateDirectory(CONFIG_PATH);
    }
  }

  public static void Save<T>(string path, T data) {
    string filePath = Path.Combine(CONFIG_PATH, $"{path}.json");

    try {
      Directory.CreateDirectory(Path.GetDirectoryName(filePath));

      string jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(filePath, jsonData);
    } catch (Exception ex) {
      Core.Log.LogError($"An error occurred while saving data: {ex.Message}");
    }
  }

  public static T Load<T>(string path) {
    string filePath = Path.Combine(CONFIG_PATH, $"{path}.json");

    if (!File.Exists(filePath)) {
      return default;
    }

    try {
      string jsonData = File.ReadAllText(filePath);
      var deserializedData = JsonSerializer.Deserialize<T>(jsonData);

      return deserializedData;
    } catch (Exception ex) {
      Core.Log.LogError($"An error occurred while loading data: {ex.Message}");
    }

    return default;
  }

  public static List<string> ListAvailablePaths() {
    if (!Directory.Exists(CONFIG_PATH)) return new();
    var files = Directory.GetFiles(CONFIG_PATH, "*.json");
    var paths = new List<string>();
    foreach (var file in files) {
      paths.Add(Path.GetFileNameWithoutExtension(file));
    }
    return paths;
  }
}