using DoxmandAPI.Models;
using static DoxmandAPI.Models.Product;

namespace DoxmandAPI.DTOs
{
    public class ProductDTO
    {
        public string Name { get; set; }
        public int SpeakerNumber { get; set; }
        public AlarmType Type { get; set; }
        public string PictureUrl { get; set; }
        public string SavedName { get; set; }
    }
}