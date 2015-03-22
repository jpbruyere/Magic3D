using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using go;
using System.Diagnostics;

namespace Magic3D
{


    public class Player : IDamagable
    {
        public enum PlayerType
        {
            human,
            ai
        }

        public static int InitialLifePoints = 20;
		public bool Keep = false;
		public int CardToDraw = 7;

        int _lifePoints;
        string _name;
        Deck _deck;
        Spell _currentSpell;
        CardInstance _currentBlockingCreature;
        List<Damage> _damages = new List<Damage>();

        public void AddDamages(Damage d)
        {
            LifePoints -= d.Amount;

            MagicEngine.CurrentEngine.RaiseMagicEvent(new DamageEventArg(d));

            if (LifePoints < 1)
            {
            }
        }

        public Matrix4 Transformations
        {
			get { return Matrix4.CreateRotationZ(zAngle); }
        }
        public float zAngle = 0.0f;

		PlayerType _type = PlayerType.human;
        public PlayerType Type
		{
			get { return _type; }
			set {
				_type = value;
				(playerPanel.FindByName ("pic") as Image).ImagePath = "image2/HAL9000.svg";
			}
		}
        public Cost ManaPool;


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
        public Spell CurrentSpell
        {
            get { return _currentSpell; }
            set
            {
                if (_currentSpell == value)
                    return;

                _currentSpell = value;
                
//                if (_currentSpell == null)
//                    Magic3D.pCurrentSpell.Visible = false;
//                else if (MagicEngine.CurrentEngine.pp.Type == PlayerType.human)
//                {
//                    Magic3D.pCurrentSpell.Visible = true;
//                    Magic3D.pCurrentSpell.Update(_currentSpell);
//                }
            }
        }

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
		public string deckPath = "Lightforce.dck";
        public Deck Deck
        {
            get { return _deck; }
            set
            {
                _deck = value;
                _deck.Player = this;
            }
        }

        public Library Library;
        public CardGroup Hand;
        public CardGroup Graveyard;
        public InPlayGroup InPlay;
        public CardGroup Exhiled;

        public CardGroup[] allGroups = new CardGroup[5];

        public Player()
        {
            Library = new Library();


            Hand = new CardGroup(CardGroupEnum.Hand);
			Hand.y = -7.5f;
			Hand.z = 3.3f;
			Hand.xAngle = MathHelper.Pi - Vector3.CalculateAngle (Magic.vLook, Vector3.UnitZ);
            Hand.HorizontalSpacing = 0.7f;
            Hand.VerticalSpacing = 0.01f;

            Graveyard = new CardGroup(CardGroupEnum.Graveyard);
            Graveyard.x = -4;
            Graveyard.y = -2.8f;

            InPlay = new InPlayGroup();

            Exhiled = new CardGroup(CardGroupEnum.Exhiled);
            Exhiled.IsVisible = false;

            allGroups[0] = Library;
            allGroups[1] = Hand;
            allGroups[2] = Graveyard;
            allGroups[3] = InPlay;
            allGroups[4] = Exhiled;
			      
        }

        #region interface
		//public Panel playerPanel;
		public Container playerPanel;
		public Label labPts;
		public Label labCpts;
        Label labName;
		public ProgressBar pgBar;

		Color ActiveColor = new Color (0.6, 0.6, 0.7, 0.7);
		Color InactiveColor = new Color (0.2, 0.2, 0.2, 0.5);

		public void initInterface(OpenTKGameWindow mainWin)
        {
			mainWin.LoadInterface ("ui/player.xml", out playerPanel);
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
			if (MagicEngine.CurrentEngine.pp == this)
				playerPanel.Background = ActiveColor;
			else
				playerPanel.Background = InactiveColor;
		}
		#endregion

        /// <summary>
        /// init life points, put all cards in library and shuffle
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
			
			Library.ShuffleAndLayoutZ();
        }
		public void initialDraw ()
		{
			for (int i = 0; i < CardToDraw; i++)
				DrawOneCard ();
//			CardsGroupAnimation cga = Hand.UpdateLayout ();
//			cga.AnimationFinished += delegate { 
//				MagicEngine.CurrentEngine.RaiseMagicEvent (
//					new MagicEventArg () { 
//						Type = MagicEventType.CardsDrawn,
//						Player = this
//					});
//			};
//			Magic.AddAnimation (cga);
//			Magic.AddAnimation (Library.UpdateLayout ());
		}

		public void TakeMulligan()
		{
			for (int i = 0; i < CardToDraw; i++) {
				Library.AddCard (Hand.TakeTopOfStack);
			}
			CardToDraw--;

			//Library.ShuffleAndLayoutZ();

			initialDraw ();
		}

