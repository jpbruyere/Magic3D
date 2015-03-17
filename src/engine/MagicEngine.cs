using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Diagnostics;
using go;

namespace Magic3D
{

    public enum EngineStates
    {
        Stopped,
        Toss,
        Init,
        CurrentPlayer,
        Opponents,
        Resolve,
    }
    public enum GamePhases
    {
        Untap,
        Upkeep,
        Draw,
        Main1,
        BeforeCombat,
        DeclareAttacker,
        DeclareBlocker,
        FirstStrikeDame,
        CombatDamage,
        EndOfCombat,
        Main2,
        EndOfTurn,
        CleanUp
    }

    public enum MagicEventType
    {
        Unset,
        BeginPhase,
        EndPhase,
        EnTurn,
        PlayLand,
        CastSpell,
        Destroy,
        Exill,
        QuitZone,
        TapCard,
        Damage
    }

    public class MagicEventArg
    {
        public MagicEventType Type;
        public CardInstance Card;
        public Player Player;
        public MagicEventArg()
        { }
        public MagicEventArg(MagicEventType t, CardInstance _card = null)
        {
            Type = t;
            Card = _card;
        }
    }
    public class PhaseEventArg : MagicEventArg
    {
        public GamePhases Phase;

    }
    public class SpellEventArg : MagicEventArg
    {
        public Spell Spell;
        public SpellEventArg()
        {
            Type = MagicEventType.CastSpell;
        }
    }
    public class AbilityEventArg : MagicEventArg
    {
        public Ability ability;
    }
    public class DamageEventArg : MagicEventArg
    {
        public Damage Damage;
        public DamageEventArg(Damage _d)
        {
            Type = MagicEventType.Damage;
            Damage = _d;
        }
    }

    public class MagicEngine
    {
        public delegate void MagicEventHandler(MagicEventArg arg);
        public static event MagicEventHandler MagicEvent;

        public static MagicEngine CurrentEngine;

        public void RaiseMagicEvent(MagicEventArg arg)
        {
            MagicEvent(arg);
        }

        int _currentPlayer;
        int _priorityPlayer;
        int _interfacePlayer = 0;   //index of player using this interface
        GamePhases _currentPhase;

        public bool landPlayed = false;
        //public int currentAttackingCreature = 0;    //combat damage resolution
        //public Damage currentDamage;
        /// <summary>
        /// player having his turn running
        /// </summary>
        public Player cp
        {
            get { return Players[currentPlayer]; }
        }
        /// <summary>
        /// player controling the interface, redirection card click
        /// </summary>
        public Player ip
        {
            get { return Players[_interfacePlayer]; }
        }
        /// <summary>
        /// player having priority
        /// </summary>
        public Player pp
        {
            get { return Players[_priorityPlayer]; }
        }

        public int currentPlayer
        {
            get { return _currentPlayer; }
        }
        public int priorityPlayer
        {
            get { return _priorityPlayer; }
        }
        public GamePhases CurrentPhase
        {
            get { return _currentPhase; }
            set { _currentPhase = value; }
        }

        public void SwitchToNextPhase()
        {
            foreach (Player p in Players)
                p.PhaseDone = false;

            MagicEvent(new PhaseEventArg
            {
                Type = MagicEventType.EndPhase,
                Phase = _currentPhase,
                Player = Players[currentPlayer]
            });

            if (CurrentPhase == GamePhases.CleanUp)
            {
                SwitchToNextPlayer();
                CurrentPhase = GamePhases.Untap;
            }
            else
                CurrentPhase++;

            MagicEvent(new PhaseEventArg
            {
                Type = MagicEventType.BeginPhase,
                Phase = _currentPhase,
                Player = Players[currentPlayer]
            });

            _priorityPlayer = _currentPlayer;
        }
        public void SwitchToNextPlayer()
        {
            MagicEvent(new MagicEventArg
            {
                Type = MagicEventType.EnTurn,
                Player = Players[currentPlayer]
            });

            _currentPlayer++;

            if (_currentPlayer == Players.Length)
                _currentPlayer = 0;

            _priorityPlayer = _currentPlayer;
            Players[_priorityPlayer].pbTimer.Visible = true;
        }

