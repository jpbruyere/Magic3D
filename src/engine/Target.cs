using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Magic3D
{
    public enum TargetType
    {
        Opponent,
        Player,
        Card,
        Permanent,
    }
    public enum ControlerType
    {
        All,
        You,
        Opponent
    }
    public enum CombatImplication
    {
        Unset,
        Attacking,
        Blocking
    }
    public enum NumericRelations
    {
        Equal,
        Greater,
        Less,
        LessOrEqual,
        GreaterOrEqual,
        NotEqual
    }
    public class NumericConstrain
    {
        public NumericRelations Relation;
        public int Value;
    }

    public class CardTarget : Target
    {
        public MultiformAttribut<CardTypes> ValidCardTypes =
            new MultiformAttribut<CardTypes>(AttributeType.Composite);
        public MultiformAttribut<Ability> HavingAbilities =
            new MultiformAttribut<Ability>(AttributeType.Composite);
        public MultiformAttribut<Ability> WithoutAbilities =
            new MultiformAttribut<Ability>(AttributeType.Composite);
        public ControlerType Controler = ControlerType.All;
        public bool CanBeTargetted = true;
        public CombatImplication CombatState = CombatImplication.Unset;
        public NumericConstrain PowerConstrain;
        public NumericConstrain ToughnessConstrain;
        
        public CardTarget(TargetType tt = TargetType.Card)
        {
            TypeOfTarget = tt;
        }
        public override string ToString()
        {
            string tmp = "";
            if (ValidCardTypes.Count == 0)
                tmp += TypeOfTarget.ToString();
            else
                tmp += ValidCardTypes.ToString();

            if (HavingAbilities.Count > 0)
                tmp += " having " + HavingAbilities.ToString();

            if (WithoutAbilities.Count > 0)
                tmp += " without " + WithoutAbilities.ToString();

            return tmp;
        }
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

    public class Target
    {
        public TargetType TypeOfTarget;


        public static MultiformAttribut<Target> ParseTargets(string str)
        {
            MultiformAttribut<Target> result = new MultiformAttribut<Target>(AttributeType.Choice);

            string[] tmp = str.Trim().Split(new char[] { ',' });
            foreach (string t in tmp)
            {
                if (string.IsNullOrWhiteSpace(t))
                    continue;

                //string[] details = t.Trim().Split(new char[] { '.' });
                string[] cardTypes = t.Trim().Split(new char[] { '.', '+' });

                switch (cardTypes[0].Trim())
                {
                    case "Opponent":
                        result.Value = TargetType.Opponent;
                        break;
                    case "Player":
                        result.Value = TargetType.Player;
                        break;
                    default:
                        CardTarget ctar = new CardTarget();
                        foreach (string ct in cardTypes)
                        {
                            if (ct == "Card")
                            {
                                continue;
                            }
                            if (ct == "Permanent")
                            {
                                ctar.TypeOfTarget = TargetType.Permanent;
                                continue;
                            }
                            if (ct == "YouCtrl")
                            {
                                ctar.Controler = ControlerType.You;
                                continue;
                            }
                            if (ct == "OppCtrl")
                            {
                                ctar.Controler = ControlerType.Opponent;
                                continue;
                            }
                            if (ct == "Other")
                            {
                                ctar.CanBeTargetted = false;
                                continue;
                            }
                            if (ct == "attacking")
                            {
                                ctar.CombatState = CombatImplication.Attacking;
                                continue;
                            }
                            if (ct == "blocking")
                            {
                                ctar.CombatState = CombatImplication.Blocking;
                                continue;
                            }
                            if (ct.StartsWith("without"))
                            {
                                ctar.WithoutAbilities.Value = Ability.SpecialK(ct.Substring(7));
                                continue;
                            }
                            if (ct.StartsWith("with"))
                            {
                                ctar.HavingAbilities.Value = Ability.SpecialK(ct.Substring(4));
                                continue;
                            }

                            NumericConstrain nc = null;
                            string strTmp = "";
                            if (ct.ToLower().StartsWith("power"))
                            {
                                ctar.PowerConstrain = new NumericConstrain();
                                nc = ctar.PowerConstrain;
                                strTmp = ct.Substring(5);
                            }
                            else if (ct.ToLower().StartsWith("toughness"))
                            {
                                ctar.ToughnessConstrain = new NumericConstrain();
                                nc = ctar.ToughnessConstrain;
                                strTmp = ct.Substring(9);
                            }

                            if (nc != null)
                            {
                                string strRelation = strTmp.Substring(0, 2);
                                switch (strRelation)
                                {
                                    case "EQ":
                                        nc.Relation = NumericRelations.Equal;
                                        break;
                                    case "LT":
                                        nc.Relation = NumericRelations.Less;
                                        break;
                                    case "LE":
                                        nc.Relation = NumericRelations.LessOrEqual;
                                        break;
                                    case "GT":
                                        nc.Relation = NumericRelations.Greater;
                                        break;
                                    case "GE":
                                        nc.Relation = NumericRelations.GreaterOrEqual;
                                        break;
                                    case "NE":
                                        nc.Relation = NumericRelations.NotEqual;
                                        break;
                                    default:
                                        break;
                                }
                                strTmp = strTmp.Substring(2);

                                if (strTmp != "X" && !string.IsNullOrWhiteSpace(strTmp))
                                    nc.Value = int.Parse(strTmp);

                                continue;
                            }

                            CardTypes cts;
                            if (Enum.TryParse<CardTypes>(ct, true, out cts))
                                ctar.ValidCardTypes.Value = (CardTypes)Enum.Parse(typeof(CardTypes), ct, true);
                            //else
                            //    cts = CardTypes.Archon;
                        }

                        result.Value = ctar;

                        break;
                }


            }


            return result;
        }

        public static implicit operator Target(TargetType tt)
        {
            return tt == TargetType.Card || tt == TargetType.Permanent ?
                new Target { TypeOfTarget = tt } :
                new CardTarget { TypeOfTarget = tt };
        }
        public static bool operator ==(Target ct, CardTypes t)
        {
            return ct is CardTarget ? (ct as CardTarget) == t : false;
        }
        public static bool operator !=(Target ct, CardTypes t)
        {
            return ct is CardTarget ? (ct as CardTarget) != t : true;
        }
        public override string ToString()
        {
            return TypeOfTarget.ToString();
        }
    }
}
