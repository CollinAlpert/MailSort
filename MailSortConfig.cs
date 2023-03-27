using CommandLine;

namespace MailSort;

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

	[Option("no-ssl", Required = false, Default = false, HelpText = "Do not connect to the IMAP server through port 993.")]
	public bool NoSsl { get; set; }

	[Option('l', "log", Required = false, Default = "imap.log", HelpText = "The file to log the protocol log to.")]
	public string LogFile { get; set; } = null!;

	[Option("no-log", Required = false, Default = false, HelpText = "Do not create a log file with the protocol log.")]
	public bool DontLog { get; set; }
}