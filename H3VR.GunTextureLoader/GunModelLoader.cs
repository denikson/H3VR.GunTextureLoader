using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using XUnity.ResourceRedirector;

namespace H3VR.GunModelLoader
{
    [BepInPlugin("horse.coder.h3vr.gunModelloader", "Gun Model loader", "1.0")]
    [BepInDependency("gravydevsupreme.xunity.resourceredirector")]
    public class GunModelLoader : BaseUnityPlugin
    {
        private readonly Dictionary<string, Mesh> MeshCache =
            new Dictionary<string, Mesh>(StringComparer.InvariantCultureIgnoreCase);

        private readonly Dictionary<string, string> MeshPaths =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        private ConfigEntry<string> modelStorePath;
        private FileSystemWatcher watcher;

        private void Awake()
        {
            modelStorePath = Config.Bind("Paths", "ModelsStore", "GunModels", "Path where Models are stored.");
            modelStorePath.SettingChanged += (sender, args) => InitWatcher();
            InitWatcher();
            //var meshesDir = Path.Combine(Paths.GameRootPath, modelStorePath.Value);
            ResourceRedirection.RegisterAssetLoadedHook(HookBehaviour.OneCallbackPerResourceLoaded, ReplaceModels);


        }

        private void LoadModelPaths()
        {
            var modPath = Path.Combine(Paths.GameRootPath, modelStorePath.Value);
            Logger.LogInfo($"Loading models from {modPath}");
            MeshPaths.Clear();
            Directory.CreateDirectory(modPath);

            foreach (var file in Directory.GetFiles(modPath, "*.assetbundlemsh", SearchOption.AllDirectories))
                MeshPaths[Path.GetFileNameWithoutExtension(file)] = file;
            Logger.LogInfo($"Found {MeshPaths.Count} models!");
        }

        private void InitWatcher()
        {
            if (watcher != null)
            {
                watcher.Dispose();
                watcher = null;
            }

            LoadModelPaths();
            watcher = new FileSystemWatcher(modelStorePath.Value, "*.assetbundlemsh") {IncludeSubdirectories = true};
            watcher.Changed += (sender, args) =>
            {
                Logger.LogInfo("Files in Model path changed, reloading!");
                LoadModelPaths();
            };
        }

        private void ReplaceModels(AssetLoadedContext obj)
        {
            foreach (var objAsset in obj.Assets)
                if (objAsset is GameObject go)
                {
                    var mrs = go.GetComponentsInChildren<MeshRenderer>();
                    foreach (var mesh in mrs)
                        {
                            var MeshName = mesh.name;
                            Logger.LogDebug($"Trying to load mesh {MeshName}");
                            if (!MeshPaths.TryGetValue(MeshName, out var path)) continue;
                            Logger.LogDebug($"Loading {path}");
                            if (!MeshCache.TryGetValue(MeshName, out var Mesh))
                            {
                            Mesh = MeshCache[MeshName] = new Mesh();
                            Mesh.LoadMesh(File.ReadAllBytes(path));
                        }

                        }
                }
        }
    }
}