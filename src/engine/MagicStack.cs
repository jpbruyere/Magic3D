//
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
using go;
using System.Diagnostics;
using OpenTK;
using System.Linq;

namespace Magic3D
{
	public class MagicStack : Stack<object> , IValueChange
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
		public string UIPlayerTitle {
			get { return UIPlayerActionIsOnStack ? 
				NextActionOnStack.Title	: "";
			}
		}
		public string UIPlayerMessage {
			get { return UIPlayerActionIsOnStack ? 
				NextActionOnStack.Message	: "";
			}
		}
		public bool UIPlayerActionIsOnStack {
			get { return NextActionOnStack == null ? false : 
				NextActionOnStack.CardSource.Controler == engine.ip ? true : false; }
		}
		public String[] CostElements
		{
			get{
				if (!UIPlayerActionIsOnStack)
					return null;
				MagicAction ma = this.Peek () as MagicAction;
				if (ma.MSERemainingCost == null)
					return null;
				string tmp = ma.MSERemainingCost.ToString ();
				return tmp.Split(' ').Where(cc => cc.Length < 3).ToArray();
			}
		}
		public void PushOnStack (object s)
		{			
			this.Push (s);
			notifyStackElementChange ();
		}

		void notifyStackElementChange(){
			NotifyValueChanged ("UIPlayerActionIsOnStack", UIPlayerActionIsOnStack);
			NotifyValueChanged ("UIPlayerTitle", UIPlayerTitle);
			NotifyValueChanged ("UIPlayerMessage", UIPlayerMessage);
			NotifyValueChanged ("CostElements", CostElements);
		}

		//TODO:should be changed...
		public void ClearIncompleteActions()
		{
			while (this.Count > 0) {
				if (Peek () is Damage)
					return;
				if ((Peek () as MagicAction).IsComplete)
					break;
				PopMSE ();
			}
		}
		public MagicStackElement PopMSE()
		{
			MagicStackElement tmp = this.Pop () as MagicStackElement;
			notifyStackElementChange ();
			return tmp;
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
		public bool TryToHandleClick(CardInstance c)
		{			
			MagicAction ma = NextActionOnStack;
			if (ma != null) {
				if (!ma.IsComplete) {
					if (ma.TryToAddTarget (c))
						return true;
				}
			}

			if (TryToAssignTargetForDamage(c))
			{
				CheckStackForUnasignedDamage();
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

