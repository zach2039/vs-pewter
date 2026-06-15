using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            // Override default shape location used in crock, instead of copying/overriding methods like we used to do for metal crocks
            Type typeBlockCrock = typeof(BlockCrock); 
            FieldInfo fieldInfoshapeLocation = typeBlockCrock.GetField("shapeLocation", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfoshapeLocation != null)
            {
                fieldInfoshapeLocation.SetValue(this, "pewter:shapes/block/metal/crock/");
            }
            else
            {
                throw new ArgumentNullException("Could not override shapeLocation for Pewter's BlockCrockMetal!");
            }
        }
    }
}
