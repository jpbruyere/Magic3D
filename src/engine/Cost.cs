﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic3D
{
    public enum CostTypes
    {
        Unset,
        Mana,
        Tap,
        Untap,
        Discard,
        Subtract,
        Sacrifice,
        Exile,
        Composite
    }
    public class OtherCost : Cost
    {
        public int Count = 0;
        public MultiformAttribut<string> ValidTarget = new MultiformAttribut<string>();

        public OtherCost(CostTypes _type)
            : base(_type)
        { }
        public OtherCost(CostTypes _type, int _count, MultiformAttribut<string> _valid)
            : base(_type)
        {
            Count = _count;
            ValidTarget = _valid;
        }
        
    }

    public class Cost
    {
        public CostTypes CostType;

        public Cost()
        { }
        public Cost(CostTypes _type)
        {
            CostType = _type;
        }
        public static implicit operator Cost(CostTypes ct)
        {
            return new Cost(ct);
        }
        public static Cost operator +(Cost c1, Cost c2)
        {
            Mana m = c1 as Mana;
            Mana n = c2 as Mana;
            Costs cst1 = c1 as Costs;
            if (cst1 != null)
            {
                for (int i = 0; i < cst1.CostList.Count; i++)
                {
                    Mana cst1m = cst1.CostList[i] as Mana;
                    if (cst1m != null)
                    {
                        if (cst1m.IsSameType(n))
                        {
                            cst1m.count += n.count;
                            return cst1;
                        }
                    }
                }
            
            }

            if (!(m == null || n == null))
            {
                if (m.IsSameType(n))
                {
                    m.count += n.count;
                    return m;
                }
            }

            if (c1 == null)
                if (c2 == null)
                    return null;
                else
                    return c2;
            else if (c2 == null)
                return c1;

            Costs sum = new Costs();
            sum += c1;
            sum += c2;
            return sum;
        }
		public static Cost operator -(Cost c1, Cost c2)
		{
			Mana m = c1 as Mana;
			Mana n = c2 as Mana;
			Costs cst1 = c1 as Costs;
			if (cst1 != null)
			{
				for (int i = 0; i < cst1.CostList.Count; i++)
				{
					Mana cst1m = cst1.CostList[i] as Mana;
					if (cst1m != null)
					{
						if (cst1m.IsSameType(n))
						{
							cst1m.count -= n.count;
							return cst1;
						}
					}
				}

			}

			if (!(m == null || n == null))
			{
				if (m.IsSameType(n))
				{
					m.count += n.count;
					return m;
				}
			}

			if (c1 == null)
			if (c2 == null)
				return null;
			else
				return c2;
			else if (c2 == null)
				return c1;

			Costs sum = new Costs();
			sum += c1;
			sum += c2;
			return sum;
		}
        
		public static bool operator <(Cost c1, Cost c2)
        {
            if (c2 == null)
                return false;
            Cost left = c1.Clone();
            Cost right = c2.Clone();
            Cost result = left.Pay(ref right);
            return result == null ? true : false;
        }
        public static bool operator >(Cost c1, Cost c2)
        {
            if (c1 == null)
                return false;
            Cost left = c2.Clone();
            Cost right = c1.Clone();
            return left.Pay(ref right) == null ? true : false;
        }
        public static Cost Parse(string costString)
        {
            if (costString.ToLower() == "no cost")
                return null;
            if (costString.ToLower() == "any")
                return new Mana(ManaTypes.Any);
            if (costString.ToLower() == "combo any")
                return new Mana(ManaTypes.ComboAny);

            Costs sum = new Costs();

            string[] tmp = costString.Split(new char[] { });

            foreach (string c in tmp)
            {
                if (string.IsNullOrWhiteSpace(c))
                    continue;

                switch (c)
                {
                    case "T":
                        sum += new Cost(CostTypes.Tap);
                        continue;
                }

                if (c.StartsWith("Discard"))
                {
                    
                }

                string number = "";
                ManaChoice choice = new ManaChoice();

                for (int i = 0; i < c.Length; i++)
                {
                    if (char.IsDigit(c[i]))
                    {
                        number += c[i];

                        if (i < c.Length - 1)
                            if (char.IsDigit(c[i + 1]))
                                continue;

                        choice += new Mana(int.Parse(number));
                        number = null;
                    }
                    else
                    {
                        switch (c[i])
                        {
                            case 'W':
                                choice += ManaTypes.White;
                                break;
                            case 'R':
                                choice += ManaTypes.Red;
                                break;
                            case 'G':
                                choice += ManaTypes.Green;
                                break;
                            case 'B':
                                choice += ManaTypes.Black;
                                break;
                            case 'U':
                                choice += ManaTypes.Blue;
                                break;
                            case 'P':
                                choice += ManaTypes.Life;
                                break;
                            case 'X':
                                choice += new Mana(-1);
                                break;
                            case '/':
                                continue;
                            default:
                                break;
                        }
                    }
                }

                if (choice.Manas.Count == 1)
                    sum += choice.Manas[0];
                else
                    sum += choice;
            }

            if (sum.CostList.Count == 0)
                return null;
            if (sum.CostList.Count == 1)
                return sum.CostList[0];
            else
                return sum;
            return sum;
        }

        public virtual bool Contains(Cost c)
        {
            Costs cst = this as Costs;
            if (cst != null)
            {
                foreach (Cost cc in cst.CostList)
                {
                    if (cc.Contains(c))
                        return true;
                }
                return false;
            }
            ManaChoice mc = this as ManaChoice;
            if (mc != null)
            {
                foreach (Mana i in mc.Manas)
                {
                    if (i.Contains(c))
                        return true;
                }
                return false;
            }
            Mana m = this as Mana;
            if (m != null)
            {
                Mana cm = c as Mana;
                if (cm == null)
                    return false;
                if ((cm.TypeOfMana == m.TypeOfMana || 
                    (cm.TypeOfMana == ManaTypes.Colorless && m.TypeOfMana <= ManaTypes.Colorless) ||
                    (m.TypeOfMana == ManaTypes.Colorless && cm.TypeOfMana <= ManaTypes.Colorless) )&&
                    m.count >= cm.count)
                    return true;
                else
                    return false;                
            }               
            return this == c ? true : false;
        }
        public virtual bool Contains(CostTypes ct)
        {
            return this.CostType == ct ? true : false;
        }
        public virtual Cost Pay(ref Cost c)
        {
            Cost result = null;

            if (c != null)
            {
                Costs cl = c as Costs;
                if (cl != null)
                {
                    for (int i = 0; i < cl.CostList.Count; i++)
                    {
                        Cost cli = cl.CostList[i];
                        if (Pay(ref cli) == null)
                        {
                            cl.CostList.RemoveAt(i);
                            return null;
                        }
                    }
                }

                if (CostType == c.CostType)
                {
                    c = null;
                    return null;
                }
            }
            return this;
        }
        public virtual Cost Clone()
        {
            return new Cost
            {
                CostType = this.CostType
            };
        }
        public override string ToString()
        {
            switch (CostType)
            {
                case CostTypes.Unset:
                case CostTypes.Mana:
                case CostTypes.Composite:
                    return "erreur => " + CostType.ToString();
                case CostTypes.Tap:
                    return "T";
            }
            return "erreur";
        }
    }
    public class Costs : Cost
    {
        public List<Cost> CostList = new List<Cost>();
        public Costs()
            : base(CostTypes.Composite)
        {
        }

        public bool IsSameType(Cost m)
        {
            return m.CostType == this.CostType ? true : false;//??????
        }
        public override bool Contains(Cost c)
        {
            foreach (Cost ct in CostList)
                if (ct.Contains(c))
                    return true;
            return false;
        }
        public override bool Contains(CostTypes t)
        {
            foreach (Cost ct in CostList)
                if (ct.Contains(t))
                    return true;
            return false;
        }
        public override Cost Pay(ref Cost c)
        {
            if (c.CostType == CostTypes.Composite)
            {
                Costs composite = c.Clone() as Costs;
                Cost result = this.Clone();
                for (int i = 0; i < composite.CostList.Count; i++)
                {
                    Cost tmp = composite.CostList[i];                                        

                    result = result.Pay(ref tmp);

                    if (tmp == null)
                    {
                        composite.CostList.RemoveAt(i);
                        i--;
                    }

                    if (result == null)
                        return null;

                }
                return result;
            }
            else
            {

                for (int i = 0; i < CostList.Count; i++)
                {
                    Cost result = CostList[i].Pay(ref c);
                    if (result == null)
                    {
                        CostList.RemoveAt(i);
                        i--;
                    }
                    else
                        CostList[i] = result;
                }
            }
            return CostList.Count == 0 ? null : this;
        }
        public override Cost Clone()
        {
            Costs c = new Costs();
            foreach (Cost ct in CostList)
                c.CostList.Add(ct.Clone());
            return c;
        }
        public static Costs operator +(Costs sum, Cost c)
        {
            Mana m = c as Mana;
            if (m != null)
            {
                for (int i = 0; i < sum.CostList.Count; i++)
                {
                    Mana mm = sum.CostList[i] as Mana;

                    if (mm == null)
                        continue;

                    if (mm.IsSameType(m))
                    {
                        sum.CostList[i] += m;
                        return sum;
                    }
                }
            }
            sum.CostList.Add(c);
            return sum;
        }

        //public static ManaSum operator +(ManaSum sum, ManaChoice choice)
        //{
        //    if (choice.Manas.Count == 1)
        //        return sum + choice.Manas[0];

        //    for (int i = 0; i < sum.Manas.Count; i++)
        //    {
        //        if (sum.Manas[i].IsSameType(choice))
        //        {
        //            sum.Manas[i] += choice;
        //            return sum;
        //        }
        //    }
        //    sum.Manas.Add(choice);
        //    return sum;
        //}

        public override string ToString()
        {
            if (CostList.Count == 0)
                return "EmptySum";

            string tmp = CostList[0].ToString();

            for (int i = 1; i < CostList.Count; i++)
                tmp += " " + CostList[i].ToString();

            return tmp;
        }
    }
}
