using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BadooSearcher
{
    class Program
    {
        public const string directoryName = "screens";
        public const string fileLogName = "girls.log";
        public static int counter = 0;
        public static ChromeDriver driver;
        public static int captchaCounter = 0;
        public static List<MiniProfile> listVisitedMiniProfiles = new List<MiniProfile>();
        public static string fileName = "girls.xml";
        static void Main(string[] args)
        {
            #region Primary setup

            if (File.Exists(fileLogName))
                File.Delete(fileLogName);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            else
            {
                foreach (var file in Directory.GetFiles(directoryName))
                {
                    File.Delete(file);
                }
            }

            XDocument doc;
            
            if (File.Exists(fileName))
            {
                doc = XDocument.Load(fileName);
            }
            else
            {
                doc = new XDocument();
                doc.Add(new XElement("Girls"));
                doc.Save(fileName);
            }

            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("test-type");
            options.AddArguments("--disable-extensions");

            driver = new ChromeDriver();
            GoToUrlWithGetRoundOfCaptcha("https://badoo.com", false);

            var vkButton = driver.FindElementsByClassName("auth-button--vk");
            foreach (var item in vkButton)
            {
                item.Click();
            }

            Thread.Sleep(TimeSpan.FromSeconds(20));

            var handles = driver.WindowHandles;

            var mainWinHandle = driver.CurrentWindowHandle;

            driver.SwitchTo().Window(handles[1]);

            var vkFormInputs = driver.FindElementsByClassName("oauth_form_input");

            int i = 0;

            foreach (var item in vkFormInputs)
            {
                if (i != 1)
                    item.SendKeys(ConfigurationManager.AppSettings["user"]);
                else
                    item.SendKeys(ConfigurationManager.AppSettings["password"]);
                i++;
            }

            var vkButtonOauth = driver.FindElementsById("install_allow");

            vkButtonOauth[0].Submit();

            Thread.Sleep(TimeSpan.FromSeconds(20));

            driver.SwitchTo().Window(handles[0]);

            #endregion

            #region Seconds

            int beforeSeconds = 15;
            int afterSeconds = 5;

            #endregion

            for (int cycle = 0; cycle < 50; cycle++)
            {
                File.AppendAllText(fileLogName, String.Format("Это {0} проход ", cycle + 1));
                File.AppendAllText(fileLogName, Environment.NewLine);
                for (int page = 1; page <= 100; page++)
                {
                    GoToUrlWithGetRoundOfCaptcha("https://badoo.com/search");

                    Thread.Sleep(TimeSpan.FromSeconds(10));

                    if (page == 1)
                    {
                        var dropButton = driver.FindElementByClassName($"btn--round");
                        dropButton.Click();
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        var onlineButtons = driver.FindElementsByClassName("js-search-filter");
                        onlineButtons[3].Click();
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }

                    if (page != 1)
                    {
                        var pageButton = driver.FindElementsByLinkText($"{page}");
                        if (pageButton.Count == 0)
                        {
                            for (int j = 2;; j++)
                            {
                                if (j > 100)
                                {
                                    break;
                                }
                                var pageButtonInFor = driver.FindElementsByLinkText($"{j}");
                                pageButtonInFor.Last().Click();
                                Thread.Sleep(TimeSpan.FromSeconds(8));

                                pageButton = driver.FindElementsByLinkText($"{page}");
                                if (pageButton.Count == 0)
                                    continue;
                                else
                                    break;
                            }
                        }
                        pageButton.Last().Click();
                        //var pageButton1 = driver.FindElementsByLinkText($"/search?filter=online&page={page}"); //pageButton.First().Click();
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }

                    var pageParser = new PageParser(driver);
                    var girlMiniProfiles = pageParser.Parse();

                    var girlsInXml = doc.Element("Girls").Elements().Select(node => new MiniProfile("noname", 0, node.Value));
                    listVisitedMiniProfiles.AddRange(girlsInXml.Except(listVisitedMiniProfiles, new MiniProfileComparer()));

                    var filteredProfiles = girlMiniProfiles.Where(g => g.Age >= 26 && g.Age <= 28).ToList();
                    var profilesWithoutPrevious = filteredProfiles.Except(listVisitedMiniProfiles, new MiniProfileComparer()).ToList();

                    foreach (var girlMiniProfile in profilesWithoutPrevious)
                    {
                        //driver.ExecuteScript($"window.open('{girlMiniProfile.Link}','_blank');");
                        GoToUrlWithGetRoundOfCaptcha(girlMiniProfile.Link);

                        //driver.FindElement(By.CssSelector("body")).SendKeys(OpenQA.Selenium.Keys.Control + "t");
                        //driver.SwitchTo().Window(driver.WindowHandles.Last());
                        //driver.Navigate().GoToUrl(girlMiniProfile.Link);

                        Thread.Sleep(TimeSpan.FromSeconds(beforeSeconds));

                        //if (driver.WindowHandles.Count > 1)
                        //{
                        //    driver.SwitchTo().Window(driver.WindowHandles[1]);
                        //    driver.SwitchTo().DefaultContent();
                        //}
                        //CreateScreenShot(
                        //    cycle: cycle,
                        //    page: page,
                        //    link: girlMiniProfile.Link);

                        string appereance = "";
                        string smoking = "";
                        string drinking = "";
                        string children = "";
                        string living = "";

                        bool superGirl = HtmlPageHasWord("Супер девушка");
                        bool talking = HtmlPageHasWord("Пообщаться");

                        bool pageLoaded = HtmlPageHasWord("Местоположение");

                        if (
                                (talking)
                                &&
                                FindSectionAndCheck(
                                sectionName: "Внешность:",
                                checkFunc: elem => elem.Text.Contains("см") && Convert.ToInt32(elem.Text.Replace(">","").Replace("<","").Substring(0, 3)) >= 170,
                                text: out appereance,
                                resultSectionNotFound: false)
                                &&
                                FindSectionAndCheck(
                                sectionName: "Курение:",
                                checkFunc: elem => elem.Text.Contains("Не курю") || elem.Text.Contains("Категорически против курения"),
                                text: out smoking,
                                resultSectionNotFound: false)
                                &&
                                FindSectionAndCheck(
                                sectionName: "Алкоголь:",
                                checkFunc: elem => elem.Text.Contains("Нет"),
                                text: out drinking,
                                resultSectionNotFound: true)
                                &&
                                FindSectionAndCheck(
                                sectionName: "Дети:",
                                checkFunc: elem => !elem.Text.Contains("Уже есть"),
                                text: out children,
                                resultSectionNotFound: true)
                                &&
                                FindSectionAndCheck(
                                sectionName: "Я живу:",
                                checkFunc: elem => !elem.Text.Contains("С родителями"),
                                text: out living,
                                resultSectionNotFound: true)
                            )
                        {
                            AddGirlToXml(
                                 doc,
                                 girlMiniProfile,
                                 appereance,
                                 smoking,
                                 drinking,
                                 children,
                                 living,
                                 talking,
                                 superGirl);
                        }
                        else
                        {
                            if (pageLoaded)
                            {
                                AddGirlToXml(
                                    doc: doc,
                                    girlMiniProfile: girlMiniProfile,
                                    appereance: FindSection("Внешность:"),
                                    smoking: FindSection("Курение:"),
                                    drinking: FindSection("Алкоголь:"),
                                    children: FindSection("Дети:"),
                                    living: FindSection("Я живу:"),
                                    talking: talking,
                                    superGirl: superGirl,
                                    description: "noCriteria");
                            }
                        }

                        if (driver.WindowHandles.Count > 1)
                        {
                            //driver.SwitchTo().Window(driver.WindowHandles[1]).Close();
                            //Thread.Sleep(TimeSpan.FromSeconds(2));
                            //driver.SwitchTo().Window(driver.WindowHandles[0]);
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(afterSeconds));
                    }
                    listVisitedMiniProfiles.AddRange(filteredProfiles);
                    File.AppendAllText(fileLogName, String.Format("{0} страница проанализирована ", page));
                    File.AppendAllText(fileLogName, Environment.NewLine);
                    //Thread.Sleep(TimeSpan.FromMinutes(25));

                    
                }
            }

            driver.Quit();
        }

        public static void CreateScreenShot(int cycle, int page, string link)
        {
            Graphics graph = null;

            var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            graph = Graphics.FromImage(bmp);

            graph.CopyFromScreen(0, 0, 0, 0, bmp.Size);

            File.AppendAllText(fileLogName, String.Format("{0}_{1}_{2} добавляю скриншот ", cycle, page, link));
            File.AppendAllText(fileLogName, Environment.NewLine);

            bmp.Save(Path.Combine(directoryName, String.Format("{0}_{1}_{2}.png", cycle, page, counter)));
            counter++;
        }

        public static bool FindSectionAndCheck(string sectionName, Func<IWebElement, bool> checkFunc, out string text, bool resultSectionNotFound = false)
        {
            IWebElement sectionInfo;
            try
            {
                sectionInfo = driver.FindElementByXPath(String.Format("//*[contains(text(), '{0}')]", sectionName));
            }
            catch (NoSuchElementException)
            {
                text = "";
                return resultSectionNotFound;
            }

            var parent = sectionInfo.FindElement(By.XPath(".."));
            var sibling = parent.FindElement(By.XPath("following-sibling::*[1]"));
            text = sibling.Text;
            return checkFunc(sibling);
        }

        public static string FindSection(string sectionName)
        {
            IWebElement sectionInfo;
            try
            {
                sectionInfo = driver.FindElementByXPath(String.Format("//*[contains(text(), '{0}')]", sectionName));
                var parent = sectionInfo.FindElement(By.XPath(".."));
                var sibling = parent.FindElement(By.XPath("following-sibling::*[1]"));
                return sibling.Text;
            }
            catch (NoSuchElementException)
            {
                return "";
            }
        }

        public static bool HtmlPageHasWord(string word)
        {
            try
            {
                driver.FindElementByXPath(String.Format("//*[contains(text(), '{0}')]", word));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public static bool CaptchaShown()
        {
            return !HtmlPageHasWord("Местоположение");
        }

        public static void GoToUrlWithGetRoundOfCaptcha(string url, bool shouldCheckCaptcha = true)
        {
            driver.Navigate().GoToUrl(url);
            //if (shouldCheckCaptcha && CaptchaShown())
            //{
            //    File.AppendAllText(fileLogName, "Ждем 1 час, т.к. капча показалась");
            //    File.AppendAllText(fileLogName, Environment.NewLine);
            //    Thread.Sleep(TimeSpan.FromHours(1));
            //}
            //File.AppendAllText(fileLogName, string.Format("shouldCheckCaptch is {0}", shouldCheckCaptcha));
            //File.AppendAllText(fileLogName, Environment.NewLine);
            //File.AppendAllText(fileLogName, "Страница без капчи");
            //File.AppendAllText(fileLogName, Environment.NewLine);
        }

        public static bool FindAge(out int age)
        {
            try
            {
                age = Convert.ToInt32(driver.FindElementByClassName("profile-header__age").Text.Replace(",", "").Replace(" ", ""));
                return true;
            }
            catch (NoSuchElementException)
            {
                age = 0;
                return false;
            }
        }

        public static void AddGirlToXml(
            XDocument doc,
            MiniProfile girlMiniProfile,
            string appereance,
            string smoking,
            string drinking,
            string children,
            string living,
            bool talking,
            bool superGirl,
            string description = ""
            )
        {
            doc.Root.Add(new XElement("link", girlMiniProfile.Link,
                                   new XAttribute("appereance", appereance),
                                   new XAttribute("name", girlMiniProfile.Name),
                                   new XAttribute("smoking", smoking),
                                   new XAttribute("drinking", drinking),
                                   new XAttribute("children", children),
                                   new XAttribute("age", girlMiniProfile.Age),
                                   new XAttribute("supergirl", superGirl),
                                   new XAttribute("living", living),
                                   new XAttribute("talking", talking),
                                   new XAttribute("description", description),
                                   new XAttribute("dateToAdd", DateTime.Now)));
            var allElements = doc.Element("Girls").Elements().OrderByDescending(elem => elem.Attribute("appereance").Value).ToList();
            doc.Element("Girls").Elements().Remove();
            foreach (var elem in allElements.ToList())
            {
                doc.Element("Girls").Add(elem);
            }
            doc.Save(fileName);
            File.AppendAllText(fileLogName, String.Format("Добавлена {0} девушка ", girlMiniProfile.Link));
            File.AppendAllText(fileLogName, Environment.NewLine);
        }
    }
}
