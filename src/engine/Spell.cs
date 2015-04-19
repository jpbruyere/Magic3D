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
				remainingCost = Source.ActivationCost.Clone ();
				remainingCost.OrderFirst (Source.ActivationCost.GetDominantMana ());
			}
			
//			else//if it's a spell ab, no cost and no message
//				return;

			if (CardSource != null) {
				if (Source.Mandatory)
					Magic.AddLog (CardSource.Model.Name + " ability activation.");
				else
					Magic.AddLog (Source.Message);

				if (CardSource.Controler.ManaPool != null && remainingCost != null)
					PayCost (ref CardSource.Controler.ManaPool);
			}

			PrintNextMessage ();

			if (Source.Category == AbilityCategory.Spell)
				return;
			if (IsComplete && GoesOnStack)
				MagicEngine.CurrentEngine.GivePriorityToNextPlayer ();
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
			if (remainingCost < Source.ActivationCost) {
				if (SelectedTargets.Count < RequiredTargetCount)
					Magic.AddLog (Source.TargetPrompt);
				else
					base.PrintNextMessage ();
			} else if (remainingCost != null)
				base.PrintNextMessage ();
			else if (WaitForTarget) {
				Magic.AddLog (Source.TargetPrompt);
			}

			if (CardSource == null)
				return;
			
			//show library cards if needeed
			if (WaitForTarget && ValidTargets != null) {
				if (ValidTargets.Values.OfType<CardTarget> ().Where
					(cct => cct.ValidGroup == CardGroupEnum.Library).Count () > 0) {
					if (!CardSource.Controler.Library.IsExpanded)
						CardSource.Controler.Library.toogleShowAll();
				}else if (CardSource.Controler.Library.IsExpanded)
					CardSource.Controler.Library.toogleShowAll();
			}
		}
		bool validated = false;
		/// <summary>
		/// action will be complete if MinTarget <= targets <= MaxTarget 
		/// </summary>
		public override void Validate ()
		{
			validated = true;
			if (!IsComplete && MagicEngine.CurrentEngine.NextActionOnStack == this)
				MagicEngine.CurrentEngine.CancelLastActionOnStack ();
		}
		public override bool TryToAddTarget (object c)
		{
			if (!WaitForTarget)
				return false;

			//other target group are possible, should change
			foreach (Target ct in ValidTargets.Values)
			{
				if (ct.Accept(c,CardSource)){
					SelectedTargets.Add(c);
					PrintNextMessage ();					
					return true;
				}
			}

			Magic.AddLog ("Invalid target: " + c.ToString());
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
			case AbilityEnum.Kicker:
				CardSource.Kicked = true;
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
				remainingCost = _cardInstance.Model.Cost.Clone ();
				remainingCost.OrderFirst(_cardInstance.Model.Cost.GetDominantMana());
			}

			Magic.AddLog ("Trying to cast: " + CardSource.Model.Name);

			if (CardSource.Controler.ManaPool != null)
				PayCost (ref CardSource.Controler.ManaPool);
			
			PrintNextMessage ();

			if (IsComplete && GoesOnStack)
				MagicEngine.CurrentEngine.GivePriorityToNextPlayer ();
		}
		#endregion
        
		List<AbilityActivation> spellAbilities = new List<AbilityActivation>();
		AbilityActivation currentAbilityActivation = null;
		public override Cost RemainingCost {
			get {
				return currentAbilityActivation == null ? remainingCost : currentAbilityActivation.RemainingCost;
			}
			set {
				if (currentAbilityActivation == null)
					remainingCost = value;
				else
					currentAbilityActivation.RemainingCost = value;
			}
		}
		public MagicAction CurrentAbility {
			get {
				if (currentAbilityActivation == null)
					currentAbilityActivation = NextAbilityToProcess;
				while (currentAbilityActivation != null) {
					if (currentAbilityActivation.IsComplete) {
						spellAbilities.Add (currentAbilityActivation);
						currentAbilityActivation = null;
					} else
						return currentAbilityActivation;
					currentAbilityActivation = NextAbilityToProcess;
				}		
				return null;
			}
		}

		public AbilityActivation NextAbilityToProcess
		{
			get {
				//already done activation
				IEnumerable<Ability> abs = spellAbilities.Select (saa => saa.Source);

				//add kicker if base cost is not paid to exclude list
				//it will be processed only when base cost is paid
				if (remainingCost != null)
					abs = abs.Concat (CardSource.Model.Abilities.Where (ema => ema.AbilityType == AbilityEnum.Kicker));				
				
				Ability a =
					CardSource.Model.Abilities.Where (
						sma => sma.Category == AbilityCategory.Spell &&
						!abs.Contains (sma)
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
			foreach (AbilityActivation aa in spellAbilities.Where(sa => sa.IsComplete)) {
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
			if (currentAbilityActivation == null){
				MagicEngine.CurrentEngine.CancelLastActionOnStack ();
				return;//maybe cancel spell if not completed
			}
			if (currentAbilityActivation.IsMandatory)
				return;

			currentAbilityActivation.Validate ();

			spellAbilities.Add (currentAbilityActivation);
			currentAbilityActivation = null;
		}
		public override bool IsMandatory {
			get {
				return false;
			}
		}

		public override bool TryToAddTarget (object c)
		{
			if (CurrentAbility == null)
				return false;
					
			if (CurrentAbility.TryToAddTarget(c))
			{				
				//trick to force update of current ability
				//should simplify currentAbilityActivation update
				MagicAction tmp = CurrentAbility;
				PrintNextMessage ();
				return true;
			}
			return false;
		}

		public override void PrintNextMessage ()
		{
			if (currentAbilityActivation != null)
				currentAbilityActivation.PrintNextMessage ();
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
		public Cost remainingCost;

		public virtual Cost RemainingCost {
			get {
				return remainingCost;
			}
			set {
				remainingCost = value;
			}
		}

		public bool GoesOnStack = true;

		public MagicAction(CardInstance _source)
		{
			if (_source == null)
				return;
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
			get { return Cost.IsNullOrCountIsZero(remainingCost); }
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
		public abstract bool TryToAddTarget (object c);
		public abstract void Validate ();
		public abstract int RequiredTargetCount { get; }
		public abstract MultiformAttribut<Target> ValidTargets { get; }
		public abstract List<Object> SelectedTargets { get; }
		public abstract bool IsMandatory { get; }
	}
}
