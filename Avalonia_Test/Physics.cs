using System;
using System.Collections.Generic;
namespace Avalonia_Test;
public static class Physics
{
    private const double Restitution = 0.9;
    private const double StopThreshold = 0.02;
    private const double Epsilon = 0.3;
    public static void UpdateBalls(List<Ball> balls, Table table, double dt)
    {
        foreach (var b in balls)
        {
            b.X += b.Vx * dt;
            b.Y += b.Vy * dt;
            b.Vx *= table.Friction;
            b.Vy *= table.Friction;
            if (b.X - b.Radius < table.Left) { b.X = table.Left + b.Radius; b.Vx = -b.Vx * table.CushionRestitution; }
            if (b.X + b.Radius > table.Left + table.Width) { b.X = table.Left + table.Width - b.Radius; b.Vx = -b.Vx * table.CushionRestitution; }
            if (b.Y - b.Radius < table.Top) { b.Y = table.Top + b.Radius; b.Vy = -b.Vy * table.CushionRestitution; }
            if (b.Y + b.Radius > table.Top + table.Height) { b.Y = table.Top + table.Height - b.Radius; b.Vy = -b.Vy * table.CushionRestitution; }
            if (Math.Abs(b.Vx) < StopThreshold) b.Vx = 0;
            if (Math.Abs(b.Vy) < StopThreshold) b.Vy = 0;
        }
        for (int iter = 0; iter < 3; iter++)
        {
            for (int i = 0; i < balls.Count; i++)
                for (int j = i + 1; j < balls.Count; j++)
                    ResolveCollision(balls[i], balls[j]);
        }
    }
    public static void SeparateBalls(List<Ball> balls)
    {
        for (int iter = 0; iter < 5; iter++)
        {
            for (int i = 0; i < balls.Count; i++)
                for (int j = i + 1; j < balls.Count; j++)
                    SeparatePair(balls[i], balls[j]);
        }
    }
    private static void SeparatePair(Ball a, Ball b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);
        double minDist = a.Radius + b.Radius + Epsilon;
        if (dist >= minDist || dist < 0.0001) return;
        double nx = dx / dist, ny = dy / dist;
        double overlap = (minDist - dist) / 2;
        a.X -= nx * overlap;
        a.Y -= ny * overlap;
        b.X += nx * overlap;
        b.Y += ny * overlap;
    }
    private static void ResolveCollision(Ball a, Ball b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);
        double minDist = a.Radius + b.Radius;
        if (dist >= minDist || dist < 0.0001) return;
        double nx = dx / dist, ny = dy / dist;
        double overlap = (minDist - dist) / 2;
        a.X -= nx * overlap;
        a.Y -= ny * overlap;
        b.X += nx * overlap;
        b.Y += ny * overlap;
        double dvx = a.Vx - b.Vx;
        double dvy = a.Vy - b.Vy;
        double dvn = dvx * nx + dvy * ny;
        if (dvn > 0) return;
        double impulse = -(1 + Restitution) * dvn / 2;
        a.Vx += impulse * nx;
        a.Vy += impulse * ny;
        b.Vx -= impulse * nx;
        b.Vy -= impulse * ny;
    }
}
