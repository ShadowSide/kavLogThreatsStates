using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace kavLogThreatsStates
{
    class Program
    {
        static int Main(string[] args)
        {
			try
            {
                if (args.Length != 1 || (args[0] is var logPath && !File.Exists(logPath)))
				{
					Console.Error.WriteLine("First argument should be log file for analyze.");
					return -1;
				}
				var lines = File.ReadLines(logPath);
				var reactive = new Regex(@"^\d\d:\d\d:\d\d\.\d\d\d\t0x\w+\t\w+\t.+\[TI: 0x(\w+)] St:(\w+) N:(.*) M:[\w ]", RegexOptions.Compiled | RegexOptions.ECMAScript);
				var Sts = new ConcurrentDictionary<string, UInt64>();
				var StChange = new ConcurrentDictionary<string, List<string>>();
				UInt64 StsCount = 0;
				foreach (var line in lines)
				{
					var r = reactive.Match(line);
					if (r.Success)
					{
						++StsCount;
						foreach (Group g in r.Groups)
							Console.WriteLine(g.Value);
						var state = r.Groups[2].Value;
						Sts.AddOrUpdate(state, 0, (k, agr) => agr + 1);
						var treat = r.Groups[1].Value;
						StChange.AddOrUpdate(treat, new List<string>(), (k, set) =>
						{
							if (/*set.Contains(state)*/Object.Equals(state, set.LastOrDefault()))
								return set;
							else
								set.Add(state);
							return set;
						});
					}
				}
				Console.WriteLine("================");
				foreach (var St in Sts)
					Console.WriteLine($"{St.Key}:{St.Value}");
				Console.WriteLine($"StsCount: {StsCount}");
				var data = StChange.GroupBy(sc => string.Join(" ", sc.Value)).ToDictionary(k => k.Key, v => string.Join(" ", v.Select(vv => vv.Key)));
				foreach (var d in data)
					Console.WriteLine($"{d.Key}: {d.Value}");
				Console.WriteLine("Ended");
			}
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
            return 0;
        }
    }
}



