using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using go;
using System.Diagnostics;
using System.Threading;
using OpenTK.Input;

namespace Magic3D
{


    public class Player : IDamagable
    {
		/// <summary>
		/// Overall game status
		/// </summary>
		public enum PlayerStates
		{
			init,
			PlayDrawChoice,		
			InitialDraw,
			KeepMuliganChoice,
			Ready,
			SelectTarget,
		}

        public static int InitialLifePoints = 20;

        int _lifePoints;
        string _name;
        Deck _deck;
		//MagicAction _currentAction;

        CardInstance _currentBlockingCreature;
        List<Damage> _damages = new List<Damage>();

		public volatile PlayerStates CurrentState;
		public volatile bool DeckLoaded = false;
		public string deckPath = "Lightforce.dck";
		public Cost ManaPool;
		public bool Keep = false;
		public int CardToDraw = 7;
		public bool PhaseDone = false;

		public Library Library;
		public CardGroup Hand;
		public CardGroup Graveyard;
		public InPlayGroup InPlay;
		public CardGroup Exhiled;
		public CardGroup[] allGroups = new CardGroup[5];

		public CardGroup GetGroup(CardGroupEnum zone)
		{
			return allGroups.Where (g => g.GroupName == zone).FirstOrDefault ();
		}

		#region CTOR
		public Player(string _name, string _deckPath)
		{
			CurrentState = PlayerStates.init;

			initInterface ();

			LoadDeck (_deckPath);

			Name = _name;

			Library = new Library();

			Hand = new CardGroup(CardGroupEnum.Hand);
			Hand.y = -7.5f;
			Hand.z = 3.3f;
			Hand.xAngle = MathHelper.Pi - Vector3.CalculateAngle (Magic.vLook, Vector3.UnitZ);
			Hand.HorizontalSpacing = 0.7f;
			Hand.VerticalSpacing = -0.01f;

			Graveyard = new CardGroup(CardGroupEnum.Graveyard);
			Graveyard.x = -4;
			Graveyard.y = -2.8f;

			InPlay = new InPlayGroup();

			Exhiled = new CardGroup(CardGroupEnum.Exhiled);
			Exhiled.IsVisible = false;

			allGroups[0] = Hand;
			allGroups[1] = InPlay;
			allGroups[2] = Graveyard;
			allGroups[3] = Exhiled;
			allGroups[4] = Library;

		}
		#endregion
		   
     
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;

                if (labName != null)
                    labName.Text = _name;
            }
        }
		public Deck Deck
		{
			get { return _deck; }
			set
			{
				_deck = value;
				_deck.Player = this;
			}
		}
        public int LifePoints
        {
            get { return _lifePoints; }
            set
            {
                _lifePoints = value;

                if (labPts != null)
                    labPts.Text = _lifePoints.ToString();
            }
        }
		public int AllowedLandsToBePlayed {
			get;
			set;
		}
        
