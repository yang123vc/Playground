//css_ref System.IO.Compression
//css_ref System.IO.Compression.FileSystem
//css_ref Newtonsoft.Json
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Collections.Specialized;
using JSON = Newtonsoft.Json.JsonConvert;

static class Script
{
	static readonly string CWD = Environment.CurrentDirectory + '\\';
	static readonly string FONTNAME = "trello";

	[STAThread]
	static public void Main(string[] args)
	{
		Console.WriteLine("Creating Fontello session..");
		string json = File.ReadAllText("config.json");
		string sessionID = CreateSession(json);

		Console.WriteLine("Done: " + sessionID);
		Console.WriteLine();
		Process.Start("http://fontello.com/" + sessionID);

		while(true)
		{
			Console.WriteLine("Press ENTER to download and unpack your font from the session (remember to 'SAVE SESSION')..");
			Console.ReadLine();

			DownloadUnpackFont(sessionID);
		}
	}

	static public string CreateSession(string json)
	{
		var obj = JSON.DeserializeObject(json);
		json = JSON.SerializeObject(obj);// reduces request size

		using(HttpClient client = new HttpClient())
		{
			var content1 = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
			content1.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
			content1.Headers.Add("Content-Disposition", "form-data; name=\"config\"; filename=\"config.json\"");

			MultipartFormDataContent content2 = new MultipartFormDataContent();
			content2.Add(content1, "config");

			string q = content2.ReadAsStringAsync().Result;
			q = HttpUtility.UrlDecode(q);


			HttpResponseMessage response = client.PostAsync("http://fontello.com/", content2).Result;
			response.EnsureSuccessStatusCode();
			return response.Content.ReadAsStringAsync().Result;
		}
	}

	static public void DownloadUnpackFont(string sessionID)
	{
		byte[] zipbin;

		using(HttpClient client = new HttpClient())
		{
			zipbin = client.GetByteArrayAsync("http://fontello.com/" + sessionID + "/get").Result;
		}

		using(ZipArchive zip = new ZipArchive(new MemoryStream(zipbin)))
		{
			string rootdir = zip.Entries[0].FullName;
			zip.GetEntry(rootdir + "config.json").ExtractToFile("config.json", true);
			zip.GetEntry(rootdir + "font/" + FONTNAME + ".ttf").ExtractToFile(FONTNAME + ".ttf", true);
			zip.GetEntry(rootdir + "css/" + FONTNAME + "-codes.css").ExtractToFile(FONTNAME + "-codes.css", true);
		}

		// adjust CSS file
		{
			string css = File.ReadAllText(FONTNAME + "-codes.css", Encoding.UTF8);
			css = css.Replace(":before", "");
			css = Regex.Replace(css, "/[*].+?[*]/", "");
			css = css.RemoveWhitespace();
			File.WriteAllText(FONTNAME + "-codes.css", css);
		}
	}

	public static string RemoveWhitespace(this string input)
	{
		return new string(input.ToCharArray()
			.Where(c => !Char.IsWhiteSpace(c))
			.ToArray());
	}

	static public void SpawnProcess(string exe, string args, string msg = "", bool ignore_error = false)
	{
		var startInfo = new ProcessStartInfo(exe, args)
		{
			FileName = exe,
			Arguments = args,
			UseShellExecute = false,
			WorkingDirectory = CWD
		};

		var p = Process.Start(startInfo);
		p.WaitForExit();

		if(p.ExitCode != 0 && ignore_error == false)
		{
			Console.ForegroundColor = ConsoleColor.Red;

			Console.WriteLine();
			Console.WriteLine("-------------------------");
			Console.WriteLine("FAILED: " + msg);
			Console.WriteLine("EXIT CODE: " + p.ExitCode);
			Console.WriteLine("Press ENTER to exit");
			Console.WriteLine("-------------------------");

			Console.ReadLine();
			Environment.Exit(0);
		}
	}
}