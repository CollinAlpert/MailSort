using CommandLine;

namespace MailSort
{
	public class MailSortConfig
	{
		[Option('h', "host", Required = true, HelpText = "The IMAP host to connect to.")]
		public string Host { get; set; } = null!;
		
		[Option('u', "username", Required = true, HelpText = "The username to connect via IMAP with.")]
		public string Username { get; set; } = null!;

		[Option('p', "password", Required = true, HelpText = "The password to authenticate to the IMAP server with.")]
		public string Password { get; set; } = null!;

		[Option('c', "config", Required = true, HelpText = "A JSON file containing the rules by which to sort emails into folders.")]
		public string ConfigFile { get; set; } = null!;
		
		[Option('s', "ssl", Required = false, Default = true, HelpText = "Whether or not to connect to the IMAP server securely.")]
		public bool UseSsl { get; set; }
	}
}