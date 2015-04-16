using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace Magic3D
{
	public class Trigger
	{
		public Trigger ()
		{			
		}

		public Trigger (MagicEventType _type, 
			MultiformAttribut<Target> _targets = null,
			Ability _exec = null)
		{			
			Type = _type;
			ValidTarget = _targets;
			Exec = _exec;
		}


		public MagicEventType Type;

		#region changezonearg

		public CardGroupEnum Origine;
		public CardGroupEnum Destination;

		#endregion

		public MultiformAttribut<Target> ValidTarget;
		//public List<Effect> Effects;
		public GamePhases Phase;
		//public CardInstance Source;
		public Ability Exec;
		public string Description;

//		public bool Match(MagicEventArg arg)
//		{
//			if (Type != arg.Type)
//				return false;
//			switch (Type) {
//			case MagicEventType.ChangeZone:
//
//				bool valid = false;
////				foreach (CardTarget ct in t.ValidTarget.Values.OfType<CardTarget>()) {
////					if (ct.Accept (arg.Source, arg.Source))
////						valid = true;		
////				}
//
//				if (!valid)
//					break;
//
//				ChangeZoneEventArg czea = arg as ChangeZoneEventArg;
//				if ((czea.Origine == Origine || Origine == CardGroupEnum.Any)
//				    && (czea.Destination == Destination || Destination == CardGroupEnum.Any))
//					return true;
//				break;
//			}			
//		}

		static List<string> list = new List<string> ();

		public override string ToString ()
		{
			return Description;
		}

		public static Trigger Parse (string str)
		{
			Trigger t = new Trigger (MagicEventType.Unset);
			string[] tmp = str.Trim ().Split (new char[] { '|' });

			//t.Targets = new List<object> ();
			//t.Effects = new List<Effect> ();


//			using (Stream s = new FileStream ("trigVars.txt",FileMode.Append)) {				
//				using (TextWriter tw = new StreamWriter (s)) {
			foreach (string i in tmp) {
				string[] f = i.Trim ().Split (new char[] { '$' });
				string data = f [1].Trim ();
				switch (f [0]) {
				case "Mode":
					switch (data) {
					case "ChangesZone":
						t.Type = MagicEventType.ChangeZone;
						break;
					case "Phase":
						break;
					case "Attacks":
						break;
					case "SpellCast":
						break;
					default:
						Debug.WriteLine ("Unknown trigger " + f [0] + " value:" + data);
						break;
					}
					break;
				case "Origin":
					t.Origine = CardGroup.ParseZoneName (data);
					break;
				case "Destination":
					t.Destination = CardGroup.ParseZoneName (data);
					break;
				case "ValidCard":														
					t.ValidTarget = Target.ParseTargets (data);
					break;
				case "Execute":
					SVarToResolve.RegisterSVar(data, t, t.GetType().GetField("Exec"));
					break;
				case "TriggerDescription":
					t.Description = data;
					break;
				case "Phase":
					t.Type = MagicEventType.BeginPhase;
					switch (data) {
					case "End of Turn":
						t.Phase = GamePhases.EndOfTurn;
						break;
					default:
						Debug.WriteLine ("Trigger parsing: Unknown phase:" + data);
						break;
					}

							//tw.WriteLine (data + ",");
					break;
				case "TriggerZones":
//							if (list.Contains (data))
//								break;
//							list.Add (data);
//							tw.WriteLine ("case \"" + data + "\":\n\tbreak;");
					break;
				case "CheckSVar":
					break;
				case "SVarCompare":
					break;
				case "OptionalDecider":
					break;
				case "ValidActivatingPlayer":
					break;
				case "Secondary":
					break;
				default:
					Debug.WriteLine ("Unknown trigger var:" + f [0]);
					break;
				}

			}
//				}
//			}
			return t;
		}
	}
}
