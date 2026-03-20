namespace ITrade.Common.Helpers
{
    public class MatchingSettings
    {
        public const string SectionName = "Matching";

        public int MaxRecommendations { get; set; } = 10;
        public int DefaultTagMatchMaxPercentage { get; set; } = 60;
        public int DefaultExperienceMaxPercentage { get; set; } = 20;
        public int DefaultReviewsMaxPercentage { get; set; } = 20;
        public int ExperiencePointsPerCompletedProject { get; set; } = 2;
        public int AvailabilityPenaltyPerActiveProject { get; set; } = 3;
        public int RecentAccountDaysForMedianReviewBonus { get; set; } = 30;
    }
}
