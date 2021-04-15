using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using MimeKit.Text;

namespace MailSort
{
	public class Program
	{
		private const int ImapPort = 143;
		private const int EncryptedImapPort = 993;
		
		private static readonly IDictionary<MatchingMethod, Func<string, string, bool>> MethodMapping = new Dictionary<MatchingMethod, Func<string, string, bool>>
		{
			[MatchingMethod.Contains] = (haystack, needle) => haystack.Contains(needle),
			[MatchingMethod.Equals] = (haystack, needle) => haystack.Equals(needle),
			[MatchingMethod.ContainsIgnoreCase] = (haystack, needle) => haystack.Contains(needle, StringComparison.CurrentCultureIgnoreCase),
			[MatchingMethod.EqualsIgnoreCase] = (haystack, needle) => haystack.Equals(needle, StringComparison.CurrentCultureIgnoreCase)
		};
			
		private static readonly IDictionary<Haystack, Func<MimeMessage, string>> HaystackMapping = new Dictionary<Haystack, Func<MimeMessage, string>>
		{
			[Haystack.Subject] = m => m.Subject,
			[Haystack.Body] = m => m.GetTextBody(TextFormat.Plain),
			[Haystack.Cc] = m => GetListHeader(m, message => message.Cc),
			[Haystack.Bcc] = m => GetListHeader(m, message => message.Bcc),
			[Haystack.Sender] = m => GetListHeader(m, message => message.From),
			[Haystack.Recipients] = m => GetListHeader(m, message => message.To),
			[Haystack.RecipientsAndCc] = m => string.Join(", ", GetListHeader(m, message => message.To), GetListHeader(m, message => message.Cc)),
			[Haystack.RecipientsAndBcc] = m => string.Join(", ", GetListHeader(m, message => message.To), GetListHeader(m, message => message.Bcc)),
			[Haystack.CcAndBcc] = m => string.Join(", ", GetListHeader(m, message => message.Cc), GetListHeader(m, message => message.Bcc)),
			[Haystack.RecipientsAndCcAndBcc] = m => string.Join(", ", GetListHeader(m, message => message.To), GetListHeader(m, message => message.Cc), GetListHeader(m, message => message.Bcc)),
		};

		private static readonly IDictionary<CombinationMethod, Func<Expression<Func<MimeMessage, bool>>, Expression<Func<MimeMessage, bool>>, Expression<Func<MimeMessage, bool>>>> CombinationMapping = new Dictionary<CombinationMethod, Func<Expression<Func<MimeMessage, bool>>, Expression<Func<MimeMessage, bool>>, Expression<Func<MimeMessage, bool>>>>
		{
			[CombinationMethod.LogicalAnd] = (a, b) => a.And(b),
			[CombinationMethod.LogicalOr] = (a, b) => a.Or(b)
		};

		public static Task Main(string[] args)
		{
			return Parser.Default.ParseArguments<MailSortConfig>(args).WithParsedAsync(RunAsync);
		}

