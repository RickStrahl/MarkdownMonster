using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Westwind.Utilities;

namespace MarkdownMonster.AddIns
{
    /// <summary>
    /// This class manages loading of addins and 
    /// raising various application events passed
    /// to all addins that they can respond to
    /// </summary>
    public class AddinManager
    {
        /// <summary>
        /// Singleton to get access to Addin Manager
        /// </summary>
        public static AddinManager Current { get; set; }

        
        public string ErrorMessage { get; set; }

        static AddinManager()
        {
            Current = new AddinManager();
        }

        public AddinManager()
        {
        
        }        

        public List<AddinItem> GetAddinList()
        {
            const string addinListRepoUrl =
                "https://raw.githubusercontent.com/RickStrahl/MarkdownMonsterAddinsRegistry/master/MarkdownMonsterAddinRegistry.json";

            var settings = new HttpRequestSettings
            {
                Url = addinListRepoUrl,
                Timeout = 5000
            };

            List<AddinItem> addinList;
            try
            {
                addinList = HttpUtils.JsonRequest<List<AddinItem>>(settings);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
                return null;
            }

            addinList
                .AsParallel()
                .ForAll(ai =>
                {
                    try
                    {
                        var dl = HttpUtils.JsonRequest<AddinItem>(new HttpRequestSettings
                        {
                            Url = ai.gitVersionUrl
                        });
                        DataUtils.CopyObjectData(dl, ai, "id,name,gitVersionUrl,gitUrl");

                        if (Directory.Exists(".\\Addins\\" + ai.id) ||
                            Directory.Exists(".\\Addins\\Installs\\" + ai.id))
                            ai.isInstalled = true;

                        if (File.Exists(".\\Addins\\Installs\\" + ai.id + ".delete"))
                            ai.isInstalled = false;
                    }
                    catch { /* ignore error */}
                });



            return addinList;
        }

        public async Task<List<AddinItem>> GetInitialAddinListAsync()
        {
            const string addinListRepoUrl =
                "https://raw.githubusercontent.com/RickStrahl/MarkdownMonsterAddinsRegistry/master/MarkdownMonsterAddinRegistry.json";

            var settings = new HttpRequestSettings
            {
                Url = addinListRepoUrl,
                Timeout = 5000
            };

            List<AddinItem> addinList;
            try
            {
                addinList = await HttpUtils.JsonRequestAsync<List<AddinItem>>(settings);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return null;
            }

            return addinList;
        }

        public async Task<List<AddinItem>> GetAddinListAsync(List<AddinItem> addinList = null)
        {
      
            if (addinList == null)
                addinList = await GetInitialAddinListAsync();

            if (addinList == null)
                return null;
      
            //foreach (var ai in addinList)

            Parallel.ForEach(addinList,
                new ParallelOptions {MaxDegreeOfParallelism = 20},
                ai =>                
                    {
                        try
                        {                  
                            // not using async here so we can wait for final list result          
                            // before returning
                            var dl = HttpUtils.JsonRequest<AddinItem>(new HttpRequestSettings
                            {
                                Url = ai.gitVersionUrl
                            });
                            DataUtils.CopyObjectData(dl, ai, "id,name,gitVersionUrl,gitUrl");
                           
                        }
                        catch (Exception ex)
                        {
                            //mmApp.Log($"Addin {ai.name} version failed", ex);
                        }
                    });
            

            return addinList
                .Where(ai => ai.updated > new DateTime(2016, 1, 1))
                .OrderBy(ai => ai.isInstalled ? 0 : 1)  
                .ThenByDescending(ai => ai.updated)
                .ToList();
        }

     
    }
}
