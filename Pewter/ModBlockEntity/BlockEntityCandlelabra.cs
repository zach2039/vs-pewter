using Pewter.ModBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Pewter.ModBlockEntity
{
    public class BlockEntityCandlelabra : BlockEntity
    {
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            this.lightHsv = base.Block.LightHsv;
        }

        public void DidPlace(string material)
        {
            this.material = material;
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
            this.Api.World.BlockAccessor.RemoveBlockLight(this.lightHsv, this.Pos);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            this.material = tree.GetString("material", "pewter");
            this.MeshAngle = tree.GetFloat("meshAngle", 0f);
            if (this.Api != null && this.Api.Side == EnumAppSide.Client)
            {
                this.MarkDirty(true, null);
            }
        }

        public byte[] GetLightHsv()
        {
            return this.lightHsv;
        }

        private MeshData getMesh(ITesselatorAPI tesselator)
        {
            Dictionary<string, MeshData> lanternMeshes = ObjectCacheUtil.GetOrCreate<Dictionary<string, MeshData>>(this.Api, "blockCandlelabraBlockMeshes", () => new Dictionary<string, MeshData>());
            MeshData mesh = null;
            BlockCandlelabra block = this.Api.World.BlockAccessor.GetBlock(this.Pos) as BlockCandlelabra;
            if (block == null)
            {
                return null;
            }
            string orient = block.LastCodePart(0);
            if (lanternMeshes.TryGetValue(string.Concat(new string[]
            {
                this.material,
                "-",
                orient,
            }), out mesh))
            {
                return mesh;
            }
            return lanternMeshes[string.Concat(new string[]
            {
                this.material,
                "-",
                orient,
            })] = block.GenMesh(this.Api as ICoreClientAPI, this.material, null, tesselator);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetString("material", this.material);
            tree.SetFloat("meshAngle", this.MeshAngle);
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            MeshData mesh = this.getMesh(tesselator);
            if (mesh == null)
            {
                return false;
            }
            string part = base.Block.LastCodePart(0);
            if (part == "up")
            {
                mesh = mesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0f, this.MeshAngle, 0f);
            }
            mesher.AddMeshData(mesh, 1);
            return true;
        }

        internal bool Interact(IPlayer byPlayer)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Empty)
            {
                return false;
            }
            CollectibleObject obj = slot.Itemstack.Collectible;
            return false;
        }

        public string material = "pewter";

        public float MeshAngle;

        private byte[] lightHsv = new byte[]
        {
            7,
            7,
            14
        };
    }
}
