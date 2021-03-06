﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.ManagedDataAccess.Client;
namespace OracleSugar
{
    /// <summary>
    /// ** 描述：底层SQL辅助函数
    /// ** 创始时间：2015-7-13
    /// ** 修改时间：-
    /// ** 作者：sunkaixuan
    /// ** 使用说明：
    /// </summary>
    public class SqlHelper : IDisposable
    {
        OracleConnection _sqlConnection;
        OracleTransaction _tran = null;
        /// <summary>
        /// 是否清空OracleParameters
        /// </summary>
        public bool isClearParameters = true;
        public int CommandTimeOut = 30000;
        /// <summary>
        /// 将页面参数自动填充到OracleParameter []，无需在程序中指定，这种情况需要注意是否有重复参数
        /// 例如：
        ///     var list = db.Queryable《Student》().Where("id=@id").ToList();
        ///     以前写法
        ///     var list = db.Queryable《Student》().Where("id=@id", new { id=Request["id"] }).ToList();
        /// </summary>
        public bool IsGetPageParas = false;
        public SqlHelper(string connectionString)
        {
            _sqlConnection = new OracleConnection(connectionString);
            _sqlConnection.Open();
        }
        public OracleConnection GetConnection()
        {
            return _sqlConnection;
        }
        public void BeginTran()
        {
            _tran = _sqlConnection.BeginTransaction();
        }

        public void BeginTran(IsolationLevel iso)
        {
            _tran = _sqlConnection.BeginTransaction(iso);
        }

