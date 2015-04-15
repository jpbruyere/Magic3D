using System;
using OpenTK;

namespace Magic3D
{
	public interface IRenderable
	{		
		Matrix4 ModelMatrix { get; set; }
		void Render();
	}
}

