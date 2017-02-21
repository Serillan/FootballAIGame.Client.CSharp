﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using FootballAIGame.AI.FSM.SimulationEntities;
using FootballAIGame.AI.FSM.UserClasses.TeamStates;

namespace FootballAIGame.AI.FSM.UserClasses.Entities
{
    class Team
    {
        public FiniteStateMachine<Team> StateMachine { get; set; }

        public Player[] Players { get; set; }

        public GoalKeeper GoalKeeper { get; set; }

        public List<Defender> Defenders { get; set; }

        public List<Midfielder> Midfielders { get; set; }

        public List<Forward> Forwards { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the team holds currently the left goal post.
        /// </summary>
        public bool IsOnLeft { get; set; }

        public Team(IList<FootballPlayer> footballPlayers)
        {
            StateMachine = new FiniteStateMachine<Team>(this, 
                TeamStates.Attacking.Instance, TeamStates.TeamGlobalState.Instance);

            Players = new Player[11];

            GoalKeeper = new GoalKeeper(footballPlayers[0]);
            Players[0] = GoalKeeper;

            Defenders = new List<Defender>(4);
            for (int i = 1; i <= 4; i++)
            {
                var defender = new Defender(footballPlayers[i]);
                Defenders.Add(defender);
                Players[i] = defender;
            }

            Midfielders = new List<Midfielder>(4);
            for (int i = 5; i <= 8; i++)
            {
                var midfielder = new Midfielder(footballPlayers[i]);
                Midfielders.Add(midfielder);
                Players[i] = midfielder;
            }

            Forwards = new List<Forward>(2);
            for (int i = 9; i <= 10; i++)
            {
                var forward = new Forward(footballPlayers[i]);
                Forwards.Add(forward);
                Players[i] = forward;
            }

            Defending.Instance.SetHomeRegions(this);
        }

        public PlayerAction[] GetActions()
        {
            StateMachine.Update();

            var actions = new PlayerAction[11];
            for (int i = 0; i < 11; i++)
            {
                actions[i] = Players[i].GetAction();
            }

            return actions;
        }
    }
}
