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
        public string SearchPattern { get; set; }
        public string[] Extensions { get; set; }

        public string SearchPhrase { get; set; }

        private byte[] SearchPhraseBytes { get; set;  }

        public SearchInFilesFinder(string path, string searchPattern, params string[] extensions)
        {
            Path = path;
            SearchPattern = searchPattern;
            if (searchPattern == null)
                SearchPattern = "*.*";
            if (extensions == null || extensions.Length == 0)
                extensions = new string[] {".md", ".markdown", ".txt"};
            Extensions = extensions;

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
            var files = Directory.GetFiles(folder, SearchPattern);
            foreach (var file in files)
            {
                var ext = System.IO.Path.GetExtension(file);
                if (!StringUtils.Inlist(ext, Extensions))
                    continue;

                var result = new SearchFileResult {Filename = file};
                if (SearchFile(result)  > 0 ||
                    file.Contains(SearchPhrase, StringComparison.InvariantCultureIgnoreCase))
                    list.Add(result);
            }

            var dirs = Directory.GetDirectories(folder, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var dir in dirs)
                SearchFolder(list, dir);
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
        public string MatchedLine  { get; set; }
    }

    public enum SearchTypes
    {
        SearchExactText,
        SearchCaseInsensitiveText,
        SearchRegEx
    }

}
