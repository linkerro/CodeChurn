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
            var enkiPath2 = @"C:\Users\linke\Documents\GitHub\enki.js";

            using (var repo = new Repository(enkiPath2))
            {
                var files = repo
                    .RetrieveStatus(new StatusOptions() {IncludeUnaltered = true})
                    .Where(s => s.State != FileStatus.Ignored && s.State != FileStatus.NewInWorkdir)
                    .Select(s => s.FilePath)
                    .ToList();
                var diffs =
                    files.Select(f => new {path = f, history = repo.Commits.QueryBy(f).Select(x => x.Commit).ToList()})
                        .ToList();

                var patches =
                    diffs.Select(
                        d =>
                            new
                            {
                                path = d.path,
                                history =
                                    d.history.Where(p => p.Parents.Any())
                                        .SelectMany(
                                            c =>
                                                repo.Diff.Compare<Patch>(c.Tree, c.Parents.First().Tree)
                                                    .Where(p => p.Path == d.path)
                                                    .ToArray())
                                        .ToArray()
                            }).ToArray();


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
