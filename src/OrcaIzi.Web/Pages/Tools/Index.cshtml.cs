namespace OrcaIzi.Web.Pages.Tools
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;

        public IndexModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchType { get; set; } = "CNPJ";

        [BindProperty(SupportsGet = true)]
        public string SearchValue { get; set; } = string.Empty;

        public CnpjResultDto? CnpjResult { get; set; }
        public CepResultDto? CepResult { get; set; }
        public CpfResultDto? CpfResult { get; set; }
        public string? ErrorMessage { get; set; }

        public List<SearchHistoryItem> SearchHistory { get; set; } = new List<SearchHistoryItem>();

        public async Task OnGetAsync()
        {
            LoadHistory();

            if (!string.IsNullOrWhiteSpace(SearchValue))
            {
                SearchValue = SearchValue.Replace(".", "").Replace("-", "").Replace("/", "");

                if (SearchType == "CNPJ" && SearchValue.Length == 14)
                {
                    CnpjResult = await _apiService.ConsultarCnpjAsync(SearchValue);
                    if (CnpjResult == null) ErrorMessage = "CNPJ não encontrado.";
                    else AddToHistory("CNPJ", SearchValue, CnpjResult.Razao_Social);
                }
                else if (SearchType == "CEP" && SearchValue.Length == 8)
                {
                    CepResult = await _apiService.ConsultarCepAsync(SearchValue);
                    if (CepResult == null || CepResult.Erro) ErrorMessage = "CEP não encontrado.";
                    else AddToHistory("CEP", SearchValue, $"{CepResult.Logradouro}, {CepResult.Bairro}");
                }
                else if (SearchType == "CPF" && SearchValue.Length == 11)
                {
                    CpfResult = await _apiService.ConsultarCpfAsync(SearchValue);
                    if (CpfResult == null) ErrorMessage = "Erro ao validar CPF.";
                    else AddToHistory("CPF", SearchValue, CpfResult.Valido ? "Válido" : "Inválido");
                }
                else
                {
                    ErrorMessage = "Formato inválido para o tipo de busca selecionado.";
                }
            }
        }

        private void LoadHistory()
        {
            var cookie = Request.Cookies["SearchHistory"];
            if (!string.IsNullOrEmpty(cookie))
            {
                try
                {
                    SearchHistory = JsonSerializer.Deserialize<List<SearchHistoryItem>>(cookie) ?? new List<SearchHistoryItem>();
                }
                catch { }
            }
        }

        private void AddToHistory(string type, string value, string description)
        {
            var newItem = new SearchHistoryItem { Type = type, Value = value, Description = description, Date = System.DateTime.Now };
            SearchHistory.Insert(0, newItem);
            if (SearchHistory.Count > 5) SearchHistory = SearchHistory.GetRange(0, 5);

            var options = new JsonSerializerOptions { WriteIndented = false };
            var json = JsonSerializer.Serialize(SearchHistory, options);
            Response.Cookies.Append("SearchHistory", json, new Microsoft.AspNetCore.Http.CookieOptions { Expires = System.DateTime.Now.AddDays(30) });
        }

        public class SearchHistoryItem
        {
            public string Type { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public System.DateTime Date { get; set; }
        }
    }
}


