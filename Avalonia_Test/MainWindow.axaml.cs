using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Avalonia_Test;
public partial class MainWindow : Window
{
    private Table _table = new();
    private List<Ball> _balls = new();
    private List<Ellipse> _ellipses = new();
    private Line _aimLine;
    private TextBlock _scoreText;
    private DispatcherTimer _timer;
    private Canvas _canvas;
    private Ball _selectedBall;
    private bool _isAiming;
    private double _mouseX, _mouseY;
    private int _score = 0;
    private List<Ellipse> _pocketVisuals = new();
    public MainWindow()
    {
        InitializeComponent();
        _canvas = this.FindControl<Canvas>("GameCanvas");
        _scoreText = this.FindControl<TextBlock>("ScoreText");
        _canvas.PointerPressed += OnPointerPressed;
        _canvas.PointerMoved += OnPointerMoved;
        _canvas.PointerReleased += OnPointerReleased;
        _aimLine = new Line { Stroke = Brushes.White, StrokeThickness = 2, IsVisible = false };
        _canvas.Children.Add(_aimLine);

        this.Loaded += (s, e) => { InitGame(); };
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnTick;
        _timer.Start();
    }
    private void InitGame()
    {
        double margin = 60;
        _table.Left = margin;
        _table.Top = margin;
        _table.Width = _canvas.Bounds.Width - 2 * margin;
        _table.Height = _canvas.Bounds.Height - 2 * margin;
        _table.Friction = 0.999;
        _table.CushionRestitution = 0.75;

        _canvas.Children.Clear();
        _canvas.Children.Add(_aimLine);
        DrawPockets();
        _balls.Clear();
        _ellipses.Clear();
        _balls.Add(new Ball(200, 300, Brushes.White));
        var colors = new[]
        {
            Brushes.Yellow, Brushes.Blue, Brushes.Red, Brushes.Purple,
            Brushes.Orange, Brushes.Green, Brushes.Brown, Brushes.Black,
            Brushes.Yellow, Brushes.Blue, Brushes.Red, Brushes.Purple,
            Brushes.Orange, Brushes.Green, Brushes.Brown
        };
        double startX = 550, startY = 300, spacing = 24;
        int row = 0, col = 0;
        foreach (var c in colors)
        {
            double x = startX + col * spacing;
            double y = startY - row * spacing / 2 + col * spacing / 2;
            _balls.Add(new Ball(x, y, c));
            col++;
            if (col > row) { row++; col = 0; }
        }
        foreach (var b in _balls)
        {
            var el = new Ellipse { Width = b.Radius * 2, Height = b.Radius * 2, Fill = b.Color };
            _canvas.Children.Add(el);
            _ellipses.Add(el);
        }
        UpdateEllipses();
        _score = 0;
        _scoreText.Text = $"Забито: {_score}";
    }
    private void DrawPockets()
    {
        double pocketRadius = 22;
        var positions = new (double x, double y)[]
        {
            (_table.Left, _table.Top),
            (_table.Left + _table.Width, _table.Top),
            (_table.Left, _table.Top + _table.Height),
            (_table.Left + _table.Width, _table.Top + _table.Height)
        };
        foreach (var p in positions)
        {
            var pocket = new Ellipse
            {
                Width = pocketRadius * 2,
                Height = pocketRadius * 2,
                Fill = Brushes.Black,
                Stroke = Brushes.DarkGray,
                StrokeThickness = 2
            };
            Canvas.SetLeft(pocket, p.x - pocketRadius);
            Canvas.SetTop(pocket, p.y - pocketRadius);
            _canvas.Children.Add(pocket);
            _pocketVisuals.Add(pocket);
        }
    }
    private void UpdateEllipses()
    {
        for (int i = 0; i < _balls.Count; i++)
        {
            Canvas.SetLeft(_ellipses[i], _balls[i].X - _balls[i].Radius);
            Canvas.SetTop(_ellipses[i], _balls[i].Y - _balls[i].Radius);
        }
    }
    private void OnTick(object? sender, EventArgs e)
    {
        Physics.SeparateBalls(_balls);
        if (!_isAiming)
        {
            Physics.UpdateBalls(_balls, _table, 0.02);
        }
        CheckPockets();
        UpdateEllipses();
    }
    private void CheckPockets()
    {
        var pockets = new (double x, double y)[]
        {
            (_table.Left, _table.Top),
            (_table.Left + _table.Width, _table.Top),
            (_table.Left, _table.Top + _table.Height),
            (_table.Left + _table.Width, _table.Top + _table.Height)
        };
        double pocketRadius = 22;
        for (int i = _balls.Count - 1; i >= 0; i--)
        {
            var b = _balls[i];
            if (b.Color == Brushes.White) continue;
            foreach (var p in pockets)
            {
                double dx = b.X - p.x;
                double dy = b.Y - p.y;
                if (dx * dx + dy * dy < pocketRadius * pocketRadius)
                {
                    _balls.RemoveAt(i);
                    var el = _ellipses[i];
                    _canvas.Children.Remove(el);
                    _ellipses.RemoveAt(i);
                    _score++;
                    _scoreText.Text = $"Забито: {_score}";
                    break;
                }
            }
        }
    }
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pos = e.GetPosition(_canvas);
        var hit = _balls.FirstOrDefault(b =>
            (b.X - pos.X) * (b.X - pos.X) + (b.Y - pos.Y) * (b.Y - pos.Y) < b.Radius * b.Radius);
        if (hit != null)
        {
            _selectedBall = hit;
            _isAiming = true;
            _mouseX = pos.X;
            _mouseY = pos.Y;
            e.Pointer.Capture(_canvas);
        }
    }
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var pos = e.GetPosition(_canvas);
        _mouseX = pos.X;
        _mouseY = pos.Y;
        if (_isAiming && _selectedBall != null)
        {
            double dx = _mouseX - _selectedBall.X;
            double dy = _mouseY - _selectedBall.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist > 1)
            {
                double len = Math.Min(dist, 300);
                _aimLine.StartPoint = new Avalonia.Point(_selectedBall.X, _selectedBall.Y);
                _aimLine.EndPoint = new Avalonia.Point(_selectedBall.X + dx / dist * len,
                                                        _selectedBall.Y + dy / dist * len);
                _aimLine.IsVisible = true;
                double strength = Math.Min(dist / 300, 1);
                byte r = (byte)(255 * strength);
                byte g = (byte)(255 * (1 - strength));
                _aimLine.Stroke = new SolidColorBrush(new Color(255, r, g, 0));
            }
            else
                _aimLine.IsVisible = false;
        }
    }
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isAiming && _selectedBall != null)
        {
            double dx = _mouseX - _selectedBall.X;
            double dy = _mouseY - _selectedBall.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist > 1)
            {
                double power = Math.Min(dist * 8.0, 800);
                _selectedBall.Vx = -dx / dist * power;
                _selectedBall.Vy = -dy / dist * power;
            }
            _isAiming = false;
            _selectedBall = null;
            _aimLine.IsVisible = false;
            e.Pointer.Capture(null);
        }
    }
}
