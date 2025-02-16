namespace TK.Blast
{
    public enum GridElementType
    {
        RedCube,
        GreenCube,
        BlueCube,
        YellowCube,
        VerticalRocket,
        HorizontalRocket,
        Box,
        Stone,
        Vase
    }

    public static class GridElementTypeExtensions
    {
        public static bool IsCube(this GridElementType elementType)
        {
            return elementType is
                GridElementType.RedCube or
                GridElementType.GreenCube or
                GridElementType.BlueCube or
                GridElementType.YellowCube;
        }

        public static bool IsRocket(this GridElementType elementType)
        {
            return elementType is
                GridElementType.VerticalRocket or
                GridElementType.HorizontalRocket;
        }

        public static bool IsObstacle(this GridElementType elementType)
        {
            return elementType is
                GridElementType.Box or
                GridElementType.Stone or
                GridElementType.Vase;
        }
    }
}