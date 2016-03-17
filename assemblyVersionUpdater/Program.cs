﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assemblyVersionUpdater
{
  class Program
  {
    private static int incParamNum = 0;

    private static string fileName = "";

    private static string versionStr = null;

    private static bool isVB = false;

    static void Main(string[] args)
    {
      for (int i = 0; i < args.Length; i++)
      {
        if (args[i].StartsWith("-inc:"))
        {
          string s = args[i].Substring("-inc:".Length);
          incParamNum = int.Parse(s);
        }
        else if (args[i].StartsWith("-set:"))
        {
          versionStr = args[i].Substring("-set:".Length);
        }
        else
          fileName = args[i];
      }

      if (Path.GetExtension(fileName).ToLower() == ".vb")
        isVB = true;

      if (fileName == "")
      {
        Console.WriteLine("Usage: AssemblyInfoUtil <path to AssemblyInfo.cs or AssemblyInfo.vb file> [options]");
        Console.WriteLine("Options: ");
        Console.WriteLine("  -set:<new version number> - set new version number (in NN.NN.NN.NN format)");
        Console.WriteLine("  -inc:<parameter index>  - increases the parameter with specified index (can be from 1 to 4)");
        return;
      }

      if (!File.Exists(fileName))
      {
        Console.WriteLine("Error: Can not find file \"" + fileName + "\"");
        return;
      }

      Console.Write("Processing \"" + fileName + "\"...");
      StreamReader reader = new StreamReader(fileName);
      StreamWriter writer = new StreamWriter(fileName + ".out");
      String line;

      while ((line = reader.ReadLine()) != null)
      {
        line = ProcessLine(line);
        writer.WriteLine(line);
      }
      reader.Close();
      writer.Close();

      File.Delete(fileName);
      File.Move(fileName + ".out", fileName);
      Console.WriteLine("Done!");
    }

    private static string ProcessLine(string line)
    {
      if (isVB)
      {
        line = ProcessLinePart(line, "<Assembly: AssemblyVersion(\"");
        line = ProcessLinePart(line, "<Assembly: AssemblyFileVersion(\"");
      }
      else
      {
        line = ProcessLinePart(line, "[assembly: AssemblyVersion(\"");
        line = ProcessLinePart(line, "[assembly: AssemblyFileVersion(\"");
      }
      return line;
    }

    private static string ProcessLinePart(string line, string part)
    {
      int spos = line.IndexOf(part);
      if (spos >= 0)
      {
        spos += part.Length;
        int epos = line.IndexOf('"', spos);
        string oldVersion = line.Substring(spos, epos - spos);
        string newVersion = "";
        bool performChange = false;

        if (incParamNum > 0)
        {
          string[] nums = oldVersion.Split('.');
          if (nums.Length >= incParamNum && nums[incParamNum - 1] != "*")
          {
            Int64 val = Int64.Parse(nums[incParamNum - 1]);
            val++;
            nums[incParamNum - 1] = val.ToString();
            newVersion = nums[0];
            for (int i = 1; i < nums.Length; i++)
            {
              newVersion += "." + nums[i];
            }
            performChange = true;
          }

        }
        else if (versionStr != null)
        {
          newVersion = versionStr;
          performChange = true;
        }

        if (performChange)
        {
          StringBuilder str = new StringBuilder(line);
          str.Remove(spos, epos - spos);
          str.Insert(spos, newVersion);
          line = str.ToString();
        }
      }
      return line;
    }

  }
}
