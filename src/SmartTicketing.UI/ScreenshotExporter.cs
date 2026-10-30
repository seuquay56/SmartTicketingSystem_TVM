using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SmartTicketing.Core.Controllers;
using SmartTicketing.Core.Models;

namespace SmartTicketing.UI
{
    public static class ScreenshotExporter
    {
        public static void ExportAllScreens(string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            var form = new MainForm();
            form.ClientSize = new Size(1140, 740);
            form.Show();

            var pnlHdr = (Panel)typeof(MainForm).GetField("_pnlHeader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(form)!;
            var pnlStep = (Panel)typeof(MainForm).GetField("_pnlStepBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(form)!;
            var pnlContent = (Panel)typeof(MainForm).GetField("_pnlContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(form)!;
            var pnlSim = (Panel)typeof(MainForm).GetField("_pnlBottomSimBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(form)!;
            var ctrl = (TVMController)typeof(MainForm).GetField("_controller", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(form)!;

            void CompositeCapture(string fileName)
            {
                form.Refresh();
                Application.DoEvents();
                System.Threading.Thread.Sleep(150);

                int w = 1140;
                int h = 740;
                using var fullBmp = new Bitmap(w, h);
                using (var g = Graphics.FromImage(fullBmp))
                {
                    g.Clear(Color.FromArgb(244, 247, 251));

                    // 1. Draw Header
                    using var bmpHdr = new Bitmap(w, pnlHdr.Height);
                    pnlHdr.DrawToBitmap(bmpHdr, new Rectangle(0, 0, w, pnlHdr.Height));
                    g.DrawImage(bmpHdr, 0, 0);

                    // 2. Draw Step Bar
                    using var bmpStep = new Bitmap(w, pnlStep.Height);
                    pnlStep.DrawToBitmap(bmpStep, new Rectangle(0, 0, w, pnlStep.Height));
                    g.DrawImage(bmpStep, 0, pnlHdr.Height);

                    // 3. Draw Bottom Sim Bar
                    int simY = h - pnlSim.Height;
                    using var bmpSim = new Bitmap(w, pnlSim.Height);
                    pnlSim.DrawToBitmap(bmpSim, new Rectangle(0, 0, w, pnlSim.Height));
                    g.DrawImage(bmpSim, 0, simY);

                    // 4. Draw Content Panel
                    int contentY = pnlHdr.Height + pnlStep.Height;
                    int contentH = simY - contentY;
                    using var bmpContent = new Bitmap(w, contentH);
                    pnlContent.DrawToBitmap(bmpContent, new Rectangle(0, 0, w, contentH));
                    g.DrawImage(bmpContent, 0, contentY);
                }

                fullBmp.Save(Path.Combine(outputDir, fileName), System.Drawing.Imaging.ImageFormat.Png);
            }

            // S01 Welcome
            form.Invoke(new Action(() => ctrl.ResetToHome()));
            CompositeCapture("screenshot_s01_welcome.png");

            // S02 Destination
            form.Invoke(new Action(() => ctrl.InitSession()));
            CompositeCapture("screenshot_s02_destination.png");

            // S03 Input Form
            form.Invoke(new Action(() =>
            {
                var dest = new Station { StationId = 6, StationName = "Riverside", BaseFare = 3.00m, StopOrder = 5, EstimatedMinutes = 14 };
                ctrl.SelectDestination(dest);
                ctrl.ConfigureTickets(FareCategory.Adult, 2);
                ctrl.SetState(TVMState.ConfiguringTickets);
            }));
            CompositeCapture("screenshot_s03_input_form.png");

            // S04 Payment Methods
            form.Invoke(new Action(() => ctrl.SetState(TVMState.SelectingPayment)));
            CompositeCapture("screenshot_s04_payment_methods.png");

            // S06 QR Payment
            form.Invoke(new Action(() => ctrl.SetState(TVMState.ProcessingQRPayment)));
            CompositeCapture("screenshot_s06_qr_payment.png");

            // S10 Output Ticket (Success Report)
            form.Invoke(new Action(() =>
            {
                var tkt = new Ticket
                {
                    TicketId = "TKT-2026-0942",
                    Barcode = "TKT-2026-0942",
                    DestinationStationName = "Riverside",
                    Category = FareCategory.Adult,
                    Price = 3.00m,
                    TicketNumber = 1,
                    TotalTicketsInOrder = 2
                };
                ctrl.IssuedTickets.Clear();
                ctrl.IssuedTickets.Add(tkt);
                ctrl.SetState(TVMState.TicketsIssued);
            }));
            CompositeCapture("screenshot_s10_output_ticket.png");

            form.Close();
        }
    }
}
