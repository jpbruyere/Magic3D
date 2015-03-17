using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic3D
{
    public enum AttributeType
    {
        Choice,
        Composite,
        Exclude
    }
    public class MultiformAttribut<T>
    {

        //public List<Effect> Effects = new List<Effect>();

        public List<T> Values = new List<T>();
        public AttributeType attributeType;

        public MultiformAttribut(AttributeType at = AttributeType.Composite)
        {
            attributeType = at;
        }
        public int Count
        {
            get { return Values.Count; }
        }
        public T Value
        {
            get
            {
                if (Values.Count == 0)
                    return default(T);
                if (Values.Count == 1)
                    return Values[0];
                //?
                return Values.FirstOrDefault();
            }
            set
            {
                Values.Add(value);
            }
        }
        public bool Contains(T a)
        {

            return Values.Contains(a) ? true : false;
        }
        //public static implicit operator T(MultiformAttribut<T> a)
        //{

        //}
        public static bool operator ==(MultiformAttribut<T> a1, MultiformAttribut<T> a2)
        {
            foreach (T i in a1.Values)
            {
                foreach (T j in a2.Values)
                {
                    if (EqualityComparer<T>.Default.Equals(i, j))
                    {
                        //if (a1.attributeType == AttributeType.Choice)
                            return a1.attributeType == AttributeType.Exclude ? false : true;
                    }
                    else if (a2.attributeType == AttributeType.Composite && a2.Count > 1)
                        return false;
                }

            }
            return a1.attributeType == AttributeType.Exclude ? true : false;
        }
        public static bool operator !=(MultiformAttribut<T> a1, MultiformAttribut<T> a2)
        {
            foreach (T i in a1.Values)
            {
                foreach (T j in a2.Values)
                {
                    if (EqualityComparer<T>.Default.Equals(i, j))
                    {
                        if (a2.attributeType == AttributeType.Choice)
                            return a1.attributeType == AttributeType.Exclude ? true : false;
                    }
                    else if (a2.attributeType == AttributeType.Composite && a2.Count > 1)
                        return true;
                }

            }
            return a1.attributeType == AttributeType.Exclude ? false : true;
        }
        public static bool operator ==(MultiformAttribut<T> a, T v)
        {
            foreach (T i in a.Values)
            {
                if (EqualityComparer<T>.Default.Equals(i, v))
                    return true;
            }
            return false;
        }
        public static bool operator !=(MultiformAttribut<T> a, T v)
        {
            foreach (T i in a.Values)
            {
                if (EqualityComparer<T>.Default.Equals(i, v))
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            if (Count == 0)
                return "empty";

            string tmp = "";
            string separator = " ";
            if (attributeType == AttributeType.Choice)
                separator = ",";

            foreach (T i in Values)
            {
                tmp += i.ToString() + separator;
            }
            return tmp.Substring(0, tmp.Length - 1);
        }
    }
}
