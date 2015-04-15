using System;
using System.Collections.Generic;

namespace Magic3D
{
	public class MagicStackLayout : Layout3d
	{
		public List<MagicAction> Actions = new List<MagicAction> ();

		#region implemented abstract members of Layout3d
		public override void Render ()
		{
//			foreach (MagicAction ma in Actions) {
//				ma.					
//			}
		}
		public override void UpdateLayout ()
		{
			
		}
		public override void toogleShowAll ()
		{
			
		}
		#endregion
	}
}