//		MagicAction priviousAction;
//		public MagicAction CurrentAction
//        {
//            get { return _currentAction; }
//            set
//            {
//                if (_currentAction == value)
//                    return;
//				
//				if (value is AbilityActivation) {
//					if ((value as AbilityActivation).Source is ManaAbility)
//						priviousAction = _currentAction;
//				}
//
//				_currentAction = value;
//
//				if (_currentAction == null && priviousAction != null) {
//					_currentAction = priviousAction;
//					priviousAction = null;
//					CurrentAction.PayCost (ref ManaPool);
//				}
//            }
//        }
        public CardInstance CurrentBlockingCreature
        {
            get { return _currentBlockingCreature; }
            set
            {
                _currentBlockingCreature = value;

//                if (_currentBlockingCreature == null)
//                {
//                    Magic3D.pCurrentSpell.Visible = false;
//                    return;
//                }
//                Magic3D.pCurrentSpell.Promt.Text = "Choose creature to block...";
//                Magic3D.pCurrentSpell.Visible = true;
            }
        }
        public CardInstance[] AttackingCreature
        {
            get
            {
                return MagicEngine.CurrentEngine.cp == this ?
                    InPlay.Cards.Where(c => c.Combating == true).ToArray() :
                    new CardInstance[0];
            }

        }
        public CardInstance[] BlockingCreature
        {
            get
            {
                return MagicEngine.CurrentEngine.cp != this ?
                    InPlay.Cards.Where(c => c.Combating == true).ToArray() :
                    new CardInstance[0];
            }

        }

        // TODO: implement multiplayer
        public Player Opponent
        {
            get
            {
                return MagicEngine.CurrentEngine.Players[0] == this ?
                    MagicEngine.CurrentEngine.Players[1] :
                    MagicEngine.CurrentEngine.Players[0];
            }
        }

		#region interface
		//public Panel playerPanel;
		public Container playerPanel;
		public Label labPts;
		public Label labCpts;
        Label labName;
		MessageBoxYesNo msgBox;
		public ProgressBar pgBar;

		Color ActiveColor = new Color (0.5, 0.5, 0.6, 0.7);
		Color InactiveColor = new Color (0.1, 0.1, 0.1, 0.4);

		public virtual void initInterface()
        {
			GraphicObject.Load ("ui/player.xml", out playerPanel, this);
			labName = playerPanel.FindByName ("labName") as Label;
			labPts = playerPanel.FindByName ("labPts") as Label;
			labCpts = playerPanel.FindByName ("labCpts") as Label;
			pgBar = playerPanel.FindByName ("pgBar") as ProgressBar;
			playerPanel.Background = InactiveColor;

        }
		public void UpdateUi()
		{
			if (ManaPool == null)
				labCpts.Text = "-";
			else
				labCpts.Text = ManaPool.ToString();
			labPts.Text = LifePoints.ToString ();

			if (MagicEngine.CurrentEngine.cp == this)
				playerPanel.Background = ActiveColor;
			else
				playerPanel.Background = InactiveColor;

			if (MagicEngine.CurrentEngine.pp == this)
				pgBar.Visible = true;
			else
				pgBar.Visible = false;
		}
		void createKeepMulliganChoice()
		{
			msgBox = new MessageBoxYesNo ("Keep or take mulligan ?");
			msgBox.btOk.MouseClick += OnKeep;
			msgBox.btOk.Text = "Keep";
			msgBox.btCancel.MouseClick += OnTakeMulligan;
			msgBox.btCancel.Text = "Mulligan";
			playerPanel.TopContainer.AddWidget (msgBox);
		}
		void OnKeep(Object sender, MouseButtonEventArgs _e)
		{
			playerPanel.TopContainer.DeleteWidget (msgBox);
			CurrentState = PlayerStates.Ready;
			MagicEngine e = MagicEngine.CurrentEngine;
			e.RaiseMagicEvent(new MagicEventArg(MagicEventType.PlayerIsReady,this));
		}
		void OnTakeMulligan(Object sender, MouseButtonEventArgs e)
		{
			playerPanel.TopContainer.DeleteWidget (msgBox);
			for (int i = 0; i < CardToDraw; i++)
				Library.AddCard (Hand.TakeTopOfStack);

			CardToDraw--;

			CurrentState = PlayerStates.InitialDraw;
		}
		//
		#endregion

		#region deck async loading

		public void LoadDeck(string _deckPath){
			Thread thread = new Thread(() => loadingThread(_deckPath));
			thread.Start();
			//loadingThread (_deckPath);
		}
		void loadingThread(string _deckPath){
			Deck tmp = Deck.PreLoadDeck (Magic.deckPath + _deckPath);
			int nbc = tmp.CardCount;
			lock (pgBar) {
				pgBar.Maximum = nbc;
				pgBar.Value = 0;
			}
			for (int i = 0; i < nbc; i++) {
				tmp.LoadNextCardsData ();
				lock (pgBar) {
					pgBar.Value++;
				}
			}
			Deck = tmp;
			lock (pgBar) {
				pgBar.Visible = false;
			}
			Reset ();

			DeckLoaded = true;
		}
		#endregion

        /// <summary>
        /// init life points, put all cards in library
        /// </summary>
        public void Reset()
        {
			Keep = false;
			CardToDraw = 7;
            LifePoints = InitialLifePoints;

            foreach (CardGroup cg in allGroups)
                cg.Cards.Clear();

            foreach (CardInstance c in Deck.Cards)
            {
                c.Controler = this;
//                foreach (Ability a in c.Model.Abilities)
//                    a.Source = c;

                c.ResetPositionAndRotation();
				c.yAngle = MathHelper.Pi;
                Library.AddCard(c);
            }
        }

		public void initialDraw ()
		{
			Animation.DelayMs = 300;
			Library.ShuffleAndLayoutZ();
			for (int i = 0; i < CardToDraw; i++) {
				Animation.DelayMs += i * 5;
				DrawOneCard ();
			}
			Animation.DelayMs = 0;
		}
 		public void DrawOneCard()
        {
            CardInstance c = Library.TakeTopOfStack;
            Hand.AddCard(c);
			MagicEngine.CurrentEngine.RaiseMagicEvent (
				new ChangeZoneEventArg (c,CardGroupEnum.Library,CardGroupEnum.Hand));
        }			
		public void AddDamages(Damage d)
		{
			LifePoints -= d.Amount;

			MagicEngine.CurrentEngine.RaiseMagicEvent(new DamageEventArg(d));

			if (LifePoints < 1)
			{
			}
		}
			
		public virtual void Process()
        {
			MagicEngine e = MagicEngine.CurrentEngine;

			switch (CurrentState) {
			#region StartUp
			case PlayerStates.init:
				return;
			case PlayerStates.PlayDrawChoice:
				return;
			case PlayerStates.InitialDraw:
				if (!DeckLoaded)
					return;
				initialDraw ();
				CurrentState = PlayerStates.KeepMuliganChoice;
				createKeepMulliganChoice ();
				return;
			case PlayerStates.KeepMuliganChoice:
				return;
				#endregion
			}

			if (e.pp != this || e.State < EngineStates.CurrentPlayer)
				return;

			switch (e.CurrentPhase)
			{
			case GamePhases.Untap:
				break;
			case GamePhases.Upkeep:
				break;
			case GamePhases.Draw:
				break;
			case GamePhases.Main1:
			case GamePhases.Main2:                        
				//e.Chrono.Reset();
				break;
			case GamePhases.BeforeCombat:
				//e.Chrono.Restart();
				break;
			case GamePhases.DeclareAttacker:
				//if (cp != this)

				//if (HaveUntapedCreatureOnTable)
				//    e.Chrono.Stop();
				//if (pp != cp || !cp.HaveUntapedCreatureOnTable)
				//{
				//    SwitchToNextPhase();
				//    GivePriorityToNextPlayer();
				//}

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
			}

			//            if (e.Chrono.ElapsedMilliseconds < MagicEngine.timerLength)
			//                return;
			//
			//            PhaseDone = true;
			//            e.Chrono.Stop();
        }
 
        public IEnumerable<CardInstance> CreaturesAbleToAttack
        {
            get
            {
				foreach (CardInstance c in InPlay.Cards) {
					if (c.Model.Types == CardTypes.Creature && 
						!c.IsTapped && !c.HasSummoningSickness) {						
							yield return c;
					}
				}
            }
        }
        public bool HasAttackingCreature
        {
            get { return AttackingCreature.Length > 0 ? true : false; }
        }
		public bool HasActionPending
		{
			get {				
				MagicEngine e = MagicEngine.CurrentEngine;
				if (e.pp != this)
					return false;
				if (e.MagicStack.Count == 0)
					return false;
				MagicAction ma = e.MagicStack.Peek () as MagicAction;
				return ma == null ? false : (ma.IsComplete || ma.CardSource.Controler != this) ? false : true;				
			}
		}
