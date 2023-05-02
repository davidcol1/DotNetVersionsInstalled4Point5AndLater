using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    List<string> versionKeys = new List<string> { "v4\\Full", "v4\\Client", "v3.5", "v3.0", "v2.0.50727" };
    versionKeys.Sort();
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
    Console.WriteLine("\n.NET Core/.NET 5+ versions:");

    try
    {
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

        Regex regex = new Regex(@"(\d+\.\d+\.\d+) \[(.+)\]", RegexOptions.Compiled);

        Console.WriteLine("SDKs:");
        foreach (Match match in regex.Matches(output))
        {
          string version = match.Groups[1].Value;
          string path = match.Groups[2].Value;

          Console.WriteLine($"Version: {version}");
          Console.WriteLine($"Path: {path}");

          // Display target frameworks
          string targetFrameworksPath = Path.Combine(path, "Microsoft.NET.Sdk", "supportedTargetFrameworks");
          if (Directory.Exists(targetFrameworksPath))
          {
            string[] targetFrameworks = Directory.GetFiles(targetFrameworksPath, "*.txt");
            Console.Write("Target Frameworks: ");
            foreach (string targetFrameworkFile in targetFrameworks)
            {
              string targetFramework = Path.GetFileNameWithoutExtension(targetFrameworkFile);
              Console.Write($"{targetFramework} ");
            }
            Console.WriteLine();
          }

          Console.WriteLine();
        }
      }

      startInfo.Arguments = "--list-runtimes";

      using (Process process = new Process { StartInfo = startInfo })
      {
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Regex regex = new Regex(@"Microsoft\.(NETCore|NET)\.App (\d+\.\d+\.\d+) \[(.+)\]", RegexOptions.Compiled);

        Console.WriteLine("Runtimes:");
        foreach (Match match in regex.Matches(output))
        {
          string version = match.Groups[2].Value;
          string path = match.Groups[3].Value;

          Console.WriteLine($"Version: {version}");
          Console.WriteLine($"Path: {path}");
          Console.WriteLine();
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error detecting .NET Core/.NET 5+ installations: " + ex.Message);
    }
  }
}
