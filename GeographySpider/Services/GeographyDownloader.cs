using Fizzler.Systems.HtmlAgilityPack;
using GeographySpider.Consts;
using GeographySpider.Models;
using GeographySpider.Utilities;
using HtmlAgilityPack;
using System.Net;

namespace GeographySpider.Services
{
    public class GeographyDownloader : IGeographyDownloader
    {
        private HttpClient _client;

        private HtmlDocument _html;

        private HtmlNode _document;

        private Dictionary<string, string> _formValues;

        private ASPxClientGrid _aspClienGrid;

        private FilterInput _filterInput;

        public async Task Initialize()
        {
            _client = new HttpClient();
            _html = new HtmlDocument();
            _formValues = new Dictionary<string, string>();
            await LoadHomePageAsync();
            GetFormValues();
        }

        public async Task<List<Province>> GetDataProvincesAsync()
        {
            var data = new List<Province>();

            _filterInput = FilterInput.Province;

            FillSearchForm();

            await SubmitFormAsync();
            var currentPage = 1;

            while (true)
            {
                data.AddRange(ExtractDataProvinces());
                var nextButton = GetNextButton();
                if (nextButton == null)
                    break;
                await LoadNextPageAsync();
                currentPage++;
            }

            return data;
        }

        public async Task<List<District>> GetDataDistrictsAsync()
        {
            var data = new List<District>();

            _filterInput = FilterInput.District;

            FillSearchForm();

            await SubmitFormAsync();
            var currentPage = 1;

            while (true)
            {
                data.AddRange(ExtractDataDistricts());
                var nextButton = GetNextButton();
                if (nextButton == null)
                    break;
                await LoadNextPageAsync();
                currentPage++;
            }

            return data;
        }

        public async Task<List<Ward>> GetDataWardsAsync()
        {
            var data = new List<Ward>();

            _filterInput = FilterInput.Ward;

            FillSearchForm();

            await SubmitFormAsync();

            var currentPage = 1;

            while (true)
            {
                data.AddRange(ExtractDataWards());
                var nextButton = GetNextButton();
                if (nextButton == null)
                    break;
                await LoadNextPageAsync();
                currentPage++;
            }

            return data;
        }

        private List<Province> ExtractDataProvinces()
        {
            var result = new List<Province>();
            var mainTable = _aspClienGrid.GetMainTable();
            if (mainTable == null)
                return result;
            var mainTableRows = mainTable.QuerySelectorAll("tr");
            foreach (var mainTableRow in mainTableRows)
            {
                var mainTableRowCells = mainTableRow.QuerySelectorAll("td").ToList();
                if (mainTableRowCells == null || mainTableRowCells.Count() != 5)
                    continue;
                var data = ExtractOneProvince(mainTableRowCells);
                if (data != null)
                    result.Add(data);
            }
            return result;
        }

        private List<District> ExtractDataDistricts()
        {
            var result = new List<District>();
            var mainTable = _aspClienGrid.GetMainTable();
            if (mainTable == null)
                return result;
            var mainTableRows = mainTable.QuerySelectorAll("tr");
            foreach (var mainTableRow in mainTableRows)
            {
                var mainTableRowCells = mainTableRow.QuerySelectorAll("td").ToList();
                if (mainTableRowCells == null || mainTableRowCells.Count() != 7)
                    continue;
                var data = ExtractOneDistrict(mainTableRowCells);
                if (data != null)
                    result.Add(data);
            }
            return result;
        }

        private List<Ward> ExtractDataWards()
        {
            var result = new List<Ward>();
            var mainTable = _aspClienGrid.GetMainTable();
            if (mainTable == null)
                return result;
            var mainTableRows = mainTable.QuerySelectorAll("tr");
            foreach (var mainTableRow in mainTableRows)
            {
                var mainTableRowCells = mainTableRow.QuerySelectorAll("td").ToList();
                if (mainTableRowCells == null || mainTableRowCells.Count() != 8)
                    continue;
                var data = ExtractOneWard(mainTableRowCells);
                if (data != null)
                    result.Add(data);
            }
            return result;
        }

        private Province ExtractOneProvince(List<HtmlNode> mainTableRowCells)
        {
            var code = WebUtility.HtmlDecode(mainTableRowCells[1].InnerText).Trim();
            var name = WebUtility.HtmlDecode(mainTableRowCells[2].InnerText).Trim();
            var levelName = WebUtility.HtmlDecode(mainTableRowCells[4].InnerText).Trim();
            return string.IsNullOrEmpty(code) ? null : new Province(code, name, levelName);
        }

