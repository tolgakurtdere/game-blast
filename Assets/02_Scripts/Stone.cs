namespace TK.Blast
{
    public class Stone : ObstacleBase
    {
        protected override GridElementModel Initialize()
        {
            return new ObstacleModel(ObstacleKind.Stone, 1, false);
        }
    }
}