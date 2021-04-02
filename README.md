# MailSort
"Is this another sorting algorithm?", you might think. 
Well in that case, you are wrong.\
This is a server-side tool for organizing your emails and keeping your inbox clean.
You may already use filters in your mail client to automatically sort emails into folders based on certain criteria.
This has a couple of drawbacks. Here are the two most prominent ones:
1) This approach requires you to open a mail client before the email can be sorted.
2) In the current day and age, you probably check your email on more than one device. Not mail clients on mobile devices support creating rules. It is also hard to synchronize all the rules between your different mail clients.

### Implementation
This tool is written using [MailKit](https://github.com/jstedfast/MailKit), an awesome .NET library for all email-related stuff, and .NET 5.

### Installation
Do you need to install .NET 5 to be able to run this tool? No.\
Just download the native binary for your operating system and you're good to go.\
Already have .NET 5 installed on the target machine? Even better. Just download the non-native binary and enjoy the smaller file size ;)

###Usage
This tool works by connecting to an IMAP server, going through all the messages in your inbox, checking if they match some criteria and if so, move that message to a folder.\
Here's an example usage:\
`./MailSort -h imap.gmail.com -u your@username.com -p very_secure -s -c /path/to/config/file.json`\
or for more clarity\
`./MailSort --host imap.gmail.com --username your@username.com --password very_secure --ssl --config /path/to/config/file.json`\
All of those flags arguments are mandatory, except for the -s/--ssl flag. It specifies whether or not to connect to the IMAP server using SSL (port 993) and is on by default.\
The config file is a JSON file where you define your rules. Here's an example:\
```json
[
  {
    "id": "rule1",
    "haystack": "subject",
    "matchingMethod": "equals",
    "needle": "Winner!",
    "targetFolder": "Spam",
    "combineWith": "rule2",
    "combinationMethod": "logicalAnd"
  },
  {
    "id": "rule2",
    "isCombinationRule": true,
    "haystack": "body",
    "matchingMethod": "containsIgnoreCase",
    "needle": "You have won!"
  },
  {
    "id": "rule3",
    "haystack": "body",
    "matchingMethod": "contains",
    "needle": "Trigger",
    "targetFolder": "Test"
  }
]
```
Let's go through the fields.\
id: Can be any string and can also be omitted. Only useful when wanting to refer to the rule in another rule.

haystack: The part of the email to search through. Possible values: subject, body, cc, bcc, sender, recipients.

matchingMethod: The type of check to perform. Should the subject exactly match something, or is it enough if it contains it? Possible values: contains, equals, containsIgnoreCase, equalsIgnoreCase.

needle: The phrase to search for in the haystack.

targetFolder: The folder to move the email to if it matches the criteria of the rule. Subfolders are currently not supported, but feel free to open a PR!

combineWith: Combine this rule with another rule (this is where the id comes into play). That way you can chain multiple rules to fine tune your sorting.

combinationMethod: How to logically combine the two rules. Possible values: logicalAnd, logicalOr.

isCombinationRule: Specifies that this rule is only used in combination with another rule which references this one. This means you can omit the targetFolder.

Once an email matches a rule, the email gets moved and the rest of the rules do not get evaluated, so the order of the rules may be important.
