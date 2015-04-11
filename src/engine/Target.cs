using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Magic3D
{
    public enum TargetType
    {
        Opponent,
        Player,
        Card,
        Permanent,
		Self,
		EnchantedBy,
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
        public MultiformAttribut<CardTypes> ValidCardTypes;
		public MultiformAttribut<CardTypes> HavingAttachedCards;
		public MultiformAttribut<Ability> HavingAbilities;
		public MultiformAttribut<Ability> WithoutAbilities;
		public MultiformAttribut<ManaTypes> ValidCardColors;
        public ControlerType Controler = ControlerType.All;
        public CombatImplication CombatState = CombatImplication.Unset;
        public NumericConstrain PowerConstrain;
        public NumericConstrain ToughnessConstrain;
		public bool CanBeTargetted = true;
		public MultiformAttribut<CardGroupEnum> ValidGroup;
        
        public CardTarget(TargetType tt = TargetType.Card)
        {
            TypeOfTarget = tt;
        }

		public override bool Accept (object _target)
		{
			CardInstance c = _target as CardInstance;
			if (c == null)
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
				//?????
			}else if (Controler == ControlerType.Opponent){
				//?????
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

		public override string ToString()
        {
            string tmp = "";
            if (ValidCardTypes.Count == 0)
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

    public class Target
    {
        public TargetType TypeOfTarget;
		//should have defining source to know opponent...

		public virtual bool Accept(object _target){
			switch (TypeOfTarget) {
			case TargetType.Opponent:
				Player p = _target as Player;
				if (p == null)
					return false;				
				break;
			case TargetType.Player:
				return (_target is Player);
				break;
			case TargetType.Card:
				return (_target is CardInstance);
				break;
			case TargetType.Permanent:
				CardInstance c = _target as CardInstance;
				if (c == null)
					return false;
				return (c.CurrentGroup.GroupName == CardGroupEnum.InPlay);
				break;
			case TargetType.Self:
			case TargetType.EnchantedBy:
				return (_target is CardInstance);
				break;
			}
			return false;
		}
//		static List<string> list = new List<String> ();

        public static MultiformAttribut<Target> ParseTargets(string str)
        {
            MultiformAttribut<Target> result = new MultiformAttribut<Target>(AttributeType.Choice);
//			using (Stream stream = new FileStream(@"/mnt/data/effects.txt", FileMode.Append)){
//				using (StreamWriter tw = new StreamWriter (stream)) {
//					
//					if (!list.Contains (str)) {
//						list.Add (str);
//						tw.WriteLine (str);
//					}
//				}
//			}
			string[] tmp = str.Trim().Split(new char[] { ',' });
			foreach (string t in tmp)
			{
				if (string.IsNullOrWhiteSpace(t))
					continue;

				string[] cardTypes = t.Trim().Split(new char[] { '.', '+' });

				switch (cardTypes[0].Trim())
				{
				case "Opponent":
					result |= TargetType.Opponent;
					break;
				case "Player":
					result |= TargetType.Player;
					break;
				default:
					CardTarget ctar = new CardTarget();

					foreach (string ct in cardTypes)
					{
						switch (ct) {
						case "Card":
							break;
						case "Permanent":
							ctar.TypeOfTarget = TargetType.Permanent;
							break;
						case "YouCtrl":
							ctar.Controler = ControlerType.You;
							break;						
						case "OppCtrl":
							ctar.Controler = ControlerType.Opponent;
							break;
						case "Other":
							ctar.CanBeTargetted = false;
							break;						
						case "attacking":
							ctar.CombatState = CombatImplication.Attacking;
							break;
						case "blocking":
							ctar.CombatState = CombatImplication.Blocking;
							break;
						case "Self":
							ctar.TypeOfTarget = TargetType.Self;
							break;
						case "EnchantedBy":
							ctar.TypeOfTarget = TargetType.EnchantedBy;
							break;
						case "equipped":
							ctar.HavingAttachedCards += CardTypes.Equipment;
							break;
						case "White":
							ctar.ValidCardColors += ManaTypes.White;
							break;
						default:							
							#region ability inclusion/exclusion
							if (ct.StartsWith("without")){
								ctar.WithoutAbilities += Ability.SpecialK(ct.Substring(7));
								break;
							}
							if (ct.StartsWith ("with")){
								ctar.HavingAbilities += Ability.SpecialK(ct.Substring(4));
								break;
							}
							#endregion
							#region numeric contrain
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

								break;
							}
							#endregion
							#region card types constrains
							CardTypes cts;
							if (Enum.TryParse<CardTypes>(ct, true, out cts))
								ctar.ValidCardTypes += (CardTypes)Enum.Parse(typeof(CardTypes), ct, true);
							else
								Debug.WriteLine ("Unknow card type: " + ct);
							#endregion
							break;
						}							
					}
					result |= ctar;
					break;
				}
            }
				
            return result;
        }

		#region Operators
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
		#endregion

		public override string ToString()
        {
            return TypeOfTarget.ToString();
        }
    }
}
