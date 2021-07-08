namespace AquaMan.WebsocketAdapter.Command
{
    public class LoginCommand
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string AgentId { get; set; }
        public Money moeny { get; set; }
    }
}
