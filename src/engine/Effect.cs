using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using System.Collections;

namespace Magic3D
{


        /// <summary>
        /// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
        /// Provides a method for performing a deep copy of an object.
        /// Binary Serialization is used to perform the copy.
        /// </summary>
        public static class ObjectCopier
        {
            /// <summary>
            /// Perform a deep Copy of the object.
            /// </summary>
            /// <typeparam name="T">The type of object being copied.</typeparam>
            /// <param name="source">The object instance to copy.</param>
            /// <returns>The copied object.</returns>
            public static T Clone<T>(this T source)
            {
                Type t = source.GetType();

                FieldInfo[] fields = t.GetFields(BindingFlags.Public|BindingFlags.Instance);

                T result = (T)Activator.CreateInstance(t);

                foreach (FieldInfo fi in fields)
                {
                    fi.SetValue(result, fi.GetValue(source));
                }
                
                return result;

            }
        }
    [Serializable]
    public class Effect
    {
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
        public CardInstance Source;
        public Cost CounterEffectCost;
        public Trigger TrigStart;
        public Trigger TrigEnd;
        public EffectList ContainingList;

        public Effect()
        { }

        public Effect(EffectType _type)
        {
            TypeOfEffect = _type;
        }

		public virtual void Apply(CardInstance _source, object _target = null)
		{
			switch (TypeOfEffect) {
			case EffectType.Unset:
				break;
			case EffectType.Loose:
				break;
			case EffectType.LooseAllAbilities:
				break;
			case EffectType.Gain:
				break;
			case EffectType.Discard:
				break;
			case EffectType.Pump:
				break;
			case EffectType.AddPower:
				break;
			case EffectType.AddTouchness:
				break;
			case EffectType.SetPower:
				break;
			case EffectType.SetTouchness:
				break;
			case EffectType.Effect:
				break;
			case EffectType.Counter:
				break;
			case EffectType.Destroy:
				break;
			case EffectType.Tap:
				if (_target == null)
					_source.tappedWithoutEvent = true;
				else if (_target is IEnumerable) {
					IList _targets = _target as IList;
					if (_targets.Count == 0)
						_source.tappedWithoutEvent = true;
					else {
						foreach (CardInstance t in _targets) {
							t.Tap ();
						}
					}
				} else
					(_target as CardInstance).Tap();				
				break;
			case EffectType.DoesNotUntap:
				break;
			case EffectType.CantAttack:
				break;
			case EffectType.TapAll:
				break;
			case EffectType.LoseLife:
				break;
			case EffectType.PreventDamage:
				break;
			case EffectType.Charm:
				break;
			case EffectType.DealDamage:
				break;
			case EffectType.ChangeZone:
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
			case EffectType.GainLife:
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

        public static IEnumerable<Effect> Parse(string s)
        {
            string[] tmp = s.Split(new char[] { '|' });

			List<Effect> effects= new List<Effect> ();

			ModeEnum Mode = Effect.ModeEnum.NotSet;

            foreach (string t in tmp)
            {
                string[] tmp2 = t.Split(new char[] { '$' });
                string value = tmp2[1].Trim();
                int v;
                switch (tmp2[0].Trim())
                {
				case "Mode":
					Mode = (ModeEnum)Enum.Parse (typeof(ModeEnum), tmp2 [1]);
                    break;
                case "Affected":
                    break;
                case "GainControl":
                    break;
                case "Description":
                    break;
                case "AddKeyword":
                    break;
                case "Condition":
                    break;
                case "AddAbility":
                    break;
                case "AddPower":
                    if (!int.TryParse(value, out v))
                        break;
                    effects.Add(new NumericEffect
                    {
                        TypeOfEffect = EffectType.AddPower,
                        NumericValue = v
                    });
                    break;
                case "AddToughness":
                    if (!int.TryParse(value, out v))
                        break;
                    effects.Add(new NumericEffect
                    {
                        TypeOfEffect = EffectType.AddTouchness,
                        NumericValue = v
                    });
                    break;                        
                case "SetPower":
                    if (!int.TryParse(value, out v))
                        break;
                    effects.Add(new NumericEffect
                    {
                        TypeOfEffect = EffectType.SetPower,
                        NumericValue = v
                    });
                    break;
                case "SetToughness":
                    if (!int.TryParse(value, out v))
                        break;
                    effects.Add(new NumericEffect
                    {
                        TypeOfEffect = EffectType.SetTouchness,
                        NumericValue = v
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

        void Effect_MagicEvent(MagicEventArg arg)
        {
            if (TrigStart.Type != MagicEventType.Unset)
            {
                if (arg.Type == TrigStart.Type)
                { }
            }
            if (TrigEnd.Type != MagicEventType.Unset)
            {
                if (arg.Type == TrigEnd.Type)
                {
//                    if (TrigEnd.Source != null && TrigEnd.Source == arg.Source)
//                    {
//                        MagicEngine.MagicEvent -= Effect_MagicEvent;
//                        ContainingList.RemoveEffect(this);
//                    }
                }
            }
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
        public int NumericValue;
		public override void Apply (CardInstance _source, object _target = null)
		{			
			switch (TypeOfEffect) {
			case EffectType.Unset:
				break;
			case EffectType.Loose:
				break;
			case EffectType.LooseAllAbilities:
				break;
			case EffectType.Gain:
				break;
			case EffectType.Discard:
				break;
			case EffectType.Pump:
				break;
			case EffectType.AddPower:
				break;
			case EffectType.AddTouchness:
				break;
			case EffectType.SetPower:
				break;
			case EffectType.SetTouchness:
				break;
			case EffectType.Effect:
				break;
			case EffectType.Counter:
				break;
			case EffectType.Destroy:
				break;
			case EffectType.Tap:
				break;
			case EffectType.DoesNotUntap:
				break;
			case EffectType.CantAttack:
				break;
			case EffectType.TapAll:
				break;
			case EffectType.LoseLife:
				(_source.Controler as Player).LifePoints -= NumericValue;
				break;
			case EffectType.PreventDamage:
				break;
			case EffectType.Charm:
				break;
			case EffectType.DealDamage:
				break;
			case EffectType.ChangeZone:
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
			case EffectType.GainLife:
				(_source.Controler as Player).LifePoints += NumericValue;
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
				break;
			}
		}
    }
    [Serializable]
    public class AbilityEffect : Effect
    {
        public Ability Ability;
    }
	public class LifeEffect : Effect
	{		
		public int Amount;
		public Cost Cost;

		public override void Apply (CardInstance _source, object _target = null)
		{
			Source = _source;
			Source.Controler.LifePoints += Amount;
		}
	}
    public class EffectList : List<Effect>
    {
        public void AddEffect(Effect e)
        {
            this.Add(e);
            e.ContainingList = this;
        }
        public void RemoveEffect(Effect e)
        {
            this.Remove(e);
            e.ContainingList = null;
        }
    }

}
