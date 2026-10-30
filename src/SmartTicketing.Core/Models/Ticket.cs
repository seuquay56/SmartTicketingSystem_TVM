using System;

namespace SmartTicketing.Core.Models
{
    public enum TicketStatus
    {
        Valid,
        Used,
        Refunded,
        Cancelled
    }

    public enum FareCategory
    {
        Adult,  // Standard 100% fare ($3.00)
        Child,  // Ages 5-15, 50% discount ($1.50)
        Senior  // Ages 65+, 50% discount ($1.50)
    }

    /// <summary>
    /// Represents a generated physical/electronic ticket with a unique barcode.
    /// Synchronized with UML Class Diagram 3.3.3.
    /// </summary>
    public class Ticket
    {
        public string TicketId { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public DateTime IssueTime { get; set; } = DateTime.UtcNow;
        public decimal Price { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Valid;
        public FareCategory Category { get; set; } = FareCategory.Adult;

        public int OriginStationId { get; set; }
        public string OriginStationName { get; set; } = "Central";

        public int DestinationStationId { get; set; }
        public string DestinationStationName { get; set; } = string.Empty;

        public string TransactionId { get; set; } = string.Empty;
        public int TicketNumber { get; set; } = 1;
        public int TotalTicketsInOrder { get; set; } = 1;

        public Ticket()
        {
            TicketId = "TKT-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            GenerateBarcode();
        }

        public void GenerateBarcode()
        {
            // Standard transport barcode format: TKT-{YYYY}-{UNIQUE_RANDOM_NUM}
            var rnd = new Random().Next(1000, 9999);
            Barcode = $"TKT-{DateTime.UtcNow:yyyy}-{rnd}";
        }

        public void MarkAsUsed()
        {
            Status = TicketStatus.Used;
        }

        public void Refund()
        {
            Status = TicketStatus.Refunded;
        }
    }
}
