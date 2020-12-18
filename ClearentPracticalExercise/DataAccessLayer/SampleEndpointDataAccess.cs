using ClearentPracticalExercise.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ClearentPracticalExercise.DataAccessLayer
{
    class SampleEndpointDataAccess
    {
        private string _connectionString;

        public SampleEndpointDataAccess(IConfiguration iconfiguration)
        {
            _connectionString = iconfiguration.GetConnectionString("Default");
        }

        public List<SampleEndpointModel> GetList()
        {
            var listSampleEndpointModel = new List<SampleEndpointModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("select * from sampleEndpoint", con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        listSampleEndpointModel.Add(new SampleEndpointModel
                        {
                            RecordKey = Convert.ToInt32(rdr[0]),
                            TextData = rdr[1].ToString(),
                            VersionKey = Convert.ToInt32(rdr[2]),
                            RowNumber = (rdr[3] is DBNull) ? 0 : Convert.ToInt32(rdr[3])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listSampleEndpointModel;
        }

        public void PutList(List<SampleEndpointModel> data)
        {
            try
            {
                DataTable tableToLoad = Utilities.ToDataTable<SampleEndpointModel>(data);
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlBulkCopy s = new SqlBulkCopy(con))
                    {
                        s.DestinationTableName = "SampleEndpoint";
                        s.WriteToServer(tableToLoad);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateStatus(ExtractionStatusModel status)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string sql = "update ExtractionStatus set PageNumber={0}, Last={5} where ExtractionId='{1}'\nIF @@ROWCOUNT=0\ninsert into ExtractionStatus(ExtractionId, VersionKey, PageNumber, PageSize, SortBy, Last) values('{1}', {2}, {0}, {3}, '{4}', {5})";
                    SqlCommand cmd = new SqlCommand(string.Format(sql, status.PageNumber, status.ExtractionId, status.VersionKey, status.PageSize, status.SortBy, status.Last ? 1 : 0), con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    int result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckForExtractInProgress(ExtractionStatusModel status)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string sql = "select * from ExtractionStatus where ExtractionId='{0}' and Last=0";
                    SqlCommand cmd = new SqlCommand(string.Format(sql, status.ExtractionId), con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        status.VersionKey = Convert.ToInt32(rdr[1]);
                        status.PageNumber = Convert.ToInt32(rdr[2]) + 1;
                        status.PageSize = Convert.ToInt32(rdr[3]);
                        status.SortBy = rdr[4].ToString();
                        status.Last = Convert.ToBoolean(rdr[5]);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
