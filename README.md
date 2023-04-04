# MailSort
"Is this another sorting algorithm?", you might think. 
Well in that case, you are wrong.\
This is a server-side tool for organizing your emails and keeping your inbox clean.
You may already use filters in your mail client to automatically sort emails into folders based on certain criteria.
This has a couple of drawbacks. Here are the two most prominent ones:
1) This approach requires you to open a mail client before the email can be sorted.
2) In the current day and age, you probably check your email on more than one device. Not mail clients on mobile devices support creating rules. It is also hard to synchronize all the rules between your different mail clients.

### Implementation
This tool uses [MailKit](https://github.com/jstedfast/MailKit), an awesome .NET library for all email-related stuff, and .NET 6.

### Installation
Do you need to install .NET 6 to be able to run this tool? No.\
Just download the native binary for your operating system and you're good to go.\
Already have .NET 6 installed on the target machine? Even better. Just download the non-native binary and enjoy the smaller file size ;)

### Usage
This tool works by connecting to an IMAP server, going through all the messages in your inbox, checking if they match some criteria and if so, move that message to a folder.\
The most common usage will probably be to run this tool via a cronjob at certain intervals.\
Here's an example usage:\
`./MailSort -h imap.gmail.com -u your@username.com -p very_secure -c /path/to/config/file.json`\
or for more clarity\
`./MailSort --host imap.gmail.com --username your@username.com --password very_secure --config /path/to/config/file.json`\
These are the mandatory arguments. There are a few more, run `./MailSort --help` to get an overview of all the options.
Please make sure you have the correct credentials to authenticate with the IMAP server. For example, Gmail users will no be able to authenticate with their Google password, they will need to generate and supply an application-specific password.

The config file is a JSON file where you define your rules. Here's an example:
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
`id`: Can be any string and can also be omitted. Only useful when wanting to refer to the rule in another rule.

`haystack`: The part of the email to search through. Possible values: `subject`, `body`, `cc`, `bcc`, `sender`, `recipients`, `recipientsAndCc`, `recipientsAndBcc`, `ccAndBcc`, `recipientsAndCcAndBcc`, `date` (yyyy-MM-dd). When using a haystack which combines two attributes (e.g. recipientsAndCc), they will be chained together using a comma and a following space and then checked against your `needle`. Thus you should probably only use these methods with the `contains` or `containsIgnoreCase` `matchingMethod`.\
Fields like `sender` are sometimes represented in the typical IMAP format ("Name" <email@example.com>), so you might want to use the `contains` `matchingMethod` with these emails.

`matchingMethod`: The type of check to perform. Should the subject exactly match something, or is it enough if it contains it? Possible values: `contains`, `equals`, `containsIgnoreCase`, `equalsIgnoreCase`, `greaterThanOrEqual`. `greaterThanOrEqual` can currently only be used in combination with the needle `date`.

`needle`: The phrase to search for in the `haystack`.

`targetFolder`: The folder to move the email to if it matches the criteria of the rule. Subfolders are currently not supported, but feel free to open a PR!

`combineWith`: Combine this rule with another rule (this is where the `id` comes into play). That way you can chain multiple rules to fine tune your sorting.

`combinationMethod`: How to logically combine the two rules. Possible values: `logicalAnd`, `logicalOr`.

`isCombinationRule`: Specifies that this rule is only used in combination with another rule which references this one. This means you can omit the `targetFolder`.

Once an email matches a rule, the email gets moved and the rest of the rules do not get evaluated, so the order of the rules may be important.

### License Information
This project uses open source software, each subject to their own license.

#### MailKit
```
MIT License

Copyright (C) 2013-2020 .NET Foundation and Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
```
