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
            var enkiPath = @"C:\Users\r.iscu\Documents\GitHub\enki.js";
            var beetleMailPath = @"C:\Work\BeetleMailer";

            using (var repo = new Repository(beetleMailPath))
            {
                var files = repo.Head.Tip.Tree.Select(t => t.Path).ToList();
                var test = repo.Index.ToList();
                var diffs = files.Select(f => repo.Commits).ToList();

                var commits = repo.Commits.ToList();
                var commitInfo = commits
                    .Select(c => new
                    {
                        commit = c,
                        parents = c.Parents.ToList(),
                        diffs = c.Parents.Select(p => repo.Diff.Compare<TreeChanges>(c.Tree, p.Tree)).ToList()
                    })
                    .ToList();

                var groupedInfo = commitInfo
                    .GroupBy(c => c.diffs.SelectMany(d => d.Modified))
                    .ToList();
            }
            Console.ReadKey();
        }

        private static Tree GetLastTree(Tree tree, List<Tree> trees)
        {
            return trees[trees.IndexOf(tree) - 1];
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