        public void GivePriorityToNextPlayer()
        {
            _priorityPlayer++;

            if (_priorityPlayer == Players.Length)
                _priorityPlayer = 0;

            if (pp.Type == Player.PlayerType.human && CurrentPhase != GamePhases.DeclareBlocker)
                startChrono();
            else
                stopChrono();

            if (NextSpellOnStack == null)
            {
                if (_priorityPlayer == _currentPlayer && cp.PhaseDone)
                    SwitchToNextPhase();
            }
            else if (NextSpellOnStack.Source.Controler == pp)
                CheckStackForResolutions();
        }

        public void startChrono()
        {
            //if (pp.Type == Player.PlayerType.ai)
            //    Debugger.Break();

            //Chrono.Restart();
            //pp.pbTimer.Visible = true;

        }
        public void stopChrono()
        {
        //    //if (pp.Type == Player.PlayerType.ai)
        //    //    Debugger.Break();
        //    Chrono.Reset();
        //    pp.pbTimer.Visible = false;
        }

        public Stopwatch Chrono = new Stopwatch();
        public static int timerLength = 1500;
        public CardLayout SpellStackLayout = new CardLayout();

        public Player[] Players;
        public Stack<object> MagicStack = new Stack<object>();
        public EngineStates State = EngineStates.Stopped;


        public MagicEngine(Player[] _players)
        {
            CurrentEngine = this;

            Players = _players;
            MagicEvent += new MagicEventHandler(MagicEngine_MagicEvent);

			SpellStackLayout.Position = Magic.vGroupedFocusedPoint;
            SpellStackLayout.HorizontalSpacing = 0.3f;
            SpellStackLayout.VerticalSpacing = 0.01f;
            SpellStackLayout.MaxHorizontalSpace = 3f;
			SpellStackLayout.xAngle = Magic.FocusAngle;
        }

        void processPhaseBegin(PhaseEventArg pea)
        {
            cp.labCurrentPhase.Text = pea.Phase.ToString();

            switch (pea.Phase)
            {
                case GamePhases.Untap:
                    InitTurn();

                    foreach (CardInstance c in cp.InPlay.Cards)
                        c.Untap();
                    break;
                case GamePhases.Upkeep:
                    break;
                case GamePhases.Draw:
                    if (pea.Player == cp)
                        cp.DrawOneCard();
                    break;
                case GamePhases.Main1:
                case GamePhases.Main2:
                    if (cp.Type == Player.PlayerType.ai && !landPlayed)
                        cp.AITryToPlayLand();
                    else
                        stopChrono();
                    break;
                case GamePhases.BeforeCombat:
                    if (!cp.HaveUntapedCreatureOnTable)
                        SwitchToNextPhase();
                    break;
                case GamePhases.DeclareAttacker:
                    if (!cp.HaveUntapedCreatureOnTable)
                        SwitchToNextPhase();
                    else if (cp.Type == Player.PlayerType.human)
                        stopChrono();
                    else
                        cp.AITryToAttack();
                    break;
                case GamePhases.DeclareBlocker:
                    if (!cp.HaveAttackingCreature)
                    {
                        SwitchToNextPhase();
                        break;
                    }
                    cp.PhaseDone = true;
                    break;
                case GamePhases.FirstStrikeDame:
                    if (!cp.HaveAttackingCreature)
                    {
                        SwitchToNextPhase();
                        break;
                    }
                    break;
                case GamePhases.CombatDamage:
                    if (!cp.HaveAttackingCreature)
                    {
                        SwitchToNextPhase();
                        break;
                    }
                    Chrono.Reset();
                    foreach (CardInstance ac in cp.AttackingCreature)
                    {
                        Damage d = new Damage(null, ac, ac.Power);

                        foreach (CardInstance def in ac.BlockingCreatures)
                            MagicStack.Push(new Damage(ac, def, def.Power));

                        if (ac.BlockingCreatures.Count == 0)
                        {
                            d.Target = cp.Opponent;
                            MagicStack.Push(d);
                        }
                        else if (ac.BlockingCreatures.Count == 1)
                        {
                            d.Target = ac.BlockingCreatures[0];
                            MagicStack.Push(d);
                        }
                        else
                        {
                            for (int i = 0; i < ac.Power; i++)
                                MagicStack.Push(new Damage(null, d.Source, 1));
                        }
                    }

                    CheckStackForUnasignedDamage();
                    break;
                case GamePhases.EndOfCombat:
                    if (!cp.HaveAttackingCreature)
                    {
                        SwitchToNextPhase();
                        break;
                    }
                    break;
                case GamePhases.EndOfTurn:
                    break;
                case GamePhases.CleanUp:
                    foreach (Player p in Players)
                    {
                        foreach (CardInstance ac in p.InPlay.Cards)
                        {
                            if (ac.Damages.Count > 0)
                            {
                                ac.Damages.Clear();
                                ac.UpdateOverlay();
                            }
                        }
                    }
                    break;
            }
        }

