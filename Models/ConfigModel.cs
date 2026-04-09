using ModelsToJson.ViewModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsToJson.Models
{
    public class ConfigModel : BindableBase
    {
        private int _eCID;
        private string _name = string.Empty;
        private string _unit = string.Empty;
        private string _setValue = string.Empty;
        private string _newValue = string.Empty;
        private string _defaultValue = string.Empty;
        private string _min = string.Empty;
        private string _max = string.Empty;
        private string _class = string.Empty;
        private ConfigDataType _templateType;
        private List<string> _options = [];
        private string _discription = string.Empty;
        private string _PLCLink = string.Empty;
        private string _sheetName = string.Empty;

        public int ECID
        { get => _eCID; set => SetProperty(ref _eCID, value); }
        public string Name
        { get => _name; set => SetProperty(ref _name, value); }
        public string Unit
        { get => _unit; set => SetProperty(ref _unit, value); }
        public string SetValue
        { get => _setValue; set => SetProperty(ref _setValue, value); }
        public string NewValue
        { get => _newValue; set => SetProperty(ref _newValue, value); }
        public string DefaultValue
        { get => _defaultValue; set => SetProperty(ref _defaultValue, value); }
        public string Min
        { get => _min; set => SetProperty(ref _min, value); }
        public string Max
        { get => _max; set => SetProperty(ref _max, value); }
        public string Class
        { get => _class; set => SetProperty(ref _class, value); }
        public ConfigDataType TemplateType
        { get => _templateType; set => SetProperty(ref _templateType, value); }
        public List<string> Option
        { get => _options; set => SetProperty(ref _options, value); }
        public string Discription
        { get => _discription; set => SetProperty(ref _discription, value); }

        public string PLCLink
        { get => _PLCLink; set => SetProperty(ref _PLCLink, value); }

        [IgnoreAttribute(IsIngore = true, IsManualAssignment = true)]        
        public string SheetName
        { get => _sheetName; set => SetProperty(ref _sheetName, value); }
    }

    public enum ConfigDataType
    {
        Enum = 1,
        Int = 2,
        Double = 3
    }
}
