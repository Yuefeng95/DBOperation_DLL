using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBOperation
{
    #region DAL
    public class DAL : AbstractIDAL
    {
        #region Exists
        public override int Exists<T>(T count, string tableName, string countName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (countName == null)
            {
                throw new ArgumentNullException(nameof(countName));
            }

            SqlParameter[] paras = new SqlParameter[]
            {
                new SqlParameter(parameterName:"@" + countName,value:count)
            };
            try
            {
                Int32 i = Convert.ToInt32(SqlHelper.ExecuteScalar(connectionString: connString, commandType: CommandType.Text, commandText: "select count(*) from " + tableName + " where " + countName + " =@" + countName, commandParameters: paras));
                if (i < 0)
                    throw new ArgumentOutOfRangeException("select result -1");
                return i;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }
        }
        #endregion

        #region AddModel
        public override int AddModel<T>(T model)
        {
            SQLInsert(model: model, commandText: out string commandText, paras: out SqlParameter[] paras);
            try
            {
                return SqlInsertQuery(commandText, paras);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }
        }

        private int SqlInsertQuery(string commandText, SqlParameter[] paras)
        {
            return SqlHelper.ExecuteNonQuery(connectionString: connString, commandType: CommandType.Text, commandText: commandText, commandParameters: paras);
        }

        private void SQLInsert<T>(T model, out string commandText, out SqlParameter[] paras)
        {
            PropertyInfo[] modelProperties = model.GetType().GetProperties();
            if (modelProperties == null)
                throw new NotImplementedException("Property id Null");
            //组合一条SQL语句
            commandText = "insert into " + model.GetType().Name + "(";
            int i = 0;
            foreach (var modelP in modelProperties)
            {
                commandText += modelP.Name + ",";
                i++;
            }
            commandText = commandText.Remove(commandText.LastIndexOf(","), ",".Length);
            commandText += ") values(";
            foreach (var modelP in modelProperties)
            {
                commandText += "@" + modelP.Name + ",";
            }
            commandText = commandText.Remove(commandText.LastIndexOf(","), ",".Length);
            commandText += ")";
            //Sql参数构建
            paras = GetSqlParameter(model);
        }


        #endregion

        #region UpdataModel
        public override int UpdateModel<T>(T model, string[] countName)
        {
            if (countName == null)
                throw new ArgumentNullException(nameof(countName));
            try
            {
                SQLUpdateModel(model, out string commandText, countName, out SqlParameter[] paras);
                return SqlHelper.ExecuteNonQuery(connectionString: connString, commandType: CommandType.Text, commandText: commandText, commandParameters: paras);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }
        }

        private void SQLUpdateModel<T>(T model, out string commandText, string[] countName, out SqlParameter[] paras)
        {
            if (countName == null)
                throw new ArgumentNullException(nameof(countName));

            PropertyInfo[] modelProperties = model.GetType().GetProperties();
            if (modelProperties == null)
                throw new NotImplementedException("Property id Null");
            //组合一条SQL语句
            commandText = "update " + model.GetType().Name + " set ";
            foreach (var modelP in modelProperties)
            {
                commandText += modelP.Name + "=@" + modelP.Name + " , ";
            }
            commandText = commandText.Remove(commandText.LastIndexOf(","), ",".Length);
            commandText += " where ";
            foreach (var modelP in modelProperties)
            {
                foreach (var countN in countName)
                {
                    if (modelP.Name == countN)
                    {
                        commandText += countN + "=@" + countN + " and ";
                    }
                }
            }
            commandText = commandText.Remove(commandText.LastIndexOf(" and "), "and ".Length);
            //Sql参数构建
            paras = GetSqlParameter(model);
        }
        #endregion

        #region DeleteModel
        public override int DeleteModel<T>(T model, string[] countName)
        {
            if (countName == null)
                throw new ArgumentNullException(countName.ToString());
            try
            {
                SQLDeleteModel(model, commandText: out string commandText, countName: countName, paras: out SqlParameter[] paras);
                return SqlHelper.ExecuteNonQuery(connectionString: connString, commandType: CommandType.Text, commandText: commandText, commandParameters: paras);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }
        }

        private void SQLDeleteModel<T>(T model, out string commandText, string[] countName, out SqlParameter[] paras)
        {
            if (countName == null)
                throw new ArgumentNullException(nameof(countName));

            PropertyInfo[] modelProperties = model.GetType().GetProperties();
            if (modelProperties == null)
                throw new NotImplementedException("Property id Null");
            //组合一条SQL语句
            commandText = "delete from  " + model.GetType().Name + " where ";
            foreach (var modelP in modelProperties)
            {
                foreach (var countN in countName)
                {
                    if (modelP.Name == countN)
                    {
                        commandText += countN + "=@" + modelP.Name + " and ";
                    }
                }
            }
            commandText = commandText.Remove(commandText.LastIndexOf("and "), "and ".Length);
            //Sql参数构建
            paras = GetSqlParameter(model);
        }
        #endregion

        #region SelectList
        public override DataSet SelectList<T>(T model, string[] countName)
        {
            return SelectList(model, countName, 0);
        }

        public override DataSet SelectList<T>(T model, string[] countName, int top)
        {
            if (countName == null)
                throw new ArgumentNullException(nameof(countName));
            try
            {
                SQLSelectList(model, out string commandText, countName, top, out SqlParameter[] paras);
                return SqlHelper.ExecuteDataset(connectionString: connString, commandType: CommandType.Text, commandText: commandText, commandParameters: paras);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return (DataSet)null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return (DataSet)null;
            }
        }

        private void SQLSelectList<T>(T model, out string commandText, string[] countName, int top, out SqlParameter[] paras)
        {
            if (countName == null)
                throw new ArgumentNullException(nameof(countName));

            //组合一条SQL语句
            commandText = "select ";
            if (top > 0)
                commandText += " top " + top.ToString() + " ";
            foreach (var countN in countName)
            {
                commandText += countN + ",";
            }
            commandText = commandText.Remove(commandText.LastIndexOf(","), ",".Length);
            commandText += " from " + model.GetType().Name;
            //无需Sql参数
            paras = (SqlParameter[])null;
        }
        #endregion

        #region SelectRow
        public override DataSet SelectRow<T>(T model, string[] countName)
        {
            return SelectRow<T>(model, countName, 0);
        }

        public override DataSet SelectRow<T>(T model, string[] countName, int top)
        {
            if (countName == null)
                throw new ArgumentNullException(nameof(countName));
            try
            {
                SQLSelectRow(model, out string commandText, countName, (DALTools<string>[])null, top, out SqlParameter[] paras);
                return SqlHelper.ExecuteDataset(connectionString: connString, commandType: CommandType.Text, commandText: commandText, commandParameters: paras);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return (DataSet)null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return (DataSet)null;
            }
        }

        public override DataSet SelectRow<T, M>(T model, DALTools<M>[] range)
        {
            return SelectRow<T, M>(model, range, 0);
        }

        public override DataSet SelectRow<T, M>(T model, DALTools<M>[] range, int top)
        {
            if (range == null)
                throw new ArgumentNullException(nameof(range));
            try
            {
                SQLSelectRow(model, out string commandText, (string[])null, range, top, out SqlParameter[] paras);
                return SqlHelper.ExecuteDataset(connectionString: connString, commandType: CommandType.Text, commandText: commandText, commandParameters: paras);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return (DataSet)null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return (DataSet)null;
            }
        }

        private void SQLSelectRow<T, M>(T model, out string commandText, string[] countName, DALTools<M>[] range, int top, out SqlParameter[] paras)
        {
            if ((countName == null) && (range == null))
                throw new ArgumentNullException(nameof(countName) + " And " + nameof(range) + " can't be NULL at the same time.");
            if ((countName != null) && (range != null))
                throw new ArgumentNullException(nameof(countName) + " And " + nameof(range) + " can't be EXIST at the same time.");

            PropertyInfo[] modelProperties = model.GetType().GetProperties();
            if (modelProperties == null)
                throw new NotImplementedException("Property id Null");
            //组合一条SQL语句
            commandText = "select  ";
            if (top > 0)
                commandText += " top " + top.ToString() + " ";
            foreach (var modelP in modelProperties)
            {
                commandText += modelP.Name + ",";
            }
            commandText = commandText.Remove(commandText.LastIndexOf(","), ",".Length);
            commandText += " from " + model.GetType().Name + " where ";
            MakeConditionText(model, modelProperties, ref commandText, countName, range, out paras);
        }
        #endregion

        #region SelectField
        public override DataSet SelectField<T>(T model, string[] premiseFieldName, string[] aimFieldName)
        {
            return SelectField<T>(model, premiseFieldName, aimFieldName, 0);
        }

        public override DataSet SelectField<T>(T model, string[] premiseFieldName, string[] aimFieldName, int top)
        {
            if ((premiseFieldName == null) || (aimFieldName == null))
            {
                throw new ArgumentNullException(nameof(premiseFieldName) + "OR" + nameof(aimFieldName));
            }

            try
            {
                SQLSelectField(model, out string commandText, premiseFieldName, aimFieldName, (DALTools<string>[])null, top, out SqlParameter[] paras);
                return SqlHelper.ExecuteDataset(connectionString: connString, commandType: CommandType.Text, commandText: commandText, commandParameters: paras);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return (DataSet)null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return (DataSet)null;
            }
        }

        public override DataSet SelectField<T, M>(T model, DALTools<M>[] range, string[] aimFieldName)
        {
            return SelectField<T, M>(model, range, aimFieldName, 0);
        }

        public override DataSet SelectField<T, M>(T model, DALTools<M>[] range, string[] aimFieldName, int top)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            try
            {
                SQLSelectField(model, out string commandText, (string[])null, aimFieldName, range, top, out SqlParameter[] paras);
                return SqlHelper.ExecuteDataset(connectionString: connString, commandType: CommandType.Text, commandText: commandText, commandParameters: paras);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return (DataSet)null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return (DataSet)null;
            }
        }

        private void SQLSelectField<T, M>(T model, out string commandText, string[] premiseFieldName, string[] aimFieldName, DALTools<M>[] range, int top, out SqlParameter[] paras)
        {
            if ((premiseFieldName == null) || (aimFieldName == null))
            {
                throw new ArgumentNullException(nameof(premiseFieldName) + "OR" + nameof(aimFieldName) + " have a NULL values.");
            }
            if ((premiseFieldName == null) && (range == null))
                throw new ArgumentNullException(nameof(premiseFieldName) + " And " + nameof(range) + " can't be NULL at the same time.");
            if ((premiseFieldName != null) && (range != null))
                throw new ArgumentNullException(nameof(premiseFieldName) + " And " + nameof(range) + " can't be EXIST at the same time.");

            PropertyInfo[] modelProperties = model.GetType().GetProperties();
            if (modelProperties == null)
                throw new NotImplementedException("Property id Null");
            //组合一条SQL语句
            commandText = "select  ";
            if (top > 0)
                commandText += " top " + top.ToString() + " ";
            foreach (var aimField in aimFieldName)
            {
                commandText += aimField + ",";
            }
            commandText = commandText.Remove(commandText.LastIndexOf(","), ",".Length);
            commandText += " from " + model.GetType().Name + " where ";
            MakeConditionText(model, modelProperties, ref commandText, premiseFieldName, range, out paras);
        }
        #endregion

        #region MostID
        public override int MostID<T>(T model, string countName)
        {
            if (countName == null)
                throw new ArgumentNullException(countName.ToString());
            try
            {
                SQLMostID(model, commandText: out string commandText, countName: countName, paras: out SqlParameter[] paras);
                return SqlHelper.ExecuteNonQuery(connectionString: connString, commandType: CommandType.Text, commandText: commandText, commandParameters: paras);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }
        }

        private void SQLMostID<T>(T model, out string commandText, string countName, out SqlParameter[] paras)
        {
            if (countName == null)
                throw new ArgumentNullException(nameof(countName));

            PropertyInfo[] modelProperties = model.GetType().GetProperties();
            if (modelProperties == null)
                throw new NotImplementedException("Property id Null");
            //组合一条SQL语句
            commandText = "select count(*) ";
            //TODO need a strWhere()
            foreach (var countN in countName)
            {
                commandText += countN + ",";
            }
            commandText = commandText.Remove(commandText.LastIndexOf(","), ",".Length);
            commandText += " from " + model.GetType().Name;
            //无需Sql参数
            paras = (SqlParameter[])null;
        }
        #endregion

        #region GetSqlParameter
        private SqlParameter[] GetSqlParameter<T>(T model)
        {
            int i = 0;
            //获取model属性个数
            foreach (var par in model.GetType().GetProperties())
                i++;
            SqlParameter[] paras = new SqlParameter[i];
            i = 0;
            foreach (var modelP in model.GetType().GetProperties())
                paras[i++] = new SqlParameter("@" + modelP.Name, modelP.GetValue(model));

            return paras;
        }
        #endregion

        #region GetSqlParameterRange
        private SqlParameter[] GetSqlParameterRange<T>(DALTools<T>[] range)
        {
            int i = 0;
            //获取model属性个数
            foreach (var par in range)
                i++;
            SqlParameter[] paras = new SqlParameter[i * 2];
            i = 0;
            foreach (var rang in range)
            {
                paras[i++] = new SqlParameter("@" + rang.CountName + "MAX", rang.MaxValues);
                paras[i++] = new SqlParameter("@" + rang.CountName + "MIN", rang.MinValues);
            }
            return paras;
        }
        #endregion

        #region MakeConditionText
        private void MakeConditionText<T, M>(T model, PropertyInfo[] modelProperties, ref string commandText, string[] countName, DALTools<M>[] range, out SqlParameter[] paras)
        {
            if (countName != null)
            {
                foreach (var modelP in modelProperties)
                {
                    foreach (var countN in countName)
                    {
                        if (modelP.Name == countN)
                        {
                            commandText += countN + "=@" + modelP.Name + " and ";
                        }
                    }
                }
                commandText = commandText.Remove(commandText.LastIndexOf(" and "), "and ".Length);
                //参数构建
                paras = GetSqlParameter(model);
            }
            else
            {
                foreach (var modelP in modelProperties)
                {
                    foreach (var rang in range)
                    {
                        if (modelP.Name == rang.CountName)
                        {
                            if (rang.MaxValues != null)
                                commandText += modelP.Name + " < @" + rang.CountName + "MAX and ";
                            if (rang.MinValues != null)
                                commandText += modelP.Name + " > @" + rang.CountName + "MIN and ";
                        }
                    }
                }
                commandText = commandText.Remove(commandText.LastIndexOf(" and "), "and ".Length);
                //参数构建
                paras = GetSqlParameterRange(range);
            }
        }
        #endregion
    }
    #endregion
}
