using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using go;
using OpenTK;
using OpenTK.Input;

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
		public Player[] Players;
		public Stack<object> MagicStack = new Stack<object> ();

		bool decksLoaded = false;

		int _currentPlayer;
		int _priorityPlayer;
		int _interfacePlayer = 0;//index of player using this interface
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

		public IList<CardInstance> CardsInPlayHavingSpellEffects
		{
			get {
				return Players.SelectMany (p => p.InPlay.Cards.Where (c => c.Model.SpellEffects.Count() > 0)).ToList();
			}
		}
		public IList<CardInstance> CardsInPlayHavingEffects
		{
			get {
				return Players.SelectMany (p => p.InPlay.Cards.Where (c => c.Effects.Count() > 0)).ToList();
			}
		}
		public IList<CardInstance> CardsInPlayHavingEffect(EffectType et)
		{
			return Players.SelectMany (p => p.InPlay.Cards.Where 
				(c => c.Effects.SelectMany (e => e.Where (ee => ee.TypeOfEffect == et)).Count () > 0)).ToList ();
		}
		public IList<CardInstance> CardsInPlayHavingTriggers
		{
			get {
				return Players.SelectMany (p => p.InPlay.Cards.Where (c => c.Model.Triggers.Count()>0)).ToList();
			}
		}
		public IList<CardInstance> CardsInPlayHavingPumpEffects
		{
			get {
				return Players.SelectMany (p => p.InPlay.Cards.Where (c => c.PumpEffect.Count() > 0)).ToList();
			}
		}
		//public List<ActiveEffect> SpellEffectsInPlay = new List<ActiveEffect> ();

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
			//first cancel incomplete action of priority player
			if (NextActionOnStack != null) {
				//cardsource could be null for action request by engine (ex: discard at cleanup)
				if (NextActionOnStack.CardSource == null){
					pp.PhaseDone = false;
					return;
				}
				if (NextActionOnStack.CardSource.Controler == pp){
					if (!NextActionOnStack.IsComplete) {
						if (!CancelLastActionOnStack ()) {
							pp.PhaseDone = false;
							return;
						}
					}
				}
			}

			priorityPlayer++;

			if (!(pp is AiPlayer) && CurrentPhase != GamePhases.DeclareBlocker)
				startChrono ();
			else
				stopChrono ();

			if (NextActionOnStack == null) {
				if (priorityPlayer == _currentPlayer && cp.PhaseDone)
					SwitchToNextPhase ();
			} else if (NextActionOnStack.CardSource.Controler == pp)
				ResolveStack ();
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


		#region CTOR
		public MagicEngine (Player[] _players)
		{
			CurrentEngine = this;

			Players = _players;
			MagicEvent += new MagicEventHandler (MagicEngine_MagicEvent);

			SpellStackLayout.Position = new Vector3 (0, 0, 2);//Magic.vGroupedFocusedPoint;
			SpellStackLayout.HorizontalSpacing = 0.1f;
			SpellStackLayout.VerticalSpacing = 0.3f;
			SpellStackLayout.MaxHorizontalSpace = 3f;
			//SpellStackLayout.xAngle = Magic.FocusAngle;
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
			//temp fix to have begin not handle before end event in Magic
			//but those kind of sync problem will surely rise 
			if (raiseBeginPhase) {
				raiseBeginPhase = false;
				MagicEvent (new PhaseEventArg {
					Type = MagicEventType.BeginPhase,
					Phase = _currentPhase,
					Player = Players [currentPlayerIndex]
				});
			}
				
			//animate only if cards are loaded
			if (decksLoaded)
				Animation.ProcessAnimations();
			else
				decksLoaded = Players.Where (p => !p.DeckLoaded).Count()==0;

			checkLastActionOnStack();

			foreach (Player p in Players)
				p.Process ();

			if (pp.PhaseDone)
				GivePriorityToNextPlayer();
		}
			
		void MagicEngine_MagicEvent (MagicEventArg arg)
		{
			#region check triggers

			//check cards in play having trigger effects
			foreach (CardInstance ci in CardsInPlayHavingTriggers) {
				foreach (Trigger t in ci.Model.Triggers){					
					if (t.ExecuteIfMatch(arg, ci)){
						Magic.AddLog("=> " + t.ToString());
					}
				}
			}
			//check pump effect
			//todo should simplify trigger checking with a single function
			foreach (CardInstance ci in CardsInPlayHavingPumpEffects) {
				List<EffectGroup> egToRemove = new List<EffectGroup>();
				foreach (EffectGroup eg in ci.PumpEffect) {
					if (eg.TrigEnd != null){
						if (eg.TrigEnd.Type != arg.Type)
							continue;
						switch (eg.TrigEnd.Type) {
						case MagicEventType.EndTurn:
							egToRemove.Add(eg);
							break;
						}
					}
				}
				foreach (EffectGroup egtr in egToRemove) {
					ci.PumpEffect.Remove(egtr);
				}
			}
			#endregion

			switch (arg.Type) {
			case MagicEventType.PlayerIsReady:
				//check if all players are ready
				foreach (Player p in Players)
					if (p.CurrentState != Player.PlayerStates.Ready)
						return;
				startGame ();
				break;
			case MagicEventType.BeginPhase:
				processPhaseBegin (arg as PhaseEventArg);
				break;
			case MagicEventType.EndPhase:
				processPhaseEnd (arg as PhaseEventArg);
				break;
			case MagicEventType.PlayLand:
				break;
			case MagicEventType.ActivateAbility:
				break;
			case MagicEventType.CastSpell:
				break;
			case MagicEventType.TapCard:
				break;
			case MagicEventType.ChangeZone:
				if (arg.Source.IsToken)
					arg.Source.CurrentGroup.Cards.Remove (arg.Source);
				break;
			case MagicEventType.Unset:
				break;
			default:
				break;
			}

			UpdateCardsControler ();
			UpdateCardsPowerAndToughness ();

			foreach (Player p in Players)
				p.InPlay.UpdateLayout ();			
		}

		void processPhaseBegin (PhaseEventArg pea)
		{
			foreach (Player p in Players)
				p.PhaseDone = false;
			
			priorityPlayer = _currentPlayer;

			switch (pea.Phase) {
			case GamePhases.Untap:
				cp.AllowedLandsToBePlayed = 1;
				cp.LifePointsGainedThisTurn = cp.LifePointsLooseThisTurn = 0;
				cp.CardToDraw = 1;
				foreach (CardInstance c in cp.InPlay.Cards) {
					c.HasSummoningSickness = false;
					c.TryToUntap ();
				}
				break;
			case GamePhases.Upkeep:
				break;
			case GamePhases.Draw:
				//if (pea.Player == cp) {
					while(cp.CardToDraw > 0){
						cp.DrawOneCard ();
						cp.CardToDraw--;
					}
				//}
				break;
			case GamePhases.Main1:
			case GamePhases.Main2:
				break;
			case GamePhases.BeforeCombat:				
				break;
			case GamePhases.DeclareAttacker:
//				if (cp.Type == Player.PlayerType.human)
//					stopChrono ();
				break;
			case GamePhases.DeclareBlocker:
				break;
			case GamePhases.FirstStrikeDame:
				Chrono.Reset ();
				foreach (CardInstance ac in cp.AttackingCreature.Where
					(cpac => cpac.HasAbility(AbilityEnum.FirstStrike) || 
						cpac.HasAbility(AbilityEnum.DoubleStrike))) {
					Damage d = new Damage (null, ac, ac.Power);

					foreach (CardInstance def in ac.BlockingCreatures.Where
						(cpac => cpac.HasAbility(AbilityEnum.FirstStrike) || 
							cpac.HasAbility(AbilityEnum.DoubleStrike)))
						MagicStack.Push (new Damage (ac, def, def.Power));

					if (ac.BlockingCreatures.Count == 0) {
						d.Target = cp.Opponent;
						MagicStack.Push (d);
					} else if (ac.BlockingCreatures.Count == 1) {
						d.Target = ac.BlockingCreatures [0];
						MagicStack.Push (d);
					} else {
						//push damages one by one for further resolution
						for (int i = 0; i < ac.Power; i++)
							MagicStack.Push (new Damage (null, d.Source, 1));
					}
				}

				CheckStackForUnasignedDamage ();
				break;
			case GamePhases.CombatDamage:
				Chrono.Reset ();
				foreach (CardInstance ac in cp.AttackingCreature) {
					Damage d = new Damage (null, ac, ac.Power);

					foreach (CardInstance def in ac.BlockingCreatures.Where
						(cpac => !cpac.HasAbility(AbilityEnum.FirstStrike)))
						MagicStack.Push (new Damage (ac, def, def.Power));

					if (ac.HasAbility (AbilityEnum.FirstStrike)&&!ac.HasAbility (AbilityEnum.DoubleStrike))
						return;
					
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
				break;
			case GamePhases.EndOfTurn:
				break;
			case GamePhases.CleanUp:
				foreach (Player p in Players) {
					foreach (CardInstance ac in p.InPlay.Cards) {
						if (ac.Damages.Count > 0) {
							ac.Damages.Clear ();
						}
					}
				}
				int cardDiff = cp.Hand.Cards.Count - 7;
				Ability discard = new Ability (EffectType.Discard);
				discard.ValidTargets += new CardTarget () { ValidGroup = CardGroupEnum.Hand };
				discard.Mandatory = true;
				discard.RequiredTargetCount = 1;

				for (int i = 0; i < cardDiff; i++) {
					MagicStack.Push (new AbilityActivation (null, discard) { GoesOnStack = false});
				}
				break;
			}
		}
		void processPhaseEnd (PhaseEventArg pea)
		{
			ClearIncompleteActions ();

			ResolveStack ();

			switch (pea.Phase) {
			case GamePhases.Untap:
				break;
			case GamePhases.Upkeep:
				break;
			case GamePhases.Draw:
				break;
			case GamePhases.Main2:
			case GamePhases.Main1:
				if (CurrentPhase == GamePhases.Main1 && cp.CreaturesAbleToAttack.Count() == 0)
					CurrentPhase = GamePhases.EndOfCombat;
				break;
			case GamePhases.BeforeCombat:
				break;
			case GamePhases.DeclareAttacker:
				if (!pea.Player.HasAttackingCreature) {
					CurrentPhase = GamePhases.EndOfCombat;
					break;
				}
				foreach (CardInstance c in pea.Player.AttackingCreature) {					
					if (!c.IsTapped && !c.HasAbility (AbilityEnum.Vigilance)) {
						c.Tap ();
						RaiseMagicEvent(new MagicEventArg(MagicEventType.Attack,c));
					}
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
				MagicEvent (new MagicEventArg {
					Type = MagicEventType.EndTurn
						//Player = Players [currentPlayer]
				});
				currentPlayerIndex++;

				priorityPlayer = _currentPlayer;
				CurrentPhase = GamePhases.Untap;
				//Players[_priorityPlayer].pbTimer.Visible = true;					
				break;
			}

			foreach (Player p in Players) {
				//p.CurrentAction = null;
				p.ManaPool = null;
				p.NotifyValueChange ("ManaPoolElements", null);
			}

			if (pea.Phase != GamePhases.CleanUp)
				CurrentPhase++;

			raiseBeginPhase = true;
		}

		bool raiseBeginPhase = false;

		public void ClickOnCard (CardInstance c)
		{
			if (pp != ip)				
				return;
			
			if (MagicStack.Count > 0) {
				MagicAction ma = MagicStack.Peek () as MagicAction;
				if (ma != null) {
					if (!ma.IsComplete) {
						if (ma.TryToAddTarget (c))
							return;
					}
				}
			}
			if (TryToAssignTargetForDamage(c))
			{
				CheckStackForUnasignedDamage();
				return;
			}

			switch (c.CurrentGroup.GroupName) {
			case CardGroupEnum.Library:
				break;
			case CardGroupEnum.Hand:
				#region hand
				//player controling interface may only click in his own hand
				if (c.Controler != ip)
					return;
				if (!CancelLastActionOnStack())
					return;
				if (CurrentPhase == GamePhases.Main1 || CurrentPhase == GamePhases.Main2){
					if (c.HasType(CardTypes.Land)) {
						if (cp.AllowedLandsToBePlayed>0){
							c.ChangeZone (CardGroupEnum.InPlay);
							cp.AllowedLandsToBePlayed--;
							MagicEvent (new MagicEventArg (MagicEventType.PlayLand, c));
						}
					} else {
						PushOnStack(new Spell (c));
					}
				}else if (CurrentPhase != GamePhases.CleanUp && CurrentPhase != GamePhases.Untap){
					//play instant and abilities
					if (c.HasType(CardTypes.Instant))
						PushOnStack(new Spell (c));
				}
				break;
				#endregion
			case CardGroupEnum.InPlay:
				#region inPlay
				if (CurrentPhase == GamePhases.DeclareAttacker) {
					if (c.CanAttack && ip == cp && c.Controler == ip){
						c.Combating = !c.Combating;
						c.CurrentGroup.UpdateLayout ();
						return;
					}
				} else if (CurrentPhase == GamePhases.DeclareBlocker) {
					if (ip != cp){//ip may declare blockers if it's not the current player
						if (c.Controler == ip) {
							if (c.CanBlock()){
								if (c.Combating) {
									c.Combating = false;
									c.BlockedCreature.BlockingCreatures.Remove (c);
									c.BlockedCreature = null;
									c.Controler.InPlay.UpdateLayout ();
								}
								c.Controler.CurrentBlockingCreature = c;
								return;
							}
						} else if (ip.CurrentBlockingCreature != null && c.Combating) {
							//TODO:there's a redundant test here
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
							}
							ip.InPlay.UpdateLayout ();
							return;
						}
					}
				} else if (CurrentPhase == GamePhases.CombatDamage) {
					
				} 
				if (c.Controler == ip) {					
					#region activable abilities
					if (!(c.IsTapped || c.HasSummoningSickness)) {
						Ability[] activableAbs = c.Model.Abilities.Where (
							                         sma => sma.IsActivatedAbility).ToArray ();

						//TODO:if multiple abs, must choose one
						if (activableAbs.Count () > 0)
							PushOnStack (new AbilityActivation (c, activableAbs [0]));
					}
					#endregion					
				}
				#endregion
				break;
			case CardGroupEnum.Graveyard:
				c.CurrentGroup.toogleShowAll ();
				break;
			case CardGroupEnum.Exhiled:
				c.CurrentGroup.toogleShowAll ();
				break;
			default:
				break;
			}
		}

		#region Stack managment
		public MagicAction NextActionOnStack {
			get { return MagicStack.Count == 0 ? null :
				MagicStack.Peek () is MagicAction ?
				MagicStack.Peek () as MagicAction : null; }
		}
		public void PushOnStack (object s)
		{			
			MagicStack.Push (s);
		}
		//TODO:should be changed...
		public void ClearIncompleteActions()
		{
			while (MagicStack.Count > 0) {
				if (MagicStack.Peek () is Damage)
					return;
				if ((MagicStack.Peek () as MagicAction).IsComplete)
					break;
				MagicStack.Pop ();
			}
			Magic.btOk.Visible = false;
		}
		/// <summary>
		/// Cancel incomplete action on stack before doing anything else
		/// </summary>
		/// <returns>True if cancelation succed or if nothing has to be canceled, false otherwise</returns>
		public bool CancelLastActionOnStack()
		{
			MagicAction ma = NextActionOnStack;
			if (ma == null)
				return true;
			if (ma.CardSource.Controler != pp) {
				Debug.Print ("Nothing to cancel");
				return true;
			}
			if (ma.IsComplete)
				Debug.Print ("Canceling completed action");
			if (ma.IsMandatory) {
				Debug.Print ("Unable to cancel mandatory action");
				return false;
			}

			MagicStack.Pop ();
			return true;
		}
		public void ResolveStack ()
		{
			while (MagicStack.Count > 0) {
				if (MagicStack.Peek () is MagicAction) {

					if (!(MagicStack.Peek () as MagicAction).IsComplete)
						break;
					
					MagicAction ma = MagicStack.Pop () as MagicAction;

					UpdateStackLayouting ();

					ma.Resolve ();

					continue;
				}

				if (MagicStack.Peek () is Damage) {
					(MagicStack.Pop () as Damage).Deal ();
					continue;
				}
			}
		}
		/// <summary>
		/// Check completeness of last action on stack.
		/// </summary>
		public void checkLastActionOnStack ()
		{
			if (MagicStack.Count == 0)
				return;
			
			MagicAction ma = MagicStack.Peek () as MagicAction;

			if (ma == null)
				return;

			if (ma.CardSource != null){
				if (ma.CardSource.Controler != pp)
					return;
			}

			if (!ma.IsComplete) {
				if (ma.remainingCost == CostTypes.Tap) {
					ma.remainingCost = null;
					ma.CardSource.Tap ();
				} else if ((pp.AvailableManaOnTable + pp.ManaPool) < ma.remainingCost) {
					Magic.AddLog ("Not enough mana available");
					CancelLastActionOnStack ();
					return;
				} else if (pp.ManaPool != null && ma.RemainingCost != null) {
					ma.PayCost (ref pp.ManaPool);
					pp.NotifyValueChange ("ManaPoolElements", pp.ManaPoolElements);
					pp.UpdateUi ();
				}
				
//				if (ma.IsComplete && ma.GoesOnStack)
//					GivePriorityToNextPlayer ();				
				
			}
			if (ma.IsComplete){
				if (ma.GoesOnStack) {
					//should show spell to player...
					UpdateStackLayouting();
					GivePriorityToNextPlayer ();				
				} else {
					MagicStack.Pop ();
					ma.Resolve ();
				}
				return;
			}


			pp.UpdateUi ();

			//			AbilityActivation aa = ma as AbilityActivation;
			//			//mana doest go on stack
			//			if (aa != null){
			//				if (aa.Source.AbilityType == AbilityEnum.Mana) {
			//					MagicEvent (new AbilityEventArg (aa.Source, aa.CardSource));				
			//					MagicStack.Pop;
			//					return;
			//				}
			//			}
		}
			
		/// <returns>true if all damages are assigned</returns>
		public bool CheckStackForUnasignedDamage ()
		{
            foreach (Damage d in MagicStack.ToArray().OfType<Damage>())
            {
                if (d.Target == null)
                {
					Magic.AddLog(d.Amount + " damage from " + d.Source.Model.Name + " to assign");                    
                    return false;
                }
            }
			return true;
		}
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
		#endregion

		#region Mouse handling
		public void processMouseMove (Point<float> ptM)
		{
			if (!decksLoaded)
				return;
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

			CardInstance tmp = null;
			if (CardInstance.selectedCard != null) {				
				foreach (CardInstance c in CardInstance.selectedCard.CurrentGroup.Cards) {
					if (c.mouseIsIn (ptM)) {
						if (tmp != null) {
							if (c.z < tmp.z)
								continue;
						}
						tmp = c;
					}
				}
			}
			if (tmp == null) {
				foreach (Player p in Players) {
					foreach (CardGroup cg in p.allGroups) {
						foreach (CardInstance c in cg.Cards) {
							if (c.mouseIsIn (ptM)) {
								if (tmp != null) {
									if (c.z < tmp.z)
										continue;
								}
								tmp = c;
							}
						}
						if (tmp != null)
							break;
					}
					if (tmp != null)
						break;
				}
			}
			CardInstance.selectedCard = tmp;
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
		public void UpdateOverlays()
		{
			foreach (CardInstance ci in CardsInPlayHavingEffects) {
				foreach (EffectGroup eg in ci.Effects) {
					foreach (CardTarget ct in eg.Affected.Values.OfType<CardTarget>()) {
						foreach (CardInstance c in ct.GetValidTargetsInPlay (ci))
							c.UpdateOverlay ();
					}	
				}
			}			
		}
		public void UpdateCardsControler()
		{
			foreach (CardInstance ci in Players.SelectMany(p => p.InPlay.Cards)) {
				ci.UpdateControler ();
			}			
		}	
		public void UpdateCardsPowerAndToughness()
		{
			foreach (CardInstance ci in Players.SelectMany(p => p.InPlay.Cards.Where(c => c.HasType(CardTypes.Creature)))) {
				ci.UpdatePowerAndToughness ();
				ci.UpdateOverlay ();
			}			
		}
		public void processRendering()
		{
			if (!decksLoaded)
				return;

			foreach (Player p in Players) {
				p.Render ();
			}

			//SpellStackLayout.Render();
		}

		public void UpdateStackLayouting()
		{
			SpellStackLayout.Cards.Clear ();
			foreach (MagicAction ma in MagicStack.OfType<MagicAction>()) {
				if (ma is Spell)
					SpellStackLayout.Cards.Add ((ma as Spell).CardSource);
				else if (ma is AbilityActivation)
					SpellStackLayout.Cards.Add (new CardInstance(ma));
			}
			SpellStackLayout.UpdateLayout ();			
		}
	}
}
