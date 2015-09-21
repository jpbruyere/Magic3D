using System;
using OpenTK;

namespace Magic3D
{
	public class Library : CardGroup
	{
		public Library()
			: base(CardGroupEnum.Library,true)
		{
			x = -6;
			y = -1.0f;
			xAngle = MathHelper.Pi;
			VerticalSpacing = 0.01f;
		}
	}
}