        private District ExtractOneDistrict(List<HtmlNode> mainTableRowCells)
        {
            var code = WebUtility.HtmlDecode(mainTableRowCells[1].InnerText).Trim();
            var name = WebUtility.HtmlDecode(mainTableRowCells[2].InnerText).Trim();
            var levelName = WebUtility.HtmlDecode(mainTableRowCells[4].InnerText).Trim();
            var provinceCode = WebUtility.HtmlDecode(mainTableRowCells[5].InnerText).Trim();
            return string.IsNullOrEmpty(code) ? null : new District(code, name, levelName, provinceCode);
        }

        private Ward ExtractOneWard(List<HtmlNode> mainTableRowCells)
        {
            var code = WebUtility.HtmlDecode(mainTableRowCells[0].InnerText).Trim();
            var name = WebUtility.HtmlDecode(mainTableRowCells[1].InnerText).Trim();
            var levelName = WebUtility.HtmlDecode(mainTableRowCells[3].InnerText).Trim();
            var districtCode = WebUtility.HtmlDecode(mainTableRowCells[4].InnerText).Trim();
            return string.IsNullOrEmpty(code) ? null : new Ward(code, name, levelName, districtCode);
        }

        private HtmlNode GetNextButton()
        {
            var pagerTop = _aspClienGrid.GetPagerTop();
            if (pagerTop == null)
                return null;
            var nextButton = pagerTop.QuerySelector("table td:last-child");
            var nextButtonClass = nextButton.Attributes["class"].Value;
            return nextButtonClass.Contains("dxpDisabledButton_Office2003_Blue") ? null : nextButton;
        }

        private async Task LoadNextPageAsync()
        {
            var grid = _document.QuerySelector($"#{_filterInput.GridName}_CallbackState");
            var gridName = grid.Attributes["name"].Value;
            var gridValue = grid.Attributes["value"].Value;

            _formValues[gridName] = gridValue;

            var arg = _aspClienGrid.GetCallbackArg(GeographyConsts.nextButtonName, ActionPage.paging);
            var callbackParam = _aspClienGrid.PrepareCallbackArg(arg);
            _formValues["__CALLBACKPARAM"] = callbackParam;

            await SubmitFormAsync();
        }

        private async Task LoadHomePageAsync()
        {
            await SubmitFormAsync();
        }

        private async Task SubmitFormAsync()
        {
            var response = await _client.PostAsync("https://danhmuchanhchinh.gso.gov.vn/default.aspx", new FormUrlEncodedContent(_formValues));
            var responseBody = await response.Content.ReadAsStringAsync();
            GetHtmlNode(responseBody);
            _aspClienGrid = new ASPxClientGrid(_filterInput?.GridName, _document);
        }

        private void GetHtmlNode(string responseBody)
        {
            _html.LoadHtml(responseBody);
            _document = _html.DocumentNode;
        }

        private void GetFormValues()
        {
            var inputs = _document.QuerySelectorAll("input");

            foreach (var input in inputs)
            {
                var inputName = input.Attributes["name"];
                if (inputName == null) continue;

                var inputValue = input.Attributes["value"];
                _formValues[inputName.Value] = inputValue?.Value ?? "";
            }
        }

        private void FillSearchForm()
        {
            _aspClienGrid = new ASPxClientGrid(_filterInput?.GridName, _document);

            _formValues["__EVENTTARGET"] = string.Empty;
            _formValues["__EVENTARGUMENT"] = string.Empty;
            _formValues["__CALLBACKID"] = _filterInput.CallbackId;

            var arg = _aspClienGrid.GetCallbackArg(string.Empty, ActionPage.submit);
            var callbackParam = _aspClienGrid.PrepareCallbackArg(arg);
            _formValues["__CALLBACKPARAM"] = callbackParam;

            _formValues["ctl00_PlaceHolderMain_cmbCap_VI"] = _filterInput.LevelCode;
            _formValues["ctl00$PlaceHolderMain$cmbCap"] = _filterInput.LevelName;
            _formValues["ctl00_PlaceHolderMain_cmbCap_DDDWS"] = "0:0:12000:760:86:0:0:0";
            _formValues["ctl00$PlaceHolderMain$cmbCap$DDD$L"] = _filterInput.LevelCode;
        }
    }
}
