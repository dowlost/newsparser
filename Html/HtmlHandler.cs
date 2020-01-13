using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace MyScrapper.Html
{
    public class HtmlHandler
    {
        private string _sourceHtml { get; set; }

        #region Constructors

        public HtmlHandler() { }

        public HtmlHandler(string newsUrl)
        {
            HandleHtml(newsUrl);
        }

        #endregion

        #region Methods

        public bool HandleHtml(string newsUrl)
        {
            try
            {
                DownloadAndTreatSource(newsUrl);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        private bool DownloadAndTreatSource(string newsUrl)
        {
            if (!string.IsNullOrEmpty(newsUrl) &&
                Uri.TryCreate(newsUrl, UriKind.RelativeOrAbsolute, out Uri newsUri))
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    using (Stream stream = httpClient.GetStreamAsync(newsUri).Result)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                            this._sourceHtml = reader.ReadToEnd();
                    }
                }

                if (!string.IsNullOrEmpty(this._sourceHtml))
                    CleanHtmlSource();
                else
                    return false;

                return true;
            }
            else
                return false;


            //function to remove useless things
            void CleanHtmlSource()
            {
                string source = this._sourceHtml;

                //Removes javascript
                var regex = new Regex(@"<script[^>]*>[\s\S]*?</script>");
                source = regex.Replace(source, "");

                //removes css
                regex = new Regex(@"<style[^>]*>[\s\S]*?</style>");
                source = regex.Replace(source, "");

                // i forgot what this removes
                regex = new Regex(@"<!--[\s\S]*?-->");
                source = regex.Replace(source, "");

                source = Regex.Replace(source, @"\t|\n|\r", "");


                //triple htmlDecode because i dont understant this shit (some pages need this)
                source = WebUtility.HtmlDecode(WebUtility.HtmlDecode(WebUtility.HtmlDecode(source)));

                this._sourceHtml = source;
            }
        }

        /// <summary>
        /// If is null, you haven't instantiated the class with the "newsUrl" or called the "HandleHtml" method.
        /// </summary>
        /// <returns></returns>
        public string GetHtmlSource()
        {
            return this._sourceHtml;
        }

        #endregion
    }
}
