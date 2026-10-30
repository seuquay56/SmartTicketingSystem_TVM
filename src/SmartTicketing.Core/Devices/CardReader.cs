using System;

namespace SmartTicketing.Core.Devices
{
    /// <summary>
    /// Hardware interface controller for the EMV / Contactless Card Reader.
    /// Synchronized with UML Class Diagram 3.3.3.
    /// </summary>
    public class CardReader
    {
        public string DevicePort { get; set; } = "COM3";
        public bool IsCardInserted { get; private set; } = false;

        private string _cardNumber = "•••• •••• •••• 4242";
        private string _holderName = "VALUED PASSENGER";

        public void InsertOrTapCard(string cardNumber = "•••• •••• •••• 4242", string holderName = "VALUED PASSENGER")
        {
            _cardNumber = cardNumber;
            _holderName = holderName;
            IsCardInserted = true;
        }

        public bool ReadCardInfo(out string maskedCardNumber, out string cardHolderName)
        {
            if (!IsCardInserted)
            {
                maskedCardNumber = string.Empty;
                cardHolderName = string.Empty;
                return false;
            }

            maskedCardNumber = _cardNumber;
            cardHolderName = _holderName;
            return true;
        }

        public void EjectCard()
        {
            IsCardInserted = false;
        }
    }
}
