using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BadooSearcher
{
    class PageParser
    {
        private ChromeDriver _driver;
        private List<MiniProfile> listMiniProfiles = new List<MiniProfile>();

        public PageParser(ChromeDriver driver)
        {
            this._driver = driver;
        }

        public List<MiniProfile> Parse()
        {
            var hrefElements = _driver.FindElementsByClassName("js-folders-user-profile-link");
            foreach (var hrefElement in hrefElements)
            {
                var ageAndNameElement = hrefElement.FindElement(By.XPath("following-sibling::*[1]/div[1]/div[1]"));
                var fullLink = hrefElement.GetAttribute("href");
                listMiniProfiles.Add(new MiniProfile(
                    name: ageAndNameElement.Text.Substring(0, ageAndNameElement.Text.IndexOf(",")),
                    age: Convert.ToInt32(ageAndNameElement.Text.Substring(ageAndNameElement.Text.IndexOf(",") + 2, 2)),
                    link: fullLink.Substring(0, fullLink.IndexOf("?"))));
            }
            return listMiniProfiles;
        }
    }
}
