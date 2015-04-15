using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic3D
{
        public interface IDamagable
    {
        void AddDamages(Damage d);

    }
    public class Damage
    {
        public IDamagable Target;
        public CardInstance Source;
        public int Amount;

        public Damage(CardInstance _target, CardInstance _source, int _amount)
        {
            Target = _target;
            Source = _source;
            Amount = _amount;
        }

        public void Deal()
        {
            if (Source.HasAbility(AbilityEnum.Trample) && Target is CardInstance)
            {
                CardInstance t = Target as CardInstance;
                Target.AddDamages(new Damage(t,Source,t.Toughness));
                Amount -= t.Toughness;
                t.Controler.AddDamages(this);
            }else{
                Target.AddDamages(this);
            }
        }
    	public override string ToString ()
		{
			return string.Format ("{0} deals {1} damage to {2}", Source.Model.Name,Amount,Target.ToString());
		}
	}

    //public class DamageList : List<Damage>
    //{
    //    public Damage AddDamage(Damage d)
    //    {
            
    //        this.Add(d);



    //        return 
    //    }
    //}
}
