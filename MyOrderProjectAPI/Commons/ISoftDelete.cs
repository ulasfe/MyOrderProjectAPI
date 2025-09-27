namespace MyOrderProjectAPI.Commons
{
    public interface ISoftDelete
    {
        public bool RecordStatus { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifyDate { get; set; } // Nullable, ilk oluşturulmada boş kalabilir.
    }
}
