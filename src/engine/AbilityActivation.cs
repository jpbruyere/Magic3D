//
//  AbilityActivation.cs
//
//  Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
//  Copyright (c) 2015 jp
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
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

				if (CardSource.Controler.ManaPool != null && remainingCost != null) {
					PayCost (ref CardSource.Controler.ManaPool);
					CardSource.Controler.NotifyValueChange ("ManaPoolElements", CardSource.Controler.ManaPoolElements);
				}
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

			object target = c;
			if (target is CardInstance) {
				CardInstance ci = target as CardInstance;
				if (ci.BindedAction != null)
					target = ci.BindedAction;
			}

			//other target group are possible, should change
			foreach (Target ct in ValidTargets.Values)
			{
				if (ct.Accept(target, CardSource)){
					SelectedTargets.Add(target);
					PrintNextMessage ();					
					return true;
				}
			}

			Magic.AddLog ("Invalid target: " + target.ToString());
			PrintNextMessage ();
			return false;
		}

		public override void Resolve ()
		{
			Magic.btOk.Visible = false;

			if (IsCountered)
				return;

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
			//MagicEngine.CurrentEngine.UpdateOverlays ();
		}
	}

}

