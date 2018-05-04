using DoranekoDB;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

//メモ　開始するには　メニュー　テストー実行(or デバッグ)-全てのテスト


namespace SampleAndTest
{
    public class UnitTest1
    {
        enum DATA
        {
            one = 1
        }


        public UnitTest1()
        {
            //必ず1回実行
            CommonData.InitDB();
        }

        [Fact(DisplayName = "データベース初期設定")]
        public void DBInit()
        {
            //CommonData.InitDB();

            Assert.NotEqual(DBFieldData.FieldDataMemberList.Count, 0);
        }

        [Fact(DisplayName = "データ追加・更新(SQL)")]
        public void DBSQL()
        {
            var db = CommonData.GetDB();

            //最大の番号を取得
            var dt = db.GetDataTable($@"
                        select 
                            isnull(max({DbTable.T_TEST.テスト用番号.Name}),0) as maxdata 
                        from
                            {DbTable.T_TEST.Name}
                ");

            int maxData = 0;
            if (dt.Rows.Count > 0)
            {
                maxData = int.Parse(dt.Rows[0]["maxdata"].ToString());
            }

            db.BeginTransaction();

            //テスト用データの挿入
            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name))      //好みで・・
            {
                //■■通常の挿入　SQL文を利用しての挿入■■

                //SQLのイメージ
                //tbl.Field のキー insert into テーブル名(★)
                //tbl.Field の値   select ★

                tbl.AddParameter(DbTable.T_TEST.TEXTDATA.Name, "テスト" + System.DateTime.Now.ToString());
                //↓↑同じ意味
                //tbl.Field[DbTable.T_TEST.TEXTDATA.Name] = db.AddInsertParameter(DbTable.T_TEST.TEXTDATA.Name, "テスト");
                //tbl[DbTable.T_TEST.TEXTDATA.Name] = db.AddInsertParameter(DbTable.T_TEST.TEXTDATA.Name, "テスト");

                maxData++;
                tbl.AddParameter(DbTable.T_TEST.テスト用番号.Name, maxData);
                tbl.AddParameter(DbTable.T_TEST.フラグ.Name, true);

                tbl.AddParameter(DbTable.T_TEST.TEXTMAX.Name, DATA.one);

                //nvarchar(50)に51文字入れた場合のテスト（こちらのDBクラスでは入らない文字は区切ってセット）⇒"あ"が欠落
                tbl.AddParameter(DbTable.T_TEST.TEXTDATA.Name, "12345678901234567890123456789012345678901234567890あ");
                //数値欄がオーバーした場合のテスト（こちらのDBクラスではnull(or デフォルト値)をセット）
                tbl.AddParameter(DbTable.T_TEST.金額.Name, double.MaxValue);

                //DbTableに定義されていないフィールドもOK  (フィールドタイプは、何かを拝借すること)
                //tbl.Field["てすと"] = db.AddParameter(DbTable.T_TEST.フラグ.Name, "aaaa");

                //(InsertIntoSQLを使えば)更新日時はCommonData.GetDB(InsertUpdateDataParameter)内で自動的に更新
                db.Execure($@"
                        insert into {DbTable.T_TEST.Name}
                                ({tbl.InsertIntoSQL})
                        values 
                                ({tbl.InsertSelectSQL}) 
                    ");


            }

            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name, TableHelper.FIELD_GET_TYPE.Field))
            {
                Assert.True(tbl.Field.Keys.Count != 0);

                //■■SQL文を利用しての挿入■■
                //tbl.Field　には Autonumber（自動挿入off）を除いたデータがセットされています。(取得したい場合は　TableHelperの引数参照)
                var oldmaxkbn = maxData;

                maxData++;
                tbl.AddParameter(DbTable.T_TEST.テスト用番号.Name, maxData);
                //sql文も可能
                tbl[DbTable.T_TEST.TEXTDATA.Name] = "substring(" + DbTable.T_TEST.TEXTDATA.Name + ",1,10)";     //tbl.Field[] の省略形
                tbl[DbTable.T_TEST.金額.Name] = "1000";

                //他設定されていないデータは元データの値をinsert
                db.Execure($@"
                        insert into {DbTable.T_TEST.Name}
                                ({tbl.InsertIntoSQL})
                        select 
                                {tbl.InsertSelectSQL}
                        from 
                                {DbTable.T_TEST.Name}
                        where 
                                {db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, oldmaxkbn)}

                    ");


                //■■同じようなテーブルにコピー（T_TESTのデータにあってT_TEST_COPYにないデータをコピー）■■
                tbl.FieldGetType = TableHelper.FIELD_GET_TYPE.FieldAndTable;
                tbl.Reset();

                //tbl.Field[DbTable.T_TEST.TEXTDATA.Name] =  "T_TEST.TEXTDATA" のデータが入っています。
                //テーブル名が必要な時は　FullName　を使用
                tbl[DbTable.T_TEST.ID_AUTO.Name] = DbTable.T_TEST.ID_AUTO.FullName;   //T_TEST_COPYのID_AUTOはオートナンバーではない
                tbl.AddParameter(DbTable.T_TEST.TEXTFIX.Name, System.DBNull.Value);

                var sql =
                    $@"
                        insert into {DbTable.T_TEST_COPY.Name}
                                ({tbl.InsertIntoSQL})
                        select 
                                {tbl.InsertSelectSQL}
                        from 
                                {DbTable.T_TEST.Name}
                                left join   {DbTable.T_TEST_COPY.Name}
                                    on {DbTable.T_TEST.ID_AUTO.FullName} = {DbTable.T_TEST_COPY.ID_AUTO.FullName} 
                        where 
                                {db.AddWhereParameter(DbTable.T_TEST_COPY.ID_AUTO.FullName, System.DBNull.Value)}

                    ";

                db.Execure(sql);
            }


