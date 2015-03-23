using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Diagnostics;
using go;
using OpenTK.Input;
using System.Threading;

namespace Magic3D
{
	public enum EngineStates
	{
		Stopped,
		WaitForPlayersToBeReady,
		CurrentPlayer,
		Opponents,
		Resolve,
	}

	public class MagicEngine
	{
		public delegate void MagicEventHandler (MagicEventArg arg);
		public static event MagicEventHandler MagicEvent;
		public static MagicEngine CurrentEngine;

		public void RaiseMagicEvent (MagicEventArg arg)
		{
			MagicEvent (arg);
		}

		public volatile EngineStates State = EngineStates.Stopped;
		public bool deckLoaded = false;

		//Sychronizer synchronizer = new Sychronizer ();

		//Random rnd = new Random ();

		int _currentPlayer;
		int _priorityPlayer;
		int _interfacePlayer = 0;
		//index of player using this interface
		GamePhases _currentPhase;
		//public int currentAttackingCreature = 0;    //combat damage resolution
		//public Damage currentDamage;
		/// <summary>
		/// player having his turn running
		/// </summary>
		public Player cp {
			get { return Players [currentPlayerIndex]; }
		}
		/// <summary>
		/// player controling the interface, redirection card click
		/// </summary>
		public Player ip {
			get { return Players [_interfacePlayer]; }
		}
		/// <summary>
		/// player having priority
		/// </summary>
		public Player pp {
			get { return Players [_priorityPlayer]; }
		}

		public int currentPlayerIndex {
			get { return _currentPlayer; }
			set
			{ 
				if (value == _currentPlayer)
					return;

				if (value >= Players.Length)
					_currentPlayer = 0;
				else
					_currentPlayer = value;

			}
		}
		public int getPlayerIndex(Player _player)
		{
			for (int i = 0; i < Players.Count(); i++) {
				if (Players [i] == _player)
					return i;
			}
			return -1;
		}
		public int priorityPlayer {
			get { return _priorityPlayer; }
			set {
				if (value == _priorityPlayer)
					return;
				int oldPp = _priorityPlayer;
				int newPp = value;
				if (newPp >= Players.Length)
					newPp = 0;

				_priorityPlayer = newPp;
				Players [oldPp].UpdateUi ();
				Players [_priorityPlayer].UpdateUi ();
			}
		}
		/// <summary>
		/// Player controling the graphic interface
		/// </summary>
		public int interfacePlayer {
			get { return _interfacePlayer; }
			set { _interfacePlayer = value; }
		}

		public GamePhases CurrentPhase {
			get { return _currentPhase; }
			set { _currentPhase = value; }
		}

		public void SwitchToNextPhase ()
		{
			foreach (Player p in Players)
				p.PhaseDone = false;

			MagicEvent (new PhaseEventArg {
				Type = MagicEventType.EndPhase,
				Phase = _currentPhase,
				Player = Players [currentPlayerIndex]
			});
		}

		public void SwitchToNextPlayer ()
		{
			MagicEvent (new MagicEventArg {
				Type = MagicEventType.EndTurn
				//Player = Players [currentPlayer]
			});
		}

		public void GivePriorityToNextPlayer ()
		{
			priorityPlayer++;

			if (pp.Type == Player.PlayerType.human && CurrentPhase != GamePhases.DeclareBlocker)
				startChrono ();
			else
				stopChrono ();

			if (NextSpellOnStack == null) {
				if (priorityPlayer == _currentPlayer && cp.PhaseDone)
					SwitchToNextPhase ();
			} else if (NextSpellOnStack.Source.Controler == pp)
				CheckStackForResolutions ();
		}

		public void startChrono ()
		{
			//if (pp.Type == Player.PlayerType.ai)
			//    Debugger.Break();

			//Chrono.Restart();
			//pp.pbTimer.Visible = true;

		}

