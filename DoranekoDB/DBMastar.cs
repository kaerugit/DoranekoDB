using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DoranekoDB
{
    public abstract class DBMastar : IDisposable
    {
        //■■■プロジェクトの固有の設定■■■
        /// <summary>接続文字列</summary>
        public string ConnectionString { get; set; } = "";


        DbConnection Connection { get; set; } = null;
        DbTransaction Transaction { get; set; } = null;
        public DataSet DataSet { get; set; } = null;

        /// <summary>Bit型trueの値</summary>
        public static int DBTrueValue { get; set; } = 1;
        /// <summary>Bit型falseの値</summary>
        public static int DBFalseValue { get; set; } = 0;

        /// <summary>コネクションタイムアウト</summary>
        public int CommandTimeout = 0;

        /// <summary>コネクションを自動closeする場合</summary>
        /// <remarks>一時テーブルを永続的に使用する場合などはfalseにしてください。</remarks>
        public bool ConnectionAutoClose = true;

        /// <summary>トランザクションのレベル</summary>
        public IsolationLevel? TransactionIsolationLevel { get; set; } = null;

        ///  <summary>データベースの型に変換できない場合のダミーの条件</summary>
        ///  <remarks>
        ///  検索条件の数値項目に”a”と入力した場合（本来はアプリ側でチェックする必要あり）に
        ///  検索結果を0件にしたい場合は   111=222(false式) をセット
        ///  検索条件として無視する場合は  111=111(true式)　をセット
        ///  </remarks>
        public static string DummyWhereString { get; set; } = " 111=222 ";

        /// <summary>データ更新時に共通した項目に値をセット(DataSet版)</summary>
        /// <remarks>
        /// 更新日時などをセットしたい場合に使用
        /// 引数：左から　SQL_UPDATE_TYPE、トランザクション中の場合：true、該当レコードのDatarow、フィールド名、値
        /// 戻り値：セットしたい値
        /// </remarks>
        public Func<DBFieldData.SQL_UPDATE_TYPE, Boolean, DataRow, String, Object, Object> InsertUpdateDataDataRow { get; set; } = null;

        /// <summary>データ更新時に共通した項目に値をセット(SQL版)</summary>
        /// <remarks>
        /// 更新日時などをセットしたい場合に使用
        /// 引数：左から　SQL_UPDATE_TYPE、トランザクション中の場合：true、全フィールド、InsertUpdateしたい値（値戻）
        /// </remarks>
        public Action<DBFieldData.SQL_UPDATE_TYPE, Boolean, List<String>, Dictionary<String, String>> InsertUpdateDataParameter = null;

        /// <summary>OpenDataSet使用時テーブルのスキーマも取得する場合：true</summary>
        /// <remarks>取得後自動的にfalseに変更されます</remarks>
        bool IsSchema { get; set; } = false;

        //■■■DB毎(SQLServer,Oracle)の固有の設定（サンプル：DBSQLServer）■■■

        /// <summary>パラメータの記号</summary>
        /// <remarks>oracleは : </remarks>
        protected String ParameterKigo { get; set; }

        /// <summary>文字列連結するときの記号</summary>
        ///  <remarks>oracleは || </remarks>
        protected String RenketuMoji { get; set; }

        /// <summary>fromがないSQLの時の構文</summary>
        ///  <remarks>
        ///  oracleは from dual 
        ///  TableHelperクラスで使用
        ///  </remarks>
        public String DummyTable { get; set; }


        /// <summary>Like検索の記号</summary>
        protected string LikeMoji { get; set; } = "%";


        protected abstract DbConnection GetConnection();
        protected abstract DbParameter GetParameter();
        protected abstract DbCommand GetCommand();
        protected abstract DbDataAdapter GetDataAdapter();
        protected abstract DbCommandBuilder GetCommandBuilder(DbDataAdapter dda);
        public abstract void GetSqlTypeToDbType(string sqlTypeString, out DbType pdbType, out int size, out decimal maxValue, out decimal minValue);

        /// <summary>
        /// スキーマ取得用SQL
        /// </summary>
        /// <returns>ドキュメントにも使えるかもしれません・・</returns>
        public abstract string GetSchemaSQL();

        //■■■その他■■■

        /// <summary>パラメータを管理</summary>
        public DBSQLParameter SQLParameter = new DBSQLParameter();
        //public Dictionary<String, DBUseParameter> ParameterDictionary { get; set; } = new Dictionary<String, DBUseParameter>();
        private DbDataAdapter adapter;

        /// <summary>コネクションのopen</summary>
        /// <remarks>基本的には必要なし、Connection　プロパティを直接参照したい場合に使用して下さい</remarks>
        private void Open()
        {
            this.openCoonection();
        }

        private void openCoonection()
        {
            if (this.Connection == null)
            {
                this.Connection = this.GetConnection();
            }

            if (this.Connection.State == ConnectionState.Closed)
            {
                this.Connection.Open();
            }
        }


        public string AddParameter(string fileName, object data)
        {
            return this.execAddParameter(DBFieldData.SQL_UPDATE_TYPE.WHERE, fileName, data, "@@@");
        }

        public string AddInsertParameter(string fileName, object data)
        {
            return this.execAddParameter(DBFieldData.SQL_UPDATE_TYPE.INSERT, fileName, data, "@@@");
        }


        //public string AddUpdateParameter(string fileName, object data)
        //{
        //    return this.execAddParameter(DBFieldData.SQL_UPDATE_TYPE.UPDATE, fileName, data, "@@@");
        //}

        /// <summary>
        /// SQLの不等号
        /// </summary>
        public enum WHERE_FUGO
        {
            /// <summary>イコール（=）</summary>
            Equal,
            /// <summary>以外（＜＞）</summary>
            Not,
            /// <summary>以下（＜=）</summary>
            Ika,
            /// <summary>以上（＞=）</summary>
            Ijyo,
            /// <summary>大きい（＞）</summary>
            OOki,
            /// <summary>小さい（＜）</summary>
            Tiisai,
            /// <summary>○○を含む(like '%○○%')</summary>
            Like,
            /// <summary>○○から始まる【前方一致】(like '○○%')</summary>
            LikeStart,
            /// <summary>○○から始まる【前方一致】(like '○○%')</summary>
            LikeEnd
        }



        public string AddWhereParameter(string fileName, object data, WHERE_FUGO whereFugo)
        {
            string fugo = "=";
            switch (whereFugo)
            {
                case WHERE_FUGO.Not:
                    fugo = "<>";
                    break;
                case WHERE_FUGO.Ika:
                    fugo = "<=";
                    break;
                case WHERE_FUGO.Ijyo:
                    fugo = ">=";
                    break;
                case WHERE_FUGO.OOki:
                    fugo = ">";
                    break;
                case WHERE_FUGO.Tiisai:
                    fugo = "<";
                    break;
                case WHERE_FUGO.Like:
                    fugo = "Like";
                    break;
                case WHERE_FUGO.LikeStart:
                    fugo = "Like%";
                    break;
                case WHERE_FUGO.LikeEnd:
                    fugo = "%Like";
                    break;
            }

            return this.execAddParameter(DBFieldData.SQL_UPDATE_TYPE.WHERE, fileName, data, fugo);
        }

        ///<summary>
        ///条件の取得
        ///</summary>
        ///<param name="fileName">フィールド名</param>
        ///<param name="data">データ
        ///List(of ○○)でin句を作成
        ///bit列は true , false で検索可能
        ///</param>
        ///<param name="fugo">符号(詳しくは、AddWhereParameterの別のバージョン参考)</param>
        ///<returns></returns>
        public string AddWhereParameter(string fileName, object data, string fugo = "=")
        {
            return this.execAddParameter(DBFieldData.SQL_UPDATE_TYPE.WHERE, fileName, data, fugo);
        }

        private string execAddParameter(DBFieldData.SQL_UPDATE_TYPE sqlUpdateType, string fileName, object data, string fugo = "")
        {
            List<object> lstParameter = new List<object>();
            string parameterString = "";
            if (data != null && (data.GetType().IsGenericType == true)) //'本来は list<> の判定が必要
            {
                //IEnumerable<object> dataList = (IEnumerable<object>)data; こちらうまく変換できないので・・
                if (data.GetType() == typeof(List<int>))
                {
                    foreach (object eachValue in (List<int>)data)
                    {
                        lstParameter.Add(eachValue);
                    }
                }
                else if (data.GetType() == typeof(List<string>))
                {
                    foreach (object eachValue in (List<string>)data)
                    {
                        lstParameter.Add(eachValue);
                    }
                }
                else if (data.GetType() == typeof(List<DateTime>))
                {
                    foreach (object eachValue in (List<DateTime>)data)
                    {
                        lstParameter.Add(eachValue);
                    }
                }
                else if (data.GetType() == typeof(List<object>))
                {
                    foreach (object eachValue in (List<object>)data)
                    {
                        lstParameter.Add(eachValue);
                    }
                }
                else
                {
                    throw new ApplicationException("変換できません");
                }

            }
            else
            {
                lstParameter.Add(data);
            }

            bool inFlag = false;
            object lastData = null;
            //値が取得できない場合に、ダミーの条件を戻す
            bool dummyWhereFlag = false;
            foreach (object eachParaValue in lstParameter)
            {
                var eachPara = eachParaValue;
                //パラメータにセットする値が増える場合は注意
                DBUseParameter para; // DbParameter = Me.GetParameter()
                para.ParameterName = this.ParameterKigo + this.SQLParameter.Keys.Count + "p";

                //列挙型なら数字に変更
                if (eachPara != null && (string.IsNullOrEmpty(eachPara.ToString()) == false) && (eachPara.GetType().IsEnum == true))
                {
                    eachPara = (int)eachPara;
                }
                if (changeValue(sqlUpdateType, null, fileName, ref eachPara, out para.DbType, false))
                {
                    dummyWhereFlag = true;
                }

                para.Value = eachPara;

                lastData = eachPara;
                this.SQLParameter.Add(para.ParameterName, para);
                // 2件目以降(in句の場合カンマを追加)
                if ((string.IsNullOrEmpty(parameterString) == false))
                {
                    if ((inFlag == false) && (sqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.WHERE))
                    {
                        inFlag = true;
                    }
                    parameterString += ",";
                }

                parameterString += para.ParameterName;
            }

            if ((inFlag == true))
            {
                if ((fugo == "="))
                {
                    fugo = "in";
                }
                else if ((fugo == "<>"))
                {
                    fugo = "not in";
                }

                parameterString = "(" + parameterString + ")";
            }

            if (fugo == "@@@")  //'AddParameterから来た場合はパラメータだけ戻す（insert文などで使用）
            {
                return parameterString;
            }
            else
            {
                if (sqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.WHERE)
                {
                    if (dummyWhereFlag == true)
                    {
                        return DummyWhereString;
                    }
                    else if (inFlag == false) //in句でない場合
                    {
                        if (fugo.ToLower().StartsWith("like%"))
                        {
                            return fileName + " like " + parameterString + " " + this.RenketuMoji + " \'" + this.LikeMoji + "\'";
                        }
                        else if (fugo.ToLower().StartsWith("%like"))
                        {
                            return fileName + " like \'" + this.LikeMoji + "\' " + this.RenketuMoji + " " + parameterString + "";
                        }
                        else if (fugo.ToLower().StartsWith("like"))
                        {
                            return fileName + " like \'" + this.LikeMoji + "\' " + this.RenketuMoji + " " + parameterString + " " + this.RenketuMoji + " \'" + this.LikeMoji + "\'";
                        }
                        else if ((lastData == System.DBNull.Value)) //nullの処理
                        {
                            // ○○ is null　に変更
                            if ((fugo == "="))
                            {
                                return (fileName + " is null ");
                            }
                            else if ((fugo == "<>"))
                            {
                                return (fileName + " is not null ");
                            }
                        }
                    }
                }

                return " " + fileName + " " + fugo + " " + parameterString + " ";
            }

        }


        private bool changeValue(DBFieldData.SQL_UPDATE_TYPE sqlUpdateType, DataRow dr, string paraFileName, ref object data, out DbType paraDbType, bool noFieldExitFlag)
        {
            // 検索時にALLOK(1=1)の条件を戻す場合：true
            bool dummyWhereFlag = false;
            string fileName = paraFileName;
            int index = fileName.LastIndexOf(".");

            //テーブル名.フィールド名の場合
            if ((index > -1))
            {
                fileName = fileName.Substring(index + 1);
            }

            DBFieldData.FieldData fieldMember;

            // 定義が存在しない場合
            fieldMember.DbType = DbType.String;
            paraDbType = fieldMember.DbType;

            fieldMember.DefaultValue = "";
            fieldMember.IsNullable = false;
            fieldMember.MaxValue = 0;
            fieldMember.MinValue = 0;
            fieldMember.Size = 0;

            bool findFlag = false;
            if (DBFieldData.FieldMember.ContainsKey(fileName))
            {
                findFlag = true;
            }
            else
            {
                // 存在しない場合は追加
                if ((noFieldExitFlag == false))
                {
                    this.setField(fileName);
                }

                // もう一度確認
                if (DBFieldData.FieldMember.ContainsKey(fileName))
                {
                    findFlag = true;
                }
                else
                {
                    // データセットからのアップデートについてはFieldMemberに存在しないものは、チェックを無視する
                    if ((noFieldExitFlag == true))
                    {
                        return dummyWhereFlag;
                    }


#if DEBUG
                    throw new ApplicationException("定義がありません");
#endif

                }

            }

            if ((findFlag == true))
            {
                fieldMember = DBFieldData.FieldMember[fileName];
            }
            paraDbType = fieldMember.DbType;

            DbType pDbType = fieldMember.DbType;
            if ((pDbType == DbType.Boolean))
            {
                // booleanは ture falseを変更
                bool outValue;

                if (data == null || data == System.DBNull.Value)
                {
                    data = DBFalseValue;
                }
                else
                {
                    if (bool.TryParse(data.ToString(), out outValue))
                    {
                        if ((outValue == true))
                        {
                            data = DBTrueValue;
                        }
                        else
                        {
                            data = DBFalseValue;
                        }
                    }
                    else
                    {
                        data = DBFalseValue;
                    }
                }

                //int型にしないとエラーになるため変換
                //data = int.Parse(data.ToString());
            }
            else if (data == null || string.IsNullOrEmpty(data.ToString()) == true)
            {
                // 空文字列の場合はnullをセット(以下のElseif はnull判定なしとする)
                data = System.DBNull.Value;
            }
            else if (pDbType == DbType.Date || pDbType == DbType.DateTime || pDbType == DbType.DateTime2) //日付系
            {
                // DBパラメータが日付型で、値が日付でない場合に日付型に変更
                DateTime outDate;
                if (data.GetType() != typeof(DateTime))
                {
                    if (DateTime.TryParse(data.ToString(), out outDate))
                    {
                        data = outDate;
                    }
                    else
                    {
                        data = System.DBNull.Value;
                        dummyWhereFlag = true;
                    }

                }

            }
            else if ((pDbType == DbType.Int16))
            {
                Int16 outValue;
                if (Int16.TryParse(data.ToString(), out outValue))
                {
                    data = outValue;
                }
                else
                {
                    data = System.DBNull.Value;
                    dummyWhereFlag = true;
                }

            }
            else if ((pDbType == DbType.Int32))
            {
                Int32 outValue;
                if (Int32.TryParse(data.ToString(), out outValue))
                {
                    data = outValue;
                }
                else
                {
                    data = System.DBNull.Value;
                    dummyWhereFlag = true;
                }

            }
            else if ((pDbType == DbType.Int64))
            {
                Int64 outValue;
                if (Int64.TryParse(data.ToString(), out outValue))
                {
                    data = outValue;
                }
                else
                {
                    data = System.DBNull.Value;
                    dummyWhereFlag = true;
                }

            }
            else if (pDbType == DbType.Currency || (pDbType == DbType.Decimal))
            {
                Decimal outValue;
                if (Decimal.TryParse(data.ToString(), out outValue))
                {
                    data = outValue;
                }
                else
                {
                    data = System.DBNull.Value;
                    dummyWhereFlag = true;
                }

            }
            else if ((pDbType == DbType.Double))
            {
                double outValue;
                if (double.TryParse(data.ToString(), out outValue))
                {
                    data = outValue;
                }
                else
                {
                    data = System.DBNull.Value;
                    dummyWhereFlag = true;
                }

            }
            else if (pDbType == DbType.Binary)
            {
                //バイナリはバイト列かどうかチェック
                //if (data.GetType() != typeof(Byte[]))

                //バイナリ列で文字列の場合は base64とみなして変換かけてみる
                if (data.GetType() == typeof(string))
                {
                    data = System.Convert.FromBase64String(data.ToString());
                }
            }

            // 検索条件以外(update、insert文)の場合は項目チェック
            if (sqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.INSERT || sqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.UPDATE)
            {
                int dataLength = data.ToString().Length;
                // サイズのチェック
                if (dataLength > 0 && fieldMember.Size > 0)
                {
                    // if  (pDbType == DbType.AnsiString(AnsiStringFixedLength) ) ※バイトチェック 本来は必要
                    if ((dataLength > fieldMember.Size))
                    {
                        data = data.ToString().Substring(0, fieldMember.Size); //セットできるところまでのデータを挿入
                    }

                }
                else if (((dataLength > 0)
                            && ((fieldMember.MinValue != 0)
                            || (fieldMember.MaxValue != 0))))
                {
                    if (((fieldMember.MinValue > Decimal.Parse(data.ToString()))
                                || (fieldMember.MaxValue < Decimal.Parse(data.ToString()))))
                    {
                        data = System.DBNull.Value;
                    }

                }

                // nullの場合、デフォルト値が入っていたらそちらをセット
                if ((data == System.DBNull.Value))
                {
                    if ((string.IsNullOrEmpty(fieldMember.DefaultValue) == false))
                    {
                        data = fieldMember.DefaultValue;
                    }

                }

                //共通項目の更新
                if (InsertUpdateDataDataRow != null && dr != null)
                {

                    data = InsertUpdateDataDataRow.Invoke(sqlUpdateType, this.IsTransaction, dr, fileName, data);
                }

            }

            return dummyWhereFlag;
        }

        private void setField(string fieldName)
        {

            foreach (var fm in DBFieldData.FieldDataMemberList.Where(f => f.COLUMN_NAME == fieldName))
            {

                DBFieldData.FieldData fd;

                DbType pdbType;
                int size;
                Decimal maxValue;
                Decimal minValue;

                this.GetSqlTypeToDbType(fm.DATA_TYPE, out pdbType, out size, out maxValue, out minValue);

                fd.DbType = pdbType;
                fd.Size = size;
                fd.MaxValue = maxValue;
                fd.MinValue = minValue;
                fd.DefaultValue = fm.COLUMN_DEFAULT;

                if (fm.IS_NULLABLE.Equals(DBMastar.DBTrueValue.ToString()))
                {
                    fd.IsNullable = true;
                }
                else
                {
                    fd.IsNullable = false;
                }

                DBFieldData.FieldMember.AddOrUpdate(fm.COLUMN_NAME, fd,
                                                    (string paraColumnName, DBFieldData.FieldData parafd) =>
                                                    {
                                                        return parafd;
                                                    }
                                                   );
                break;
            }
        }

        public void ClearSQLParameter()
        {
            this.SQLParameter.Clear();
        }


        /// <summary>
        /// パラメータ付でSQL文をコピー
        /// </summary>
        /// <param name="copyidb">コピーするデータベースオブジェクト</param>
        /// <param name="sql">SQL文</param>
        ///<remarks>
        /// dim idb As DBMastar = CommonData.GetDB()
        /// dim idbWhere As DBMastar = CommonData.GetDB()
        /// dim where as string = _
        ///     " where " & _
        ///         idbWhere.AddWhereParameter
        ///
        /// idbWhere→idb　にコピー
        /// dim sql as string = _
        ///     idb.CopySQL(idbWhere,where)     '戻り値は where
        ///
        /// </remarks>
        /// <returns></returns>
        public string CopySQL(DBMastar copyidb, string sql)
        {
            foreach (string eachKey in copyidb.SQLParameter.Keys)
            {
                //パラメータの値がダブってはいけないので違う番号を付与
                DBUseParameter para;
                para.ParameterName = this.ParameterKigo + this.SQLParameter.Keys.Count + "c";
                para.DbType = copyidb.SQLParameter[eachKey].DbType;
                para.Value = copyidb.SQLParameter[eachKey].Value;

                //元のSQL文のパラメータを新しいパラメータに置換
                if (sql.Contains(eachKey))
                {
                    sql = sql.Replace(eachKey, para.ParameterName);
                    this.SQLParameter.Add(para.ParameterName, para);
                }
            }

            return sql;
        }


        /// <summary>
        /// データテーブルの取得
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string sql)
        {
            DbCommand cmd = getSQLCommand(sql);
            DbDataReader ddr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(ddr);
            ddr.Close();
            if (this.IsTransaction == false && this.ConnectionAutoClose)
            {
                this.Close();
            }

            return dt;
        }

        /// <summary>
        /// SQLの直実行
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int Execure(string sql)
        {
            DbCommand cmd = getSQLCommand(sql);
            return cmd.ExecuteNonQuery();
        }


        /// <summary>
        /// DataSetのOpen
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="tableName">テーブル別名を付けたいときに使用
        /// ※複数のテーブルを開きたいときはこちらを使用してください
        /// </param>
        /// <remarks>
        /// 開いたDataSetは　(DBオブジェクト).DataSet.Table("テーブル名") or (DBオブジェクト).DataSet.Table(0) で取得可能
        /// </remarks>
        public void OpenDataSet(string sql, string tableName = "Default")
        {
            this.adapter = this.GetDataAdapter();
            this.adapter.SelectCommand = getSQLCommand(sql);
            DbCommandBuilder cb = this.GetCommandBuilder(this.adapter);
            cb.GetInsertCommand();

            //主キーのないものはエラーになるが、エラーを無視する
            try
            {
                cb.GetDeleteCommand();
            }
            catch { }
            try
            {
                cb.GetUpdateCommand();
            }
            catch { }



            this.DataSet = new DataSet();
            if (this.IsSchema == true)
            {
                this.IsSchema = false;
                this.adapter.FillSchema(this.DataSet, SchemaType.Mapped, tableName);
            }
            else
            {
                this.adapter.Fill(this.DataSet, tableName);
            }

            if (this.IsTransaction == false && this.ConnectionAutoClose)
            {
                this.Close();
            }
        }


        /// <summary>
        ///DataSetのUpdate
        ///</summary>
        ///<param name="tableName">一部のテーブルのみアップデートしたい場合はtableNameをセット（デフォルトはDataSet内の全テーブルをアップデート）</param>
        ///<remarks>
        ///tableNameを指定しない場合は、DataSetを一旦Clearします。
        ///</remarks>
        public void UpdateDataSet(string tableName = "")
        {
            foreach (DataTable dt in this.DataSet.Tables)
            {
                if ((string.IsNullOrEmpty(tableName) == false))
                {
                    if ((dt.TableName.Equals(tableName) == false))
                    {
                        continue;
                    }

                }

                foreach (DataRow dr in dt.Rows)
                {
                    DBFieldData.SQL_UPDATE_TYPE? sqlUpdateType = null;
                    if (dr.RowState == DataRowState.Added)
                    {
                        sqlUpdateType = DBFieldData.SQL_UPDATE_TYPE.INSERT;
                    }
                    else if (dr.RowState == DataRowState.Modified)
                    {
                        sqlUpdateType = DBFieldData.SQL_UPDATE_TYPE.UPDATE;
                    }

                    if (sqlUpdateType.HasValue)
                    {
                        for (int i = 0; (i
                                    <= (dt.Columns.Count - 1)); i++)
                        {
                            DataColumn col = dt.Columns[i];
                            object data = dr[i];
                            DbType outDbType;
                            changeValue(sqlUpdateType.Value, dr, col.ColumnName, ref data, out outDbType, true);
                            dr[i] = data;
                        }

                    }

                }

                //一時テーブルなどエラーが出る場合は　ConnectionAutoClose　プロパティを見直せばよいかもしれません。
                this.adapter.Update(dt);
            }

            if ((string.IsNullOrEmpty(tableName) == true))
            {
                this.DataSet.Clear();
            }

        }

        private DbCommand getSQLCommand(string sql)
        {
            this.openCoonection();

            var cmd = this.GetCommand();
            cmd.Connection = this.Connection;
            cmd.CommandText = sql;
            cmd.CommandTimeout = this.CommandTimeout;

            //該当するキーが存在するかどうか(基本的にはパラメータは残りっぱなしなので, 著しく速度が低下する場合は、ClearParameterすることをお勧めします)
            if (this.SQLParameter.Keys.Count > 0)
            {
                foreach (string eachKey in this.SQLParameter.Keys)
                {
                    if (sql.Contains(eachKey))
                    {
                        DbParameter para = this.GetParameter();
                        DBUseParameter motoPara = this.SQLParameter[eachKey];
                        para.ParameterName = motoPara.ParameterName;
                        para.DbType = motoPara.DbType;
                        if (motoPara.Value == null)
                        {
                            para.Value = System.DBNull.Value;
                        }
                        else { 
                            para.Value = motoPara.Value;
                        }
                        cmd.Parameters.Add(para);
                    }

                }

            }

            if (this.IsTransaction == true)
            {

                cmd.Transaction = this.Transaction;
            }

            return cmd;
        }

        public void Close()
        {
            if (this.IsTransaction)
            {
                this.Transaction.Rollback();
            }

            this.Transaction = null;
            this.Connection.Close();
        }

        #region "Transaction関連"
        public bool IsTransaction
        {
            get
            {
                if (this.Transaction != null)
                {
                    return true;
                }

                return false;
            }
        }
        public void BeginTransaction()
        {
            this.openCoonection();
            if (this.TransactionIsolationLevel.HasValue)
            {
                this.Transaction = this.Connection.BeginTransaction(this.TransactionIsolationLevel.Value);
            }
            else
            {
                this.Transaction = this.Connection.BeginTransaction();
            }

        }

        public void Commit()
        {
            if (this.IsTransaction)
            {
                this.Transaction.Commit();
                this.Transaction = null;
            }

        }

        public void Rollback()
        {
            if (this.IsTransaction)
            {
                this.Transaction.Rollback();
            }

        }
        #endregion

       

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    this.Close();

                    this.ClearSQLParameter();

                    if (this.adapter != null)
                    {
                        this.adapter = null;
                    }
                }

                if (this.DataSet != null)
                {
                    this.DataSet.Clear();
                    this.DataSet = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~DBMastar() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
