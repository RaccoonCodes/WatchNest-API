namespace WatchNestApp.DTO
{
    public class LinkDTO
    {
        public string Href { get; private set; } 
        public string Rel { get; private set; } 
        public string Type { get; private set; }

        public LinkDTO(string href, string rel, string type)
        => (Href, Rel, Type) = (href, rel, type);
    }
}
