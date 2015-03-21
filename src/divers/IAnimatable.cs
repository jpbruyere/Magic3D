using System;

namespace Magic3D
{
	public interface IAnimatable
	{
		event EventHandler<EventArgs> AnimationFinished;

		//float EllapseTime { get; set; }
		void Animate(float ellapseTime = 0f);
	}
}

