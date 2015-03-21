using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Magic3D
{
    public class Abilities : List<Ability>
    {
        public bool Has(AbilityEnum ab)
        {
            foreach (Ability a in this)
            {
                if (a.AbilityType == ab)
                    return true;
            }
            return false;
        }
    }

    public class TriggeredAbility : Ability
    {
    }

    public class ActivatedAbility : Ability
    {
        //public Cost ActivationCost;

    }
    public class StaticAbility : Ability
    {

    }

    public class ManaAbility : ActivatedAbility
    {
        public ManaAbility()
        {
            AbilityType = AbilityEnum.Mana;
        }
        public Cost ProducedMana;
    }
		
    public class Ability
    {
		#region CTOR
		public Ability()
		{
		}

		public Ability(AbilityEnum a)
		{
			AbilityType = a;
		}
		#endregion
        
        int _requiredTargetCount = 0;
        public int RequiredTargetCount
        {
            get
            {
                return _requiredTargetCount;
            }
            set
            {
                _requiredTargetCount = value;
            }
        }

		public AbilityEnum AbilityType = AbilityEnum.Unset;
        public List<Effect> Effects = new List<Effect>();
        public Cost ActivationCost;
        public string Description = "";

        MultiformAttribut<Target> _validTargets;
        public MultiformAttribut<Target> ValidTargets
        {
            get { return _validTargets; }
            set { _validTargets = value; }
        }
			
        List<Object> _selectedTargets = new List<object>();        
        public List<Object> SelectedTargets
        {
            get { return _selectedTargets; }
            set
            {
                _selectedTargets = value;
            }
        }

		public static Ability Parse(string strAbility)
		{
			Ability a = new Ability();


			string[] tmp = strAbility.Split(new char[] { '|' });

			foreach (string ab in tmp)
			{
				int v;

				if (string.IsNullOrWhiteSpace(ab))
					continue;

				int dollarPos = ab.IndexOf('$');

				if (dollarPos < 0)
					continue;

				AbilityFieldsEnum varName = (AbilityFieldsEnum)Enum.Parse(typeof(AbilityFieldsEnum), ab.Substring(0, dollarPos), true);
				string value = ab.Substring(dollarPos + 1).Trim();

				Effect e = null;

				switch (varName)
				{
				case AbilityFieldsEnum.SP:
				case AbilityFieldsEnum.AB:
					#region ability type
					switch (value)
					{
					case "Discard":
						break;
					case "Mana":
						a = new ManaAbility();
						break;
					case "Pump":
						break;
					case "Attach":
					case "Animate":
						a.AbilityType = AbilityEnum.Attach;
						a.RequiredTargetCount = 1;
						break;
					case "Effect":
						break;
					case "Counter":
						break;
					case "Destroy":
						break;
					case "Tap":
						break;
					case "TapAll":
						break;
					case "LoseLife":
						break;
					case "PreventDamage":
						break;
					case "Charm":
						break;
					case "DealDamage":
						break;
					case "ChangeZone":
						break;
					case "Draw":
						break;
					case "DestroyAll":
						break;
					case "RepeatEach":
						break;
					case "Token":
						break;
					case "GainControl":
						break;
					case "Repeat":
						break;
					case "Debuff":
						break;
					case "ChooseColor":
						break;
					case "Dig":
						break;
					case "PumpAll":
						break;
					case "RemoveCounterAll":
						break;
					case "ChangeZoneAll":
						break;
					case "DamageAll":
						break;
					case "UntapAll":
						break;
					case "PutCounter":
						break;
					case "GainLife":
						break;
					case "PutCounterAll":
						break;
					case "StoreSVar":
						break;
					case "Regenerate":
						break;
					case "FlipACoin":
						break;
					case "SacrificeAll":
						break;
					case "Untap":
						break;
					case "Mill":
						break;
					case "Fog":
						break;
					case "RemoveCounter":
						break;
					case "Protection":
						break;
					case "ExchangeZone":
						break;
					case "AnimateAll":
						break;
					case "ChooseCard":
						break;
					case "Reveal":
						break;
					case "ChooseSource":
						break;
					case "MustBlock":
						break;
					case "ExchangeControl":
						break;
					case "RearrangeTopOfLibrary":
						break;
					case "CopyPermanent":
						break;
					case "SetState":
						break;
					case "Balance":
						break;
					case "RevealHand":
						break;
					case "Sacrifice":
						break;
					case "AddTurn":
						break;
					case "TwoPiles":
						break;
					case "ManaReflected":
						break;
					case "SetLife":
						break;
					case "DebuffAll":
						break;
					case "Fight":
						break;
					case "ChooseType":
						break;
					case "Shuffle":
						break;
					case "NameCard":
						break;
					case "PermanentNoncreature":
						break;
					case "PermanentCreature":
						break;
					case "TapOrUntap":
						break;
					case "GenericChoice":
						break;
					case "Play":
						break;
					case "BecomesBlocked":
						break;
					case "AddOrRemoveCounter":
						break;
					case "WinsGame":
						break;
					case "Proliferate":
						break;
					case "Scry":
						break;
					case "MoveCounter":
						break;
					case "GainOwnership":
						break;
					case "ChangeTargets":
						break;
					case "UnattachAll":
						break;
					case "PeekAndReveal":
						break;
					case "LosesGame":
						break;
					case "DigUntil":
						break;
					case "CopySpellAbility":
						break;
					case "RollPlanarDice":
						break;
					case "RegenerateAll":
						break;
					case "DelayedTrigger":
						break;
					case "MustAttack":
						break;
					case "ProtectionAll":
						break;
					case "RemoveFromCombat":
						break;
					case "RestartGame":
						break;
					case "PreventDamageAll":
						break;
					case "ExchangeLife":
						break;
					case "DeclareCombatants":
						break;
					case "ControlPlayer":
						break;
					case "Phases":
						break;
					case "Clone":
						break;
					case "Clash":
						break;
					case "ChooseNumber":
						break;
					case "EachDamage":
						break;
					case "ReorderZone":
						break;
					case "ChoosePlayer":
						break;
					case "EndTurn":
						break;
					case "MultiplePiles":
						break;

					default:
						break;
					}
					break;
					#endregion
				case AbilityFieldsEnum.Cost:
					a.ActivationCost = Cost.Parse(value);
					break;
				case AbilityFieldsEnum.ValidTgts:
					a.ValidTargets = Target.ParseTargets(value);
					break;
				case AbilityFieldsEnum.Mode:
					switch (value.Trim().ToLower())
					{
					case "continuous":

						break;
					default:
						break;
					}
					break;
				case AbilityFieldsEnum.NumCards:
					break;
				case AbilityFieldsEnum.References:
					break;
				case AbilityFieldsEnum.SpellDescription:
					break;
				case AbilityFieldsEnum.Produced:
					(a as ManaAbility).ProducedMana = Mana.Parse(value);
					break;
				case AbilityFieldsEnum.NumDef:                        
					if (!int.TryParse(value, out v))
						break;
					a.Effects.Add(new NumericEffect
						{
							TypeOfEffect = EffectType.AddTouchness,
							NumericValue = v
						});
					break;
				case AbilityFieldsEnum.AILogic:
					#region AIlogic
					switch (value.Trim())
					{
					case "GainControl":
						break;
					case "BeginningOfOppTurn":
						break;
					case "Pump":
						e = new Effect { TypeOfEffect = EffectType.Pump };
						break;
					case "Never":
						break;
					case "MostProminentInHumanDeck":
						break;
					case "RedirectSpellDamageFromPlayer":
						break;
					case "Curse":
						break;
					case "MostProminentAttackers":
						break;
					case "Fog":
						break;
					case "Animate":
						break;
					case "Reanimate":
						break;
					case "MostProminentInComputerGraveyard":
						break;
					case "BeforeCombat":
						break;
					case "MostProminentInComputerDeck":
						break;
					case "NeedsPrevention":
						break;
					case "Evasion":
						break;
					case "BalanceCreaturesAndLands":
						break;
					case "BalancePermanents":
						break;
					case "AtLeast1":
						break;
					case "KeepTapped":
						a.Effects.Add(new Effect { TypeOfEffect = EffectType.DoesNotUntap });
						break;
					case "DontCast":
						break;
					case "MostProminentOnBattlefield":
						break;
					case "ChangeType":
						break;
					case "Clone":
						break;
					case "Worst":
						break;
					case "MostProminentComputerControls":
						break;
					case "Always":
						break;
					case "BestBlocker":
						break;
					case "MostProminentHumanControls":
						break;
					case "ZeroToughness":
						break;
					case "EndOfOppTurn":
						break;
					case "DoubleCounters":
						break;
					case "SpecificCard":
						break;
					case "HighestEvaluation":
						break;
					case "Main2":
						break;
					case "EndOfOpponentsTurn":
						break;
					case "WorstCard":
						break;
					case "DiscardLands":
						break;
					case "MostProminentInComputerHand":
						break;
					case "WildHunt":
						break;
					case "MomirAvatar":
						break;
					case "AtLeast2":
						break;
					case "BalanceLands":
						break;
					case "CloneAllTokens":
						break;
					case "CloneMyTokens":
						break;
					case "Color":
						break;
					case "MostProminentHumanCreatures":
						break;
					case "RemoveAllCounters":
						break;
					case "EOT":
						break;
					case "RestrictBlocking":
						break;
					default:
						break;
					}
					#endregion
					break;
				case AbilityFieldsEnum.Name:
					break;
				case AbilityFieldsEnum.Power:                        
					if (!int.TryParse(value, out v))
						break;
					a.Effects.Add(new NumericEffect
						{
							TypeOfEffect = EffectType.SetPower,
							NumericValue = v
						});
					break;
				case AbilityFieldsEnum.Toughness:
					if (!int.TryParse(value, out v))
						break;
					a.Effects.Add(new NumericEffect
						{
							TypeOfEffect = EffectType.SetTouchness,
							NumericValue = v
						});
					break;
				case AbilityFieldsEnum.StaticAbilities:
					break;
				case AbilityFieldsEnum.RememberObjects:
					break;
				case AbilityFieldsEnum.SubAbility:
					break;
				case AbilityFieldsEnum.TargetType:
					break;
				case AbilityFieldsEnum.TgtPrompt:
					break;
				case AbilityFieldsEnum.ValidCards:
					break;
				case AbilityFieldsEnum.KW:
					break;
				case AbilityFieldsEnum.LifeAmount:
					break;
				case AbilityFieldsEnum.Amount:
					break;
				case AbilityFieldsEnum.Choices:
					break;
				case AbilityFieldsEnum.NumDmg:
					break;
				case AbilityFieldsEnum.Origin:
					break;
				case AbilityFieldsEnum.Destination:
					break;
				case AbilityFieldsEnum.NumAtt:
					break;
				case AbilityFieldsEnum.Defined:
					break;
				case AbilityFieldsEnum.Triggers:
					break;
				case AbilityFieldsEnum.SVars:
					break;
				case AbilityFieldsEnum.ImprintCards:
					break;
				case AbilityFieldsEnum.ActivationPhases:
					break;
				case AbilityFieldsEnum.RepeatPlayers:
					break;
				case AbilityFieldsEnum.RepeatSubAbility:
					break;
				case AbilityFieldsEnum.IsCurse:
					break;
				case AbilityFieldsEnum.StackDescription:
					break;
				case AbilityFieldsEnum.TokenAmount:
					break;
				case AbilityFieldsEnum.TokenName:
					break;
				case AbilityFieldsEnum.TokenTypes:
					break;
				case AbilityFieldsEnum.TokenOwner:
					break;
				case AbilityFieldsEnum.TokenColors:
					break;
				case AbilityFieldsEnum.TokenPower:
					break;
				case AbilityFieldsEnum.TokenToughness:
					break;
				case AbilityFieldsEnum.ChangeType:
					break;
				case AbilityFieldsEnum.ChangeNum:
					break;
				case AbilityFieldsEnum.GainControl:
					break;
				case AbilityFieldsEnum.LoseControl:
					break;
				case AbilityFieldsEnum.Untap:
					break;
				case AbilityFieldsEnum.AddKWs:
					break;
				case AbilityFieldsEnum.RepeatOptional:
					break;
				case AbilityFieldsEnum.Keywords:
					break;
				case AbilityFieldsEnum.TokenImage:
					break;
				case AbilityFieldsEnum.TokenKeywords:
					break;
				case AbilityFieldsEnum.DigNum:
					break;
				case AbilityFieldsEnum.ConditionCheckSVar:
					break;
				case AbilityFieldsEnum.ConditionSVarCompare:
					break;
				case AbilityFieldsEnum.ReplacementEffects:
					break;
				case AbilityFieldsEnum.Stackable:
					break;
				case AbilityFieldsEnum.ChangeValid:
					break;
				case AbilityFieldsEnum.DestinationZone:
					break;
				case AbilityFieldsEnum.RememberChanged:
					break;
				case AbilityFieldsEnum.TargetMin:
					break;
				case AbilityFieldsEnum.TargetMax:
					break;
				case AbilityFieldsEnum.AllCounterTypes:
					break;
				case AbilityFieldsEnum.NoReg:
					break;
				case AbilityFieldsEnum.AnyPlayer:
					break;
				case AbilityFieldsEnum.Optional:
					break;
				case AbilityFieldsEnum.Reveal:
					break;
				case AbilityFieldsEnum.LibraryPosition:
					break;
				case AbilityFieldsEnum.NoRegen:
					break;
				case AbilityFieldsEnum.RememberDamaged:
					break;
				case AbilityFieldsEnum.ValidDescription:
					break;
				case AbilityFieldsEnum.SorcerySpeed:
					break;
				case AbilityFieldsEnum.Mandatory:
					break;
				case AbilityFieldsEnum.Planeswalker:
					break;
				case AbilityFieldsEnum.CounterNum:
					break;
				case AbilityFieldsEnum.CounterType:
					break;
				case AbilityFieldsEnum.Ultimate:
					break;
				case AbilityFieldsEnum.TokenStaticAbilities:
					break;
				case AbilityFieldsEnum.TokenSVars:
					break;
				case AbilityFieldsEnum.Permanent:
					break;
				case AbilityFieldsEnum.ActivationLimit:
					break;
				case AbilityFieldsEnum.PlayerTurn:
					break;
				case AbilityFieldsEnum.SVar:
					break;
				case AbilityFieldsEnum.Type:
					break;
				case AbilityFieldsEnum.Expression:
					break;
				case AbilityFieldsEnum.WinSubAbility:
					break;
				case AbilityFieldsEnum.LoseSubAbility:
					break;
				case AbilityFieldsEnum.CostDesc:
					break;
				case AbilityFieldsEnum.RememberTargets:
					break;
				case AbilityFieldsEnum.RestrictValid:
					break;
				case AbilityFieldsEnum.DiscardValid:
					break;
				case AbilityFieldsEnum.Types:
					break;
				case AbilityFieldsEnum.RemoveCreatureTypes:
					break;
				case AbilityFieldsEnum.RememberMilled:
					break;
				case AbilityFieldsEnum.DestinationZone2:
					break;
				case AbilityFieldsEnum.IsPresent:
					break;
				case AbilityFieldsEnum.Colors:
					break;
				case AbilityFieldsEnum.OverwriteColors:
					break;
				case AbilityFieldsEnum.TokenHiddenKeywords:
					break;
				case AbilityFieldsEnum.Choser:
					break;
				case AbilityFieldsEnum.AITgts:
					break;
				case AbilityFieldsEnum.TgtZone:
					break;
				case AbilityFieldsEnum.AnnounceType:
					break;
				case AbilityFieldsEnum.Gains:
					break;
				case AbilityFieldsEnum.DefinedPlayer:
					break;
				case AbilityFieldsEnum.Chooser:
					break;
				case AbilityFieldsEnum.Unique:
					break;
				case AbilityFieldsEnum.Duration:
					break;
				case AbilityFieldsEnum.Hidden:
					break;
				case AbilityFieldsEnum.OriginChoice:
					break;
				case AbilityFieldsEnum.OriginAlternative:
					break;
				case AbilityFieldsEnum.AlternativeMessage:
					break;
				case AbilityFieldsEnum.PrecostDesc:
					break;
				case AbilityFieldsEnum.ActivationZone:
					break;
				case AbilityFieldsEnum.DividedAsYouChoose:
					break;
				case AbilityFieldsEnum.RememberCountered:
					break;
				case AbilityFieldsEnum.Announce:
					break;
				case AbilityFieldsEnum.Zone2:
					break;
				case AbilityFieldsEnum.CheckSVar:
					break;
				case AbilityFieldsEnum.Sacrifice:
					break;
				case AbilityFieldsEnum.RemoveTypes:
					break;
				case AbilityFieldsEnum.Activation:
					break;
				case AbilityFieldsEnum.sVars:
					break;
				case AbilityFieldsEnum.TokenTapped:
					break;
				case AbilityFieldsEnum.PresentCompare:
					break;
				case AbilityFieldsEnum.PresentZone:
					break;
				case AbilityFieldsEnum.ChoiceZone:
					break;
				case AbilityFieldsEnum.GameActivationLimit:
					break;
				case AbilityFieldsEnum.ValidPlayers:
					break;
				case AbilityFieldsEnum.UntilUntaps:
					break;
				case AbilityFieldsEnum.OpponentTurn:
					break;
				case AbilityFieldsEnum.RevealValid:
					break;
				case AbilityFieldsEnum.RememberRevealed:
					break;
				case AbilityFieldsEnum.XColor:
					break;
				case AbilityFieldsEnum.CharmNum:
					break;
				case AbilityFieldsEnum.RepeatCards:
					break;
				case AbilityFieldsEnum.Zone:
					break;
				case AbilityFieldsEnum.RememberChosen:
					break;
				case AbilityFieldsEnum.NoMove:
					break;
				case AbilityFieldsEnum.RememberLKI:
					break;
				case AbilityFieldsEnum.TargetValidTargeting:
					break;
				case AbilityFieldsEnum.UnlessCost:
					break;
				case AbilityFieldsEnum.Valid:
					break;
				case AbilityFieldsEnum.LibraryPosition2:
					break;
				case AbilityFieldsEnum.ChoiceTitle:
					break;
				case AbilityFieldsEnum.Abilities:
					break;
				case AbilityFieldsEnum.Tapped:
					break;
				case AbilityFieldsEnum.UnlessPayer:
					break;
				case AbilityFieldsEnum.UnlessAI:
					break;
				case AbilityFieldsEnum.TargetUnique:
					break;
				case AbilityFieldsEnum.TargetsFromSingleZone:
					break;
				case AbilityFieldsEnum.SacValid:
					break;
				case AbilityFieldsEnum.Radiance:
					break;
				case AbilityFieldsEnum.ConditionManaSpent:
					break;
				case AbilityFieldsEnum.NumTurns:
					break;
				case AbilityFieldsEnum.Separator:
					break;
				case AbilityFieldsEnum.ChosenPile:
					break;
				case AbilityFieldsEnum.UnchosenPile:
					break;
				case AbilityFieldsEnum.ColorOrType:
					break;
				case AbilityFieldsEnum.ReflectProperty:
					break;
				case AbilityFieldsEnum.Static:
					break;
				case AbilityFieldsEnum.RevealNumber:
					break;
				case AbilityFieldsEnum.TargetsWithDifferentControllers:
					break;
				case AbilityFieldsEnum.ConditionDefined:
					break;
				case AbilityFieldsEnum.ConditionPresent:
					break;
				case AbilityFieldsEnum.ConditionCompare:
					break;
				case AbilityFieldsEnum.SVarCompare:
					break;
				case AbilityFieldsEnum.NonBasicSpell:
					break;
				case AbilityFieldsEnum.staticAbilities:
					break;
				case AbilityFieldsEnum.AddsNoCounter:
					break;
				case AbilityFieldsEnum.RememberSacrificed:
					break;
				case AbilityFieldsEnum.RememberCounters:
					break;
				case AbilityFieldsEnum.ValidDesc:
					break;
				case AbilityFieldsEnum.MinCharmNum:
					break;
				case AbilityFieldsEnum.AllValid:
					break;
				case AbilityFieldsEnum.RememberUntapped:
					break;
				case AbilityFieldsEnum.AnyNumber:
					break;
				case AbilityFieldsEnum.TokenAbilities:
					break;
				case AbilityFieldsEnum.OptionalDecider:
					break;
				case AbilityFieldsEnum.Destroy:
					break;
				case AbilityFieldsEnum.NewState:
					break;
				case AbilityFieldsEnum.PreventionSubAbility:
					break;
				case AbilityFieldsEnum.ShieldEffectTarget:
					break;
				case AbilityFieldsEnum.OptionQuestion:
					break;
				case AbilityFieldsEnum.ImprintTargets:
					break;
				case AbilityFieldsEnum.SacMessage:
					break;
				case AbilityFieldsEnum.FromBottom:
					break;
				case AbilityFieldsEnum.Shuffle:
					break;
				case AbilityFieldsEnum.RememberDiscarded:
					break;
				case AbilityFieldsEnum.ValidZone:
					break;
				case AbilityFieldsEnum.WithoutManaCost:
					break;
				case AbilityFieldsEnum.Controller:
					break;
				case AbilityFieldsEnum.Image:
					break;
				case AbilityFieldsEnum.InstantSpeed:
					break;
				case AbilityFieldsEnum.RemoveKeywords:
					break;
				case AbilityFieldsEnum.RememberDestroyed:
					break;
				case AbilityFieldsEnum.ST:
					break;
				case AbilityFieldsEnum.DefinedCards:
					break;
				case AbilityFieldsEnum.DiscardMessage:
					break;
				case AbilityFieldsEnum.Equip:
					break;
				case AbilityFieldsEnum.AnyOpponent:
					break;
				case AbilityFieldsEnum.UpTo:
					break;
				case AbilityFieldsEnum.MaxFromEffect:
					break;
				case AbilityFieldsEnum.ValidCard:
					break;
				case AbilityFieldsEnum.ForgetOtherTargets:
					break;
				case AbilityFieldsEnum.NoPrevention:
					break;
				case AbilityFieldsEnum.AllType:
					break;
				case AbilityFieldsEnum.InvalidTypes:
					break;
				case AbilityFieldsEnum.HiddenKeywords:
					break;
				case AbilityFieldsEnum.ForgetOtherRemembered:
					break;
				case AbilityFieldsEnum.RememberCostCards:
					break;
				case AbilityFieldsEnum.ScryNum:
					break;
				case AbilityFieldsEnum.UntilYourNextUpkeep:
					break;
				case AbilityFieldsEnum.Imprint:
					break;
				case AbilityFieldsEnum.RegenerationTrigger:
					break;
				case AbilityFieldsEnum.ReplaceCardUID:
					break;
				case AbilityFieldsEnum.OverwriteTypes:
					break;
				case AbilityFieldsEnum.KeepSupertypes:
					break;
				case AbilityFieldsEnum.KeepCardTypes:
					break;
				case AbilityFieldsEnum.RemoveAllAbilities:
					a.Effects.Add(new Effect(EffectType.LooseAllAbilities));                        
					break;
				case AbilityFieldsEnum.TargetsSingleTarget:
					break;
				case AbilityFieldsEnum.UseImprinted:
					break;
				case AbilityFieldsEnum.ChooseOrder:
					break;
				case AbilityFieldsEnum.UseAllOriginZones:
					break;
				case AbilityFieldsEnum.UnattachValid:
					break;
				case AbilityFieldsEnum.DamageSource:
					break;
				case AbilityFieldsEnum.ConditionDescription:
					break;
				case AbilityFieldsEnum.PeekAmount:
					break;
				case AbilityFieldsEnum.RevealOptional:
					break;
				case AbilityFieldsEnum.ActivationNumberSacrifice:
					break;
				case AbilityFieldsEnum.PumpZone:
					break;
				case AbilityFieldsEnum.OrColors:
					break;
				case AbilityFieldsEnum.RandomKeyword:
					break;
				case AbilityFieldsEnum.NoRepetition:
					break;
				case AbilityFieldsEnum.RememberTokens:
					break;
				case AbilityFieldsEnum.CopyOnce:
					break;
				case AbilityFieldsEnum.CopyCard:
					break;
				case AbilityFieldsEnum.FoundDestination:
					break;
				case AbilityFieldsEnum.RevealedDestination:
					break;
				case AbilityFieldsEnum.FoundLibraryPosition:
					break;
				case AbilityFieldsEnum.RevealedLibraryPosition:
					break;
				case AbilityFieldsEnum.RememberFound:
					break;
				case AbilityFieldsEnum.RepeatCheckSVar:
					break;
				case AbilityFieldsEnum.RepeatSVarCompare:
					break;
				case AbilityFieldsEnum.DefinedLandwalk:
					break;
				case AbilityFieldsEnum.RememberDrawn:
					break;
				case AbilityFieldsEnum.LoseAtEndStep:
					break;
				case AbilityFieldsEnum.DivideEvenly:
					break;
				case AbilityFieldsEnum.Flashback:
					break;
				case AbilityFieldsEnum.ActivationCardsInHand:
					break;
				case AbilityFieldsEnum.CopyIsColor:
					break;
				case AbilityFieldsEnum.AtRandom:
					break;
				case AbilityFieldsEnum.UntilHostLeavesPlay:
					break;
				case AbilityFieldsEnum.Deplete:
					break;
				case AbilityFieldsEnum.Phase:
					break;
				case AbilityFieldsEnum.Execute:
					break;
				case AbilityFieldsEnum.Defender:
					break;
				case AbilityFieldsEnum.AndOrValid:
					break;
				case AbilityFieldsEnum.DifferentNames:
					break;
				case AbilityFieldsEnum.RepeatCounters:
					break;
				case AbilityFieldsEnum.ControlledByTarget:
					break;
				case AbilityFieldsEnum.NewController:
					break;
				case AbilityFieldsEnum.RememberRemoved:
					break;
				case AbilityFieldsEnum.EachBasicType:
					break;
				case AbilityFieldsEnum.TargetsAtRandom:
					break;
				case AbilityFieldsEnum.Condition:
					break;
				case AbilityFieldsEnum.MaxRepeat:
					break;
				case AbilityFieldsEnum.ExileFaceDown:
					break;
				case AbilityFieldsEnum.AttachedTo:
					break;
				case AbilityFieldsEnum.AddsKeywords:
					break;
				case AbilityFieldsEnum.AtEOT:
					break;
				case AbilityFieldsEnum.RestRandomOrder:
					break;
				case AbilityFieldsEnum.Changetype:
					break;
				case AbilityFieldsEnum.Ninjutsu:
					break;
				case AbilityFieldsEnum.DestinationChoice:
					break;
				case AbilityFieldsEnum.ForgetRemembered:
					break;
				case AbilityFieldsEnum.Random:
					break;
				case AbilityFieldsEnum.RememberRemovedFromCombat:
					break;
				case AbilityFieldsEnum.RememberTargetedCard:
					break;
				case AbilityFieldsEnum.UntilYourNextTurn:
					break;
				case AbilityFieldsEnum.UntilEndOfCombat:
					break;
				case AbilityFieldsEnum.AnySupportedCard:
					break;
				case AbilityFieldsEnum.RandomCopied:
					break;
				case AbilityFieldsEnum.RandomNum:
					break;
				case AbilityFieldsEnum.ChoiceNum:
					break;
				case AbilityFieldsEnum.TargetControls:
					break;
				case AbilityFieldsEnum.RestrictFromZone:
					break;
				case AbilityFieldsEnum.RestrictFromValid:
					break;
				case AbilityFieldsEnum.Min:
					break;
				case AbilityFieldsEnum.Max:
					break;
				case AbilityFieldsEnum.TapOnLose:
					break;
				case AbilityFieldsEnum.DeclareAttackers:
					break;
				case AbilityFieldsEnum.DeclareBlockers:
					break;
				case AbilityFieldsEnum.Until:
					break;
				case AbilityFieldsEnum.DestroyTgt:
					break;
				case AbilityFieldsEnum.RememberSplicedOntoCounteredSpell:
					break;
				case AbilityFieldsEnum.Mitosis:
					break;
				case AbilityFieldsEnum.AddAbilities:
					break;
				case AbilityFieldsEnum.AddSVars:
					break;
				case AbilityFieldsEnum.ValidAttribute:
					break;
				case AbilityFieldsEnum.EffectOwner:
					break;
				case AbilityFieldsEnum.MayShuffle:
					break;
				case AbilityFieldsEnum.Monstrosity:
					break;
				case AbilityFieldsEnum.NoCall:
					break;
				case AbilityFieldsEnum.HeadsSubAbility:
					break;
				case AbilityFieldsEnum.TailsSubAbility:
					break;
				case AbilityFieldsEnum.StaticCommandCheckSVar:
					break;
				case AbilityFieldsEnum.StaticCommandSVarCompare:
					break;
				case AbilityFieldsEnum.AddsCounters:
					break;
				case AbilityFieldsEnum.UntilControllerNextUntap:
					break;
				case AbilityFieldsEnum.RandomChange:
					break;
				case AbilityFieldsEnum.TokenController:
					break;
				case AbilityFieldsEnum.NumCopies:
					break;
				case AbilityFieldsEnum.OwnerOnly:
					break;
				case AbilityFieldsEnum.RememberAllPumped:
					break;
				case AbilityFieldsEnum.TargetsFromDifferentZone:
					break;
				case AbilityFieldsEnum.TgtPromp:
					break;
				case AbilityFieldsEnum.UnlessResolveSubs:
					break;
				case AbilityFieldsEnum.RepeatDefined:
					break;
				case AbilityFieldsEnum.RepeatPresent:
					break;
				case AbilityFieldsEnum.RepeatCompare:
					break;
				case AbilityFieldsEnum.OtherwiseSubAbility:
					break;
				case AbilityFieldsEnum.ConditionTargetValidTargeting:
					break;
				case AbilityFieldsEnum.ConditionTargetsSingleTarget:
					break;
				case AbilityFieldsEnum.TargetRestriction:
					break;
				case AbilityFieldsEnum.CopyForEachCanTarget:
					break;
				case AbilityFieldsEnum.CanTargetPlayer:
					break;
				case AbilityFieldsEnum.Redistribute:
					break;
				case AbilityFieldsEnum.ListTitle:
					break;
				case AbilityFieldsEnum.TargetsWithoutSameCreatureType:
					break;
				case AbilityFieldsEnum.ContionSVarCompare:
					break;
				case AbilityFieldsEnum.isPresent:
					break;
				case AbilityFieldsEnum.ImprintTokens:
					break;
				case AbilityFieldsEnum.DamageDesc:
					break;
				case AbilityFieldsEnum.ConditionChosenColor:
					break;
				case AbilityFieldsEnum.SkipUntap:
					break;
				case AbilityFieldsEnum.RecordChoice:
					break;
				case AbilityFieldsEnum.ChoosePlayer:
					break;
				case AbilityFieldsEnum.TokenTriggers:
					break;
				case AbilityFieldsEnum.Source:
					break;
				case AbilityFieldsEnum.XCantBe0:
					break;
				case AbilityFieldsEnum.ReplacePlayerName:
					break;
				case AbilityFieldsEnum.ChangeSingleTarget:
					break;
				case AbilityFieldsEnum.NoReveal:
					break;
				case AbilityFieldsEnum.RememberTapped:
					break;
				case AbilityFieldsEnum.DelayedTrigger:
					break;
				case AbilityFieldsEnum.DestroyPermanent:
					break;
				case AbilityFieldsEnum.SkipReorder:
					break;
				case AbilityFieldsEnum.NoShuffle:
					break;
				case AbilityFieldsEnum.MaxRevealed:
					break;
				case AbilityFieldsEnum.Reference:
					break;
				case AbilityFieldsEnum.PhaseInOrOutAll:
					break;
				case AbilityFieldsEnum.ConditionLifeTotal:
					break;
				case AbilityFieldsEnum.ConditionLifeAmount:
					break;
				case AbilityFieldsEnum.NoPeek:
					break;
				case AbilityFieldsEnum.ImprintRevealed:
					break;
				case AbilityFieldsEnum.ValidTypes:
					break;
				case AbilityFieldsEnum.RememberControlled:
					break;
				case AbilityFieldsEnum.Bonus:
					break;
				case AbilityFieldsEnum.BonusProduced:
					break;
				case AbilityFieldsEnum.IsCursed:
					break;
				case AbilityFieldsEnum.RandomChosen:
					break;
				case AbilityFieldsEnum.Piles:
					break;
				case AbilityFieldsEnum.PreCostDesc:
					break;
				case AbilityFieldsEnum.UnlessType:
					break;
				default:
					break;
				}


			}

			return a;
		}


        public static Ability SpecialK(string str)
        {
            Ability a = null;

            string[] tmp = str.Split(new char[] { ' ' });

            
            switch (tmp[0])
            {
                case "CARDNAME":
                    break;
                case "Swampwalk":
                    a = new Ability(AbilityEnum.Swampwalk);
                    break;
                case "Islandwalk":
                    a = new Ability(AbilityEnum.Islandwalk);
                    break;
                case "Plainswalk":
                    a = new Ability(AbilityEnum.Plainswalk);
                    break;
                case "Forestwalk":
                    a = new Ability(AbilityEnum.Forestwalk);
                    break;
                case "Mountainwalk":
                    a = new Ability(AbilityEnum.Mountainwalk);
                    break;
                case "First":
                    if (tmp[1] != "Strike")
                        Debugger.Break();
                    return new Ability(AbilityEnum.FirstStrike);
                case "Flying":
                    a = new Ability(AbilityEnum.Flying);
                    break;
                case "Protection":
                    a = new Ability(AbilityEnum.Protection);
                    break;
                case "Vigilance":
                    a = new Ability(AbilityEnum.Vigilance);
                    break;
                case "Trample":
                    a = new Ability(AbilityEnum.Trample);
                    break;
                case "Intimidate":
                    a = new Ability(AbilityEnum.Intimidate);
                    break;
                case "Deathtouch":
                    a = new Ability(AbilityEnum.Deathtouch);
                    break;
                case "Unblockable":
                    a = new Ability(AbilityEnum.Unblockable);
                    break;
                case "Defender":
                    a = new Ability(AbilityEnum.Defender);
                    break;
                case "Haste":
                    a = new Ability(AbilityEnum.Haste);
                    break;
                case "Banding":
                    a = new Ability(AbilityEnum.Banding);
                    break;
                case "Bushido":
                    a = new Ability(AbilityEnum.Bushido);
                    break;
                case "Horsemanship":
                    a = new Ability(AbilityEnum.Horsemanship);
                    break;
                case "Enchant":
                    //AttachAbility aa = new AttachAbility();
                    //aa.ValidTargets.Value = (CardTypes)Enum.Parse(typeof(CardTypes),tmp[1],true);
                    return null;
                case "Cumulative":
                    break;
                case "Haunt":
                    break;
                case "TypeCycling":
                    break;
                case "Entwine":
                    break;
                case "Equip":
                    break;
                case "Reach":
                    break;
                case "Flashback":
                    break;
                case "Echo":
                    break;
                case "ETBReplacement":
                    break;
                case "Evolve":
                    break;
                case "Suspend":
                    break;
                case "Rampage":
                    break;
                case "Persist":
                    break;
                case "Kicker":
                    break;
                case "etbCounter":
                    break;
                case "Flash":
                    break;
                case "Evoke":
                    break;
                case "Conspire":
                    break;
                case "Lifelink":
                    break;
                case "Exalted":
                    break;
                case "Morph":
                    break;
                case "Cycling":
                    break;
                case "Shroud":
                    break;
                case "Fuse":
                    break;
                case "Buyback":
                    break;
                case "You":
                    break;
                case "CantBeBlockedBy":
                    break;
                case "Remove":
                    break;
                case "Unearth":
                    break;
                case "Fading":
                    break;
                case "Convoke":
                    break;
                case "Split":
                    break;
                case "Multikicker":
                    break;
                case "Graft":
                    break;
                case "Monstrosity":
                    break;
                case "At":
                    break;
                case "Modular":
                    break;
                case "Fear":
                    break;
                case "Sunburst":
                    break;
                case "Cascade":
                    break;
                case "Permanents":
                    break;
                case "PreventAllDamageBy":
                    break;
                case "Madness":
                    break;
                case "Annihilator":
                    break;
                case "Hexproof":
                    break;
                case "Storm":
                    break;
                case "Shadow":
                    break;
                case "Creatures":
                    break;
                case "Indestructible":
                    break;
                case "Vanishing":
                    break;
                case "Amplify":
                    break;
                case "Legendary":
                    break;
                case "Bestow":
                    break;
                case "Miracle":
                    break;
                case "Extort":
                    break;
                case "Bloodthirst":
                    break;
                case "Living":
                    break;
                case "Totem":
                    break;
                case "Level":
                    break;
                case "maxLevel":
                    break;
                case "Flanking":
                    break;
                case "Infect":
                    break;
                case "Splice":
                    break;
                case "If":
                    break;
                case "Soulshift":
                    break;
                case "Champion":
                    break;
                case "Wither":
                    break;
                case "Double":
                    break;
                case "Transmute":
                    break;
                case "CantBlock":
                    break;
                case "Phasing":
                    break;
                case "Provoke":
                    break;
                case "Undying":
                    break;
                case "Devour":
                    break;
                case "No":
                    break;
                case "MayEffectFromOpeningHand":
                    break;
                case "CantBeBlockedByAmount":
                    break;
                case "Prevent":
                    break;
                case "As":
                    break;
                case "Rebound":
                    break;
                case "Recover":
                    break;
                case "Play":
                    break;
                case "Dredge":
                    break;
                case "Fortify":
                    break;
                case "Soulbond":
                    break;
                case "Delve":
                    break;
                case "Desertwalk":
                    break;
                case "Each":
                    break;
                case "SpellCantTarget":
                    break;
                case "Nonbasic":
                    break;
                case "All":
                    break;
                case "Epic":
                    break;
                case "Damage":
                    break;
                case "Tribute":
                    break;
                case "Players":
                    break;
                case "AlternateAdditionalCost":
                    break;
                case "Replicate":
                    break;
                case "Hideaway":
                    break;
                case "CantEquip":
                    break;
                case "Snow":
                    break;
                case "Goblin":
                    break;
                case "Fox":
                    break;
                case "Moonfolk":
                    break;
                case "Rat":
                    break;
                case "Snake":
                    break;
                case "Reveal":
                    break;
                case "etbCounters":
                    break;
                case "Whenever":
                    break;
                case "Ripple":
                    break;
                default:
                    break;
            }
            return a;
        }
        
		//?
		public static List<string> strings = new List<string>();
        

        //public static bool operator ==(Ability a, EffectType e)
        //{
        //    if (a.AbilityType == e)
        //        return true;

        //    foreach (Effect ef in a.Effects)
        //    {
        //        if (ef.TypeOfEffect == e)
        //            return true;
        //    }
        //    return false;
        //}
        //public static bool operator !=(Ability a, EffectType e)
        //{
        //    if (a.AbilityType == e)
        //        return false;

        //    foreach (Effect ef in a.Effects)
        //    {
        //        if (ef.TypeOfEffect == e)
        //            return false;
        //    }
        //    return true;
        //}
    }

}