            try
            {
                db.Commit();
            }
            catch
            {
                db.Rollback();
            }

            db = CommonData.GetDB();
            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name))      //好みで・・
            {
                //■■Updateテスト■■
                tbl.AddParameter(DbTable.T_TEST.TEXTFIX.Name, "TEST");

                //直接SQL文を書くことも可能
                tbl.Field[DbTable.T_TEST.金額.Name] = $@"
                            case when {db.AddWhereParameter(DbTable.T_TEST.金額.Name, 0, "<>")} then
                                {DbTable.T_TEST.金額.Name} + 100
                            else
                                {DbTable.T_TEST.金額.Name}
                            end
                            "
                     ;
                //(UpdateSetSQLを使えば)更新日時なども自動でセット
                db.Execure($@"
                        update {DbTable.T_TEST_COPY.Name}
                        set 
                                {tbl.UpdateSetSQL}
                    ");
            }

            db.Close();

        }

        [Fact(DisplayName = "通常のSELECT")]
        public void Select1()
        {

            var db = CommonData.GetDB();


            //最大の番号を取得
            var dt = db.GetDataTable($@"
                        select 
                            isnull(max({DbTable.T_TEST.テスト用番号.Name}),0) as maxdata 
                        from
                            {DbTable.T_TEST.Name}
                ");

            int maxData = 0;
            if (dt.Rows.Count > 0)
            {
                maxData = int.Parse(dt.Rows[0]["maxdata"].ToString());
            }

            //通常のwhere
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, maxData)}
                    
               ");

            Assert.Equal(dt.Rows.Count, 1);

            //■■in句■■
            var lst = new List<int>() { maxData, maxData - 1 };
            //AddWhereParameter の場所で　テスト用番号 in (xx,yy) のデータが作成
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}  
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, lst)}
                    
               ");
            Assert.True(dt.Rows.Count > 1);


            //■■enumのテスト■■
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTMAX.Name, DATA.one)}
               ");

            Assert.True(dt.Rows.Count > 1);

            //■■以上■■
            //テスト用番号>=xx 
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, maxData, DBMastar.WHERE_FUGO.Ijyo)}
                    
               ");

            Assert.Equal(dt.Rows.Count, 1);


            //■■Likeのテスト■■
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTDATA.Name, "1234", DBMastar.WHERE_FUGO.Like)}
               ");

            Assert.True(dt.Rows.Count > 1);

            //■■フラグのテスト■■
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.フラグ.Name, null)}
               ");

            Assert.True(dt.Rows.Count > 1);

        }


        [Fact(DisplayName = "データ追加・更新(Dataset)")]
        public void DBDataSet()
        {
            var db = CommonData.GetDB();

            db.BeginTransaction();

            //■■新規追加■■
            db.OpenDataSet($@"
                    select 
                        * 
                    from 
                        {DbTable.T_TEST.Name}
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.ID_AUTO.Name, System.DBNull.Value)}
                ");


            var dt = db.DataSet.Tables[0];
            var nowString = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            //100件データを作成
            for (var i = 0; i < 100; i++)
            {
                var drNew = dt.NewRow();

                drNew[DbTable.T_TEST.TEXTMAX.Name] = nowString;
                drNew[DbTable.T_TEST.TEXTDATA.Name] = "挿入" + (i + 1).ToString();
                drNew[DbTable.T_TEST.金額.Name] = i * 1000;
                dt.Rows.Add(drNew);
            }

            //更新(更新日時は CommonData.GetDB InsertUpdateDataDataRow 内で処理)
            db.UpdateDataSet();

            //■■更新■■
            db.OpenDataSet($@"
                    select 
                         * 
                    from 
                        {DbTable.T_TEST.Name}
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTDATA.Name, "挿入", DBMastar.WHERE_FUGO.Like)}
                ", "なまえつき");


            dt = db.DataSet.Tables["なまえつき"];


            foreach (System.Data.DataRow dr in dt.Rows)
            {


                dr[DbTable.T_TEST.TEXTDATA.Name] = dr[DbTable.T_TEST.TEXTDATA.Name].ToString() + "更新";
                //dr[DbTable.T_TEST.TEXTDATA.Name] = dr[DbTable.T_TEST.TEXTDATA.Name].ToString() + "更新";
                dr[DbTable.T_TEST.金額.Name] = decimal.Parse(dr[DbTable.T_TEST.金額.Name].ToString()) + 100;
            }

            //更新
            db.UpdateDataSet("なまえつき");


            try
            {
                db.Commit();
            }
            catch
            {
                db.Rollback();
            }


            //
            var sql = $@"
                    select 
                         * 
                        ,{DbTable.T_TEST.フラグ.Name} as 定義フィールドを勝手に追加
                    from 
                        {DbTable.T_TEST.Name}
                    where
                             {db.AddWhereParameter(DbTable.T_TEST.金額.Name, 0, ">=")}
                        and  {db.AddWhereParameter(DbTable.T_TEST.TEXTMAX.Name, nowString)}
                ";

            var dtSelect = db.GetDataTable(sql);

            //最後のデータを削除
            dtSelect.Rows.Remove(dtSelect.Rows[dtSelect.Rows.Count - 1]);

            //データを変更
            var drSelect = dtSelect.NewRow();
            drSelect[DbTable.T_TEST.TEXTDATA.Name] = "ついかでーた";
            drSelect[DbTable.T_TEST.金額.Name] = 10;

            if (System.IO.File.Exists(@"C:\inetpub\wwwroot\iis-85.png") == true)
            {
                FileStream fs = new FileStream(@"C:\inetpub\wwwroot\iis-85.png", FileMode.OpenOrCreate, FileAccess.Read);
                byte[] MyData = new byte[fs.Length];
                fs.Read(MyData, 0, System.Convert.ToInt32(fs.Length));

                fs.Close();

                drSelect[DbTable.T_TEST.バイナリー.Name] = MyData;
            }
            drSelect[DbTable.T_TEST.フラグ.Name] = DBMastar.DBTrueValue;
            drSelect["定義フィールドを勝手に追加"] = DBMastar.DBTrueValue;
            dtSelect.Rows.Add(drSelect);


            dtSelect.Rows[0][DbTable.T_TEST.入力日.Name] = System.DateTime.Now.Date;
            dtSelect.Rows[0][DbTable.T_TEST.金額.Name] = 888;


            //jsonに変換(web apiなどで送信)
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dtSelect);


            //jsonからDataTableに変換(web apiなどで受信)
            //var dtInput = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(jsonString);

            //jsonからの戻し(SQLServer2016 だと json をそのままtable化できるのでそちらを使用したほうがよいと思う　hint OPENJSON関数 )
            var dataList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonString);
            //var dtInput = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(jsonString);

            var tempTableName = "##temp" + nowString;

            if (dataList.Count > 0)
            {
                var dummyFieldChange = new Dictionary<string, string>()
                        {
                            { "定義フィールドを勝手に追加", DbTable.T_TEST.フラグ.Name }
                        };


                var tbl = new TableHelper(db, DbTable.T_TEST.Name, paraDummyFieldChange: dummyFieldChange);
                //現在のデータからCreatetableを行う
                //ToListするには　using System.Linq;　が必要
                db.Execure(tbl.GetCreateSQL(tempTableName, dataList[0].Keys.ToList()));

                //～通常版
                if (true)
                {
                    //db.BeginTransaction();
                    db.ConnectionAutoClose = false; //##テーブルが消えるので・・

                    db.OpenDataSet($"select * from  {tempTableName}");

                    var dtTemp = db.DataSet.Tables[0];
                    foreach (var data in dataList)
                    {
                        var dr = dtTemp.NewRow();
                        foreach (var key in data.Keys)
                        {
                            //無視するフィールド
                            if (tbl.IgnoreFieldList.Contains(key))
                            {
                                continue;
                            }

                            if (data[key] == null)
                            {
                                dr[key] = System.DBNull.Value;
                            }
                            else
                            {
                                //バイナリ列のみ特別処理（オブジェクトをそのまま入れるとエラーになるので、byteデータとして処理）
                                if (dtTemp.Columns[key].DataType == typeof(System.Byte[]))
                                {
                                    dr[key] = System.Convert.FromBase64String(data[key].ToString());
                                }
                                else
                                {
                                    dr[key] = data[key];
                                }
                            }
                        }
                        dtTemp.Rows.Add(dr);
                    }

                    db.UpdateDataSet();
                    //db.Commit();
                }


                //～高速版（だけど負荷がかかります）
                if (false)
                {
                    var filed = tbl.InsertIntoSQL;

                    Parallel.ForEach(dataList, data =>
                    {

                        //Parallel.ForEach 内でのdbオブジェクトの使いまわしは厳禁
                        using (var dbpara = CommonData.GetDB())
                        {
                            var tblpara = new TableHelper(dbpara, DbTable.T_TEST.Name, paraDummyFieldChange: dummyFieldChange);

                            sql = $@"
                                insert into {tempTableName}
                                    ({filed})
                                select 
                                    {filed}
                                from 
                                    (
                                    {tblpara.GetSQL(data)}
                                    ) as ins
                                ";

                            dbpara.Execure(sql);
                        }
                    });
                }




                //遅い
                if (false)
                {
                    var filed = tbl.InsertIntoSQL;
                    foreach (var data in dataList)
                    {
                        sql = $@"
                            insert into {tempTableName}
                                ({filed})
                            select 
                                {filed}
                            from 
                                (
                                {tbl.GetSQL(data)}
                                ) as ins
                           ";

                        db.Execure(sql);

                    }
                }
            }


            //更新
            db.BeginTransaction();
            var margeSql = SQLHelper.GetMargeSQL(db, DbTable.T_TEST.Name, $" select * from {tempTableName}");
            db.Execure(margeSql);
            try
            {
                db.Commit();
            }
            catch
            {
                db.Rollback();
            }

        }

        [Fact(DisplayName = "パラメータの保存")]
        public void SaveParameter()
        {
            var db = CommonData.GetDB();

            //パラメータの保存
            db.ClearSQLParameter();

            var lst = new List<int>() { 100, 200 };

            var sql = $@"
                 select 
                   * 
                 from
                    {DbTable.T_TEST.Name} 
                 where
                        {db.AddWhereParameter(DbTable.T_TEST.金額.Name, lst, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.金額.Name, DATA.one, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.金額.Name, System.DBNull.Value, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.フラグ.Name, false, DBMastar.WHERE_FUGO.Not)}
            ";

            var datacount = db.GetDataTable(sql).Rows.Count;

            //jsonオブジェクトに変換
            var save = Newtonsoft.Json.JsonConvert.SerializeObject(db.SQLParameter);
            var db2 = CommonData.GetDB();

            //戻す（この場合System.DBNull.Value ⇒ nullに変換されるのがDBMastarクラスで対応済）
            db2.SQLParameter = Newtonsoft.Json.JsonConvert.DeserializeObject<DBSQLParameter>(save);
            var dt2 = db2.GetDataTable(sql);

            Assert.Equal(datacount, dt2.Rows.Count);

        }

    }
}
