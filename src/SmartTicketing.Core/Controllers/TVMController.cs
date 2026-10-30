using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTicketing.Core.Devices;
using SmartTicketing.Core.Models;

namespace SmartTicketing.Core.Controllers
{
    public enum TVMState
    {
        Idle_Welcome,           // S01: Welcome screen
        SelectingDestination,   // S02: Choose destination
        ConfiguringTickets,     // S03: Choose tickets & fare
        SelectingPayment,       // S04: Choose payment method
        ProcessingCardPayment,  // S05: Tap/insert card
        ProcessingQRPayment,    // S06: Scan QR & countdown
        PaymentDeclined,        // S07: Card declined exception
        QRTimedOut,             // S08: QR timed out exception
        PrintingTickets,        // S09: Payment confirmed, printing
        TicketsIssued,          // S10: Tickets ready for collection
        OutOfService            // S11: Printer fault / Out of service
    }

    /// <summary>
    /// Central Controller orchestrating the Ticket Vendor Machine hardware, business logic, and UI states.
    /// Synchronized with UML Class Diagram 3.3.3 and State Chart Diagram 3.3.2.
    /// </summary>
    public class TVMController
    {
        public string MachineId { get; set; } = "TVM-01";
        public string Location { get; set; } = "Central Station - Main Concourse";
        public bool IsInService { get; set; } = true;

        public TVMState CurrentState { get; private set; } = TVMState.Idle_Welcome;

        // Current Session Data
        public Station OriginStation { get; set; }
        public Station? SelectedDestination { get; set; }
        public FareCategory CurrentCategory { get; set; } = FareCategory.Adult;
        public int TicketQuantity { get; set; } = 1;
        public decimal TotalAmount { get; private set; } = 0.00m;

        public PaymentTransaction? CurrentTransaction { get; set; }
        public List<Ticket> IssuedTickets { get; } = new();

        // Hardware Components (Composition)
        public BarcodePrinter Printer { get; } = new();
        public CardReader Reader { get; } = new();

        // Events for UI binding
        public event Action<TVMState>? StateChanged;
        public event Action<int>? QRCountdownTick;
        public event Action<string>? LogMessageGenerated;

        public TVMController()
        {
            OriginStation = new Station
            {
                StationId = 1,
                StationName = "Central",
                LineName = "Riverside Line [01]",
                BaseFare = 0.00m,
                StopOrder = 0
            };
        }

        public void SetState(TVMState newState)
        {
            CurrentState = newState;
            LogMessageGenerated?.Invoke($"[State Changed]: {newState}");
            StateChanged?.Invoke(newState);
        }

        public void InitSession()
        {
            if (!IsInService)
            {
                SetState(TVMState.OutOfService);
                return;
            }

            SelectedDestination = null;
            CurrentCategory = FareCategory.Adult;
            TicketQuantity = 1;
            TotalAmount = 0.00m;
            CurrentTransaction = null;
            IssuedTickets.Clear();
            Reader.EjectCard();

            SetState(TVMState.SelectingDestination);
        }

        public void SelectDestination(Station destination)
        {
            SelectedDestination = destination;
            RecalculateTotal();
            SetState(TVMState.ConfiguringTickets);
        }

        public void ConfigureTickets(FareCategory category, int quantity)
        {
            CurrentCategory = category;
            TicketQuantity = Math.Clamp(quantity, 1, 6);
            RecalculateTotal();
            SetState(TVMState.SelectingPayment);
        }

        public void RecalculateTotal()
        {
            if (SelectedDestination == null)
            {
                TotalAmount = 0.00m;
                return;
            }

            decimal unitPrice = SelectedDestination.BaseFare;
            if (CurrentCategory == FareCategory.Child || CurrentCategory == FareCategory.Senior)
            {
                unitPrice /= 2.0m; // 50% discount ($1.50)
            }

            TotalAmount = unitPrice * TicketQuantity;
        }

