using System.Text.Json.Serialization;

namespace MailSort;

public class MailSortRule
{
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	[JsonPropertyName("haystack")]
	[JsonConverter(typeof(EnumCapitalizationConverter<Haystack>))]
	public Haystack Haystack { get; set; }
		
	[JsonPropertyName("needle")]
	public string Needle { get; set; } = null!;

	[JsonPropertyName("matchingMethod")]
	[JsonConverter(typeof(EnumCapitalizationConverter<MatchingMethod>))]
	public MatchingMethod MatchingMethod { get; set; }

	[JsonPropertyName("targetFolder")]
	public string TargetFolder { get; set; } = null!;

	[JsonPropertyName("combineWith")]
	public string? CombineWith { get; set; }
		
	[JsonPropertyName("combinationMethod")]
	[JsonConverter(typeof(EnumCapitalizationConverter<CombinationMethod>))]
	public CombinationMethod CombinationMethod { get; set; }

	[JsonPropertyName("isCombinationRule")]
	public bool IsCombinationRule { get; set; }
}

public enum MatchingMethod
{
	Contains, Equals, ContainsIgnoreCase, EqualsIgnoreCase, GreaterThanOrEqual
}

public enum CombinationMethod
{
	LogicalAnd, LogicalOr
}

public enum Haystack
{
	Subject, Body, Cc, Bcc, Sender, Recipients, RecipientsAndCc, RecipientsAndBcc, CcAndBcc, RecipientsAndCcAndBcc, Date
}