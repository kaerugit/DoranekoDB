using System;
using System.Collections.Generic;
using System.Linq;

namespace DoranekoDB
{
    public class TableHelper : IDisposable
    {
        /// <summary>フィールド名</summary>
        /// <remarks>
        /// TableHelperをnewしたときに自動的に作成
        /// デフォルトでは
        /// key   = FieldName
        /// Value = FieldName となっています。
        /// </remarks>
        public Dictionary<string, string> Field { get; set; } = new Dictionary<string, string>();

        List<string> ColumnList { get; set; } = new List<string>();
        /// <summary>PrimaryKey</summary>
        public List<string> PrimaryKeyList { get; set; } = new List<string>();

        /// <summary>AutoNumberのFieldName</summary>
        public string AutoNumberFieldName { get; set; } = "";

        /// <summary>PrimaryKey</summary>
        public List<string> IgnoreFieldList { get; set; } = new List<string>();


        /// <summary>定義(Dbtable)に無いものは代用の定義を使う場合にセット　※DBFieldData.DummyFieldChangePublicのローカル版</summary>
        public Dictionary<string, string> DummyFieldChange { get; set; } = null;

        string TableName { get; set; }
        private DBMastar db;

        public enum FIELD_GET_TYPE
        {
            None,
            /// <summary>フィールド（AutoNumber,タイムスタンプ列などは除く） </summary>
            Field,
            /// <summary>テーブル名　＋　フィールド（AutoNumberなどは除く）</summary>
            FieldAndTable,
            /// <summary>フィールド（タイムスタンプ列は除く） </summary>
            NotIgnoreField,
            /// <summary>フィールド（タイムスタンプ列は除く）＋　フィールド（全て） </summary>
            NotIgnoreFieldAndTable,
            /// <summary>フィールド（全て） </summary>
            AllField,
            /// <summary>テーブル名　＋　フィールド（全て）</summary>
            AllFieldAndTable
        }


        /// <summary>取得する値のオプション</summary>
        /// <remarks>
        /// 使い回ししたい場合、以下のコードで使用可
        /// TableHelper.FieldGetType = ～;
        /// TableHelper.Reset()
        /// </remarks>
        public FIELD_GET_TYPE FieldGetType;


        /// <summary>
        /// TableHelper
        /// </summary>
        /// <param name="paradb">データベースオブジェクト</param>
        /// <param name="paraTableName">テーブル名</param>
        /// <param name="fgt">取得オプション</param>
        /// <param name="paraDummyFieldChange">定義(Dbtable)に無いものは代用の定義を使う場合にセット　※DBFieldData.DummyFieldChangePublicのローカル版</param>
        public TableHelper(DBMastar paradb, string paraTableName, FIELD_GET_TYPE fgt = FIELD_GET_TYPE.None, Dictionary<string, string> paraDummyFieldChange = null)
        {
            this.db = paradb;
            this.TableName = paraTableName;

            this.FieldGetType = fgt;

            if (paraDummyFieldChange != null)
            {
                this.DummyFieldChange = paraDummyFieldChange;
            }
            this.Reset();
        }

        /// <summary>
        /// Fieldの省略形
        /// </summary>
        public string this[string key]
        {
            get
            {
                if (this.Field.ContainsKey("") == false)
                {
                    this.Field[key] = "";
                }
                return this.Field[key];
            }
            set
            {
                this.Field[key] = value;
            }
        }

        /// <summary>
        /// パラメータの追加
        /// </summary>
        /// <param name="fildName"></param>
        /// <param name="data"></param>
        /// <remarks>
        /// tbl.Field("××") = db.AddInsertParameter("××", data) の省略形
        /// </remarks>
        public void AddParameter(string fildName, object data)
        {
            this.Field[fildName] = db.AddInsertParameter(fildName, data);
        }