		public void stopChrono ()
		{
			//    //if (pp.Type == Player.PlayerType.ai)
			//    //    Debugger.Break();
			//    Chrono.Reset();
			//    pp.pbTimer.Visible = false;
		}

		public Stopwatch Chrono = new Stopwatch ();
		public static int timerLength = 1500;
		public CardLayout SpellStackLayout = new CardLayout ();
		public Player[] Players;
		public Stack<object> MagicStack = new Stack<object> ();

		#region CTOR
		public MagicEngine (Player[] _players)
		{
			CurrentEngine = this;

			Players = _players;
			MagicEvent += new MagicEventHandler (MagicEngine_MagicEvent);

			SpellStackLayout.Position = Magic.vGroupedFocusedPoint;
			SpellStackLayout.HorizontalSpacing = 0.3f;
			SpellStackLayout.VerticalSpacing = 0.01f;
			SpellStackLayout.MaxHorizontalSpace = 3f;
			SpellStackLayout.xAngle = Magic.FocusAngle;
		}
		#endregion

		void startGame()
		{
			_currentPhase = GamePhases.Main1;
			cp.AllowedLandsToBePlayed = 1;//it's normaly set in the untap phase...
			cp.UpdateUi ();
			State = EngineStates.CurrentPlayer;
			MagicEvent (new PhaseEventArg {
				Type = MagicEventType.BeginPhase,
				Phase = _currentPhase,
				Player = cp
			});
		}

		public void Process ()
		{
			checkCurrentSpell();

			foreach (Player p in Players)
				p.Process ();

			if (pp.PhaseDone)
				GivePriorityToNextPlayer();
		}


		void MagicEngine_MagicEvent (MagicEventArg arg)
		{
			switch (arg.Type) {
			case MagicEventType.PlayerIsReady:
				//check if all players are ready
				foreach (Player p in Players)
					if (p.CurrentState != Player.PlayerStates.Ready)
						return;
				startGame ();
				break;
			case MagicEventType.Unset:
				break;
			case MagicEventType.BeginPhase:
				processPhaseBegin (arg as PhaseEventArg);
				break;
			case MagicEventType.EndPhase:
				processPhaseEnd (arg as PhaseEventArg);
				break;
			case MagicEventType.PlayLand:
				cp.Hand.RemoveCard (arg.Source);
				cp.InPlay.AddCard(arg.Source);
				cp.AllowedLandsToBePlayed--;
				break;
			case MagicEventType.ActivateAbility:
				AbilityEventArg aea = arg as AbilityEventArg;
				switch (aea.Ability.AbilityType) {
				case AbilityEnum.Mana:
					ManaAbility ma = aea.Ability as ManaAbility;
					aea.Source.Controler.ManaPool += ma.ProducedMana;
					aea.Source.Controler.UpdateUi ();
					break;
				default:
					break;
				}					
				break;
			case MagicEventType.CastSpell:
				//if (arg.Card.Controler.Type == Player.PlayerType.ai)
				//    Debugger.Break();
				SpellEventArg sea = arg as SpellEventArg;
				if (sea.Spell.SelectedTargets.Count > 0) {
					Ability a = sea.Spell.Source.getAbilitiesByType (AbilityEnum.Attach).FirstOrDefault ();
					if (a != null) {
						CardInstance c = sea.Spell.SelectedTargets [0] as CardInstance;

						c.AttachedCards.Add (sea.Spell.Source);
						sea.Spell.Source.IsAttached = true;

						foreach (Effect e in a.Effects) {
							Effect ee = e.Clone ();
							ee.TrigEnd = new Trigger {
								Card = sea.Spell.Source,
								Type = MagicEventType.QuitZone
							};
							c.Effects.AddEffect (ee);
						}

						c.UpdateOverlay ();
					}
				}
				//sumoning sickness
				if ((sea.Spell.Source.Model.Types == CardTypes.Creature) &&
				    !sea.Spell.Source.HasAbility (AbilityEnum.Haste))
					sea.Spell.Source.HasSummoningSickness = true;
				sea.Spell.Source.Controler.InPlay.AddCard (sea.Spell.Source);
				break;
			case MagicEventType.TapCard:

				break;
			case MagicEventType.QuitZone:
				foreach (CardInstance ci in arg.Source.AttachedCards) {
					ci.IsAttached = false;
				}
				arg.Source.AttachedCards.Clear ();
				break;
			
			default:
				break;
			}
		}

