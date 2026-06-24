using System;
using System.Collections.Generic;
namespace Avalonia_Test;
public static class Physics
{
    private const double Restitution = 1.0;
    private const double StopThreshold = 0.02;
    private const int SubSteps = 8;
    private const int Iterations = 5;

    public static void UpdateBalls(List<Ball> balls, Table table, double dt)
    {
        double subDt = dt / SubSteps;

        for (int step = 0; step < SubSteps; step++)
        {
            foreach (var b in balls)
            {
                b.X += b.Vx * subDt;
                b.Y += b.Vy * subDt;
                b.Vx *= table.Friction;
                b.Vy *= table.Friction;

                if (Math.Abs(b.Vx) < StopThreshold) b.Vx = 0;
                if (Math.Abs(b.Vy) < StopThreshold) b.Vy = 0;
            }
            for (int iter = 0; iter < Iterations; iter++)
            {
                for (int i = 0; i < balls.Count; i++)
                    for (int j = i + 1; j < balls.Count; j++)
                        ResolveCollision(balls[i], balls[j]);
            }
        }
    }
    public static void SeparateBalls(List<Ball> balls) { }
    private static void ResolveCollision(Ball a, Ball b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);
        double minDist = a.Radius + b.Radius;
        if (dist >= minDist || dist < 0.0001) return;
        double nx = dx / dist;
        double ny = dy / dist;
        double overlap = minDist - dist;
        a.X -= nx * overlap * 0.5;
        a.Y -= ny * overlap * 0.5;
        b.X += nx * overlap * 0.5;
        b.Y += ny * overlap * 0.5;
        double dvx = a.Vx - b.Vx;
        double dvy = a.Vy - b.Vy;
        double dvn = dvx * nx + dvy * ny;
        if (dvn > 0) return;
        double impulse = -dvn;
        a.Vx += impulse * nx;
        a.Vy += impulse * ny;
        b.Vx -= impulse * nx;
        b.Vy -= impulse * ny;
    }
}
