using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace PMMPGuiApp.Data {
    internal class PoggitData {

        private List<PoggitListData> poggitList = new();

        private int maxValue;


        public void setList() {
            var url = DownloadString(@"https://poggit.pmmp.io/plugins.min.json");
            JArray jsondata = JArray.Parse(url);

            List<int> list = new();

            System.Diagnostics.Debug.Print(jsondata.Count.ToString());

            for (int i = 0; i < jsondata.Count; i++) {
                if (!list.Contains(int.Parse(jsondata[i]["repo_id"].ToString()))) {
                    PoggitListData pd = new PoggitListData();
                    pd.Id = int.Parse(jsondata[i]["id"].ToString());
                    pd.Name = jsondata[i]["name"].ToString();
                    pd.Url = jsondata[i]["artifact_url"].ToString() + "/" + pd.Name + ".phar";
                    pd.Image = jsondata[i]["icon_url"].ToString();
                    pd.Tagline = jsondata[i]["tagline"].ToString();
                    pd.Download = jsondata[i]["downloads"].ToString();
                    pd.RepositoryId = int.Parse(jsondata[i]["repo_id"].ToString());
                    poggitList.Add(pd);
                    list.Add(pd.RepositoryId);
                }
            };
            maxValue = list.Count / 10;
            if(maxValue % 10 != 0) {
                maxValue++;
            }

            GC.Collect();
        }

        private string DownloadString(string url) {
            using (WebClient webClient = new WebClient()) {
                webClient.Encoding = System.Text.Encoding.UTF8;
                return webClient.DownloadString(url).Replace("\n", "");

            }
        }

        public List<PoggitListData> getPoggitDataInPage(int page) {
            List<PoggitListData> pd = new();
            int max = page * 10 + 9;
            for (int i = page * 10; i < max; i++) {
                if (poggitList[i] == null) break;
                pd.Add(poggitList[i]);
            }
            return pd;
        }


        public List<PoggitListData> GetPoggitListDatas() {
            return poggitList;
        }

        public int getMax() {
            return maxValue;
        }

        public void Disponse() {
            poggitList = null;
            GC.Collect();
        }


    }

    internal class PoggitListData {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string Tagline { get; set; }

        public string Download { get; set; }

        public string Image { get; set; }

        public int RepositoryId { get; set; }
    }
}
