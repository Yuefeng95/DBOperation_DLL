using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBOperation
{
    #region IDAL
    interface IDAL
    {
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
        int Exists<T>(T count, string tableName, string countName);
        /// <summary>
        /// 向数据库插入一条数据
        /// </summary>
        /// <remarks>
        /// 参数所属的类名应对应数据库表名，属性名对应字段名
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类</param>
        /// <returns>返回数据库中受影响的行数</returns>0
        int AddModel<T>(T model);
        /// <summary>
        /// 向数据库更新一条/多条？数据
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为唯一约束
        /// 
        /// 未测试！！！
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类</param>
        /// <param name="countName">sql语句中where判断字段，其值为参数model中的值</param>
        /// <returns>返回数据库中受影响的行数？</returns>0
        int UpdateModel<T>(T model, string[] countName);
        /// <summary>
        /// 向数据库删除一条/多条？数据
        /// </summary>
        /// <remarks>
        /// 函数不会判断countName是否为唯一约束
        /// countName对应的model中的属性值应正确初始化
        /// </remarks>
        /// <param name="model">数据库中表对应的实体类</param>
        /// <param name="countName">sql语句中where判断字段，其值为参数model中的值</param>
        /// <returns>返回受影响行数</returns>
        int DeleteModel<T>(T mdoel, string[] countName);
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
        DataSet SelectList<T>(T model, string[] countName);
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
        DataSet SelectList<T>(T model, string[] countName, int top);//TODO:SelectList need TEST
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
        DataSet SelectRow<T>(T model, string[] countName);// TODO:SelectRow need TEST once
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
        DataSet SelectRow<T>(T model, string[] countName, int top);

        DataSet SelectRow<T, M>(T model, DALTools<M>[] range);//TODO:SelectRow need TEST

        DataSet SelectRow<T, M>(T model, DALTools<M>[] range, int top);
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
        DataSet SelectField<T>(T model, string[] premiseFieldName, string[] aimFieldName);//TODO:SelectField need TEST
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
        DataSet SelectField<T>(T model, string[] premiseFieldName, string[] aimFieldName, int top);//TODO:SelectField need TEST

        DataSet SelectField<T, M>(T model, DALTools<M>[] range, string[] aimFieldName);

        DataSet SelectField<T, M>(T model, DALTools<M>[] range, string[] aimFieldName, int top);
        //where区间查询
        //获取某项数据总数
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
        /// <returns>返回前top行满足premiseFieldName字段条件的aimFieldName字段的参数</returns>
        int MostID<T>(T model, string countName);//TODO:MostID need TEST
    }
    #endregion

    #region DALTools
    public class DALTools<T>
    {
        //
        // 摘要:
        //     最小值
        //
        public DALTools(T maxValues, T minValues, string countName)
        {
            MaxValues = maxValues;
            MinValues = minValues;
            CountName = countName;
        }
        //
        // 摘要:
        //     最大值
        //
        // 返回结果:
        //     返回T类型的值，代表一个上限值
        public T MaxValues { get; }
        //
        // 摘要:
        //     最小值
        //
        // 返回结果:
        //     返回T类型的值，代表一个下限值
        public T MinValues { get; }
        //
        // 摘要:
        //     所代表的字段的名称
        //
        // 返回结果:
        //     返回字段名称
        public string CountName { get; }
    }
    #endregion
}

