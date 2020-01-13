using MyScrapper.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyScrapper.Interface
{
    public interface IGenericParser
    {
        HtmlNodeDto ScrapTitle();
        HtmlNodeDto ScrapContent();
        HtmlNodeDto ScrapDescription();
        HtmlNodeDto ScrapPubDate();
        HtmlNodeDto ScrapAuthor();
        string ScrapImage();
        HtmlNodeDto ScrapSecondaryImages();
        HtmlNodeDto ScrapKeywords();
        HtmlNodeDto ScrapVideos();
    }
}
