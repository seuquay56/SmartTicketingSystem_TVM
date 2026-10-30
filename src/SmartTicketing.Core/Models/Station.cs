namespace SmartTicketing.Core.Models
{
    /// <summary>
    /// Represents a transit station along a train/metro line.
    /// Synchronized with UML Class Diagram 3.3.3 and Figma Riverside Line [01].
    /// </summary>
    public class Station
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string LineName { get; set; } = "Riverside Line [01]";
        public decimal BaseFare { get; set; } = 3.00m;
        public int StopOrder { get; set; }
        public int EstimatedMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsPopular { get; set; }

        public override string ToString()
        {
            return $"{StationName} (${BaseFare:F2})";
        }
    }
}
