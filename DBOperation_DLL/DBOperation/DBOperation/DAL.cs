using Microsoft.ApplicationBlocks.Data;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Windows.Forms;

namespace DBOperation
{
    #region DAL
    /// <summary>
    /// 作为数据传输层DAL的数据库操作库
    /// 需配合数据库映射层Model执行
    /// </summary>
    public sealed class DAL
    {
        //
        // 摘要:
        //     连接语句
        //
        private static readonly string connString = System.Configuration.ConfigurationManager.ConnectionStrings["connection"].ToString();

        #region Exists
        /// <summary>
        /// 检查是否已经存在
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  if (UserI.Exists(userInfo.UserName,userInfo.GetType().Name,nameof(userInfo.UserName)))
        /// </remarks>
        /// <param name="count">一个对应数据库一个字段的方法中的属性</param>
        /// <param name="tableName">表名</param>
        /// <param name="countName">字段名</param>
        /// <returns>返回true为已存在</returns>
        public static int Exists<T>(T count, string tableName, string countName)
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
        /// <summary>
        /// 向数据库插入一条数据
        /// </summary>
        /// <remarks>
        /// 参数所属的类名应对应数据库表名，属性名对应字段名
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类</param>
        /// <returns>返回数据库中受影响的行数</returns>0
        public static int AddModel<T>(T model)
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

        private static int SqlInsertQuery(string commandText, SqlParameter[] paras)
        {
            return SqlHelper.ExecuteNonQuery(connectionString: connString, commandType: CommandType.Text, commandText: commandText, commandParameters: paras);
        }

        private static void SQLInsert<T>(T model, out string commandText, out SqlParameter[] paras)
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
        /// <summary>
        /// 向数据库更新一条/多条？数据
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为唯一约束
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类</param>
        /// <param name="countName">sql语句中where判断字段，其值为参数model中的值</param>
        /// <returns>返回数据库中受影响的行数？</returns>
        public static int UpdateModel<T>(T model, string[] countName)
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

        private static void SQLUpdateModel<T>(T model, out string commandText, string[] countName, out SqlParameter[] paras)
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
        /// <summary>
        /// 向数据库删除一条/多条数据
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为唯一约束
        /// countName对应的model中的属性值应正确初始化
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类</param>
        /// <param name="countName">sql语句中where判断字段，其值为参数model中的值</param>
        /// <returns>返回受影响行数</returns>
        public static int DeleteModel<T>(T model, string[] countName)
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

        private static void SQLDeleteModel<T>(T model, out string commandText, string[] countName, out SqlParameter[] paras)
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
        /// <summary>
        /// 获取countName所对应字段的所有行的值，无需初始化model成员属性（未测试返回数量上限）
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为model的属性名称
        /// 返回所有countName所在字段的参数！！
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类，无需初始化成员属性</param>
        /// <param name="countName">sql语句中select的字段</param>
        /// <returns>返回所有countName所在字段的参数</returns>
        public static DataSet SelectList<T>(T model, string[] countName)
        {
            return SelectList(model, countName, 0);
        }
        /// <summary>
        /// 获取countName所对应字段的所有行的值，无需初始化model成员属性
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为model的属性名称
        /// 返回前top行countName所在字段的参数！！
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类，无需初始化成员属性</param>
        /// <param name="countName">sql语句中select的字段</param>
        /// <param name="top">返回参数最大行数</param>
        /// <returns>返回前top行countName所在字段的参数</returns>
        public static DataSet SelectList<T>(T model, string[] countName, int top)
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

        private static void SQLSelectList<T>(T model, out string commandText, string[] countName, int top, out SqlParameter[] paras)
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
        /// <summary>
        /// 获取满足where条件的所有数据行（未测试返回数量上限）
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为model的属性名称
        /// 返回所有满足条件的model属性中字段的参数！！
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类，无需初始化成员属性</param>
        /// <param name="countName">sql语句中where判断的字段</param>
        /// <returns>返回所有满足条件的model属性中字段的参数</returns>
        public static DataSet SelectRow<T>(T model, string[] countName)
        {
            return SelectRow<T>(model, countName, 0);
        }
        /// <summary>
        /// 获取满足where条件的前top行数据行
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为model的属性名称
        /// 返回前top行满足条件的model属性中字段的参数！！
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类，无需初始化成员属性</param>
        /// <param name="countName">sql语句中where判断的字段</param>
        /// <param name="top">返回参数最大行数</param>
        /// <returns>返回前top行满足条件的model属性中字段的参数</returns>
        public static DataSet SelectRow<T>(T model, string[] countName, int top)
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
        /// <summary>
        /// 获取满足where条件的所有数据行
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为model的属性名称
        /// 返回所有满足条件的model属性中字段的参数！！
        /// 参数range不能上下限同时为null
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类，无需初始化成员属性</param>
        /// <param name="range">sql语句中where判断的字段及其范围，具体见DALTools类</param>
        /// <returns>返回所有满足条件的model属性中字段的参数</returns>
        public static DataSet SelectRow<T, M>(T model, DALTools<M>[] range)
        {
            return SelectRow<T, M>(model, range, 0);
        }
        /// <summary>
        /// 获取满足where条件的前top行数据行
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为model的属性名称
        /// 返回前top行满足条件的model属性中字段的参数！！
        /// 参数range不能上下限同时为null
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类，无需初始化成员属性</param>
        /// <param name="range">sql语句中where判断的字段及其范围，具体见DALTools类</param>
        /// <param name="top">返回的最大行数</param>
        /// <returns>返回前top行满足条件的model属性中字段的参数</returns>
        public static DataSet SelectRow<T, M>(T model, DALTools<M>[] range, int top)
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