		void processPhaseBegin (PhaseEventArg pea)
		{
			priorityPlayer = _currentPlayer;

			//cp.labCurrentPhase.Text = pea.Phase.ToString();

			switch (pea.Phase) {
			case GamePhases.Untap:
				cp.AllowedLandsToBePlayed = 1;
				cp.CardToDraw = 1;
				foreach (CardInstance c in cp.InPlay.Cards) {
					c.HasSummoningSickness = false;
					c.Untap ();
				}
				break;
			case GamePhases.Upkeep:
				break;
			case GamePhases.Draw:
				if (pea.Player == cp) {
					while(cp.CardToDraw>0){
						cp.DrawOneCard ();
						cp.CardToDraw--;
					}
				}
				break;
			case GamePhases.Main1:
			case GamePhases.Main2:

				break;
			case GamePhases.BeforeCombat:
				if (!cp.HaveCreaturesOnTableToAttack)
					SwitchToNextPhase ();
				break;
			case GamePhases.DeclareAttacker:
				if (!cp.HaveCreaturesOnTableToAttack)
					SwitchToNextPhase ();
				else if (cp.Type == Player.PlayerType.human)
					stopChrono ();

				break;
			case GamePhases.DeclareBlocker:
				if (!cp.HaveAttackingCreature) {
					SwitchToNextPhase ();
					break;
				}
				cp.PhaseDone = true;
				break;
			case GamePhases.FirstStrikeDame:
				if (!cp.HaveAttackingCreature) {
					SwitchToNextPhase ();
					break;
				}
				break;
			case GamePhases.CombatDamage:
				if (!cp.HaveAttackingCreature) {
					SwitchToNextPhase ();
					break;
				}
				Chrono.Reset ();
				foreach (CardInstance ac in cp.AttackingCreature) {
					Damage d = new Damage (null, ac, ac.Power);

					foreach (CardInstance def in ac.BlockingCreatures)
						MagicStack.Push (new Damage (ac, def, def.Power));

					if (ac.BlockingCreatures.Count == 0) {
						d.Target = cp.Opponent;
						MagicStack.Push (d);
					} else if (ac.BlockingCreatures.Count == 1) {
						d.Target = ac.BlockingCreatures [0];
						MagicStack.Push (d);
					} else {
						for (int i = 0; i < ac.Power; i++)
							MagicStack.Push (new Damage (null, d.Source, 1));
					}
				}

				CheckStackForUnasignedDamage ();
				break;
			case GamePhases.EndOfCombat:
				if (!cp.HaveAttackingCreature) {
					SwitchToNextPhase ();
					break;
				}
				break;
			case GamePhases.EndOfTurn:
				break;
			case GamePhases.CleanUp:
				foreach (Player p in Players) {
					foreach (CardInstance ac in p.InPlay.Cards) {
						if (ac.Damages.Count > 0) {
							ac.Damages.Clear ();
							ac.UpdateOverlay ();
						}
					}
				}
				break;
			}
		}
		void processPhaseEnd (PhaseEventArg pea)
		{
			CheckStackForResolutions ();

			switch (pea.Phase) {
			case GamePhases.Untap:
				break;
			case GamePhases.Upkeep:
				break;
			case GamePhases.Draw:
				break;
			case GamePhases.Main2:
			case GamePhases.Main1:
				if (CurrentPhase == GamePhases.Main1 && !cp.HaveCreaturesOnTableToAttack)
					CurrentPhase = GamePhases.EndOfCombat;
				break;
			case GamePhases.BeforeCombat:
				break;
			case GamePhases.DeclareAttacker:
				if (!pea.Player.HaveAttackingCreature) {
					CurrentPhase = GamePhases.EndOfCombat;
					break;
				}
				foreach (CardInstance c in pea.Player.InPlay.Cards) {
					if (c.Combating) {
						if (!c.IsTapped)
							c.Tap ();
					}else
						Debugger.Break ();
				}
				break;
			case GamePhases.DeclareBlocker:
				break;
			case GamePhases.FirstStrikeDame:
				break;
			case GamePhases.CombatDamage:
				break;
			case GamePhases.EndOfCombat:
				foreach (Player p in Players) {
					bool updateLayout = false;
					foreach (CardInstance c in p.InPlay.Cards) {
						if (c.Combating) {
							updateLayout = true;
							c.Combating = false;
						}
					}
					if (updateLayout)
						p.InPlay.UpdateLayout ();
				}
				break;
			case GamePhases.EndOfTurn:
				break;
			case GamePhases.CleanUp:
				currentPlayerIndex++;

				priorityPlayer = _currentPlayer;
				CurrentPhase = GamePhases.Untap;
				//Players[_priorityPlayer].pbTimer.Visible = true;					
				break;
			}

			foreach (Player p in Players) {
				p.CurrentSpell = null;
				p.ManaPool = null;
			}

			if (pea.Phase != GamePhases.CleanUp)
				CurrentPhase++;

			MagicEvent (new PhaseEventArg {
				Type = MagicEventType.BeginPhase,
				Phase = _currentPhase,
				Player = Players [currentPlayerIndex]
			});


		}
			
