using System.Text;
using EKsuNewsScrapper.Steps;
using NewsScrapper;
using NewsScrapper.Models;

namespace EKsuNewsScrapper;

class EKsuScrapper : ScrapperBase
{
    public override async Task Walk()
    {
        var rt = await new GetContentList().Step();
        var tt = await new GetContents().Step(rt.ToArray());
    }

    public static void Main()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        BaseRoutine();
    }
}