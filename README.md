# ChatPlusServer

The relay program for [ChatPlusClient](https://github.com/ahron33/ChatPlusClient). It is a small Windows app (C#, WinForms, .NET Framework) that watches the `Users/` folders on a web host and copies each new message into the shared chat file that all clients read.

Clients never write to the shared chat themselves. If they did, two people sending at the same moment could overwrite each other's messages. With this relay as the **only writer** of `Chat/chat.txt`, messages are appended one at a time, in order.

## What it does

The server runs a loop while it is switched on:

1. Lists the user folders under `Users/` over FTP ([FluentFTP](https://github.com/robinrodricks/FluentFTP)).
2. For each user folder, reads `status.txt` over HTTP.
3. If the status is `Pending`:
   1. downloads the current `Chat/chat.txt`,
   2. downloads that user's `chat.txt` (the new message),
   3. uploads `Chat/chat.txt` again with the new message added on a new line,
   4. sets that user's `status.txt` to `MessageSent`, which tells the client the message went through.
4. Waits a moment and repeats.

If a connection drops or a request fails, it disconnects, waits about 3 seconds, and reconnects. A `NOOP` command is sent at the end of each pass to keep the FTP connection alive.

## Using the app

| Control | What it does |
| --- | --- |
| **Start / Stop button** (`button3`) | Its text colour is the state: **red** = stopped, **green** = running. Click to switch. After you stop it, the loop ends at the next pass and shows "Chat stopped." |
| **Clean chat button** (`button1`) | Overwrites `Chat/chat.txt` with a single separator line (`----ChatPlus----`), clearing the history for everyone. |

## Folder layout on the host

```
ChatPlus/
├── Chat/
│   └── chat.txt              the shared chat (written only by this server)
└── Users/
    └── <machine-id>/
        ├── nickname.txt
        ├── chat.txt          the message the user just sent
        └── status.txt        "Pending" -> "MessageSent"
```

The `Users/` folders are created by the client. The server only reads and updates them.

## Requirements

- Windows
- Visual Studio with the .NET desktop development workload (the project references `System.Management`)
- [FluentFTP](https://www.nuget.org/packages/FluentFTP) 55.1.0
- The same web host and FTP account the clients use

## Getting started

1. Clone the repository:
   ```
   git clone https://github.com/ahron33/ChatPlusServer.git
   ```
2. Open the solution in Visual Studio and restore the NuGet packages.
3. Set your own values for these fields at the top of `Form1.cs`:

   | Field | Meaning | Example |
   | --- | --- | --- |
   | `url` | FTP host | `"ftp.example.com"` |
   | `user` | FTP username | `"YOUR_FTP_USER"` |
   | `pass` | FTP password | `"YOUR_FTP_PASSWORD"` |
   | `path` | FTP path to the `ChatPlus/` folder (keep the trailing `/`) | `"/domains/example.com/public_html/ChatPlus/"` |
   | `urlhttp` | public web address of the `ChatPlus/` folder (keep the trailing `/`) | `"https://example.com/ChatPlus/"` |
