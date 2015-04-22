using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

namespace Magic3D
{
	public class EffectGroup : List<Effect>
	{
		public Effect.ModeEnum Mode;
		public Cost CounterEffectCost;
		public Trigger TrigStart;
		public Trigger TrigEnd;
		public MultiformAttribut<Target> Affected;

		public virtual void Apply(CardInstance _source, Ability _ability = null, object _target = null)
		{
			IList<CardInstance> targets = GetAffectedCardInstances (_source, _ability);							
				
			foreach (Effect e in this) {
				if (_target == null)
					e.Apply (_source, _ability, targets);
				else
					e.Apply (_source, _ability, _target);
			}
		}

		public IList<CardInstance> GetAffectedCardInstances (CardInstance _source, Ability _ability){
			List<CardInstance> temp = new List<CardInstance> ();

			if (Affected == null)
				return null;
			
			MagicEngine engine = MagicEngine.CurrentEngine;
			foreach (CardTarget ct in Affected.Values.OfType<CardTarget>()) {
				foreach (CardInstance ci in ct.GetValidTargetsInPlay (_source))
					temp.Add(ci);
//				switch (ct.TypeOfTarget) {
//				case TargetType.Self:
//					yield return _source;
//					break;
//				case TargetType.EnchantedBy:
//					if (_source.IsAttached)
//						yield return _source.AttachedTo;
//					break;
//				case TargetType.Card:
//					if (ct.Controler == ControlerType.All)
//						cards = engine.Players.SelectMany (p => p.InPlay.Cards);
//					else if (ct.Controler == ControlerType.You)
//						cards = _source.Controler.InPlay.Cards;
//					else
//						cards = _source.Controler.Opponent.InPlay.Cards;
//
//					foreach (CardInstance ci in cards) {
//						if (ct.Accept (ci, _source))
//							yield return ci;	
//					}
//
//					break;
//				}
			}	
			return temp.Count > 0 ? temp : null;
		}

		public static EffectGroup Parse(string s)
		{
			string[] tmp = s.Split(new char[] { '|' });

			EffectGroup effects= new EffectGroup ();


			foreach (string t in tmp)
			{
				string[] tmp2 = t.Split(new char[] { '$' });
				string value = tmp2[1].Trim();
				int v;
				NumericEffect numEff = null;

				switch (tmp2[0].Trim())
				{
				case "Mode":
					effects.Mode = (Effect.ModeEnum)Enum.Parse (typeof(Effect.ModeEnum), tmp2 [1]);
					break;
				case "Affected":
					effects.Affected = Target.ParseTargets (value);
					break;
				case "GainControl":
					effects.Add (new Effect(EffectType.GainControl));
					break;
				case "Description":
					break;
				case "AddKeyword":
					AbilityEnum ae = AbilityEnum.Unset;
					if (Enum.TryParse (value, true, out ae)) {
						effects.Add (new AbilityEffect (new Ability (ae)));
						break;
					}
					switch (value) {
					case "Double Strike":
						effects.Add (new AbilityEffect (new Ability(AbilityEnum.DoubleStrike)));
						break;
					default:
						Debug.WriteLine ("unknown AddKeyword in effect: " + value);
						break;
					}
					break;
				case "Condition":
					break;
				case "AddAbility":
					break;
				case "AddPower":
					numEff = new NumericEffect (EffectType.AddPower);
					if (int.TryParse (value, out v))
						numEff.Amount = v;
					else
						SVarToResolve.RegisterSVar(value, numEff, numEff.GetType().GetField("Amount"));
						
					effects.Add (numEff);
					numEff = null;
					break;
				case "AddToughness":
					numEff = new NumericEffect (EffectType.AddTouchness);
					if (int.TryParse (value, out v))
						numEff.Amount = v;
					else
						SVarToResolve.RegisterSVar (value, numEff, numEff.GetType ().GetField ("Amount"));

					effects.Add (numEff);
					numEff = null;
					break;
				case "SetPower":
					if (!int.TryParse(value, out v))
						break;
					effects.Add(new NumericEffect
						{
							TypeOfEffect = EffectType.SetPower,
							Amount = v,
						});
					break;
				case "SetToughness":
					if (!int.TryParse(value, out v))
						break;
					effects.Add(new NumericEffect
						{
							TypeOfEffect = EffectType.SetTouchness,
							Amount = v,
						});
					break;
				case "EffectZone":
					break;
				case "CharacteristicDefining":
					break;
				case "AddType":
					break;
				case "References":
					break;
				case "ValidCard":
					break;
				case "AddHiddenKeyword":
					switch (value) {
					case "CARDNAME can't attack or block.":
						effects.Add (EffectType.CantAttack);
						effects.Add (EffectType.CantBlock);
						break;
					default:
						Debug.WriteLine ("Unkwnown HiddenKeyword: " + value);
						break;
					}
					break;
				case "CheckSVar":
					break;
				case "SVarCompare":
					break;
				case "AffectedZone":
					break;
				case "Activator":
					break;
				case "Type":
					break;
				case "Color":
					break;
				case "Amount":
					break;
				case "SetColor":
					break;
				case "Caster":
					break;
				case "OpponentAttackedWithCreatureThisTurn":
					break;
				case "AddColor":
					break;
				case "AddSVar":
					break;
				case "Spell":
					break;
				case "SetMaxHandSize":
					break;
				case "AddTrigger":
					break;
				case "RemoveKeyword":
					break;
				case "GlobalRule":
					break;
				case "Attacker":
					break;
				case "Cost":
					break;
				case "Player":
					break;
				case "Phases":
					break;
				case "Target":
					break;
				case "Optional":
					break;
				case "AILogic":
					break;
				case "CheckSecondSVar":
					break;
				case "SecondSVarCompare":
					break;
				case "RemoveSubTypes":
					break;
				case "RemoveAllAbilities":
					break;
				case "AddStaticAbility":
					break;
				case "SharedKeywordsZone":
					break;
				case "SharedRestrictions":
					break;
				case "MaxDamage":
					break;
				case "Source":
					break;
				case "RemoveCreatureTypes":
					break;
				case "TopCardOfLibraryIs":
					break;
				case "NonMana":
					break;
				case "GainsAbilitiesOf":
					break;
				case "GainsAbilitiesOfZones":
					break;
				case "RemoveCardTypes":
					break;
				case "CombatDamage":
					break;
				case "ValidTarget":
					break;
				case "RemoveType":
					break;
				case "ValidSource":
					break;
				case "RaiseMaxHandSize":
					break;
				case "Origin":
					break;
				case "MinMana":
					break;
				case "ValidSpellTarget":
					break;
				case "TapAbility":
					break;
				case "KeywordMultiplier":
					break;
				case "CheckThirdSVar":
					break;
				case "CheckFourthSVar":
					break;
				case "AddReplacementEffects":
					break;
				case "OnlySorcerySpeed":
					break;
				default:
					break;
				}
			}


			return effects;
		}

	}
}