        void processPhaseEnd(PhaseEventArg pea)
        {
            CheckStackForResolutions();

            switch (pea.Phase)
            {
                case GamePhases.Untap:
                    break;
                case GamePhases.Upkeep:
                    break;
                case GamePhases.Draw:
                    break;
                case GamePhases.Main2:
                case GamePhases.Main1:
                    break;
                case GamePhases.BeforeCombat:
                    break;
                case GamePhases.DeclareAttacker:
                    foreach (CardInstance c in pea.Player.InPlay.Cards)
                    {
                        if (c.Combating)
                            if (!c.IsTapped)
                                c.Tap();
                            else
                                Debugger.Break();
                    }
                    break;
                case GamePhases.DeclareBlocker:
                    break;
                case GamePhases.FirstStrikeDame:
                    break;
                case GamePhases.CombatDamage:
                    break;
                case GamePhases.EndOfCombat:
                    foreach (Player p in Players)
                    {
                        bool updateLayout = false;
                        foreach (CardInstance c in p.InPlay.Cards)
                        {
                            if (c.Combating)
                            {
                                updateLayout = true;
                                c.Combating = false;
                            }
                        }
                        if (updateLayout)
                            p.InPlay.UpdateLayout();
                    }
                    break;
                case GamePhases.EndOfTurn:
                    break;
                case GamePhases.CleanUp:
                    break;
            }
        }

        void MagicEngine_MagicEvent(MagicEventArg arg)
        {
            switch (arg.Type)
            {
                case MagicEventType.Unset:
                    break;
                case MagicEventType.BeginPhase:
                    processPhaseBegin(arg as PhaseEventArg);
                    break;
                case MagicEventType.EndPhase:
                    processPhaseEnd(arg as PhaseEventArg);
                    break;
                case MagicEventType.PlayLand:
                    cp.Hand.RemoveCard(arg.Card);
                    cp.PutCardInPlay(arg.Card);
                    landPlayed = true;
                    break;
                case MagicEventType.CastSpell:
                    //if (arg.Card.Controler.Type == Player.PlayerType.ai)
                    //    Debugger.Break();
                    SpellEventArg sea = arg as SpellEventArg;
                    Ability a = sea.Spell as Ability;
                    if (a != null)
                    {
                        #region abilities
                        switch (a.AbilityType)
                        {
                            case AbilityEnum.Mana:
                                ManaAbility ma = a as ManaAbility;
                                sea.Spell.Source.Controler.ManaPool += ma.ProducedMana;
                                break;
                            default:
                                break;
                        }

                        #endregion
                    }
                    else
                    {
                        if (sea.Spell.SelectedTargets.Count > 0)
                        {
                            a = sea.Spell.Source.getAbilityByType(AbilityEnum.Attach);
                            if (a != null)
                            {
                                CardInstance c = sea.Spell.SelectedTargets[0] as CardInstance;

                                c.AttachedCards.Add(sea.Spell.Source);
                                sea.Spell.Source.IsAttached = true;

                                foreach (Effect e in a.Effects)
                                {
                                    Effect ee = e.Clone();
                                    ee.TrigEnd = new Trigger { Card = sea.Spell.Source, Type = MagicEventType.QuitZone };
                                    c.Effects.AddEffect(ee);
                                }

                                c.UpdateOverlay();
                            }
                        }
                        sea.Spell.Source.Controler.PutCardInPlay(sea.Spell.Source);
                    }
                    break;
                case MagicEventType.TapCard:

                    break;
                case MagicEventType.QuitZone:
                    foreach (CardInstance ci in arg.Card.AttachedCards)
                    {
                        ci.IsAttached = false;
                    }
                    arg.Card.AttachedCards.Clear();
                    break;
                default:
                    break;
            }
        }

//        void OnPlayFirst(Button sender)
//        {
//            Interface.UnloadPanel(sender.panel);
//
//            _currentPlayer = 0;
//            _priorityPlayer = 0;
//
//            start();
//        }
//
//        void onDrawFirst(Button sender)
//        {
//            Interface.UnloadPanel(sender.panel);
//
//            _currentPlayer = 1;
//            _priorityPlayer = 1;
//
//            start();
//        }

