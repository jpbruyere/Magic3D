using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Magic3D
{
	public class SVarToResolve
	{
		public static Dictionary<string,List<SVarToResolve>> UnresolvedSVars = new Dictionary<string, List<SVarToResolve>>();
		//public string id;
		public object instance;
		public MemberInfo member;

		SVarToResolve (object _instance, MemberInfo _member)
		{
			instance = _instance;
			member = _member;
		}
		public static void RegisterSVar(string _id, object _instance, MemberInfo _member)
		{
			if (!UnresolvedSVars.ContainsKey (_id))
				UnresolvedSVars.Add (_id, new List<SVarToResolve> ());
			
			UnresolvedSVars [_id].Add (new SVarToResolve (_instance, _member));
		}

		public static bool TryToParseAndSetValue(string _id, string datas){
			if (!SVarToResolve.UnresolvedSVars.ContainsKey (_id))
				return false;
			
			List<SVarToResolve> svars = UnresolvedSVars [_id];
			object o = svars.FirstOrDefault ().instance;
			object value = null;

			if (o is Ability)
				value = Ability.Parse (datas);
			if (o is Trigger)
				value = Ability.Parse (datas, o as Trigger);
			else {
				string[] tmp = datas.Split ('$');
				switch (tmp[0]) {
				case "Count":
					CardCounter cc = new CardCounter ();
					string[] div = tmp [1].Split ('/');
					string[] tmp2 = div [0].Split (' ');
					if (tmp2 [0] == "Valid")
						cc.CardsToCount = Target.ParseTargets (tmp2 [1]);
					if (div.Count() == 2) {
						tmp2 = div [1].Split ('.');
						int v = 0;
						if (tmp2 [0] == "Times") {
							if (int.TryParse (tmp2 [1], out v))
								cc.Multiplier = v;
							else
								SVarToResolve.RegisterSVar(tmp2 [1], cc, cc.GetType().GetField("Multiplier"));							
						}
					}
					value = cc;
					break;
				default:
					Debug.WriteLine ("unandle svar: " + datas);
					break;
				}
			}

			return TryToSetValue (_id, value);
		}
		public static bool TryToSetValue(string _id, object _value)
		{
			if (!UnresolvedSVars.ContainsKey (_id))
				return false;

			List<SVarToResolve> svars = UnresolvedSVars [_id];

			foreach (SVarToResolve sv  in svars) {
				if (sv.member is FieldInfo) {
					FieldInfo fi = sv.member as FieldInfo;
					fi.SetValue (sv.instance, _value);
				} else if (sv.member is PropertyInfo) {
					PropertyInfo pi = sv.member as PropertyInfo;
					pi.GetSetMethod ().Invoke (sv.instance, new object[] {_value});
				}
			}

			UnresolvedSVars.Remove(_id);
			return true;
		}
	}
}

