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

        public PlayerType Type = PlayerType.human;
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

                if (labPoints != null)
                    labPoints.Text = _lifePoints.ToString();
            }
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


            Hand = new CardGroup(CardGroups.Hand);
			Hand.y = -7.5f;
			Hand.z = 3.3f;
            Hand.HorizontalSpacing = 0.7f;
            Hand.VerticalSpacing = -0.001f;

            Graveyard = new CardGroup(CardGroups.Graveyard);
            Graveyard.x = -4;
            Graveyard.y = -2.8f;

            InPlay = new InPlayGroup();

            Exhiled = new CardGroup(CardGroups.Exhiled);
            Exhiled.IsVisible = false;

            allGroups[0] = Library;
            allGroups[1] = Hand;
            allGroups[2] = Graveyard;
            allGroups[3] = InPlay;
            allGroups[4] = Exhiled;

            initInterface();
        }

        #region interface
		//public Panel playerPanel;
        Label labPoints;
        Label labName;
        public Label labCurrentPhase;
        public ProgressBar pbTimer;

        void initInterface()
        {
//            playerPanel = Interface.addPanel(new go.Rectangle(10, 200, 150, 100));
//            Group g = playerPanel.setChild(new Group());
//            labPoints = g.addChild(new Label("###"));
//            labPoints.FontSize = 20;
//            labPoints.VerticalAlignment = VerticalAlignment.Stretch;
//            labPoints.HorizontalAlignment = HorizontalAlignment.Stretch;
//            //labPoints.textAlignment = Alignment.VerticalStretch;
//            labPoints.HorizontalAlignment = HorizontalAlignment.Right;
//            VerticalStack vs = g.addChild(new VerticalStack());
//            labName = vs.addChild(new Label("PlayerName"));
//            labName.FontSize = 14;
//            labCurrentPhase = vs.addChild(new Label("Init"));
//            labCurrentPhase.HorizontalAlignment = HorizontalAlignment.Stretch;
//            labName.FontSize = 14;
//            pbTimer = g.addChild(new ProgressBar(MagicEngine.timerLength));
//            pbTimer.VerticalAlignment = VerticalAlignment.Bottom;
//            pbTimer.HorizontalAlignment = HorizontalAlignment.Right;
        }
        #endregion

        /// <summary>
        /// init life points, put all cards in library and shuffle
        /// </summary>
        public void Reset()
        {
            LifePoints = InitialLifePoints;

            foreach (CardGroup cg in allGroups)
                cg.Cards.Clear();

            foreach (CardInstance c in Deck.Cards)
            {
                c.Controler = this;
                foreach (Ability a in c.Model.Abilities)
                    a.Source = c;

                c.ResetPositionAndRotation();
                c.yAngle = MathHelper.Pi;
                Library.AddCard(c);
            }

            Library.ShuffleAndLayoutZ();
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
            //Hand.RemoveCard(c);
            InPlay.AddCard(c);
            Animation.StartAnimation(new AngleAnimation(c, "xAngle", 0, MathHelper.Pi * 0.3f));
        }

        public void Render()
        {
			Matrix4 mSave = Magic.texturedShader.ModelViewMatrix;            
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
//            if (Type == Player.PlayerType.ai)
//            {
//                Magic3D.btContinue.Visible = false;
//                AIPlayerProcess();
//            }
//            else
//            {
//                Magic3D.btContinue.Visible = true;
//                HumanPlayerProcess();
//            }
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

            if (e.Chrono.ElapsedMilliseconds < MagicEngine.timerLength)
                return;

            PhaseDone = true;
            e.Chrono.Stop();
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
                            engine.RaiseMagicEvent(new SpellEventArg { Spell = ma });
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


    }
}