        public void RollbackTran()
        {
            if (_tran != null)
            {
                _tran.Rollback();
                _tran = null;
            }
        }
        public void CommitTran()
        {
            if (_tran != null)
            {
                _tran.Commit();
                _tran = null;
            }
        }
        public string GetString(string sql, object pars)
        {
            return GetString(sql, SqlSugarTool.GetParameters(pars));
        }
        public string GetString(string sql, params OracleParameter[] pars)
        {
            return Convert.ToString(GetScalar(sql, pars));
        }
        public int GetInt(string sql, object pars)
        {
            return GetInt(sql, SqlSugarTool.GetParameters(pars));
        }
        public int GetInt(string sql, params OracleParameter[] pars)
        {
            return Convert.ToInt32(GetScalar(sql, pars));
        }
        public Double GetDouble(string sql, params OracleParameter[] pars)
        {
            return Convert.ToDouble(GetScalar(sql, pars));
        }
        public decimal GetDecimal(string sql, params OracleParameter[] pars)
        {
            return Convert.ToDecimal(GetScalar(sql, pars));
        }
        public DateTime GetDateTime(string sql, params OracleParameter[] pars)
        {
            return Convert.ToDateTime(GetScalar(sql, pars));
        }
        public object GetScalar(string sql, object pars)
        {
            return GetScalar(sql, SqlSugarTool.GetParameters(pars));
        }
        public object GetScalar(string sql, params OracleParameter[] pars)
        {
            OracleCommand sqlCommand = new OracleCommand(sql, _sqlConnection);
            sqlCommand.BindByName = true; 
            if (_tran != null)
            {
                sqlCommand.Transaction = _tran;
            }
            sqlCommand.CommandTimeout = this.CommandTimeOut;
            if (pars != null)
                sqlCommand.Parameters.AddRange(pars);
            if (IsGetPageParas)
            {
                SqlSugarToolExtensions.RequestParasToSqlParameters(sqlCommand.Parameters);
            }
            object scalar = sqlCommand.ExecuteScalar();
            scalar = (scalar == null ? 0 : scalar);
            sqlCommand.Parameters.Clear();
            return scalar;
        }
        public int ExecuteCommand(string sql, object pars)
        {
            return ExecuteCommand(sql, SqlSugarTool.GetParameters(pars));
        }
        public int ExecuteCommand(string sql, params OracleParameter[] pars)
        {
            OracleCommand sqlCommand = new OracleCommand(sql, _sqlConnection);
            sqlCommand.BindByName = true; 
            sqlCommand.CommandTimeout = this.CommandTimeOut;
            if (_tran != null)
            {
                sqlCommand.Transaction = _tran;
            }
            if (pars != null)
                sqlCommand.Parameters.AddRange(pars);
            if (IsGetPageParas)
            {
                SqlSugarToolExtensions.RequestParasToSqlParameters(sqlCommand.Parameters);
            }
            int count = sqlCommand.ExecuteNonQuery();
            sqlCommand.Parameters.Clear();
            return count;
        }
        public OracleDataReader GetReader(string sql, object pars)
        {
            return GetReader(sql, SqlSugarTool.GetParameters(pars));
        }
        public OracleDataReader GetReader(string sql, params OracleParameter[] pars)
        {
            OracleCommand sqlCommand = new OracleCommand(sql, _sqlConnection);
            sqlCommand.BindByName = true; 
            sqlCommand.CommandTimeout = this.CommandTimeOut;
            if (_tran != null)
            {
                sqlCommand.Transaction = _tran;
            }
            if (pars != null)
                sqlCommand.Parameters.AddRange(pars);
            if (IsGetPageParas)
            {
                SqlSugarToolExtensions.RequestParasToSqlParameters(sqlCommand.Parameters);
            }
            OracleDataReader sqlDataReader = sqlCommand.ExecuteReader();
            if (isClearParameters)
                sqlCommand.Parameters.Clear();
            return sqlDataReader;
        }
        public List<T> GetList<T>(string sql, object pars)
        {
            return GetList<T>(sql, SqlSugarTool.GetParameters(pars));
        }
        public List<T> GetList<T>(string sql, params OracleParameter[] pars)
        {
            var reval = SqlSugarTool.DataReaderToList<T>(typeof(T), GetReader(sql, pars), null);
            return reval;
        }
        public T GetSingle<T>(string sql, object pars)
        {
            return GetSingle<T>(sql, SqlSugarTool.GetParameters(pars));
        }
        public T GetSingle<T>(string sql, params OracleParameter[] pars)
        {
            var reval = SqlSugarTool.DataReaderToList<T>(typeof(T), GetReader(sql, pars), null).Single();
            return reval;
        }
        public DataTable GetDataTable(string sql, object pars)
        {
            return GetDataTable(sql, SqlSugarTool.GetParameters(pars));
        }
        public DataTable GetDataTable(string sql, params OracleParameter[] pars)
        {
            OracleDataAdapter _sqlDataAdapter = new OracleDataAdapter(sql, _sqlConnection);
            _sqlDataAdapter.SelectCommand.Parameters.AddRange(pars);
            _sqlDataAdapter.SelectCommand.BindByName = true; 
            if (IsGetPageParas)
            {
                SqlSugarToolExtensions.RequestParasToSqlParameters(_sqlDataAdapter.SelectCommand.Parameters);
            }
            _sqlDataAdapter.SelectCommand.CommandTimeout = this.CommandTimeOut;
            if (_tran != null)
            {
                _sqlDataAdapter.SelectCommand.Transaction = _tran;
            }
            DataTable dt = new DataTable();
            _sqlDataAdapter.Fill(dt);
            _sqlDataAdapter.SelectCommand.Parameters.Clear();
            return dt;
        }
        public DataSet GetDataSetAll(string sql, object pars)
        {
            return GetDataSetAll(sql, SqlSugarTool.GetParameters(pars));
        }
        public DataSet GetDataSetAll(string sql, params OracleParameter[] pars)
        {
            OracleDataAdapter _sqlDataAdapter = new OracleDataAdapter(sql, _sqlConnection);
            if (_tran != null)
            {
                _sqlDataAdapter.SelectCommand.Transaction = _tran;
            }
            if (IsGetPageParas)
            {
                SqlSugarToolExtensions.RequestParasToSqlParameters(_sqlDataAdapter.SelectCommand.Parameters);
            }
            _sqlDataAdapter.SelectCommand.CommandTimeout = this.CommandTimeOut;
            _sqlDataAdapter.SelectCommand.BindByName = true; 
            _sqlDataAdapter.SelectCommand.Parameters.AddRange(pars);
            DataSet ds = new DataSet();
            _sqlDataAdapter.Fill(ds);
            _sqlDataAdapter.SelectCommand.Parameters.Clear();
            return ds;
        }

        public void Dispose()
        {
            if (_sqlConnection != null)
            {
                if (_sqlConnection.State != ConnectionState.Closed)
                {
                    if (_tran != null)
                        _tran.Commit();
                    _sqlConnection.Close();
                }
            }
        }
    }
}