        void start()
        {
            State = EngineStates.CurrentPlayer;

            initialDraw();

            _currentPhase = GamePhases.Main1;
            MagicEvent(new PhaseEventArg
            {
                Type = MagicEventType.BeginPhase,
                Phase = _currentPhase,
                Player = cp
            });

        }

        public void StartNewGame()
        {
            InitTurn();

            foreach (Player p in Players)
                p.Reset();

            State = EngineStates.Init;
			//Interface.addPanel(new MsgBoxYesNo("You won the toss, Play first?", OnPlayFirst, onDrawFirst));
        }

        public Spell NextSpellOnStack
        {
            get { return MagicStack.Count == 0 ? null : MagicStack.Peek() as Spell; }
        }

        public void PushSpellOnStack(Spell s)
        {
            //should show spell to player...

            MagicStack.Push(s);

            s.Source.Controler.Hand.RemoveCard(s.Source);

            SpellStackLayout.Cards = MagicStack.OfType<Spell>().Select(sp => sp.Source).ToList();
            SpellStackLayout.UpdateLayout();
        }

        public Spell PopSpellFromStack()
        {
            Spell tmp = MagicStack.Pop() as Spell;
            SpellStackLayout.Cards = MagicStack.OfType<Spell>().Select(sp => sp.Source).ToList();
            SpellStackLayout.UpdateLayout();

            //AttachAbility ab = tmp.getAttachAbilityIfPresent;
            //if (ab != null)
            //{
            //    tmp.Source.AttachedTo = tmp.SelectedTargets.FirstOrDefault() as CardInstance;
            //}
            return tmp;
        }