		public void ClickOnCard (CardInstance c)
		{


			//            if (Magic3D.pCurrentSpell.Visible) //promt for something
			//            {
			if (ip.CurrentSpell != null)
			{
				if (ip.CurrentSpell.SelectedTargets.Count < ip.CurrentSpell.RequiredTargetCount)
				if (ip.CurrentSpell.TryToAddTarget(c))
					return;
			}
			else if (TryToAssignTargetForDamage(c))
			{
				CheckStackForUnasignedDamage();
				return;
			}
			//            }

			switch (c.CurrentGroup.GroupName) {
			case CardGroupEnum.Library:
				break;
			case CardGroupEnum.Hand:
				#region hand
				if (c.Controler != ip || c.Controler != cp)
					return;

				switch (CurrentPhase) {
				case GamePhases.Untap:
					break;
				case GamePhases.Upkeep:
					break;
				case GamePhases.Draw:
					break;
				case GamePhases.Main1:
				case GamePhases.Main2:
					if (c.Model.Types == CardTypes.Land) {
						if (cp.AllowedLandsToBePlayed>0)
							MagicEvent (new MagicEventArg (MagicEventType.PlayLand, c));
					} else
						ip.CurrentSpell = new Spell (c);
					break;
				case GamePhases.BeforeCombat:
					break;
				case GamePhases.DeclareAttacker:

					break;
				case GamePhases.DeclareBlocker:
					break;
				case GamePhases.FirstStrikeDame:
					break;
				case GamePhases.CombatDamage:
					break;
				case GamePhases.EndOfCombat:
					break;
				case GamePhases.EndOfTurn:
					break;
				case GamePhases.CleanUp:
					break;
				default:
					break;
				}
				break;
				#endregion
			case CardGroupEnum.InPlay:
				#region inPlay
				if (c.Controler != ip &&
					CurrentPhase != GamePhases.DeclareBlocker &&
					CurrentPhase != GamePhases.CombatDamage)
					return;

				if (c.IsTapped || c.HasSummoningSickness)
					return;

				#region ManaAbilities
				//could be played anywhen
				Ability[] manaAbilities = c.getAbilitiesByType(AbilityEnum.Mana);
				if (manaAbilities.Count()>0)
				{
					if (manaAbilities.Count()==1){
						if (manaAbilities[0].ActivationCost.CostType == CostTypes.Tap) {
							c.Tap ();
							MagicEvent (new AbilityEventArg(manaAbilities[0],c));
						}
					}else{
						//show choice
					}
				}
				#endregion

				switch (CurrentPhase) {
				case GamePhases.Untap:
					break;
				case GamePhases.Upkeep:
					break;
				case GamePhases.Draw:
					break;
				case GamePhases.Main1:
				case GamePhases.Main2:
					if (c.Model.Abilities.Count > 0) {
						foreach (Ability ab in c.Model.Abilities) {

						}
					}
					break;
				case GamePhases.BeforeCombat:
					break;
				case GamePhases.DeclareAttacker:
					if (!c.Combating && !c.CanAttack)
						break;
					c.Combating = !c.Combating;
					c.CurrentGroup.UpdateLayout ();
					break;
				case GamePhases.DeclareBlocker:
					if (c.Controler == ip) {
						if (c.Combating) {
							c.Combating = false;
							c.BlockedCreature.BlockingCreatures.Remove (ip.CurrentBlockingCreature);
							c.BlockedCreature = null;
							c.Controler.InPlay.UpdateLayout ();
						}
						c.Controler.CurrentBlockingCreature = c;
						break;
					} else if (ip.CurrentBlockingCreature != null && c.Combating) {
						if (ip.CurrentBlockingCreature.Combating) {
							//remove blocker
							ip.CurrentBlockingCreature.Combating = false;
							ip.CurrentBlockingCreature.BlockedCreature.BlockingCreatures.Remove (ip.CurrentBlockingCreature);
							ip.CurrentBlockingCreature.BlockedCreature = null;
						} else if (ip.CurrentBlockingCreature.CanBlock (c)) {
							//try to add blocker
							c.BlockingCreatures.Add (ip.CurrentBlockingCreature);
							ip.CurrentBlockingCreature.BlockedCreature = c;
							ip.CurrentBlockingCreature.Combating = true;
							ip.CurrentBlockingCreature = null;
						} else
							break;

						ip.InPlay.UpdateLayout ();
					}
					break;
				case GamePhases.FirstStrikeDame:
					break;
				case GamePhases.CombatDamage:
					break;
				case GamePhases.EndOfCombat:
					break;
				case GamePhases.EndOfTurn:
					break;
				case GamePhases.CleanUp:
					break;
				default:
					break;
				}
				break;
				#endregion
			case CardGroupEnum.Graveyard:
				c.CurrentGroup.toogleShowAll ();
				break;
			case CardGroupEnum.Exhiled:
				break;
			default:
				break;
			}
		}

