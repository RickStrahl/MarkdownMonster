using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NHunspell;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HtmlAgilityPack;
using MarkdownMonster.Windows;
using Westwind.Utilities;

namespace MarkdownMonster.Utilities
{
    public class SpellChecker
    {
        public static string ExternalDictionaryFolder { get; } = Path.Combine(mmApp.Configuration.CommonFolder, "DownloadedDictionaries");

        public static readonly string InternalDictionaryFolder = Path.Combine(App.InitialStartDirectory, "Editor");

        public static Hunspell GetSpellChecker(string language = "en-US", bool reload = false)
        {
            if (reload || _spellChecker == null)
            {
                string dic = Path.Combine(InternalDictionaryFolder, language + ".dic");
                string aff = Path.ChangeExtension(dic, "aff");

                if (!File.Exists(dic))
                {
                    if (!Directory.Exists(ExternalDictionaryFolder))
                        Directory.CreateDirectory(ExternalDictionaryFolder);

                    dic = Path.Combine(ExternalDictionaryFolder, language + ".dic");
                    aff = Path.ChangeExtension(dic, "aff");
                    
                    if (!File.Exists(dic))
                    {
                        mmApp.Configuration.Editor.Dictionary = "en-US";
                        if (mmApp.Model?.Window != null)
                            mmApp.Model.Window.ShowStatusError($"Dictionary for language {language} not found. Resetting to en-US. Please reinstall your language.");

                        return GetSpellChecker("en-US", true);
                    }
                }

                _spellChecker?.Dispose();
                try
                {
                    _spellChecker = new Hunspell(aff, dic);
                }
                catch
                {
                    if (!DownloadDictionary(language))
                    {
                        if (language != "en-US")
                        {
                            mmApp.Configuration.Editor.Dictionary = "en-US";
                            return GetSpellChecker("en-US", true);
                        }
                        throw new InvalidOperationException("Language not installed. Reverting to US English.");
                    }
                }

                // Custom Dictionary if any
                string custFile = Path.Combine(ExternalDictionaryFolder, language + "_custom.txt");
                if (File.Exists(custFile))
                {
                    var lines = File.ReadAllLines(custFile);
                    foreach (var line in lines)
                    {
                        _spellChecker.Add(line);
                    }
                }
            }

            return _spellChecker;
        }
        private static Hunspell _spellChecker = null;


        /// <summary>
        /// Check spelling of an individual word
        /// </summary>
        /// <param name="text"></param>
        /// <param name="language"></param>
        /// <param name="reload"></param>
        /// <returns></returns>
        public static bool CheckSpelling(string text, string language = "en-US", bool reload = false)
        {
            var hun = GetSpellChecker(language, reload);
            if (hun == null)
                return false;

            return hun.Spell(text);
        }


        /// <summary>
        /// Shows spell check context menu options
        /// </summary>
        /// <param name="text"></param>
        /// <param name="language"></param>
        /// <param name="reload"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetSuggestions(string text, string language = "en-US", bool reload = false)
        {
            IEnumerable<string> suggestions;

            if (!string.IsNullOrEmpty(text))
            {
                var hun = GetSpellChecker(language, reload);
                suggestions = hun.Suggest(text).Take(10);
            }
            else
                suggestions = new string[] {};

            return suggestions;
        }

        /// <summary>
        /// Adds a new word to add-on the dictionary for a given locale
        /// </summary>
        /// <param name="word"></param>
        /// <param name="lang"></param>
        public static bool AddWordToDictionary(string word, string lang = "en-US")
        {
            try
            {
                if (!Directory.Exists(ExternalDictionaryFolder))
                    Directory.CreateDirectory(ExternalDictionaryFolder);

                var dictPath = Path.Combine(ExternalDictionaryFolder, lang + "_custom.txt");
                File.AppendAllText(dictPath, word + "\r\n");
                _spellChecker.Add(word);
                return true;
            }
            catch (Exception ex)
            {
                mmApp.LogLocal($"Add to Dictionary failed: {ex.Message}");
            }

            return false;
        }

        const string DictionaryDownloadUrl = "https://raw.githubusercontent.com/wooorm/dictionaries/master/dictionaries/{0}/index.dic";

        /// <summary>
        /// Downloads a dictionary file for a given language
        /// </summary>
        /// <param name="language">en-GB, it-IT, ko-KO</param>
        /// <param name="basePath">The path that dictionaries are downloaded to</param>
        /// <returns></returns>
        public static bool DownloadDictionary(string language, string basePath = null)
        {
            if (basePath == null)
                basePath = ExternalDictionaryFolder;

            try
            {
                var diItem = DictionaryDownloads.FirstOrDefault(di =>
                    language.Equals(di.Code, StringComparison.InvariantCultureIgnoreCase));
                if (diItem != null)
                {
                    string url = null;
                    if (!string.IsNullOrEmpty(diItem.CustomDownloadUrlForDic))
                        url = diItem.CustomDownloadUrlForDic;
                    else
                        url = string.Format(DictionaryDownloadUrl, diItem.Code);

                    if (!Directory.Exists(basePath))
                        Directory.CreateDirectory(basePath);

                    var dicFile = Path.Combine(basePath, language + ".dic");
                    var web = new WebClient();
                    web.DownloadFile(new Uri(url), dicFile);

                    var affFile = dicFile.Replace(".dic", ".aff");
                    url = url.Replace(".dic", ".aff");
                    web.DownloadFile(new Uri(url), affFile);

                    return File.Exists(dicFile) && File.Exists(affFile);
                }
            }
            catch(Exception ex)
            {
                mmApp.Log("Failed to download dictionary", ex, false, LogLevels.Warning);
            }

            return false;
        }

