using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using Pewter.ModBlockEntity;

namespace Pewter.ModBlock
{
    public class BlockCandlelabra : BlockGroundAndSideAttachable, ITexPositionSource
    {
        public Size2i AtlasSize { get; set; }

        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                if (textureCode == "material")
                {
                    return this.tmpTextureSource[this.curMat];
                }
                return this.tmpTextureSource[textureCode];
            }
        }

        internal void initRotations()
        {
            for (int i = 0; i < 17; i++)
            {
                Matrixf j = new Matrixf();
                j.Translate(0.5f, 0.5f, 0.5f);
                j.RotateYDeg((float)(i * 22.5));
                j.Translate(-0.5f, -0.5f, -0.5f);
                Vec3f[] poses = this.candleWickPositionsByRot[i] = new Vec3f[this.candleWickPositions.Length];
                for (int k = 0; k < poses.Length; k++)
                {
                    Vec4f rotated = j.TransformVector(new Vec4f(this.candleWickPositions[k].X / 16f, this.candleWickPositions[k].Y / 16f, this.candleWickPositions[k].Z / 16f, 1f));
                    poses[k] = new Vec3f(rotated.X, rotated.Y, rotated.Z);
                }
            }
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            this.initRotations();
        }

        public override bool ShouldReceiveClientParticleTicks(IWorldAccessor world, IPlayer player, BlockPos pos, out bool isWindAffected)
        {
            isWindAffected = true;
            return true;
        }

        public override string GetHeldTpIdleAnimation(ItemSlot activeHotbarSlot, Entity forEntity, EnumHand hand)
        {
            EntityPlayer entityPlayer = forEntity as EntityPlayer;
            IPlayer player = (entityPlayer != null) ? entityPlayer.Player : null;
            if (forEntity.AnimManager.IsAnimationActive(new string[]
            {
                "sleep",
                "wave",
                "cheer",
                "shrug",
                "cry",
                "nod",
                "facepalm",
                "bow",
                "laugh",
                "rage",
                "scythe",
                "bowaim",
                "bowhit",
                "spearidle"
            }))
            {
                return null;
            }
            bool flag;
            if (player == null)
            {
                flag = (null != null);
            }
            else
            {
                IPlayerInventoryManager inventoryManager = player.InventoryManager;
                flag = (((inventoryManager != null) ? inventoryManager.ActiveHotbarSlot : null) != null);
            }
            if (flag && !player.InventoryManager.ActiveHotbarSlot.Empty && hand == EnumHand.Left)
            {
                ItemStack stack = player.InventoryManager.ActiveHotbarSlot.Itemstack;
                if (stack != null)
                {
                    CollectibleObject collectible = stack.Collectible;
                    if (((collectible != null) ? collectible.GetHeldTpIdleAnimation(player.InventoryManager.ActiveHotbarSlot, forEntity, EnumHand.Right) : null) != null)
                    {
                        return null;
                    }
                }
                if (player != null)
                {
                    EntityPlayer entity = player.Entity;
                    if (((entity != null) ? new bool?(entity.Controls.LeftMouseDown) : null).GetValueOrDefault() && stack != null)
                    {
                        CollectibleObject collectible2 = stack.Collectible;
                        if (((collectible2 != null) ? collectible2.GetHeldTpHitAnimation(player.InventoryManager.ActiveHotbarSlot, forEntity) : null) != null)
                        {
                            return null;
                        }
                    }
                }
            }
            if (hand != EnumHand.Left)
            {
                return "holdinglanternrighthand";
            }
            return "holdinglanternlefthand";
        }

        public override byte[] GetLightHsv(IBlockAccessor blockAccessor, BlockPos pos, ItemStack stack = null)
        {
            return base.GetLightHsv(blockAccessor, pos, stack);
        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            Dictionary<string, MultiTextureMeshRef> meshrefs = ObjectCacheUtil.GetOrCreate<Dictionary<string, MultiTextureMeshRef>>(capi, "blockCandlelabraGuiMeshRefs", () => new Dictionary<string, MultiTextureMeshRef>());
            string material = itemstack.Attributes.GetString("material", null);
            string key = string.Concat(new string[]
            {
                material
            });
            MultiTextureMeshRef meshref;
            if (!meshrefs.TryGetValue(key, out meshref))
            {
                AssetLocation shapeloc = this.Shape.Base.CopyWithPathPrefixAndAppendixOnce("shapes/", ".json");
                Shape shape = Vintagestory.API.Common.Shape.TryGet(capi, shapeloc);
                MeshData mesh = this.GenMesh(capi, material, shape, null);
                meshref = (meshrefs[key] = capi.Render.UploadMultiTextureMesh(mesh));
            }
            renderinfo.ModelRef = meshref;
            renderinfo.CullFaces = false;
        }

        public override void OnUnloaded(ICoreAPI api)
        {
            ICoreClientAPI capi = api as ICoreClientAPI;
            if (capi == null)
            {
                return;
            }
            object obj;
            if (capi.ObjectCache.TryGetValue("blockCandlelabraGuiMeshRefs", out obj))
            {
                foreach (KeyValuePair<string, MultiTextureMeshRef> val in (obj as Dictionary<string, MultiTextureMeshRef>))
                {
                    val.Value.Dispose();
                }
                capi.ObjectCache.Remove("blockCandlelabraGuiMeshRefs");
            }
        }

        public MeshData GenMesh(ICoreClientAPI capi, string material, Shape shape = null, ITesselatorAPI tesselator = null)
        {
            if (tesselator == null)
            {
                tesselator = capi.Tesselator;
            }
            this.tmpTextureSource = tesselator.GetTextureSource(this, 0, false);
            if (shape == null)
            {
                shape = Vintagestory.API.Common.Shape.TryGet(capi, this.Code.Domain + ":shapes/" + this.Shape.Base.Path + ".json");
            }
            if (shape == null)
            {
                return null;
            }
            this.AtlasSize = capi.BlockTextureAtlas.Size;
            this.curMat = material;
            MeshData mesh;
            tesselator.TesselateShape("blockCandlelabra", shape, out mesh, this, new Vec3f(this.Shape.rotateX, this.Shape.rotateY, this.Shape.rotateZ), 0, 0, 0, null, null);
            return mesh;
        }
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode))
            {
                BlockEntityCandlelabra be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityCandlelabra;
                if (be != null)
                {
                    string material = itemstack.Attributes.GetString("material", null);
                    be.DidPlace(material);
                    BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
                    double y = byPlayer.Entity.Pos.X - ((double)targetPos.X + blockSel.HitPosition.X);
                    double dz = byPlayer.Entity.Pos.Z - ((double)targetPos.Z + blockSel.HitPosition.Z);
                    double num = (double)((float)Math.Atan2(y, dz));
                    float deg22dot5rad = 0.3926991f;
                    float roundRad = (float)((int)Math.Round(num / (double)deg22dot5rad)) * deg22dot5rad;
                    be.MeshAngle = roundRad;
                    return true;
                }
            }

            return false;
        }

        public override void OnAsyncClientParticleTick(IAsyncParticleManager manager, BlockPos pos, float windAffectednessAtPos, float secondsTicking)
        {
            if (this.ParticleProperties != null && this.ParticleProperties.Length != 0)
            {
                string orientation = this.Variant["orientation"];
                if (orientation == "up")
                {
                    float meshAngle = 0.0f;
                    BlockEntityCandlelabra be = manager.BlockAccess.GetBlockEntity(pos) as BlockEntityCandlelabra;
                    if (be != null)
                    {
                        meshAngle = be.MeshAngle;
                    }
                    int rotIdx = (int)Math.Round((meshAngle + Math.PI) / 0.3926991f);
                    Vec3f[] poses = this.candleWickPositionsByRot[rotIdx];
                    for (int i = 0; i < this.ParticleProperties.Length; i++)
                    {
                        AdvancedParticleProperties bps = this.ParticleProperties[i];
                        bps.WindAffectednesAtPos = windAffectednessAtPos;
                        for (int j = 0; j < 3; j++)
                        {
                            Vec3f dp = poses[j];
                            bps.basePos.X = (double)((float)pos.X + dp.X);
                            bps.basePos.Y = (double)((float)pos.Y + dp.Y);
                            bps.basePos.Z = (double)((float)pos.Z + dp.Z);
                            manager.Spawn(bps);
                        }
                    }
                }
                else 
                {
                    // Fun with particles
                    float xOff = 0f;
                    float yOff = 0.1f;
                    float zOff = 0f;
                    int rot = 0;
                    switch (BlockFacing.FromCode(orientation).HorizontalAngleIndex)
                    {
                        case 0:
                            rot = 4;
                            xOff -= 0.25f;
                            break;
                        case 1:
                            zOff += 0.25f;
                            break;
                        case 2:
                            rot = 4;
                            xOff += 0.25f;
                            break;
                        case 3:
                            zOff -= 0.25f;
                            break;
                        default:
                            break;
                    }
                    for (int i = 0; i < this.ParticleProperties.Length; i++)
                    {
                        AdvancedParticleProperties bps = this.ParticleProperties[i];
                        bps.WindAffectednesAtPos = windAffectednessAtPos;
                        Vec3f[] poses = this.candleWickPositionsByRot[rot];
                        for (int j = 0; j < 3; j++)
                        {
                            Vec3f dp = poses[j];
                            bps.basePos.X = (double)((float)pos.X + dp.X + xOff);
                            bps.basePos.Y = (double)((float)pos.Y + dp.Y + yOff);
                            bps.basePos.Z = (double)((float)pos.Z + dp.Z + zOff);
                            manager.Spawn(bps);
                        }
                    }
                }  
            }
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            ItemStack stack = new ItemStack(world.GetBlock(base.CodeWithParts("up")), 1);
            BlockEntityCandlelabra be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityCandlelabra;
            if (be != null)
            {
                stack.Attributes.SetString("material", be.material);
            }
            else
            {
                stack.Attributes.SetString("material", "pewter");
            }
            return stack;
        }

        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            return new BlockDropItemStack[]
            {
                new BlockDropItemStack(handbookStack, 1f)
            };
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            bool preventDefault = false;
            foreach (BlockBehavior blockBehavior in this.BlockBehaviors)
            {
                EnumHandling handled = EnumHandling.PassThrough;
                blockBehavior.OnBlockBroken(world, pos, byPlayer, ref handled);
                if (handled == EnumHandling.PreventDefault)
                {
                    preventDefault = true;
                }
                if (handled == EnumHandling.PreventSubsequent)
                {
                    return;
                }
            }
            if (preventDefault)
            {
                return;
            }
            if (world.Side == EnumAppSide.Server && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative))
            {
                ItemStack[] drops = new ItemStack[]
                {
                    this.OnPickBlock(world, pos)
                };
                if (drops != null)
                {
                    for (int i = 0; i < drops.Length; i++)
                    {
                        world.SpawnItemEntity(drops[i], pos, null);
                    }
                }
                world.PlaySoundAt(this.Sounds.GetBreakSound(byPlayer), pos, -0.5, byPlayer, true, 32f, 1f);
            }
            if (this.EntityClass != null)
            {
                BlockEntity entity = world.BlockAccessor.GetBlockEntity(pos);
                if (entity != null)
                {
                    entity.OnBlockBroken(null);
                }
            }
            world.BlockAccessor.SetBlock(0, pos);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            return (!byPlayer.Entity.Controls.ShiftKey && (world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityCandlelabra).Interact(byPlayer)) || base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override string GetHeldItemName(ItemStack itemStack)
        {
            string material = itemStack.Attributes.GetString("material", null);
            string[] array = new string[5];
            int num = 0;
            AssetLocation code = this.Code;
            array[num] = ((code != null) ? code.Domain : null);
            array[1] = ":block-";
            int num2 = 2;
            AssetLocation code2 = this.Code;
            array[num2] = ((code2 != null) ? code2.Path : null);
            array[3] = "-";
            array[4] = material;
            return Lang.GetMatching(string.Concat(array), Array.Empty<object>());
        }

        public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing, int rndIndex = -1)
        {
            BlockEntityCandlelabra be = capi.World.BlockAccessor.GetBlockEntity(pos) as BlockEntityCandlelabra;
            if (be != null)
            {
                CompositeTexture tex = null;
                if (this.Textures.TryGetValue(be.material, out tex))
                {
                    return capi.BlockTextureAtlas.GetRandomColor(tex.Baked.TextureSubId, rndIndex);
                }
            }
            return base.GetRandomColor(capi, pos, facing, rndIndex);
        }

        public override List<ItemStack> GetHandBookStacks(ICoreClientAPI capi)
        {
            if (this.Code == null)
            {
                return null;
            }
            bool inCreativeTab = this.CreativeInventoryTabs != null && this.CreativeInventoryTabs.Length != 0;
            bool inCreativeTabStack = this.CreativeInventoryStacks != null && this.CreativeInventoryStacks.Length != 0;
            JsonObject attributes = this.Attributes;
            bool flag;
            if (attributes == null)
            {
                flag = false;
            }
            else
            {
                JsonObject jsonObject = attributes["handbook"];
                flag = ((jsonObject != null) ? new bool?(jsonObject["include"].AsBool(false)) : null).GetValueOrDefault();
            }
            bool explicitlyIncluded = flag;
            JsonObject attributes2 = this.Attributes;
            bool flag2;
            if (attributes2 == null)
            {
                flag2 = false;
            }
            else
            {
                JsonObject jsonObject2 = attributes2["handbook"];
                flag2 = ((jsonObject2 != null) ? new bool?(jsonObject2["exclude"].AsBool(false)) : null).GetValueOrDefault();
            }
            if (flag2)
            {
                return null;
            }
            if (!explicitlyIncluded && !inCreativeTab && !inCreativeTabStack)
            {
                return null;
            }
            List<ItemStack> stacks = new List<ItemStack>();
            if (inCreativeTabStack)
            {
                for (int i = 0; i < this.CreativeInventoryStacks.Length; i++)
                {
                    for (int j = 0; j < this.CreativeInventoryStacks[i].Stacks.Length; j++)
                    {
                        ItemStack stack = this.CreativeInventoryStacks[i].Stacks[j].ResolvedItemstack;
                        stack.ResolveBlockOrItem(capi.World);
                        stack = stack.Clone();
                        stack.StackSize = stack.Collectible.MaxStackSize;
                        if (!stacks.Any((ItemStack stack1) => stack1.Equals(stack)))
                        {
                            stacks.Add(stack);
                        }
                    }
                }
            }
            else
            {
                stacks.Add(new ItemStack(this, 1));
            }
            return stacks;
        }

        protected Vec3f[] candleWickPositions = new Vec3f[]
        {
            new Vec3f(2.5f, 8f, 8f),
            new Vec3f(8f, 11f, 8f),
            new Vec3f(13.5f, 8f, 8f),
        };

        protected Vec3f[][] candleWickPositionsByRot = new Vec3f[17][];

        private string curMat;

        private ITexPositionSource tmpTextureSource;
    }
}
