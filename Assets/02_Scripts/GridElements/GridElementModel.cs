using System;
using TK.Blast;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract record GridElementModel(GridElementType ElementType, bool CanFall = true)
{
    public GridElementType ElementType { get; } = ElementType;
    public bool CanFall { get; private set; } = CanFall;
    public Vector2Int Coordinate { get; private set; } = new(-1, -1);
    public abstract string LoadPath { get; }

    public void SetCoordinate(Vector2Int newCoordinate)
    {
        Coordinate = newCoordinate;
    }

    public virtual bool IsSameWith(GridElementModel model)
    {
        return model.ElementType == ElementType;
    }

    public abstract bool CanMatchWith(GridElementModel model);
}

public record CubeModel(GridElementColor Color)
    : GridElementModel(GridElementType.Cube)
{
    public static readonly CubeModel Red = new(GridElementColor.Red);
    public static readonly CubeModel Green = new(GridElementColor.Green);
    public static readonly CubeModel Blue = new(GridElementColor.Blue);
    public static readonly CubeModel Yellow = new(GridElementColor.Yellow);
    public override string LoadPath => $"{Color}Cube";
    public GridElementColor Color { get; } = Color;

    public static CubeModel GetRandom()
    {
        var color = (GridElementColor)Random.Range(0, 4);
        return color switch
        {
            GridElementColor.Red => Red,
            GridElementColor.Green => Green,
            GridElementColor.Blue => Blue,
            GridElementColor.Yellow => Yellow,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };
    }

    public override bool IsSameWith(GridElementModel model)
    {
        return base.IsSameWith(model) && ((CubeModel)model).Color == Color;
    }

    public override bool CanMatchWith(GridElementModel model)
    {
        switch (model.ElementType)
        {
            case GridElementType.Cube when ((CubeModel)model).Color == Color:
            case GridElementType.Obstacle when ((ObstacleModel)model).Kind is ObstacleKind.Box or ObstacleKind.Vase:
                return true;
            default:
                return false;
        }
    }
}

public record RocketModel(RocketDirection Direction)
    : GridElementModel(GridElementType.Rocket)
{
    public static readonly RocketModel Vertical = new(RocketDirection.Vertical);
    public static readonly RocketModel Horizontal = new(RocketDirection.Horizontal);
    public override string LoadPath => $"{Direction}Rocket";
    public RocketDirection Direction { get; } = Direction;

    public static RocketModel GetRandom()
    {
        var direction = (RocketDirection)Random.Range(0, 2);
        return direction switch
        {
            RocketDirection.Vertical => Vertical,
            RocketDirection.Horizontal => Horizontal,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public override bool IsSameWith(GridElementModel model)
    {
        return base.IsSameWith(model) && ((RocketModel)model).Direction == Direction;
    }

    public override bool CanMatchWith(GridElementModel model)
    {
        return model.ElementType == GridElementType.Rocket;
    }
}

public record ObstacleModel(ObstacleKind Kind, int Hp = 1, bool CanFall = true)
    : GridElementModel(GridElementType.Obstacle, CanFall)
{
    public static readonly ObstacleModel Box = new(ObstacleKind.Box);
    public static readonly ObstacleModel Stone = new(ObstacleKind.Stone);
    public static readonly ObstacleModel Vase = new(ObstacleKind.Vase);
    public override string LoadPath => Kind.ToString();
    public ObstacleKind Kind { get; } = Kind;
    public int Hp { get; private set; } = Hp;

    public static ObstacleModel ByType(ObstacleKind elementType)
    {
        return elementType switch
        {
            ObstacleKind.Box => Box,
            ObstacleKind.Stone => Stone,
            ObstacleKind.Vase => Vase,
            _ => throw new ArgumentOutOfRangeException(nameof(elementType), elementType, null)
        };
    }

    public override bool CanMatchWith(GridElementModel model)
    {
        return false;
    }

    public void DecreaseHp()
    {
        Hp--;
    }
}