        public void Reset()
        {
            var fullNameFlag = false;
            var autoNumberSetFlag = true;
            var ignoreFieldSetFlag = true;

            if (this.FieldGetType == FIELD_GET_TYPE.FieldAndTable ||
                this.FieldGetType == FIELD_GET_TYPE.AllFieldAndTable ||
                this.FieldGetType == FIELD_GET_TYPE.NotIgnoreFieldAndTable
                )
            {
                fullNameFlag = true;
            }

            if (this.FieldGetType == FIELD_GET_TYPE.Field || this.FieldGetType == FIELD_GET_TYPE.FieldAndTable)
            {
                autoNumberSetFlag = false;  //オートナンバーをセットしない
                ignoreFieldSetFlag = false; //無視するフィールド（タイムスタンプなど）セットしない
            }

            if (this.FieldGetType == FIELD_GET_TYPE.NotIgnoreField || this.FieldGetType == FIELD_GET_TYPE.NotIgnoreFieldAndTable)
            {
                ignoreFieldSetFlag = false; //無視するフィールド（タイムスタンプなど）セットしない
            }

            this.Field.Clear();
            this.PrimaryKeyList.Clear();
            this.IgnoreFieldList.Clear();
            this.ColumnList.Clear();


            foreach (var fm in DBFieldData.FieldDataMemberList.Where(f => f.TABLE_NAME == this.TableName))
            {
                string fieldName = fm.COLUMN_NAME;
                if ((this.ColumnList.Contains(fieldName) == false))
                {
                    this.ColumnList.Add(fieldName);
                }

                //bool primaryKeyFlag = false;
                if (fm.IS_PRIMARYKEY.Equals(DBMastar.DBTrueValue.ToString()))
                {
                    //primaryKeyFlag = true;

                    if ((this.PrimaryKeyList.Contains(fieldName) == false))
                    {
                        this.PrimaryKeyList.Add(fieldName);
                    }
                }

                bool autoNumberExistsFlag = false;
                if (fm.IS_AUTO_NUMBER.Equals(DBMastar.DBTrueValue.ToString()))
                {
                    autoNumberExistsFlag = true;
                    this.AutoNumberFieldName = fm.COLUMN_NAME;
                }

                bool ignoreFieldExistsFlag = false;
                if (fm.IS_IGNORE_FIELD.Equals(DBMastar.DBTrueValue.ToString()))
                {
                    ignoreFieldExistsFlag = true;

                    if ((this.IgnoreFieldList.Contains(fieldName) == false))
                    {
                        this.IgnoreFieldList.Add(fieldName);
                    }

                }

                // オートナンバーを無視
                if (
                    (autoNumberSetFlag == false && autoNumberExistsFlag == true) ||
                    (ignoreFieldSetFlag == false && ignoreFieldExistsFlag == true)
                    )
                {
                    continue;
                }
                else if (this.FieldGetType == FIELD_GET_TYPE.None)
                {
                    continue;
                }

                string fieldNameValue = fieldName;
                // テーブル名付でセット
                if ((fullNameFlag == true))
                {
                    fieldNameValue = this.TableName + "." + fieldNameValue;
                }

                this.Field[fieldName] = fieldNameValue;
            }

        }

        /// <summary>
        /// Insert文のinsertフィールド名を取得
        /// </summary>
        /// <returns></returns>
        public string InsertIntoSQL
        {
            get
            {
                if (db.InsertUpdateDataParameter != null)
                {
                    db.InsertUpdateDataParameter.Invoke(DBFieldData.SQL_UPDATE_TYPE.INSERT, db.IsTransaction, this.ColumnList, this.Field);
                }

                string fieldName = string.Join(",", this.Field.Keys);

                return fieldName;
            }
        }

        /// <summary>
        /// Insert文のselectフィールド名を取得
        /// </summary>
        /// <returns></returns>
        public string InsertSelectSQL
        {
            get
            {
                return getSelectSQL(false);
            }
        }

        /// <summary>
        /// Insert文のselectフィールド名を取得(as 句付き)
        /// </summary>
        /// <returns></returns>
        public string InsertSelectSQLAndField
        {
            get
            {
                return getSelectSQL(true);
            }
        }


