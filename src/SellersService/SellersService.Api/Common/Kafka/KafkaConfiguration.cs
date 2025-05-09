namespace SellersService.Api.Common.Kafka;

public class KafkaConfiguration
{
    public static string SectionName = "Kafka";
    
    public string BootstrapServices { get; set; }
    
    public string PaymentEventsTopic { get; set; }
    public string OrderEventsTopic { get; set; }
    public string SellerEventsTopic { get; set; }
    public string UserEventsTopic { get; set; }
}