using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

using var client = new HttpClient();

try
{
    var url = "https://www.bashopera.ru/affiche/";
    string htmlContent = await client.GetStringAsync(url);
    Console.WriteLine(htmlContent);
}
catch (HttpRequestException e)
{
    Console.WriteLine($"Ошибка получения страницы: {e.Message}");
}