        public decimal GetUnitPrice()
        {
            if (SelectedDestination == null) return 0.00m;
            decimal unitPrice = SelectedDestination.BaseFare;
            if (CurrentCategory == FareCategory.Child || CurrentCategory == FareCategory.Senior)
            {
                unitPrice /= 2.0m;
            }
            return unitPrice;
        }

        public async Task<bool> ProcessCardPaymentAsync(bool simulateDecline = false)
        {
            SetState(TVMState.ProcessingCardPayment);
            Reader.InsertOrTapCard();

            var cardTxn = new CreditCardTransaction(TotalAmount)
            {
                MachineId = MachineId,
                SimulateDecline = simulateDecline
            };
            CurrentTransaction = cardTxn;

            bool success = await cardTxn.ProcessAsync();
            if (success)
            {
                return await IssueTicketAsync();
            }
            else
            {
                SetState(TVMState.PaymentDeclined);
                return false;
            }
        }

        public async Task<bool> ProcessQRPaymentAsync(string provider = "Banking / E-wallet QR", bool simulateTimeout = false)
        {
            SetState(TVMState.ProcessingQRPayment);

            var qrTxn = new DigitalWalletTransaction(TotalAmount, provider)
            {
                MachineId = MachineId,
                SimulateTimeout = simulateTimeout
            };
            CurrentTransaction = qrTxn;

            if (simulateTimeout)
            {
                for (int sec = 3; sec >= 0; sec--)
                {
                    QRCountdownTick?.Invoke(sec);
                    await Task.Delay(400);
                }
                qrTxn.Status = TransactionStatus.Timeout;
                SetState(TVMState.QRTimedOut);
                return false;
            }

            // Normal successful payment: simulate quick countdown then success
            QRCountdownTick?.Invoke(59);
            await Task.Delay(400);
            QRCountdownTick?.Invoke(58);
            await Task.Delay(500);

            bool success = await qrTxn.ProcessAsync();
            if (success)
            {
                return await IssueTicketAsync();
            }
            else
            {
                SetState(TVMState.QRTimedOut);
                return false;
            }
        }

        public async Task<bool> IssueTicketAsync()
        {
            SetState(TVMState.PrintingTickets);
            await Task.Delay(1000); // Printing animation time

            if (!Printer.CheckPaperStatus())
            {
                LogMessageGenerated?.Invoke("[FAULT]: Printer hardware failure mid-transaction. Initiating automatic refund...");
                if (CurrentTransaction != null)
                {
                    await CurrentTransaction.RefundAsync();
                }
                IsInService = false;
                SetState(TVMState.OutOfService);
                return false;
            }

            IssuedTickets.Clear();
            decimal unitPrice = GetUnitPrice();

            for (int i = 1; i <= TicketQuantity; i++)
            {
                var ticket = new Ticket
                {
                    OriginStationId = OriginStation.StationId,
                    OriginStationName = OriginStation.StationName,
                    DestinationStationId = SelectedDestination?.StationId ?? 0,
                    DestinationStationName = SelectedDestination?.StationName ?? "Riverside",
                    Category = CurrentCategory,
                    Price = unitPrice,
                    TransactionId = CurrentTransaction?.TransactionId ?? "N/A",
                    TicketNumber = i,
                    TotalTicketsInOrder = TicketQuantity
                };

                Printer.PrintTicket(ticket, out _);
                IssuedTickets.Add(ticket);
            }

            SetState(TVMState.TicketsIssued);
            return true;
        }

        public void CancelSession()
        {
            LogMessageGenerated?.Invoke("[User Action]: Transaction cancelled by user.");
            CurrentTransaction = null;
            Reader.EjectCard();
            SetState(TVMState.Idle_Welcome);
        }

        public void ResetToHome()
        {
            CurrentTransaction = null;
            SelectedDestination = null;
            IssuedTickets.Clear();
            Reader.EjectCard();
            SetState(TVMState.Idle_Welcome);
        }
    }
}
