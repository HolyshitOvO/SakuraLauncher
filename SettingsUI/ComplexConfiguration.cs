using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace FrontendDemo
{
    /// <summary>
    ///  not allow float long
    /// </summary>
    internal class ComplexConfiguration
    {
        [ConfigTitle("用户名")]
        [UsedImplicitly]
        public string Username { get; set; }
        public string Username2 { get; set; }
        public int Username3 { get; set; }
        public double Username4 { get; set; }

        public List<string> Username7 { get; set; }
        
        [JsonIgnore]
        [ConfigTitle("第一个按钮")]
		public ButtonClick ButtonGoto { get; set; }

        [JsonIgnore]
		public ButtonClick ButtonGoto2 { get; set; }
        
        [JsonIgnore]
        public ButtonClick ButtonGoto3 { get; set; }
        
        // TODO: 添加分隔的tag，添加slider，下拉框的值和显示名需要分离，添加文件选择器级承编辑框
        // TODO: 添加专门显示文本的（可以延迟加载,参考LoadHtml），快捷键编辑，颜色选择器，更高更大的文本框，
        // TODO: 有更多按钮的项（可以通过注解来设置多个key，然后在样式里进行一一binding）
        // TODO: 或者slider使用 int型，注解的方式
        // public SliderBarInt NumberRange { get; set; }
        
        [PredefinedValues("Attack helicopter", "I rather not say", "Why is this even important?")]
        [UsedImplicitly]
        public string Gender { get; set; }

        public string Url { get; set; }

        public string Password { get; set; }

        [CalculatedValuesAsync(nameof(LoadHtml))]
        public string ImTheHtmlOfThrUrl { get; set; }

        private string _lastUrl;
        private string _lastHtmlResult;
        public async Task<IEnumerable<object>> LoadHtml()
        {
            if (Url == _lastUrl)
                return new List<object> {_lastHtmlResult};
            var httpClient = new HttpClient();
            await Task.Delay(4000);
            try
            {
                var x = new WebClient();
                var source = x.DownloadString(Url);
                var title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",RegexOptions.IgnoreCase).Groups["Title"].Value;
                _lastHtmlResult = title;
                return new List<object> {_lastHtmlResult};
            }
            catch (Exception e)
            {
                return new List<object> {"Failed to retrieve html."};
            }
        }

        [UsedImplicitly]
        public ApplicationMode ApplicationMode { get; set; }

        [UsedImplicitly]
        public bool UseLightTheme { get; set; }


        //[UsedImplicitly]
        //public ObservableCollection<Curreny> Currencies { get; set; }

        //[CalculatedValues(nameof(ActiveCurrencyPossibleValues))]
        //[UsedImplicitly]
        //public Curreny ActiveCurrency { get; set; }
        
        //[UsedImplicitly]
        //public IEnumerable<object> ActiveCurrencyPossibleValues() => Currencies;
        

        //[TypesForInstantiation(typeof(Dictionary<string,string>))]
        //public IReadOnlyDictionary<string, string> SomeDictionary { get; set; }

        //public Curreny SomeCurrencyThatJustIsHereForDemonstrationPurposes { get; set; }

        //[TypesForInstantiation(typeof(List<Curreny>))]
        //public IList<Curreny> AllowedStringsForSubInstances { get; set; }

        //public IEnumerable<string> AllowedStringsForRestrictedList() => AllowedStringsForSubInstances.Select(x => x.DisplayName);

        //[TypesForInstantiation(typeof(List<RestrictedStringList>))]
        //[CalculatedValues(nameof(AllowedStringsForRestrictedList), nameof(AllowedStringsForRestrictedList))]
        //public IList<RestrictedStringList> SubInstancesWithRestrictedList { get; set; }
    }
}