        public static bool AskForLicenseAcceptance(string language)
        {
            var form = new BrowserMessageBox()
            {
                Owner = mmApp.Model.Window
            };

            mmApp.Model.Window.ShowStatusProgress($"Downloading dictionary license for {language}");

            //download license
            var wc = new WebClient();
            wc.Encoding = Encoding.UTF8;

            var url = $"https://raw.githubusercontent.com/wooorm/dictionaries/master/dictionaries/{language}/license";
            var dd = DictionaryDownloads.FirstOrDefault(dx => dx.Code == language);
            if (!string.IsNullOrEmpty(dd?.CustomDownloadUrlForLicense))
                url = dd.CustomDownloadUrlForLicense;

            string md;
            try
            {
                md = wc.DownloadString(url);
            }
            catch
            {
                return false;
            }

            mmApp.Model.Window.ShowStatusSuccess($"Downloaded license for {language}");

            if (string.IsNullOrEmpty(md))
                return false;

            form.ShowMarkdown(md);
            form.Icon = mmApp.Model.Window.Icon;
            form.ButtonOkText.Text = "Accept";
            form.SetMessage("Please accept the license for the dictionary:");
            form.ShowDialog();

            return form.ButtonResult == form.ButtonOk;
        }


        public static string GetDictionaryListStringFromWebSite()
        {
            var url = "https://raw.githubusercontent.com/wooorm/dictionaries/master/readme.md";

            string html = HttpUtils.HttpRequestString(url);
            var data = StringUtils.ExtractString(html, "| ---- | ----------- | ------- |", "<!--support end-->").Trim();


            StringBuilder sb = new StringBuilder();

            var lines =StringUtils.GetLines(data);
            foreach (var line in lines)
            {
                var code = StringUtils.ExtractString(line, "](dictionaries/", ")");
                if (code == null || code == "en-US" || code == "fr" || code == "de" || code == "es")
                    continue;

                var lang = StringUtils.ExtractString(line, ") | ", " | ");

                sb.AppendLine("new DictionaryLanguage");
                sb.AppendLine("{");
                sb.AppendLine($"\tName = \"{lang}\", Code = \"{ code}\"");
                sb.AppendLine("},");
            }

            return sb.ToString();
        }


