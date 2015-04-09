using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace Magic3D
{    
	public enum TrigExec
	{
		TrigGainLife,
		TrigToken,
		TrigChangeZone,
		TrigExile,
		TrigPump,
		TrigKicker,
		TrigChange,
		TrigTap
	}

    public class Trigger
    {    
		public Trigger()
		{			
		}
		public Trigger(MagicEventType _type)
		{			
			Type = _type;
		}

        public MagicEventType Type;
		#region changezonearg
		public CardGroupEnum Origine;
		public CardGroupEnum Destination;
		#endregion
		public List<Object> Targets = new List<object> ();
		//public List<Effect> Effects;
        public GamePhases Phase;
        //public CardInstance Source;
		public TrigExec Exec;
		public string Description;

		static List<string> list = new List<string> ();
		public override string ToString ()
		{
			return Description;
		}

		public static Trigger Parse(string str)
		{
			Trigger t = new Trigger (MagicEventType.Unset);
			string[] tmp = str.Trim ().Split (new char[] { '|' });

			//t.Targets = new List<object> ();
			//t.Effects = new List<Effect> ();


			using (Stream s = new FileStream ("trigVars.txt",FileMode.Append)) {				
				using (TextWriter tw = new StreamWriter (s)) {
					foreach (string i in tmp) {
						string[] f = i.Trim ().Split (new char[] { '$' });
						string data = f [1].Trim();
						switch (f[0]) {
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
								Debug.WriteLine ("Unknown trigger " + f[0] + " value:" + data);
								break;
							}
							break;
						case "Origin":
							switch (data) {
							case "Any":
								t.Origine = CardGroupEnum.Any;
								break;
							default:
								Debug.WriteLine ("Unknown trigger " + f[0] + " value:" + data);
								break;
							}							
							break;
						case "Destination":
							switch (data) {
							case "Battlefield":
								t.Destination = CardGroupEnum.InPlay;
								break;
							default:
								Debug.WriteLine ("Unknown trigger " + f[0] + " value:" + data);
								break;
							}							
							break;
						case "ValidCard":
							switch (data) {
							case "Card.Self":
								t.Targets.Add ("this");
								break;
							case "Card.Self+kicked":
								t.Targets.Add ("this");
								break;
							case "Creature.Self":
								t.Targets.Add ("this");
								break;
							case "Card.White":
								t.Targets.Add (ManaTypes.White);
								break;
							case "Plains.YouCtrl":
								t.Targets.Add (new object[] { CardTypes.Plains, "self" });
								break;
							default:
								Debug.WriteLine ("Unknown trigger " + f[0] + " value:" + data);
								break;
							}
							break;
						case "Execute":
							t.Exec = (TrigExec)Enum.Parse (typeof(TrigExec), data);
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
							if (list.Contains (data))
								break;
							list.Add (data);
							tw.WriteLine ("case \"" + data + "\":\n\tbreak;");
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
				}
			}
			return t;
		}
    }
}
