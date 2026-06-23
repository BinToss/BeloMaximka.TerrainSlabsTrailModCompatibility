using System;
using System.Collections.Generic;
using HarmonyLib;
using TrailModUpdated;
using Vintagestory.API.Common;

namespace TerrainSlabsTrailModCompatibility;

[HarmonyPatch]
public static class TrailModPatch
{
    const string modPrefix = "terrainslabstrailmodcompatibility";

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TrailChunkManager), "CreateTrailBlockTransform")]
    public static void AddSlabVariants(
        TrailChunkManager __instance,
        Dictionary<int, TrailBlockTouchTransformData> ___trailBlockTouchTransforms,
        int blockID,
        int transformOnTouchCount,
        int transformBlockID,
        bool transformByPlayerOnly
    )
    {
        if (__instance.worldAccessor == null) return;

        IWorldAccessor world = __instance.worldAccessor;
        AssetLocation newBlockAsset = new(__instance.worldAccessor.Blocks[blockID].Code);
        int pos = newBlockAsset.Path.IndexOf('-');
        if (pos < 0)
        {
            world.Logger.Warning("[{0}] Unable to get index of '-' in {1}", modPrefix, newBlockAsset.Path);
            return;
        }
        newBlockAsset.Path = $"{newBlockAsset.Path.AsSpan(0, pos)}-{newBlockAsset.Domain}{newBlockAsset.Path.AsSpan(pos)}";
        newBlockAsset.Domain = "terrainslabs";

        AssetLocation transformBlockAsset = new(__instance.worldAccessor.Blocks[transformBlockID].Code);
        pos = transformBlockAsset.Path.IndexOf('-');
        if (pos < 0)
        {
            world.Logger.Warning("[{0}] Unable to get index of '-' in {1}", modPrefix, transformBlockAsset.Path);
            return;
        }
        transformBlockAsset.Path =
            $"{transformBlockAsset.Path.AsSpan(0, pos)}-{transformBlockAsset.Domain}{transformBlockAsset.Path.AsSpan(pos)}";
        transformBlockAsset.Domain = "terrainslabs";

        Block? block = world.GetBlock(newBlockAsset);
        if (block is null)
        {
            world.Logger.Warning("[{0}] Unable to get block with code {1}", modPrefix, newBlockAsset);
            return;
        }
        Block? transformBlock = world.GetBlock(transformBlockAsset);
        if (transformBlock is null)
        {
            world.Logger.Warning("[{0}] Unable to get transform block with code {1}", modPrefix, transformBlockAsset);
            return;
        }

        int newBlockID = block.BlockId;
        int newTransformBlockID = transformBlock.BlockId;

        TrailBlockTouchTransformData touchTransformData = new(
            newBlockAsset,
            transformOnTouchCount,
            newTransformBlockID,
            transformByPlayerOnly
        );
        ___trailBlockTouchTransforms.Add(newBlockID, touchTransformData);
    }
}
