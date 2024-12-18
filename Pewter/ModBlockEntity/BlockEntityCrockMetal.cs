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
    public class BlockEntityCrockMetal : BlockEntityCrock
    {
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            this.ownBlock = (base.Block as BlockCrockMetal);
            this.inventory.OnAcquireTransitionSpeed += new CustomGetTransitionSpeedMulDelegate(this.Inv_OnAcquireTransitionSpeed);
        }

        private float Inv_OnAcquireTransitionSpeed(EnumTransitionType transType, ItemStack stack, float mulByConfig)
        {
            BlockCrockMetal blockCrockMetal = this.ownBlock;
            return mulByConfig * ((blockCrockMetal != null) ? blockCrockMetal.GetContainingTransitionModifierPlaced(this.Api.World, this.Pos, transType) : 1f);
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (this.currentMesh == null)
            {
                this.currentMesh = this.getMesh(tesselator);
                if (this.currentMesh == null)
                {
                    return false;
                }
            }
            mesher.AddMeshData(this.currentMesh, 1);
            return true;
        }

        private MeshData getMesh(ITesselatorAPI tesselator)
        {
            BlockCrockMetal blockCrockMetal = Api.World.BlockAccessor.GetBlock(Pos) as BlockCrockMetal;
            if (blockCrockMetal == null)
            {
                return null;
            }

            ItemStack[] stacks = (from slot in inventory
                                  where !slot.Empty
                                  select slot.Itemstack).ToArray();
            Vec3f rot = new Vec3f(0f, blockCrockMetal.Shape.rotateY, 0f);
            return GetMesh(tesselator, Api, blockCrockMetal, stacks, RecipeCode, rot);
        }

        public static MeshData GetMesh(ITesselatorAPI tesselator, ICoreAPI api, BlockCrockMetal block, ItemStack[] stacks, string recipeCode, Vec3f rot)
        {
            Dictionary<string, MeshData> orCreate = ObjectCacheUtil.GetOrCreate(api, "blockCrockMeshes", () => new Dictionary<string, MeshData>());
            MeshData value = null;
            AssetLocation assetLocation = block.LabelForContents(recipeCode, stacks);
            if (assetLocation == null)
            {
                return null;
            }

            string key = assetLocation.ToShortString() + block.Code.ToShortString() + "/" + rot.Y + "/" + rot.X + "/" + rot.Z;
            if (orCreate.TryGetValue(key, out value))
            {
                return value;
            }

            return orCreate[key] = block.GenMesh(api as ICoreClientAPI, assetLocation, rot);
        }

        private MeshData currentMesh;

        private BlockCrockMetal ownBlock;
    }
}
