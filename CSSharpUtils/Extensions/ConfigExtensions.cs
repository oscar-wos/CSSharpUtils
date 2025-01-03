﻿using System.Reflection;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace CSSharpUtils.Extensions;

/// <summary>
/// Provides extension methods for <see cref="BasePluginConfig"/>.
/// </summary>
public static class ConfigExtensions
{
    // Specifies the options for JSON serialization, including indentation for readability.
    private static readonly JsonSerializerOptions WriteSerializerOptions = new() { WriteIndented = true };
    
    // Specifies the options for JSON deserialization.
    private static readonly JsonSerializerOptions ReadSerializerOptions = new() { ReadCommentHandling = JsonCommentHandling.Skip };

    /// <summary>
    /// Updates the version of the provided configuration object and serializes it back to JSON.
    /// This method ensures that the configuration file reflects the most recent version,
    /// including all properties of the configuration object, even those not initially set.
    /// Also backs up the current configuration file before updating it.
    /// </summary>
    /// <typeparam name="T">The type of the configuration object, must inherit from BasePluginConfig.</typeparam>
    /// <param name="config">The configuration object to update and serialize.</param>
    /// <returns><c>true</c> if the config is updated; otherwise, <c>false</c>.</returns>
    public static bool Update<T>(this T config) where T : BasePluginConfig, new()
    {
        // get the name of the calling assembly
        var assemblyName = Assembly.GetCallingAssembly().GetName().Name ?? null;
        
        // check if the assembly name is null
        if (assemblyName == null)
            return false;

        var configPath = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/{assemblyName}/{assemblyName}";

        // get newest config version
        var newCfgVersion = new T().Version;

        // loaded config is up-to-date
        if (config.Version == newCfgVersion)
            return false;

        // get counter of backup file
        var backupCount = GetBackupCount(configPath);

        // create a backup of the current config
        File.Copy($"{configPath}.json", $"{configPath}-{backupCount}.bak", true);

        // update the version
        config.Version = newCfgVersion;

        // serialize the updated config back to json
        var updatedJsonContent = JsonSerializer.Serialize(config, WriteSerializerOptions);
        File.WriteAllText($"{configPath}.json", updatedJsonContent);
        return true;
    }

    /// <summary>
    /// Reloads the configuration from disk, deserializing it back into the configuration object.
    /// </summary>
    /// <typeparam name="T">The type of the configuration object, must inherit from BasePluginConfig.</typeparam>
    /// <returns>The reloaded configuration object.</returns>
    /// <remarks>
    /// You should pass the result of this method to your plugins OnConfigParsed() method.
    /// </remarks>
    public static T Reload<T>(this T config) where T : BasePluginConfig, new()
    {
        // get the name of the calling assembly
        var assemblyName = Assembly.GetCallingAssembly().GetName().Name ?? null;

        // check if the assembly name is null
        if (assemblyName == null)
            return new();

        var configPath = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/{assemblyName}/{assemblyName}";

        // read the configuration file content
        var configContent = File.ReadAllText($"{configPath}.json");

        // deserialize the configuration content back to the object
        return JsonSerializer.Deserialize<T>(configContent, ReadSerializerOptions)!;
    }

    private static int GetBackupCount(string configPath)
    {
        var counter = 0;

        while (File.Exists($"{configPath}-{counter}.bak"))
            counter++;

        return counter;
    }
}