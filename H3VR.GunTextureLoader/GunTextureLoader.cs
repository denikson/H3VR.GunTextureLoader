using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using XUnity.ResourceRedirector;

namespace H3VR.GunTextureLoader
{
    [BepInPlugin("horse.coder.h3vr.guntextureloader", "Gun texture loader", "1.0")]
    [BepInDependency("gravydevsupreme.xunity.resourceredirector")]
    public class GunTextureLoader : BaseUnityPlugin
    {
        private readonly Dictionary<string, Texture2D> TexCache =
            new Dictionary<string, Texture2D>(StringComparer.InvariantCultureIgnoreCase);

        private readonly Dictionary<string, string> TexPaths =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        private ConfigEntry<string> textureStorePath;
        private FileSystemWatcher watcher;

        private void Awake()
        {
            textureStorePath = Config.Bind("Paths", "TexturesStore", "GunSkins", "Path where textures are stored.");
            textureStorePath.SettingChanged += (sender, args) => InitWatcher();
            InitWatcher();

            ResourceRedirection.RegisterAssetLoadedHook(HookBehaviour.OneCallbackPerResourceLoaded, ReplaceTextures);
            // ResourceRedirection.RegisterAssetLoadedHook(HookBehaviour.OneCallbackPerResourceLoaded, context =>
            // {
            //     Logger.LogInfo(
            //         $"Asset '{context.Parameters.Name}' LoadType: {context.Parameters.LoadType} Type: {context.Parameters.Type} Loaded items: {context.Assets.Length}, first item type: {context.Asset?.GetType()}");
            //
            //     if (context.Asset != null && context.Asset is GameObject go)
            //     {
            //         Logger.LogInfo($"Got asset {go}");
            //         var mrs = go.GetComponentsInChildren<MeshRenderer>();
            //         foreach (var meshRenderer in mrs)
            //             Logger.LogInfo($"  - {meshRenderer} => {meshRenderer.material.mainTexture}");
            //     }
            // });
        }

        private void LoadTexturePaths()
        {
            var texPath = Path.Combine(Paths.GameRootPath, textureStorePath.Value);
            Logger.LogInfo($"Loading textures from {texPath}");
            TexPaths.Clear();
            Directory.CreateDirectory(texPath);

            foreach (var file in Directory.GetFiles(texPath, "*.png", SearchOption.AllDirectories))
                TexPaths[Path.GetFileNameWithoutExtension(file)] = file;
            Logger.LogInfo($"Found {TexPaths.Count} textures!");
        }

        private void InitWatcher()
        {
            if (watcher != null)
            {
                watcher.Dispose();
                watcher = null;
            }

            LoadTexturePaths();
            watcher = new FileSystemWatcher(textureStorePath.Value, "*.png") {IncludeSubdirectories = true};
            watcher.Changed += (sender, args) =>
            {
                Logger.LogInfo("Files in texture path changed, reloading!");
                LoadTexturePaths();
            };
        }

        private void ReplaceTextures(AssetLoadedContext obj)
        {
            foreach (var objAsset in obj.Assets)
                if (objAsset is GameObject go)
                {
                    var mrs = go.GetComponentsInChildren<MeshRenderer>();
                    foreach (var meshRenderer in mrs)
                        if (meshRenderer.material && meshRenderer.material.mainTexture)
                        {
                            var texName = meshRenderer.material.mainTexture.name;
                            Logger.LogDebug($"Trying to load tex {texName}");
                            if (!TexPaths.TryGetValue(texName, out var path)) continue;
                            Logger.LogDebug($"Loading {path}");
                            if (!TexCache.TryGetValue(texName, out var tex))
                            {
                                tex = TexCache[texName] = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                                tex.LoadImage(File.ReadAllBytes(path));
                            }

                            meshRenderer.material.mainTexture = tex;
                        }
                }
        }
    }
}