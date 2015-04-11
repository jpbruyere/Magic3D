using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Magic3D
{   
	public class AbilityActivation : MagicAction
	{
		List<object> selectedTargets = new List<object> ();

		public Ability Source;

		public AbilityActivation(CardInstance _source, Ability a) : base(_source)
		{
			Source = a;
			if (!Cost.IsNullOrCountIsZero(Source.ActivationCost))
				RemainingCost = Source.ActivationCost.Clone ();
			else//if it's a spell ab, no cost and no message
				return;

			Magic.AddLog ("Trying to activate " + CardSource.Model.Name + " ability");

			if (CardSource.Controler.ManaPool != null)
				PayCost (ref CardSource.Controler.ManaPool);

			PrintNextMessage ();
		}
		/// <summary>
		/// True as long as adding targets is valid
		/// </summary>
		public bool WaitForTarget
		{
			get
			{
				return SelectedTargets.Count < Source.PossibleTargetCount ? true : false;
			}
		}

		public override void Resolve ()
		{
			switch (Source.AbilityType) {
			case AbilityEnum.Attach:
				(selectedTargets.FirstOrDefault() as CardInstance).AttachCard (CardSource);
				break;
			case AbilityEnum.Enchant:
				(selectedTargets.FirstOrDefault() as CardInstance).AttachCard (CardSource);
				break;
			case AbilityEnum.Equip:
				(selectedTargets.FirstOrDefault() as CardInstance).AttachCard (CardSource);
				break;
			case AbilityEnum.Instant:
				break;
			case AbilityEnum.Interrupt:
				break;
			case AbilityEnum.Mana:
				break;
			default:
				//triggered Ability ou activated
				ActivatedAbility aa = Source as ActivatedAbility;
				if (aa == null) {
					Debug.WriteLine ("unset static ability");
					break;
				}
				aa.Activate(CardSource, SelectedTargets);
				break;
			}
		}

		public override List<object> SelectedTargets {
			get {
				return selectedTargets;
			}
		}
		public override MultiformAttribut<Target> ValidTargets {
			get {
				return Source.ValidTargets;
			}
		}
		public override int RequiredTargetCount {
			get {
				return Source.RequiredTargetCount;
			}
		}

		public override bool IsComplete
		{
			get {
				return (SelectedTargets.Count < RequiredTargetCount) ||
					!base.IsComplete ? false :					
					WaitForTarget && !validated ? false : true;
			}
		}
		public override void PrintNextMessage ()
		{			
			//once a mana has been spent for this ab, prompt for target is shown
			//only if required target count not reached
			if (RemainingCost < Source.ActivationCost) {
				if (SelectedTargets.Count < RequiredTargetCount)
					Magic.AddLog (Source.TargetPrompt);
				else
					base.PrintNextMessage ();
			} else if (RemainingCost != null)
				base.PrintNextMessage ();
			else if (WaitForTarget) {//if cost is paid but some targets could be added
				Magic.AddLog (Source.TargetPrompt);
				Magic.btOk.Visible = true;
			}
		}
		bool validated = false;
		public override void Validate ()
		{
			validated = true;
		}
		public override bool TryToAddTarget (CardInstance c)
		{
			if (!WaitForTarget)
				return false;

			//other target group are possible, should change
			foreach (CardTarget ct in ValidTargets.Values.OfType<CardTarget>())
			{
				if (ct.Accept(c)){
					SelectedTargets.Add(c);
					PrintNextMessage ();					
					return true;
				}
			}

			Magic.AddLog ("Invalid target: " + c.Model.Name);
			PrintNextMessage ();
			return false;
		}
	}

	public class Spell : MagicAction
    {
        
		List<AbilityActivation> spellAbilities = new List<AbilityActivation>();
		List<Ability> completedAbilityActivations = new List<Ability>();
		public AbilityActivation currentAbility = null;

		public MagicAction CurrentAbility {
			get {
				//TODO: double check currentAbility
				if (currentAbility != null){
					if (currentAbility.IsComplete) {
						spellAbilities.Add (currentAbility);
						completedAbilityActivations.Add (currentAbility.Source);
					}
				}
				currentAbility = NextAbilityToProcess;
				return currentAbility;
			}
		}
        
		public override void Resolve ()
		{
			foreach (AbilityActivation aa in spellAbilities) {
				aa.Resolve ();
			}

			//sumoning sickness
			if (CardSource.HasType (CardTypes.Creature) &&
				!CardSource.HasAbility (AbilityEnum.Haste))
				CardSource.HasSummoningSickness = true;

			CardGroupEnum dest = CardGroupEnum.InPlay;

			if (CardSource.HasType (CardTypes.Instant) ||
				CardSource.HasType (CardTypes.Sorcery))
				dest = CardGroupEnum.Graveyard;
			
			CardSource.ChangeZone (dest);

			MagicEngine.CurrentEngine.RaiseMagicEvent (new SpellEventArg (this));
		}

		public Spell(CardInstance _cardInstance) : base(_cardInstance)
        {            

			if (_cardInstance.Model.Cost != null)
				RemainingCost = _cardInstance.Model.Cost.Clone ();

			Magic.AddLog ("Trying to cast: " + CardSource.Model.Name);

			if (CardSource.Controler.ManaPool != null)
				PayCost (ref CardSource.Controler.ManaPool);
			PrintNextMessage ();
        }

		public AbilityActivation NextAbilityToProcess
		{
			get {
				Ability a =
					CardSource.Model.Abilities.Where (
						sma => sma is StaticAbility &&
						!completedAbilityActivations.Contains (sma) && 
						(sma.ActivationCost != null || sma.AcceptTargets)
					).FirstOrDefault();
				
				return a == null ? null :
					new AbilityActivation (CardSource, a);
			}
		}

		public override bool IsComplete
		{			
			get {
				return (CurrentAbility == null) ?
					base.IsComplete : CurrentAbility.IsComplete;
			}
		}
		public override void Validate ()
		{
			if (CurrentAbility != null)
				currentAbility.Validate();
		}

		public override void PrintNextMessage ()
		{
			if (CurrentAbility != null)
				CurrentAbility.PrintNextMessage ();
			else
				base.PrintNextMessage ();
		}

		public override int RequiredTargetCount
		{
			get
			{
				return (CurrentAbility == null) ?
					base.RequiredTargetCount : CurrentAbility.RequiredTargetCount;
			}
		}
		public override MultiformAttribut<Target> ValidTargets {
			get
			{
				return (CurrentAbility == null) ?
					base.ValidTargets : CurrentAbility.ValidTargets;
			}	
		}
		public override List<Object> SelectedTargets
		{
			get
			{
				return (CurrentAbility == null) ?
					base.SelectedTargets : CurrentAbility.SelectedTargets;
			}
		}
		public override bool TryToAddTarget (CardInstance c)
		{
			if (CurrentAbility == null)
				return base.TryToAddTarget (c);
					
			if (CurrentAbility.TryToAddTarget(c))
			{				
				if (currentAbility.IsComplete)
					PrintNextMessage ();
				return true;
			}
			return false;
		}

		public override string ToString ()
		{
			return CardSource.Model.Name;
		}			
    }

	public class MagicAction
	{
		public CardInstance CardSource;
		public bool GoesOnStack = true;

		public MagicAction(CardInstance _source)
		{
			CardSource = _source;
			if (CardSource.HasType (CardTypes.Land))
				GoesOnStack = false;
		}

		public Cost RemainingCost;			
		/// <summary>
		/// Processes the activation of a spell or ability with target selection or cost payment
		/// </summary>
		/// <returns>true if spellActivation has all targets and the cost(s) is paid</returns>
		public virtual bool IsComplete
		{
			get { return Cost.IsNullOrCountIsZero(RemainingCost);	}
		}
		//				if (_currentSpell != null) {
		//					if (_currentSpell.RemainingCost != _currentSpell.Source.Model.Cost) {
		//						//put already spent mana back into Manapool
		//						ManaPool = _currentSpell.Source.Model.Cost - _currentSpell.RemainingCost;
		//					}
		//				}
		//                    else if (pp.Type == Player.PlayerType.human)
		//                        Magic3D.pCurrentSpell.Update(pp.CurrentSpell);

		public virtual void Resolve()
		{
		}

		public virtual void PayCost(ref Cost _amount)
		{
			RemainingCost = RemainingCost.Pay (ref _amount);
			PrintNextMessage ();
		}
		public virtual void PrintNextMessage()
		{
			if (!Cost.IsNullOrCountIsZero(RemainingCost))
				Magic.AddLog ("\t remaining cost: " + RemainingCost.ToString());
		}

		public virtual bool TryToAddTarget(CardInstance c)
		{
			return false;
		}
		public virtual void Validate()
		{
		}
		public virtual int RequiredTargetCount { get { return 0; } }
		public virtual MultiformAttribut<Target> ValidTargets {
			get { return null; }
		}
		public virtual List<Object> SelectedTargets {
			get { return new List<object>(); }
		}
	}
}
