using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class Board : Node2D
{
    [Signal] public delegate void AnimationStartedEventHandler();
    [Signal] public delegate void AnimationFinishedEventHandler();

    private const double MOVEMENT_TIME = 0.3;
    private const double EXPLOSION_TIME = 0.25;

    [Export] private Game _game;

    [Export] private Vector2I _boardSize = new Vector2I(8, 8);
    [Export] private int _cellSize = 48;
    [Export] private int _padding = 16;
    private int _distanceBetweenElements;

    public Vector2 BoardSizePixels { get; private set; }

    [Export] private bool _drawGrid = true;

    [Export] private PackedScene[] _elementScenes;

    private AGameElement[,] _elements;
    private AGameElement _selectedElement;

    private bool _isItFilled = false;

    public override void _EnterTree()
    {
        _distanceBetweenElements = _cellSize + _padding;
        _elements = new AGameElement[_boardSize.X, _boardSize.Y];

        BoardSizePixels = _boardSize * _distanceBetweenElements;
    }

    public override void _Draw()
    {
        if (_drawGrid)
        {
            DrawGrid();
        }
    }

    private void DrawGrid()
    {
        var startPoint = -_distanceBetweenElements / 2;
        var endPoint = _distanceBetweenElements * (_boardSize.X - 1) + _distanceBetweenElements / 2;

        var color = new Color(0, 0, 0, 0.25f);

        for (int x = 0; x < _boardSize.X - 1; x++)
        {
            var from = new Vector2(-startPoint + x * _distanceBetweenElements, startPoint);
            var to = new Vector2(-startPoint + x * _distanceBetweenElements, endPoint);

            DrawLine(from, to, color, 2);
        }

        for (int y = 0; y < _boardSize.Y - 1; y++)
        {
            var from = new Vector2(startPoint, -startPoint + y * _distanceBetweenElements);
            var to = new Vector2(endPoint, -startPoint + y * _distanceBetweenElements);

            DrawLine(from, to, color, 2);
        }
    }

    /// <summary>
    /// Заполнить игровое поле.
    /// </summary>
    public async void Fill()
    {
        if (_isItFilled) 
        {
            return;
        }

        _isItFilled = true;

        EmitSignal(SignalName.AnimationStarted);

        while (IsEmptyCells())
        {
            var tweens = MoveElementsDown();
            tweens.AddRange(AddElementsToTop());

            await WaitForTweens(tweens);
        }

        EmitSignal(SignalName.AnimationFinished);

        _isItFilled = false;

        await RemoveMatches();
    }

    private async Task<bool> RemoveMatches(params AGameElement[] movedElements) 
    {
        var matches = SearchMatches();

        if (matches.Count == 0) 
        {
            return false;
        }


        foreach (var match in matches)
        {
            int score = 0;

            foreach (var point in match.Points)
            {
                var e = _elements[point.X, point.Y];
                if (e != null)
                {
                    score += await DestroyElement(e);
                }
            }

            _game.AddScore(score);
        }

        // Добавить бонусные элементы, если ход сделал игрок.
        if (movedElements.Length > 0)
        {
            AddBonuses(matches, movedElements);
        }

        Fill();

        return true;
    }

    private List<MatchLine> SearchMatches()
    {
        var matches = new List<MatchLine>();

        // Поиск горизонтальных линий.
        for (int y = 0; y < _boardSize.Y; y++)
        {
            int matchLength = 1;
            for (int x = 1; x <= _boardSize.X; x++)
            {
                if (x < _boardSize.X && _elements[x, y].Type == _elements[x - 1, y].Type)
                {
                    matchLength++;
                }
                else
                {
                    if (matchLength >= 3)
                    {
                        var points = new List<Vector2I>();

                        for (int i = x - matchLength; i < x; i++)
                        {
                            points.Add(new Vector2I(i, y));
                        }

                        var type = _elements[points[0].X, points[0].Y].Type;
                        var match = new MatchLine(type, Direction.Horizontal, points.ToArray());
                        matches.Add(match);
                    }
                    matchLength = 1;
                }
            }
        }

        // Поиск вертикальных линий.
        for (int x = 0; x < _boardSize.X; x++)
        {
            int matchLength = 1;
            for (int y = 1; y <= _boardSize.Y; y++)
            {
                if (y < _boardSize.Y && _elements[x, y].Type == _elements[x, y - 1].Type)
                {
                    matchLength++;
                }
                else
                {
                    if (matchLength >= 3)
                    {
                        var points = new List<Vector2I>();

                        for (int j = y - matchLength; j < y; j++)
                        {
                            points.Add(new Vector2I(x, j));
                        }

                        var type = _elements[points[0].X, points[0].Y].Type;
                        var match = new MatchLine(type, Direction.Vertical, points.ToArray());
                        matches.Add(match);
                    }
                    matchLength = 1;
                }
            }
        }

        return matches;
    }

    private void AddBonuses(List<MatchLine> matches, params AGameElement[] movedElements)
    {
        var bonusPositions = new List<Vector2I>();

        // Добавление бомб в точках пересечения.
        var intersections = FindIntersections(matches);

        foreach (var intersection in intersections)
        {
            foreach (var movedElement in movedElements)
            {
                // Если точка пересечения в этом элементе, который переместил игрок...
                if (movedElement.BoardPosition == intersection)
                {
                    if (!bonusPositions.Contains(movedElement.BoardPosition))
                    {
                        // ...создаём бомбу.
                        var bomb = Bomb.Create(movedElement);
                        AddElement(bomb, movedElement.BoardPosition.X, movedElement.BoardPosition.Y);

                        bonusPositions.Add(movedElement.BoardPosition);
                    }
                }
            }
        }

        // Добавление бонусов для рядов.
        foreach (var match in matches)
        {
            // Добавление бомб для рядов с комбинацией больше 4-х элементов.
            if (match.Length > 4)
            {
                foreach (var movedElement in movedElements)
                {
                    if (match.Points.Contains(movedElement.BoardPosition))
                    {
                        if (!bonusPositions.Contains(movedElement.BoardPosition))
                        {
                            var bomb = Bomb.Create(movedElement);
                            AddElement(bomb, movedElement.BoardPosition.X, movedElement.BoardPosition.Y);

                            bonusPositions.Add(movedElement.BoardPosition);
                        }
                    }
                }
            }
            // Добавление линий для рядов с комбинацией в 4 элемента.
            else if (match.Length == 4)
            {
                foreach (var movedElement in movedElements)
                {
                    if (match.Points.Contains(movedElement.BoardPosition))
                    {
                        if (!bonusPositions.Contains(movedElement.BoardPosition))
                        {
                            BonusGameElement line;

                            if (match.Direction == Direction.Horizontal)
                            {
                                line = LineHorizontal.Create(movedElement);
                            }
                            else
                            {
                                line = LineVertical.Create(movedElement);
                            }

                            AddElement(line, movedElement.BoardPosition.X, movedElement.BoardPosition.Y);

                            bonusPositions.Add(movedElement.BoardPosition);
                        }
                    }
                }
            }
        }
    }

    private List<Vector2I> FindIntersections(List<MatchLine> matches) 
    {
        var horizontalLines = matches.Where(m => m.Direction == Direction.Horizontal).ToList();
        var verticalLines = matches.Where(m => m.Direction == Direction.Vertical).ToList();

        var intersections = new List<Vector2I>();

        foreach (var hline in horizontalLines) 
        {
            foreach (var p in hline.Points) 
            {
                foreach (var vline in verticalLines) 
                {
                    if (vline.Points.Contains(p)) 
                    {
                        intersections.Add(p);
                        break;
                    }
                }
            }
        }

        return intersections;
    }

    private async Task<int> ActivateBombBonus(Bomb bomb)
    {
        bomb.Activate();

        // Ожидание 250 мс по условию ТЗ.
        await WaitForSignal(GetTree().CreateTimer(EXPLOSION_TIME), "timeout");

        int score = 0;

        var explosionArea = bomb.ExplosionArea;

        foreach (var point in explosionArea)
        {
            if (IsPointInsideBoard(point))
            {
                score += await DestroyElement(_elements[point.X, point.Y]);
            }
        }

        return score;
    }

    private async Task<int> ActivateLineBonus(BonusGameElement line)
    {
        line.Activate();

        var destroyer_scene = GD.Load<PackedScene>("res://scenes/Destroyer.tscn");

        var destroyer1 = destroyer_scene.Instantiate<Destroyer>();
        AddChild(destroyer1);

        var destroyer2 = destroyer_scene.Instantiate<Destroyer>();
        AddChild(destroyer2);

        destroyer1.DestroyElement += OnDestroyerDestroyElementAsync;
        destroyer2.DestroyElement += OnDestroyerDestroyElementAsync;

        Vector2 direction1, direction2;

        if (line is LineHorizontal)
        {
            direction1 = Vector2.Left;
            direction2 = Vector2.Right;
        }
        else
        {
            direction1 = Vector2.Up;
            direction2 = Vector2.Down;
        }

        destroyer1.Launch(line.Position, direction1, BoardSizePixels);
        destroyer2.Launch(line.Position, direction2, BoardSizePixels);

        await Task.WhenAll(
            WaitForSignal(destroyer1, Destroyer.SignalName.Done),
            WaitForSignal(destroyer2, Destroyer.SignalName.Done)
            );

        destroyer1.DestroyElement -= OnDestroyerDestroyElementAsync;
        destroyer2.DestroyElement -= OnDestroyerDestroyElementAsync;

        return 0;
    }

    private async void OnDestroyerDestroyElementAsync(AGameElement gameElement)
    {
        int score = await DestroyElement(gameElement);
        _game.AddScore(score);
    }

    private Vector2 GetCellPosition(int x, int y)
    {
        return new Vector2(x * _distanceBetweenElements, y * _distanceBetweenElements);
    }

    private bool IsEmptyCells()
    {
        foreach (var element in _elements) 
        {
            if (element == null) 
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPointInsideBoard(Vector2I p)
    {
        return p.X >= 0 && p.X < _boardSize.X && p.Y >= 0 && p.Y < _boardSize.Y;
    }

    private async void SwapElements(AGameElement e1, AGameElement e2, bool checkMatches = true)
    {
        var path1 = new Vector2[] 
        {
            e2.Position,
        };

        var t1 = e1.CreateTween();
        foreach (var p in path1)
        {
            t1.TweenProperty(e1, "position", p, MOVEMENT_TIME / path1.Length).SetTrans(Tween.TransitionType.Cubic);
        }

        var path2 = new Vector2[]
        {
            e1.Position,
        };

        var t2 = e2.CreateTween();
        foreach (var p in path2)
        {
            t2.TweenProperty(e2, "position", p, MOVEMENT_TIME / path2.Length).SetTrans(Tween.TransitionType.Cubic);
        }

        _elements[e1.BoardPosition.X, e1.BoardPosition.Y] = e2;
        _elements[e2.BoardPosition.X, e2.BoardPosition.Y] = e1;

        var bp1 = e1.BoardPosition;
        e1.BoardPosition = e2.BoardPosition;
        e2.BoardPosition = bp1;

        EmitSignal(SignalName.AnimationStarted);

        await WaitForTweens([t1, t2]);

        EmitSignal(SignalName.AnimationFinished);

        if (checkMatches)
        {
            var isMatches = await RemoveMatches(e1, e2);
            if (!isMatches) 
            {
                SwapElements(e1, e2, false);
            }
        }
    }

    private Tween MoveElementDown(AGameElement e)
    {
        var newPosition = new Vector2I(e.BoardPosition.X, e.BoardPosition.Y + 1);

        var tween = e.CreateTween();
        var p = tween.TweenProperty(e, "position", GetCellPosition(newPosition.X, newPosition.Y), MOVEMENT_TIME);
        p.SetTrans(Tween.TransitionType.Cubic);

        if (e.BoardPosition.Y > -1)
        {
            _elements[e.BoardPosition.X, e.BoardPosition.Y] = null;
        }

        _elements[newPosition.X, newPosition.Y] = e;
        e.BoardPosition = new Vector2I(newPosition.X, newPosition.Y);

        return tween;
    }

    /// <summary>
    /// Сдвинуть игровые элементы вниз.
    /// </summary>
    /// <returns>Список анимаций.</returns>
    private List<Tween> MoveElementsDown()
    {
        var tweens = new List<Tween>();

        // Проходим массив элементов снизу вверх.
        for (int y = _boardSize.Y - 2; y > -1; y--)
        {
            for (int x = 0; x < _boardSize.X; x++)
            {
                var element = _elements[x, y];

                // Пропускаем шаг, если элемента нет.
                if (element == null) 
                {
                    continue;
                }

                var downElement = _elements[x, y + 1];

                // Пропускаем шаг, если снизу есть элемент.
                if (downElement != null)
                {
                    continue;
                }

                var tween = MoveElementDown(element);
                tweens.Add(tween);
            }
        }

        return tweens;
    }

    /// <summary>
    /// Добавить новые элементы сверху, если это возможно.
    /// </summary>
    /// <returns></returns>
    private List<Tween> AddElementsToTop()
    {
        var tweens = new List<Tween>();

        for (int x = 0; x < _boardSize.X; x++)
        {
            if (_elements[x, 0] == null)
            {
                var element = SimpleGameElement.CreateRandom();

                AddElement(element, x, -1);

                var tween = MoveElementDown(element);
                tweens.Add(tween);

            }
        }

        return tweens;
    }

    private void AddElement(AGameElement element, int x, int y)
    {
        AddChild(element);

        element.Position = GetCellPosition(x, y);
        element.BoardPosition = new Vector2I(x, y);
        element.Click += OnElementSelected;

        if (y > -1)
        {
            _elements[x, y] = element;
        }
    }

    private async Task<int> DestroyElement(AGameElement element)
    {
        if (element == null) 
        {
            return 0;
        }

        int score = 0;

        if (element is Bomb bomb && !bomb.IsActivated)
        {
            score = await ActivateBombBonus(bomb);
        }
        else if ((element is LineHorizontal || element is LineVertical) && !(element as BonusGameElement).IsActivated)
        {
            score = await ActivateLineBonus(element as BonusGameElement);
        }
        else
        {
            score = element.Score;

            _elements[element.BoardPosition.X, element.BoardPosition.Y] = null;
            element.Click -= OnElementSelected;
            element.Destroy();
        }

        return score;
    }

    private void OnElementSelected(AGameElement e)
    {
        if (_game.State != GameState.Input) 
        {
            return;
        }

        if (_selectedElement == e)
        {
            return;
        }
        else if (_selectedElement == null)
        {
            _selectedElement = e;
            _selectedElement.Select();
        }
        else
        {
            if (IsElementsAreNeighbors(_selectedElement, e))
            {
                SwapElements(_selectedElement, e);
            }

            _selectedElement.Deselect();
            _selectedElement = null;
        }
    }

    private bool IsElementsAreNeighbors(AGameElement e1, AGameElement e2) 
    {
        var deltaX = Mathf.Abs(e1.BoardPosition.X - e2.BoardPosition.X);
        var deltaY = Mathf.Abs(e1.BoardPosition.Y - e2.BoardPosition.Y);

        return (deltaX + deltaY) < 2;
    }

    private async Task WaitForTweens(List<Tween> tweens)
    {
        var tasks = tweens.Select(t => WaitForSignal(t, Tween.SignalName.Finished)).ToArray();
        await Task.WhenAll(tasks);
    }

    private async Task WaitForSignal(GodotObject node, StringName signal)
    {
        await ToSignal(node, signal);
    }
}
