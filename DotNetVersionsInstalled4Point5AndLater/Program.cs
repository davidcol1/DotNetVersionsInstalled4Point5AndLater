using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;

class Program
{
  static void Main()
  {
    GetDotNetFrameworkVersions();
    GetDotNetCoreAndNewerVersions();
    Console.ReadLine();
  }

  static void GetDotNetFrameworkVersions()
  {
    string[] versionKeys = new string[] { "v4\\Full", "v4\\Client", "v3.5", "v3.0", "v2.0.50727" };
    string registryPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\";

    Console.WriteLine("Installed .NET Framework versions:");

    foreach (string versionKey in versionKeys)
    {
      using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(registryPath + versionKey))
      {
        if (ndpKey != null)
        {
          object version = ndpKey.GetValue("Version");
          object sp = ndpKey.GetValue("SP");
          object install = ndpKey.GetValue("Install");

          if (version != null && Convert.ToInt32(install) == 1)
          {
            Console.WriteLine($"{versionKey} - Version: {version} - Service Pack: {sp ?? 0}");
          }
        }
      }
    }
  }

  static void GetDotNetCoreAndNewerVersions()
  {
    Console.WriteLine("\nLatest .NET Core/.NET 5+ versions:");

    try
    {
      Dictionary<int, Version> latestSdks = new Dictionary<int, Version>();
      Dictionary<int, Version> latestRuntimes = new Dictionary<int, Version>();

      ProcessStartInfo startInfo = new ProcessStartInfo
      {
        FileName = "dotnet",
        Arguments = "--list-sdks",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
      };

      using (Process process = new Process { StartInfo = startInfo })
      {
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Regex regex = new Regex(@"(\d+\.\d+\.\d+)", RegexOptions.Compiled);

        foreach (Match match in regex.Matches(output))
        {
          Version currentVersion = new Version(match.Value);
          int majorVersion = currentVersion.Major;

          if (!latestSdks.ContainsKey(majorVersion) || currentVersion > latestSdks[majorVersion])
          {
            latestSdks[majorVersion] = currentVersion;
          }
        }
      }

      startInfo.Arguments = "--list-runtimes";

      using (Process process = new Process { StartInfo = startInfo })
      {
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Regex regex = new Regex(@"Microsoft\.NETCore\.App (\d+\.\d+\.\d+)", RegexOptions.Compiled);

        foreach (Match match in regex.Matches(output))
        {
          Version currentVersion = new Version(match.Groups[1].Value);
          int majorVersion = currentVersion.Major;

          if (!latestRuntimes.ContainsKey(majorVersion) || currentVersion > latestRuntimes[majorVersion])
          {
            latestRuntimes[majorVersion] = currentVersion;
          }
        }
      }

      foreach (var sdk in latestSdks)
      {
        Console.WriteLine($"SDK - Version: {sdk.Value}");
      }

      foreach (var runtime in latestRuntimes)
      {
        Console.WriteLine($"Runtime - Version: {runtime.Value}");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error detecting .NET Core/.NET 5+ installations: " + ex.Message);
    }
  }
}
