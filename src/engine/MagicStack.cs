﻿//
//  MagicStack.cs
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
using Crow;
using System.Diagnostics;
using OpenTK;
using System.Linq;

namespace Magic3D
{
	public class MagicStack : Stack<MagicStackElement> , IValueChange
	{
		#region IValueChange implementation
		public event EventHandler<ValueChangeEventArgs> ValueChanged;
		public virtual void NotifyValueChanged(string MemberName, object _value)
		{
			ValueChanged.Raise(this, new ValueChangeEventArgs(MemberName, _value));			
		}
		#endregion

		MagicEngine engine;
		public CardLayout SpellStackLayout = new CardLayout ();

		#region CTOR
		public MagicStack (MagicEngine _engine) : base()
		{
			engine = _engine;

			SpellStackLayout.Position = new Vector3 (0, 0, 2);//Magic.vGroupedFocusedPoint;
			SpellStackLayout.HorizontalSpacing = 0.1f;
			SpellStackLayout.VerticalSpacing = 0.3f;
			SpellStackLayout.MaxHorizontalSpace = 3f;
			//SpellStackLayout.xAngle = Magic.FocusAngle;
		}
		#endregion

		#region Stack managment
		public MagicAction NextActionOnStack {
			get { return this.Count == 0 ? null :
				this.Peek () is MagicAction ?
				this.Peek () as MagicAction : null; }
		}
		public List<MagicStackElement> Choices {
			get { return UIActionIsChoice ? 
				(this.Peek() as MagicChoice).Choices:new List<MagicStackElement>(); }
		}
		public bool UIActionIsChoice{
			get { 
				if (this.Count == 0)
					return false;
				MagicStackElement mse = this.Peek ();
				if (mse.Player != engine.ip)
					return false;				
				return mse is MagicChoice;
			}
		}

		public bool UIPlayerActionIsOnStack {
			get { 
				return this.Count == 0 ? false : 
					((this.Peek ()).Player == engine.ip);
			}
		}

		public string UIPlayerTitle {
			get { return UIPlayerActionIsOnStack ? 
				this.Peek().Title	: "";
			}
		}
		public string UIPlayerMessage {
			get { return UIPlayerActionIsOnStack ? 
				this.Peek().Message	: "";
			}
		}
		public String[] CostElements
		{ get{ return UIPlayerActionIsOnStack ? this.Peek ().MSECostElements : null; }}
		public bool CostIsNotNull
		{ get { return CostElements != null; }}
		public String[] OtherCostElements
		{ get{ return UIPlayerActionIsOnStack ? this.Peek ().MSEOtherCostElements : null; }}
		public bool MessageIsNotNull
		{ get { return !string.IsNullOrEmpty(UIPlayerMessage); }}

		public void PushOnStack (MagicStackElement s)
		{			
			this.Push (s);
			notifyStackElementChange ();
			Magic.CurrentGameWin.NotifyValueChange ("MagicStack", this.ToList());
		}
		public MagicStackElement PopMSE()
		{
			MagicStackElement tmp = this.Pop ();
			notifyStackElementChange ();
			Magic.CurrentGameWin.NotifyValueChange ("MagicStack", this.ToList());
			return tmp;
		}

		void notifyStackElementChange(){
			bool uipaios = UIPlayerActionIsOnStack;
			NotifyValueChanged ("UIPlayerActionIsOnStack", uipaios);
			if (!uipaios)
				return;
			NotifyValueChanged ("UIPlayerTitle", UIPlayerTitle);
			NotifyValueChanged ("UIPlayerMessage", UIPlayerMessage);
			NotifyValueChanged ("CostElements", CostElements);
			NotifyValueChanged ("MSEOtherCostElements", OtherCostElements);
			NotifyValueChanged ("CostIsNotNull", CostIsNotNull);
			NotifyValueChanged ("MessageIsNotNull", MessageIsNotNull);
			NotifyValueChanged ("UIActionIsChoice", UIActionIsChoice);
			if (UIActionIsChoice)
				NotifyValueChanged ("Choices", Choices);
		}