        private static void SQLSelectRow<T, M>(T model, out string commandText, string[] countName, DALTools<M>[] range, int top, out SqlParameter[] paras)
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
        /// <summary>
        /// 获取满足where条件的所有若干字段（未测试返回数量上限）
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为model的属性名称
        /// 返回所有满足premiseFieldName字段条件的aimFieldName字段的参数！！
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类，无需初始化成员属性</param>
        /// <param name="premiseFieldName">sql语句中where判断的字段</param>
        /// <param name="aimFieldName">sql语句中要查询的字段</param>
        /// <returns>返回所有满足premiseFieldName字段条件的aimFieldName字段的参数</returns>
        public static DataSet SelectField<T>(T model, string[] premiseFieldName, string[] aimFieldName)
        {
            return SelectField<T>(model, premiseFieldName, aimFieldName, 0);
        }
        /// <summary>
        /// 获取满足where条件的前top行若干字段
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为model的属性名称
        /// 返回前top行满足premiseFieldName字段条件的aimFieldName字段的参数！！
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类，无需初始化成员属性</param>
        /// <param name="premiseFieldName">sql语句中where判断的字段</param>
        /// <param name="aimFieldName">sql语句中要查询的字段</param>
        /// <param name="top">返回参数最大行数</param>
        /// <returns>返回前top行满足premiseFieldName字段条件的aimFieldName字段的参数</returns>
        public static DataSet SelectField<T>(T model, string[] premiseFieldName, string[] aimFieldName, int top)
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
        /// <summary>
        /// 获取满足where条件的所有若干字段（未测试返回数量上限）
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为model的属性名称
        /// 返回所有满足premiseFieldName字段条件的aimFieldName字段的参数！！
        /// 参数range不能上下限同时为null
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类，无需初始化成员属性</param>
        /// <param name="range">sql语句中where判断的字段及其范围，具体见DALTools类</param>
        /// <param name="aimFieldName">sql语句中要查询的字段</param>
        /// <returns>返回所有满足premiseFieldName字段条件的aimFieldName字段的参数</returns>
        public static DataSet SelectField<T, M>(T model, DALTools<M>[] range, string[] aimFieldName)
        {
            return SelectField<T, M>(model, range, aimFieldName, 0);
        }
        /// <summary>
        /// 获取满足where条件的前top行若干字段
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为model的属性名称
        /// 返回前top行满足premiseFieldName字段条件的aimFieldName字段的参数！！
        /// 参数range不能上下限同时为null
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类，无需初始化成员属性</param>
        /// <param name="range">sql语句中where判断的字段及其范围，具体见DALTools类</param>
        /// <param name="aimFieldName">sql语句中要查询的字段</param>
        /// <param name="top">返回参数最大行数</param>
        /// <returns>返回前top行满足premiseFieldName字段条件的aimFieldName字段的参数</returns>
        public static DataSet SelectField<T, M>(T model, DALTools<M>[] range, string[] aimFieldName, int top)
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

        private static void SQLSelectField<T, M>(T model, out string commandText, string[] premiseFieldName, string[] aimFieldName, DALTools<M>[] range, int top, out SqlParameter[] paras)
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
        /// <summary>
        /// 获取一个字段的行数
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为model的属性名称
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类，无需初始化成员属性</param>
        /// <param name="countName">查询的字段名</param>
        /// <returns>返回字段的行数</returns>
        public int MostID<T>(T model, string countName)
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
        private static SqlParameter[] GetSqlParameter<T>(T model)
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
        private static SqlParameter[] GetSqlParameterRange<T>(DALTools<T>[] range)
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
        private static void MakeConditionText<T, M>(T model, PropertyInfo[] modelProperties, ref string commandText, string[] countName, DALTools<M>[] range, out SqlParameter[] paras)
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

        #region Class DALTools
        /// <summary>
        /// 包含sql语句中判断条件的字段名和上下限范围
        /// </summary>
        /// <typeparam name="T">数据传输层Model的类</typeparam>
        public class DALTools<T>
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="maxValues">上限</param>
            /// <param name="minValues">下限</param>
            /// <param name="countName">字段名</param>
            public DALTools(T maxValues, T minValues, string countName)
            {
                MaxValues = maxValues;
                MinValues = minValues;
                CountName = countName;
            }
            /// <summary>
            /// 返回T类型的值，代表一个上限值
            /// </summary>
            public T MaxValues { get; }
            /// <summary>
            /// 返回T类型的值，代表一个下限值
            /// </summary>
            public T MinValues { get; }
            /// <summary>
            /// 返回字段名称
            /// </summary>
            public string CountName { get; }
        }
        #endregion
    }
    #endregion
}
