using DoxmandBackend.DTOs;

namespace DoxmandBackend.Models
{
    public class Product
    {
        public Product()
        {
            Product_ID = "";
            Name = "";
            SpeakerNumber = -1;
            Type = 0;
            PictureUrl = "";
            SavedName = "";
            Room_ID = -1;
        }

        public Product(string name, int speakerNumber, AlarmType type, string pictureUrl, string savedName, long roomId)
        {
            Name = name;
            SpeakerNumber = speakerNumber;
            Type = type;
            PictureUrl = pictureUrl;
            SavedName = savedName;
            Room_ID = roomId;
        }

        public Product(ProductDTO productDto)
        {
            Name = productDto.Name;
            SpeakerNumber = productDto.SpeakerNumber;
            Type = productDto.Type;
            PictureUrl = productDto.PictureUrl;
            SavedName = productDto.SavedName;
            Room_ID = productDto.Room_ID;
        }

        public enum AlarmType
        {
            Wild,
            Rodent,
            Starling
        }
        
        public string Product_ID { get; set; }
        public string Name { get; set; }
        public int SpeakerNumber { get; set; }
        public AlarmType Type { get; set; }
        public string PictureUrl { get; set; }
        public string SavedName { get; set; }
        public long Room_ID { get; set; }
    }
}