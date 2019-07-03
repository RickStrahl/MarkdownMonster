using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Westwind.Utilities;

namespace MarkdownMonster.Utilities
{
    public class SearchInFilesFinder
    {
        public string Path { get; set; }
        public string SearchFilePattern { get; set; }

        public string SearchPhrase { get; set; }

        public string ReplacePhrase { get; set; }

        public bool NoMatchDetails { get; set; }

        public bool SearchSubFolders { get; set; } = true;

        private byte[] SearchPhraseBytes { get; set;  }

        public SearchInFilesFinder(string path, string searchFilePattern, params string[] extensions)
        {
            Path = path;
            SearchFilePattern = searchFilePattern;
            if (searchFilePattern == null)
                SearchFilePattern = "*.*";
            
        }


        public Task<List<SearchFileResult>> SearchFilesAsync(string searchPhrase = null,
            SearchTypes searchType = SearchTypes.SearchCaseInsensitiveText)
        {
            return Task.Run( ()=> SearchFiles(searchPhrase, searchType) );
        }


        public List<SearchFileResult> SearchFiles(string searchPhrase = null,
            SearchTypes searchType = SearchTypes.SearchCaseInsensitiveText)
        {
            if (searchPhrase != null)
                SearchPhrase = searchPhrase;

            SearchPhraseBytes = Encoding.UTF8.GetBytes(searchPhrase);

            var list = new List<SearchFileResult>();

            SearchFolder(list, Path);

            return list;
        }


        private void SearchFolder(List<SearchFileResult> list, string folder)
        {
            var filters = SearchFilePattern.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var filter in filters)
            {
                string[] files;
                try
                {
                    files = Directory.GetFiles(folder, filter, SearchOption.TopDirectoryOnly);
                }
                catch
                {
                    continue;
                }

                foreach (var file in files)
                {
                    var result = new SearchFileResult {Filename = file};
                    if (SearchFile(result) > 0 ||
                        System.IO.Path.GetFileName(file)
                            .Contains(SearchPhrase, StringComparison.InvariantCultureIgnoreCase))
                        list.Add(result);
                    
                }
            }

            if (!SearchSubFolders)
                return;

            try
            {
                var folders = Directory.GetDirectories(folder, "*.*");
                foreach (var childFolder in folders)
                    SearchFolder(list, childFolder);
            }
            catch { }

        }

        private int SearchFile(SearchFileResult result)
        {
            var text = File.ReadAllText(result.Filename);

            int matches = 0;
            int i = 0;
            while (true)
            {
                var pos = text.IndexOfNth(SearchPhrase,i + 1,StringComparison.InvariantCultureIgnoreCase);
                i++;
                if (pos == -1)
                    break;

                if (NoMatchDetails)
                    return 1;

                matches++;

                var match = new SearchTextMatch();
                match.StartPos = pos;
                match.EndPos = match.StartPos + SearchPhrase.Length;

                result.Matches.Add(match);
            }

            return matches;
        }

    }


    public class SearchFileResult
    {
        
        public string Filename { get; set; }

        public string FilePath => Path.GetDirectoryName(Filename);

        public string FileOnly => Path.GetFileName(Filename);


        public List<SearchTextMatch> Matches
        {
            get
            {
                if (_matches == null)
                    _matches = new List<SearchTextMatch>();
                return _matches;
            }
            set => _matches = value;
        }
        private List<SearchTextMatch> _matches;
    }

    public class SearchTextMatch {
        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public string MatchedLines  { get; set; }
    }

    public enum SearchTypes
    {
        SearchExactText,
        SearchCaseInsensitiveText,
        SearchRegEx
    }

}
