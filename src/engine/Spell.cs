using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Magic3D
{    
	public class Spell
    {
        public CardInstance Source;
        public Cost RemainingCost;
        
        public Spell()
        { }

        public Spell(CardInstance _cardInstance)
        {
            Source = _cardInstance;
            if (_cardInstance.Model.Cost != null)
                RemainingCost = _cardInstance.Model.Cost.Clone();
        }

        public bool WaitForTarget
        {
            get
            {
                return SelectedTargets.Count < RequiredTargetCount ? true : false;
            }
        }

        public bool TryToAddTarget(CardInstance c)
        {
            if (!WaitForTarget)
                return false;

			//other target group are possible, should change
            if (c.CurrentGroup.GroupName != CardGroupEnum.InPlay)
                return false;
				
            foreach (CardTarget ct in ValidTargets.Values.OfType<CardTarget>())
            {
                if (c.Model.Types == ct.ValidCardTypes)
                {
                    SelectedTargets.Add(c);
					//Magic3D.pCurrentSpell.Update(this);
                    return true;
                }
            }

            return false;
        }
        public virtual int RequiredTargetCount
        {
            get
            {
				Ability a = Source.getAbilitiesByType(AbilityEnum.Attach).FirstOrDefault();
                if (a == null)
                    return 0;

                return a.RequiredTargetCount;
            }
            set
            {
                Debug.WriteLine("trying to set required target count for non ability spell");
            }
        }
        public virtual MultiformAttribut<Target> ValidTargets
        {
            get
            {
				Ability a = Source.getAbilitiesByType(AbilityEnum.Attach).FirstOrDefault();
                if (a == null)
                    return null;

                return a.ValidTargets;
            }
            set
            {
                Debug.WriteLine("trying to set selected targets for non ability spell");
            }
        }
        public virtual List<Object> SelectedTargets
        {
            get
            {
				Ability a = Source.getAbilitiesByType(AbilityEnum.Attach).FirstOrDefault();
                if (a == null)
                    return new List<object>();

                return a.SelectedTargets;
            }
            set
            {
                Debug.WriteLine("trying to set selected targets for non ability spell");
            }
        }

//        public bool Resolve()
//        {
//            return Source.Model.Cost == RemainingCost ? true : false;
//        }
    }
}
