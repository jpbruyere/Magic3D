using System;
using System.Collections.Generic;

namespace Magic3D
{
	public class EffectGroupInstance : List<Effect>
	{
		public CardInstance Source;
		public EffectGroup Effects;
		public Trigger TrigEnd;

		public EffectGroupInstance (CardInstance _source, EffectGroup _effects, Trigger _trigEnd = null)
		{
			Source = _source;
			this.AddRange(_effects);
			TrigEnd = _trigEnd;
		}
	}
}

