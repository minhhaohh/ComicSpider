using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace GeographySpider.Utilities
{
    public class ASPxClientGrid
    {
        string _gridName;
        HtmlNode _document;

        public ASPxClientGrid(string gridName, HtmlNode document) 
        {
            _gridName = gridName;
            _document = document;
        }

        public List<HtmlNode> GetRootTable()
        {
            return _document.QuerySelectorAll($"#{_gridName} td").ToList();
        }

        public HtmlNode GetGridTD()
        {
            var table = GetRootTable();
            if (table == null || table.Count == 0)
                return null;
            return table.First();
        }

        public HtmlNode GetArrowDragDownImage()
        {
            return GetChildElementById("IADD");
        }

        public HtmlNode GetArrowDragUpImage()
        {
            return GetChildElementById("IADU");
        }

        public HtmlNode GetArrowDragFieldImage()
        {
            return GetChildElementById("IDHF");
        }

        public HtmlNode GetCallbackState()
        {
            return GetChildElementById("CallbackState");
        }

        public HtmlNode GetSelectionInput()
        {
            return GetChildElementById("DXSelInput");
        }

        public HtmlNode GetFocusedRowInput()
        {
            return GetChildElementById("DXFocusedRowInput");
        }

        public HtmlNode GetColResizedInput()
        {
            return GetChildElementById("DXColResizedInput");
        }

        public HtmlNode GetLoadingPanelDiv()
        {
            return GetChildElementById("LPD");
        }

        public HtmlNode GetHorzScrollDiv()
        {
            return GetChildElementById("DXHorzScrollDiv");
        }

        public HtmlNode GetFixedColumnsDiv()
        {
            return GetChildElementById("DXFixedColumnsDiv");
        }

        public HtmlNode GetPagerTop()
        {
            return GetChildElementById("DXPagerTop");
        }

        public string GetParentRowsWindow()
        {
            return _gridName + "_DXparentrowswindow";
        }

        public string GetEditorPrefix()
        {
            return _gridName + "DXEditor";
        }

        public string GetPopupEditForm()
        {
            return _gridName + "_DXPEForm";
        }

        public string GetFilterRowMenu()
        {
            return _gridName + "_DXFilterRowMenu";
        }

        public string GetFilterControlPopup()
        {
            return _gridName + "_DXPFCForm";
        }

        public string GetFilterControl()
        {
            return _gridName + "_DXPFCForm_DXPFC";
        }

        public HtmlNode GetChildElementById(string chidName)
        {
            if (string.IsNullOrEmpty(_gridName))
            {
                return _document.QuerySelector($"#{_gridName}");
            }

            return _document.QuerySelector($"#{_gridName}_{chidName}");
        }

        public string PrepareCallbackArg(string arg, HtmlNode rootTD)
        {
            var prepareArg = FormatCallbackArg("EV", GetEditorValues()) + FormatCallbackArg("SR", GetSelectedState()) + FormatCallbackArg("FR", GetFocusedRowInput()) + FormatCallbackArg("CR", GetColResizedInput()) + FormatCallbackArg("GB", arg);
            return prepareArg;
        }

        public string FormatCallbackArg(string prefix, object arg)
        {
            if (arg == null)
            {
                return string.Empty;
            }
            var value = string.Empty;
            if (arg is HtmlNode node && node != null && node.Attributes["value"] != null)
            {
                value = node.Attributes["value"].Value;
            }
            else if (arg is string str)
            {
                value = str;
            }
            return string.IsNullOrEmpty(value) ? "" : $"{prefix}|{value.Length};{value};";
        }

        public List<string> GetEditors()
        {
            var list = new List<string>();
            //for (var i = 0; i < this.editorIDList.length; i++)
            //{
            //    var editor = aspxGetControlCollection().Get(this.editorIDList[i]);
            //    if (editor && editor.GetMainElement && _aspxIsExistsElement(editor.GetMainElement()))
            //    {
            //        if (!_aspxIsExists(editor.Validate))
            //            continue;
            //        list.push(editor);
            //    }
            //}
            return list;
        }

        public string GetEditorValues()
        {
            var list = GetEditors();
            if (list == null || list.Count == 0)
                return string.Empty;
            var res = list.Count + ";";
            for (var i = 0; i < list.Count; i++)
            {
                res += GetEditorValue(list[i]);
            }
            return res;
        }

        public string GetEditorValue(string editor)
        {
            var value = editor.ToString();
            var valueLength = -1;
            if (value == null)
            {
                value = string.Empty;
            }
            else
            {
                value = value;
                valueLength = value.Length;
            }
            return GetEditorIndex(editor) + "," + valueLength + "," + value + ";";
        }

        public string GetEditorIndex(string editorId)
        {
            var i = editorId.IndexOf(GetEditorPrefix());
            if (i < 0)
                return string.Empty;
            return editorId.Substring(i + GetEditorPrefix().Length);
        }

        public string GetSelectedState()
        {
            return GetSelectionInput().InnerText;
        }

        public string GetCallbackArg(string id, ActionPage action)
        {
            string arg = string.Empty;
            switch (action)
            {
                case ActionPage.submit:
                    arg = "CUSTOMCALLBACK";
                    return arg.Length + "|" + arg + id.Length + "|" + id;
                case ActionPage.paging:
                    arg = "PAGERONCLICK";
                    return arg.Length + "|" + arg + id.Length + "|" + id;
            }
            return arg;
        }
    }

    public enum ActionPage
    {
        submit = 1,
        paging = 2,
    }
}
