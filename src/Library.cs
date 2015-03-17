using System;

namespace Magic3D
{
	public class Library : CardGroup
	{
		public Library()
			: base(CardGroups.Library)
		{
			x = -6;
			y = -1.0f;            
			VerticalSpacing = 0.01f;
		}
	}
}

