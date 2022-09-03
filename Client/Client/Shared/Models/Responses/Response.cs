namespace Client.Shared.Models.Responses
{
    public class Response
    {
        public string Message { get; set; } = "";
        public Dictionary<string, string> Tokens { get; set; } = new();
    }
}
