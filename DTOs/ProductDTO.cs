using static DoxmandBackend.Models.Product;

namespace DoxmandBackend.DTOs
{
    public class ProductDTO
    {
        public string Name { get; set; }
        public int SpeakerNumber { get; set; }
        public AlarmType Type { get; set; }
        public string PictureUrl { get; set; }
        public string SavedName { get; set; }
        public long Room_ID { get; set; }
        public string MapIconUrl { get; set; }
    }
}