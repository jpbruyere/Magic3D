using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Magic3D
{
    public enum EffectType
    {
        Unset,  
        Loose,
        LooseAllAbilities,
        Gain,
        Discard,
        Pump,
        AddPower,
        AddTouchness,
        SetPower,
        SetTouchness,
        Effect,
        Counter,
        Destroy,
        Tap,
        DoesNotUntap,
        CantAttack,
        TapAll,
        LoseLife,
        PreventDamage,
        Charm,
        DealDamage,
        ChangeZone,
        Draw,
        DestroyAll,
        RepeatEach,
        Token,
        GainControl,
        Repeat,
        Debuff,
        ChooseColor,
        Dig,
        PumpAll,
        RemoveCounterAll,
        ChangeZoneAll,
        DamageAll,
        UntapAll,
        PutCounter,
        GainLife,
        PutCounterAll,
        StoreSVar,        
        FlipACoin,
        SacrificeAll,
        Untap,
        Mill,
        Animate,
        Fog,
        RemoveCounter,
        ExchangeZone,
        AnimateAll,
        ChooseCard,
        Reveal,
        ChooseSource,
        MustBlock,
        ExchangeControl,
        RearrangeTopOfLibrary,
        CopyPermanent,
        SetState,
        Balance,
        RevealHand,
        Sacrifice,
        AddTurn,
        TwoPiles,
        ManaReflected,
        SetLife,
        DebuffAll,
        Fight,
        ChooseType,
        Shuffle,
        NameCard,
        PermanentNoncreature,
        PermanentCreature,
        TapOrUntap,
        GenericChoice,
        Play,
        BecomesBlocked,
        AddOrRemoveCounter,
        WinsGame,
        Proliferate,
        Scry,
        MoveCounter,
        GainOwnership,
        ChangeTargets,
        UnattachAll,
        PeekAndReveal,
        LosesGame,
        DigUntil,
        CopySpellAbility,
        RollPlanarDice,
        RegenerateAll,
        DelayedTrigger,
        MustAttack,
        ProtectionAll,
        RemoveFromCombat,
        RestartGame,
        PreventDamageAll,
        ExchangeLife,
        DeclareCombatants,
        ControlPlayer,
        Phases,
        Clone,
        Clash,
        ChooseNumber,
        EachDamage,
        ReorderZone,
        ChoosePlayer,
        EndTurn,
        MultiplePiles
    }

    public class Spell
    {
        public CardInstance Source;
        public Cost RemainingCost;
        

        public Spell()
        { }

        public Spell(CardInstance _cardInstance)
        {
            Source = _cardInstance;
            if (_cardInstance.Model.Cost != null)
                RemainingCost = _cardInstance.Model.Cost.Clone();
        }

        public bool WaitForTarget
        {
            get
            {
                return SelectedTargets.Count < RequiredTargetCount ? true : false;
            }
        }

        public bool TryToAddTarget(CardInstance c)
        {
            if (!WaitForTarget)
                return false;

            if (c.CurrentGroup.GroupName != CardGroups.InPlay)
                return false;


            foreach (CardTarget ct in ValidTargets.Values.OfType<CardTarget>())
            {
                if (c.Model.Types == ct.ValidCardTypes)
                {
                    SelectedTargets.Add(c);
					//Magic3D.pCurrentSpell.Update(this);
                    return true;
                }
            }

            return false;
        }
        public virtual int RequiredTargetCount
        {
            get
            {
                Ability a = Source.getAbilityByType(AbilityEnum.Attach);
                if (a == null)
                    return 0;

                return a.RequiredTargetCount;
            }
            set
            {
                Debug.WriteLine("trying to set required target count for non ability spell");
            }
        }
        public virtual MultiformAttribut<Target> ValidTargets
        {
            get
            {
                Ability a = Source.getAbilityByType(AbilityEnum.Attach);
                if (a == null)
                    return null;

                return a.ValidTargets;
            }
            set
            {
                Debug.WriteLine("trying to set selected targets for non ability spell");
            }
        }
        public virtual List<Object> SelectedTargets
        {
            get
            {
                Ability a = Source.getAbilityByType(AbilityEnum.Attach);
                if (a == null)
                    return new List<object>();

                return a.SelectedTargets;
            }
            set
            {
                Debug.WriteLine("trying to set selected targets for non ability spell");
            }
        }

        public bool Resolve()
        {
            return Source.Model.Cost == RemainingCost ? true : false;
        }
    }
}