        public void ClickOnCard(CardInstance c)
        {
            if (ip != pp)
            {
                c.SwitchFocus();
                return;
            }

//            if (Magic3D.pCurrentSpell.Visible) //promt for something
//            {
//                if (ip.CurrentSpell != null)
//                {
//                    if (ip.CurrentSpell.SelectedTargets.Count < ip.CurrentSpell.RequiredTargetCount)
//                        if (ip.CurrentSpell.TryToAddTarget(c))
//                            return;
//                }
//                else if (TryToAssignTargetForDamage(c))
//                {
//                    CheckStackForUnasignedDamage();
//                    return;
//                }
//            }

            switch (c.CurrentGroup.GroupName)
            {
                case CardGroups.Library:
                    break;
                case CardGroups.Hand:
                    #region hand
                    if (c.Controler != ip)
                        return;

                    switch (CurrentPhase)
                    {
                        case GamePhases.Untap:
                            break;
                        case GamePhases.Upkeep:
                            break;
                        case GamePhases.Draw:
                            break;
                        case GamePhases.Main1:
                        case GamePhases.Main2:
                            if (c.Model.Types == CardTypes.Land)
                            {
                                if (!landPlayed)
                                    MagicEvent(new MagicEventArg(MagicEventType.PlayLand, c));
                            }
                            else
                                ip.CurrentSpell = new Spell(c);

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
                case CardGroups.InPlay:
                    #region inPlay
                    if (c.Controler != ip &&
                        CurrentPhase != GamePhases.DeclareBlocker &&
                        CurrentPhase != GamePhases.CombatDamage)
                    {
                        c.SwitchFocus();
                        return;
                    }

                    switch (CurrentPhase)
                    {
                        case GamePhases.Untap:
                            break;
                        case GamePhases.Upkeep:
                            break;
                        case GamePhases.Draw:
                            break;
                        case GamePhases.Main1:
                        case GamePhases.Main2:
                            if (c.IsTapped == true)
                                break;
                            if (c.Model.Abilities.Count > 0)
                            {
                                foreach (Ability a in c.Model.Abilities)
                                {
                                    if (a.ActivationCost.CostType == CostTypes.Tap)
                                    {
                                        c.Tap();
                                        MagicEvent(new SpellEventArg { Spell = a });
                                    }
                                }
                            }
                            break;
                        case GamePhases.BeforeCombat:
                            break;
                        case GamePhases.DeclareAttacker:
                            if (!c.Combating && !c.CanAttack)
                                break;
                            c.Combating = !c.Combating;
                            c.CurrentGroup.UpdateLayout();
                            break;
                        case GamePhases.DeclareBlocker:
                            if (c.Controler == ip)
                            {
                                if (c.Combating)
                                {
                                    c.Combating = false;
                                    c.BlockedCreature.BlockingCreatures.Remove(ip.CurrentBlockingCreature);
                                    c.BlockedCreature = null;
                                    c.Controler.InPlay.UpdateLayout();
                                }
                                c.Controler.CurrentBlockingCreature = c;
                                break;
                            }
                            else if (ip.CurrentBlockingCreature != null && c.Combating)
                            {
                                if (ip.CurrentBlockingCreature.Combating)
                                {
                                    //remove blocker
                                    ip.CurrentBlockingCreature.Combating = false;
                                    ip.CurrentBlockingCreature.BlockedCreature.BlockingCreatures.Remove(ip.CurrentBlockingCreature);
                                    ip.CurrentBlockingCreature.BlockedCreature = null;
                                }
                                else if (ip.CurrentBlockingCreature.CanBlock(c))
                                {
                                    //try to add blocker
                                    c.BlockingCreatures.Add(ip.CurrentBlockingCreature);
                                    ip.CurrentBlockingCreature.BlockedCreature = c;
                                    ip.CurrentBlockingCreature.Combating = true;
                                    ip.CurrentBlockingCreature = null;
                                }
                                else
                                    break;

                                ip.InPlay.UpdateLayout();
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
                case CardGroups.Graveyard:
                    c.CurrentGroup.showAll();
                    break;
                case CardGroups.Exhiled:
                    break;
                default:
                    break;
            }
        }

        public void CheckStackForResolutions()
        {
            while (MagicStack.Count > 0)
            {
                if (MagicStack.Peek() is Spell)
                {
                    MagicEvent(new SpellEventArg { Spell = PopSpellFromStack() });
                    continue;
                }

                if (MagicStack.Peek() is Damage)
                {
                    (MagicStack.Pop() as Damage).Deal();
                    continue;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if all damages are assigned</returns>
        public bool CheckStackForUnasignedDamage()
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

        public bool TryToAssignTargetForDamage(CardInstance c)
        {
            foreach (Damage d in MagicStack.ToArray().OfType<Damage>())
            {
                if (d.Target == null)
                {
                    d.Target = c;
					//Magic3D.pCurrentSpell.Visible = false;
                    return true;
                }
            }
            return false;
        }

        public void checkCurrentSpell()
        {
            if (pp.CurrentSpell != null)
            {
                if (pp.CurrentSpell.SelectedTargets.Count < pp.CurrentSpell.RequiredTargetCount)
                {
//                    if (pp.Type == Player.PlayerType.human)
//                        Magic3D.pCurrentSpell.Update(pp.CurrentSpell);
                }

                if (pp.ManaPool != null)
                {
                    pp.CurrentSpell.RemainingCost = pp.CurrentSpell.RemainingCost.Pay(ref pp.ManaPool);
                    if (pp.CurrentSpell.RemainingCost == null)
                    {
                        PushSpellOnStack(pp.CurrentSpell);
                        pp.CurrentSpell = null;
                        GivePriorityToNextPlayer();
                    }
//                    else if (pp.Type == Player.PlayerType.human)
//                        Magic3D.pCurrentSpell.Update(pp.CurrentSpell);
                }
            }
        }

        public void InitTurn()
        {
            landPlayed = false;
        }




        public void initialDraw()
        {
            foreach (Player p in Players)
            {
                for (int i = 0; i < 7; i++)
                    p.DrawOneCard();
            }


        }


    }
}
