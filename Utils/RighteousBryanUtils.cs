using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;

namespace BetterCamera.Utils
{
	/// <summary>
	/// Old ass funkin Utilities from the dead dead legacy 1.3 mods
	/// </summary>
	public static class RighteousBryanUtils 
	{
		/// <summary>
		/// Get Direction from a position
		/// </summary>
		public static Vector2 DirectionFrom(this Vector2 From,Vector2 Source){
			return Vector2.Normalize(From - Source);
		}

		/// <summary>
		/// Round Vector2 to an intreger , used for pixel perfect stuff
		/// </summary>
		/// <param name="pos"> the position </param>
		/// <returns></returns>
		public static Vector2 RoundToInt(this Vector2 pos){
			return new Vector2((int)pos.X,(int)pos.Y);
		}
	}
}