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
			if (!Cost.IsNullOrCountIsZero (Source.ActivationCost)) {
				RemainingCost = Source.ActivationCost.Clone ();
				RemainingCost.OrderFirst (Source.ActivationCost.GetDominantMana ());
			}
			
//			else//if it's a spell ab, no cost and no message
//				return;

			if (Source.Mandatory)
				Magic.AddLog (CardSource.Model.Name + " ability activation.");
			else
				Magic.AddLog ("Trying to activate " + CardSource.Model.Name + " ability");

			if (CardSource.Controler.ManaPool != null && RemainingCost != null)
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
		public override bool IsMandatory {
			get {
				return Source.Mandatory;
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
			if (!IsMandatory)
				Magic.btOk.Visible = true;
			//once a mana has been spent for this ab, prompt for target is shown
			//only if required target count not reached
			if (RemainingCost < Source.ActivationCost) {
				if (SelectedTargets.Count < RequiredTargetCount)
					Magic.AddLog (Source.TargetPrompt);
				else
					base.PrintNextMessage ();
			} else if (RemainingCost != null)
				base.PrintNextMessage ();
			else if (WaitForTarget) {
				Magic.AddLog (Source.TargetPrompt);
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
				if (ct.Accept(c,CardSource)){
					SelectedTargets.Add(c);
					PrintNextMessage ();					
					return true;
				}
			}

			Magic.AddLog ("Invalid target: " + c.Model.Name);
			PrintNextMessage ();
			return false;
		}

		public override void Resolve ()
		{
			Magic.btOk.Visible = false;

			switch (Source.AbilityType) {
			case AbilityEnum.Attach:
				if (CardSource.IsAttached)
					CardSource.AttachedTo.DetacheCard (CardSource);
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
				ManaAbility ma = Source as ManaAbility;
				Player p = CardSource.Controler;
				p.ManaPool += ma.ProducedMana.Clone();
				p.UpdateUi ();
				break;
			default:
				//triggered Ability ou activated
				if (!(this.Source.IsActivatedAbility||this.Source.IsTriggeredAbility)) {
					Debug.WriteLine ("unset static ability");
				}
				this.Source.Activate(CardSource, SelectedTargets);
				break;
			}
			MagicEngine.CurrentEngine.RaiseMagicEvent (new ActivatedAbilityEventArg (Source, CardSource));	
			MagicEngine.CurrentEngine.UpdateOverlays ();
		}
	}

	public class Spell : MagicAction
    {
		#region CTOR
		public Spell(CardInstance _cardInstance) : base(_cardInstance)
		{            

			if (_cardInstance.Model.Cost != null) {
				RemainingCost = _cardInstance.Model.Cost.Clone ();
				RemainingCost.OrderFirst(_cardInstance.Model.Cost.GetDominantMana());
			}

			Magic.AddLog ("Trying to cast: " + CardSource.Model.Name);

			if (CardSource.Controler.ManaPool != null)
				PayCost (ref CardSource.Controler.ManaPool);
			
			PrintNextMessage ();
		}
		#endregion
        
		List<AbilityActivation> spellAbilities = new List<AbilityActivation>();
		AbilityActivation currentAbilityActivation = null;

		public MagicAction CurrentAbility {
			get {
				//TODO: double check currentAbility
				if (currentAbilityActivation != null){
					if (currentAbilityActivation.IsComplete) {
						spellAbilities.Add (currentAbilityActivation);
						currentAbilityActivation = null;
					}
				}

				if (currentAbilityActivation == null)
					currentAbilityActivation = NextAbilityToProcess;
				
				return currentAbilityActivation;
			}
		}
		public AbilityActivation NextAbilityToProcess
		{
			get {
				IEnumerable<Ability> abs = spellAbilities.Select (saa => saa.Source);
				Ability a =
					CardSource.Model.Abilities.Where (
						sma => sma.Category == AbilityCategory.Spell &&
						!abs.Contains (sma) && 
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
		public override int RequiredTargetCount
		{
			get
			{
				return (CurrentAbility == null) ?
					RequiredTargetCount : CurrentAbility.RequiredTargetCount;
			}
		}
		public override MultiformAttribut<Target> ValidTargets {
			get
			{
				return (CurrentAbility == null) ?
					ValidTargets : CurrentAbility.ValidTargets;
			}	
		}
		public override List<Object> SelectedTargets
		{
			get
			{
				return (CurrentAbility == null) ?
					SelectedTargets : CurrentAbility.SelectedTargets;
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
			MagicEngine.CurrentEngine.UpdateOverlays ();
		}			
		public override void Validate ()
		{
			if (CurrentAbility != null)
				currentAbilityActivation.Validate();
		}
		public override bool IsMandatory {
			get {
				return false;
			}
		}

		public override bool TryToAddTarget (CardInstance c)
		{
			if (CurrentAbility == null)
				return false;
					
			if (CurrentAbility.TryToAddTarget(c))
			{				
				if (currentAbilityActivation.IsComplete)
					PrintNextMessage ();
				return true;
			}
			return false;
		}

		public override void PrintNextMessage ()
		{
			if (CurrentAbility != null)
				CurrentAbility.PrintNextMessage ();
			else
				base.PrintNextMessage ();
		}

		public override string ToString ()
		{
			return CardSource.Model.Name;
		}			
    }

	public abstract class MagicAction
	{
		public CardInstance CardSource;
		public Cost RemainingCost;
		public bool GoesOnStack = true;

		public MagicAction(CardInstance _source)
		{
			CardSource = _source;
			if (CardSource.HasType (CardTypes.Land))
				GoesOnStack = false;
		}
						
		public virtual void PayCost(ref Cost _amount)
		{
			RemainingCost = RemainingCost.Pay (ref _amount);
			PrintNextMessage ();
		}
		public virtual bool IsComplete {
			get { return Cost.IsNullOrCountIsZero(RemainingCost); }
		}
		public virtual void PrintNextMessage()
		{
			if (!Cost.IsNullOrCountIsZero(RemainingCost))
				Magic.AddLog ("\t remaining cost: " + RemainingCost.ToString());
		}
			
		/// <summary>
		/// Processes the activation of a spell or ability with target selection or cost payment
		/// </summary>
		/// <returns>true if spellActivation has all possible targets and the cost(s) is paid</returns>
		public abstract void Resolve ();
		public abstract bool TryToAddTarget (CardInstance c);
		public abstract void Validate ();
		public abstract int RequiredTargetCount { get; }
		public abstract MultiformAttribut<Target> ValidTargets { get; }
		public abstract List<Object> SelectedTargets { get; }
		public abstract bool IsMandatory { get; }
	}
}
