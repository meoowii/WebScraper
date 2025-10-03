using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

class Product { public string Title; public string Price; }

class Program
{
    static void Main()
    {
        var list = Scrape();
        using var sw = new StreamWriter("ciuszki.csv");
        sw.WriteLine("Title,Price");
        foreach (var x in list)
        {
            sw.WriteLine($"\"{x.Title}\",\"{x.Price}\"");
        }
    }

    static List<Product> Scrape()
    {
        var res = new List<Product>();
        var opt = new ChromeOptions();
        opt.AddArgument("--headless=new");
        var drv = new ChromeDriver(opt);
        var wait = new WebDriverWait(drv, TimeSpan.FromSeconds(20));

        drv.Navigate().GoToUrl("https://fashion-freak.pl/sklep/");
        var sel = By.CssSelector("ul.products li.product");
        wait.Until(d => d.FindElements(sel).Count > 0);

        var js = (IJavaScriptExecutor)drv;
        int prev = 0, same = 0;
        long hPrev = Convert.ToInt64(js.ExecuteScript("return document.body.scrollHeight"));

        for (int i = 0; i < 400; i++)
        {
            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight); window.dispatchEvent(new Event('scroll'));");
            Thread.Sleep(1000);

            int c = drv.FindElements(sel).Count;
            long h = Convert.ToInt64(js.ExecuteScript("return document.body.scrollHeight"));

            if (c == prev && h == hPrev) same++;
            else { same = 0; prev = c; hPrev = h; }

            if (same >= 8) break;
        }

        foreach (var li in drv.FindElements(sel))
        {
            string t;
            t = li.FindElement(By.CssSelector("h2.woocommerce-loop-product__title")).Text.Trim();

            string p;
            p = li.FindElement(By.CssSelector("span.price bdi")).Text.Trim();

            res.Add(new Product { Title = t, Price = p });
        }

        drv.Quit();
        return res;
    }
}
