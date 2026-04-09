using HandyControl.Controls;
using ModelsToJson.Extension;
using ModelsToJson.Models;
using Newtonsoft.Json;
using NPOI.HSSF.Record.Chart;
using NPOI.HSSF.UserModel;
using NPOI.OpenXmlFormats.Vml;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ModelsToJson.ViewModels
{
    public class ModelToJsonViewModel : BindableBase
    {

        private string excelFilePath;

        public string ExcelFilePath { get => excelFilePath; set => SetProperty(ref excelFilePath, value); }

        private string baseJsonPath;

        public string BaseJsonPath { get => baseJsonPath; set => SetProperty(ref baseJsonPath, value); }

        private string jsonFileName;

        public string JsonFileName { get => jsonFileName; set => SetProperty(ref jsonFileName, value); }

        private int ignoreCsvHeadRows;

        public int IgnoreCsvHeadRows
        {
            get { return ignoreCsvHeadRows; }
            set { ignoreCsvHeadRows = value; RaisePropertyChanged(); }
        }

        private int ignoreCsvLeftColumns;

        public int IgnoreCsvLeftColumns
        {
            get { return ignoreCsvLeftColumns; }
            set { ignoreCsvLeftColumns = value; RaisePropertyChanged(); }
        }

        private int correctedValue;

        public int CorrectedValue
        {
            get { return correctedValue; }
            set { correctedValue = value; RaisePropertyChanged(); }
        }

        private bool isMergeSheetJson;

        public bool IsMergeSheetJson
        {
            get { return isMergeSheetJson; }
            set { isMergeSheetJson = value; RaisePropertyChanged(); }
        }

        private EncodingRule encodRule;

        public EncodingRule EncodRule
        {
            get { return encodRule; }
            set { encodRule = value; RaisePropertyChanged(); }
        }

        private string selectedClassName;

        private ObservableCollection<string> modelNames;

        public ObservableCollection<string> ModelNames
        {
            get { return modelNames; }
            set { modelNames = value; RaisePropertyChanged(); }
        }


        private ObservableCollection<string> headers;

        public ObservableCollection<string> Headers
        {
            get { return headers; }
            set { headers = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ModelPropertyHead> modelProperties;

        public ObservableCollection<ModelPropertyHead> ModelProperties
        {
            get { return modelProperties; }
            set { modelProperties = value; RaisePropertyChanged(); }
        }


        private ObservableCollection<IgnoreRowContentIfColumnEmpry> ignoreRowContentIfColumnEmpries;

        public ObservableCollection<IgnoreRowContentIfColumnEmpry> IgnoreRowContentIfColumnEmpries
        {
            get { return ignoreRowContentIfColumnEmpries; }
            set { ignoreRowContentIfColumnEmpries = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<XlsFileModel> xlsFileModels;

        public ObservableCollection<XlsFileModel> XlsFileModels
        {
            get { return xlsFileModels; }
            set { xlsFileModels = value; RaisePropertyChanged(); }
        }

        
        private readonly Dictionary<string, int> _headCorreIndexes = [];

       
        private readonly Dictionary<string, string> propertyCorreHead = [];

        private List<string[]> _contentPatams = [];

        public DelegateCommand<string> GetModelPropertiesCommand { get; set; }

        public DelegateCommand LoadExcelCommand { get; set; }

        public DelegateCommand GetHeadersCommand { get; set; }

        public DelegateCommand GenerateJsonCommand { get; set; }


        public ModelToJsonViewModel()
        {
            ignoreCsvHeadRows = 0;
            ignoreCsvLeftColumns = 0;
            isMergeSheetJson = false;
            encodRule = EncodingRule.GB2312;
            baseJsonPath = "C:\\Users\\23539\\Desktop\\";
            excelFilePath = "C:\\Users\\23539\\Desktop\\Roll A2_For_Parameter_Rev1.xls";
            selectedClassName = string.Empty;
            modelNames = [];
            headers = [];
            modelProperties = [];
            ignoreRowContentIfColumnEmpries = [];
            xlsFileModels = [];
            _headCorreIndexes = [];
            propertyCorreHead = [];
            _contentPatams = [];
            GetModelPropertiesCommand = new DelegateCommand<string>(OnGetModelProoerties);
            LoadExcelCommand = new DelegateCommand(OnLoadExcelFile);
            GetHeadersCommand = new DelegateCommand(OnGetHeaders);
            GenerateJsonCommand = new DelegateCommand(OnGenerateJson);
            var path = GetCurrentModelDirecory();
            GetModelNames(path);
        }

        private void OnGenerateJson()
        {
            if (string.IsNullOrEmpty(jsonFileName)) { Growl.Error("Json文件名不能为空"); return; }
            if (string.IsNullOrEmpty(baseJsonPath)) { Growl.Error("基础路径不能为空"); return; }
            if (!Directory.Exists(baseJsonPath))
            {
                Growl.Error($"{baseJsonPath}路径不存在！");
                return;
            }
            if (excelFilePath.EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase))
            {
                if (_headCorreIndexes.Count == 0)
                {
                    Growl.Error("请先读取Csv文件！");
                    return;
                }
            }
            if (excelFilePath.EndsWith(".xls", StringComparison.CurrentCultureIgnoreCase) || excelFilePath.EndsWith(".xlsx", StringComparison.CurrentCultureIgnoreCase))
            {
                if (xlsFileModels.Count == 0)
                {
                    Growl.Error("请先加载xls or xlsx文件！");
                    return;
                }
                if (Headers.Count == 0)
                {
                    Growl.Error("请先读取文件表头！");
                    return;
                }
                if (!xlsFileModels.Any(x => x.IsSelected))
                {
                    Growl.Error("请先选中需要被转换为json的sheet表格！");
                    return;
                }
            }
            GetPropertyCorreHead();
            if (propertyCorreHead.Where(x => !string.IsNullOrEmpty(x.Value)).Count() != modelProperties.Count)
            {
                Growl.Error("请先选中对应的标签！");
                return;
            }
            Task.Run(() =>
            {
                var jsonPath = Path.Combine(BaseJsonPath, $"{JsonFileName}.json");
                if (excelFilePath.EndsWith(".csv"))
                {
                    bool success = GenerateCsvJson(_contentPatams, jsonPath);
                    Growl.Success($"Json文件生成成功！路径：{jsonPath}");
                    if (success) Growl.Success($"Json文件生成成功！路径：{jsonPath}");
                    else Growl.Success($"Json文件生成失败！路径：{jsonPath}");
                }
                if (excelFilePath.EndsWith(".xlsx") || excelFilePath.EndsWith(".xls"))
                {
                    if (isMergeSheetJson)
                    {
                        var objectModels = new ObservableCollection<object>();
                        for (int i = 0; i < XlsFileModels.Count; i++)
                        {
                            if (!XlsFileModels[i].IsSelected) continue;
                            ISheet sheet = XlsFileModels[i].Sheet;
                            var contentPatams = GetSheetContent(sheet);
                            var TempObjectModels = GenerateXslxJson(contentPatams, sheet.SheetName);
                            if (TempObjectModels == null)
                            {
                                Growl.Success($"Json文件生成失败！路径：{jsonPath}");
                                return;
                            }
                            objectModels.AddRange(TempObjectModels);
                        }
                        var jsonStr = JsonConvert.SerializeObject(objectModels, Formatting.Indented);
                        using var fileStream = new FileStream(jsonPath, FileMode.Create, FileAccess.Write);
                        using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                        streamWriter.Write(jsonStr);
                        Growl.Success($"Json文件生成成功！路径：{jsonPath}");
                    }
                    else
                    {
                        var selecteadXls = new ObservableCollection<XlsFileModel>(XlsFileModels.Where(x => x.IsSelected));
                        for (int i = 0; i < selecteadXls.Count; i++)
                        {
                            string partJsonPath = Path.Combine(BaseJsonPath, $"{JsonFileName}-{i}.json");
                            ISheet sheet = selecteadXls[i].Sheet;
                            var contentPatams = GetSheetContent(sheet);
                            var TempObjectModels = GenerateXslxJson(contentPatams, sheet.SheetName);
                            if (TempObjectModels == null) { Growl.Success($"Json文件生成失败！路径：{jsonPath}"); return; }
                            var jsonStr = JsonConvert.SerializeObject(TempObjectModels, Formatting.Indented);
                            using var fileStream = new FileStream(partJsonPath, FileMode.Create, FileAccess.Write);
                            using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                            streamWriter.Write(jsonStr);
                            Growl.Success($"Json文件生成成功！路径：{partJsonPath}");
                        }
                    }
                }
            });
        }

        private void OnLoadExcelFile()
        {
            if (!File.Exists(excelFilePath))
            {
                Growl.Error($"{excelFilePath} is not exist!");
                return;
            }
            if (excelFilePath.EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase))
            {
                _contentPatams = ReadCSV();
            }
            else if (excelFilePath.EndsWith(".xls", StringComparison.CurrentCultureIgnoreCase) || excelFilePath.EndsWith(".xlsx", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {
                    ReadXls();
                }
                catch (Exception ex)
                {
                    Growl.Error($"文件加载失败，{ex.Message}.");
                }               
            }
            else
            {
                Growl.Error($"加载失败，{excelFilePath} 既不是csv文件也不是xls、xlsx文件！");
            }
        }

        private void OnGetHeaders()
        {
            //读取表头
            if (xlsFileModels.Count == 0)
            {
                Growl.Error("请先加载xls or xlsx文件！");
                return;
            }
            if (!xlsFileModels.Any(x => x.IsSelected))
            {
                Growl.Error("请先选中需要被转换为json的sheet表格！");
                return;
            }
            ISheet sheet = xlsFileModels.First(X => X.IsSelected).Sheet;
            for (int r = 0; r < sheet.LastRowNum; r++)
            {
                if (r == ignoreCsvHeadRows)
                {
                    IRow cells = sheet.GetRow(r);
                    string[] rowContent = new string[cells.LastCellNum + 1];
                    for (int i = 0; i <= cells.LastCellNum; i++)
                    {
                        ICell cell = cells.GetCell(i);
                        string str = GetCellValue(cell);
                        rowContent[i] = str;
                    }
                    string[] headerLine = [.. rowContent.Skip(ignoreCsvLeftColumns)];
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        headers.Clear();
                        Headers.Add("None");
                        foreach (var item in headerLine) { Headers.Add(item); }
                    });
                    break;
                }
            }
            Growl.Success("表头读取完成！");
        }

        private void OnGetModelProoerties(string className)
        {
            if (string.IsNullOrEmpty(className)) return;
            selectedClassName = className;
            modelProperties.Clear();
            ignoreRowContentIfColumnEmpries.Clear();
            var types = Assembly.GetExecutingAssembly().GetTypes();
            var type = types.FirstOrDefault(x => x.Name == className);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<IgnoreAttribute>();
                    if (attribute != null && attribute.IsIngore) continue;

                    var model = new ModelPropertyHead()
                    {
                        PropertyName = property.Name,
                        SelectedHead = "",
                        Prefix = "",
                        Suffix = "",
                        Separator = "",
                        PrefixIsVisble = property.PropertyType == typeof(string),
                        SuffixIsVisble = property.PropertyType == typeof(string),
                        SeparatorIsVisble = typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string),
                    };
                    ModelProperties.Add(model);
                    var ignore = new IgnoreRowContentIfColumnEmpry()
                    {
                        PropertyName = property.Name,
                        Selected = false
                    };
                    ignoreRowContentIfColumnEmpries.Add(ignore);
                }
            });
        }

        private bool GenerateCsvJson(List<string[]> currentContent, string jsonPath)
        {
            bool success = false;
            if (currentContent == null || currentContent.Count == 0) return success;
            if (string.IsNullOrEmpty(jsonPath)) return success;
            try
            {
                ObservableCollection<object> configModels = GetJsonModels(currentContent, "");
                var jsonStr = JsonConvert.SerializeObject(configModels, Formatting.Indented);
                using var fileStream = new FileStream(jsonPath, FileMode.Create, FileAccess.Write);
                using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                streamWriter.Write(jsonStr);
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        private ObservableCollection<object> GenerateXslxJson(List<string[]> currentContent, string sheetName)
        {
            if (currentContent == null || currentContent.Count == 0) return null;
            try
            {
                ObservableCollection<object> configModels = GetJsonModels(currentContent, sheetName);

                return configModels;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return null;
            }
        }

        private List<string[]> ReadCSV()
        {
            Encoding encoding = Encoding.GetEncoding(GetEncodingString());
            char separator = ',';
            using FileStream fileStream = new(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var stream = new StreamReader(fileStream, encoding);
            List<string> readContents = [];
            List<string[]> contentPatams = [];
            while (true)
            {
                var line = stream.ReadLine();
                if (string.IsNullOrEmpty(line)) break;
                int indexSub = GetIndexOfBySpecifiedNumber(line, separator, ignoreCsvLeftColumns);
                var lineSub = line[(indexSub + 1)..];
                readContents.Add(lineSub);
            }
            string[] allContents = new string[readContents.Count - IgnoreCsvHeadRows];
            Array.Copy(readContents.ToArray(), IgnoreCsvHeadRows, allContents, 0, readContents.Count - IgnoreCsvHeadRows);

            contentPatams.Clear();
            _headCorreIndexes.Clear();
            //获取头部对照字典
            string first = allContents[0];
            var headerLine = allContents.FirstOrDefault().Split(separator, StringSplitOptions.TrimEntries);
            for (int i = 0; i < headerLine.Length; i++)
            {
                _headCorreIndexes.TryAdd(headerLine[i], i);
            }
            //获取表格内容(不包含表头)
            for (int i = 1; i < allContents.Length; i++)
            {
                var contentLine = allContents[i].Split(separator, StringSplitOptions.TrimEntries);
                contentPatams.Add(contentLine);
            }
            //获取表头信息反馈到界面
            Application.Current.Dispatcher.Invoke(() =>
            {
                headers.Clear();
                Headers.Add("None");
                foreach (var item in headerLine) { Headers.Add(item); }
            });
            Growl.Success("Csv文件读取成功！");
            return contentPatams;
        }

        private void ReadXls()
        {
            //读取sheet
            xlsFileModels.Clear();
            using var fileStream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //IWorkbook workbook = excelFilePath.EndsWith(".xls", StringComparison.CurrentCultureIgnoreCase) ? new HSSFWorkbook(fileStream) : new XSSFWorkbook(fileStream);
            IWorkbook workbook = WorkbookFactory.Create(fileStream);
            for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
            {
                XlsFileModel model = new()
                {
                    Sheet = workbook.GetSheetAt(sheetIndex),
                    IsSelected = false,
                    SheetIndex = sheetIndex,
                    SheetName = workbook.GetSheetName(sheetIndex)
                };
                xlsFileModels.Add(model);
            }
        }

        private List<string[]> GetSheetContent(ISheet sheet)
        {
            if (sheet == null) return null;
            var originAllContents = new List<string[]>();//原始内容
            var allContents = new List<string[]>();//被忽略掉指定个数的行和列的内容
            var notHeadContents = new List<string[]>();//不含表头的内容
            int maxCellNum = 0;
            for (int r = 0; r <= sheet.LastRowNum; r++)
            {
                if (sheet.GetRow(r).LastCellNum > maxCellNum)
                {
                    maxCellNum = sheet.GetRow(r).LastCellNum;
                }
            }
            for (int r = 0; r <= sheet.LastRowNum; r++)
            {
                if (r == 76)
                {

                }
                IRow cells = sheet.GetRow(r);
                if (cells == null) continue;
                string[] rowContent = new string[maxCellNum + 1];
                for (int i = 0; i <= cells.LastCellNum; i++)
                {
                    ICell cell = cells.GetCell(i);
                    string str = GetCellValue(cell);
                    rowContent[i] = str;
                }

                originAllContents.Add([.. rowContent]);
            }
            for (int i = 0; i < originAllContents.Count; i++)
            {
                if (i < ignoreCsvHeadRows)
                {
                    continue;
                }                
                var rowContent = originAllContents[i];
                string[] subRow = new string[rowContent.Length - ignoreCsvLeftColumns];
                Array.Copy(rowContent, ignoreCsvLeftColumns, subRow, 0, rowContent.Length - ignoreCsvLeftColumns);
                allContents.Add(subRow);
            }

            //获取属性->头部标签对照字典
            _headCorreIndexes.Clear();
            var headerLine = allContents[0];
            for (int i = 0; i < headerLine.Length; i++)
            {
                _headCorreIndexes.TryAdd(headerLine[i], i);
            }
            //获取内容不包含表头
            for (int i = 1; i < allContents.Count; i++)
            {
                notHeadContents.Add(allContents[i]);
            }
            return notHeadContents;
        }

        private ObservableCollection<object> GetJsonModels(List<string[]> currentContent, string sheetName)
        {
            ObservableCollection<object> configModels = [];
            var ignoreConditions = ignoreRowContentIfColumnEmpries.Where(x => x.Selected).ToList();
            Type type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => x.Name == selectedClassName);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            try
            {
                for (int i = 0; i < currentContent.Count; i++)
                {
                    var data = currentContent[i];
                    if (data.Length != _headCorreIndexes.Count)//必须完全对齐，否则容易错位
                    {
                        Growl.Error($"第{i + 1}行的数据长度{data.Length}与对照组长度{_headCorreIndexes.Count}不一致。Sheet名：{sheetName}");
                        throw new InvalidOperationException($"第{i + 1}行的数据长度{data.Length}与对照组长度{_headCorreIndexes.Count}不一致。");                        
                    }
                    bool isContinue = false;
                    foreach (var item in ignoreConditions)
                    {
                        if (string.IsNullOrEmpty(data[_headCorreIndexes[propertyCorreHead[item.PropertyName]]]))
                        {
                            isContinue = true;
                            break;
                        }
                    }
                    if (isContinue) { continue; }

                    var instance = Activator.CreateInstance(type);

                    foreach (var property in properties)
                    {
                        var attribute = property.GetCustomAttribute<IgnoreAttribute>();
                        if (attribute != null && attribute.IsManualAssignment)
                        {
                            property.SetValue(instance, sheetName);
                            continue;
                        }

                        object value = new();
                        if (propertyCorreHead[property.Name] == "None")
                        {
                            value = null;
                        }
                        else
                        {
                            value = data[_headCorreIndexes[propertyCorreHead[property.Name]]];
                        }
                        if (property.Name == "PLCLink" && value.ToString().EndsWith("25514"))
                        {

                        }
                        var modelProperty = modelProperties.FirstOrDefault(x => x.PropertyName == property.Name);
                        object convertValue = ConvertValue(value, property.PropertyType, modelProperty.Separator);
                        if (property.PropertyType == typeof(string))
                        {
                            StringBuilder sb = new();
                            sb.Append(modelProperty.Prefix);
                            sb.Append(convertValue.ToString());
                            sb.Append(modelProperty.Suffix);
                            convertValue = sb.ToString();
                        }
                        property.SetValue(instance, convertValue);
                    }

                    configModels.Add(instance);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            
            return configModels;
        }

        private static string GetCurrentModelDirecory()
        {
            string path = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
            path = Path.Combine(path, "Models");
            return path;
        }

        private void GetModelNames(string path)
        {
            modelNames.Clear();
            DirectoryInfo directoryInfo = new(path);
            var fileInfos = directoryInfo.GetFiles();
            foreach (var file in fileInfos)
            {
                ModelNames.Add(Path.GetFileNameWithoutExtension(file.Name));
            }
        }

        private void GetPropertyCorreHead()
        {
            propertyCorreHead.Clear();
            foreach (var item in modelProperties)
            {
                if (propertyCorreHead.ContainsKey(item.PropertyName))
                {
                    propertyCorreHead[item.PropertyName] = item.SelectedHead;
                }
                else
                {
                    propertyCorreHead.Add(item.PropertyName, item.SelectedHead);
                }
            }
        }

        private static int GetIndexOfBySpecifiedNumber(string sourceStr, char word, int specified)
        {
            if (specified <= 0) return -1;
            int startIndex = 0;
            for (int i = 0; i < specified; i++)
            {
                startIndex = sourceStr.IndexOf(word, startIndex);
                if (startIndex == -1) return -1;
                if (i == specified - 1) break;
                startIndex += 1;
            }
            return startIndex;
        }

        private string GetEncodingString()
        {
            string encodingString = "utf-8";
            switch (encodRule)
            {
                case EncodingRule.GB2312:
                    encodingString = "gb2312";
                    break;
                case EncodingRule.GB18030:
                    encodingString = "gb18030";
                    break;
                case EncodingRule.UTF8:
                    encodingString = "utf-8";
                    break;
                case EncodingRule.UTF16:
                    encodingString = "utf-16";
                    break;
                case EncodingRule.Big5:
                    encodingString = "big5";
                    break;
                case EncodingRule.ASCII:
                    encodingString = "ASCII";
                    break;
                case EncodingRule.ANSI:
                    encodingString = "ANSI";
                    break;
                default:
                    break;
            }
            return encodingString;
        }

        private static string GetCellValue(ICell cell)
        {
            if (cell == null) return string.Empty;
            return cell.CellType switch
            {
                CellType.String => cell.StringCellValue,
                CellType.Numeric => cell.NumericCellValue.ToString(),
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                CellType.Formula => cell.CachedFormulaResultType == CellType.String ? cell.StringCellValue : cell.NumericCellValue.ToString(),
                _ => string.Empty
            };
        }

        private static object ConvertValue(object value, Type targetType, string separator)
        {
            if (value == null && targetType == typeof(string))
                return string.Empty;

            if (value == null)
                return GetDefaultValue(targetType);

            string stringValue = value.ToString();

            if (string.IsNullOrEmpty(stringValue) && targetType == typeof(string))
                return string.Empty;

            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                if (underlyingType == typeof(bool))
                {
                    // 处理布尔值（"是"/"否"、"true"/"false"、"1"/"0"等）
                    return ConvertToBoolean(stringValue);
                }
                else if (underlyingType.IsEnum)
                {
                    return Enum.Parse(underlyingType, stringValue, true);
                }
                else if (underlyingType.IsArray)
                {
                    var arrayValue = value.ToString().Replace(" ", "").Replace("；", separator).Split(separator);
                    return Convert.ChangeType(arrayValue, underlyingType);
                }
                else if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type genericType = underlyingType.GenericTypeArguments.FirstOrDefault();
                    var arrayValue = value.ToString().Replace(" ", "").Replace("；", separator).Split(separator);
                    Type listType = typeof(List<>).MakeGenericType(genericType);
                    var list = Activator.CreateInstance(listType);
                    MethodInfo method = listType.GetMethod("Add");
                    foreach (var item in arrayValue)
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        object convertItem = Convert.ChangeType(item, genericType);
                        method.Invoke(list, [convertItem]);
                    }
                    return Convert.ChangeType(list, underlyingType);
                }
                else
                {
                    return Convert.ChangeType(value, underlyingType);
                }
            }
            catch
            {
                throw;
            }
        }

        private static object GetDefaultValue(Type type)
        {
            return Activator.CreateInstance(type);
        }

        private static bool ConvertToBoolean(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            value = value.Trim().ToLower();

            return value == "true" || value == "是" || value == "1" || value == "yes";
        }

    }

    public class ModelPropertyHead : BindableBase
    {
        private string propertyName;

        public string PropertyName { get => propertyName; set => SetProperty(ref propertyName, value); }

        private string selectedHead;

        public string SelectedHead { get => selectedHead; set => SetProperty(ref selectedHead, value); }

        private string prefix;

        public string Prefix { get => prefix; set => SetProperty(ref prefix, value); }

        private string suffix;

        public string Suffix { get => suffix; set => SetProperty(ref suffix, value); }

        private string separator;

        public string Separator { get => separator; set => SetProperty(ref separator, value); }

        private bool prefixIsVisble;

        public bool PrefixIsVisble { get => prefixIsVisble; set => SetProperty(ref prefixIsVisble, value); }

        private bool suffixIsVisble;

        public bool SuffixIsVisble { get => suffixIsVisble; set => SetProperty(ref suffixIsVisble, value); }

        private bool separatorIsVisble;

        public bool SeparatorIsVisble { get => separatorIsVisble; set => SetProperty(ref separatorIsVisble, value); }
    }

    /// <summary>
    /// 当对应列名的单元格为空的时候，忽略此行不计入实体
    /// </summary>
    public class IgnoreRowContentIfColumnEmpry : BindableBase
    {
        private string propertyName;

        public string PropertyName { get => propertyName; set => SetProperty(ref propertyName, value); }

        private bool selected;

        public bool Selected { get => selected; set => SetProperty(ref selected, value); }
    }

    /// <summary>
    /// xls或xlsx文件的Sheet信息模型
    /// </summary>
    public class XlsFileModel
    {
        public string SheetName { get; set; }

        public int SheetIndex { get; set; }

        public bool IsSelected { get; set; }

        public ISheet Sheet { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IgnoreAttribute : Attribute
    {
        /// <summary>
        /// 指示是否忽略该属性，根据自身的需求
        /// </summary>
        public bool IsIngore;
        /// <summary>
        /// 指示是否应该手动赋值
        /// </summary>
        public bool IsManualAssignment;
        public IgnoreAttribute()
        {
        }
    }
}
