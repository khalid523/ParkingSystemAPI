﻿namespace ParkingSystemAPI.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
