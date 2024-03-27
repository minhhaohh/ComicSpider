using Fizzler.Systems.HtmlAgilityPack;
using GeographySpider.Utilities;
using HtmlAgilityPack;
using System.Reflection.Metadata;
using System.Xml.Linq;

internal class Program
{
    async static Task Main(string[] args)
    {
        await LoadHomePage();

        var inputs = GetFormValues();

        FillSearchForm();

        SubmitSearchForm();

        while (true)
        {
            var data = ExtractData();
            if (!HasNextPage())
                break;
            LoadNextPage();
        }

        var provinceCallBackID = "ctl00$PlaceHolderMain$grid1";
        var dicstrictCallBackID = "ctl00$PlaceHolderMain$grid2";
        var wardCallBackID = "ctl00$PlaceHolderMain$grid3";

        var provinceGridName = "ctl00_PlaceHolderMain_grid1";
        var dicstrictGridName = "ctl00_PlaceHolderMain_grid2";
        var wardGridName = "ctl00_PlaceHolderMain_grid3";

        var client = new HttpClient();
        var response = await client.PostAsync("https://danhmuchanhchinh.gso.gov.vn/default.aspx", null);
        string responseBody = await response.Content.ReadAsStringAsync();

        var html = new HtmlDocument();
        html.LoadHtml(responseBody);
        var document = html.DocumentNode;

        var formValues = new Dictionary<string, string>();

        var inputs = document.QuerySelectorAll("input");

        foreach (var input in inputs)
        {
            var inputName = input.Attributes["name"];
            if (inputName == null) continue;
 
            var inputValue = input.Attributes["value"];
            formValues[inputName.Value] = inputValue?.Value ?? "";
        }

        formValues["__EVENTTARGET"] = string.Empty;
        formValues["__EVENTARGUMENT"] = string.Empty;
        formValues["__CALLBACKID"] = wardCallBackID;

        var aspClienGrid = new ASPxClientGrid(wardGridName, document);
        var arg = aspClienGrid.GetCallbackArg(string.Empty, ActionPage.submit);
        var rootElement = aspClienGrid.GetGridTD();
        var callbackParam = aspClienGrid.PrepareCallbackArg(arg, rootElement);
        formValues["__CALLBACKPARAM"] = callbackParam;

        formValues["ctl00_PlaceHolderMain_cmbCap_VI"] = "3";
        formValues["ctl00$PlaceHolderMain$cmbCap"] = "Xã";
        formValues["ctl00_PlaceHolderMain_cmbCap_DDDWS"] = "0:0:12000:760:86:0:0:0";
        formValues["ctl00$PlaceHolderMain$cmbCap$DDD$L"] = "3";

        response = await client.PostAsync("https://danhmuchanhchinh.gso.gov.vn/default.aspx", new FormUrlEncodedContent(formValues));
        responseBody = await response.Content.ReadAsStringAsync();

        File.WriteAllText("ward.html", responseBody);

        html.LoadHtml(responseBody);
        document = html.DocumentNode;
        aspClienGrid = new ASPxClientGrid(wardGridName, document);
        var pagerTop = aspClienGrid.GetPagerTop();
        if (pagerTop != null)
        {
            var pages = pagerTop.QuerySelectorAll("table td");
            var nextButton = pages.LastOrDefault();
        }


        var grid = document.QuerySelector("#ctl00_PlaceHolderMain_grid3_CallbackState");
        var gridName = grid.Attributes["name"].Value;
        var gridValue = grid.Attributes["value"].Value;

        formValues[gridName] = gridValue;

        arg = aspClienGrid.GetCallbackArg("PN3", ActionPage.paging);
        rootElement = aspClienGrid.GetGridTD();
        callbackParam = aspClienGrid.PrepareCallbackArg(arg, rootElement);
        formValues["__CALLBACKPARAM"] = callbackParam;

        response = await client.PostAsync("https://danhmuchanhchinh.gso.gov.vn/default.aspx", new FormUrlEncodedContent(formValues));
        responseBody = await response.Content.ReadAsStringAsync();

        File.WriteAllText("wardPage.html", responseBody);

        Console.WriteLine("Hello, World!");
    }

    private static Dictionary<string, string> GetFormValues(HtmlNode document)
    {
        var formValues = new Dictionary<string, string>();

        var inputs = document.QuerySelectorAll("input");

        foreach (var input in inputs)
        {
            var inputName = input.Attributes["name"];
            if (inputName == null) continue;

            var inputValue = input.Attributes["value"];
            formValues[inputName.Value] = inputValue?.Value ?? "";
        }

        formValues["__EVENTTARGET"] = string.Empty;
        formValues["__EVENTARGUMENT"] = string.Empty;
        formValues["__CALLBACKID"] = wardCallBackID;

        var aspClienGrid = new ASPxClientGrid(wardGridName, document);
        var arg = aspClienGrid.GetCallbackArg(string.Empty, ActionPage.submit);
        var rootElement = aspClienGrid.GetGridTD();
        var callbackParam = aspClienGrid.PrepareCallbackArg(arg, rootElement);
        formValues["__CALLBACKPARAM"] = callbackParam;

        formValues["ctl00_PlaceHolderMain_cmbCap_VI"] = "3";
        formValues["ctl00$PlaceHolderMain$cmbCap"] = "Xã";
        formValues["ctl00_PlaceHolderMain_cmbCap_DDDWS"] = "0:0:12000:760:86:0:0:0";
        formValues["ctl00$PlaceHolderMain$cmbCap$DDD$L"] = "3";

        return formValues;
    }

    private async static Task<string> LoadHomePage()
    {
        var client = new HttpClient();
        var response = await client.PostAsync("https://danhmuchanhchinh.gso.gov.vn/default.aspx", null);
        return await response.Content.ReadAsStringAsync();
    }
}
        
