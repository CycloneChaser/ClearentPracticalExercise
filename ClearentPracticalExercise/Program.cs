using ClearentPracticalExercise.DataAccessLayer;
using ClearentPracticalExercise.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ClearentPracticalExercise
{
    class Program
    {
        private static IConfiguration _iconfiguration;
        private static ExtractionStatusModel _status;
        private static int _endPage;
        private static string _baseAddress;
        private static SampleEndpointDataAccess _sampleEndpointDataAccess;
        private static SampleEndpointDataExtractor _dataExtractor;

        static async Task Main(string[] args)
        {
            Initialize();
            while (!_status.Last)
            {
                List<SampleEndpointModel> list = await _dataExtractor.GetNewPage(_status.PageNumber);
                _sampleEndpointDataAccess.PutList(list);
//                _dataExtractor.DisplayRecords();
                _status.Last = (_status.PageNumber >= _endPage) || _dataExtractor.IsDone();
                _sampleEndpointDataAccess.UpdateStatus(_status);
                _status.PageNumber = _dataExtractor.GetPageNumber() + 1;
                Console.Write(".");
            }
        }

        static void Initialize()
        {
            GetAppSettingsFile();
            _status = new ExtractionStatusModel();
            _status.Last = false;
            _status.ExtractionId = _iconfiguration["extractionId"];
            _status.PageNumber = Convert.ToInt32(_iconfiguration["startPage"]);
            _status.PageSize = Convert.ToInt32(_iconfiguration["pageSize"]);
            _status.VersionKey = Convert.ToInt32(_iconfiguration["devVersionKey"]);    // All records are below 1200000
            _status.SortBy = _iconfiguration["sortBy"];
            _endPage = Convert.ToInt32(_iconfiguration["endPage"]);
            _baseAddress = _iconfiguration["baseAddress"];
            _sampleEndpointDataAccess = new SampleEndpointDataAccess(_iconfiguration);
            if (!_sampleEndpointDataAccess.CheckForExtractInProgress(_status))
            {
                _status.ExtractionId = Utilities.RandomString(20);
            }
            _dataExtractor = new SampleEndpointDataExtractor(_baseAddress, _status.VersionKey, _status.PageNumber, _status.PageSize, _status.SortBy);
        }

        static void GetAppSettingsFile()
        {
            _iconfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}
