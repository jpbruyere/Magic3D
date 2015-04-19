using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace Magic3D
{
    [Serializable]
    public class Effect
    {
		static bool ListIsNullOrEmpty(IList list)
		{
			if (list == null)
				return true;
			return (list.Count == 0);
		}

		public enum Types
        {
            OneShot,
            Continuous,
            Replacement,
            Prevention,
        }
		public enum ModeEnum
		{
			NotSet,
			Continuous,
			RaiseCost,
			ReduceCost,
			CantBeCast,
			CantPlayLand,
			CantAttack,
			ETBTapped
		}
			
        public EffectType TypeOfEffect;
        
		#region CTOR
        public Effect() { }
        public Effect(EffectType _type)
        {
            TypeOfEffect = _type;
        }
		#endregion
		public virtual void Apply(CardInstance _source, Ability _ability, Player _target)
		{
			Player affected = _target;
			if (affected == null)
				affected = _source.Controler;
			
			switch (TypeOfEffect) {

			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
		protected virtual void ApplySingle(CardInstance _source, Ability _ability, object _target = null)
		{			
			Player player = _target as Player;
			CardInstance ci = _target as CardInstance;
			if (ci == null)
				ci = _source;
			if (player == null && _source != null)
				player = _source.Controler;
			
			switch (TypeOfEffect) {
			case EffectType.GainLife:				
				player.LifePoints += (this as NumericEffect).Amount.GetValue(_source);
				break;
			case EffectType.LoseLife:
				player.LifePoints -= (this as NumericEffect).Amount.GetValue(_source);
				break;
			case EffectType.Unset:
				break;
			case EffectType.Loose:
				break;
			case EffectType.LooseAllAbilities:
				break;
			case EffectType.Gain:
				
				break;
			case EffectType.Discard:
				ci.ChangeZone (CardGroupEnum.Graveyard);
				break;
			case EffectType.Pump:
				break;
			case EffectType.Effect:
				break;
			case EffectType.Counter:
				break;
			case EffectType.Destroy:
				ci.PutIntoGraveyard ();
				break;
			case EffectType.Tap:
				ci.tappedWithoutEvent = true;
				break;
			case EffectType.DoesNotUntap:
				break;			
			case EffectType.TapAll:
				break;
			case EffectType.PreventDamage:
				break;
			case EffectType.Charm:
				break;
			case EffectType.DealDamage:
				break;
			case EffectType.ChangeZone:
				ChangeZoneAbility cza = _ability as ChangeZoneAbility;
				ci.Reset ();
				ci.ChangeZone (cza.Destination);
				if (cza.Tapped)
					ci.tappedWithoutEvent = true;
				else
					ci.tappedWithoutEvent = false;
				break;
			case EffectType.Draw:
				break;
			case EffectType.DestroyAll:
				break;
			case EffectType.RepeatEach:
				break;
			case EffectType.Token:
				break;
			case EffectType.GainControl:
				break;
			case EffectType.Repeat:
				break;
			case EffectType.Debuff:
				break;
			case EffectType.ChooseColor:
				break;
			case EffectType.Dig:
				break;
			case EffectType.PumpAll:
				break;
			case EffectType.RemoveCounterAll:
				break;
			case EffectType.ChangeZoneAll:
				break;
			case EffectType.DamageAll:
				break;
			case EffectType.UntapAll:
				break;
			case EffectType.PutCounter:
				break;
			case EffectType.PutCounterAll:
				break;
			case EffectType.StoreSVar:
				break;
			case EffectType.FlipACoin:
				break;
			case EffectType.SacrificeAll:
				break;
			case EffectType.Untap:
				break;
			case EffectType.Mill:
				break;
			case EffectType.Animate:
				break;
			case EffectType.Fog:
				break;
			case EffectType.RemoveCounter:
				break;
			case EffectType.ExchangeZone:
				break;
			case EffectType.AnimateAll:
				break;
			case EffectType.ChooseCard:
				break;
			case EffectType.Reveal:
				break;
			case EffectType.ChooseSource:
				break;
			case EffectType.MustBlock:
				break;
			case EffectType.ExchangeControl:
				break;
			case EffectType.RearrangeTopOfLibrary:
				break;
			case EffectType.CopyPermanent:
				break;
			case EffectType.SetState:
				break;
			case EffectType.Balance:
				break;
			case EffectType.RevealHand:
				break;
			case EffectType.Sacrifice:
				break;
			case EffectType.AddTurn:
				break;
			case EffectType.TwoPiles:
				break;
			case EffectType.ManaReflected:
				break;
			case EffectType.SetLife:
				break;
			case EffectType.DebuffAll:
				break;
			case EffectType.Fight:
				break;
			case EffectType.ChooseType:
				break;
			case EffectType.Shuffle:
				break;
			case EffectType.NameCard:
				break;
			case EffectType.PermanentNoncreature:
				break;
			case EffectType.PermanentCreature:
				break;
			case EffectType.TapOrUntap:
				break;
			case EffectType.GenericChoice:
				break;
			case EffectType.Play:
				break;
			case EffectType.BecomesBlocked:
				break;
			case EffectType.AddOrRemoveCounter:
				break;
			case EffectType.WinsGame:
				break;
			case EffectType.Proliferate:
				break;
			case EffectType.Scry:
				break;
			case EffectType.MoveCounter:
				break;
			case EffectType.GainOwnership:
				break;
			case EffectType.ChangeTargets:
				break;
			case EffectType.UnattachAll:
				break;
			case EffectType.PeekAndReveal:
				break;
			case EffectType.LosesGame:
				break;
			case EffectType.DigUntil:
				break;
			case EffectType.CopySpellAbility:
				break;
			case EffectType.RollPlanarDice:
				break;
			case EffectType.RegenerateAll:
				break;
			case EffectType.DelayedTrigger:
				break;
			case EffectType.MustAttack:
				break;
			case EffectType.ProtectionAll:
				break;
			case EffectType.RemoveFromCombat:
				break;
			case EffectType.RestartGame:
				break;
			case EffectType.PreventDamageAll:
				break;
			case EffectType.ExchangeLife:
				break;
			case EffectType.DeclareCombatants:
				break;
			case EffectType.ControlPlayer:
				break;
			case EffectType.Phases:
				break;
			case EffectType.Clone:
				break;
			case EffectType.Clash:
				break;
			case EffectType.ChooseNumber:
				break;
			case EffectType.EachDamage:
				break;
			case EffectType.ReorderZone:
				break;
			case EffectType.ChoosePlayer:
				break;
			case EffectType.EndTurn:
				break;
			case EffectType.MultiplePiles:
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
		public virtual void Apply(CardInstance _source, Ability _ability, object _target)
		{			
			IList targets = _target as IList;
			if (ListIsNullOrEmpty(targets))
				ApplySingle (_source, _ability, _target);
			foreach (object o in targets)
				ApplySingle (_source, _ability, o);					
		}


		#region operators
		public static implicit operator Effect(EffectType et)
		{
			return new Effect (et);
		}
		#endregion

		public override string ToString ()
		{
			return TypeOfEffect.ToString ();
		}
        void Effect_MagicEvent(MagicEventArg arg)
        {
//            if (TrigStart.Type != MagicEventType.Unset)
//            {
//                if (arg.Type == TrigStart.Type)
//                { }
//            }
//            if (TrigEnd.Type != MagicEventType.Unset)
//            {
//                if (arg.Type == TrigEnd.Type)
//                {
////                    if (TrigEnd.Source != null && TrigEnd.Source == arg.Source)
////                    {
////                        MagicEngine.MagicEvent -= Effect_MagicEvent;
////                        ContainingList.RemoveEffect(this);
////                    }
//                }
//            }
        }
    }
	 
    [Serializable]
    public class ControlEffect : Effect
    {
        public Player Controler;
    }

    [Serializable]
    public class NumericEffect : Effect
    {
		public int Multiplier = 1;
        public IntegerValue Amount;

		public NumericEffect() : base() {}
		public NumericEffect(EffectType et) : base(et){}
		public NumericEffect(EffectType et, IntegerValue amount) : base(et)
		{
			Amount = amount;
		}
    }
    [Serializable]
    public class AbilityEffect : Effect
    {
        public Ability Ability;
		public AbilityEffect(Ability _ability)
		{
			this.TypeOfEffect = EffectType.Gain;
			Ability = _ability;
		}
    }
//	public class LifeEffect : Effect
//	{		
//		public int Amount;
//		public Cost Cost;
//
//		public override void Apply (CardInstance _source, object _target = null)
//		{
//			Source = _source;
//			Source.Controler.LifePoints += Amount;
//		}
//	}
//    public class EffectList : List<Effect>
//    {
//        public void AddEffect(Effect e)
//        {
//            this.Add(e);
//            e.ContainingList = this;
//        }
//        public void RemoveEffect(Effect e)
//        {
//            this.Remove(e);
//            e.ContainingList = null;
//        }
//    }

	public class CardCounter : IntegerValue
	{
		public MultiformAttribut<Target> CardsToCount = new MultiformAttribut<Target> ();
		public int Multiplier = 1;
		#region implemented abstract members of IntegerValue
		public override int Value {
			get {
				int sum = 0;
				foreach (CardTarget ct in CardsToCount.Values.OfType<CardTarget>()) {
					sum += ct.GetValidTargetsInPlay (null).Count();
				}
				return sum * Multiplier;
			}
			set {
				throw new NotImplementedException ();
			}
		}
		public override int GetValue (CardInstance _source, object _target = null)
		{
			int sum = 0;
			foreach (CardTarget ct in CardsToCount.Values.OfType<CardTarget>()) {
				sum += ct.GetValidTargetsInPlay (_source).Count();
			}
			return sum * Multiplier;
		}
		#endregion
	}


	public class SimpleIntegerValue : IntegerValue
	{
		int _value;

		public SimpleIntegerValue(int v)
		{
			_value = v;
		}


		#region implemented abstract members of IntegerValue
		public override int Value {
			get { return _value; }
			set { _value = value; }
		}
		public override int GetValue (CardInstance _source, object _target = null)
		{
			return _value;
		}
		#endregion

		public static implicit operator SimpleIntegerValue(int v)
		{
			return new SimpleIntegerValue (v);
		}
	}

	public abstract class IntegerValue{
		public abstract int Value { get; set;}
		public abstract int GetValue (CardInstance _source, object _target = null);

		public static implicit operator int(IntegerValue iv)
		{
			return iv.GetValue (null, null);
		}
		public static implicit operator IntegerValue(int v)
		{
			return new SimpleIntegerValue (v) as IntegerValue;
		}
	}
}
