using System;
using System.Linq;
using System.Collections.Generic;

namespace Magic3D
{
	public class CardTarget : Target
	{
		public MultiformAttribut<CardTypes> ValidCardTypes;
		public MultiformAttribut<CardTypes> HavingAttachedCards;
		public MultiformAttribut<Ability> HavingAbilities;
		public MultiformAttribut<Ability> WithoutAbilities;
		public MultiformAttribut<ManaTypes> ValidCardColors;
		public ControlerType Controler = ControlerType.All;
		public CombatImplication CombatState = CombatImplication.Unset;
		public NumericConstrain PowerConstrain;
		public NumericConstrain ToughnessConstrain;
		/// <summary>
		/// If false, origin card can't be targeted
		/// </summary>
		public bool CanBeTargetted = true;
		public MultiformAttribut<CardGroupEnum> ValidGroup;

		public CardTarget(TargetType tt = TargetType.Card)
		{
			TypeOfTarget = tt;
		}

		/// <summary>
		/// True if target is valid
		/// </summary>
		/// <param name="_target">CardTarget to test</param>
		/// <param name="_source">The original card providing this targeting conditions</param>
		public override bool Accept (object _target, CardInstance _source)
		{
			CardInstance c = _target as CardInstance;
			if (c == null)
				return false;
			if (TypeOfTarget == TargetType.EquipedBy || TypeOfTarget == TargetType.EnchantedBy) {
				if (_target is CardInstance)
					return  _source.AttachedTo == _target;
				return false;
			} else if (TypeOfTarget == TargetType.Attached) {
				if (_target is CardInstance)
					return  (_target as CardInstance).AttachedTo == _source;
				return false;
			}
			if (!CanBeTargetted && _target == _source)
				return false;
			if (!(c.Model.Types >= ValidCardTypes))
				return false;

			if (ValidCardColors != null) {
				foreach (ManaTypes mc in ValidCardColors.Values) {
					if (!c.HasColor (mc))
						return false;
				}
			}

			if (Controler == ControlerType.You) {
				if (_source.Controler != c.Controler)
					return false;
			}else if (Controler == ControlerType.Opponent){
				if (_source.Controler == c.Controler)
					return false;
			}

			if (CombatState == CombatImplication.Attacking) {
				if (!c.Controler.AttackingCreature.Contains (c))
					return false;
			}else if (CombatState == CombatImplication.Blocking){
				if (!c.Controler.BlockingCreature.Contains (c))
					return false;
			}

			if (ValidGroup != null) {
				if (!ValidGroup.Contains(c.CurrentGroup.GroupName))
					return false;
			}
			//TODO: power constrains
			//TODO: abilities check
			//TODO: having attached cards

			return true;
		}

		#region Operators
		public static bool operator ==(CardTarget ct, CardTypes t)
		{
			return ct.HavingAbilities.Count > 0 ?
				false :
				ct.ValidCardTypes == t;
		}
		public static bool operator !=(CardTarget ct, CardTypes t)
		{
			return ct.HavingAbilities.Count > 0 ?
				true :
				ct.ValidCardTypes != t;
		}
		#endregion

		public IEnumerable<CardInstance> GetValidTargetsInPlay (CardInstance _source){
			MagicEngine engine = MagicEngine.CurrentEngine;
			IEnumerable<CardInstance> cards;

			switch (TypeOfTarget) {
			case TargetType.Self:
				yield return _source;
				break;
			case TargetType.EnchantedBy:
			case TargetType.EquipedBy:
				if (_source.IsAttached)
					yield return _source.AttachedTo;
				break;
			case TargetType.Card:
			case TargetType.Attached:
				if (Controler == ControlerType.All)
					cards = engine.Players.SelectMany (p => p.InPlay.Cards);
				else if (Controler == ControlerType.You)
					cards = _source.Controler.InPlay.Cards;
				else
					cards = _source.Controler.Opponent.InPlay.Cards;

				foreach (CardInstance ci in cards) {
					if (Accept (ci, _source))
						yield return ci;	
				}
				break;
			}
		}

		public override string ToString()
		{
			string tmp = "";
			if (ValidCardTypes == null)
				tmp += TypeOfTarget.ToString();
			else
				tmp += ValidCardTypes.ToString();

			if (HavingAbilities != null)
				tmp += " having " + HavingAbilities.ToString();

			if (WithoutAbilities != null)
				tmp += " without " + WithoutAbilities.ToString();

			return tmp;
		}

		//public static bool operator ==(CardTarget ct, EffectType t)
		//{
		//    return ct.ValidCardTypes.Count > 0 ?
		//        false :
		//        ct.HavingAbilities == t;
		//}
		//public static bool operator ==(CardTarget ct, EffectType t)
		//{
		//    return ct.ValidCardTypes.Count > 0 ?
		//        false :
		//        ct.HavingAbilities == t;
		//}
	}

}

