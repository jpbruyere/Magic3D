using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Magic3D
{
	public class ActivatedAbility : Ability
	{
		//public Cost ActivationCost;
		public virtual void Activate (CardInstance _source, List<object> _targets)
		{
			foreach (Effect e in Effects) {
				e.Apply (_source, _targets);
			}
		}
	}
}
