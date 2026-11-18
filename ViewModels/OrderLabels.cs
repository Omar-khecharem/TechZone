using TechZone.Models;

namespace TechZone.ViewModels
{
    public static class OrderLabels
    {
        public static string GetStatusLabel(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Confirmed => "Confirmée",
                OrderStatus.Shipped => "Expédiée",
                OrderStatus.Delivered => "Livrée",
                OrderStatus.Cancelled => "Annulée",
                _ => "Inconnu"
            };
        }

        public static string GetPaymentMethodLabel(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.CashOnDelivery => "Paiement à la livraison",
                _ => "Non renseigné"
            };
        }

        public static string GetPaymentStatusLabel(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "En attente",
                PaymentStatus.Paid => "Payé",
                PaymentStatus.Failed => "Échoué",
                _ => "Inconnu"
            };
        }
    }
}
