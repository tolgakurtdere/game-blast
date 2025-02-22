namespace TK.Blast
{
    public class Box : ObstacleBase
    {
        protected override GridElementModel Initialize()
        {
            return new ObstacleModel(ObstacleKind.Box, 1, false);
        }
    }
}