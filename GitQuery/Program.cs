using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace GitQuery
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var repo = new Repository(@"C:\Users\r.iscu\Documents\GitHub\enki.js"))
            {
                var RFC2822Format = "ddd dd MMM HH:mm:ss yyyy K";

                var commits = repo.Commits.ToList();
                var commitNotes = commits.Select(c => c.GetType()).ToList();
                var trees = commits
                    .Select(c => c.Tree)
                    .ToList();
                var diffs = trees
                    .Skip(1)
                    .Select(t => repo.Diff.Compare<TreeChanges>(t, GetLastTree(t, trees)))
                    .ToList();
                var fileModifications = diffs
                    .SelectMany(d => d.Modified)
                    .GroupBy(c => c.Path)
                    .ToList();
            }
            Console.ReadKey();
        }

        private static Tree GetLastTree(Tree tree, List<Tree> trees)
        {
            return trees[trees.IndexOf(tree)-1];
        }

        private static void ListCommits(Repository repo, string rfc2822Format)
        {
            foreach (Commit c in repo.Commits)
            {
                Console.WriteLine($"commit {c.Id}");

                if (c.Parents.Count() > 1)
                {
                    Console.WriteLine("Merge: {0}",
                        string.Join(" ", c.Parents.Select(p => p.Id.Sha.Substring(0, 7)).ToArray()));
                }

                Console.WriteLine($"Author: {c.Author.Name} <{c.Author.Email}>");
                Console.WriteLine("Date:   {0}", c.Author.When.ToString(rfc2822Format, CultureInfo.InvariantCulture));
                Console.WriteLine();
                Console.WriteLine(c.Message);
                Console.WriteLine();
            }
        }
    }
}
