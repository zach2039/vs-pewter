using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Pewter.ModBlock
{
    public class BlockCrockMetal : BlockCrock, IContainedMeshSource
    {
        private AssetLocation getMostCommonMealIngredient(ItemStack[] contents)
        {
            Dictionary<AssetLocation, int> dictionary = new Dictionary<AssetLocation, int>();
            foreach (ItemStack itemStack in contents)
            {
                dictionary.TryGetValue(itemStack.Collectible.Code, out var value);
                dictionary[itemStack.Collectible.Code] = 1 + value;
            }

            AssetLocation key = dictionary.Aggregate((KeyValuePair<AssetLocation, int> l, KeyValuePair<AssetLocation, int> r) => (l.Value <= r.Value) ? r : l).Key;
            if (dictionary[key] < 3)
            {
                return null;
            }

            return key;
        }

        public new AssetLocation LabelForContents(string recipeCode, ItemStack[] contents)
        {
            string text;
            if (recipeCode != null && recipeCode.Length > 0)
            {
                AssetLocation mostCommonMealIngredient = getMostCommonMealIngredient(contents);
                if (mostCommonMealIngredient != null && (text = CodeToLabel(mostCommonMealIngredient)) != null)
                {
                    return AssetLocation.Create("shapes/block/metal/crock/label-" + text + ".json", Code.Domain);
                }

                return AssetLocation.Create("shapes/block/metal/crock/label-meal.json", Code.Domain);
            }

            if (contents == null || contents.Length == 0 || contents[0] == null)
            {
                return AssetLocation.Create("shapes/block/metal/crock/label-empty.json", Code.Domain);
            }

            if (MealMeshCache.ContentsRotten(contents))
            {
                return AssetLocation.Create("shapes/block/metal/crock/label-rot.json", Code.Domain);
            }

            text = CodeToLabel(contents[0].Collectible.Code) ?? "empty";
            return AssetLocation.Create("shapes/block/metal/crock/label-" + text + ".json", Code.Domain);
        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            ItemStack[] nonEmptyContents = GetNonEmptyContents(capi.World, itemstack);
            string @string = itemstack.Attributes.GetString("recipeCode");
            AssetLocation assetLocation = LabelForContents(@string, nonEmptyContents);
            if (!(assetLocation == null))
            {
                Dictionary<string, MultiTextureMeshRef> orCreate = ObjectCacheUtil.GetOrCreate(capi, "blockcrockGuiMeshRefs", () => new Dictionary<string, MultiTextureMeshRef>());
                string key = Code.ToShortString() + assetLocation.ToShortString();
                if (!orCreate.TryGetValue(key, out var value))
                {
                    MeshData data = GenMesh(capi, assetLocation, new Vec3f(0f, 270f, 0f));
                    value = (orCreate[key] = capi.Render.UploadMultiTextureMesh(data));
                }

                renderinfo.ModelRef = value;
            }
        }

        public override string GetMeshCacheKey(ItemStack itemstack)
        {
            ItemStack[] nonEmptyContents = GetNonEmptyContents(api.World, itemstack);
            string @string = itemstack.Attributes.GetString("recipeCode");
            AssetLocation assetLocation = LabelForContents(@string, nonEmptyContents);
            return Code.ToShortString() + assetLocation.ToShortString();
        }

        public new MeshData GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas, BlockPos forBlockPos = null)
        {
            ItemStack[] nonEmptyContents = GetNonEmptyContents(api.World, itemstack);
            string @string = itemstack.Attributes.GetString("recipeCode");
            return GenMesh(api as ICoreClientAPI, LabelForContents(@string, nonEmptyContents));
        }

        public new MeshData GenMesh(ICoreClientAPI capi, AssetLocation labelLoc, Vec3f rot = null)
        {
            ITesselatorAPI tesselator = capi.Tesselator;
            Shape shape = Vintagestory.API.Common.Shape.TryGet(capi, AssetLocation.Create("shapes/block/metal/crock/base.json", Code.Domain));
            Shape shape2 = Vintagestory.API.Common.Shape.TryGet(capi, labelLoc);
            tesselator.TesselateShape(this, shape, out var modeldata, rot);
            tesselator.TesselateShape(this, shape2, out var modeldata2, rot);
            modeldata.AddMeshData(modeldata2);
            return modeldata;
        }
    }
}
