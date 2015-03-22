using System;
using System.Linq;

namespace Magic3D
{
	public class InPlayGroup : CardGroup
	{
		public CardLayout LandsLayout = new CardLayout();
		public CardLayout CreatureLayout = new CardLayout();
		public CardLayout OtherLayout = new CardLayout();
		public CardLayout CombatingCreature = new CardLayout();

		public InPlayGroup()
			: base(CardGroupEnum.InPlay)
		{
			y = -2.5f;
			HorizontalSpacing = 1.5f;
			MaxHorizontalSpace = 7f;

			LandsLayout.x = -1f;
			LandsLayout.y = -4.2f;
			LandsLayout.HorizontalSpacing = 1.5f;
			LandsLayout.VerticalSpacing = -0.001f;
			LandsLayout.MaxHorizontalSpace = 4f;

			CreatureLayout.x = 0f;
			CreatureLayout.y = -1.5f;
			CreatureLayout.HorizontalSpacing = 1.5f;
			CreatureLayout.VerticalSpacing = 0.001f;
			CreatureLayout.MaxHorizontalSpace = 6f;

			OtherLayout.x = 2f; 
			OtherLayout.y = -2.7f;
			OtherLayout.HorizontalSpacing = 1.5f;
			OtherLayout.VerticalSpacing = 0.001f;
			OtherLayout.MaxHorizontalSpace = 3f;

			CombatingCreature.x = -0;
			CombatingCreature.y = -0.6f;
			CombatingCreature.HorizontalSpacing = 1.5f;
			CombatingCreature.VerticalSpacing = 0.001f;
			CombatingCreature.MaxHorizontalSpace = 7f;
		}

		public override void AddCard(CardInstance c)
		{
			base.AddCard(c);
			UpdateLayout();
		}
		public override void RemoveCard(CardInstance c)
		{
			base.RemoveCard(c);
			UpdateLayout();
		}
		public override void UpdateLayout()
		{
			LandsLayout.Cards = Cards.Where(c => c.Model.Types == CardTypes.Land && !(c.IsAttached || c.Combating)).ToList();
			CreatureLayout.Cards = Cards.Where(c => c.Model.Types == CardTypes.Creature && !(c.IsAttached || c.Combating)).ToList();
			OtherLayout.Cards = Cards.Where(c => c.Model.Types != CardTypes.Land && c.Model.Types != CardTypes.Creature
				&& !(c.IsAttached || c.Combating)).ToList();

			LandsLayout.UpdateLayout();
			CreatureLayout.UpdateLayout();
			OtherLayout.UpdateLayout();

			if (MagicEngine.CurrentEngine.CurrentPhase > GamePhases.BeforeCombat &&
				MagicEngine.CurrentEngine.CurrentPhase <= GamePhases.EndOfCombat)
			{
				CombatingCreature.Cards = Cards.Where(c => c.Model.Types == CardTypes.Creature && c.Combating).ToList();
				if (CombatingCreature.Cards.Count == 0)
					return;
				if (MagicEngine.CurrentEngine.cp == Cards[0].Controler)
					CombatingCreature.UpdateLayout();
				else
					CombatingCreature.UpdateDefendersLayout();
			}
			//base.UpdateLayout();
		}
	}
}