//		public bool PlayableSpell
//		{
//			get{
//				MagicEngine e = MagicEngine.CurrentEngine;
//
//				if (e.cp == this) {
//					if (e.CurrentPhase == GamePhases.Main1 || e.CurrentPhase == GamePhases.Main2) {
//						
//					}
//				}
//
//			}
//		}
		public IEnumerable<CardInstance> PlayableInstants{
			get {
				Cost availableMana = AvailableManaOnTable;

				foreach (CardInstance c in Hand.Cards) {
					if (c.Model.Types == CardTypes.Instant) {
						if (c.Model.Cost < availableMana)
							yield return c;
					}
				}
			}
		}
        public void ActivateAvailableMana(MagicEngine engine)
        {
			MagicAction ma = engine.MagicStack.Peek () as MagicAction;
			if (ma == null) {
				Debug.WriteLine ("no action to activate mana for.");
				return;
			}

            foreach (CardInstance c in InPlay.Cards.Where(crd => !crd.IsTapped))
            {
				foreach (ManaAbility a in c.Model.Abilities.OfType<ManaAbility>())
                {
                    if (a.ActivationCost.Contains(CostTypes.Tap))
                    {
                        if (ma.RemainingCost.Contains(a.ProducedMana))
                        {
							MagicEngine.CurrentEngine.PushOnStack(new AbilityActivation(c,a));
                            return;
                        }
                    }
                }
            }
        }

        public Cost AvailableManaOnTable
        {
            get
            {
                Cost availableMana = null;
                foreach (CardInstance c in InPlay.Cards.Where(crd => !crd.IsTapped))
                {
                    ManaChoice mc = new ManaChoice();
                    foreach (ManaAbility ma in c.Model.Abilities.OfType<ManaAbility>())
                    {
                        if (ma.ActivationCost.Contains(CostTypes.Tap))
                            mc += ma.ProducedMana.Clone() as Mana;
                    }
                    if (mc.Manas.Count == 0)
                        continue;
                    if (mc.Manas.Count == 1)
                        availableMana += mc.Manas[0];
                    else
                        availableMana += mc;
                }
                return availableMana;
            }
        }
			

		#region Rendering
		public float zAngle = 0.0f;
		public Matrix4 Transformations
		{
			get { return Matrix4.CreateRotationZ(zAngle); }
		}

		public void Render()
		{
			Matrix4 mSave = Magic.texturedShader.ModelMatrix;            
			Magic.texturedShader.ModelMatrix *= Transformations;

			foreach (CardGroup cg in allGroups)
			{
				if (cg.IsVisible)
					cg.Render();
			}
			Magic.texturedShader.ModelMatrix = mSave;
		}
		#endregion

		public override string ToString ()
		{
			return string.Format (Name);
		}

    }
}
