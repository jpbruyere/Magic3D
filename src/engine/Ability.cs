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

    //public class AttachAbility : Ability
    //{

    //    public AttachAbility()
    //        : base(AbilityEnum.Attach)
    //    {
    //    }
    //}

    public class Ability : Spell
    {
        public AbilityEnum AbilityType = AbilityEnum.Unset;
        //public int RequiredTargetCount;
        int _requiredTargetCount = 0;
        public override int RequiredTargetCount
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

        
        public List<Effect> Effects = new List<Effect>();
        public Cost ActivationCost;
        public string Description = "";

        MultiformAttribut<Target> _validTargets;
        public override MultiformAttribut<Target> ValidTargets
        {
            get { return _validTargets; }
            set { _validTargets = value; }
        }


        List<Object> _selectedTargets = new List<object>();        
        public override List<Object> SelectedTargets
        {
            get { return _selectedTargets; }
            set
            {
                _selectedTargets = value;
            }
        }



        public Ability()
        {
        }

        public Ability(AbilityEnum a)
        {
            AbilityType = a;
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
        public static List<string> strings = new List<string>();
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

                AbilitiesVariables varName = (AbilitiesVariables)Enum.Parse(typeof(AbilitiesVariables), ab.Substring(0, dollarPos), true);
                string value = ab.Substring(dollarPos + 1).Trim();

                Effect e = null;
                
                switch (varName)
                {
                    case AbilitiesVariables.SP:
                    case AbilitiesVariables.AB:
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
                    case AbilitiesVariables.Cost:
                        a.ActivationCost = Cost.Parse(value);
                        break;
                    case AbilitiesVariables.ValidTgts:
                        a.ValidTargets = Target.ParseTargets(value);
                        break;
                    case AbilitiesVariables.Mode:
                        switch (value.Trim().ToLower())
                        {
                            case "continuous":

                                break;
                            default:
                                break;
                        }
                        break;
                    case AbilitiesVariables.NumCards:
                        break;
                    case AbilitiesVariables.References:
                        break;
                    case AbilitiesVariables.SpellDescription:
                        break;
                    case AbilitiesVariables.Produced:
                        (a as ManaAbility).ProducedMana = Mana.Parse(value);
                        break;
                    case AbilitiesVariables.NumDef:                        
                        if (!int.TryParse(value, out v))
                            break;
                        a.Effects.Add(new NumericEffect
                        {
                            TypeOfEffect = EffectType.AddTouchness,
                            NumericValue = v
                        });
                        break;
                    case AbilitiesVariables.AILogic:
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
                    case AbilitiesVariables.Name:
                        break;
                    case AbilitiesVariables.Power:                        
                        if (!int.TryParse(value, out v))
                            break;
                        a.Effects.Add(new NumericEffect
                        {
                            TypeOfEffect = EffectType.SetPower,
                            NumericValue = v
                        });
                        break;
                    case AbilitiesVariables.Toughness:
                        if (!int.TryParse(value, out v))
                            break;
                        a.Effects.Add(new NumericEffect
                        {
                            TypeOfEffect = EffectType.SetTouchness,
                            NumericValue = v
                        });
                        break;
                    case AbilitiesVariables.StaticAbilities:
                        break;
                    case AbilitiesVariables.RememberObjects:
                        break;
                    case AbilitiesVariables.SubAbility:
                        break;
                    case AbilitiesVariables.TargetType:
                        break;
                    case AbilitiesVariables.TgtPrompt:
                        break;
                    case AbilitiesVariables.ValidCards:
                        break;
                    case AbilitiesVariables.KW:
                        break;
                    case AbilitiesVariables.LifeAmount:
                        break;
                    case AbilitiesVariables.Amount:
                        break;
                    case AbilitiesVariables.Choices:
                        break;
                    case AbilitiesVariables.NumDmg:
                        break;
                    case AbilitiesVariables.Origin:
                        break;
                    case AbilitiesVariables.Destination:
                        break;
                    case AbilitiesVariables.NumAtt:
                        break;
                    case AbilitiesVariables.Defined:
                        break;
                    case AbilitiesVariables.Triggers:
                        break;
                    case AbilitiesVariables.SVars:
                        break;
                    case AbilitiesVariables.ImprintCards:
                        break;
                    case AbilitiesVariables.ActivationPhases:
                        break;
                    case AbilitiesVariables.RepeatPlayers:
                        break;
                    case AbilitiesVariables.RepeatSubAbility:
                        break;
                    case AbilitiesVariables.IsCurse:
                        break;
                    case AbilitiesVariables.StackDescription:
                        break;
                    case AbilitiesVariables.TokenAmount:
                        break;
                    case AbilitiesVariables.TokenName:
                        break;
                    case AbilitiesVariables.TokenTypes:
                        break;
                    case AbilitiesVariables.TokenOwner:
                        break;
                    case AbilitiesVariables.TokenColors:
                        break;
                    case AbilitiesVariables.TokenPower:
                        break;
                    case AbilitiesVariables.TokenToughness:
                        break;
                    case AbilitiesVariables.ChangeType:
                        break;
                    case AbilitiesVariables.ChangeNum:
                        break;
                    case AbilitiesVariables.GainControl:
                        break;
                    case AbilitiesVariables.LoseControl:
                        break;
                    case AbilitiesVariables.Untap:
                        break;
                    case AbilitiesVariables.AddKWs:
                        break;
                    case AbilitiesVariables.RepeatOptional:
                        break;
                    case AbilitiesVariables.Keywords:
                        break;
                    case AbilitiesVariables.TokenImage:
                        break;
                    case AbilitiesVariables.TokenKeywords:
                        break;
                    case AbilitiesVariables.DigNum:
                        break;
                    case AbilitiesVariables.ConditionCheckSVar:
                        break;
                    case AbilitiesVariables.ConditionSVarCompare:
                        break;
                    case AbilitiesVariables.ReplacementEffects:
                        break;
                    case AbilitiesVariables.Stackable:
                        break;
                    case AbilitiesVariables.ChangeValid:
                        break;
                    case AbilitiesVariables.DestinationZone:
                        break;
                    case AbilitiesVariables.RememberChanged:
                        break;
                    case AbilitiesVariables.TargetMin:
                        break;
                    case AbilitiesVariables.TargetMax:
                        break;
                    case AbilitiesVariables.AllCounterTypes:
                        break;
                    case AbilitiesVariables.NoReg:
                        break;
                    case AbilitiesVariables.AnyPlayer:
                        break;
                    case AbilitiesVariables.Optional:
                        break;
                    case AbilitiesVariables.Reveal:
                        break;
                    case AbilitiesVariables.LibraryPosition:
                        break;
                    case AbilitiesVariables.NoRegen:
                        break;
                    case AbilitiesVariables.RememberDamaged:
                        break;
                    case AbilitiesVariables.ValidDescription:
                        break;
                    case AbilitiesVariables.SorcerySpeed:
                        break;
                    case AbilitiesVariables.Mandatory:
                        break;
                    case AbilitiesVariables.Planeswalker:
                        break;
                    case AbilitiesVariables.CounterNum:
                        break;
                    case AbilitiesVariables.CounterType:
                        break;
                    case AbilitiesVariables.Ultimate:
                        break;
                    case AbilitiesVariables.TokenStaticAbilities:
                        break;
                    case AbilitiesVariables.TokenSVars:
                        break;
                    case AbilitiesVariables.Permanent:
                        break;
                    case AbilitiesVariables.ActivationLimit:
                        break;
                    case AbilitiesVariables.PlayerTurn:
                        break;
                    case AbilitiesVariables.SVar:
                        break;
                    case AbilitiesVariables.Type:
                        break;
                    case AbilitiesVariables.Expression:
                        break;
                    case AbilitiesVariables.WinSubAbility:
                        break;
                    case AbilitiesVariables.LoseSubAbility:
                        break;
                    case AbilitiesVariables.CostDesc:
                        break;
                    case AbilitiesVariables.RememberTargets:
                        break;
                    case AbilitiesVariables.RestrictValid:
                        break;
                    case AbilitiesVariables.DiscardValid:
                        break;
                    case AbilitiesVariables.Types:
                        break;
                    case AbilitiesVariables.RemoveCreatureTypes:
                        break;
                    case AbilitiesVariables.RememberMilled:
                        break;
                    case AbilitiesVariables.DestinationZone2:
                        break;
                    case AbilitiesVariables.IsPresent:
                        break;
                    case AbilitiesVariables.Colors:
                        break;
                    case AbilitiesVariables.OverwriteColors:
                        break;
                    case AbilitiesVariables.TokenHiddenKeywords:
                        break;
                    case AbilitiesVariables.Choser:
                        break;
                    case AbilitiesVariables.AITgts:
                        break;
                    case AbilitiesVariables.TgtZone:
                        break;
                    case AbilitiesVariables.AnnounceType:
                        break;
                    case AbilitiesVariables.Gains:
                        break;
                    case AbilitiesVariables.DefinedPlayer:
                        break;
                    case AbilitiesVariables.Chooser:
                        break;
                    case AbilitiesVariables.Unique:
                        break;
                    case AbilitiesVariables.Duration:
                        break;
                    case AbilitiesVariables.Hidden:
                        break;
                    case AbilitiesVariables.OriginChoice:
                        break;
                    case AbilitiesVariables.OriginAlternative:
                        break;
                    case AbilitiesVariables.AlternativeMessage:
                        break;
                    case AbilitiesVariables.PrecostDesc:
                        break;
                    case AbilitiesVariables.ActivationZone:
                        break;
                    case AbilitiesVariables.DividedAsYouChoose:
                        break;
                    case AbilitiesVariables.RememberCountered:
                        break;
                    case AbilitiesVariables.Announce:
                        break;
                    case AbilitiesVariables.Zone2:
                        break;
                    case AbilitiesVariables.CheckSVar:
                        break;
                    case AbilitiesVariables.Sacrifice:
                        break;
                    case AbilitiesVariables.RemoveTypes:
                        break;
                    case AbilitiesVariables.Activation:
                        break;
                    case AbilitiesVariables.sVars:
                        break;
                    case AbilitiesVariables.TokenTapped:
                        break;
                    case AbilitiesVariables.PresentCompare:
                        break;
                    case AbilitiesVariables.PresentZone:
                        break;
                    case AbilitiesVariables.ChoiceZone:
                        break;
                    case AbilitiesVariables.GameActivationLimit:
                        break;
                    case AbilitiesVariables.ValidPlayers:
                        break;
                    case AbilitiesVariables.UntilUntaps:
                        break;
                    case AbilitiesVariables.OpponentTurn:
                        break;
                    case AbilitiesVariables.RevealValid:
                        break;
                    case AbilitiesVariables.RememberRevealed:
                        break;
                    case AbilitiesVariables.XColor:
                        break;
                    case AbilitiesVariables.CharmNum:
                        break;
                    case AbilitiesVariables.RepeatCards:
                        break;
                    case AbilitiesVariables.Zone:
                        break;
                    case AbilitiesVariables.RememberChosen:
                        break;
                    case AbilitiesVariables.NoMove:
                        break;
                    case AbilitiesVariables.RememberLKI:
                        break;
                    case AbilitiesVariables.TargetValidTargeting:
                        break;
                    case AbilitiesVariables.UnlessCost:
                        break;
                    case AbilitiesVariables.Valid:
                        break;
                    case AbilitiesVariables.LibraryPosition2:
                        break;
                    case AbilitiesVariables.ChoiceTitle:
                        break;
                    case AbilitiesVariables.Abilities:
                        break;
                    case AbilitiesVariables.Tapped:
                        break;
                    case AbilitiesVariables.UnlessPayer:
                        break;
                    case AbilitiesVariables.UnlessAI:
                        break;
                    case AbilitiesVariables.TargetUnique:
                        break;
                    case AbilitiesVariables.TargetsFromSingleZone:
                        break;
                    case AbilitiesVariables.SacValid:
                        break;
                    case AbilitiesVariables.Radiance:
                        break;
                    case AbilitiesVariables.ConditionManaSpent:
                        break;
                    case AbilitiesVariables.NumTurns:
                        break;
                    case AbilitiesVariables.Separator:
                        break;
                    case AbilitiesVariables.ChosenPile:
                        break;
                    case AbilitiesVariables.UnchosenPile:
                        break;
                    case AbilitiesVariables.ColorOrType:
                        break;
                    case AbilitiesVariables.ReflectProperty:
                        break;
                    case AbilitiesVariables.Static:
                        break;
                    case AbilitiesVariables.RevealNumber:
                        break;
                    case AbilitiesVariables.TargetsWithDifferentControllers:
                        break;
                    case AbilitiesVariables.ConditionDefined:
                        break;
                    case AbilitiesVariables.ConditionPresent:
                        break;
                    case AbilitiesVariables.ConditionCompare:
                        break;
                    case AbilitiesVariables.SVarCompare:
                        break;
                    case AbilitiesVariables.NonBasicSpell:
                        break;
                    case AbilitiesVariables.staticAbilities:
                        break;
                    case AbilitiesVariables.AddsNoCounter:
                        break;
                    case AbilitiesVariables.RememberSacrificed:
                        break;
                    case AbilitiesVariables.RememberCounters:
                        break;
                    case AbilitiesVariables.ValidDesc:
                        break;
                    case AbilitiesVariables.MinCharmNum:
                        break;
                    case AbilitiesVariables.AllValid:
                        break;
                    case AbilitiesVariables.RememberUntapped:
                        break;
                    case AbilitiesVariables.AnyNumber:
                        break;
                    case AbilitiesVariables.TokenAbilities:
                        break;
                    case AbilitiesVariables.OptionalDecider:
                        break;
                    case AbilitiesVariables.Destroy:
                        break;
                    case AbilitiesVariables.NewState:
                        break;
                    case AbilitiesVariables.PreventionSubAbility:
                        break;
                    case AbilitiesVariables.ShieldEffectTarget:
                        break;
                    case AbilitiesVariables.OptionQuestion:
                        break;
                    case AbilitiesVariables.ImprintTargets:
                        break;
                    case AbilitiesVariables.SacMessage:
                        break;
                    case AbilitiesVariables.FromBottom:
                        break;
                    case AbilitiesVariables.Shuffle:
                        break;
                    case AbilitiesVariables.RememberDiscarded:
                        break;
                    case AbilitiesVariables.ValidZone:
                        break;
                    case AbilitiesVariables.WithoutManaCost:
                        break;
                    case AbilitiesVariables.Controller:
                        break;
                    case AbilitiesVariables.Image:
                        break;
                    case AbilitiesVariables.InstantSpeed:
                        break;
                    case AbilitiesVariables.RemoveKeywords:
                        break;
                    case AbilitiesVariables.RememberDestroyed:
                        break;
                    case AbilitiesVariables.ST:
                        break;
                    case AbilitiesVariables.DefinedCards:
                        break;
                    case AbilitiesVariables.DiscardMessage:
                        break;
                    case AbilitiesVariables.Equip:
                        break;
                    case AbilitiesVariables.AnyOpponent:
                        break;
                    case AbilitiesVariables.UpTo:
                        break;
                    case AbilitiesVariables.MaxFromEffect:
                        break;
                    case AbilitiesVariables.ValidCard:
                        break;
                    case AbilitiesVariables.ForgetOtherTargets:
                        break;
                    case AbilitiesVariables.NoPrevention:
                        break;
                    case AbilitiesVariables.AllType:
                        break;
                    case AbilitiesVariables.InvalidTypes:
                        break;
                    case AbilitiesVariables.HiddenKeywords:
                        break;
                    case AbilitiesVariables.ForgetOtherRemembered:
                        break;
                    case AbilitiesVariables.RememberCostCards:
                        break;
                    case AbilitiesVariables.ScryNum:
                        break;
                    case AbilitiesVariables.UntilYourNextUpkeep:
                        break;
                    case AbilitiesVariables.Imprint:
                        break;
                    case AbilitiesVariables.RegenerationTrigger:
                        break;
                    case AbilitiesVariables.ReplaceCardUID:
                        break;
                    case AbilitiesVariables.OverwriteTypes:
                        break;
                    case AbilitiesVariables.KeepSupertypes:
                        break;
                    case AbilitiesVariables.KeepCardTypes:
                        break;
                    case AbilitiesVariables.RemoveAllAbilities:
                        a.Effects.Add(new Effect(EffectType.LooseAllAbilities));                        
                        break;
                    case AbilitiesVariables.TargetsSingleTarget:
                        break;
                    case AbilitiesVariables.UseImprinted:
                        break;
                    case AbilitiesVariables.ChooseOrder:
                        break;
                    case AbilitiesVariables.UseAllOriginZones:
                        break;
                    case AbilitiesVariables.UnattachValid:
                        break;
                    case AbilitiesVariables.DamageSource:
                        break;
                    case AbilitiesVariables.ConditionDescription:
                        break;
                    case AbilitiesVariables.PeekAmount:
                        break;
                    case AbilitiesVariables.RevealOptional:
                        break;
                    case AbilitiesVariables.ActivationNumberSacrifice:
                        break;
                    case AbilitiesVariables.PumpZone:
                        break;
                    case AbilitiesVariables.OrColors:
                        break;
                    case AbilitiesVariables.RandomKeyword:
                        break;
                    case AbilitiesVariables.NoRepetition:
                        break;
                    case AbilitiesVariables.RememberTokens:
                        break;
                    case AbilitiesVariables.CopyOnce:
                        break;
                    case AbilitiesVariables.CopyCard:
                        break;
                    case AbilitiesVariables.FoundDestination:
                        break;
                    case AbilitiesVariables.RevealedDestination:
                        break;
                    case AbilitiesVariables.FoundLibraryPosition:
                        break;
                    case AbilitiesVariables.RevealedLibraryPosition:
                        break;
                    case AbilitiesVariables.RememberFound:
                        break;
                    case AbilitiesVariables.RepeatCheckSVar:
                        break;
                    case AbilitiesVariables.RepeatSVarCompare:
                        break;
                    case AbilitiesVariables.DefinedLandwalk:
                        break;
                    case AbilitiesVariables.RememberDrawn:
                        break;
                    case AbilitiesVariables.LoseAtEndStep:
                        break;
                    case AbilitiesVariables.DivideEvenly:
                        break;
                    case AbilitiesVariables.Flashback:
                        break;
                    case AbilitiesVariables.ActivationCardsInHand:
                        break;
                    case AbilitiesVariables.CopyIsColor:
                        break;
                    case AbilitiesVariables.AtRandom:
                        break;
                    case AbilitiesVariables.UntilHostLeavesPlay:
                        break;
                    case AbilitiesVariables.Deplete:
                        break;
                    case AbilitiesVariables.Phase:
                        break;
                    case AbilitiesVariables.Execute:
                        break;
                    case AbilitiesVariables.Defender:
                        break;
                    case AbilitiesVariables.AndOrValid:
                        break;
                    case AbilitiesVariables.DifferentNames:
                        break;
                    case AbilitiesVariables.RepeatCounters:
                        break;
                    case AbilitiesVariables.ControlledByTarget:
                        break;
                    case AbilitiesVariables.NewController:
                        break;
                    case AbilitiesVariables.RememberRemoved:
                        break;
                    case AbilitiesVariables.EachBasicType:
                        break;
                    case AbilitiesVariables.TargetsAtRandom:
                        break;
                    case AbilitiesVariables.Condition:
                        break;
                    case AbilitiesVariables.MaxRepeat:
                        break;
                    case AbilitiesVariables.ExileFaceDown:
                        break;
                    case AbilitiesVariables.AttachedTo:
                        break;
                    case AbilitiesVariables.AddsKeywords:
                        break;
                    case AbilitiesVariables.AtEOT:
                        break;
                    case AbilitiesVariables.RestRandomOrder:
                        break;
                    case AbilitiesVariables.Changetype:
                        break;
                    case AbilitiesVariables.Ninjutsu:
                        break;
                    case AbilitiesVariables.DestinationChoice:
                        break;
                    case AbilitiesVariables.ForgetRemembered:
                        break;
                    case AbilitiesVariables.Random:
                        break;
                    case AbilitiesVariables.RememberRemovedFromCombat:
                        break;
                    case AbilitiesVariables.RememberTargetedCard:
                        break;
                    case AbilitiesVariables.UntilYourNextTurn:
                        break;
                    case AbilitiesVariables.UntilEndOfCombat:
                        break;
                    case AbilitiesVariables.AnySupportedCard:
                        break;
                    case AbilitiesVariables.RandomCopied:
                        break;
                    case AbilitiesVariables.RandomNum:
                        break;
                    case AbilitiesVariables.ChoiceNum:
                        break;
                    case AbilitiesVariables.TargetControls:
                        break;
                    case AbilitiesVariables.RestrictFromZone:
                        break;
                    case AbilitiesVariables.RestrictFromValid:
                        break;
                    case AbilitiesVariables.Min:
                        break;
                    case AbilitiesVariables.Max:
                        break;
                    case AbilitiesVariables.TapOnLose:
                        break;
                    case AbilitiesVariables.DeclareAttackers:
                        break;
                    case AbilitiesVariables.DeclareBlockers:
                        break;
                    case AbilitiesVariables.Until:
                        break;
                    case AbilitiesVariables.DestroyTgt:
                        break;
                    case AbilitiesVariables.RememberSplicedOntoCounteredSpell:
                        break;
                    case AbilitiesVariables.Mitosis:
                        break;
                    case AbilitiesVariables.AddAbilities:
                        break;
                    case AbilitiesVariables.AddSVars:
                        break;
                    case AbilitiesVariables.ValidAttribute:
                        break;
                    case AbilitiesVariables.EffectOwner:
                        break;
                    case AbilitiesVariables.MayShuffle:
                        break;
                    case AbilitiesVariables.Monstrosity:
                        break;
                    case AbilitiesVariables.NoCall:
                        break;
                    case AbilitiesVariables.HeadsSubAbility:
                        break;
                    case AbilitiesVariables.TailsSubAbility:
                        break;
                    case AbilitiesVariables.StaticCommandCheckSVar:
                        break;
                    case AbilitiesVariables.StaticCommandSVarCompare:
                        break;
                    case AbilitiesVariables.AddsCounters:
                        break;
                    case AbilitiesVariables.UntilControllerNextUntap:
                        break;
                    case AbilitiesVariables.RandomChange:
                        break;
                    case AbilitiesVariables.TokenController:
                        break;
                    case AbilitiesVariables.NumCopies:
                        break;
                    case AbilitiesVariables.OwnerOnly:
                        break;
                    case AbilitiesVariables.RememberAllPumped:
                        break;
                    case AbilitiesVariables.TargetsFromDifferentZone:
                        break;
                    case AbilitiesVariables.TgtPromp:
                        break;
                    case AbilitiesVariables.UnlessResolveSubs:
                        break;
                    case AbilitiesVariables.RepeatDefined:
                        break;
                    case AbilitiesVariables.RepeatPresent:
                        break;
                    case AbilitiesVariables.RepeatCompare:
                        break;
                    case AbilitiesVariables.OtherwiseSubAbility:
                        break;
                    case AbilitiesVariables.ConditionTargetValidTargeting:
                        break;
                    case AbilitiesVariables.ConditionTargetsSingleTarget:
                        break;
                    case AbilitiesVariables.TargetRestriction:
                        break;
                    case AbilitiesVariables.CopyForEachCanTarget:
                        break;
                    case AbilitiesVariables.CanTargetPlayer:
                        break;
                    case AbilitiesVariables.Redistribute:
                        break;
                    case AbilitiesVariables.ListTitle:
                        break;
                    case AbilitiesVariables.TargetsWithoutSameCreatureType:
                        break;
                    case AbilitiesVariables.ContionSVarCompare:
                        break;
                    case AbilitiesVariables.isPresent:
                        break;
                    case AbilitiesVariables.ImprintTokens:
                        break;
                    case AbilitiesVariables.DamageDesc:
                        break;
                    case AbilitiesVariables.ConditionChosenColor:
                        break;
                    case AbilitiesVariables.SkipUntap:
                        break;
                    case AbilitiesVariables.RecordChoice:
                        break;
                    case AbilitiesVariables.ChoosePlayer:
                        break;
                    case AbilitiesVariables.TokenTriggers:
                        break;
                    case AbilitiesVariables.Source:
                        break;
                    case AbilitiesVariables.XCantBe0:
                        break;
                    case AbilitiesVariables.ReplacePlayerName:
                        break;
                    case AbilitiesVariables.ChangeSingleTarget:
                        break;
                    case AbilitiesVariables.NoReveal:
                        break;
                    case AbilitiesVariables.RememberTapped:
                        break;
                    case AbilitiesVariables.DelayedTrigger:
                        break;
                    case AbilitiesVariables.DestroyPermanent:
                        break;
                    case AbilitiesVariables.SkipReorder:
                        break;
                    case AbilitiesVariables.NoShuffle:
                        break;
                    case AbilitiesVariables.MaxRevealed:
                        break;
                    case AbilitiesVariables.Reference:
                        break;
                    case AbilitiesVariables.PhaseInOrOutAll:
                        break;
                    case AbilitiesVariables.ConditionLifeTotal:
                        break;
                    case AbilitiesVariables.ConditionLifeAmount:
                        break;
                    case AbilitiesVariables.NoPeek:
                        break;
                    case AbilitiesVariables.ImprintRevealed:
                        break;
                    case AbilitiesVariables.ValidTypes:
                        break;
                    case AbilitiesVariables.RememberControlled:
                        break;
                    case AbilitiesVariables.Bonus:
                        break;
                    case AbilitiesVariables.BonusProduced:
                        break;
                    case AbilitiesVariables.IsCursed:
                        break;
                    case AbilitiesVariables.RandomChosen:
                        break;
                    case AbilitiesVariables.Piles:
                        break;
                    case AbilitiesVariables.PreCostDesc:
                        break;
                    case AbilitiesVariables.UnlessType:
                        break;
                    default:
                        break;
                }


            }

            return a;
        }

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

    public enum AbilitiesVariables
    {
        SP,
        Cost,
        ValidTgts,
        Mode,
        NumCards,
        References,
        SpellDescription,
        AB,
        Produced,
        NumDef,
        AILogic,
        Name,
        StaticAbilities,
        RememberObjects,
        SubAbility,
        TargetType,
        TgtPrompt,
        ValidCards,
        KW,
        LifeAmount,
        Amount,
        Choices,
        NumDmg,
        Origin,
        Destination,
        NumAtt,
        Defined,
        Triggers,
        SVars,
        ImprintCards,
        ActivationPhases,
        RepeatPlayers,
        RepeatSubAbility,
        IsCurse,
        StackDescription,
        TokenAmount,
        TokenName,
        TokenTypes,
        TokenOwner,
        TokenColors,
        TokenPower,
        TokenToughness,
        ChangeType,
        ChangeNum,
        GainControl,
        LoseControl,
        Untap,
        AddKWs,
        RepeatOptional,
        Keywords,
        TokenImage,
        TokenKeywords,
        DigNum,
        ConditionCheckSVar,
        ConditionSVarCompare,
        ReplacementEffects,
        Stackable,
        ChangeValid,
        DestinationZone,
        RememberChanged,
        TargetMin,
        TargetMax,
        AllCounterTypes,
        NoReg,
        AnyPlayer,
        Optional,
        Reveal,
        LibraryPosition,
        NoRegen,
        RememberDamaged,
        ValidDescription,
        SorcerySpeed,
        Mandatory,
        Planeswalker,
        CounterNum,
        CounterType,
        Ultimate,
        TokenStaticAbilities,
        TokenSVars,
        Permanent,
        ActivationLimit,
        PlayerTurn,
        SVar,
        Type,
        Expression,
        WinSubAbility,
        LoseSubAbility,
        CostDesc,
        RememberTargets,
        RestrictValid,
        DiscardValid,
        Types,
        RemoveCreatureTypes,
        RememberMilled,
        DestinationZone2,
        IsPresent,
        Colors,
        OverwriteColors,
        TokenHiddenKeywords,
        Choser,
        AITgts,
        TgtZone,
        Power,
        Toughness,
        AnnounceType,
        Gains,
        DefinedPlayer,
        Chooser,
        Unique,
        Duration,
        Hidden,
        OriginChoice,
        OriginAlternative,
        AlternativeMessage,
        PrecostDesc,
        ActivationZone,
        DividedAsYouChoose,
        RememberCountered,
        Announce,
        Zone2,
        CheckSVar,
        Sacrifice,
        RemoveTypes,
        Activation,
        sVars,
        TokenTapped,
        PresentCompare,
        PresentZone,
        ChoiceZone,
        GameActivationLimit,
        ValidPlayers,
        UntilUntaps,
        OpponentTurn,
        RevealValid,
        RememberRevealed,
        XColor,
        CharmNum,
        RepeatCards,
        Zone,
        RememberChosen,
        NoMove,
        RememberLKI,
        TargetValidTargeting,
        UnlessCost,
        Valid,
        LibraryPosition2,
        ChoiceTitle,
        Abilities,
        Tapped,
        UnlessPayer,
        UnlessAI,
        TargetUnique,
        TargetsFromSingleZone,
        SacValid,
        Radiance,
        ConditionManaSpent,
        NumTurns,
        Separator,
        ChosenPile,
        UnchosenPile,
        ColorOrType,
        ReflectProperty,
        Static,
        RevealNumber,
        TargetsWithDifferentControllers,
        ConditionDefined,
        ConditionPresent,
        ConditionCompare,
        SVarCompare,
        NonBasicSpell,
        staticAbilities,
        AddsNoCounter,
        RememberSacrificed,
        RememberCounters,
        ValidDesc,
        MinCharmNum,
        AllValid,
        RememberUntapped,
        AnyNumber,
        TokenAbilities,
        OptionalDecider,
        Destroy,
        NewState,
        PreventionSubAbility,
        ShieldEffectTarget,
        OptionQuestion,
        ImprintTargets,
        SacMessage,
        FromBottom,
        Shuffle,
        RememberDiscarded,
        ValidZone,
        WithoutManaCost,
        Controller,
        Image,
        InstantSpeed,
        RemoveKeywords,
        RememberDestroyed,
        ST,
        DefinedCards,
        DiscardMessage,
        Equip,
        AnyOpponent,
        UpTo,
        MaxFromEffect,
        ValidCard,
        ForgetOtherTargets,
        NoPrevention,
        AllType,
        InvalidTypes,
        HiddenKeywords,
        ForgetOtherRemembered,
        RememberCostCards,
        ScryNum,
        UntilYourNextUpkeep,
        Imprint,
        RegenerationTrigger,
        ReplaceCardUID,
        OverwriteTypes,
        KeepSupertypes,
        KeepCardTypes,
        RemoveAllAbilities,
        TargetsSingleTarget,
        UseImprinted,
        ChooseOrder,
        UseAllOriginZones,
        UnattachValid,
        DamageSource,
        ConditionDescription,
        PeekAmount,
        RevealOptional,
        ActivationNumberSacrifice,
        PumpZone,
        OrColors,
        RandomKeyword,
        NoRepetition,
        RememberTokens,
        CopyOnce,
        CopyCard,
        FoundDestination,
        RevealedDestination,
        FoundLibraryPosition,
        RevealedLibraryPosition,
        RememberFound,
        RepeatCheckSVar,
        RepeatSVarCompare,
        DefinedLandwalk,
        RememberDrawn,
        LoseAtEndStep,
        DivideEvenly,
        Flashback,
        ActivationCardsInHand,
        CopyIsColor,
        AtRandom,
        UntilHostLeavesPlay,
        Deplete,
        Phase,
        Execute,
        Defender,
        AndOrValid,
        DifferentNames,
        RepeatCounters,
        ControlledByTarget,
        NewController,
        RememberRemoved,
        EachBasicType,
        TargetsAtRandom,
        Condition,
        MaxRepeat,
        ExileFaceDown,
        AttachedTo,
        AddsKeywords,
        AtEOT,
        RestRandomOrder,
        Changetype,
        Ninjutsu,
        DestinationChoice,
        ForgetRemembered,
        Random,
        RememberRemovedFromCombat,
        RememberTargetedCard,
        UntilYourNextTurn,
        UntilEndOfCombat,
        AnySupportedCard,
        RandomCopied,
        RandomNum,
        ChoiceNum,
        TargetControls,
        RestrictFromZone,
        RestrictFromValid,
        Min,
        Max,
        TapOnLose,
        DeclareAttackers,
        DeclareBlockers,
        Until,
        DestroyTgt,
        RememberSplicedOntoCounteredSpell,
        Mitosis,
        AddAbilities,
        AddSVars,
        ValidAttribute,
        EffectOwner,
        MayShuffle,
        Monstrosity,
        NoCall,
        HeadsSubAbility,
        TailsSubAbility,
        StaticCommandCheckSVar,
        StaticCommandSVarCompare,
        AddsCounters,
        UntilControllerNextUntap,
        RandomChange,
        TokenController,
        NumCopies,
        OwnerOnly,
        RememberAllPumped,
        TargetsFromDifferentZone,
        TgtPromp,
        UnlessResolveSubs,
        RepeatDefined,
        RepeatPresent,
        RepeatCompare,
        OtherwiseSubAbility,
        ConditionTargetValidTargeting,
        ConditionTargetsSingleTarget,
        TargetRestriction,
        CopyForEachCanTarget,
        CanTargetPlayer,
        Redistribute,
        ListTitle,
        TargetsWithoutSameCreatureType,
        ContionSVarCompare,
        isPresent,
        ImprintTokens,
        DamageDesc,
        ConditionChosenColor,
        SkipUntap,
        RecordChoice,
        ChoosePlayer,
        TokenTriggers,
        Source,
        XCantBe0,
        ReplacePlayerName,
        ChangeSingleTarget,
        NoReveal,
        RememberTapped,
        DelayedTrigger,
        DestroyPermanent,
        SkipReorder,
        NoShuffle,
        MaxRevealed,
        Reference,
        PhaseInOrOutAll,
        ConditionLifeTotal,
        ConditionLifeAmount,
        NoPeek,
        ImprintRevealed,
        ValidTypes,
        RememberControlled,
        Bonus,
        BonusProduced,
        IsCursed,
        RandomChosen,
        Piles,
        PreCostDesc,
        UnlessType
    }
}