		//TODO:should be changed...
		public void ClearIncompleteActions()
		{
			while (this.Count > 0) {
				if (Peek () is Damage)
					return;
				//TODO: check if choice may be canceled
				if (Peek () is MagicChoice) {
					PopMSE ();
					return;
				}
				if ((Peek () as MagicAction).IsComplete)
					break;
				PopMSE ();
			}
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
			if (ma.CardSource.Controler != engine.pp) {
				Debug.Print ("Nothing to cancel");
				return true;
			}
			if (ma.IsComplete)
				Debug.Print ("Canceling completed action");
			if (ma.IsMandatory) {
				Debug.Print ("Unable to cancel mandatory action");
				return false;
			}

			PopMSE ();
			return true;
		}

		public void ResolveStack ()
		{
			while (Count > 0) {
				if (Peek () is MagicAction) {

					if (!(Peek () as MagicAction).IsComplete)
						break;

					MagicAction ma = PopMSE() as MagicAction;

					UpdateStackLayouting ();

					ma.Resolve ();

					continue;
				}

 				if (this.Peek () is Damage) {
					(PopMSE() as Damage).Deal ();
					continue;
				}
			}
		}
		/// <summary>
		/// Check completeness of last action on stack.
		/// </summary>
		public void CheckLastActionOnStack ()
		{
			if (this.Count == 0)
				return;

			MagicAction ma = this.Peek () as MagicAction;

			if (ma == null)
				return;

			if (ma.CardSource != null){
				if (ma.CardSource.Controler != engine.pp)
					return;
			}
			Magic.CurrentGameWin.CursorVisible = true;
			if (!ma.IsComplete) {
				if (ma.remainingCost == CostTypes.Tap) {
					ma.remainingCost = null;
					ma.CardSource.Tap ();
				} else if ((engine.pp.AvailableManaOnTable + engine.pp.ManaPool) < ma.remainingCost) {
					Magic.AddLog ("Not enough mana available");
					CancelLastActionOnStack ();
					return;
				} else if (engine.pp.ManaPool != null && ma.RemainingCost != null) {
					ma.PayCost (ref engine.pp.ManaPool);
					engine.pp.NotifyValueChange ("ManaPoolElements", engine.pp.ManaPoolElements);
					engine.pp.UpdateUi ();
					notifyStackElementChange ();
				}

				//				if (ma.IsComplete && ma.GoesOnStack)
				//					GivePriorityToNextPlayer ();				

			}
			if (ma.IsComplete){
				if (ma.GoesOnStack) {
					//should show spell to player...
					UpdateStackLayouting();
					engine.GivePriorityToNextPlayer ();				
				} else {
					PopMSE ();
					ma.Resolve ();
				}
				return;
			}


			engine.pp.UpdateUi ();

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
		public bool TryToHandleClick(Object target)
		{
			Magic.CurrentGameWin.CursorVisible = true;
			MagicAction ma = NextActionOnStack;
			if (ma != null) {
				if (!ma.IsComplete) {
					if (ma.TryToAddTarget (target)) {
						notifyStackElementChange ();
						return true;
					}
				}
			}
			if (!(target is CardInstance))
				return false;
			if (TryToAssignTargetForDamage(target as CardInstance))
			{
				notifyStackElementChange ();
				CheckStackForUnasignedDamage();//TODO:this do nothing...
				return true;
			}			
			return false;
		}
		/// <returns>true if all damages are assigned</returns>
		public bool CheckStackForUnasignedDamage ()
		{
			foreach (Damage d in this.ToArray().OfType<Damage>())
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
			foreach (Damage d in this.ToArray().OfType<Damage>()) {
				if (d.Target == null) {
					d.Target = c;
					//Magic3D.pCurrentSpell.Visible = false;
					return true;
				}
			}
			return false;
		}

		public void UpdateStackLayouting()
		{
			SpellStackLayout.Cards.Clear ();
			foreach (MagicAction ma in this.OfType<MagicAction>()) {
				if (ma is Spell)
					SpellStackLayout.Cards.Add ((ma as Spell).CardSource);
				else if (ma is AbilityActivation)
					SpellStackLayout.Cards.Add (new CardInstance(ma));
			}
			SpellStackLayout.UpdateLayout ();			
		}
		#endregion
	}
}

