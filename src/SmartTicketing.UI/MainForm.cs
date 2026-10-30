using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartTicketing.Core.Controllers;
using SmartTicketing.Core.Models;
using SmartTicketing.Data;
using SmartTicketing.Data.Repositories;
using SmartTicketing.UI.Utils;

namespace SmartTicketing.UI
{
    public partial class MainForm : Form
    {
        private readonly TVMController _controller;
        private readonly IStationRepository _stationRepo;
        private readonly ITransactionRepository _txnRepo;

        // UI Panels for Each State
        private Panel _pnlHeader = null!;
        private Panel _pnlStepBar = null!;
        private Panel _pnlContent = null!;
        private Panel _pnlBottomSimBar = null!;

        // Dynamic State Views
        // Built on demand per state

        // Data caches
        private List<Station> _stations = new();
        private System.Windows.Forms.Timer _qrTimer = null!;
        private int _qrSecondsRemaining = 60;
        private Label _lblQrCountdown = null!;
        private Label _lblDbStatus = null!;

        public MainForm()
        {
            _controller = new TVMController();
            _stationRepo = new StationRepository();
            _txnRepo = new TransactionRepository();

            InitializeComponentCustom();
            SetupControllerEvents();
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            _stations = await _stationRepo.GetActiveStationsAsync();
            UpdateDbStatusLabel();
            ShowStateView(TVMState.Idle_Welcome);
        }

        private void UpdateDbStatusLabel()
        {
            bool hasSql = DatabaseHelper.CanConnectToSqlServer();
            if (hasSql)
            {
                _lblDbStatus.Text = "● MSSQL: Connected (SmartTicketingDB)";
                _lblDbStatus.ForeColor = Color.LightGreen;
            }
            else
            {
                _lblDbStatus.Text = "● Database: In-Memory Seed (Auto-Synced)";
                _lblDbStatus.ForeColor = Color.Gold;
            }
        }