        public static List<DictionaryLanguage> DictionaryDownloads => new List<DictionaryLanguage>
        {
            new DictionaryLanguage
            {
                Name = "English (US)",
                Code = "en-US",
                PreInstalled = true,

            },
            new DictionaryLanguage
            {
                Name = "German",
                Code = "de",
                PreInstalled = true,

            },
            new DictionaryLanguage
            {
                Name = "French",
                Code = "fr",
                PreInstalled = true,

            },
            new DictionaryLanguage
            {
                Name = "Spanish",
                Code = "es",
                PreInstalled = true,
            },
            new DictionaryLanguage
            {
                Name = "Bulgarian",
                Code = "bg"
            },
            new DictionaryLanguage
            {
                Name = "Breton",
                Code = "br"
            },
            new DictionaryLanguage
            {
                Name = "Catalan",
                Code = "ca"
            },
            new DictionaryLanguage
            {
                Name = "Catalan (Valencian)",
                Code = "ca-valencia"
            },
            new DictionaryLanguage
            {
                Name = "Czech",
                Code = "cs"
            },
            new DictionaryLanguage
            {
                Name = "Danish",
                Code = "da"
            },
            new DictionaryLanguage
            {
                Name = "German (Austria)",
                Code = "de-AT"
            },
            new DictionaryLanguage
            {
                Name = "German (Switzerland)",
                Code = "de-CH"
            },
            new DictionaryLanguage
            {
                Name = "Modern Greek",
                Code = "el"
            },
            new DictionaryLanguage
            {
                Name = "Modern Greek (Polytonic Greek)",
                Code = "el-polyton"
            },
            new DictionaryLanguage
            {
                Name = "English (Australia)",
                Code = "en-AU"
            },
            new DictionaryLanguage
            {
                Name = "English (Canada)",
                Code = "en-CA"
            },
            new DictionaryLanguage
            {
                Name = "English (United Kingdom)",
                Code = "en-GB"
            },
            new DictionaryLanguage
            {
                Name = "English (South Africa)",
                Code = "en-ZA"
            },
            new DictionaryLanguage
            {
                Name = "Esperanto",
                Code = "eo"
            },
            new DictionaryLanguage
            {
                Name = "Estonian",
                Code = "et"
            },
            new DictionaryLanguage
            {
                Name = "Basque",
                Code = "eu"
            },
            new DictionaryLanguage
            {
                Name = "Faroese",
                Code = "fo"
            },
            new DictionaryLanguage
            {
                Name = "Friulian",
                Code = "fur"
            },
            new DictionaryLanguage
            {
                Name = "Western Frisian",
                Code = "fy"
            },
            new DictionaryLanguage
            {
                Name = "Irish",
                Code = "ga"
            },
            new DictionaryLanguage
            {
                Name = "Scottish Gaelic (or Gaelic)",
                Code = "gd"
            },
            new DictionaryLanguage
            {
                Name = "Galician",
                Code = "gl"
            },
            new DictionaryLanguage
            {
                Name = "Hebrew",
                Code = "he"
            },
            new DictionaryLanguage
            {
                Name = "Croatian",
                Code = "hr"
            },
            new DictionaryLanguage
            {
                Name = "Hungarian",
                Code = "hu"
            },
            new DictionaryLanguage
            {
                Name = "Armenian (Eastern Armenian)",
                Code = "hy-arevela"
            },
            new DictionaryLanguage
            {
                Name = "Armenian (Western Armenian)",
                Code = "hy-arevmda"
            },
            new DictionaryLanguage
            {
                Name = "Interlingua",
                Code = "ia"
            },
            new DictionaryLanguage
            {
                Name = "Interlingue (or Occidental)",
                Code = "ie"
            },
            new DictionaryLanguage
            {
                Name = "Icelandic",
                Code = "is"
            },
            new DictionaryLanguage
            {
                Name = "Italian",
                Code = "it"
            },
            new DictionaryLanguage
            {
                Name = "Korean",
                Code = "ko"
            },
            new DictionaryLanguage
            {
                Name = "Latin",
                Code = "la"
            },
            new DictionaryLanguage
            {
                Name = "Luxembourgish (or Letzeburgesch)",
                Code = "lb"
            },
            new DictionaryLanguage
            {
                Name = "Lithuanian",
                Code = "lt"
            },
            new DictionaryLanguage
            {
                Name = "Latgalian",
                Code = "ltg"
            },
            new DictionaryLanguage
            {
                Name = "Latvian",
                Code = "lv"
            },
            new DictionaryLanguage
            {
                Name = "Macedonian",
                Code = "mk"
            },
            new DictionaryLanguage
            {
                Name = "Mongolian",
                Code = "mn"
            },
            new DictionaryLanguage
            {
                Name = "Norwegian Bokmål",
                Code = "nb"
            },
            new DictionaryLanguage
            {
                Name = "Low German (or Low Saxon)",
                Code = "nds"
            },
            new DictionaryLanguage
            {
                Name = "Nepali",
                Code = "ne"
            },
            new DictionaryLanguage
            {
                Name = "Dutch (or Flemish)",
                Code = "nl"
            },
            new DictionaryLanguage
            {
                Name = "Norwegian Nynorsk",
                Code = "nn"
            },
            new DictionaryLanguage
            {
                Name = "Polish",
                Code = "pl"
            },
            new DictionaryLanguage
            {
                Name = "Portuguese",
                Code = "pt"
            },
            new DictionaryLanguage
            {
                Name = "Portuguese (Brazil)",
                Code = "pt-BR"
            },
            new DictionaryLanguage
            {
                Name = "Romanian (or Moldavian, or Moldovan)",
                Code = "ro"
            },
            new DictionaryLanguage
            {
                Name = "Russian",
                Code = "ru"
            },
            new DictionaryLanguage
            {
                Name = "Kinyarwanda",
                Code = "rw"
            },
            new DictionaryLanguage
            {
                Name = "Slovak",
                Code = "sk"
            },
            new DictionaryLanguage
            {
                Name = "Slovenian",
                Code = "sl"
            },
            new DictionaryLanguage
            {
                Name = "Serbian",
                Code = "sr"
            },
            new DictionaryLanguage
            {
                Name = "Serbian (Latin)",
                Code = "sr-Latn"
            },
            new DictionaryLanguage
            {
                Name = "Swedish",
                Code = "sv"
            },
            new DictionaryLanguage
            {
                Name = "Turkish",
                Code = "tr"
            },
            new DictionaryLanguage
            {
                Name = "Ukrainian",
                Code = "uk"
            },
            new DictionaryLanguage
            {
                Name = "Vietnamese",
                Code = "vi"
            },
            new DictionaryLanguage
            {
                Name = "Indian - Hindi",
                Code = "HI_in",
                CustomDownloadUrlForDic = "https://github.com/Shreeshrii/hindi-hunspell/raw/master/Hindi/hi_IN.dic",
                CustomDownloadUrlForLicense = "https://raw.githubusercontent.com/Shreeshrii/hindi-hunspell/master/Hindi/LICENSES-hi.txt"
            }
        };

    }

    public class DictionaryLanguage
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Url { get; set; }
        public bool PreInstalled { get; set; }
        public int SortOrder { get; set; }

        public string CustomDownloadUrlForLicense { get; set; }
        public string CustomDownloadUrlForDic { get; set; }
        public string CustomDownloadUrlForZip { get; set; }
    }


}
