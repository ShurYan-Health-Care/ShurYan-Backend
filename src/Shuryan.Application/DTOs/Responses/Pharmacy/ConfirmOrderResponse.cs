namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class ConfirmOrderResponse
    {
        public Guid OrderId { get; set; }
        public Core.Enums.Pharmacy.PharmacyOrderStatus Status { get; set; }
        public DateTime ConfirmedAt { get; set; }
    }
}
