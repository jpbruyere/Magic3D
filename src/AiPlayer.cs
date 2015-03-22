using System;
using System.Diagnostics;
using System.Linq;
using go;

namespace Magic3D
{
	public class AiPlayer : Player
	{
		#region CTOR
		public AiPlayer () : base()
		{
			Type = PlayerType.ai;
		}
		#endregion

		public override void initInterface (go.OpenTKGameWindow mainWin)
		{
			base.initInterface (mainWin);
			(playerPanel.FindByName ("pic") as Image).ImagePath = "image2/HAL9000.svg";
		}

		public override void Process ()
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
					if (AllowedLandsToBePlayed > 0)
						if (AITryToPlayLand ())
							break;
					if (!CastAvailableAndAllowedCreature())
						PhaseDone = true;
					break;
				case GamePhases.BeforeCombat:
					PhaseDone = true;
					break;
				case GamePhases.DeclareAttacker:
					AITryToAttack ();
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
			}		}

		public void aiPayManaIfNeedeed()
		{
			if (CurrentSpell != null)
			{
				if (CurrentSpell.RemainingCost != null)
				{

				}
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
		public bool AITryToPlayLand()
		{            
			CardInstance[] lands = Hand.Cards.Where(c => c.Model.Types == CardTypes.Land).ToArray();

			if (lands.Length > 0) {
				MagicEngine.CurrentEngine.RaiseMagicEvent (new MagicEventArg (MagicEventType.PlayLand, lands [0]));
			}
			return false;
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

	}
}

