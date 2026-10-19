namespace EmberBlade.Missions
{
    public static class MissionSequence
    {
        public static string Next(string missionId)
        {
            return missionId switch
            {
                "1.1" => "1.2",
                "1.2" => "1.3",
                _ => string.Empty
            };
        }
    }
}
