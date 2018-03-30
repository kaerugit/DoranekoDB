using DoranekoDB;
using System;


namespace SampleAndTest
{
    public class CommonData
    {
        public static void InitDB()
        {
            //todo:

            //全体の設定（どこかで1度実行すればOK）
            //DBBit型 true,false値　★必要であれば変更
            //DBMastar.DBTrueValue = "1"
            //DBMastar.DBFalseValue = "0"

            using (DBMastar db = CommonData.GetDB())
            {
                DBFieldData.FieldDataMember = db.GetDataTable(db.GetSchemaSQL());
            }
        }

        /// <summary>
        /// データベースオブジェクトの取得
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns>
        /// 業務毎で自由にカスタマイズしてください
        /// </returns>
        public static DBMastar GetDB(string connectionString = "")
        {

            var db = new DBSQLServer();

            //デフォルトのconnectionString
            if (string.IsNullOrEmpty(connectionString))
            {
                db.ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=DoranekoDB;Integrated Security=True;";        //外部ファイルから取得
            }


            //コマンドのタイムアウト ★（GetDBの引数の変更も含め）必要であれば変更
            //db.CommandTimeout = 30
            //TransactionのIsolationLevel★（GetDBの引数の変更も含め）必要であれば変更
            //db.TransactionIsolationLevel = IsolationLevel.ReadCommitted

            string nowString = DateTime.Now.ToString("yyyyMMddHHmmss");


            //システム固有で、決まったフィールド（例：更新日など）を登録（更新）する場合
            //こちらのプロパティをセットしておくと自動的に追加されます
            //⇒事前に更新日などの更新漏れを防ぐことが出来ます。
            //⇒システム共通のトリガのようなイメージです。

            //決まった項目の値をセット(DataRow版)　　★意味不明な場合はコメント推奨
            db.InsertUpdateDataDataRow = (paraSqlUpdateType, paraIsTransaction, paraDr, paraFileName, paraData) =>
            {
                if (paraIsTransaction)  //トランザクション中の場合
                {
                }

                //if (paraSqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.INSERT)

                if (paraSqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.UPDATE)
                {
                    if (paraFileName == "更新日")
                    {
                        paraData = nowString;
                    }
                }

                //応用　削除フラグがONの場合、削除日をセット
                /*
                if (paraFileName == "削除日")
                {

                    if ((bool)(paraDr["削除フラグ"])) {
                        paraData = nowString;
                    }
                }
                */
                return paraData;
            };


            db.InsertUpdateDataParameter = (paraSqlUpdateType, paraIsTransaction, paraFieldList, paraField) =>
            {
                if (paraIsTransaction)
                {
                }

                //更新日のフィールドが存在する場合
                if (paraFieldList.Contains("更新日"))
                {
                    //if (paraSqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.INSERT)  //InsertSQL
                    if (paraSqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.UPDATE)    //UpdateSQL
                    {
                        paraField["更新日"] = db.AddInsertParameter("更新日", nowString);
                    }
                }
            };

           
            return db;
        }
    }

}
