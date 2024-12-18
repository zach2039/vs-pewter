using Pewter.ModBlock;
using Pewter.ModBlockEntity;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Pewter
{
    public class PewterModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        { 
            api.RegisterBlockClass("BlockCrockMetal", typeof(BlockCrockMetal));
            api.RegisterBlockClass("BlockCandlelabra", typeof(BlockCandlelabra));
            api.RegisterBlockEntityClass("BlockEntityCrockMetal", typeof(BlockEntityCrockMetal));
            api.RegisterBlockEntityClass("BlockEntityCandlelabra", typeof(BlockEntityCandlelabra));

            api.Logger.Notification("Loaded Pewter!");
        }
    }
}
