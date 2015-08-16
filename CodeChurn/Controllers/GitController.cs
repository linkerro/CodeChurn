using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LibGit2Sharp;

namespace CodeChurn.Controllers
{
    public class GitController : ApiController
    {
        // GET api/values
        public HttpResponseMessage Get(string path)
        {
            using (var repo = new Repository(path))
            {
                var files = repo
                    .RetrieveStatus(new StatusOptions() { IncludeUnaltered = true })
                    .Where(s => s.State != FileStatus.Ignored && s.State != FileStatus.NewInWorkdir)
                    .Select(s => s.FilePath)
                    .ToList();
                var diffs =
                    files.Select(f => new { path = f, history = repo.Commits.QueryBy(f).Select(x => x.Commit).ToList() })
                        .ToList();

                var patches =
                    diffs.Select(
                        d =>
                            new
                            {
                                d.path,
                                history =
                                    d.history.Where(p => p.Parents.Any())
                                        .SelectMany(
                                            c =>
                                                repo.Diff.Compare<Patch>(c.Parents.First().Tree,c.Tree)
                                                    .Where(p => p.Path == d.path)
                                                    .Select(p => new { p.LinesAdded, p.LinesDeleted, c.Author.When })
                                                    .ToArray())
                                        .ToArray()
                            }).ToArray();
                return Request.CreateResponse(HttpStatusCode.OK, patches);
            }
        }
    }
}
