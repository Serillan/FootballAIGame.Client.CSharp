﻿using System;
using FootballAIGame.Client.AIs.Fsm.Entities;
using FootballAIGame.Client.CustomDataTypes;

namespace FootballAIGame.Client.AIs.Fsm.SteeringBehaviors
{
    class Flee : SteeringBehavior
    {
        public Vector From { get; set; }

        public double SafeDistance { get; set; }

        public Flee(Player player, int priority, double weight, Vector from, 
            double safeDistance) : base(player, priority, weight)
        {
            From = from;
            SafeDistance = safeDistance;
        }

        public override Vector CalculateAccelerationVector()
        {
            if (Vector.DistanceBetween(Player.Position, From) >= SafeDistance)
                return new Vector(0, 0);

            var desiredMovement = Vector.Difference(Player.Movement, From);

            if (Math.Abs(desiredMovement.LengthSquared) < 0.01)
                desiredMovement = new Vector(1, 0);

            desiredMovement.Resize(Player.MaxSpeed);

            var acceleration = Vector.Difference(desiredMovement, Player.Movement);
            acceleration.Truncate(Player.MaxAcceleration);

            return acceleration;
        }
    }
}