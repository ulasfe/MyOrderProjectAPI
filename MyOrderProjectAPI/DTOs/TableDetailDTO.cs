using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.DTOs
{
    public class TableDetailDTO
    {
        public int Id { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public Status Status { get; set; } = Status.Boş;

        // Masa dolu olduğunda o masaya ait sipariş bilgilerine ulaşabilmk için gerekli.
        public int? CurrentOrderId { get; set; }
    }
}