 		public void DrawOneCard()
        {
            CardInstance c = Library.TakeTopOfStack;
            Hand.AddCard(c);
            Animation.StartAnimation(new AngleAnimation(c, "yAngle", 0, MathHelper.Pi * 0.1f));
            Animation.StartAnimation(new AngleAnimation(c, "xAngle",
				MathHelper.Pi - Vector3.CalculateAngle(Magic.vLook, Vector3.UnitZ), MathHelper.Pi * 0.03f));
        }
        public void PutCardInPlay(CardInstance c)
        {
            Hand.RemoveCard(c);
            InPlay.AddCard(c);
			Animation.StartAnimation(new AngleAnimation(c, "xAngle", 0, MathHelper.Pi * 0.3f));
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

       

        public void aiPayManaIfNeedeed()
        {
            if (CurrentSpell != null)
            {
                if (CurrentSpell.RemainingCost != null)
                {

                }
            }

        }

        public bool PhaseDone = false;

        public void Process()
        {
            if (Type == Player.PlayerType.ai)
            {
				//Magic3D.btContinue.Visible = false;
                AIPlayerProcess();
            }
            else
            {
				//Magic3D.btContinue.Visible = true;
                HumanPlayerProcess();
            }
        }

        void HumanPlayerProcess()
        {
            MagicEngine e = MagicEngine.CurrentEngine;

            if (e.pp != this)
            {
                Debug.WriteLine("canceling human player without priority");
                return;
            }                        

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
        void AIPlayerProcess()
        {
            MagicEngine e = MagicEngine.CurrentEngine;

            if (e.pp != this)
            {
                Debug.WriteLine("canceling ai without priority");
                return;
            }

            if (CurrentSpell != null)
            {
                ActivateAvailableMana(e);
                return;
            }

            if (e.cp == this)
            {
                switch (e.CurrentPhase)
                {
                    case GamePhases.Untap:
                        PhaseDone = true;
                        break;
                    case GamePhases.Upkeep:
                        PhaseDone = true;
                        break;
                    case GamePhases.Draw:
                        PhaseDone = true;
                        break;
                    case GamePhases.Main1:
                    case GamePhases.Main2:
                        if (!CastAvailableAndAllowedCreature())
                            PhaseDone = true;
                        break;
                    case GamePhases.BeforeCombat:
                        PhaseDone = true;
                        break;
                    case GamePhases.DeclareAttacker:
                        PhaseDone = true;
                        break;
                    case GamePhases.DeclareBlocker:
                        PhaseDone = true;
                        break;
                    case GamePhases.FirstStrikeDame:
                        PhaseDone = true;
                        break;
                    case GamePhases.CombatDamage:
                        PhaseDone = true;
                        break;
                    case GamePhases.EndOfCombat:
                        PhaseDone = true;
                        break;
                    case GamePhases.EndOfTurn:
                        PhaseDone = true;
                        break;
                    case GamePhases.CleanUp:
                        PhaseDone = true;
                        break;
                }
            }
            else
            {
                switch (e.CurrentPhase)
                {
                    case GamePhases.Untap:
                        PhaseDone = true;
                        break;
                    case GamePhases.Upkeep:
                        PhaseDone = true;
                        break;
                    case GamePhases.Draw:
                        PhaseDone = true;
                        break;
                    case GamePhases.Main1:
                        PhaseDone = true;
                        break;
                    case GamePhases.BeforeCombat:
                        PhaseDone = true;
                        break;
                    case GamePhases.DeclareAttacker:
                        PhaseDone = true;
                        break;
                    case GamePhases.DeclareBlocker:
                        PhaseDone = true;
                        break;
                    case GamePhases.FirstStrikeDame:
                        PhaseDone = true;
                        break;
                    case GamePhases.CombatDamage:
                        PhaseDone = true;
                        break;
                    case GamePhases.EndOfCombat:
                        PhaseDone = true;
                        break;
                    case GamePhases.Main2:
                        PhaseDone = true;
                        break;
                    case GamePhases.EndOfTurn:
                        PhaseDone = true;
                        break;
                    case GamePhases.CleanUp:
                        PhaseDone = true;
                        break;
                }
            }
        }

        public void AITryToPlayLand()
        {            
            CardInstance[] lands = Hand.Cards.Where(c => c.Model.Types == CardTypes.Land).ToArray();

            if (lands.Length > 0)
                MagicEngine.CurrentEngine.RaiseMagicEvent(new MagicEventArg(MagicEventType.PlayLand, lands[0]));

        }

        public void AITryToAttack()
        {
            foreach (CardInstance c in InPlay.Cards.Where(c => c.Model.Types == CardTypes.Creature))
            {
                if (c.CanAttack)
                    c.Combating = true;
            }
            InPlay.UpdateLayout();
        }

        public bool HaveUntapedCreatureOnTable
        {
            get
            {
                return InPlay.Cards.Where(c => !c.IsTapped && c.Model.Types == CardTypes.Creature).Count() > 0 ? true : false;
            }
        }
        public bool HaveAttackingCreature
        {
            get { return AttackingCreature.Length > 0 ? true : false; }
        }

        public void ActivateAvailableMana(MagicEngine engine)
        {
            foreach (CardInstance c in InPlay.Cards.Where(crd => !crd.IsTapped))
            {
                foreach (ManaAbility ma in c.Model.Abilities.OfType<ManaAbility>())
                {
                    if (ma.ActivationCost.Contains(CostTypes.Tap))
                    {
                        if (CurrentSpell.RemainingCost.Contains(ma.ProducedMana))
                        {
                            c.Tap();
							engine.RaiseMagicEvent(new AbilityEventArg(ma,c));
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

        public bool CastAvailableAndAllowedCreature()
        {
            Cost availableMana = AvailableManaOnTable;

            foreach (CardInstance c in Hand.Cards)
            {
                if (c.Model.Types == CardTypes.Creature)
                {
                    if (c.Model.Cost < availableMana)
                    {
                        CurrentSpell = new Spell(c);
                        return true;
                    }
                }
            }
            return false;
        }

		public override string ToString ()
		{
			return string.Format (Name);
		}

    }
}
