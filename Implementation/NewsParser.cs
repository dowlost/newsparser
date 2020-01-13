using HtmlAgilityPack;
using MyScrapper.DTO;
using MyScrapper.Html;
using MyScrapper.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace MyScrapper.Scrapper
{
    public class NewsParser : IGenericParser
    {
        private HtmlHandler _htmlHandler { get; set; }
        private HtmlDocument _htmlDocument { get; set; }
        private string _newsUrl { get; set; }
        private Uri _newsUri { get; set; }
        private HtmlNode _titleNode { get; set; }        

        #region Constructors

        public NewsParser(string newsUrl)
        {
            _newsUrl = newsUrl;
            _htmlHandler = new HtmlHandler(newsUrl);

            _newsUri = new Uri(newsUrl);

            _htmlDocument = new HtmlDocument();
            _htmlDocument.LoadHtml(_htmlHandler.GetHtmlSource());

            CleanHtml();
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Gets all available news data.
        /// </summary>
        /// <returns></returns>
        public News ScrapFullData()
        {
            var title = ScrapTitle();
            var desc = ScrapDescription();
            var content = ScrapContent();
            var img = ScrapImage();

            return new News()
            {
                Title = title.NodeText,
                Description = desc.NodeText,
                Content = desc.NodeText,
                MainImage = img
            };
        }

        public HtmlNodeDto ScrapTitle()
        {
            var titleNode = new HtmlNodeDto();

            titleNode.Node = _htmlDocument.DocumentNode.Descendants().Where(x => (x.Name == "title" && x.InnerText.Count(c => !Char.IsWhiteSpace(c)) > 25)).FirstOrDefault();
            if (titleNode.Node == null || string.IsNullOrWhiteSpace(titleNode.Node.InnerText))
            {
                titleNode.Node = _htmlDocument.DocumentNode.Descendants().Where(x => (x.Name == "meta" && x.Attributes["name"] != null && x.Attributes["name"].Value.Contains("tit"))).FirstOrDefault();

                if (titleNode.Node == null || string.IsNullOrWhiteSpace(titleNode.Node.Attributes["content"].Value))
                {
                    titleNode.Node = _htmlDocument.DocumentNode.Descendants().Where(x => (x.Name == "meta" && x.Attributes["property"] != null && x.Attributes["property"].Value.Contains("tit"))).FirstOrDefault();

                    if (titleNode == null || string.IsNullOrWhiteSpace(titleNode.Node.Attributes["content"].Value))
                    {
                        titleNode.Node = _htmlDocument.DocumentNode.Descendants().Where(x => (x.Name == "h1" && x.InnerText.Count(c => !Char.IsWhiteSpace(c)) > 25)).FirstOrDefault();
                    }
                }
            }

            //Pega o InnerText ou Content do titulo e formata o texto
            if (titleNode.Node != null)
            {
                if (titleNode.Node.Name == "meta")
                    titleNode.NodeText = Regex.Replace(titleNode.Node.Attributes["content"].Value, @"\t|\n|\r", "").Trim();
                else
                    titleNode.NodeText = Regex.Replace(titleNode.Node.InnerText, @"\t|\n|\r", "").Trim();

                _titleNode = titleNode.Node;
            }

            return titleNode;
        }

        /// <summary>
        /// TO DO --
        /// </summary>
        /// <returns></returns>
        public HtmlNodeDto ScrapContent()
        {
            var contentNode = new HtmlNodeDto();
            var body = _htmlDocument.DocumentNode.Descendants().Where(x => (x.Name == "section" || x.Name == "div" || x.Name == "main") &&
                                    x.Attributes["id"] != null &&
                                    (x.Attributes["id"].Value.ToLower().Equals("content") ||
                                    x.Attributes["id"].Value.ToLower().Equals("body") ||
                                    x.Attributes["id"].Value.ToLower().Equals("main") ||
                                    x.Attributes["id"].Value.ToLower().Equals("primary"))).FirstOrDefault();

            if (body != null)
                contentNode.Node = body.Descendants().Where(x => ((x.Name == "div" || x.Name == "p") && x.InnerText.Count(c => !Char.IsWhiteSpace(c)) > 50)).FirstOrDefault();

            if (contentNode.Node == null)
                contentNode.Node = _htmlDocument.DocumentNode.Descendants().Where(x => (x.Name == "p" && x.InnerText.Count(c => !Char.IsWhiteSpace(c)) > 50)).FirstOrDefault();
            
            //Pega o InnerText ou Content da descrição e formata o texto
            if (contentNode.Node != null)
            {
                contentNode.NodeText = Regex.Replace(contentNode.Node.InnerText, @"\t|\n|\r", "").Trim();

                if (contentNode.NodeText.Contains("&ccedil;") || contentNode.NodeText.Contains("&atilde;"))
                    contentNode.NodeText = WebUtility.HtmlDecode(contentNode.NodeText);
            }

            return contentNode;
        }

        /// <summary>
        /// Looks for a short description for the news.  
        /// </summary>
        /// <returns></returns>
        public HtmlNodeDto ScrapDescription()
        {
            var descNode = new HtmlNodeDto();

            descNode.Node = _htmlDocument.DocumentNode.Descendants().Where(x => (x.Name == "meta" && x.Attributes["name"] != null && x.Attributes["name"].Value.Contains("descr"))).FirstOrDefault();

            if (descNode.Node == null || descNode.Node.Attributes["content"] == null || string.IsNullOrWhiteSpace(descNode.Node.Attributes["content"].Value))
            {
                descNode.Node = _htmlDocument.DocumentNode.Descendants().Where(x => (x.Name == "meta" && x.Attributes["property"] != null && x.Attributes["property"].Value.Contains("descr"))).FirstOrDefault();

                if (descNode.Node == null || string.IsNullOrWhiteSpace(descNode.Node.Attributes["content"].Value))
                {
                    var body = _htmlDocument.DocumentNode.Descendants().Where(x => (x.Name == "section" || x.Name == "div" || x.Name == "main") &&
                                            x.Attributes["id"] != null &&
                                            (x.Attributes["id"].Value.ToLower().Equals("content") ||
                                            x.Attributes["id"].Value.ToLower().Equals("body") ||
                                            x.Attributes["id"].Value.ToLower().Equals("main") ||
                                            x.Attributes["id"].Value.ToLower().Equals("primary"))).FirstOrDefault();

                    if (body != null)
                        descNode.Node = body.Descendants().Where(x => ((x.Name == "div" || x.Name == "p") && x.InnerText.Count(c => !Char.IsWhiteSpace(c)) > 50)).FirstOrDefault();

                    if (descNode.Node == null)
                        descNode.Node = _htmlDocument.DocumentNode.Descendants().Where(x => (x.Name == "p" && x.InnerText.Count(c => !Char.IsWhiteSpace(c)) > 50)).FirstOrDefault();
                }
            }

            //Pega o InnerText ou Content da descrição e formata o texto
            if (descNode.Node != null)
            {
                if (descNode.Node.Name == "meta")
                    descNode.NodeText = Regex.Replace(descNode.Node.Attributes["content"].Value, @"\t|\n|\r", "").Trim();
                else
                    descNode.NodeText = Regex.Replace(descNode.Node.InnerText, @"\t|\n|\r", "").Trim();

                if (descNode.NodeText.Contains("&ccedil;") || descNode.NodeText.Contains("&atilde;"))
                    descNode.NodeText = WebUtility.HtmlDecode(descNode.NodeText);
            }

            return descNode;
        }

        /// <summary>
        /// TO DO
        /// </summary>
        /// <returns></returns>
        public HtmlNodeDto ScrapPubDate()
        {
            return null;
        }

        /// <summary>
        /// TO DO
        /// </summary>
        /// <returns></returns>
        public HtmlNodeDto ScrapAuthor()
        {
            return null;
        }

        /// <summary>
        /// Looks for the main news image url.
        /// </summary>
        /// <returns></returns>
        public string ScrapImage()
        {
            var imageNodes = _htmlDocument.DocumentNode.Descendants().Where(n => n.Attributes.Any(a => a.Value.Contains("og:image")))
                            ?? _htmlDocument.DocumentNode.Descendants().Where(n => n.Attributes.Any(a => a.Value.Contains("twitter:image:src")));

            if (imageNodes != null && imageNodes.Count() > 0)
                return (imageNodes.First()).GetAttributeValue("content", "");

            return null;
        }

        /// <summary>
        /// TO DO
        /// </summary>
        /// <returns></returns>
        public HtmlNodeDto ScrapSecondaryImages()
        {
            return null;
        }

        /// <summary>
        /// TO DO
        /// </summary>
        /// <returns></returns>
        public HtmlNodeDto ScrapKeywords()
        {
            return null;
        }

        /// <summary>
        /// TO DO
        /// </summary>
        /// <returns></returns>
        public HtmlNodeDto ScrapVideos()
        {
            return null;
        }

        #endregion

        #region Helper
        
        /// <summary>
        /// Returns the next node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private HtmlNode GetNextNode(HtmlNode node)
        {
            if (node.HasChildNodes)
            {
                return node.ChildNodes.First();
            }
            else if (node.NextSibling != null)
            {
                return node.NextSibling;
            }
            else
            {
                if (node.ParentNode != null)
                {
                    node = node.ParentNode;
                    while (node != null)
                    {
                        if (node.NextSibling != null)
                            return node.NextSibling;
                        else
                            node = node.ParentNode;
                    }
                    return node;
                }
                else return null;
            }
        }

        private void CleanHtml()
        {
            var iframes = _htmlDocument.DocumentNode.SelectNodes("//iframe");
            if (iframes != null)
                foreach (HtmlNode a in iframes)
                    a.RemoveAllChildren();
        }

        #endregion
    }
}