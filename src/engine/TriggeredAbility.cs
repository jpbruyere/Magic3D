using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Magic3D
{
	public class TriggeredAbility : ActivatedAbility
	{
		public Trigger Trigger;

		public TriggeredAbility (Trigger _trig)
		{
			Trigger = _trig;
		}
		public override string ToString ()
		{
			return string.Format ("Triggered Ability: {0}", Trigger.ToString());
		}
	}
}