        private string getSelectSQL(bool fieldAppendflag)
        {
            string fieldName = 
                string.Join(",",
                    this.Field.Keys.Select(paraKey => {
                        if (fieldAppendflag)
                        {
                            return this.Field[paraKey] + " as " + paraKey;
                        }
                        return this.Field[paraKey];
                    })
               );

            return fieldName;
        }
        /// <summary>
        /// Update文のsetを取得
        /// </summary>
        /// <returns></returns>
        public string UpdateSetSQL
        {
            get
            {
                if (db.InsertUpdateDataParameter != null)
                {
                    db.InsertUpdateDataParameter.Invoke(DBFieldData.SQL_UPDATE_TYPE.UPDATE, db.IsTransaction, this.ColumnList, this.Field);
                }

                string fieldName =
                    string.Join(",",
                        this.Field.Keys.Select(paraKey => {
                            return paraKey + "=" + this.Field[paraKey];
                        })
                   );

                return fieldName;
            }
        }

        /// <summary>
        /// Dictionaryを疑似SQL文に変更
        /// </summary>
        /// <param name="data"></param>
        /// <returns>
        /// select xx as 〇〇,yy as △△　を作成
        /// </returns>
        public string GetSQL(Dictionary<string, object> data)
        {

            this.Field.Clear();

            foreach (var key in data.Keys)
            {
                //無視のフィールド
                if (this.IgnoreFieldList.Contains(key)==true)
                {
                    continue;
                }

                var fieldName = changeDefinition(key);
                this.Field[key] = db.AddInsertParameter(fieldName, data[key]);
            }

            var sql = " select " + this.InsertSelectSQLAndField + " " + db.DummyTable;

            return sql;

        }

        /// <summary>
        /// CreateTable文の作成
        /// </summary>
        /// <param name="tableName">作成したいSQL文</param>
        /// <param name="fieldDataList">作成したいフィールド</param>
        /// <returns></returns>
        /// <remarks>
        /// オートナンバーの定義は考慮なし（元の型で作成される）
        /// this.Field　はいったんクリアされます。⇒　作成後　InsertIntoSQL　でフィールド取得可
        /// </remarks>
        public string GetCreateSQL(string tableName, List<string> fieldDataList)
        {

            var exitFlag = false;
            if (string.IsNullOrEmpty(DBFieldData.CreateTableSQL))
            {
                exitFlag = true;
#if DEBUG
                throw new ApplicationException("CreateTableSQLが設定されていません。");
#endif
               
            }

            if (fieldDataList.Count == 0)
            {
                exitFlag = true;
            }

            if(exitFlag == true)
            {
                return "";
            }


            this.Field.Clear();

            var sql = "";
            foreach (var key in fieldDataList)
            {
                //無視のフィールド
                if (this.IgnoreFieldList.Contains(key) == true)
                {
                    continue;
                }

                sql += "," + key + " " + GetFildType(key);

                this.Field[key] = "";
            }

            if (string.IsNullOrEmpty(sql) == true)
            {
                return "";
            }

            return string.Format(
                DBFieldData.CreateTableSQL, tableName, sql.Substring(1));
        }

        
        /// <summary>
        /// 定義のないもの(Dbtableに無いもの)をほかの定義に変換
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private  string changeDefinition(string fieldName)
        {
            //定義(Dbtable)に無いものは代用の定義を使う
            if (DBFieldData.DummyFieldChangePublic != null && DBFieldData.DummyFieldChangePublic.ContainsKey(fieldName))
            {
                fieldName = DBFieldData.DummyFieldChangePublic[fieldName];
            }
            else if (this.DummyFieldChange !=null && this.DummyFieldChange.ContainsKey(fieldName))
            {
                fieldName = this.DummyFieldChange[fieldName];
            }
            return fieldName;
        }

        private string GetFildType(string fieldName)
        {
            fieldName = changeDefinition(fieldName);
            var dataType = "";
            
            foreach (var fm in DBFieldData.FieldDataMemberList.Where(f => f.COLUMN_NAME == fieldName))
            {
                dataType = fm.DATA_TYPE;
            }
            
            if (string.IsNullOrEmpty(dataType))
            {
#if DEBUG
                throw new ApplicationException("定義がありません");
#endif
            }

            return dataType;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~TableHelper() {
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
