using System;
using OpenTK;

namespace Magic3D
{
	public class Library : CardGroup
	{
		public Library()
			: base(CardGroupEnum.Library)
		{
			x = -6;
			y = -1.0f;
			xAngle = MathHelper.Pi;
			VerticalSpacing = 0.02f;
		}
	}
}

