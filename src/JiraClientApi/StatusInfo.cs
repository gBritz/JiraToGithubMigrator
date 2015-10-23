namespace JiraApi
{
    public class StatusInfo
    {
        public string Name { get; set; }

        public string IconUrl { get; set; }

        public string Description { get; set; }

        public bool IsClosed
        {
            get { return Name == "Closed"; }
        }
    }
}