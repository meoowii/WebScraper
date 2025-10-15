namespace WebScraper.Tests.TestAssets.Html;

public static class SampleHtml
{
    // Strona startowa sklepu odzieżowego z dwiema kategoriami
    public const string StartPageWithTwoCategories = @"
<html><body>
  <a class='category-item' href='/sukienki'>Sukienki</a>
  <a class='category-item' href='/spodnie'>Spodnie</a>
</body></html>";

    // Jedna karta produktu z ceną 
    public static string OneProductCard(string sku = "sku-111", string title = "Sukienka letnia", string price = "249,99 PLN")
        => $@"
<html><body>
  <div class='card'>
     <a href='/p/{sku}'><span class='title'>{title}</span></a>
     <span class='price'>{price}</span>
     <span class='sku-inline'>{sku}</span>
  </div>
</body></html>";

    // Dwie identyczne karty produktu (duplikat po SKU)
    public const string TwoCardsWithDuplicateSku = @"
<html><body>
  <div class='card'>
     <a href='/p/sku-1'><span class='title'>Koszulka basic</span></a>
     <span class='price'>79,99 PLN</span>
  </div>
  <div class='card'>
     <a href='/p/sku-1'><span class='title'>Koszulka basic</span></a>
     <span class='price'>79,99 PLN</span>
  </div>
</body></html>";
}