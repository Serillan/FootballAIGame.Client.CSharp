﻿using FootballAIGame.Client.AIs.Fsm.Messaging;
using FootballAIGame.Client.AIs.Fsm.PlayerStates;
using FootballAIGame.Client.AIs.Fsm.PlayerStates.GlobalStates;
using FootballAIGame.Client.SimulationEntities;

namespace FootballAIGame.Client.AIs.Fsm.Entities
{
    class Midfielder : FieldPlayer
    {
        public Midfielder(FootballPlayer player, FsmAI footballAI) : base(player, footballAI)
        {
            StateMachine = new FiniteStateMachine<Player>(this, new Default(this, footballAI), new MidfielderGlobalState(this, footballAI));
        }

        public override void ProcessMessage(IMessage message)
        {
            StateMachine.ProcessMessage(this, message);
        }

    }
}