		private static async Task RunAsync(MailSortConfig config)
		{
			var rules = GetRules(config.ConfigFile).ToList();
			if (rules.Count == 0)
			{
				throw new Exception("No rules found. Please define rules first.");
			}
			
			IEnumerable<Tuple<Func<MimeMessage, bool>, string>> predicatesAndTargetFolders =
				rules.Where(r => !r.IsCombinationRule)
					.Select(r => Tuple.Create(BuildPredicate(GetCombinedRules(r, new Queue<MailSortRule>(new[] { r }), rules)), r.TargetFolder)).ToList();
			
			IProtocolLogger logger = config.DontLog ? new NullProtocolLogger() : new ProtocolLogger(config.LogFile);
			using var imapClient = new ImapClient(logger);
			await imapClient.ConnectAsync(config.Host, config.UseSsl ? EncryptedImapPort : ImapPort, config.UseSsl).ConfigureAwait(false);
			await imapClient.AuthenticateAsync(config.Username, config.Password).ConfigureAwait(false);
			
			var inbox = imapClient.Inbox;
			await inbox.OpenAsync(FolderAccess.ReadWrite).ConfigureAwait(false);
			var summaries = await inbox.FetchAsync(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.InternalDate | MessageSummaryItems.Flags);
			foreach (var summary in summaries)
			{
				if (summary.Flags!.Value.HasFlag(MessageFlags.Deleted))
				{
					continue;
				}
				
				var message = await inbox.GetMessageAsync(summary.UniqueId).ConfigureAwait(false);
				if (message == null)
				{
					continue;
				}

				var tuple = predicatesAndTargetFolders.FirstOrDefault(t => t.Item1(message));
				if (tuple == null)
				{
					continue;
				}

				IMailFolder destinationFolder;
				try
				{
					destinationFolder = await imapClient.GetFolderAsync(tuple.Item2).ConfigureAwait(false);
				}
				catch (FolderNotFoundException)
				{
					var folders = await imapClient.GetFoldersAsync(imapClient.PersonalNamespaces[0]).ConfigureAwait(false);
					throw new Exception(
						$"The folder '{tuple.Item2}' was not found. The following folders are available: {string.Join(", ", folders.Select(f => f.FullName))}");
				}
					
				await destinationFolder.OpenAsync(FolderAccess.ReadWrite).ConfigureAwait(false);
				await destinationFolder.AppendAsync(message, summary.Flags!.Value, summary.InternalDate!.Value).ConfigureAwait(false);
				await destinationFolder.CloseAsync().ConfigureAwait(false);
						
				await inbox.OpenAsync(FolderAccess.ReadWrite).ConfigureAwait(false); 
				await inbox.AddFlagsAsync(summary.UniqueId, MessageFlags.Deleted, true).ConfigureAwait(false);
			}

			await inbox.CloseAsync(true).ConfigureAwait(false);
			await imapClient.DisconnectAsync(true).ConfigureAwait(false);
		}

		private static Queue<MailSortRule> GetCombinedRules(MailSortRule rule, Queue<MailSortRule> foundRules, IReadOnlyList<MailSortRule> allRules)
		{
			if (string.IsNullOrWhiteSpace(rule.CombineWith))
			{
				return foundRules;
			}

			var combinedRule = allRules.FirstOrDefault(x => rule.CombineWith == x.Id);
			if (combinedRule == null)
			{
				throw new Exception($"Rule with id '{rule.CombineWith}' does not exist and thus cannot be referenced by another rule.");
			}
			
			foundRules.Enqueue(combinedRule);

			return GetCombinedRules(combinedRule, foundRules, allRules);
		}

		private static Func<MimeMessage, bool> BuildPredicate(Queue<MailSortRule> rules)
		{
			var builder = PredicateBuilder.True<MimeMessage>();
			
			var nextCombinationMethod = CombinationMethod.LogicalAnd;
			while (rules.TryDequeue(out var rule))
			{
				var haystackMethod = HaystackMapping[rule.Haystack];
				var method = MethodMapping[rule.MatchingMethod];
				builder = CombinationMapping[nextCombinationMethod](builder, m => method(haystackMethod(m), rule.Needle));
				nextCombinationMethod = rule.CombinationMethod;
			}
			
			return builder.Compile();
		}

		private static IEnumerable<MailSortRule> GetRules(string configLocation)
		{
			if (!File.Exists(configLocation))
			{
				throw new Exception("Config file could not be found.");
			}

			var json = File.ReadAllText(configLocation);
			var rules = JsonSerializer.Deserialize<IEnumerable<MailSortRule>>(json, new JsonSerializerOptions {AllowTrailingCommas = true});
			if (rules == null)
			{
				throw new Exception("Rules could not be parsed. Please create an issue on GitHub.");
			}

			return rules;
		}
		
		private static string GetListHeader(MimeMessage message, Func<MimeMessage, InternetAddressList> mapping)
		{
			return string.Join(", ", mapping(message).Select(x => x.ToString()));
		}
	}
}