        private void SetupControllerEvents()
        {
            _controller.StateChanged += (newState) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => ShowStateView(newState)));
                }
                else
                {
                    ShowStateView(newState);
                }
            };

            _controller.QRCountdownTick += (sec) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateQrCountdown(sec)));
                }
                else
                {
                    UpdateQrCountdown(sec);
                }
            };
        }

        private void UpdateQrCountdown(int sec)
        {
            _qrSecondsRemaining = sec;
            if (_lblQrCountdown != null)
            {
                _lblQrCountdown.Text = $"00:{sec:D2}";
            }
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Smart Ticketing System - MetroLink TVM Kiosk (Visual Programming C# & MSSQL)";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.ClientSize = new Size(1140, 740);
            this.AutoScaleMode = AutoScaleMode.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.LightBg;
            this.Font = new Font("Segoe UI", 10, FontStyle.Regular);

            // 1. Bottom Simulation Bar (Dock: Bottom)
            _pnlBottomSimBar = CreateBottomSimulationBar();
            this.Controls.Add(_pnlBottomSimBar);

            // 2. Content Panel (Dock: Fill)
            _pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.LightBg,
                Padding = new Padding(0)
            };
            this.Controls.Add(_pnlContent);

            // 3. Step Progress Bar (Dock: Top)
            _pnlStepBar = CreateStepBarPanel();
            this.Controls.Add(_pnlStepBar);

            // 4. Header Bar (Dock: Top)
            _pnlHeader = CreateHeaderPanel();
            this.Controls.Add(_pnlHeader);

            _pnlContent.SendToBack();

            // Timer for QR countdown in UI
            _qrTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _qrTimer.Tick += (s, e) =>
            {
                if (_qrSecondsRemaining > 0)
                {
                    _qrSecondsRemaining--;
                    UpdateQrCountdown(_qrSecondsRemaining);
                }
                else
                {
                    _qrTimer.Stop();
                    _controller.SetState(TVMState.QRTimedOut);
                }
            };
        }

        private Panel CreateHeaderPanel()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = UITheme.NavyDark
            };

            // Brand Logo & Title
            var lblLogo = new Label
            {
                Text = "M",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = UITheme.PrimaryBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(36, 36),
                Location = new Point(25, 12)
            };
            pnl.Controls.Add(lblLogo);

            var lblBrand = new Label
            {
                Text = "MetroLink",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(70, 16)
            };
            pnl.Controls.Add(lblBrand);

            var lblStation = new Label
            {
                Text = "📍 Central Station",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = UITheme.TextMuted,
                AutoSize = true,
                Location = new Point(190, 20)
            };
            pnl.Controls.Add(lblStation);

            // Top right utility pills in right-docked flow panel
            var pnlRightUtils = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 260,
                Height = 60,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 18, 25, 0),
                BackColor = Color.Transparent
            };

            var lblHelp = new Label
            {
                Text = "❓ Help",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            pnlRightUtils.Controls.Add(lblHelp);

            var lblAudio = new Label
            {
                Text = "🔊 Audio",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            pnlRightUtils.Controls.Add(lblAudio);

            var lblLang = new Label
            {
                Text = "🌐 English",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            pnlRightUtils.Controls.Add(lblLang);

            pnl.Controls.Add(pnlRightUtils);

            return pnl;
        }

        private Panel CreateStepBarPanel()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = UITheme.NavyMedium
            };
            pnl.Paint += (s, e) => DrawStepBar(e.Graphics, pnl.Width, pnl.Height);
            return pnl;
        }

        private void DrawStepBar(Graphics g, int w, int h)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            string[] steps = { "1. Destination", "2. Tickets", "3. Payment", "4. Collect" };
            int currentStepIdx = 0;

            switch (_controller.CurrentState)
            {
                case TVMState.Idle_Welcome:
                case TVMState.SelectingDestination:
                    currentStepIdx = 0;
                    break;
                case TVMState.ConfiguringTickets:
                    currentStepIdx = 1;
                    break;
                case TVMState.SelectingPayment:
                case TVMState.ProcessingCardPayment:
                case TVMState.ProcessingQRPayment:
                case TVMState.PaymentDeclined:
                case TVMState.QRTimedOut:
                    currentStepIdx = 2;
                    break;
                case TVMState.PrintingTickets:
                case TVMState.TicketsIssued:
                case TVMState.OutOfService:
                    currentStepIdx = 3;
                    break;
            }

            int stepWidth = 180;
            int startX = 30;

            for (int i = 0; i < steps.Length; i++)
            {
                int x = startX + (i * stepWidth);
                int y = 10;

                bool isDone = i < currentStepIdx;
                bool isCurrent = i == currentStepIdx;

                // Step Circle
                Color circleColor = isDone ? UITheme.PrimaryBlue : (isCurrent ? Color.White : Color.FromArgb(40, 60, 90));
                using var brush = new SolidBrush(circleColor);
                g.FillEllipse(brush, x, y, 22, 22);

                // Circle text
                string num = isDone ? "✓" : (i + 1).ToString();
                Color numColor = (isCurrent) ? UITheme.NavyDark : Color.White;
                using var fontNum = new Font("Segoe UI", 9, FontStyle.Bold);
                using var brushNum = new SolidBrush(numColor);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(num, fontNum, brushNum, new RectangleF(x, y, 22, 22), sf);

                // Step Label
                Color textColor = (isCurrent || isDone) ? Color.White : UITheme.TextMuted;
                using var fontText = new Font("Segoe UI", 9, isCurrent ? FontStyle.Bold : FontStyle.Regular);
                using var brushText = new SolidBrush(textColor);
                g.DrawString(steps[i].Substring(3), fontText, brushText, x + 28, y + 2);
            }
        }

        private Panel CreateBottomSimulationBar()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = Color.FromArgb(241, 245, 249),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblGuide = new Label
            {
                Text = "Grading Quick-Sim: ",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                AutoSize = true,
                Location = new Point(10, 14)
            };
            pnl.Controls.Add(lblGuide);

            // Simulation Buttons
            var btnSimCard = new Button
            {
                Text = "⚡ Card Success",
                Size = new Size(110, 28),
                Location = new Point(135, 9),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            };
            btnSimCard.Click += async (s, e) =>
            {
                PrepareQuickPurchase();
                await _controller.ProcessCardPaymentAsync(simulateDecline: false);
                await SaveAndSyncTransaction();
            };
            pnl.Controls.Add(btnSimCard);

            var btnSimQR = new Button
            {
                Text = "⚡ QR Success",
                Size = new Size(105, 28),
                Location = new Point(250, 9),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            };
            btnSimQR.Click += async (s, e) =>
            {
                PrepareQuickPurchase();
                await _controller.ProcessQRPaymentAsync("Momo / Banking QR", simulateTimeout: false);
                await SaveAndSyncTransaction();
            };
            pnl.Controls.Add(btnSimQR);

            var btnSimDecline = new Button
            {
                Text = "⚠️ Card Decline",
                Size = new Size(110, 28),
                Location = new Point(360, 9),
                BackColor = UITheme.DangerBg,
                ForeColor = UITheme.DangerRed,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            };
            btnSimDecline.Click += async (s, e) =>
            {
                PrepareQuickPurchase();
                await _controller.ProcessCardPaymentAsync(simulateDecline: true);
            };
            pnl.Controls.Add(btnSimDecline);

            var btnSimTimeout = new Button
            {
                Text = "⏰ QR Timeout",
                Size = new Size(105, 28),
                Location = new Point(475, 9),
                BackColor = UITheme.WarningBg,
                ForeColor = UITheme.WarningGold,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            };
            btnSimTimeout.Click += async (s, e) =>
            {
                PrepareQuickPurchase();
                await _controller.ProcessQRPaymentAsync("Banking QR", simulateTimeout: true);
            };
            pnl.Controls.Add(btnSimTimeout);

            var btnSimJam = new Button
            {
                Text = "🖨️ Paper Jam",
                Size = new Size(100, 28),
                Location = new Point(585, 9),
                BackColor = UITheme.DangerBg,
                ForeColor = UITheme.DangerRed,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            };
            btnSimJam.Click += async (s, e) =>
            {
                PrepareQuickPurchase();
                _controller.Printer.SimulatePaperJam();
                await _controller.ProcessCardPaymentAsync(simulateDecline: false);
            };
            pnl.Controls.Add(btnSimJam);

            // DB Status docked right
            var pnlDbStatus = new Panel
            {
                Dock = DockStyle.Right,
                Width = 350,
                BackColor = Color.Transparent
            };
            _lblDbStatus = new Label
            {
                Text = "● Checking Database...",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.SuccessGreen,
                AutoSize = true,
                Location = new Point(15, 14),
            };
            pnlDbStatus.Controls.Add(_lblDbStatus);
            pnl.Controls.Add(pnlDbStatus);

            return pnl;
        }

        private void PrepareQuickPurchase()
        {
            var dest = _stations.Find(s => s.StationName == "Riverside") ?? new Station { StationId = 6, StationName = "Riverside", BaseFare = 3.00m };
            _controller.InitSession();
            _controller.SelectDestination(dest);
            _controller.ConfigureTickets(FareCategory.Adult, 2);
        }

        private async Task SaveAndSyncTransaction()
        {
            if (_controller.CurrentTransaction != null && _controller.IssuedTickets.Count > 0)
            {
                await _txnRepo.SaveTransactionAsync(_controller.CurrentTransaction, _controller.IssuedTickets);
            }
        }

        private void ShowStateView(TVMState state)
        {
            _pnlContent.Controls.Clear();
            _pnlStepBar.Invalidate();

            switch (state)
            {
                case TVMState.Idle_Welcome:
                    _pnlContent.Controls.Add(BuildViewS01_Welcome());
                    break;
                case TVMState.SelectingDestination:
                    _pnlContent.Controls.Add(BuildViewS02_Destination());
                    break;
                case TVMState.ConfiguringTickets:
                    _pnlContent.Controls.Add(BuildViewS03_Tickets());
                    break;
                case TVMState.SelectingPayment:
                    _pnlContent.Controls.Add(BuildViewS04_PaymentMethod());
                    break;
                case TVMState.ProcessingCardPayment:
                    _pnlContent.Controls.Add(BuildViewS05_CardPayment());
                    break;
                case TVMState.ProcessingQRPayment:
                    _pnlContent.Controls.Add(BuildViewS06_QRPayment());
                    break;
                case TVMState.PaymentDeclined:
                    _pnlContent.Controls.Add(BuildViewS07_Declined());
                    break;
                case TVMState.QRTimedOut:
                    _pnlContent.Controls.Add(BuildViewS08_QRTimeout());
                    break;
                case TVMState.PrintingTickets:
                    _pnlContent.Controls.Add(BuildViewS09_Printing());
                    break;
                case TVMState.TicketsIssued:
                    _pnlContent.Controls.Add(BuildViewS10_TicketIssued());
                    break;
                case TVMState.OutOfService:
                    _pnlContent.Controls.Add(BuildViewS11_PrinterFault());
                    break;
            }
        }

        #region Screen Views Builders (Matching Figma 100%)

        // S01: Welcome Screen
        private Panel BuildViewS01_Welcome()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.LightBg };

            // Left Side: Greeting & Popular Quick Picks
            var lblTag = new Label
            {
                Text = "METRO TICKETS · ALL SERVICES RUNNING",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.SuccessGreen,
                Location = new Point(30, 20),
                AutoSize = true
            };
            pnl.Controls.Add(lblTag);

            var lblTitle = new Label
            {
                Text = "Good journeys\nstart here.",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                Location = new Point(25, 45),
                Size = new Size(400, 100)
            };
            pnl.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text = "Buy a single-journey ticket. Choose your stop to get started.",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(30, 150),
                Size = new Size(420, 25)
            };
            pnl.Controls.Add(lblSubtitle);

            var lblPopHeading = new Label
            {
                Text = "Popular destinations",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Location = new Point(30, 195),
                AutoSize = true
            };
            pnl.Controls.Add(lblPopHeading);

            // Popular Quick Pick Cards
            string[] populars = { "Market Street", "Riverside", "Harbour" };
            for (int i = 0; i < populars.Length; i++)
            {
                string destName = populars[i];
                var btnPop = new Button
                {
                    Text = $"📍  {destName}                                →",
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(380, 52),
                    Location = new Point(30, 225 + (i * 62)),
                    BackColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = UITheme.TextPrimary,
                    Cursor = Cursors.Hand
                };
                btnPop.FlatAppearance.BorderColor = UITheme.BorderColor;
                btnPop.Click += (s, e) =>
                {
                    var dest = _stations.Find(st => st.StationName == destName) ?? new Station { StationName = destName, BaseFare = 3.00m };
                    _controller.InitSession();
                    _controller.SelectDestination(dest);
                };
                pnl.Controls.Add(btnPop);
            }

            var lblNote = new Label
            {
                Text = "Pay by card, mobile wallet or banking QR.",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(30, 420),
                AutoSize = true
            };
            pnl.Controls.Add(lblNote);

            // Right Side: Transit Line Map
            var pnlRouteMap = CreateRouteMapCard(720, 20, 380, 485, selectedStationName: "");
            pnl.Controls.Add(pnlRouteMap);

            var btnCancelS01 = CreateCancelButton();
            btnCancelS01.Location = new Point(35, 525);
            pnl.Controls.Add(btnCancelS01);

            // Bottom CTA Button
            var btnStart = new Button
            {
                Text = "→  Start · Buy tickets",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Size = new Size(380, 52),
                Location = new Point(720, 520),
                BackColor = UITheme.PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += (s, e) => _controller.InitSession();
            pnl.Controls.Add(btnStart);

            return pnl;
        }

        // S02: Choose Destination
        private Panel BuildViewS02_Destination()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.LightBg };

            var lblTitle = new Label
            {
                Text = "Where are you going?",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                Location = new Point(30, 20),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text = "Touch a station in the list or on the map.",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(30, 60),
                AutoSize = true
            };
            pnl.Controls.Add(lblSubtitle);

            // Column Header
            var lblColLeft = new Label { Text = "FROM CENTRAL", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UITheme.TextMuted, Location = new Point(35, 95), AutoSize = true };
            var lblColRight = new Label { Text = "ADULT FARE", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UITheme.TextMuted, Location = new Point(330, 95), AutoSize = true };
            pnl.Controls.Add(lblColLeft);
            pnl.Controls.Add(lblColRight);

            // Radio Button List of Stations
            Station? chosen = _controller.SelectedDestination ?? _stations.Find(s => s.StationName == "Riverside");
            int yPos = 120;

            foreach (var st in _stations)
            {
                if (st.StationName == "Central") continue; // Origin

                bool isSelected = (chosen != null && chosen.StationName == st.StationName);
                var btnStation = new Button
                {
                    Text = $"  ○  {st.StationName,-20}               ${st.BaseFare:F2}",
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Size = new Size(380, 48),
                    Location = new Point(30, yPos),
                    BackColor = isSelected ? UITheme.PrimaryBlue : Color.White,
                    ForeColor = isSelected ? Color.White : UITheme.TextPrimary,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                };
                btnStation.FlatAppearance.BorderColor = isSelected ? UITheme.PrimaryBlue : UITheme.BorderColor;

                btnStation.Click += (s, e) =>
                {
                    chosen = st;
                    _controller.SelectDestination(st);
                };

                pnl.Controls.Add(btnStation);
                yPos += 54;
            }

            // Summary Text below stations
            var lblSummary = new Label
            {
                Text = $"{chosen?.StationName ?? "Riverside"} selected · {chosen?.StopOrder ?? 5} stops · {chosen?.EstimatedMinutes ?? 14} min",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                ForeColor = UITheme.PrimaryBlue,
                Location = new Point(30, yPos + 10),
                AutoSize = true
            };
            pnl.Controls.Add(lblSummary);

            // Right Side Route Map Card
            var pnlMap = CreateRouteMapCard(720, 20, 380, 485, chosen?.StationName ?? "Riverside");
            pnl.Controls.Add(pnlMap);

            // Bottom Buttons
            var btnBack = CreateBackButton(() => _controller.ResetToHome());
            btnBack.Location = new Point(30, 520);
            pnl.Controls.Add(btnBack);

            var btnCancel = CreateCancelButton();
            btnCancel.Location = new Point(160, 525);
            pnl.Controls.Add(btnCancel);

            var btnContinue = new Button
            {
                Text = "→  Continue to tickets",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(380, 52),
                Location = new Point(720, 520),
                BackColor = UITheme.PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnContinue.FlatAppearance.BorderSize = 0;
            btnContinue.Click += (s, e) =>
            {
                if (chosen != null)
                {
                    _controller.SelectDestination(chosen);
                }
            };
            pnl.Controls.Add(btnContinue);

            return pnl;
        }

        // S03: Choose Tickets & Fare
        private Panel BuildViewS03_Tickets()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.LightBg };

            var dest = _controller.SelectedDestination ?? new Station { StationName = "Riverside", BaseFare = 3.00m };

            var lblTitle = new Label
            {
                Text = "Choose your tickets",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                Location = new Point(30, 20),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text = $"Single journey to {dest.StationName}. Set the fare category and number of tickets.",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(30, 60),
                AutoSize = true
            };
            pnl.Controls.Add(lblSubtitle);

            // Destination Banner Box
            var pnlDestBox = new Panel
            {
                Size = new Size(540, 64),
                Location = new Point(30, 95),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var lblDestHead = new Label { Text = "DESTINATION", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = UITheme.TextMuted, Location = new Point(15, 10), AutoSize = true };
            var lblDestVal = new Label { Text = dest.StationName, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(14, 28), AutoSize = true };
            var btnChange = new Button
            {
                Text = "Change",
                Size = new Size(80, 32),
                Location = new Point(445, 16),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.PrimaryBlue
            };
            btnChange.Click += (s, e) => _controller.SetState(TVMState.SelectingDestination);
            pnlDestBox.Controls.Add(lblDestHead);
            pnlDestBox.Controls.Add(lblDestVal);
            pnlDestBox.Controls.Add(btnChange);
            pnl.Controls.Add(pnlDestBox);

            // 1. Choose a fare category
            var lblStep1 = new Label { Text = "1. Choose a fare category", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(30, 175), AutoSize = true };
            pnl.Controls.Add(lblStep1);

            // Fare Category Cards
            var catAdult = CreateCategoryButton("Adult", "Standard fare", "$3.00", _controller.CurrentCategory == FareCategory.Adult, () =>
            {
                _controller.ConfigureTickets(FareCategory.Adult, _controller.TicketQuantity);
                ShowStateView(TVMState.ConfiguringTickets);
            });
            catAdult.Location = new Point(30, 205);
            pnl.Controls.Add(catAdult);

            var catChild = CreateCategoryButton("Child", "Ages 5-15", "$1.50", _controller.CurrentCategory == FareCategory.Child, () =>
            {
                _controller.ConfigureTickets(FareCategory.Child, _controller.TicketQuantity);
                ShowStateView(TVMState.ConfiguringTickets);
            });
            catChild.Location = new Point(215, 205);
            pnl.Controls.Add(catChild);

            var catSenior = CreateCategoryButton("Senior", "Ages 65+", "$1.50", _controller.CurrentCategory == FareCategory.Senior, () =>
            {
                _controller.ConfigureTickets(FareCategory.Senior, _controller.TicketQuantity);
                ShowStateView(TVMState.ConfiguringTickets);
            });
            catSenior.Location = new Point(400, 205);
            pnl.Controls.Add(catSenior);

            // 2. How many tickets?
            var lblStep2 = new Label { Text = "2. How many tickets?", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(30, 305), AutoSize = true };
            pnl.Controls.Add(lblStep2);

            var pnlCounter = new Panel { Size = new Size(200, 50), Location = new Point(30, 335), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var btnMinus = new Button { Text = "-", Font = new Font("Segoe UI", 14, FontStyle.Bold), Size = new Size(50, 48), Location = new Point(0, 0), FlatStyle = FlatStyle.Flat, ForeColor = UITheme.PrimaryBlue };
            var lblQty = new Label { Text = _controller.TicketQuantity.ToString(), Font = new Font("Segoe UI", 14, FontStyle.Bold), Size = new Size(98, 48), Location = new Point(51, 0), TextAlign = ContentAlignment.MiddleCenter };
            var btnPlus = new Button { Text = "+", Font = new Font("Segoe UI", 14, FontStyle.Bold), Size = new Size(50, 48), Location = new Point(149, 0), FlatStyle = FlatStyle.Flat, ForeColor = UITheme.PrimaryBlue };

            btnMinus.Click += (s, e) =>
            {
                if (_controller.TicketQuantity > 1)
                {
                    _controller.ConfigureTickets(_controller.CurrentCategory, _controller.TicketQuantity - 1);
                    ShowStateView(TVMState.ConfiguringTickets);
                }
            };
            btnPlus.Click += (s, e) =>
            {
                if (_controller.TicketQuantity < 6)
                {
                    _controller.ConfigureTickets(_controller.CurrentCategory, _controller.TicketQuantity + 1);
                    ShowStateView(TVMState.ConfiguringTickets);
                }
            };
            pnlCounter.Controls.Add(btnMinus);
            pnlCounter.Controls.Add(lblQty);
            pnlCounter.Controls.Add(btnPlus);
            pnl.Controls.Add(pnlCounter);

            var lblCounterNote = new Label
            {
                Text = "Valid today only · No booking fee (Max 6 tickets per purchase)",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(30, 395),
                AutoSize = true
            };
            pnl.Controls.Add(lblCounterNote);

            // Right Panel: Order Summary
            var pnlSummary = CreateOrderSummaryCard(720, 20, 380, 485);
            pnl.Controls.Add(pnlSummary);

            // Bottom Buttons
            var btnBack = CreateBackButton(() => _controller.SetState(TVMState.SelectingDestination));
            btnBack.Location = new Point(30, 520);
            pnl.Controls.Add(btnBack);

            var btnCancel = CreateCancelButton();
            btnCancel.Location = new Point(160, 525);
            pnl.Controls.Add(btnCancel);

            var btnContinue = new Button
            {
                Text = $"→  Continue · ${_controller.TotalAmount:F2}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(380, 52),
                Location = new Point(720, 520),
                BackColor = UITheme.PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnContinue.FlatAppearance.BorderSize = 0;
            btnContinue.Click += (s, e) => _controller.SetState(TVMState.SelectingPayment);
            pnl.Controls.Add(btnContinue);

            return pnl;
        }

        // S04: Choose Payment Method
        private Panel BuildViewS04_PaymentMethod()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.LightBg };

            var lblTitle = new Label
            {
                Text = "How would you like to pay?",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                Location = new Point(30, 20),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text = $"Choose a payment method for your ${_controller.TotalAmount:F2} purchase.",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(30, 60),
                AutoSize = true
            };
            pnl.Controls.Add(lblSubtitle);

            // Option 1: Credit / Debit Card
            var pnlCardOpt = new Panel
            {
                Size = new Size(260, 200),
                Location = new Point(30, 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };
            var lblCardIcon = new Label { Text = "💳", Font = new Font("Segoe UI", 24), Location = new Point(15, 15), AutoSize = true };
            var lblCardTitle = new Label { Text = "Credit / Debit card", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(15, 65), AutoSize = true };
            var lblCardDesc = new Label { Text = "Tap a card or mobile wallet, or insert your chip card.", Font = new Font("Segoe UI", 9.5f), ForeColor = UITheme.TextSecondary, Location = new Point(15, 95), Size = new Size(220, 40) };
            var lblCardBadge = new Label { Text = "CONTACTLESS · CHIP", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = UITheme.PrimaryBlue, Location = new Point(15, 155), AutoSize = true };
            pnlCardOpt.Controls.AddRange(new Control[] { lblCardIcon, lblCardTitle, lblCardDesc, lblCardBadge });
            pnlCardOpt.Click += (s, e) => _controller.SetState(TVMState.ProcessingCardPayment);
            pnl.Controls.Add(pnlCardOpt);

            // Option 2: Banking / E-wallet QR
            var pnlQrOpt = new Panel
            {
                Size = new Size(260, 200),
                Location = new Point(310, 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };
            var lblQrIcon = new Label { Text = "📱", Font = new Font("Segoe UI", 24), Location = new Point(15, 15), AutoSize = true };
            var lblQrTitle = new Label { Text = "Banking / E-wallet QR", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(15, 65), AutoSize = true };
            var lblQrDesc = new Label { Text = "Scan with your mobile banking or e-wallet app.", Font = new Font("Segoe UI", 9.5f), ForeColor = UITheme.TextSecondary, Location = new Point(15, 95), Size = new Size(220, 40) };
            var lblQrBadge = new Label { Text = "60 SECONDS TO PAY", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = UITheme.PrimaryBlue, Location = new Point(15, 155), AutoSize = true };
            pnlQrOpt.Controls.AddRange(new Control[] { lblQrIcon, lblQrTitle, lblQrDesc, lblQrBadge });
            pnlQrOpt.Click += (s, e) => _controller.SetState(TVMState.ProcessingQRPayment);
            pnl.Controls.Add(pnlQrOpt);

            // Secure Payment Banner
            var pnlSecurity = new Panel { Size = new Size(540, 50), Location = new Point(30, 320), BackColor = Color.FromArgb(238, 242, 255), BorderStyle = BorderStyle.FixedSingle };
            var lblSec = new Label { Text = "🔒  Secure payment: No cash accepted. Your ticket prints after payment is confirmed.", Font = new Font("Segoe UI", 9, FontStyle.Regular), ForeColor = UITheme.NavyDark, Location = new Point(15, 15), AutoSize = true };
            pnlSecurity.Controls.Add(lblSec);
            pnl.Controls.Add(pnlSecurity);

            // Right Order Summary
            var pnlSummary = CreateOrderSummaryCard(720, 20, 380, 485);
            pnl.Controls.Add(pnlSummary);

            // Bottom Buttons
            var btnBack = CreateBackButton(() => _controller.SetState(TVMState.ConfiguringTickets));
            btnBack.Location = new Point(30, 520);
            pnl.Controls.Add(btnBack);

            var btnCancel = CreateCancelButton();
            btnCancel.Location = new Point(160, 525);
            pnl.Controls.Add(btnCancel);

            return pnl;
        }

        // S05: Card Payment
        private Panel BuildViewS05_CardPayment()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.LightBg };

            var lblTitle = new Label { Text = "Tap or insert your card", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(30, 20), AutoSize = true };
            var lblSubtitle = new Label { Text = "Use the payment reader below this screen. Do not remove an inserted card too soon.", Font = new Font("Segoe UI", 11), ForeColor = UITheme.TextSecondary, Location = new Point(30, 60), AutoSize = true };
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblSubtitle);

            // Card Reader Simulation Box
            var pnlReader = new Panel { Size = new Size(540, 260), Location = new Point(30, 100), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            // Graphic POS Terminal
            var pnlTerminal = new Panel { Size = new Size(130, 170), Location = new Point(25, 20), BackColor = UITheme.NavyDark };
            var lblTermScreen = new Label { Text = $"TOTAL\n${_controller.TotalAmount:F2}", Font = new Font("Consolas", 11, FontStyle.Bold), ForeColor = UITheme.NavyDark, BackColor = Color.FromArgb(220, 230, 242), Size = new Size(110, 50), Location = new Point(10, 15), TextAlign = ContentAlignment.MiddleCenter };
            var lblContactless = new Label { Text = "📶", Font = new Font("Segoe UI", 22), ForeColor = Color.White, Size = new Size(110, 40), Location = new Point(10, 75), TextAlign = ContentAlignment.MiddleCenter };
            pnlTerminal.Controls.Add(lblTermScreen);
            pnlTerminal.Controls.Add(lblContactless);
            pnlReader.Controls.Add(pnlTerminal);

            var lblTapText = new Label { Text = "Tap to pay", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(180, 25), AutoSize = true };
            var lblTapSub = new Label { Text = "Hold your contactless card or mobile wallet over the symbol.", Font = new Font("Segoe UI", 9.5f), ForeColor = UITheme.TextSecondary, Location = new Point(180, 52), Size = new Size(330, 25) };
            var lblInsertText = new Label { Text = "Or insert your chip card", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(180, 95), AutoSize = true };
            var lblInsertSub = new Label { Text = "Leave it in the reader until prompted to remove.", Font = new Font("Segoe UI", 9.5f), ForeColor = UITheme.TextSecondary, Location = new Point(180, 122), Size = new Size(330, 25) };

            var btnTapSuccess = new Button { Text = "✔ Simulate Card Tap (Approved)", Font = new Font("Segoe UI", 10, FontStyle.Bold), Size = new Size(230, 38), Location = new Point(180, 160), BackColor = UITheme.PrimaryBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnTapSuccess.Click += async (s, e) =>
            {
                await _controller.ProcessCardPaymentAsync(simulateDecline: false);
                await SaveAndSyncTransaction();
            };

            var btnTapDecline = new Button { Text = "✖ Simulate Card Decline", Font = new Font("Segoe UI", 9.5f), Size = new Size(180, 38), Location = new Point(420, 160), BackColor = Color.White, ForeColor = UITheme.DangerRed, FlatStyle = FlatStyle.Flat };
            btnTapDecline.Click += async (s, e) =>
            {
                await _controller.ProcessCardPaymentAsync(simulateDecline: true);
            };

            pnlReader.Controls.AddRange(new Control[] { lblTapText, lblTapSub, lblInsertText, lblInsertSub, btnTapSuccess, btnTapDecline });
            pnl.Controls.Add(pnlReader);

            // Privacy Security Notice
            var pnlPriv = new Panel { Size = new Size(540, 50), Location = new Point(30, 380), BackColor = Color.FromArgb(238, 242, 255), BorderStyle = BorderStyle.FixedSingle };
            var lblPriv = new Label { Text = "🔒  Your card details stay private. Never enter a PIN or CVV on this screen.", Font = new Font("Segoe UI", 9), ForeColor = UITheme.NavyDark, Location = new Point(15, 15), AutoSize = true };
            pnlPriv.Controls.Add(lblPriv);
            pnl.Controls.Add(pnlPriv);

            // Right Order Summary
            var pnlSummary = CreateOrderSummaryCard(720, 20, 380, 485);
            pnl.Controls.Add(pnlSummary);

            // Bottom Buttons
            var btnBack = CreateBackButton(() => _controller.SetState(TVMState.SelectingPayment));
            btnBack.Location = new Point(30, 520);
            pnl.Controls.Add(btnBack);

            var btnCancel = CreateCancelButton();
            btnCancel.Location = new Point(160, 525);
            pnl.Controls.Add(btnCancel);

            return pnl;
        }

        // S06: QR Payment (60s countdown)
        private Panel BuildViewS06_QRPayment()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.LightBg };

            var lblTitle = new Label { Text = $"Scan to pay ${_controller.TotalAmount:F2}", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(30, 20), AutoSize = true };
            var lblSubtitle = new Label { Text = "Use your mobile banking or e-wallet app. Keep this screen open until payment is confirmed.", Font = new Font("Segoe UI", 11), ForeColor = UITheme.TextSecondary, Location = new Point(30, 60), AutoSize = true };
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblSubtitle);

            // QR Box
            var pnlQrCard = new Panel { Size = new Size(540, 320), Location = new Point(30, 95), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            // QR Code Image
            var qrBmp = BarcodeRenderer.GenerateQRCodeBitmap($"METROLINK|TVM-01|TOTAL:{_controller.TotalAmount:F2}|{DateTime.UtcNow:O}", 160);
            var pbQr = new PictureBox { Image = qrBmp, Size = new Size(160, 160), Location = new Point(30, 25), SizeMode = PictureBoxSizeMode.CenterImage };
            pnlQrCard.Controls.Add(pbQr);

            // Countdown Timer Card
            var pnlTimer = new Panel { Size = new Size(300, 75), Location = new Point(210, 25), BackColor = UITheme.WarningBg, BorderStyle = BorderStyle.FixedSingle };
            var lblTimerTitle = new Label { Text = "Payment window ends in:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UITheme.WarningGold, Location = new Point(15, 12), AutoSize = true };
            _lblQrCountdown = new Label { Text = "00:59", Font = new Font("Consolas", 22, FontStyle.Bold), ForeColor = UITheme.WarningGold, Location = new Point(15, 30), AutoSize = true };
            pnlTimer.Controls.Add(lblTimerTitle);
            pnlTimer.Controls.Add(_lblQrCountdown);
            pnlQrCard.Controls.Add(pnlTimer);

            // Provider Logos & Instructions
            var lblProvider = new Label
            {
                Text = "Supported: VietQR · Momo · VNPay · ZaloPay · Mobile Banking",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                Location = new Point(210, 120),
                AutoSize = true
            };
            pnlQrCard.Controls.Add(lblProvider);

            var lblInstr = new Label
            {
                Text = "1. Open your banking or e-wallet app\n2. Select 'Scan QR' and confirm payment\n3. Tickets print immediately upon confirmation",
                Font = new Font("Segoe UI", 9),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(210, 145),
                Size = new Size(310, 50)
            };
            pnlQrCard.Controls.Add(lblInstr);

            // Simulation Action Buttons
            var btnScanSuccess = new Button
            {
                Text = "✔ Simulate Scan & Pay (Success)",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(230, 38),
                Location = new Point(30, 260),
                BackColor = UITheme.PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnScanSuccess.Click += async (s, e) =>
            {
                _qrTimer.Stop();
                await _controller.ProcessQRPaymentAsync("Momo", simulateTimeout: false);
                await SaveAndSyncTransaction();
            };

            var btnForceTimeout = new Button
            {
                Text = "⏰ Simulate 60s Timeout",
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(180, 38),
                Location = new Point(275, 260),
                BackColor = Color.White,
                ForeColor = UITheme.WarningGold,
                FlatStyle = FlatStyle.Flat
            };
            btnForceTimeout.Click += async (s, e) =>
            {
                _qrTimer.Stop();
                await _controller.ProcessQRPaymentAsync("Banking QR", simulateTimeout: true);
            };

            pnlQrCard.Controls.Add(btnScanSuccess);
            pnlQrCard.Controls.Add(btnForceTimeout);
            pnl.Controls.Add(pnlQrCard);

            // Right Summary
            var pnlSummary = CreateOrderSummaryCard(720, 20, 380, 485);
            pnl.Controls.Add(pnlSummary);

            // Bottom Buttons
            var btnBack = CreateBackButton(() =>
            {
                _qrTimer.Stop();
                _controller.SetState(TVMState.SelectingPayment);
            });
            btnBack.Location = new Point(30, 520);
            pnl.Controls.Add(btnBack);

            var btnCancel = CreateCancelButton();
            btnCancel.Location = new Point(160, 525);
            pnl.Controls.Add(btnCancel);

            // Start countdown timer
            _qrSecondsRemaining = 60;
            _qrTimer.Start();

            return pnl;
        }

        // S07: Card Declined Exception
        private Panel BuildViewS07_Declined()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.LightBg };

            var lblTitle = new Label { Text = "Your card payment was declined", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = UITheme.DangerRed, Location = new Point(30, 20), AutoSize = true };
            var lblSubtitle = new Label { Text = "No payment was taken and no ticket has been issued.", Font = new Font("Segoe UI", 11), ForeColor = UITheme.TextSecondary, Location = new Point(30, 60), AutoSize = true };
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblSubtitle);

            var pnlAlert = new Panel { Size = new Size(540, 220), Location = new Point(30, 100), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            var lblHead = new Label { Text = "Please try another way", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(20, 20), AutoSize = true };
            var lblMsg = new Label { Text = "Your bank did not approve this payment. Your ticket selection is saved.", Font = new Font("Segoe UI", 10), ForeColor = UITheme.TextSecondary, Location = new Point(20, 50), AutoSize = true };

            var pnlWarn = new Panel { Size = new Size(500, 50), Location = new Point(20, 85), BackColor = UITheme.DangerBg, BorderStyle = BorderStyle.FixedSingle };
            var lblWarn = new Label { Text = "💳  Remove your card from the reader. Contact your bank or use banking/e-wallet QR.", Font = new Font("Segoe UI", 9.5f), ForeColor = UITheme.DangerRed, Location = new Point(10, 14), AutoSize = true };
            pnlWarn.Controls.Add(lblWarn);

            var btnRetryCard = new Button { Text = "Try another card", Font = new Font("Segoe UI", 10, FontStyle.Bold), Size = new Size(180, 42), Location = new Point(20, 155), BackColor = UITheme.PrimaryBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRetryCard.Click += (s, e) => _controller.SetState(TVMState.ProcessingCardPayment);

            var btnChangeMethod = new Button { Text = "Choose another method", Font = new Font("Segoe UI", 10), Size = new Size(190, 42), Location = new Point(215, 155), BackColor = Color.White, ForeColor = UITheme.NavyDark, FlatStyle = FlatStyle.Flat };
            btnChangeMethod.Click += (s, e) => _controller.SetState(TVMState.SelectingPayment);

            pnlAlert.Controls.AddRange(new Control[] { lblHead, lblMsg, pnlWarn, btnRetryCard, btnChangeMethod });
            pnl.Controls.Add(pnlAlert);

            // Right Summary
            var pnlSummary = CreateOrderSummaryCard(720, 20, 380, 485);
            pnl.Controls.Add(pnlSummary);

            // Bottom Buttons
            var btnBack = CreateBackButton(() => _controller.SetState(TVMState.SelectingPayment));
            btnBack.Location = new Point(35, 525);
            pnl.Controls.Add(btnBack);

            var btnCancel = CreateCancelButton();
            btnCancel.Location = new Point(145, 525);
            pnl.Controls.Add(btnCancel);

            var btnChooseAnother = new Button
            {
                Text = "→  Choose another method",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(380, 52),
                Location = new Point(720, 520),
                BackColor = UITheme.PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChooseAnother.FlatAppearance.BorderSize = 0;
            btnChooseAnother.Click += (s, e) => _controller.SetState(TVMState.SelectingPayment);
            pnl.Controls.Add(btnChooseAnother);

            return pnl;
        }

        // S08: QR Timed Out Exception
        private Panel BuildViewS08_QRTimeout()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.LightBg };

            var lblTitle = new Label { Text = "Your QR payment has timed out", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = UITheme.WarningGold, Location = new Point(30, 20), AutoSize = true };
            var lblSubtitle = new Label { Text = "The 60-second payment window has ended. This transaction has been cancelled.", Font = new Font("Segoe UI", 11), ForeColor = UITheme.TextSecondary, Location = new Point(30, 60), AutoSize = true };
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblSubtitle);

            var pnlAlert = new Panel { Size = new Size(540, 240), Location = new Point(30, 100), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            // Gold zero timer banner
            var pnlZeroTimer = new Panel { Size = new Size(500, 60), Location = new Point(20, 20), BackColor = UITheme.WarningBg, BorderStyle = BorderStyle.FixedSingle };
            var lblEnded = new Label { Text = "⏱️ Payment time ended", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = UITheme.WarningGold, Location = new Point(15, 18), AutoSize = true };
            var lblZero = new Label { Text = "00:00", Font = new Font("Consolas", 18, FontStyle.Bold), ForeColor = UITheme.WarningGold, Location = new Point(410, 14), AutoSize = true };
            pnlZeroTimer.Controls.Add(lblEnded);
            pnlZeroTimer.Controls.Add(lblZero);
            pnlAlert.Controls.Add(pnlZeroTimer);

            var lblExpHead = new Label { Text = "The old QR code is no longer valid.", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(20, 95), AutoSize = true };
            var lblExpSub = new Label { Text = "No payment was confirmed. Create a new code with a fresh 60 seconds, or choose a different payment method.", Font = new Font("Segoe UI", 9.5f), ForeColor = UITheme.TextSecondary, Location = new Point(20, 122), Size = new Size(500, 40) };
            pnlAlert.Controls.Add(lblExpHead);
            pnlAlert.Controls.Add(lblExpSub);

            var btnNewQR = new Button { Text = "🔄 Create a new QR code", Font = new Font("Segoe UI", 10, FontStyle.Bold), Size = new Size(200, 42), Location = new Point(20, 175), BackColor = UITheme.PrimaryBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnNewQR.Click += (s, e) => _controller.SetState(TVMState.ProcessingQRPayment);

            var btnAnother = new Button { Text = "Choose another payment method", Font = new Font("Segoe UI", 10), Size = new Size(230, 42), Location = new Point(235, 175), BackColor = Color.White, ForeColor = UITheme.NavyDark, FlatStyle = FlatStyle.Flat };
            btnAnother.Click += (s, e) => _controller.SetState(TVMState.SelectingPayment);

            pnlAlert.Controls.Add(btnNewQR);
            pnlAlert.Controls.Add(btnAnother);
            pnl.Controls.Add(pnlAlert);

            // Right Summary
            var pnlSummary = CreateOrderSummaryCard(720, 20, 380, 485);
            pnl.Controls.Add(pnlSummary);

            // Bottom Buttons
            var btnBack = CreateBackButton(() => _controller.SetState(TVMState.SelectingPayment));
            btnBack.Location = new Point(35, 525);
            pnl.Controls.Add(btnBack);

            var btnCancel = CreateCancelButton();
            btnCancel.Location = new Point(145, 525);
            pnl.Controls.Add(btnCancel);

            var btnRegenQR = new Button
            {
                Text = "→  Create a new QR code",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(380, 52),
                Location = new Point(720, 520),
                BackColor = UITheme.PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegenQR.FlatAppearance.BorderSize = 0;
            btnRegenQR.Click += (s, e) => _controller.SetState(TVMState.ProcessingQRPayment);
            pnl.Controls.Add(btnRegenQR);

            return pnl;
        }

        // S09: Payment Confirmed & Printing Animation
        private Panel BuildViewS09_Printing()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.LightBg };

            var lblTitle = new Label { Text = "Payment confirmed. Printing your tickets.", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(30, 20), AutoSize = true };
            var lblSubtitle = new Label { Text = $"Please wait for your {_controller.TicketQuantity} ticket(s) to appear in the collection tray below.", Font = new Font("Segoe UI", 11), ForeColor = UITheme.TextSecondary, Location = new Point(30, 60), AutoSize = true };
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblSubtitle);

            var pnlPrintCard = new Panel { Size = new Size(540, 280), Location = new Point(30, 100), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            // Ticket Graphic Outline
            var pbTicketAnim = new PictureBox
            {
                Size = new Size(220, 140),
                Location = new Point(25, 25),
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle
            };
            pbTicketAnim.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.FillRectangle(new SolidBrush(UITheme.NavyDark), 0, 0, 220, 30);
                g.DrawString("PRINTING TICKETS", new Font("Segoe UI", 8.5f, FontStyle.Bold), Brushes.White, new PointF(10, 8));
                g.DrawString($"Central → {_controller.SelectedDestination?.StationName ?? "Riverside"}", new Font("Segoe UI", 9.5f, FontStyle.Bold), Brushes.Black, new PointF(10, 42));
                g.DrawString($"Fare: ${_controller.GetUnitPrice():F2}", new Font("Segoe UI", 8.5f), Brushes.DarkSlateGray, new PointF(10, 65));

                // Simulated Barcode lines
                for (int x = 10; x < 210; x += 4)
                {
                    g.FillRectangle(Brushes.Black, x, 90, 2, 35);
                }
            };
            pnlPrintCard.Controls.Add(pbTicketAnim);

            // Steps Checklist
            var lblStep1 = new Label { Text = "✔  Payment received", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = UITheme.SuccessGreen, Location = new Point(270, 30), AutoSize = true };
            var lblStep1Sub = new Label { Text = $"${_controller.TotalAmount:F2} approved", Font = new Font("Segoe UI", 9), ForeColor = UITheme.TextSecondary, Location = new Point(292, 52), AutoSize = true };

            var lblStep2 = new Label { Text = "🖨️  Printing tickets...", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = UITheme.PrimaryBlue, Location = new Point(270, 85), AutoSize = true };
            var lblStep2Sub = new Label { Text = $"{_controller.TicketQuantity} ticket(s) in progress", Font = new Font("Segoe UI", 9), ForeColor = UITheme.TextSecondary, Location = new Point(292, 107), AutoSize = true };

            var lblStep3 = new Label { Text = "⬇  Collect below", Font = new Font("Segoe UI", 11, FontStyle.Regular), ForeColor = UITheme.TextMuted, Location = new Point(270, 140), AutoSize = true };

            pnlPrintCard.Controls.AddRange(new Control[] { lblStep1, lblStep1Sub, lblStep2, lblStep2Sub, lblStep3 });

            // Animated Progress Bar
            var prgBar = new ProgressBar { Size = new Size(490, 12), Location = new Point(25, 195), Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 30 };
            pnlPrintCard.Controls.Add(prgBar);

            var lblStay = new Label { Text = "Your payment is complete. Do not pay again or leave until your tickets are ready.", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = UITheme.TextSecondary, Location = new Point(25, 225), AutoSize = true };
            pnlPrintCard.Controls.Add(lblStay);

            pnl.Controls.Add(pnlPrintCard);

            // Right Summary
            var pnlSummary = CreateOrderSummaryCard(720, 20, 380, 485);
            pnl.Controls.Add(pnlSummary);

            return pnl;
        }

        // S10: Tickets Ready for Collection (Final Output Screen)
        private Panel BuildViewS10_TicketIssued()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.LightBg };

            var lblTitle = new Label { Text = "Your tickets are ready", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(30, 20), AutoSize = true };
            var lblSubtitle = new Label { Text = "Collect tickets from the tray below. Have a good journey.", Font = new Font("Segoe UI", 11), ForeColor = UITheme.TextSecondary, Location = new Point(30, 60), AutoSize = true };
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblSubtitle);

            // Left: High Fidelity Ticket Render Card
            var pnlTicketCard = new Panel
            {
                Size = new Size(540, 290),
                Location = new Point(30, 95),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var ticket = (_controller.IssuedTickets.Count > 0) ? _controller.IssuedTickets[0] : new Ticket
            {
                DestinationStationName = _controller.SelectedDestination?.StationName ?? "Riverside",
                Price = _controller.GetUnitPrice()
            };

            // Ticket Visual Graphic Box
            var pnlTicketVis = new Panel { Size = new Size(240, 175), Location = new Point(20, 20), BackColor = Color.FromArgb(248, 250, 252), BorderStyle = BorderStyle.FixedSingle };
            var lblTktHdr = new Label { Text = "COLLECT TICKET HERE", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.White, BackColor = UITheme.NavyDark, Dock = DockStyle.Top, Height = 26, TextAlign = ContentAlignment.MiddleCenter };
            var lblTktRoute = new Label { Text = $"Central → {ticket.DestinationStationName}", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(8, 35), AutoSize = true };
            var lblTktCat = new Label { Text = $"{ticket.Category.ToString().ToUpperInvariant()} · SINGLE JOURNEY   ${ticket.Price:F2}", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = UITheme.TextSecondary, Location = new Point(8, 58), AutoSize = true };

            // Render Barcode
            var bmpBarcode = BarcodeRenderer.GenerateBarcodeBitmap(ticket.Barcode, 220, 60);
            var pbBarcode = new PictureBox { Image = bmpBarcode, Size = new Size(220, 60), Location = new Point(10, 80), SizeMode = PictureBoxSizeMode.CenterImage };
            var lblTktCode = new Label { Text = $"VALID {DateTime.UtcNow:dd MMM yyyy} | {ticket.Barcode}", Font = new Font("Consolas", 8), ForeColor = Color.DarkSlateGray, Location = new Point(10, 145), AutoSize = true };

            pnlTicketVis.Controls.AddRange(new Control[] { lblTktHdr, lblTktRoute, lblTktCat, pbBarcode, lblTktCode });
            pnlTicketCard.Controls.Add(pnlTicketVis);

            // Instructions on right of ticket
            var lblReadyHead = new Label { Text = $"✔  {_controller.TicketQuantity} ticket(s) issued", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = UITheme.SuccessGreen, Location = new Point(280, 25), AutoSize = true };
            var lblReadyTitle = new Label { Text = "Take all tickets", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(280, 52), AutoSize = true };
            var lblReadyInstr = new Label { Text = $"Scan each barcode at the fare gate. Keep your ticket until you exit at {ticket.DestinationStationName}.", Font = new Font("Segoe UI", 9.5f), ForeColor = UITheme.TextSecondary, Location = new Point(280, 80), Size = new Size(240, 55) };
            pnlTicketCard.Controls.AddRange(new Control[] { lblReadyHead, lblReadyTitle, lblReadyInstr });

            // Green Success Banner at bottom of ticket card
            var pnlGreenSuccess = new Panel { Size = new Size(500, 50), Location = new Point(20, 215), BackColor = UITheme.SuccessBg, BorderStyle = BorderStyle.FixedSingle };
            var lblSuccessMsg = new Label
            {
                Text = $"✔  Purchase complete · Transaction recorded in system\nReference: ML-{DateTime.UtcNow:yyyyMMdd}-{ticket.TicketId[4..]} · {DateTime.UtcNow:dd MMM yyyy}",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = UITheme.SuccessGreen,
                Location = new Point(10, 8),
                AutoSize = true
            };
            pnlGreenSuccess.Controls.Add(lblSuccessMsg);
            pnlTicketCard.Controls.Add(pnlGreenSuccess);

            pnl.Controls.Add(pnlTicketCard);

            // Right Summary
            var pnlSummary = CreateOrderSummaryCard(720, 20, 380, 485);
            pnl.Controls.Add(pnlSummary);

            // Finish Button
            var btnFinish = new Button
            {
                Text = "✔  Finish · Return to start",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(380, 52),
                Location = new Point(720, 520),
                BackColor = UITheme.PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFinish.FlatAppearance.BorderSize = 0;
            btnFinish.Click += (s, e) => _controller.ResetToHome();
            pnl.Controls.Add(btnFinish);

            return pnl;
        }

        // S11: Printer Fault & Out of Service Exception
        private Panel BuildViewS11_PrinterFault()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.LightBg };

            var lblTitle = new Label { Text = "Out of service", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = UITheme.DangerRed, Location = new Point(30, 20), AutoSize = true };
            var lblSubtitle = new Label { Text = "Your payment was received, but your tickets could not be printed.", Font = new Font("Segoe UI", 11), ForeColor = UITheme.TextSecondary, Location = new Point(30, 60), AutoSize = true };
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblSubtitle);

            var pnlFaultCard = new Panel { Size = new Size(540, 260), Location = new Point(30, 100), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            var lblFaultHead = new Label { Text = "🖨️  Ticket printer unavailable", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = UITheme.DangerRed, Location = new Point(20, 20), AutoSize = true };
            var lblFaultDesc = new Label { Text = "A printer jam stopped this purchase. No tickets were issued.", Font = new Font("Segoe UI", 10), ForeColor = UITheme.TextSecondary, Location = new Point(20, 50), AutoSize = true };

            // Automatic Refund Banner
            var pnlRefund = new Panel { Size = new Size(500, 60), Location = new Point(20, 85), BackColor = UITheme.WarningBg, BorderStyle = BorderStyle.FixedSingle };
            var lblRefundTitle = new Label { Text = $"🔄  ${_controller.TotalAmount:F2} automatic refund initiated", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = UITheme.WarningGold, Location = new Point(10, 10), AutoSize = true };
            var lblRefundSub = new Label { Text = "The refund has been requested to your original payment method. Processing time depends on bank.", Font = new Font("Segoe UI", 8.5f), ForeColor = UITheme.NavyDark, Location = new Point(10, 32), AutoSize = true };
            pnlRefund.Controls.Add(lblRefundTitle);
            pnlRefund.Controls.Add(lblRefundSub);

            var lblMaint = new Label { Text = "🔧  Maintenance team notified. This machine is locked from new purchases.", Font = new Font("Segoe UI", 9.5f), ForeColor = UITheme.TextSecondary, Location = new Point(20, 160), AutoSize = true };

            // Reload / Repair Button for testing
            var btnReload = new Button { Text = "Clear Jam & Restore Service", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Size = new Size(220, 36), Location = new Point(20, 200), BackColor = UITheme.PrimaryBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnReload.Click += (s, e) =>
            {
                _controller.Printer.ClearPaperJam();
                _controller.Printer.ReloadPaper(500);
                _controller.IsInService = true;
                _controller.ResetToHome();
            };

            pnlFaultCard.Controls.AddRange(new Control[] { lblFaultHead, lblFaultDesc, pnlRefund, lblMaint, btnReload });
            pnl.Controls.Add(pnlFaultCard);

            // Right Summary
            var pnlSummary = CreateOrderSummaryCard(720, 20, 380, 485);
            pnl.Controls.Add(pnlSummary);

            return pnl;
        }

        #endregion

        #region Helper Reusable UI Cards

        private Panel CreateRouteMapCard(int x, int y, int w, int h, string selectedStationName)
        {
            var pnl = new Panel
            {
                Size = new Size(w, h),
                Location = new Point(x, y),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Header line badge
            var lblBadge = new Label
            {
                Text = "01",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = UITheme.LineBlue,
                Size = new Size(28, 22),
                Location = new Point(15, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var lblLineName = new Label
            {
                Text = "Riverside line",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                Location = new Point(48, 15),
                AutoSize = true
            };
            var lblToHarbour = new Label
            {
                Text = "To Harbour →",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(w - 110, 17),
                AutoSize = true
            };
            pnl.Controls.AddRange(new Control[] { lblBadge, lblLineName, lblToHarbour });

            // Draw Transit Line Route
            var pnlCanvas = new Panel { Size = new Size(w - 30, h - 85), Location = new Point(15, 45), BackColor = Color.White };
            pnlCanvas.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                string[] route = { "Central", "Market Street", "Civic Square", "City Park", "Museum", "Riverside", "Harbour" };
                int lineX = 25;
                int startY = 20;
                int stepY = 40;

                // Vertical Blue Transit Line
                using var penLine = new Pen(UITheme.LineBlue, 4);
                g.DrawLine(penLine, lineX, startY, lineX, startY + (route.Length - 1) * stepY);

                for (int i = 0; i < route.Length; i++)
                {
                    string stName = route[i];
                    int cy = startY + (i * stepY);

                    bool isSelected = (stName == selectedStationName);
                    bool isOrigin = (i == 0);

                    // Circle stop
                    if (isSelected)
                    {
                        using var brushSel = new SolidBrush(UITheme.NavyDark);
                        g.FillEllipse(brushSel, lineX - 7, cy - 7, 14, 14);
                    }
                    else
                    {
                        using var brushStop = new SolidBrush(Color.White);
                        using var penStop = new Pen(UITheme.LineBlue, 3);
                        g.FillEllipse(brushStop, lineX - 6, cy - 6, 12, 12);
                        g.DrawEllipse(penStop, lineX - 6, cy - 6, 12, 12);
                    }

                    // Station Name Text
                    using var fontSt = new Font("Segoe UI", 9.5f, (isSelected || isOrigin) ? FontStyle.Bold : FontStyle.Regular);
                    using var brushSt = new SolidBrush((isSelected || isOrigin) ? UITheme.NavyDark : UITheme.TextPrimary);
                    g.DrawString(stName, fontSt, brushSt, lineX + 18, cy - 8);

                    // Tags
                    if (isOrigin)
                    {
                        using var fontTag = new Font("Segoe UI", 8, FontStyle.Bold);
                        using var brushTag = new SolidBrush(UITheme.PrimaryBlue);
                        g.DrawString("YOU ARE HERE", fontTag, brushTag, w - 140, cy - 7);
                    }
                    else if (isSelected)
                    {
                        using var brushBadge = new SolidBrush(UITheme.NavyDark);
                        g.FillRectangle(brushBadge, w - 120, cy - 9, 75, 18);
                        using var fontBadge = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                        g.DrawString("SELECTED", fontBadge, Brushes.White, w - 115, cy - 7);
                    }
                    else if (stName == "Riverside")
                    {
                        using var brushBadge = new SolidBrush(UITheme.PrimaryBlue);
                        g.FillRectangle(brushBadge, w - 110, cy - 9, 65, 18);
                        using var fontBadge = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                        g.DrawString("POPULAR", fontBadge, Brushes.White, w - 105, cy - 7);
                    }
                }
            };
            pnl.Controls.Add(pnlCanvas);

            var lblFooter = new Label { Text = "One line. No changes needed.", Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = UITheme.TextSecondary, Location = new Point(15, h - 30), AutoSize = true };
            pnl.Controls.Add(lblFooter);

            return pnl;
        }

        private Panel CreateOrderSummaryCard(int x, int y, int w, int h)
        {
            var pnl = new Panel
            {
                Size = new Size(w, h),
                Location = new Point(x, y),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var dest = _controller.SelectedDestination ?? new Station { StationName = "Riverside", BaseFare = 3.00m };

            var lblHead = new Label { Text = "Your journey", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(20, 20), AutoSize = true };

            var lblBadge = new Label { Text = "01", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.White, BackColor = UITheme.LineBlue, Size = new Size(24, 18), Location = new Point(20, 52), TextAlign = ContentAlignment.MiddleCenter };
            var lblLine = new Label { Text = "Riverside line", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(48, 52), AutoSize = true };

            var lblRoute = new Label { Text = $"📍 Central\n⬇\n📍 {dest.StationName}", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(20, 85), Size = new Size(200, 60) };
            var lblStops = new Label { Text = "5 stops · About 14 minutes", Font = new Font("Segoe UI", 8.5f), ForeColor = UITheme.TextSecondary, Location = new Point(20, 150), AutoSize = true };

            var lineSep1 = new Panel { Size = new Size(w - 40, 1), Location = new Point(20, 180), BackColor = UITheme.BorderColor };

            var lblQtyRow = new Label { Text = $"{_controller.TicketQuantity} × {_controller.CurrentCategory}", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(20, 195), AutoSize = true };
            var lblUnitPrice = new Label { Text = $"Single journey · ${_controller.GetUnitPrice():F2} each", Font = new Font("Segoe UI", 8.5f), ForeColor = UITheme.TextSecondary, Location = new Point(20, 218), AutoSize = true };
            var lblSubTotal = new Label { Text = $"${_controller.TotalAmount:F2}", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(w - 90, 195), AutoSize = true };

            var lineSep2 = new Panel { Size = new Size(w - 40, 1), Location = new Point(20, 250), BackColor = UITheme.BorderColor };

            var lblTotalHead = new Label { Text = "Total to pay", Font = new Font("Segoe UI", 9.5f), ForeColor = UITheme.TextSecondary, Location = new Point(20, 265), AutoSize = true };
            var lblTotalVal = new Label { Text = $"${_controller.TotalAmount:F2}", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(18, 290), AutoSize = true };

            pnl.Controls.AddRange(new Control[] { lblHead, lblBadge, lblLine, lblRoute, lblStops, lineSep1, lblQtyRow, lblUnitPrice, lblSubTotal, lineSep2, lblTotalHead, lblTotalVal });
            return pnl;
        }

        private Button CreateCategoryButton(string title, string sub, string price, bool isSelected, Action onClick)
        {
            var btn = new Button
            {
                Size = new Size(170, 85),
                BackColor = isSelected ? Color.FromArgb(238, 242, 255) : Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = isSelected ? UITheme.PrimaryBlue : UITheme.BorderColor;
            btn.FlatAppearance.BorderSize = isSelected ? 2 : 1;

            var lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = UITheme.NavyDark, Location = new Point(12, 10), AutoSize = true };
            var lblSub = new Label { Text = sub, Font = new Font("Segoe UI", 8.5f), ForeColor = UITheme.TextSecondary, Location = new Point(12, 32), AutoSize = true };
            var lblPrice = new Label { Text = price, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = UITheme.PrimaryBlue, Location = new Point(12, 52), AutoSize = true };

            // Bubble clicks
            lblTitle.Click += (s, e) => onClick();
            lblSub.Click += (s, e) => onClick();
            lblPrice.Click += (s, e) => onClick();
            btn.Click += (s, e) => onClick();

            btn.Controls.AddRange(new Control[] { lblTitle, lblSub, lblPrice });
            return btn;
        }

        private Button CreateBackButton(Action onBack)
        {
            var btn = new Button
            {
                Text = "←  Back",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(110, 44),
                BackColor = Color.White,
                ForeColor = UITheme.NavyDark,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = UITheme.BorderColor;
            btn.Click += (s, e) => onBack();
            return btn;
        }

        private Button CreateCancelButton()
        {
            var btn = new Button
            {
                Text = "✕  Cancel",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Size = new Size(100, 34),
                BackColor = Color.Transparent,
                ForeColor = UITheme.DangerRed,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                var dr = MessageBox.Show("Are you sure you want to cancel this purchase session?", "Cancel Transaction", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    _controller.CancelSession();
                }
            };
            return btn;
        }

        #endregion
    }
}
