using System;
using System.Text;
using SmartTicketing.Core.Models;

namespace SmartTicketing.Core.Devices
{
    /// <summary>
    /// Hardware interface controller for the thermal ticket printer.
    /// Synchronized with UML Class Diagram 3.3.3.
    /// </summary>
    public class BarcodePrinter
    {
        public string PrinterModel { get; set; } = "Citizen CT-S310II High-Speed Thermal";
        public int RemainingPaperUnits { get; set; } = 500;
        public bool HasPaperJam { get; set; } = false;

        public bool CheckPaperStatus()
        {
            if (HasPaperJam) return false;
            return RemainingPaperUnits > 0;
        }

        public bool PrintTicket(Ticket ticket, out string receiptText)
        {
            var sb = new StringBuilder();

            if (!CheckPaperStatus())
            {
                receiptText = "[HARDWARE FAULT]: Printer jam or paper depleted!";
                return false;
            }

            RemainingPaperUnits--;

            sb.AppendLine("========================================");
            sb.AppendLine("           METROLINK TRANSIT            ");
            sb.AppendLine("       Single-Journey Barcode Ticket    ");
            sb.AppendLine("========================================");
            sb.AppendLine($"Ticket ID   : {ticket.TicketId}");
            sb.AppendLine($"Route       : Central -> {ticket.DestinationStationName}");
            sb.AppendLine($"Category    : {ticket.Category.ToString().ToUpperInvariant()}");
            sb.AppendLine($"Fare Price  : ${ticket.Price:F2} USD");
            sb.AppendLine($"Issue Time  : {ticket.IssueTime:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"Barcode     : ||| |||| | |||||| || | {ticket.Barcode}");
            sb.AppendLine($"Item        : {ticket.TicketNumber} of {ticket.TotalTicketsInOrder}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine(" Keep ticket until exiting destination. ");
            sb.AppendLine("========================================");

            receiptText = sb.ToString();
            return true;
        }

        public void SimulatePaperJam()
        {
            HasPaperJam = true;
        }

        public void ClearPaperJam()
        {
            HasPaperJam = false;
        }

        public void ReloadPaper(int units = 500)
        {
            RemainingPaperUnits = units;
            HasPaperJam = false;
        }
    }
}
