namespace ProjectFunctions.Models
{
    /// <summary>
    /// This class is used to represent the topic message
    /// </summary>
    public class PRInfo
    {
        public PRInfo()  {  }
        public string Prefix { get; set; }
        public string PRId { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string BaseURL { get; set; }
        public string RepoID { get; set; }
        public string PullRequestID { get; set; }
        public string Token { get; set; }
    }
}
