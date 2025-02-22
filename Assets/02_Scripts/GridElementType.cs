namespace TK.Blast
{
    public enum GridElementType
    {
        Cube,
        Rocket,
        Obstacle
    }

    public enum GridElementColor
    {
        Red,
        Green,
        Blue,
        Yellow
    }

    public enum RocketDirection
    {
        Vertical,
        Horizontal
    }

    public enum ObstacleKind
    {
        Box,
        Stone,
        Vase
    }

    public static class GridElementTypeExtensions
    {
        public static bool IsCube(this GridElementType elementType)
        {
            return elementType == GridElementType.Cube;
        }

        public static bool IsRocket(this GridElementType elementType)
        {
            return elementType == GridElementType.Rocket;
        }

        public static bool IsObstacle(this GridElementType elementType)
        {
            return elementType == GridElementType.Obstacle;
        }
    }
}