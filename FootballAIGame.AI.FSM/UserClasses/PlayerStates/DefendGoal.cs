﻿using FootballAIGame.AI.FSM.CustomDataTypes;
using FootballAIGame.AI.FSM.UserClasses.Entities;
using FootballAIGame.AI.FSM.UserClasses.SteeringBehaviors;
using FootballAIGame.AI.FSM.UserClasses.TeamStates;

namespace FootballAIGame.AI.FSM.UserClasses.PlayerStates
{
    class DefendGoal : PlayerState
    {
        private Interpose Interpose { get; set; }

        public DefendGoal(Player player, Ai ai) : base(player, ai)
        {
        }

        public override void Enter()
        {
            var goalCenter = new Vector(0, GameClient.FieldHeight / 2);
            if (!Ai.MyTeam.IsOnLeft)
                goalCenter.X = GameClient.FieldWidth;

            Interpose = new Interpose(Player, 1, 1.0, Ai.Ball, goalCenter)
            {
                PreferredDistanceFromSecond = Parameters.DefendGoalDistance
            };

            Player.SteeringBehaviorsManager.AddBehavior(Interpose);
        }

        public override void Run()
        {
            if (Ai.MyTeam.StateMachine.CurrentState is Defending &&
                Vector.DistanceBetween(Ai.Ball.Position, Ai.MyTeam.GoalCenter) < Parameters.GoalKeeperInterceptRange)
            {
                Player.StateMachine.ChangeState(new InterceptBall(Player, Ai));
            }
        }

        public override void Exit()
        {
            Player.SteeringBehaviorsManager.RemoveBehavior(Interpose);
        }
    }
}
