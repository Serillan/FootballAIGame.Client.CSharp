﻿using System.Collections.Generic;
using FootballAIGame.Client.AIs.Fsm.Entities;
using FootballAIGame.Client.AIs.Fsm.SteeringBehaviors;
using FootballAIGame.Client.CustomDataTypes;

namespace FootballAIGame.Client.AIs.Fsm.TeamStates
{
    class Defending : TeamState
    {
        private List<Interpose> Interposes { get; set; }

        public Defending(Team team, FsmAI footballAI) : base(team, footballAI)
        {
        }

        public override void Enter()
        {
            SetHomeRegions();

            Interposes = new List<Interpose>();

            var controllingOpponent = AI.OpponentTeam.NearestPlayerToBall;

            var firstNearestToControlling = AI.OpponentTeam.GetNearestPlayerToPosition(
                controllingOpponent.Position, controllingOpponent);

            var secondNearestToControlling = AI.OpponentTeam.GetNearestPlayerToPosition(
                controllingOpponent.Position, controllingOpponent, firstNearestToControlling);

            var interpose1 = new Interpose(Team.Forwards[0], 2, 0.8, controllingOpponent, firstNearestToControlling);
            var interpose2 = new Interpose(Team.Forwards[1], 2, 0.8, controllingOpponent, secondNearestToControlling);

            Interposes.Add(interpose1);
            Interposes.Add(interpose2);

            Team.Forwards[0].SteeringBehaviorsManager.AddBehavior(interpose1);
            Team.Forwards[1].SteeringBehaviorsManager.AddBehavior(interpose2);
        }

        public override void Run()
        {
            if (Team.PlayerInBallRange != null && AI.OpponentTeam.PlayerInBallRange == null)
            {
                Team.StateMachine.ChangeState(new Attacking(Team, AI));
                return;
            }

            UpdateSteeringBehaviors();
        }

        private void UpdateSteeringBehaviors()
        {
            var controllingOpponent = AI.OpponentTeam.NearestPlayerToBall;

            var firstNearestToControlling = AI.OpponentTeam.GetNearestPlayerToPosition(
                controllingOpponent.Position, controllingOpponent);

            var secondNearestToControlling = AI.OpponentTeam.GetNearestPlayerToPosition(
                controllingOpponent.Position, controllingOpponent, firstNearestToControlling);

            Interposes[0].First = controllingOpponent;
            Interposes[1].First = controllingOpponent;

            Interposes[0].Second = firstNearestToControlling;
            Interposes[1].Second = secondNearestToControlling;
        }

        public override void Exit()
        {
            for(int i = 0; i < 2; i++)
                Team.Forwards[i].SteeringBehaviorsManager.RemoveBehavior(Interposes[i]);
        }

        public override void SetHomeRegions()
        {
            Team.GoalKeeper.HomeRegion = Region.GetRegion(0, 4);

            Team.Defenders[0].HomeRegion = Region.GetRegion(1, 1);
            Team.Defenders[1].HomeRegion = Region.GetRegion(1, 3);
            Team.Defenders[2].HomeRegion = Region.GetRegion(1, 5);
            Team.Defenders[3].HomeRegion = Region.GetRegion(1, 7);

            Team.Midfielders[0].HomeRegion = Region.GetRegion(2, 1);
            Team.Midfielders[1].HomeRegion = Region.GetRegion(2, 3);
            Team.Midfielders[2].HomeRegion = Region.GetRegion(2, 5);
            Team.Midfielders[3].HomeRegion = Region.GetRegion(2, 7);

            Team.Forwards[0].HomeRegion = Region.GetRegion(3, 2);
            Team.Forwards[1].HomeRegion = Region.GetRegion(3, 6);

            if (Team.IsOnLeft) return;

            // team is on the right side -> mirror image
            foreach (var player in Team.Players)
            {
                player.HomeRegion = Region.GetRegion(
                    (Region.NumberOfColumns - 1) - player.HomeRegion.X, player.HomeRegion.Y);
            }
        }

    }
}