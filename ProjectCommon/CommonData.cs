using DoranekoDB;
using System;
using System.Collections.Generic;

/// <summary>
/// プロジェクト共通（自由にカスタマイズしてください）
/// </summary>
public class CommonData
{
    public static void InitDB()
    {
        //todo:

        //全体の設定（どこかで1度実行すればOK）
        //DBBit型 true,false値　★必要であれば変更
        //DBMastar.DBTrueValue = "1"
        //DBMastar.DBFalseValue = "0"

        //テーブル情報の読込
        DBFieldData.FieldDataMemberList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FieldDataMember>>(CommonDataInfo.SchemaJsonString);


        //一時テーブル作成時のSQL文（必要なければコメント推奨）
        DBFieldData.CreateTableSQL =
            " create table {0} " +
                "(" +
                    " {1}" +
                    ",TEMP_AUTO_NUMBER int identity not null" +
                    ",primary key (TEMP_AUTO_NUMBER)" +
            ");";


        //共通の変換定義（DBtableに無いDataTypeを変更する場合に使用）
        //サンプルの意味：テーブル定義のない　ダミーフラグの定義は　DbTable.T_TEST.フラグ.Name　を使用する
        //DBFieldData.DummyFieldChangePublic =
        //        new Dictionary<string, string>() {
        //              { "ダミーフラグ",DbTable.T_TEST.フラグ.Name}
        //          };


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
            db.ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=DoranekoDB;Integrated Security=True;Max Pool Size=3000;";        //外部ファイルから取得
        }
        else
        {
            db.ConnectionString = connectionString;
        }

        //コマンドのタイムアウト ★（GetDBの引数の変更も含め）必要であれば変更
        //db.CommandTimeout = 30
        //TransactionのIsolationLevel★（GetDBの引数の変更も含め）必要であれば変更
        //db.TransactionIsolationLevel = IsolationLevel.ReadCommitted

        //コネクションを開きっぱなしにする場合は、falseをセットしてください。
        //db.ConnectionAutoClose = false;


        //システム固有で、決まったフィールド（例：更新日など）を登録（更新）する場合
        //こちらのプロパティをセットしておくと自動的に追加されます
        //⇒事前に更新日などの更新漏れを防ぐことが出来ます。
        //⇒システム共通のトリガのようなイメージです。

        DateTime nowTime = DateTime.Now;

        //決まった項目の値をセット(DataRow版)　　★意味不明な場合はコメント推奨
        db.InsertUpdateDataDataRow = (paraSqlUpdateType, paraIsTransaction, paraIsError　, paraDr, paraFieldName, paraData ) =>
            {
                if (paraIsTransaction)  //トランザクション中の場合
                {
                }

                //csvなどのデータをDatatableにセットした後、型のチェックのみしたい場合などに使用(db.UpdateDataSet の　引数：updateFlag と共に使用)
                if (paraIsError)  //値変換エラー発生した場合
                {

                    //更新を中止させる事も可(DBへの登録を行わない)
                    //db.IsInsertUpdateCancel = true;
                }


                //if (paraSqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.INSERT)

                if (
                    paraSqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.INSERT ||
                    paraSqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.UPDATE
                 )
                {
                    if (paraFieldName == DbTable.T_TEST.更新日時.Name)
                    {
                        paraData = nowTime;
                    }
                }

                //応用　削除フラグがONの場合、削除日をセット
                /*
                if (paraFieldName == "削除日")
                {

                    if ((bool)(paraDr["削除フラグ"])) {
                        paraData = nowString;
                    }
                }
                */
                return paraData;
            };

        //決まった項目の値をセット(Execute系)　　★意味不明な場合はコメント推奨
        db.InsertUpdateDataParameter = (paraSqlUpdateType, paraIsTransaction, paraFieldList, paraField) =>
        {
            if (paraIsTransaction)
            {
            }

            //paraFieldListには該当するテーブルのフィールドが入っています。
            //更新日のフィールドが存在する場合
            if (paraFieldList.Contains(DbTable.T_TEST.更新日時.Name))
            {
                //if (paraSqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.INSERT)  //InsertSQL
                if (
                paraSqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.INSERT ||
                paraSqlUpdateType == DBFieldData.SQL_UPDATE_TYPE.UPDATE)    //UpdateSQL
                {
                    paraField[DbTable.T_TEST.更新日時.Name] = db.AddInsertParameter(DbTable.T_TEST.更新日時.Name, nowTime);
                }
            }
        };


        //（必要であれば）SQLLogを吐き出し　paraSQLはGetDebugSQLの結果がセット

        db.SetSQLLog = (paraSQL) =>
        {
            //log4net　などを利用し、paraSQLをどこかに吐き出すPGを作成
        };


        return db;
    }
}

