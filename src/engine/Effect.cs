using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;

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

        public static Effect Parse(string s, ref Ability a)
        {
            string[] tmp = s.Split(new char[] { '|' });

            foreach (string t in tmp)
            {
                string[] tmp2 = t.Split(new char[] { '$' });
                string value = tmp2[1].Trim();
                int v;
                switch (tmp2[0].Trim())
                {
                    case "Mode":
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
                        a.Effects.Add(new NumericEffect
                        {
                            TypeOfEffect = EffectType.AddPower,
                            NumericValue = v
                        });
                        break;
                    case "AddToughness":
                        if (!int.TryParse(value, out v))
                            break;
                        a.Effects.Add(new NumericEffect
                        {
                            TypeOfEffect = EffectType.AddTouchness,
                            NumericValue = v
                        });
                        break;                        
                    case "SetPower":
                        if (!int.TryParse(value, out v))
                            break;
                        a.Effects.Add(new NumericEffect
                        {
                            TypeOfEffect = EffectType.SetPower,
                            NumericValue = v
                        });
                        break;
                    case "SetToughness":
                        if (!int.TryParse(value, out v))
                            break;
                        a.Effects.Add(new NumericEffect
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

            return new Effect();
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
                    if (TrigEnd.Card != null && TrigEnd.Card == arg.Source)
                    {
                        MagicEngine.MagicEvent -= Effect_MagicEvent;
                        ContainingList.RemoveEffect(this);
                    }
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
    }
    [Serializable]
    public class AbilityEffect : Effect
    {
        public Ability Ability;
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
