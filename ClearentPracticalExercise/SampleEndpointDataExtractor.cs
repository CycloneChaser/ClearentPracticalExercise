using ClearentPracticalExercise.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClearentPracticalExercise
{
    public class SampleEndpointDataExtractor
    {
        private string _baseAddress;
        private int _versionKey;
        private int _pageNumber;
        private int _pageSize;
        private string _sortBy;
        private string _uriFormat;
        private JsonDocument _jsonDocument;
        public SampleEndpointDataExtractor(string baseAddress, int versionKey, int pageNumber, int pageSize, string sortBy)
        {
            _baseAddress = baseAddress;
            _versionKey = versionKey;
            _pageNumber = pageNumber;
            _pageSize = pageSize;
            _sortBy = sortBy;
            _uriFormat = _baseAddress + "?pageNumber={0}&pageSize=" + _pageSize + "&devVersionKey=" + _versionKey;
            if (!string.IsNullOrEmpty(_sortBy))
                _uriFormat += "&sortBy=" + _sortBy;
        }

        public async Task<List<SampleEndpointModel>> GetNewPage(int pageNumber)
        {
            _pageNumber = pageNumber;
            _jsonDocument = await GetResponse();
            return ConvertToList();
        }

        private async Task<JsonDocument> GetResponse()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "Clearent Practical Exercise");
                client.DefaultRequestHeaders.Add("AccessKey", "52e1ac56-bc96-4c74-86c3-8a0e20467492");

                string uri = string.Format(_uriFormat, _pageNumber);
                var streamTask = client.GetStreamAsync(uri);
                return JsonDocument.Parse(await streamTask);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool IsDone()
        {
            JsonElement root = _jsonDocument.RootElement;
            JsonElement pageElement = root.GetProperty("page");
            JsonElement lastElement = pageElement.GetProperty("last");
            return (lastElement.GetBoolean());
        }

        public int GetPageNumber()
        {
            JsonElement root = _jsonDocument.RootElement;
            JsonElement pageElement = root.GetProperty("page");
            JsonElement numberElement = pageElement.GetProperty("number");
            return numberElement.GetInt32();
        }

        private List<SampleEndpointModel> ConvertToList()
        {
            List<SampleEndpointModel> list = new List<SampleEndpointModel>();

            JsonElement root = _jsonDocument.RootElement;
            JsonElement sampleEndpointElement = root.GetProperty("sampleEndpoint");
            foreach (JsonElement sample in sampleEndpointElement.EnumerateArray())
            {
                JsonElement keyElement = sample.GetProperty("devInterviewKey");
                int key = Convert.ToInt32(keyElement.GetString());
                JsonElement textDataElement = sample.GetProperty("devInterviewTextData");
                string textData = textDataElement.GetString();
                JsonElement versionElement = sample.GetProperty("devVersionKey");
                int version = Convert.ToInt32(versionElement.GetString());
                JsonElement rowNumberElement = sample.GetProperty("rowNumber");
                int rowNumber = Convert.ToInt32(rowNumberElement.GetString());
                list.Add(new SampleEndpointModel() { RecordKey = key, TextData = textData, VersionKey = version, RowNumber = rowNumber });
            }
            return list;
        }

        public void DisplayRecords()
        {
            JsonElement root = _jsonDocument.RootElement;
            JsonElement sampleEndpointElement = root.GetProperty("sampleEndpoint");
            Console.WriteLine("sampleEndpoint ---------------");
            foreach (JsonElement sample in sampleEndpointElement.EnumerateArray())
            {
                JsonElement keyElement = sample.GetProperty("devInterviewKey");
                int key = Convert.ToInt32(keyElement.GetString());
                JsonElement textDataElement = sample.GetProperty("devInterviewTextData");
                string textData = textDataElement.GetString();
                JsonElement versionElement = sample.GetProperty("devVersionKey");
                int version = Convert.ToInt32(versionElement.GetString());
                Console.WriteLine(string.Format("{0}, {1}, {2}", key, textData, version));
            }

            JsonElement pageElement = root.GetProperty("page");
            JsonElement numberElement = pageElement.GetProperty("number");
            int number = numberElement.GetInt32();
            JsonElement sizeElement = pageElement.GetProperty("size");
            int size = sizeElement.GetInt32();
            string sort = String.Empty;
            JsonElement sortElement = pageElement.GetProperty("sort");
            if (sortElement.ValueKind != JsonValueKind.Null)
            {
                JsonElement fieldsElement;
                if (sortElement.TryGetProperty("fields", out fieldsElement))
                {
                    foreach (JsonElement field in fieldsElement.EnumerateArray())
                    {
                        JsonElement keyElement = field.GetProperty("key");
                        string key = keyElement.GetString();
                        JsonElement sortingTypeElement = field.GetProperty("sortingType");
                        string sortingType = sortingTypeElement.GetString();
                        sort += string.Format("{0}|{1} ", key, sortingType);
                    }
                }
            }
            else
                sort = "null";
            JsonElement totalElementsElement = pageElement.GetProperty("totalElements");
            int totalElements = totalElementsElement.GetInt32();
            JsonElement lastElement = pageElement.GetProperty("last");
            bool last = lastElement.GetBoolean();
            Console.WriteLine("page -------------------------");
            Console.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}", number, size, sort, totalElements, last));

            JsonElement metadataElement = root.GetProperty("metadata");
            JsonElement exchangeIdElement = metadataElement.GetProperty("exchangeId");
            string exchangeId = exchangeIdElement.ToString();
            JsonElement timestampElement = metadataElement.GetProperty("timestamp");
            string timestamp = timestampElement.ToString();
            Console.WriteLine("metadata ---------------------");
            Console.WriteLine(string.Format("{0}, {1}", exchangeId, timestamp));
        }
    }
}
