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
    public const string OneProductCard = @"
<html><body>
  <div class='card'>
     <a href='/p/sku-111'><span class='title'>Sukienka letnia</span></a>
     <span class='price'>249,99 PLN</span>
     <span class='sku-inline'>IGNORED</span>
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