		#region Stack managment
		public Spell NextSpellOnStack {
			get { return MagicStack.Count == 0 ? null : MagicStack.Peek () as Spell; }
		}
		public void PushSpellOnStack (Spell s)
		{
			//should show spell to player...

			MagicStack.Push (s);

			s.Source.Controler.Hand.RemoveCard (s.Source);

			SpellStackLayout.Cards = MagicStack.OfType<Spell> ().Select (sp => sp.Source).ToList ();
			SpellStackLayout.UpdateLayout ();
		}
		public Spell PopSpellFromStack ()
		{
			Spell tmp = MagicStack.Pop () as Spell;
			SpellStackLayout.Cards = MagicStack.OfType<Spell> ().Select (sp => sp.Source).ToList ();
			SpellStackLayout.UpdateLayout ();

			//AttachAbility ab = tmp.getAttachAbilityIfPresent;
			//if (ab != null)
			//{
			//    tmp.Source.AttachedTo = tmp.SelectedTargets.FirstOrDefault() as CardInstance;
			//}
			return tmp;
		}
		public void CheckStackForResolutions ()
		{
			while (MagicStack.Count > 0) {
				if (MagicStack.Peek () is Spell) {
					MagicEvent (new SpellEventArg { Spell = PopSpellFromStack () });
					continue;
				}

				if (MagicStack.Peek () is Damage) {
					(MagicStack.Pop () as Damage).Deal ();
					continue;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>true if all damages are assigned</returns>
		public bool CheckStackForUnasignedDamage ()
		{
			//            foreach (Damage d in MagicStack.ToArray().OfType<Damage>())
			//            {
			//                if (d.Target == null)
			//                {
			//                    Magic3D.pCurrentSpell.Promt.Text = d.Amount + " damage from " + d.Source.Model.Name + " to assign";
			//                    Magic3D.pCurrentSpell.Visible = true;
			//                    return false;
			//                }
			//            }
			return true;
		}

		#endregion

		#region Mouse handling

		Vector3 computeVMouseAtHeight (Vector3 M, float z)
		{
			float t = (z - M.Z) / Magic.vMouse.Z;
			float x = M.X + t * Magic.vMouse.X;
			float y = M.Y + t * Magic.vMouse.Y;
			return new Vector3 (x, y, z);
		}

		public void processMouseMove (Point<float> ptM)
		{
//			Vector3 M = new Vector3( glHelper.UnProject (ref Magic.projection, Magic.modelview, Magic.viewport, new Vector2 (ptM.X, ptM.Y))) ;
//			Magic.vMouse = Vector3.Normalize (Magic.vEye - M);
//
//			Vector3 vMPos = Vector3.Zero;
//			vMPos = computeVMouseAtHeight (M, cg.z); 
//						if (cg.PointIsIn (vMPos)) {
//							selOk = true;
//							cg.IsSelected = true;
//						} else
//							cg.IsSelected = false;
//						break;
//					case CardGroupEnum.Hand:
			ptM.Y = Magic.viewport [3] - ptM.Y;

			if (CardInstance.selectedCard != null) {
				if (CardInstance.selectedCard.mouseIsIn (ptM))
					return;
				else
					CardInstance.selectedCard = null;
			}

			foreach (Player p in Players) {
				foreach (CardGroup cg in p.allGroups) {
					foreach (CardInstance c in cg.Cards) {
						if (c.mouseIsIn (ptM)) {
							CardInstance.selectedCard = c;
							break;
						}
					}
					if (CardInstance.selectedCard != null)
						break;
				}			
			}
		}
		public void processMouseDown (MouseButtonEventArgs e)
		{
			if (CardInstance.selectedCard == null)
				return;

			switch (e.Button) {
			case MouseButton.Left:
				ClickOnCard (CardInstance.selectedCard);
				break;
			default:
				CardInstance.selectedCard.SwitchFocus ();
				break;
			}
		}
		#endregion


		public bool TryToAssignTargetForDamage (CardInstance c)
		{
			foreach (Damage d in MagicStack.ToArray().OfType<Damage>()) {
				if (d.Target == null) {
					d.Target = c;
					//Magic3D.pCurrentSpell.Visible = false;
					return true;
				}
			}
			return false;
		}

		public void checkCurrentSpell ()
		{
			if (pp.CurrentSpell != null) {
				if (pp.CurrentSpell.SelectedTargets.Count < pp.CurrentSpell.RequiredTargetCount) {
//                    if (pp.Type == Player.PlayerType.human)
//                        Magic3D.pCurrentSpell.Update(pp.CurrentSpell);
				}

				if (pp.ManaPool != null) {
					pp.CurrentSpell.RemainingCost = pp.CurrentSpell.RemainingCost.Pay (ref pp.ManaPool);
					pp.UpdateUi ();
					if (pp.CurrentSpell.RemainingCost == null) {
						PushSpellOnStack (pp.CurrentSpell);
						pp.CurrentSpell = null;
						GivePriorityToNextPlayer ();
					}
//                    else if (pp.Type == Player.PlayerType.human)
//                        Magic3D.pCurrentSpell.Update(pp.CurrentSpell);
				}
			}
		}


		public void processRendering()
		{
			if (!deckLoaded)
				return;

			foreach (Player p in Players) {
				p.Render ();
			}

			SpellStackLayout.Render();
		}
	}
}
