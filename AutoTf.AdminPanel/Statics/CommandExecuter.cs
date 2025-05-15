using System.Diagnostics;

namespace AutoTf.AdminPanel.Statics;

public static class CommandExecuter
{
	public static string ExecuteCommand(string command)
	{
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "/bin/bash",
				Arguments = $"-c \"{command}\"",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};

		process.Start();
		string result = process.StandardOutput.ReadToEnd();
		process.WaitForExit();

		return result.Trim();
	}

	public static void ExecuteSilent(string command, bool ignoreExceptions)
	{
		try
		{
			Process process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "/bin/bash",
					Arguments = $"-c \"{command}\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};
			process.Start();
			process.WaitForExit();

			string error = process.StandardError.ReadToEnd();

			if (!string.IsNullOrEmpty(error) && !command.Contains("which"))
			{
				throw new Exception($"Error: {error}");
			}
            
		}
		catch (Exception e)
		{
			if(ignoreExceptions)
				Console.Write("Ignored exception:");
			
			Console.WriteLine(e.ToString());
			if (!ignoreExceptions)
				throw;
		}
	}
}