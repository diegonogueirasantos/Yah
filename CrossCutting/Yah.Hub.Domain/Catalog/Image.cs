namespace Yah.Hub.Domain.Catalog
{
    public class Image : ICloneable
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsMain { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
