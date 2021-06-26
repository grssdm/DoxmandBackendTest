namespace DoxmandAPI.Models
{
    public class Product
    {
        public Product(string name, int speakerNumber, AlarmType type, string pictureUrl, string savedName)
        {
            Name = name;
            SpeakerNumber = speakerNumber;
            Type = type;
            PictureUrl = pictureUrl;
            SavedName = savedName;
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
    }
}