namespace EmberBlade.Combat
{
    public static class BarricadeResult
    {
        public static int WallsPlaced { get; private set; }
        public static int TrapsPlaced { get; private set; }

        public static void Record(int walls, int traps)
        {
            WallsPlaced = walls;
            TrapsPlaced = traps;
        }
    }
}
